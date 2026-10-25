using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace neotonic.Models
{
    [Table("products")]
    [Index(nameof(NameEn), IsUnique = true, Name = "ix_products_name_en")]
    [Index(nameof(NameAr), IsUnique = true, Name = "ix_products_name_ar")]
    [Index(nameof(ProductNumber), IsUnique = true, Name = "ix_products_product_number")]
    [Index(nameof(AccountingCode), IsUnique = true, Name = "ix_products_accounting_code")]
    public class Product
    {
           
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(200)]
        [Column("name_en")]
        public string NameEn { get; set; } = string.Empty;

        [Required, MaxLength(200)]
        [Column("name_ar")]
        public string NameAr { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        [Column("product_number")]
        public string ProductNumber { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        [Column("accounting_code")]
        public string AccountingCode { get; set; } = string.Empty;
        [MaxLength(1000)]
        [Column("notes")]
        public string? Notes { get; set; }

        public int CategoryId { get; set; }

        [ForeignKey(nameof(CategoryId))]
        public Category Category { get; set; } = null!;


        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Column("is_serialized")]
        public bool IsSerialized { get; set; } = true;
    }

}

