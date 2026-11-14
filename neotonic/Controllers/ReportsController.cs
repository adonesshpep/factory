using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using neotonic.DTOs;
using neotonic.DTOs.Responses;
using neotonic.Models;

namespace neotonic.Controllers
{
    [Route("api/reports")]
    [ApiController]
    [Authorize(Policy = "ActiveUser")]
    public class ReportsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ReportsController(AppDbContext context) => _context = context;


        [HttpGet("washing/index")]
        public async Task<IActionResult> WashingIndex(
     [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
     [FromQuery] int? preparingOrderId = null,
     [FromQuery] int? uomId = null,
     [FromQuery] string? uomBarcode = null,
     [FromQuery] int? workerId = null,
     [FromQuery] string? userId = null,
     [FromQuery] DateTime? fromDate = null,
     [FromQuery] DateTime? toDate = null)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;

            var q = _context.WashingReports.AsQueryable();

            if (preparingOrderId.HasValue) q = q.Where(r => r.PreparingOrderId == preparingOrderId.Value);
            if (uomId.HasValue) q = q.Where(r => r.UomId == uomId.Value);
            if (!string.IsNullOrWhiteSpace(uomBarcode))
            {
                var b = uomBarcode.Trim();
                q = q.Where(r => r.Uom.Barcode == b);
            }
            if (workerId.HasValue) q = q.Where(r => r.WorkerId == workerId.Value);
            if (!string.IsNullOrWhiteSpace(userId))
            {
                var u = userId.Trim();
                q = q.Where(r => r.UserId == u);
            }
            if (fromDate.HasValue) q = q.Where(r => r.CreatedAt >= fromDate.Value);
            if (toDate.HasValue) q = q.Where(r => r.CreatedAt <= toDate.Value);

            var ordered = q.OrderByDescending(r => r.CreatedAt).ThenByDescending(r => r.Id);
            var total = await ordered.CountAsync();
            var items = await ordered.Skip((page - 1) * pageSize).Take(pageSize)
                .Select(r => new WashingReportResponseDto
                {
                    Id = r.Id,
                    PreparingOrderId = r.PreparingOrderId,
                    PreparingOrderNumber = r.PreparingOrder.Number,
                    UomId = r.UomId,
                    UomBarcode = r.Uom.Barcode,
                    StockMoveId = r.StockMoveId,
                    BeforeWeight = r.BeforeWeight,
                    AfterWeight = r.AfterWeight,
                    LossPercentage = r.LossPercentage,
                    Impurities = r.Impurities,
                    Temperature = r.Temperature,
                    HumidityPercentage = r.HumidityPercentage,
                    WorkerId = r.WorkerId,
                    WorkerNumber = r.Worker.WorkerNumber,
                    WorkerNameEn = r.Worker.NameEn,
                    WorkerNameAr = r.Worker.NameAr,
                    Notes = r.Notes,
                    UserId = r.UserId,
                    CreatedAt = r.CreatedAt
                }).ToListAsync();

            return Ok(new PagedResultDto<WashingReportResponseDto>
            {
                Items = items,
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            });
        }

        [HttpGet("purification/index")]
        public async Task<IActionResult> PurificationIndex(
            [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
            [FromQuery] int? preparingOrderId = null,
            [FromQuery] int? uomId = null,
            [FromQuery] string? uomBarcode = null,
            [FromQuery] int? workerId = null,
            [FromQuery] string? userId = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;

            var q = _context.PurificationReports.AsQueryable();

            if (preparingOrderId.HasValue) q = q.Where(r => r.PreparingOrderId == preparingOrderId.Value);
            if (uomId.HasValue) q = q.Where(r => r.UomId == uomId.Value);
            if (!string.IsNullOrWhiteSpace(uomBarcode))
            {
                var b = uomBarcode.Trim();
                q = q.Where(r => r.Uom.Barcode == b);
            }
            if (workerId.HasValue) q = q.Where(r => r.WorkerId == workerId.Value);
            if (!string.IsNullOrWhiteSpace(userId))
            {
                var u = userId.Trim();
                q = q.Where(r => r.UserId == u);
            }
            if (fromDate.HasValue) q = q.Where(r => r.CreatedAt >= fromDate.Value);
            if (toDate.HasValue) q = q.Where(r => r.CreatedAt <= toDate.Value);

            var ordered = q.OrderByDescending(r => r.CreatedAt).ThenByDescending(r => r.Id);
            var total = await ordered.CountAsync();
            var items = await ordered.Skip((page - 1) * pageSize).Take(pageSize)
                .Select(r => new PurificationReportResponseDto
                {
                    Id = r.Id,
                    PreparingOrderId = r.PreparingOrderId,
                    PreparingOrderNumber = r.PreparingOrder.Number,
                    UomId = r.UomId,
                    UomBarcode = r.Uom.Barcode,
                    StockMoveId = r.StockMoveId,
                    BeforeWeight = r.BeforeWeight,
                    AfterWeight = r.AfterWeight,
                    LossPercentage = r.LossPercentage,
                    TheoreticalImpurityWeight = r.TheoreticalImpurityWeight,
                    PracticalImpurityWeight = r.PracticalImpurityWeight,
                    WorkerId = r.WorkerId,
                    WorkerNumber = r.Worker.WorkerNumber,
                    WorkerNameEn = r.Worker.NameEn,
                    WorkerNameAr = r.Worker.NameAr,
                    Notes = r.Notes,
                    UserId = r.UserId,
                    CreatedAt = r.CreatedAt
                }).ToListAsync();

            return Ok(new PagedResultDto<PurificationReportResponseDto>
            {
                Items = items,
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            });
        }

        [HttpGet("drying/index")]
        public async Task<IActionResult> DryingIndex(
            [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
            [FromQuery] int? preparingOrderId = null,
            [FromQuery] int? uomId = null,
            [FromQuery] string? uomBarcode = null,
            [FromQuery] int? workerId = null,
            [FromQuery] string? userId = null,
            [FromQuery] int? sampleId = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;

            var q = _context.DryingReports.AsQueryable();

            if (preparingOrderId.HasValue) q = q.Where(r => r.PreparingOrderId == preparingOrderId.Value);
            if (uomId.HasValue) q = q.Where(r => r.UomId == uomId.Value);
            if (!string.IsNullOrWhiteSpace(uomBarcode))
            {
                var b = uomBarcode.Trim();
                q = q.Where(r => r.Uom.Barcode == b);
            }
            if (workerId.HasValue) q = q.Where(r => r.WorkerId == workerId.Value);
            if (!string.IsNullOrWhiteSpace(userId))
            {
                var u = userId.Trim();
                q = q.Where(r => r.UserId == u);
            }
            if (sampleId.HasValue) q = q.Where(r => r.SampleId == sampleId.Value);
            if (fromDate.HasValue) q = q.Where(r => r.CreatedAt >= fromDate.Value);
            if (toDate.HasValue) q = q.Where(r => r.CreatedAt <= toDate.Value);

            var ordered = q.OrderByDescending(r => r.CreatedAt).ThenByDescending(r => r.Id);
            var total = await ordered.CountAsync();
            var items = await ordered.Skip((page - 1) * pageSize).Take(pageSize)
                .Select(r => new DryingReportResponseDto
                {
                    Id = r.Id,
                    PreparingOrderId = r.PreparingOrderId,
                    PreparingOrderNumber = r.PreparingOrder.Number,
                    UomId = r.UomId,
                    UomBarcode = r.Uom.Barcode,
                    StockMoveId = r.StockMoveId,
                    BeforeWeight = r.BeforeWeight,
                    AfterWeight = r.AfterWeight,
                    LossPercentage = r.LossPercentage,
                    HumidityBeforePercentage = r.HumidityBeforePercentage,
                    HumidityAfterPercentage = r.HumidityAfterPercentage,
                    Duration = r.Duration,
                    DryingKind = r.DryingKind,
                    Temperature = r.Temperature,
                    SampleId = r.SampleId,
                    SampleNumber = r.Sample.Number,
                    WorkerId = r.WorkerId,
                    WorkerNumber = r.Worker.WorkerNumber,
                    WorkerNameEn = r.Worker.NameEn,
                    WorkerNameAr = r.Worker.NameAr,
                    Notes = r.Notes,
                    UserId = r.UserId,
                    CreatedAt = r.CreatedAt
                }).ToListAsync();

            return Ok(new PagedResultDto<DryingReportResponseDto>
            {
                Items = items,
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            });
        }

        [HttpGet("sterilization/index")]
        public async Task<IActionResult> SterilizationIndex(
            [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
            [FromQuery] int? preparingOrderId = null,
            [FromQuery] int? uomId = null,
            [FromQuery] string? uomBarcode = null,
            [FromQuery] int? workerId = null,
            [FromQuery] string? userId = null,
            [FromQuery] int? sampleId = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;

            var q = _context.SterilizationReports.AsQueryable();

            if (preparingOrderId.HasValue) q = q.Where(r => r.PreparingOrderId == preparingOrderId.Value);
            if (uomId.HasValue) q = q.Where(r => r.UomId == uomId.Value);
            if (!string.IsNullOrWhiteSpace(uomBarcode))
            {
                var b = uomBarcode.Trim();
                q = q.Where(r => r.Uom.Barcode == b);
            }
            if (workerId.HasValue) q = q.Where(r => r.WorkerId == workerId.Value);
            if (!string.IsNullOrWhiteSpace(userId))
            {
                var u = userId.Trim();
                q = q.Where(r => r.UserId == u);
            }
            if (sampleId.HasValue) q = q.Where(r => r.SampleId == sampleId.Value);
            if (fromDate.HasValue) q = q.Where(r => r.CreatedAt >= fromDate.Value);
            if (toDate.HasValue) q = q.Where(r => r.CreatedAt <= toDate.Value);

            var ordered = q.OrderByDescending(r => r.CreatedAt).ThenByDescending(r => r.Id);
            var total = await ordered.CountAsync();
            var items = await ordered.Skip((page - 1) * pageSize).Take(pageSize)
                .Select(r => new SterilizationReportResponseDto
                {
                    Id = r.Id,
                    PreparingOrderId = r.PreparingOrderId,
                    PreparingOrderNumber = r.PreparingOrder.Number,
                    UomId = r.UomId,
                    UomBarcode = r.Uom.Barcode,
                    StockMoveId = r.StockMoveId,
                    BeforeWeight = r.BeforeWeight,
                    AfterWeight = r.AfterWeight,
                    LossPercentage = r.LossPercentage,
                    Duration = r.Duration,
                    Wavelength = r.Wavelength,
                    Thickness = r.Thickness,
                    Speed = r.Speed,
                    Temperature = r.Temperature,
                    HumidityPercentage = r.HumidityPercentage,
                    SampleId = r.SampleId,
                    SampleNumber = r.Sample.Number,
                    WorkerId = r.WorkerId,
                    WorkerNumber = r.Worker.WorkerNumber,
                    WorkerNameEn = r.Worker.NameEn,
                    WorkerNameAr = r.Worker.NameAr,
                    Notes = r.Notes,
                    UserId = r.UserId,
                    CreatedAt = r.CreatedAt
                }).ToListAsync();

            return Ok(new PagedResultDto<SterilizationReportResponseDto>
            {
                Items = items,
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            });
        }

        // ---------------------------------------------------------------
        // Single report by id
        // ---------------------------------------------------------------

        [HttpGet("washing/{id}")]
        public async Task<IActionResult> WashingGet(int id)
        {
            var r = await _context.WashingReports
                .Include(x => x.Uom).Include(x => x.Worker).Include(x => x.PreparingOrder)
                .FirstOrDefaultAsync(x => x.Id == id);
            if (r is null) return NotFound(new { message = "Washing report not found" });

            return Ok(new WashingReportResponseDto
            {
                Id = r.Id,
                PreparingOrderId = r.PreparingOrderId,
                PreparingOrderNumber = r.PreparingOrder.Number,
                UomId = r.UomId,
                UomBarcode = r.Uom.Barcode,
                StockMoveId = r.StockMoveId,
                BeforeWeight = r.BeforeWeight,
                AfterWeight = r.AfterWeight,
                LossPercentage = r.LossPercentage,
                Impurities = r.Impurities,
                Temperature = r.Temperature,
                HumidityPercentage = r.HumidityPercentage,
                WorkerId = r.WorkerId,
                WorkerNumber = r.Worker.WorkerNumber,
                WorkerNameEn = r.Worker.NameEn,
                WorkerNameAr = r.Worker.NameAr,
                Notes = r.Notes,
                UserId = r.UserId,
                CreatedAt = r.CreatedAt
            });
        }

        [HttpGet("purification/{id}")]
        public async Task<IActionResult> PurificationGet(int id)
        {
            var r = await _context.PurificationReports
                .Include(x => x.Uom).Include(x => x.Worker).Include(x => x.PreparingOrder)
                .FirstOrDefaultAsync(x => x.Id == id);
            if (r is null) return NotFound(new { message = "Purification report not found" });

            return Ok(new PurificationReportResponseDto
            {
                Id = r.Id,
                PreparingOrderId = r.PreparingOrderId,
                PreparingOrderNumber = r.PreparingOrder.Number,
                UomId = r.UomId,
                UomBarcode = r.Uom.Barcode,
                StockMoveId = r.StockMoveId,
                BeforeWeight = r.BeforeWeight,
                AfterWeight = r.AfterWeight,
                LossPercentage = r.LossPercentage,
                TheoreticalImpurityWeight = r.TheoreticalImpurityWeight,
                PracticalImpurityWeight = r.PracticalImpurityWeight,
                WorkerId = r.WorkerId,
                WorkerNumber = r.Worker.WorkerNumber,
                WorkerNameEn = r.Worker.NameEn,
                WorkerNameAr = r.Worker.NameAr,
                Notes = r.Notes,
                UserId = r.UserId,
                CreatedAt = r.CreatedAt
            });
        }

        [HttpGet("drying/{id}")]
        public async Task<IActionResult> DryingGet(int id)
        {
            var r = await _context.DryingReports
                .Include(x => x.Uom).Include(x => x.Sample).Include(x => x.Worker).Include(x => x.PreparingOrder)
                .FirstOrDefaultAsync(x => x.Id == id);
            if (r is null) return NotFound(new { message = "Drying report not found" });

            return Ok(new DryingReportResponseDto
            {
                Id = r.Id,
                PreparingOrderId = r.PreparingOrderId,
                PreparingOrderNumber = r.PreparingOrder.Number,
                UomId = r.UomId,
                UomBarcode = r.Uom.Barcode,
                StockMoveId = r.StockMoveId,
                BeforeWeight = r.BeforeWeight,
                AfterWeight = r.AfterWeight,
                LossPercentage = r.LossPercentage,
                HumidityBeforePercentage = r.HumidityBeforePercentage,
                HumidityAfterPercentage = r.HumidityAfterPercentage,
                Duration = r.Duration,
                DryingKind = r.DryingKind,
                Temperature = r.Temperature,
                SampleId = r.SampleId,
                SampleNumber = r.Sample.Number,
                WorkerId = r.WorkerId,
                WorkerNumber = r.Worker.WorkerNumber,
                WorkerNameEn = r.Worker.NameEn,
                WorkerNameAr = r.Worker.NameAr,
                Notes = r.Notes,
                UserId = r.UserId,
                CreatedAt = r.CreatedAt
            });
        }

        [HttpGet("sterilization/{id}")]
        public async Task<IActionResult> SterilizationGet(int id)
        {
            var r = await _context.SterilizationReports
                .Include(x => x.Uom).Include(x => x.Sample).Include(x => x.Worker).Include(x => x.PreparingOrder)
                .FirstOrDefaultAsync(x => x.Id == id);
            if (r is null) return NotFound(new { message = "Sterilization report not found" });

            return Ok(new SterilizationReportResponseDto
            {
                Id = r.Id,
                PreparingOrderId = r.PreparingOrderId,
                PreparingOrderNumber = r.PreparingOrder.Number,
                UomId = r.UomId,
                UomBarcode = r.Uom.Barcode,
                StockMoveId = r.StockMoveId,
                BeforeWeight = r.BeforeWeight,
                AfterWeight = r.AfterWeight,
                LossPercentage = r.LossPercentage,
                Duration = r.Duration,
                Wavelength = r.Wavelength,
                Thickness = r.Thickness,
                Speed = r.Speed,
                Temperature = r.Temperature,
                HumidityPercentage = r.HumidityPercentage,
                SampleId = r.SampleId,
                SampleNumber = r.Sample.Number,
                WorkerId = r.WorkerId,
                WorkerNumber = r.Worker.WorkerNumber,
                WorkerNameEn = r.Worker.NameEn,
                WorkerNameAr = r.Worker.NameAr,
                Notes = r.Notes,
                UserId = r.UserId,
                CreatedAt = r.CreatedAt
            });
        }
    }
}