using BooksPortal.Application.Features.MasterData.DTOs;

namespace BooksPortal.Application.Features.MasterData.Interfaces;

public interface IMasterDataHierarchyBulkService
{
    Task<HierarchyBulkUpsertResponse> UpsertAsync(HierarchyBulkUpsertRequest request);
}
