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
        private readonly AppDbContext _context;
        private readonly IAuditLogService _auditLogService;
        private readonly INotificationService _notifications;

        public CertificatesOfAnalysisController(AppDbContext context, IAuditLogService auditLogService, INotificationService notifications)
        {
            _context = context;
            _auditLogService = auditLogService;
            _notifications = notifications;
        }

        // ---------------------------------------------------------------
        // create / update (never touch Result)
        // ---------------------------------------------------------------

        [Authorize(Roles = "Admin, Lab")]
        [HttpPost("create")]
        public async Task<IActionResult> Create(CreateCertificateOfAnalysisDto dto)
        {
            var sample = await _context.Samples
                .Include(s => s.ContractReview).ThenInclude(cr => cr!.RFQ).ThenInclude(r => r.Material)
                .Include(s => s.ContractReview).ThenInclude(cr => cr!.Supplier)
                .Include(s => s.Uom).ThenInclude(u => u!.Material)
                .FirstOrDefaultAsync(s => s.Id == dto.SampleId);

            if (sample is null || !sample.IsActive)
                return BadRequest(new { message = "Sample does not exist or is inactive" });

            var analysisDate = dto.AnalysisDate!.Value;
            if (analysisDate.Date < sample.SampleDate.Date)
                return BadRequest(new { message = "Analysis date cannot be before the sample date" });

            var certificate = new CertificateOfAnalysis
            {
                Number = dto.Number.Trim(),
                AnalysisDate = analysisDate,
                SampleId = sample.Id,
                PhysicalAppearance = Normalize(dto.PhysicalAppearance),
                MoisturePercentage = dto.MoisturePercentage,
                BacterialTest = Normalize(dto.BacterialTest),
                FungalTest = Normalize(dto.FungalTest),
                MicroscopicAppearance = Normalize(dto.MicroscopicAppearance),
                Color = Normalize(dto.Color),
                Odor = Normalize(dto.Odor),
                Result = AnalysisResults.Pending,
                RejectionReason = null,
                Notes = Normalize(dto.Notes)
            };

            await using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                _context.CertificatesOfAnalysis.Add(certificate);
                await _context.SaveChangesAsync();

                await _notifications.NotifyAsync(
                    "Admin",
                    $"Certificate {certificate.Number} for sample {sample.Number} is pending a result",
                    NotificationRelatedTypes.CertificatePendingResult,
                    relatedId: certificate.Id);

                await _context.SaveChangesAsync();
                await tx.CommitAsync();
            }
            catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlEx && sqlEx.Number is 2601 or 2627)
            {
                await tx.RollbackAsync();
                return Conflict(new { message = "A certificate of analysis with this number already exists" });
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }

            await _auditLogService.LogAsync(
                User,
                "CreateCertificateOfAnalysis",
                $"Created certificate of analysis '{certificate.Number}' (Id: {certificate.Id}, Sample: {sample.Number}, SampleType: {sample.SampleType}, Result: Pending)",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(await ToResponseDto(certificate.Id));
        }

        [Authorize(Roles = "Admin, Lab")]
        [HttpPut("update/{id}")]
        public async Task<IActionResult> Update(int id, UpdateCertificateOfAnalysisDto dto)
        {
            var certificate = await _context.CertificatesOfAnalysis.FindAsync(id);
            if (certificate is null)
                return NotFound(new { message = "Certificate of analysis not found" });

            if (!string.IsNullOrWhiteSpace(dto.Number))
                certificate.Number = dto.Number.Trim();

            if (dto.AnalysisDate.HasValue)
                certificate.AnalysisDate = dto.AnalysisDate.Value;

            if (dto.SampleId.HasValue)
            {
                var newSample = await _context.Samples.FindAsync(dto.SampleId.Value);
                if (newSample is null || !newSample.IsActive)
                    return BadRequest(new { message = "Sample does not exist or is inactive" });
                certificate.SampleId = newSample.Id;
            }

            if (dto.AnalysisDate.HasValue || dto.SampleId.HasValue)
            {
                var sample = await _context.Samples.FindAsync(certificate.SampleId);
                if (certificate.AnalysisDate.Date < sample!.SampleDate.Date)
                    return BadRequest(new { message = "Analysis date cannot be before the sample date" });
            }

            if (dto.PhysicalAppearance != null) certificate.PhysicalAppearance = Normalize(dto.PhysicalAppearance);
            if (dto.MoisturePercentage.HasValue) certificate.MoisturePercentage = dto.MoisturePercentage.Value;
            if (dto.BacterialTest != null) certificate.BacterialTest = Normalize(dto.BacterialTest);
            if (dto.FungalTest != null) certificate.FungalTest = Normalize(dto.FungalTest);
            if (dto.MicroscopicAppearance != null) certificate.MicroscopicAppearance = Normalize(dto.MicroscopicAppearance);
            if (dto.Color != null) certificate.Color = Normalize(dto.Color);
            if (dto.Odor != null) certificate.Odor = Normalize(dto.Odor);
            if (dto.Notes != null) certificate.Notes = Normalize(dto.Notes);

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
                $"Updated certificate of analysis (Id: {certificate.Id}, Number: {certificate.Number})",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(await ToResponseDto(certificate.Id));
        }

        // ---------------------------------------------------------------
        // result route
        // ---------------------------------------------------------------

        // TODO(role): tighten to a named "SetCertificateResult" policy when a QA role exists.
        [Authorize(Roles = "Admin, Quality Manager")]
        [HttpPut("result/{id}")]
        public async Task<IActionResult> SetResult(int id, SetCertificateResultDto dto)
        {
            var result = dto.Result!.Value;
            var reason = Normalize(dto.RejectionReason);

            var resultError = ValidateResultInput(result, reason);
            if (resultError is not null)
                return BadRequest(new { message = resultError });

            await using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                // Lock the certificate row.
                var certificate = await _context.CertificatesOfAnalysis
                    .FromSqlRaw("SELECT * FROM certificates_of_analysis WITH (UPDLOCK) WHERE Id = {0}", id)
                    .Include(c => c.Sample)
                    .FirstOrDefaultAsync();

                if (certificate is null)
                {
                    await tx.RollbackAsync();
                    return NotFound(new { message = "Certificate of analysis not found" });
                }

                var sample = certificate.Sample;

                bool boxMoved = false;
                int? newLocationId = null;
                string? fromCode = null;
                string? toCode = null;

                if (sample.SampleType == SampleTypes.Delivery)
                {
                    if (sample.UomId is null)
                    {
                        await tx.RollbackAsync();
                        return BadRequest(new { message = "Delivery sample is not linked to a box" });
                    }

                    // Lock the box.
                    var box = await _context.UOMs
                        .FromSqlRaw("SELECT * FROM uoms WITH (UPDLOCK) WHERE Id = {0}", sample.UomId.Value)
                        .Include(u => u.Location)
                        .FirstOrDefaultAsync();

                    if (box is null)
                    {
                        await tx.RollbackAsync();
                        return BadRequest(new { message = "Box for this sample no longer exists" });
                    }

                    // Reservation guard: any Done transfer line referencing this box blocks the change.
                    var hasDoneReservation = await _context.TransferOrderLines
                        .AnyAsync(l => l.UomId == box.Id && l.TransferOrder.Status == TransferOrderStatuses.Done);
                    if (hasDoneReservation)
                    {
                        await tx.RollbackAsync();
                        return Conflict(new { message = "Box is reserved on a Done transfer order; change the result after it is executed or cancelled" });
                    }

                    // Current-zone guard.

                    //reeeeeeeeeeeeeecheeeeeeeck this thing
                    if (box.Location.Kind != LocationKinds.Inventory || box.Location.Zone is null)
                    {
                        await tx.RollbackAsync();
                        return Conflict(new { message = "Box is not in a real inventory zone; cannot apply certificate result" });
                    }

                    var invId = box.Location.InventoryId!.Value;
                    var targetZone = result == AnalysisResults.Accepted
                        ? InventoryZones.Accepted
                        : InventoryZones.Rejected;

                    var targetLocation = await _context.Locations
                        .FirstOrDefaultAsync(l => l.InventoryId == invId && l.Zone == targetZone);

                    if (targetLocation is null)
                    {
                        await tx.RollbackAsync();
                        return Conflict(new { message = "Target zone is missing from the inventory" });
                    }

                    if (box.LocationId == targetLocation.Id && box.ShelfId is null)
                    {
                        // Already in the target zone with no shelf: only the result changes.
                    }
                    else
                    {
                        var fromLocation = box.Location;
                        var fromShelfId = box.ShelfId;

                        box.LocationId = targetLocation.Id;
                        box.ShelfId = null;

                        _context.StockMoves.Add(new StockMove
                        {
                            UomId = box.Id,
                            FromLocationId = fromLocation.Id,
                            ToLocationId = targetLocation.Id,
                            FromShelfId = fromShelfId,
                            ToShelfId = null,
                            Quantity = box.Quantity,
                            MoveType = StockMoveTypes.Move,
                            SourceType = StockMoveSources.CertificateOfAnalysis,
                            SourceId = certificate.Id,
                            Reason = "a certificate result was set",
                            MoveDate = DateTime.UtcNow,
                            UserId = CurrentUserId()
                        });

                        boxMoved = true;
                        newLocationId = targetLocation.Id;
                        fromCode = fromLocation.Code;
                        toCode = targetLocation.Code;
                    }
                }

                certificate.Result = result;
                certificate.RejectionReason = reason;
                if (sample.SampleType == SampleTypes.Delivery && boxMoved)
                {
                    await _notifications.NotifyAsync(
                        "Warehouse",
                        $"Box {sample.Uom!.Barcode} moved to a new zone; put it away",
                        NotificationRelatedTypes.BoxNeedsPutAway,
                        relatedId: sample.UomId);
                }

                if (result == AnalysisResults.Rejected)
                {
                    await _notifications.NotifyAsync(
                        "Admin",
                        $"Certificate {certificate.Number} is Rejected; decide on the box",
                        NotificationRelatedTypes.CertificateRejected,
                        relatedId: certificate.Id);
                }

                // Latest active certificate on the sample wins.
                var siblings = await _context.CertificatesOfAnalysis
                    .Where(c => c.SampleId == sample.Id && c.Id != certificate.Id && c.IsActive)
                    .ToListAsync();
                foreach (var s in siblings)
                    s.IsActive = false;

                await _context.SaveChangesAsync();
                await tx.CommitAsync();

                var moveNote = boxMoved
                    ? $"; box moved: Yes (from {fromCode} to {toCode})"
                    : "; box moved: No";

                await _auditLogService.LogAsync(
                    User,
                    "SetCertificateResult",
                    $"Set result {result} on certificate {certificate.Number} (Id: {certificate.Id}, Sample: {sample.Number}, SampleType: {sample.SampleType}){moveNote}",
                    HttpContext.Connection.RemoteIpAddress?.ToString());

                var response = await ToResponseDto(certificate.Id);
                response.BoxMoved = boxMoved;
                response.BoxNewLocationId = newLocationId;
                return Ok(response);
            }
            catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlEx && sqlEx.Number is 2601 or 2627)
            {
                await tx.RollbackAsync();
                return Conflict(new { message = "A certificate of analysis with this number already exists" });
            }
            catch
            {
                await tx.RollbackAsync();
                return StatusCode(500, new { message = "An unexpected error occurred while setting the certificate result" });
            }
        }

        // ---------------------------------------------------------------
        // read / status / delete
        // ---------------------------------------------------------------

        [HttpGet("index")]
        public async Task<IActionResult> Index(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] int? sampleId = null,
            [FromQuery] AnalysisResults? result = null,
            [FromQuery] int? contractReviewId = null)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;

            var query = QueryWithIncludes();

            if (sampleId.HasValue)
                query = query.Where(c => c.SampleId == sampleId.Value);

            if (result.HasValue)
                query = query.Where(c => c.Result == result.Value);

            if (contractReviewId.HasValue)
                query = query.Where(c => c.Sample.ContractReviewId == contractReviewId.Value);

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
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOne(int id)
        {
            var coa = await _context.CertificatesOfAnalysis.FindAsync(id);
            if (coa is null) return NotFound(new { message = "Certificate of Analysis not found" });
            return Ok(await ToResponseDto(id));
        }
        [Authorize(Roles = "Admin, Technical, Manager")]
        [HttpPut("status/{id}")]
        public async Task<IActionResult> ChangeStatus(int id, ChangeStatusDto dto)
        {
            var certificate = await _context.CertificatesOfAnalysis.FindAsync(id);
            if (certificate is null)
                return NotFound(new { message = "Certificate of analysis not found" });

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
                return NotFound(new { message = "Certificate of analysis not found" });

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

        // ---------------------------------------------------------------
        // helpers
        // ---------------------------------------------------------------

        private string CurrentUserId() =>
            User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value ?? "unknown";

        private static string? Normalize(string? value) =>
            string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        private static string? ValidateResultInput(AnalysisResults result, string? reason) => result switch
        {
            AnalysisResults.Pending =>
                "Result must be Accepted or Rejected; Pending cannot be set via this endpoint",
            AnalysisResults.Rejected when reason is null =>
                "Rejection reason is required when the result is Rejected",
            AnalysisResults.Accepted when reason is not null =>
                "Rejection reason is only allowed when the result is Rejected",
            AnalysisResults.Accepted or AnalysisResults.Rejected => null,
            _ => "Invalid result"
        };

        private IQueryable<CertificateOfAnalysis> QueryWithIncludes() =>
            _context.CertificatesOfAnalysis
                .Include(c => c.Sample).ThenInclude(s => s.ContractReview!).ThenInclude(cr => cr.Supplier)
                .Include(c => c.Sample).ThenInclude(s => s.ContractReview!).ThenInclude(cr => cr.RFQ).ThenInclude(r => r.Material)
                .Include(c => c.Sample).ThenInclude(s => s.Uom!).ThenInclude(u => u.Material);

        private async Task<CertificateOfAnalysisResponseDto> ToResponseDto(int id)
        {
            var certificate = await QueryWithIncludes().FirstAsync(c => c.Id == id);
            return ToDto(certificate);
        }

        private static CertificateOfAnalysisResponseDto ToDto(CertificateOfAnalysis c)
        {
            var sample = c.Sample;
            var (resultAr, resultEn) = c.Result.GetBilingualNames();

            // Material / supplier / CR come from different places depending on sample type.
            string materialNumber, materialEn, materialAr;
            int materialId;
            Supplier? supplier = null;
            ContractReview? review = null;

            if (sample.SampleType == SampleTypes.Supplier)
            {
                review = sample.ContractReview!;
                supplier = review.Supplier;
                var m = review.RFQ.Material;
                materialId = m.Id; materialNumber = m.MaterialNumber; materialEn = m.NameEn; materialAr = m.NameAr;
            }
            else
            {
                var m = sample.Uom!.Material!;
                materialId = m.Id; materialNumber = m.MaterialNumber; materialEn = m.NameEn; materialAr = m.NameAr;
            }

            string? reviewStatusEn = null, reviewStatusAr = null;
            if (review is not null)
            {
                var (ar, en) = review.Status.GetBilingualNames();
                reviewStatusAr = ar; reviewStatusEn = en;
            }

            return new CertificateOfAnalysisResponseDto
            {
                Id = c.Id,
                Number = c.Number,
                AnalysisDate = c.AnalysisDate,

                SampleId = c.SampleId,
                SampleNumber = sample.Number,
                SampleType = sample.SampleType,
                SampleDate = sample.SampleDate,

                SupplierId = supplier?.Id,
                SupplierNumber = supplier?.SupplierNumber,
                SupplierNameEn = supplier?.NameEn,
                SupplierNameAr = supplier?.NameAr,

                ContractReviewId = review?.Id,
                ContractReviewNumber = review?.Number,
                ContractReviewCreationDate = review?.CreationDate,
                ContractReviewExpectedDate = review?.ExpectedDate,
                ContractReviewQuantity = review?.Quantity,
                ContractReviewStatus = review?.Status,
                ContractReviewStatusNameEn = reviewStatusEn,
                ContractReviewStatusNameAr = reviewStatusAr,

                MaterialId = materialId,
                MaterialNumber = materialNumber,
                MaterialNameEn = materialEn,
                MaterialNameAr = materialAr,

                UomId = sample.UomId,
                UomBarcode = sample.Uom?.Barcode,

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
                IsActive = c.IsActive,

                BoxMoved = null,
                BoxNewLocationId = null
            };
        }
    }
}