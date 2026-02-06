using BooksPortal.Application.Features.Distribution.DTOs;
using BooksPortal.Application.Features.Returns.DTOs;
using BooksPortal.Application.Features.TeacherIssues.DTOs;

namespace BooksPortal.Application.Common.Interfaces;

public interface IPdfService
{
    byte[] GenerateDistributionSlip(DistributionSlipResponse slip);
    byte[] GenerateReturnSlip(ReturnSlipResponse slip);
    byte[] GenerateTeacherIssueSlip(TeacherIssueResponse issue);
}
