// Models/Department.cs
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace neotonic.Models
{
    [Table("departments")]
    [Index(nameof(DepartmentNumber), IsUnique = true, Name = "ix_departments_department_number")]
    [Index(nameof(NameEn), IsUnique = true, Name = "ix_departments_name_en")]
    [Index(nameof(NameAr), IsUnique = true, Name = "ix_departments_name_ar")]
    public class Department
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(50)]
        [Column("department_number")]
        public string DepartmentNumber { get; set; } = string.Empty;

        [Required, MaxLength(200)]
        [Column("name_en")]
        public string NameEn { get; set; } = string.Empty;

        [Required, MaxLength(200)]
        [Column("name_ar")]
        public string NameAr { get; set; } = string.Empty;

        [Column("is_active")]
        public bool IsActive { get; set; } = true;
    }
}