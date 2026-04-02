using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelManagement.Core.Models;

public class TaxRoom
{
    [Key]
    public int BillID { get; set; }

    public decimal HEduTax { get; set; }

    public decimal HEduTaxAmount { get; set; }

    public decimal EducationalTax { get; set; }

    public decimal EducationalTaxAmount { get; set; }

    [ForeignKey(nameof(BillID))]
    public CheckoutRoom? CheckoutRoom { get; set; }
}
