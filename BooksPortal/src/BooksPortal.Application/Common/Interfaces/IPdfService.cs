using BooksPortal.Application.Features.Distribution.DTOs;
using BooksPortal.Application.Features.Returns.DTOs;
using BooksPortal.Application.Features.TeacherIssues.DTOs;

namespace BooksPortal.Application.Common.Interfaces;

public interface IPdfService
{
    Task<byte[]> GenerateDistributionSlipAsync(DistributionSlipResponse slip);
    Task<byte[]> GenerateReturnSlipAsync(ReturnSlipResponse slip);
    Task<byte[]> GenerateTeacherIssueSlipAsync(TeacherIssueResponse issue);
    Task<byte[]> GenerateTeacherReturnSlipAsync(TeacherReturnSlipResponse slip);
}
