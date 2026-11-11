using System.ComponentModel.DataAnnotations;

namespace neotonic.DTOs.Updates
{
    public class UpdateShelfDto
    {
        [Range(1, int.MaxValue)]
        public int? Number { get; set; }
    }
}