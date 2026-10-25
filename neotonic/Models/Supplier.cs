// Models/Supplier.cs
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace neotonic.Models
{
    [Table("suppliers")]
    [Index(nameof(SupplierNumber), IsUnique = true, Name = "ix_suppliers_supplier_number")]
    [Index(nameof(NameEn), IsUnique = true, Name = "ix_suppliers_name_en")]
    [Index(nameof(NameAr), IsUnique = true, Name = "ix_suppliers_name_ar")]
    public class Supplier
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(50)]
        [Column("supplier_number")]
        public string SupplierNumber { get; set; } = string.Empty;

        [Required, MaxLength(200)]
        [Column("name_en")]
        public string NameEn { get; set; } = string.Empty;

        [Required, MaxLength(200)]
        [Column("name_ar")]
        public string NameAr { get; set; } = string.Empty;

        [MaxLength(200)]
        [Column("contact")]
        public string? Contact { get; set; }

        [MaxLength(30)]
        [Column("phone_number")]
        public string? PhoneNumber { get; set; }

        [MaxLength(200)]
        [Column("email")]
        public string? Email { get; set; }

        [MaxLength(1000)]
        [Column("notes")]
        public string? Notes { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; } = true;
    }
}