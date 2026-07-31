namespace HotelManagement.Core.Services;

/// <summary>
/// Inventory calculation logic from frmPurchaseInventory.vb and frmStock.vb.
/// </summary>
public class InventoryService
{
    /// <summary>
    /// Calculate total price for purchase inventory.
    /// TotalPrice = Quantity × UnitPrice
    /// </summary>
    public decimal CalculateTotalPrice(int quantity, decimal unitPrice)
    {
        return quantity * unitPrice;
    }

    /// <summary>
    /// Calculate total volume for liquor stock.
    /// TotalVolume = (NoOfBottles × Volume) + ExistingVolume
    /// </summary>
    public decimal CalculateTotalVolume(int noOfBottles, decimal volume, decimal existingVolume)
    {
        return (noOfBottles * volume) + existingVolume;
    }
}
