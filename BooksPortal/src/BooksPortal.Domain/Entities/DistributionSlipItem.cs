using BooksPortal.Domain.Common;

namespace BooksPortal.Domain.Entities;

public class DistributionSlipItem : BaseEntity
{
    public int DistributionSlipId { get; set; }
    public int BookId { get; set; }
    public int Quantity { get; set; }

    public DistributionSlip DistributionSlip { get; set; } = null!;
    public Book Book { get; set; } = null!;
}
