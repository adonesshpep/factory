using neotonic.Models;
using System.ComponentModel.DataAnnotations;

namespace neotonic.DTOs
{
    public class SetCertificateResultDto
    {
        // Pending is the initial state, not a target. Only Accepted or Rejected are accepted here.
        [Required, EnumDataType(typeof(AnalysisResults))]
        public AnalysisResults? Result { get; set; }

        // Required iff Result = Rejected; forbidden otherwise.
        [MaxLength(1000)]
        public string? RejectionReason { get; set; }
    }
}