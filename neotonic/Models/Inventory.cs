// Models/Inventory.cs  (NEW)
// A real inventory: Receiving, Preparing, Filling, Products-produced, Products-ready-to-sell.
// Each real inventory has the three zones (Quarantine / Accepted / Rejected).
// Virtual locations (Consumed, Scrap, Supplier, Production floor) are NOT inventories:
// they are Location rows with Kind = Virtual and no InventoryId.
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace neotonic.Models
{
    [Table("inventories")]
    [Index(nameof(Code), IsUnique = true, Name = "ix_inventories_code")]
    public class Inventory
    {
        [Key]
        public int Id { get; set; }

        // Short code used in shelf labels: I1, I2, FIL, PRD, PTS.
        [Required, MaxLength(10)]
        [Column("code")]
        public string Code { get; set; } = string.Empty;

        [Required, MaxLength(200)]
        [Column("name_en")]
        public string NameEn { get; set; } = string.Empty;

        [Required, MaxLength(200)]
        [Column("name_ar")]
        public string NameAr { get; set; } = string.Empty;

        [Column("sort_order")]
        public int SortOrder { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        // A box entering this inventory lands in its Quarantine and waits for a certificate there.
        public ICollection<Location> Locations { get; set; } = new List<Location>();
    }

    public static class InventoryIds
    {
        public const int Receiving = 1;
        public const int Preparing = 2;
        public const int Filling = 3;
        public const int ProductsProduced = 4;
        public const int ProductsReadyToSell = 5;
    }

    public static class InventoryCodes
    {
        public const string Receiving = "I1";
        public const string Preparing = "I2";
        public const string Filling = "FIL";
        public const string ProductsProduced = "PRD";
        public const string ProductsReadyToSell = "PTS";
    }
}