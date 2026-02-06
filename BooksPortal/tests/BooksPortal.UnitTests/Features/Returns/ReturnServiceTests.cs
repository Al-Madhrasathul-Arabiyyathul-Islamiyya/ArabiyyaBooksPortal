using BooksPortal.Application.Common.Exceptions;
using BooksPortal.Application.Common.Interfaces;
using BooksPortal.Application.Features.Returns.DTOs;
using BooksPortal.Application.Features.Returns.Services;
using BooksPortal.Domain.Entities;
using BooksPortal.Domain.Enums;
using FluentAssertions;
using NSubstitute;

namespace BooksPortal.UnitTests.Features.Returns;

public class ReturnServiceTests
{
    private readonly IRepository<ReturnSlip> _slipRepo;
    private readonly IRepository<Book> _bookRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IReferenceNumberService _refService;
    private readonly IPdfService _pdfService;
    private readonly ISlipStorageService _storageService;
    private readonly ReturnService _sut;

    public ReturnServiceTests()
    {
        _slipRepo = Substitute.For<IRepository<ReturnSlip>>();
        _bookRepo = Substitute.For<IRepository<Book>>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _refService = Substitute.For<IReferenceNumberService>();
        _pdfService = Substitute.For<IPdfService>();
        _storageService = Substitute.For<ISlipStorageService>();
        _sut = new ReturnService(_slipRepo, _bookRepo, _unitOfWork, _refService, _pdfService, _storageService);
    }

    private static Book CreateBook(int id = 1, int totalStock = 100, int distributed = 30,
        int withTeachers = 0, int damaged = 5, int lost = 3)
    {
        return new Book
        {
            Id = id, Code = $"BK{id:D3}", Title = $"Book {id}",
            SubjectId = 1, Subject = new Subject { Id = 1, Name = "Math", Code = "MAT" },
            TotalStock = totalStock, Distributed = distributed,
            WithTeachers = withTeachers, Damaged = damaged, Lost = lost
        };
    }

    // --- ApplyReturnToBook: condition-based counter logic ---

    [Theory]
    [InlineData(BookCondition.Good)]
    [InlineData(BookCondition.Fair)]
    [InlineData(BookCondition.Poor)]
    public void ApplyReturn_GoodFairPoor_DecrementsDistributedOnly(BookCondition condition)
    {
        var book = CreateBook(distributed: 30, damaged: 5, lost: 3);

        ReturnService.ApplyReturnToBook(book, quantity: 5, condition);

        book.Distributed.Should().Be(25);  // 30 - 5
        book.Damaged.Should().Be(5);       // unchanged
        book.Lost.Should().Be(3);          // unchanged
    }

    [Fact]
    public void ApplyReturn_Damaged_DecrementsDistributedAndIncrementsDamaged()
    {
        var book = CreateBook(distributed: 30, damaged: 5);

        ReturnService.ApplyReturnToBook(book, quantity: 3, BookCondition.Damaged);

        book.Distributed.Should().Be(27);  // 30 - 3
        book.Damaged.Should().Be(8);       // 5 + 3
    }

    [Fact]
    public void ApplyReturn_Lost_DecrementsDistributedAndIncrementsLost()
    {
        var book = CreateBook(distributed: 30, lost: 3);

        ReturnService.ApplyReturnToBook(book, quantity: 2, BookCondition.Lost);

        book.Distributed.Should().Be(28);  // 30 - 2
        book.Lost.Should().Be(5);          // 3 + 2
    }

    // --- ReverseReturnFromBook: cancel reversal logic ---

    [Theory]
    [InlineData(BookCondition.Good)]
    [InlineData(BookCondition.Fair)]
    [InlineData(BookCondition.Poor)]
    public void ReverseReturn_GoodFairPoor_IncrementsDistributedOnly(BookCondition condition)
    {
        var book = CreateBook(distributed: 25, damaged: 5, lost: 3);

        ReturnService.ReverseReturnFromBook(book, quantity: 5, condition);

        book.Distributed.Should().Be(30);  // 25 + 5
        book.Damaged.Should().Be(5);       // unchanged
        book.Lost.Should().Be(3);          // unchanged
    }

    [Fact]
    public void ReverseReturn_Damaged_IncrementsDistributedAndDecrementsDamaged()
    {
        var book = CreateBook(distributed: 27, damaged: 8);

        ReturnService.ReverseReturnFromBook(book, quantity: 3, BookCondition.Damaged);

        book.Distributed.Should().Be(30);  // 27 + 3
        book.Damaged.Should().Be(5);       // 8 - 3
    }

    [Fact]
    public void ReverseReturn_Lost_IncrementsDistributedAndDecrementsLost()
    {
        var book = CreateBook(distributed: 28, lost: 5);

        ReturnService.ReverseReturnFromBook(book, quantity: 2, BookCondition.Lost);

        book.Distributed.Should().Be(30);  // 28 + 2
        book.Lost.Should().Be(3);          // 5 - 2
    }

    // --- ApplyReturn then ReverseReturn: round-trip ---

    [Theory]
    [InlineData(BookCondition.Good)]
    [InlineData(BookCondition.Damaged)]
    [InlineData(BookCondition.Lost)]
    public void ApplyThenReverse_RestoresOriginalValues(BookCondition condition)
    {
        var book = CreateBook(distributed: 30, damaged: 5, lost: 3);
        var originalDistributed = book.Distributed;
        var originalDamaged = book.Damaged;
        var originalLost = book.Lost;

        ReturnService.ApplyReturnToBook(book, quantity: 4, condition);
        ReturnService.ReverseReturnFromBook(book, quantity: 4, condition);

        book.Distributed.Should().Be(originalDistributed);
        book.Damaged.Should().Be(originalDamaged);
        book.Lost.Should().Be(originalLost);
    }

    // --- Create: book not found ---

    [Fact]
    public async Task Create_BookNotFound_ThrowsNotFoundException()
    {
        _bookRepo.GetByIdAsync(999).Returns((Book?)null);
        _refService.GenerateAsync(SlipType.Return, 1).Returns("RTN2026000001");
        var request = new CreateReturnSlipRequest
        {
            AcademicYearId = 1, StudentId = 1, ReturnedById = 1,
            Items = new() { new() { BookId = 999, Quantity = 1, Condition = BookCondition.Good } }
        };

        var act = () => _sut.CreateAsync(request, userId: 1);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    // --- Create: stock updates per item ---

    [Fact]
    public async Task Create_MultipleItems_UpdatesEachBookCorrectly()
    {
        var book1 = CreateBook(1, distributed: 20, damaged: 2);
        var book2 = CreateBook(2, distributed: 15, lost: 1);
        _bookRepo.GetByIdAsync(1).Returns(book1);
        _bookRepo.GetByIdAsync(2).Returns(book2);
        _refService.GenerateAsync(SlipType.Return, 1).Returns("RTN2026000001");

        var request = new CreateReturnSlipRequest
        {
            AcademicYearId = 1, StudentId = 1, ReturnedById = 1,
            Items = new()
            {
                new() { BookId = 1, Quantity = 3, Condition = BookCondition.Damaged },
                new() { BookId = 2, Quantity = 2, Condition = BookCondition.Good }
            }
        };

        try { await _sut.CreateAsync(request, userId: 1); } catch { }

        book1.Distributed.Should().Be(17);  // 20 - 3
        book1.Damaged.Should().Be(5);       // 2 + 3
        book2.Distributed.Should().Be(13);  // 15 - 2
        book2.Lost.Should().Be(1);          // unchanged for Good condition
    }
}
