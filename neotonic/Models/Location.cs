// Models/Location.cs  (CHANGED)
// Real inventory location = (InventoryId, Zone), unique per pair.
// Virtual location        = no inventory, no zone, no shelves (Kind = Virtual).
// Virtual rows: Consumed, Scrap, Supplier, Production floor.
using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace neotonic.Models
{
    [Table("locations")]
    [Index(nameof(InventoryId), nameof(Zone), IsUnique = true, Name = "ix_locations_inventory_zone")]
    [Index(nameof(Code), IsUnique = true, Name = "ix_locations_code")]
    public class Location
    {
        [Key]
        public int Id { get; set; }

        // Stable code: "I1-QUAR", "I1-ACC", "I1-REJ", "VIRT-CONSUMED", ...
        [Required, MaxLength(50)]
        [Column("code")]
        public string Code { get; set; } = string.Empty;

        [Required, MaxLength(200)]
        [Column("name_en")]
        public string NameEn { get; set; } = string.Empty;

        [Required, MaxLength(200)]
        [Column("name_ar")]
        public string NameAr { get; set; } = string.Empty;

        [Column("kind")]
        public LocationKinds Kind { get; set; } = LocationKinds.Inventory;

        // Set for real locations, null for virtual ones.
        public int? InventoryId { get; set; }

        [ForeignKey(nameof(InventoryId))]
        public Inventory? Inventory { get; set; }

        // Set for real locations, null for virtual ones.
        [Column("zone")]
        public InventoryZones? Zone { get; set; }

        [Column("sort_order")]
        public int SortOrder { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        public ICollection<Shelf> Shelves { get; set; } = new List<Shelf>();
    }

    // Stable IDs so seeds and code never use magic numbers.
    public static class LocationIds
    {
        // Real inventory zones (inventory 1..5, zones 1..3, Id = inv*10 + zone).
        public const int ReceivingQuarantine = 11;
        public const int ReceivingAccepted = 12;
        public const int ReceivingRejected = 13;

        public const int PreparingQuarantine = 21;
        public const int PreparingAccepted = 22;
        public const int PreparingRejected = 23;

        public const int FillingQuarantine = 31;
        public const int FillingAccepted = 32;
        public const int FillingRejected = 33;

        public const int ProductsProducedQuarantine = 41;
        public const int ProductsProducedAccepted = 42;
        public const int ProductsProducedRejected = 43;

        public const int ProductsReadyToSellQuarantine = 51;
        public const int ProductsReadyToSellAccepted = 52;
        public const int ProductsReadyToSellRejected = 53;

        // Virtual locations.
        public const int Consumed = 901;
        public const int Scrap = 902;
        public const int SupplierReturns = 903;
        public const int ProductionFloor = 904;
        public const int VirtualWashing = 911;
        public const int VirtualPurification = 912;
        public const int VirtualSterilization = 913;
        public const int VirtualDrying = 914;
    }
}