// Models/Shelf.cs  (NEW)
// Admin-created shelves inside ONE real inventory location.
// The label shown to users is derived, never stored: inventory code + "-" + zone letter + number,
// e.g. I1-B12 (Receiving, Accepted, shelf 12). Zone letter: Quarantine=A, Accepted=B, Rejected=C.
// Virtual locations have no shelves.
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace neotonic.Models
{
    [Table("shelves")]
    [Index(nameof(LocationId), nameof(Number), IsUnique = true, Name = "ix_shelves_location_number")]
    // Composite key target so UOM(ShelfId, LocationId) can FK to it and a box can only be on a shelf
    // of its own location.
    [Index(nameof(Id), nameof(LocationId), IsUnique = true, Name = "ix_shelves_id_location")]
    public class Shelf
    {
        [Key]
        public int Id { get; set; }

        public int LocationId { get; set; }

        [ForeignKey(nameof(LocationId))]
        public Location Location { get; set; } = null!;

        [Column("number")]
        public int Number { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; } = true;
    }
}