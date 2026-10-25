using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using neotonic.DTOs;
using neotonic.DTOs.Creates;
using neotonic.DTOs.Responses;
using neotonic.DTOs.Updates;
using neotonic.Helpers;
using neotonic.Models;
using neotonic.Services;

namespace neotonic.Controllers
{
    [Route("api/certificatesofanalysis")]
    [ApiController]
    [Authorize(Policy = "ActiveUser")]
    public class CertificatesOfAnalysisController : ControllerBase
    {
        private const string DateBeforeDeliveryMessage =
            "Analysis date cannot be before the sample's delivery date";

        private readonly AppDbContext _context;
        private readonly IAuditLogService _auditLogService;

        public CertificatesOfAnalysisController(AppDbContext context, IAuditLogService auditLogService)
        {
            _context = context;
            _auditLogService = auditLogService;
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create(CreateCertificateOfAnalysisDto dto)
        {
            var sample = await _context.SupplierSamples.FindAsync(dto.SupplierSampleId);
            if (sample is null || !sample.IsActive)
            {
                return BadRequest(new { message = "Supplier sample does not exist or is inactive" });
            }

            var analysisDate = dto.AnalysisDate!.Value;
            if (analysisDate.Date < sample.DeliveryDate.Date)
            {
                return BadRequest(new { message = DateBeforeDeliveryMessage });
            }

            var result = dto.Result!.Value;
            var reason = Normalize(dto.RejectionReason);
            var resultError = ValidateResult(result, reason);
            if (resultError is not null)
            {
                return BadRequest(new { message = resultError });
            }

            var certificate = new CertificateOfAnalysis
            {
                Number = dto.Number.Trim(),
                AnalysisDate = analysisDate,
                SupplierSampleId = sample.Id,
                PhysicalAppearance = Normalize(dto.PhysicalAppearance),
                MoisturePercentage = dto.MoisturePercentage,
                BacterialTest = Normalize(dto.BacterialTest),
                FungalTest = Normalize(dto.FungalTest),
                MicroscopicAppearance = Normalize(dto.MicroscopicAppearance),
                Color = Normalize(dto.Color),
                Odor = Normalize(dto.Odor),
                Result = result,
                RejectionReason = reason,
                Notes = Normalize(dto.Notes)
            };

            try
            {
                _context.CertificatesOfAnalysis.Add(certificate);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlEx && sqlEx.Number is 2601 or 2627)
            {
                return Conflict(new { message = "A certificate of analysis with this number already exists" });
            }

            await _auditLogService.LogAsync(
                User,
                "CreateCertificateOfAnalysis",
                $"Created certificate of analysis '{certificate.Number}' (Id: {certificate.Id}, Sample: {sample.Number}, Result: {certificate.Result})",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(await ToResponseDto(certificate.Id));
        }

        [HttpPut("update/{id}")]
        public async Task<IActionResult> Update(int id, UpdateCertificateOfAnalysisDto dto)
        {
            var certificate = await _context.CertificatesOfAnalysis.FindAsync(id);
            if (certificate is null)
            {
                return NotFound(new { message = "Certificate of analysis not found" });
            }

            if (!string.IsNullOrWhiteSpace(dto.Number))
                certificate.Number = dto.Number.Trim();

            if (dto.AnalysisDate.HasValue)
                certificate.AnalysisDate = dto.AnalysisDate.Value;

            if (dto.SupplierSampleId.HasValue)
            {
                var newSample = await _context.SupplierSamples.FindAsync(dto.SupplierSampleId.Value);
                if (newSample is null || !newSample.IsActive)
                    return BadRequest(new { message = "Supplier sample does not exist or is inactive" });
                certificate.SupplierSampleId = newSample.Id;
            }

            // If either the date or the sample changed, re-check the date against the final sample
            if (dto.AnalysisDate.HasValue || dto.SupplierSampleId.HasValue)
            {
                var sample = await _context.SupplierSamples.FindAsync(certificate.SupplierSampleId);
                if (certificate.AnalysisDate.Date < sample!.DeliveryDate.Date)
                    return BadRequest(new { message = DateBeforeDeliveryMessage });
            }

            if (dto.PhysicalAppearance != null)
                certificate.PhysicalAppearance = Normalize(dto.PhysicalAppearance);

            if (dto.MoisturePercentage.HasValue)
                certificate.MoisturePercentage = dto.MoisturePercentage.Value;

            if (dto.BacterialTest != null)
                certificate.BacterialTest = Normalize(dto.BacterialTest);

            if (dto.FungalTest != null)
                certificate.FungalTest = Normalize(dto.FungalTest);

            if (dto.MicroscopicAppearance != null)
                certificate.MicroscopicAppearance = Normalize(dto.MicroscopicAppearance);

            if (dto.Color != null)
                certificate.Color = Normalize(dto.Color);

            if (dto.Odor != null)
                certificate.Odor = Normalize(dto.Odor);

            if (dto.Notes != null)
                certificate.Notes = Normalize(dto.Notes);

            // Result + rejection reason are validated together on the final values
            if (dto.Result.HasValue)
            {
                certificate.Result = dto.Result.Value;

                // Switching to Accepted without sending a reason clears the old reason
                if (certificate.Result == AnalysisResults.Accepted && dto.RejectionReason is null)
                    certificate.RejectionReason = null;
            }

            if (dto.RejectionReason != null)
                certificate.RejectionReason = Normalize(dto.RejectionReason);

            var resultError = ValidateResult(certificate.Result, certificate.RejectionReason);
            if (resultError is not null)
            {
                return BadRequest(new { message = resultError });
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlEx && sqlEx.Number is 2601 or 2627)
            {
                return Conflict(new { message = "A certificate of analysis with this number already exists" });
            }

            await _auditLogService.LogAsync(
                User,
                "UpdateCertificateOfAnalysis",
                $"Updated certificate of analysis (Id: {certificate.Id}, Number: {certificate.Number}, Result: {certificate.Result})",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(await ToResponseDto(certificate.Id));
        }

        [HttpGet("index")]
        public async Task<IActionResult> Index(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] int? supplierSampleId = null,
            [FromQuery] AnalysisResults? result = null,
            [FromQuery] int? contractReviewId = null)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;

            var query = QueryWithIncludes();

            if (supplierSampleId.HasValue)
                query = query.Where(c => c.SupplierSampleId == supplierSampleId.Value);

            if (result.HasValue)
                query = query.Where(c => c.Result == result.Value);
            if (contractReviewId.HasValue)
                query = query.Where(c => c.SupplierSample.ContractReviewId == contractReviewId.Value);

            var ordered = query
                .OrderByDescending(c => c.AnalysisDate)
                .ThenByDescending(c => c.Id);

            var totalCount = await ordered.CountAsync();

            var certificates = await ordered
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new PagedResultDto<CertificateOfAnalysisResponseDto>
            {
                Items = certificates.Select(ToDto).ToList(),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            });
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("status/{id}")]
        public async Task<IActionResult> ChangeStatus(int id, ChangeStatusDto dto)
        {
            var certificate = await _context.CertificatesOfAnalysis.FindAsync(id);
            if (certificate is null)
            {
                return NotFound(new { message = "Certificate of analysis not found" });
            }

            certificate.IsActive = dto.IsActive;
            await _context.SaveChangesAsync();

            await _auditLogService.LogAsync(
                User,
                dto.IsActive ? "ActivateCertificateOfAnalysis" : "DeactivateCertificateOfAnalysis",
                $"{(dto.IsActive ? "Activated" : "Deactivated")} certificate of analysis '{certificate.Number}' (Id: {certificate.Id})",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(new { message = $"Certificate of analysis {(dto.IsActive ? "activated" : "deactivated")} successfully" });
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var certificate = await _context.CertificatesOfAnalysis.FindAsync(id);
            if (certificate is null)
            {
                return NotFound(new { message = "Certificate of analysis not found" });
            }

            _context.CertificatesOfAnalysis.Remove(certificate);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                var conflictPayload = await _context.GetDeleteConflictsAsync(certificate);
                return Conflict(conflictPayload);
            }

            await _auditLogService.LogAsync(
                User,
                "DeleteCertificateOfAnalysis",
                $"Deleted certificate of analysis '{certificate.Number}' (Id: {certificate.Id})",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(new { message = "Certificate of analysis deleted successfully" });
        }

        // ---- helpers ----

        private static string? Normalize(string? value) =>
            string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        // Rejected needs a reason; Accepted must not have one.
        private static string? ValidateResult(AnalysisResults result, string? reason) => result switch
        {
            AnalysisResults.Rejected when reason is null =>
                "Rejection reason is required when the result is Rejected",
            AnalysisResults.Accepted when reason is not null =>
                "Rejection reason is only allowed when the result is Rejected",
            AnalysisResults.Accepted or AnalysisResults.Rejected => null,
            _ => "Invalid result"
        };

        private IQueryable<CertificateOfAnalysis> QueryWithIncludes() =>
            _context.CertificatesOfAnalysis
                .Include(c => c.SupplierSample).ThenInclude(s => s.ContractReview).ThenInclude(r => r.Supplier)
                .Include(c => c.SupplierSample).ThenInclude(s => s.ContractReview).ThenInclude(r => r.Material);

        private async Task<CertificateOfAnalysisResponseDto> ToResponseDto(int id)
        {
            var certificate = await QueryWithIncludes().FirstAsync(c => c.Id == id);
            return ToDto(certificate);
        }

        private static CertificateOfAnalysisResponseDto ToDto(CertificateOfAnalysis c)
        {
            var sample = c.SupplierSample;
            var review = sample.ContractReview;
            var (resultAr, resultEn) = c.Result.GetBilingualNames();
            var (reviewStatusAr, reviewStatusEn) = review.Status.GetBilingualNames();

            return new CertificateOfAnalysisResponseDto
            {
                Id = c.Id,
                Number = c.Number,
                AnalysisDate = c.AnalysisDate,

                SupplierSampleId = c.SupplierSampleId,
                SupplierSampleNumber = sample.Number,
                SampleDeliveryDate = sample.DeliveryDate,

                SupplierId = review.SupplierId,
                SupplierNumber = review.Supplier.SupplierNumber,
                SupplierNameEn = review.Supplier.NameEn,
                SupplierNameAr = review.Supplier.NameAr,

                ContractReviewId = review.Id,
                ContractReviewNumber = review.Number,
                ContractReviewCreationDate = review.CreationDate,
                ContractReviewExpectedDate = review.ExpectedDate,
                ContractReviewQuantity = review.Quantity,
                ContractReviewStatus = review.Status,
                ContractReviewStatusNameEn = reviewStatusEn,
                ContractReviewStatusNameAr = reviewStatusAr,

                MaterialId = review.MaterialId,
                MaterialNumber = review.Material.MaterialNumber,
                MaterialNameEn = review.Material.NameEn,
                MaterialNameAr = review.Material.NameAr,

                PhysicalAppearance = c.PhysicalAppearance,
                MoisturePercentage = c.MoisturePercentage,
                BacterialTest = c.BacterialTest,
                FungalTest = c.FungalTest,
                MicroscopicAppearance = c.MicroscopicAppearance,
                Color = c.Color,
                Odor = c.Odor,

                Result = c.Result,
                ResultNameEn = resultEn,
                ResultNameAr = resultAr,
                RejectionReason = c.RejectionReason,

                Notes = c.Notes,
                IsActive = c.IsActive
            };
        }
    }
}