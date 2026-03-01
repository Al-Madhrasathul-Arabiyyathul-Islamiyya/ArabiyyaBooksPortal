using BooksPortal.Application.Common.Models;
using BooksPortal.Application.Features.Books.DTOs;

namespace BooksPortal.Application.Features.Books.Interfaces;

public interface IBookService
{
    Task<PaginatedList<BookResponse>> GetPagedAsync(int pageNumber, int pageSize, int? subjectId = null, string? search = null);
    Task<BookResponse> GetByIdAsync(int id);
    Task<BookResponse> CreateAsync(CreateBookRequest request);
    Task<BookResponse> UpdateAsync(int id, CreateBookRequest request);
    Task DeleteAsync(int id);
    Task<StockEntryResponse> AddStockAsync(int bookId, AddStockRequest request, int userId);
    Task<PaginatedList<StockEntryResponse>> GetStockEntriesAsync(int bookId, int pageNumber, int pageSize);
    Task<PaginatedList<StockMovementResponse>> GetStockMovementsAsync(int bookId, int pageNumber, int pageSize);
    Task AdjustStockAsync(int bookId, AdjustStockRequest request, int userId);
    Task<List<BookResponse>> SearchAsync(string query);
}
