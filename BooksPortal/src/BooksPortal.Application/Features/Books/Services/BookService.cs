using BooksPortal.Application.Common.Exceptions;
using BooksPortal.Application.Common.Interfaces;
using BooksPortal.Application.Common.Models;
using BooksPortal.Application.Features.Books.DTOs;
using BooksPortal.Application.Features.Books.Interfaces;
using BooksPortal.Domain.Entities;
using BooksPortal.Domain.Enums;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace BooksPortal.Application.Features.Books.Services;

public class BookService : IBookService
{
    private readonly IRepository<Book> _bookRepo;
    private readonly IRepository<StockEntry> _stockEntryRepo;
    private readonly IUnitOfWork _unitOfWork;

    public BookService(
        IRepository<Book> bookRepo,
        IRepository<StockEntry> stockEntryRepo,
        IUnitOfWork unitOfWork)
    {
        _bookRepo = bookRepo;
        _stockEntryRepo = stockEntryRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task<PaginatedList<BookResponse>> GetPagedAsync(int pageNumber, int pageSize, int? subjectId = null, string? search = null)
    {
        var query = _bookRepo.Query().Include(b => b.Subject).AsQueryable();

        if (subjectId.HasValue)
            query = query.Where(b => b.SubjectId == subjectId.Value);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(b => b.Title.Contains(search) || b.Code.Contains(search) || (b.ISBN != null && b.ISBN.Contains(search)));

        var projected = query.OrderBy(b => b.Title).Select(b => new BookResponse
        {
            Id = b.Id,
            ISBN = b.ISBN,
            Code = b.Code,
            Title = b.Title,
            Author = b.Author,
            Edition = b.Edition,
            Publisher = b.Publisher,
            PublishedYear = b.PublishedYear,
            SubjectId = b.SubjectId,
            SubjectName = b.Subject.Name,
            Grade = b.Grade,
            TotalStock = b.TotalStock,
            Distributed = b.Distributed,
            WithTeachers = b.WithTeachers,
            Damaged = b.Damaged,
            Lost = b.Lost
        });

        return await PaginatedList<BookResponse>.CreateAsync(projected, pageNumber, pageSize);
    }

    public async Task<BookResponse> GetByIdAsync(int id)
    {
        var book = await _bookRepo.Query()
            .Include(b => b.Subject)
            .FirstOrDefaultAsync(b => b.Id == id)
            ?? throw new NotFoundException(nameof(Book), id);

        return new BookResponse
        {
            Id = book.Id, ISBN = book.ISBN, Code = book.Code, Title = book.Title,
            Author = book.Author, Edition = book.Edition, Publisher = book.Publisher,
            PublishedYear = book.PublishedYear, SubjectId = book.SubjectId,
            SubjectName = book.Subject.Name, Grade = book.Grade,
            TotalStock = book.TotalStock, Distributed = book.Distributed,
            WithTeachers = book.WithTeachers, Damaged = book.Damaged, Lost = book.Lost
        };
    }

    public async Task<BookResponse> CreateAsync(CreateBookRequest request)
    {
        if (await _bookRepo.AnyAsync(b => b.Code == request.Code))
            throw new BusinessRuleException($"Book with code '{request.Code}' already exists.");

        var entity = request.Adapt<Book>();
        _bookRepo.Add(entity);
        await _unitOfWork.SaveChangesAsync();
        return await GetByIdAsync(entity.Id);
    }

    public async Task<BookResponse> UpdateAsync(int id, CreateBookRequest request)
    {
        var entity = await _bookRepo.GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(Book), id);

        if (await _bookRepo.AnyAsync(b => b.Code == request.Code && b.Id != id))
            throw new BusinessRuleException($"Book with code '{request.Code}' already exists.");

        entity.ISBN = request.ISBN;
        entity.Code = request.Code;
        entity.Title = request.Title;
        entity.Author = request.Author;
        entity.Edition = request.Edition;
        entity.Publisher = request.Publisher;
        entity.PublishedYear = request.PublishedYear;
        entity.SubjectId = request.SubjectId;
        entity.Grade = request.Grade;
        _bookRepo.Update(entity);
        await _unitOfWork.SaveChangesAsync();
        return await GetByIdAsync(entity.Id);
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _bookRepo.GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(Book), id);

        if (entity.Distributed > 0 || entity.WithTeachers > 0)
            throw new BusinessRuleException("Cannot delete a book with outstanding distributions or teacher issues.");

        _bookRepo.SoftDelete(entity);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<StockEntryResponse> AddStockAsync(int bookId, AddStockRequest request, int userId)
    {
        var book = await _bookRepo.GetByIdAsync(bookId)
            ?? throw new NotFoundException(nameof(Book), bookId);

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var entry = new StockEntry
            {
                BookId = bookId,
                AcademicYearId = request.AcademicYearId,
                Quantity = request.Quantity,
                Source = request.Source,
                Notes = request.Notes,
                EnteredById = userId,
                EnteredAt = DateTime.UtcNow
            };
            _stockEntryRepo.Add(entry);

            book.TotalStock += request.Quantity;
            _bookRepo.Update(book);

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

            return entry.Adapt<StockEntryResponse>();
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    public async Task<PaginatedList<StockEntryResponse>> GetStockEntriesAsync(int bookId, int pageNumber, int pageSize)
    {
        if (!await _bookRepo.AnyAsync(b => b.Id == bookId))
            throw new NotFoundException(nameof(Book), bookId);

        var query = _stockEntryRepo.Query()
            .Where(e => e.BookId == bookId)
            .OrderByDescending(e => e.EnteredAt)
            .Select(e => new StockEntryResponse
            {
                Id = e.Id,
                BookId = e.BookId,
                AcademicYearId = e.AcademicYearId,
                Quantity = e.Quantity,
                Source = e.Source,
                Notes = e.Notes,
                EnteredById = e.EnteredById,
                EnteredAt = e.EnteredAt
            });

        return await PaginatedList<StockEntryResponse>.CreateAsync(query, pageNumber, pageSize);
    }

    public async Task<PaginatedList<StockMovementResponse>> GetStockMovementsAsync(int bookId, int pageNumber, int pageSize)
    {
        if (!await _bookRepo.AnyAsync(b => b.Id == bookId))
            throw new NotFoundException(nameof(Book), bookId);

        // StockMovement doesn't extend BaseEntity, so we query through DbContext via the Book's nav property
        var book = await _bookRepo.Query()
            .Include(b => b.StockMovements)
            .FirstOrDefaultAsync(b => b.Id == bookId);

        var items = book!.StockMovements
            .OrderByDescending(m => m.ProcessedAt)
            .Select(m => new StockMovementResponse
            {
                Id = m.Id, BookId = m.BookId, MovementType = m.MovementType,
                Quantity = m.Quantity, ReferenceId = m.ReferenceId,
                ReferenceType = m.ReferenceType, Notes = m.Notes,
                ProcessedById = m.ProcessedById, ProcessedAt = m.ProcessedAt
            }).ToList();

        var totalCount = items.Count;
        var pagedItems = items
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new PaginatedList<StockMovementResponse>(pagedItems, totalCount, pageNumber, pageSize);
    }

    public async Task AdjustStockAsync(int bookId, AdjustStockRequest request, int userId)
    {
        var book = await _bookRepo.GetByIdAsync(bookId)
            ?? throw new NotFoundException(nameof(Book), bookId);

        switch (request.MovementType)
        {
            case MovementType.MarkDamaged:
                book.Damaged += request.Quantity;
                break;
            case MovementType.MarkLost:
                book.Lost += request.Quantity;
                break;
            case MovementType.WriteOff:
                book.TotalStock -= request.Quantity;
                break;
            case MovementType.Adjustment:
                book.TotalStock += request.Quantity;
                break;
            default:
                throw new BusinessRuleException($"Movement type '{request.MovementType}' is not valid for stock adjustment.");
        }

        _bookRepo.Update(book);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<List<BookResponse>> SearchAsync(string query)
    {
        var books = await _bookRepo.Query()
            .Include(b => b.Subject)
            .Where(b => b.Title.Contains(query) || b.Code.Contains(query) || (b.ISBN != null && b.ISBN.Contains(query)))
            .Take(20)
            .ToListAsync();

        return books.Select(b => new BookResponse
        {
            Id = b.Id, ISBN = b.ISBN, Code = b.Code, Title = b.Title,
            Author = b.Author, SubjectId = b.SubjectId, SubjectName = b.Subject.Name,
            Grade = b.Grade, TotalStock = b.TotalStock, Distributed = b.Distributed,
            WithTeachers = b.WithTeachers, Damaged = b.Damaged, Lost = b.Lost
        }).ToList();
    }
}
