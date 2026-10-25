using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using neotonic.DTOs;
using neotonic.Models;
using neotonic.Services;

namespace neotonic.Controllers
{
    [Route("api/memorwins")]
    [ApiController]
    [Authorize(Policy = "ActiveUser")]
    public class MemoRWInController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IAuditLogService _auditLogService;

        public MemoRWInController(AppDbContext context, IAuditLogService auditLogService)
        {
            _context = context;
            _auditLogService = auditLogService;
        }

        // ---- Header CRUD ----

        [HttpPost("create")]
        public async Task<IActionResult> Create(CreateMemoRWInDto dto)
        {
            var purchaseOrder = await _context.PurchaseOrders.FindAsync(dto.PurchaseOrderId);
            if (purchaseOrder is null || !purchaseOrder.IsActive)
                return BadRequest(new { message = "Purchase order does not exist or is inactive" });

            var worker = await _context.Workers.FindAsync(dto.WorkerId);
            if (worker is null || !worker.IsActive)
                return BadRequest(new { message = "Worker does not exist or is inactive" });

            var memo = new MemoRWIn
            {
                MemoDate = dto.MemoDate ?? DateTime.UtcNow,
                PurchaseOrderId = purchaseOrder.Id,
                WorkerId = worker.Id,
                Notes = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes.Trim()
            };

            try
            {
                _context.MemoRWIns.Add(memo);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlEx && sqlEx.Number is 2601 or 2627)
            {
                return Conflict(new { message = "A memo with this number already exists" });
            }

            await _auditLogService.LogAsync(
                User,
                "CreateMemoRWIn",
                $"Created inbound raw memo '{memo.Number}' (Id: {memo.Id}, PO: {purchaseOrder.Id}, Worker: {worker.WorkerNumber})",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(await ToResponseDto(memo.Id));
        }

        [HttpPut("update/{id}")]
        public async Task<IActionResult> Update(int id, UpdateMemoRWInDto dto)
        {
            var memo = await _context.MemoRWIns.FindAsync(id);
            if (memo is null)
                return NotFound(new { message = "Memo not found" });

            if (dto.MemoDate.HasValue)
                memo.MemoDate = dto.MemoDate.Value;

            if (dto.PurchaseOrderId.HasValue)
            {
                var purchaseOrder = await _context.PurchaseOrders.FindAsync(dto.PurchaseOrderId.Value);
                if (purchaseOrder is null || !purchaseOrder.IsActive)
                    return BadRequest(new { message = "Purchase order does not exist or is inactive" });
                memo.PurchaseOrderId = purchaseOrder.Id;
            }

            if (dto.WorkerId.HasValue)
            {
                var worker = await _context.Workers.FindAsync(dto.WorkerId.Value);
                if (worker is null || !worker.IsActive)
                    return BadRequest(new { message = "Worker does not exist or is inactive" });
                memo.WorkerId = worker.Id;
            }

            if (dto.Notes != null)
                memo.Notes = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes.Trim();

            await _context.SaveChangesAsync();

            await _auditLogService.LogAsync(
                User,
                "UpdateMemoRWIn",
                $"Updated inbound raw memo (Id: {memo.Id}, Number: {memo.Number})",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(await ToResponseDto(memo.Id));
        }

        [HttpGet("index")]
        public async Task<IActionResult> Index([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;

            var query = _context.MemoRWIns
                .Include(m => m.PurchaseOrder)
                .Include(m => m.Worker)
                .OrderByDescending(m => m.MemoDate)
                .ThenByDescending(m => m.Id);

            var totalCount = await query.CountAsync();

            var memos = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(m => new MemoRWInResponseDto
                {
                    Id = m.Id,
                    Number = m.Number,
                    MemoDate = m.MemoDate,
                    PurchaseOrderId = m.PurchaseOrderId,
                    PurchaseOrderNumber = m.PurchaseOrder.Number,
                    WorkerId = m.WorkerId,
                    WorkerNumber = m.Worker.WorkerNumber,
                    WorkerNameEn = m.Worker.NameEn,
                    WorkerNameAr = m.Worker.NameAr,
                    Notes = m.Notes,
                    IsActive = m.IsActive
                    // Details intentionally omitted in the list view for performance
                })
                .ToListAsync();

            return Ok(new PagedResultDto<MemoRWInResponseDto>
            {
                Items = memos,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOne(int id)
        {
            var memo = await _context.MemoRWIns.FindAsync(id);
            if (memo is null)
                return NotFound(new { message = "Memo not found" });

            return Ok(await ToResponseDto(id));
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("status/{id}")]
        public async Task<IActionResult> ChangeStatus(int id, ChangeStatusDto dto)
        {
            var memo = await _context.MemoRWIns.FindAsync(id);
            if (memo is null)
                return NotFound(new { message = "Memo not found" });

            memo.IsActive = dto.IsActive;
            await _context.SaveChangesAsync();

            await _auditLogService.LogAsync(
                User,
                dto.IsActive ? "ActivateMemoRWIn" : "DeactivateMemoRWIn",
                $"{(dto.IsActive ? "Activated" : "Deactivated")} inbound raw memo '{memo.Number}' (Id: {memo.Id})",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(new { message = $"Memo {(dto.IsActive ? "activated" : "deactivated")} successfully" });
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var memo = await _context.MemoRWIns.FindAsync(id);
            if (memo is null)
                return NotFound(new { message = "Memo not found" });

            _context.MemoRWIns.Remove(memo);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                var conflictPayload = await _context.  (memo);
                return Conflict(conflictPayload);
            }

            await _auditLogService.LogAsync(
                User,
                "DeleteMemoRWIn",
                $"Deleted inbound raw memo '{memo.Number}' (Id: {memo.Id})",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(new { message = "Memo deleted successfully" });
        }

        // ---- Details: add one at a time ----

        // Adds a single detail and returns it along with the current max barcode
        // in the whole memo_rw_details table, so the client can keep its counter
        // in sync without a second round trip.
        [HttpPost("{id}/details")]
        public async Task<IActionResult> AddDetail(int id, CreateMemoRWDetailDto dto)
        {
            var memo = await _context.MemoRWIns.FindAsync(id);
            if (memo is null)
                return NotFound(new { message = "Memo not found" });

            if (!memo.IsActive)
                return BadRequest(new { message = "Cannot add details to an inactive memo" });

            var barcode = dto.Barcode!.Value;
            var barcodeExists = await _context.MemoRWDetails.AnyAsync(d => d.Barcode == barcode);
            if (barcodeExists)
                return Conflict(new { message = "A detail with this barcode already exists" });

            var detail = new MemoRWDetail
            {
                MemoId = memo.Id,
                Barcode = barcode,
                AWeight = dto.AWeight!.Value,
                BWeight = dto.BWeight!.Value
            };

            try
            {
                _context.MemoRWDetails.Add(detail);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlEx && sqlEx.Number is 2601 or 2627)
            {
                // Raced with another client between the check above and the save
                return Conflict(new { message = "A detail with this barcode already exists" });
            }

            var currentMax = await GetCurrentMaxBarcodeAsync();

            await _auditLogService.LogAsync(
                User,
                "AddMemoRWDetail",
                $"Added detail barcode '{detail.Barcode}' to memo '{memo.Number}' (MemoId: {memo.Id}, DetailId: {detail.Id})",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(new AddMemoRWDetailResponseDto
            {
                Detail = ToDetailDto(detail),
                CurrentMaxBarcode = currentMax
            });
        }

        // Updates every detail of the memo in one request. Rows omitted from the
        // list are left untouched (use the delete endpoint to remove one). Barcode
        // changes are re-validated globally, so uniqueness can never be broken
        // even if the client sends a whole list at once.
        [HttpPut("{id}/details")]
        public async Task<IActionResult> UpdateDetails(int id, UpdateMemoRWDetailsDto dto)
        {
            var memo = await _context.MemoRWIns
                .Include(m => m.Details)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (memo is null)
                return NotFound(new { message = "Memo not found" });

            var incomingIds = dto.Details.Select(d => d.Id).ToList();
            if (incomingIds.Distinct().Count() != incomingIds.Count)
                return BadRequest(new { message = "The same detail cannot appear twice in one request" });

            var existingById = memo.Details.ToDictionary(d => d.Id);

            var unknownIds = incomingIds.Where(i => !existingById.ContainsKey(i)).ToList();
            if (unknownIds.Count > 0)
                return BadRequest(new { message = $"Details do not belong to this memo: {string.Join(", ", unknownIds)}" });

            // Pre-validate all barcode changes before touching the DB, so a bad
            // row anywhere in the batch leaves every row untouched.
            var requestedBarcodes = dto.Details
                .Where(d => d.Barcode.HasValue)
                .Select(d => d.Barcode!.Value)
                .ToList();

            if (requestedBarcodes.Distinct().Count() != requestedBarcodes.Count)
                return BadRequest(new { message = "The same barcode cannot appear twice in one request" });

            if (requestedBarcodes.Count > 0)
            {
                // A barcode is a conflict if it already belongs to a different detail
                var conflicting = await _context.MemoRWDetails
                    .Where(d => requestedBarcodes.Contains(d.Barcode) && !incomingIds.Contains(d.Id))
                    .Select(d => d.Barcode)
                    .ToListAsync();

                if (conflicting.Count > 0)
                    return Conflict(new
                    {
                        message = $"Barcode(s) already used by another detail: {string.Join(", ", conflicting)}"
                    });
            }

            foreach (var row in dto.Details)
            {
                var detail = existingById[row.Id];

                if (row.Barcode.HasValue)
                    detail.Barcode = row.Barcode.Value;

                if (row.AWeight.HasValue)
                    detail.AWeight = row.AWeight.Value;

                if (row.BWeight.HasValue)
                    detail.BWeight = row.BWeight.Value;
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlEx && sqlEx.Number is 2601 or 2627)
            {
                // Raced with another client between the pre-check and the save
                return Conflict(new { message = "One of the barcodes is already used by another detail" });
            }

            await _auditLogService.LogAsync(
                User,
                "UpdateMemoRWDetails",
                $"Updated {dto.Details.Count} detail(s) of inbound raw memo '{memo.Number}' (MemoId: {memo.Id})",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(await ToResponseDto(memo.Id));
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("details/{detailId}")]
        public async Task<IActionResult> DeleteDetail(int detailId)
        {
            var detail = await _context.MemoRWDetails
                .Include(d => d.Memo)
                .FirstOrDefaultAsync(d => d.Id == detailId);
            if (detail is null)
                return NotFound(new { message = "Detail not found" });

            var memoNumber = detail.Memo.Number;

            _context.MemoRWDetails.Remove(detail);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                var conflictPayload = await _context.GetDeleteConflictsAsync(detail);
                return Conflict(conflictPayload);
            }

            await _auditLogService.LogAsync(
                User,
                "DeleteMemoRWDetail",
                $"Deleted detail barcode '{detail.Barcode}' from memo '{memoNumber}' (DetailId: {detail.Id})",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(new { message = "Detail deleted successfully" });
        }

        // ---- Barcode peek ----

        // Returns the current maximum barcode in the details table (0 if empty).
        // Intended for the UI to display before opening the add-detail form.
        [HttpGet("next-barcode")]
        public async Task<IActionResult> NextBarcode()
        {
            var currentMax = await GetCurrentMaxBarcodeAsync();
            return Ok(new NextBarcodeResponseDto { CurrentMaxBarcode = currentMax });
        }

        // ---- helpers ----

        // Returns 0 when the table is empty, which MaxAsync would throw on otherwise.
        private async Task<long> GetCurrentMaxBarcodeAsync()
        {
            return await _context.MemoRWDetails
                .Select(d => (long?)d.Barcode)
                .MaxAsync() ?? 0L;
        }

        private static MemoRWDetailResponseDto ToDetailDto(MemoRWDetail d) => new()
        {
            Id = d.Id,
            MemoId = d.MemoId,
            Barcode = d.Barcode,
            AWeight = d.AWeight,
            BWeight = d.BWeight
        };

        private async Task<MemoRWInResponseDto> ToResponseDto(int id)
        {
            var memo = await _context.MemoRWIns
                .Include(m => m.PurchaseOrder)
                .Include(m => m.Worker)
                .Include(m => m.Details)
                .FirstAsync(m => m.Id == id);

            return new MemoRWInResponseDto
            {
                Id = memo.Id,
                Number = memo.Number,
                MemoDate = memo.MemoDate,
                PurchaseOrderId = memo.PurchaseOrderId,
                PurchaseOrderNumber = memo.PurchaseOrder.Number,
                WorkerId = memo.WorkerId,
                WorkerNumber = memo.Worker.WorkerNumber,
                WorkerNameEn = memo.Worker.NameEn,
                WorkerNameAr = memo.Worker.NameAr,
                Notes = memo.Notes,
                IsActive = memo.IsActive,
                Details = memo.Details
                    .OrderBy(d => d.Barcode)
                    .Select(ToDetailDto)
                    .ToList()
            };
        }
    }
}