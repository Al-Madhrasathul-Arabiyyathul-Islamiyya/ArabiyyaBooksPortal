using BooksPortal.Application.Common.Exceptions;
using BooksPortal.Application.Common.Interfaces;
using BooksPortal.Application.Features.Books.DTOs;
using BooksPortal.Application.Features.Books.Services;
using BooksPortal.Domain.Entities;
using BooksPortal.Domain.Enums;
using FluentAssertions;
using NSubstitute;

namespace BooksPortal.UnitTests.Features.Books;

public class BookServiceTests
{
    private readonly IRepository<Book> _bookRepo;
    private readonly IRepository<StockEntry> _stockEntryRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly BookService _sut;

    public BookServiceTests()
    {
        _bookRepo = Substitute.For<IRepository<Book>>();
        _stockEntryRepo = Substitute.For<IRepository<StockEntry>>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _sut = new BookService(_bookRepo, _stockEntryRepo, _unitOfWork);
    }

    private static Book CreateBook(int id = 1, int totalStock = 100, int distributed = 20,
        int withTeachers = 10, int damaged = 5, int lost = 3)
    {
        return new Book
        {
            Id = id, Code = $"BK{id:D3}", Title = $"Book {id}",
            SubjectId = 1, Subject = new Subject { Id = 1, Name = "Math", Code = "MAT" },
            TotalStock = totalStock, Distributed = distributed,
            WithTeachers = withTeachers, Damaged = damaged, Lost = lost
        };
    }

    // --- AdjustStock Tests ---

    [Fact]
    public async Task AdjustStock_MarkDamaged_IncrementsDamagedCounter()
    {
        var book = CreateBook(damaged: 5);
        _bookRepo.GetByIdAsync(1).Returns(book);
        var request = new AdjustStockRequest { MovementType = MovementType.MarkDamaged, Quantity = 3 };

        await _sut.AdjustStockAsync(1, request, userId: 1);

        book.Damaged.Should().Be(8);
        _bookRepo.Received(1).Update(book);
        await _unitOfWork.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task AdjustStock_MarkLost_IncrementsLostCounter()
    {
        var book = CreateBook(lost: 3);
        _bookRepo.GetByIdAsync(1).Returns(book);
        var request = new AdjustStockRequest { MovementType = MovementType.MarkLost, Quantity = 2 };

        await _sut.AdjustStockAsync(1, request, userId: 1);

        book.Lost.Should().Be(5);
    }

    [Fact]
    public async Task AdjustStock_WriteOff_DecrementsTotalStock()
    {
        var book = CreateBook(totalStock: 100);
        _bookRepo.GetByIdAsync(1).Returns(book);
        var request = new AdjustStockRequest { MovementType = MovementType.WriteOff, Quantity = 10 };

        await _sut.AdjustStockAsync(1, request, userId: 1);

        book.TotalStock.Should().Be(90);
    }

    [Fact]
    public async Task AdjustStock_Adjustment_IncrementsTotalStock()
    {
        var book = CreateBook(totalStock: 100);
        _bookRepo.GetByIdAsync(1).Returns(book);
        var request = new AdjustStockRequest { MovementType = MovementType.Adjustment, Quantity = 15 };

        await _sut.AdjustStockAsync(1, request, userId: 1);

        book.TotalStock.Should().Be(115);
    }

    [Theory]
    [InlineData(MovementType.Distribution)]
    [InlineData(MovementType.Return)]
    [InlineData(MovementType.TeacherIssue)]
    [InlineData(MovementType.TeacherReturn)]
    [InlineData(MovementType.StockEntry)]
    public async Task AdjustStock_InvalidMovementType_ThrowsBusinessRuleException(MovementType type)
    {
        var book = CreateBook();
        _bookRepo.GetByIdAsync(1).Returns(book);
        var request = new AdjustStockRequest { MovementType = type, Quantity = 1 };

        var act = () => _sut.AdjustStockAsync(1, request, userId: 1);

        await act.Should().ThrowAsync<BusinessRuleException>()
            .WithMessage("*not valid for stock adjustment*");
    }

    [Fact]
    public async Task AdjustStock_BookNotFound_ThrowsNotFoundException()
    {
        _bookRepo.GetByIdAsync(999).Returns((Book?)null);
        var request = new AdjustStockRequest { MovementType = MovementType.MarkDamaged, Quantity = 1 };

        var act = () => _sut.AdjustStockAsync(999, request, userId: 1);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    // --- Delete Tests ---

    [Fact]
    public async Task Delete_BookWithNoOutstanding_SoftDeletes()
    {
        var book = CreateBook(distributed: 0, withTeachers: 0);
        _bookRepo.GetByIdAsync(1).Returns(book);

        await _sut.DeleteAsync(1);

        _bookRepo.Received(1).SoftDelete(book);
        await _unitOfWork.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task Delete_BookWithDistributed_ThrowsBusinessRuleException()
    {
        var book = CreateBook(distributed: 5, withTeachers: 0);
        _bookRepo.GetByIdAsync(1).Returns(book);

        var act = () => _sut.DeleteAsync(1);

        await act.Should().ThrowAsync<BusinessRuleException>()
            .WithMessage("*outstanding distributions*");
    }

    [Fact]
    public async Task Delete_BookWithTeacherIssues_ThrowsBusinessRuleException()
    {
        var book = CreateBook(distributed: 0, withTeachers: 3);
        _bookRepo.GetByIdAsync(1).Returns(book);

        var act = () => _sut.DeleteAsync(1);

        await act.Should().ThrowAsync<BusinessRuleException>()
            .WithMessage("*outstanding distributions or teacher issues*");
    }

    [Fact]
    public async Task Delete_BookNotFound_ThrowsNotFoundException()
    {
        _bookRepo.GetByIdAsync(999).Returns((Book?)null);

        var act = () => _sut.DeleteAsync(999);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    // --- AddStock Tests ---

    [Fact]
    public async Task AddStock_ValidRequest_IncrementsTotalStockAndCreatesEntry()
    {
        var book = CreateBook(totalStock: 50);
        _bookRepo.GetByIdAsync(1).Returns(book);
        var request = new AddStockRequest { AcademicYearId = 1, Quantity = 25, Source = "MOE" };

        await _sut.AddStockAsync(1, request, userId: 1);

        book.TotalStock.Should().Be(75);
        _stockEntryRepo.Received(1).Add(Arg.Is<StockEntry>(e =>
            e.BookId == 1 && e.Quantity == 25 && e.Source == "MOE" && e.EnteredById == 1));
        _bookRepo.Received(1).Update(book);
        await _unitOfWork.Received(1).BeginTransactionAsync();
        await _unitOfWork.Received(1).CommitTransactionAsync();
    }

    [Fact]
    public async Task AddStock_BookNotFound_ThrowsNotFoundException()
    {
        _bookRepo.GetByIdAsync(999).Returns((Book?)null);
        var request = new AddStockRequest { AcademicYearId = 1, Quantity = 10 };

        var act = () => _sut.AddStockAsync(999, request, userId: 1);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    // --- Create Tests ---

    [Fact]
    public async Task Create_DuplicateCode_ThrowsBusinessRuleException()
    {
        _bookRepo.AnyAsync(Arg.Any<System.Linq.Expressions.Expression<Func<Book, bool>>>())
            .Returns(true);
        var request = new CreateBookRequest { Code = "EXISTING", Title = "Test" };

        var act = () => _sut.CreateAsync(request);

        await act.Should().ThrowAsync<BusinessRuleException>()
            .WithMessage("*already exists*");
    }
}
