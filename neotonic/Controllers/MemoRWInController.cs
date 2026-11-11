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

        // ============================================================
        // Header CRUD
        // ============================================================

        [Authorize(Roles = "Admin, Warehouse, Warehouse Keeper")]
        [HttpPost("create")]
        public async Task<IActionResult> Create(CreateMemoRWInDto dto)
        {
            var po = await _context.PurchaseOrders.FindAsync(dto.PurchaseOrderId);
            if (po is null || !po.IsActive)
                return BadRequest(new { message = "Purchase order does not exist or is inactive" });

            var worker = await _context.Workers.FindAsync(dto.WorkerId);
            if (worker is null || !worker.IsActive)
                return BadRequest(new { message = "Worker does not exist or is inactive" });

            var memo = new MemoRWIn
            {
                MemoDate = dto.MemoDate ?? DateTime.UtcNow,
                PurchaseOrderId = po.Id,
                WorkerId = worker.Id,
                Notes = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes.Trim()
            };

            try { _context.MemoRWIns.Add(memo); await _context.SaveChangesAsync(); }
            catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlEx && sqlEx.Number is 2601 or 2627)
            {
                return Conflict(new { message = "A memo with this number already exists" });
            }

            await _auditLogService.LogAsync(
                User, "CreateMemoRWIn",
                $"Created inbound raw memo '{memo.Number}' (Id: {memo.Id}, PO: {po.Id}, Worker: {worker.WorkerNumber})",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(await ToResponseDto(memo.Id));
        }

        [HttpPut("update/{id}")]
        public async Task<IActionResult> Update(int id, UpdateMemoRWInDto dto)
        {
            var memo = await _context.MemoRWIns.FindAsync(id);
            if (memo is null) return NotFound(new { message = "Memo not found" });

            if (dto.MemoDate.HasValue) memo.MemoDate = dto.MemoDate.Value;

            if (dto.PurchaseOrderId.HasValue)
            {
                var po = await _context.PurchaseOrders.FindAsync(dto.PurchaseOrderId.Value);
                if (po is null || !po.IsActive)
                    return BadRequest(new { message = "Purchase order does not exist or is inactive" });
                memo.PurchaseOrderId = po.Id;
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
                User, "UpdateMemoRWIn",
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
                .OrderByDescending(m => m.MemoDate).ThenByDescending(m => m.Id);

            var totalCount = await query.CountAsync();
            var memos = await query.Skip((page - 1) * pageSize).Take(pageSize)
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
                    MaterialId = m.PurchaseOrder.ContractReview.RFQ.MaterialId,
                    MaterialNumber = m.PurchaseOrder.ContractReview.RFQ.Material.MaterialNumber,
                    MaterialNameEn = m.PurchaseOrder.ContractReview.RFQ.Material.NameEn,
                    MaterialNameAr = m.PurchaseOrder.ContractReview.RFQ.Material.NameAr,
                    Notes = m.Notes,
                    IsActive = m.IsActive
                }).ToListAsync();

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
            var exists = await _context.MemoRWIns.AnyAsync(m => m.Id == id);
            if (!exists) return NotFound(new { message = "Memo not found" });
            return Ok(await ToResponseDto(id));
        }

        [Authorize(Roles = "Admin, Technical, Manager")]
        [HttpPut("status/{id}")]
        public async Task<IActionResult> ChangeStatus(int id, ChangeStatusDto dto)
        {
            var memo = await _context.MemoRWIns.FindAsync(id);
            if (memo is null) return NotFound(new { message = "Memo not found" });

            memo.IsActive = dto.IsActive;
            await _context.SaveChangesAsync();

            await _auditLogService.LogAsync(
                User, dto.IsActive ? "ActivateMemoRWIn" : "DeactivateMemoRWIn",
                $"{(dto.IsActive ? "Activated" : "Deactivated")} inbound raw memo '{memo.Number}' (Id: {memo.Id})",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(new { message = $"Memo {(dto.IsActive ? "activated" : "deactivated")} successfully" });
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var memo = await _context.MemoRWIns.FindAsync(id);
            if (memo is null) return NotFound(new { message = "Memo not found" });

            _context.MemoRWIns.Remove(memo);

            try { await _context.SaveChangesAsync(); }
            catch (DbUpdateException)
            {
                var conflictPayload = await _context.GetDeleteConflictsAsync(memo);
                return Conflict(conflictPayload);
            }

            await _auditLogService.LogAsync(
                User, "DeleteMemoRWIn",
                $"Deleted inbound raw memo '{memo.Number}' (Id: {memo.Id})",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(new { message = "Memo deleted successfully" });
        }

        // ============================================================
        // Details
        // ============================================================

        [Authorize(Roles = "Admin, Warehouse, Warehouse Keeper")]
        [HttpPost("{id}/details")]
        public async Task<IActionResult> AddDetail(int id, CreateMemoRWDetailDto dto)
        {
            var memo = await _context.MemoRWIns.FindAsync(id);
            if (memo is null) return NotFound(new { message = "Memo not found" });
            if (!memo.IsActive) return BadRequest(new { message = "Cannot add details to an inactive memo" });

            var material = await _context.Materials
                .Include(m => m.MaterialType)
                .FirstOrDefaultAsync(m => m.Id == dto.MaterialId);
            if (material is null || !material.IsActive)
                return BadRequest(new { message = "Material does not exist or is inactive" });

            if (material.MaterialType.FlowId is null)
                return BadRequest(new { message = $"Material type '{material.MaterialType.NameEn}' has no flow configured" });

            var firstStep = await _context.FlowSteps
                .Where(fs => fs.FlowId == material.MaterialType.FlowId.Value)
                .OrderBy(fs => fs.OrderIndex)
                .FirstOrDefaultAsync();
            if (firstStep is null)
                return BadRequest(new { message = "Material type's flow has no steps" });

            if (!long.TryParse(dto.Barcode.Trim(), out var parsedNumber) || parsedNumber <= 0)
                return BadRequest(new { message = "Barcode must be a positive numeric value" });

            var barcode = dto.Barcode.Trim();
            var barcodeExists = await _context.UOMs.AnyAsync(u => u.Barcode == barcode);
            if (barcodeExists) return Conflict(new { message = "A box with this barcode already exists" });

            if (dto.ShelfId.HasValue)
            {
                var shelf = await _context.Shelves.FirstOrDefaultAsync(s => s.Id == dto.ShelfId.Value && s.IsActive);
                if (shelf is null) return BadRequest(new { message = "Shelf does not exist or is inactive" });
                if (shelf.LocationId != firstStep.LocationId)
                    return BadRequest(new { message = "Shelf does not belong to the receiving location" });
            }
            var nextNumber = (await _context.UOMs.Select(u => (long?)u.Number).MaxAsync() ?? 0L) + 1;
            if(nextNumber != parsedNumber)
                return BadRequest(new { message = $"Barcode number must be the next sequential number: {nextNumber}" });
            await using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                var box = new UOM
                {
                    Barcode = barcode,
                    Number = nextNumber,
                    Kind = UOMKinds.Material,
                    MaterialId = material.Id,
                    Quantity = dto.Quantity,
                    LocationId = firstStep.LocationId,
                    ShelfId = dto.ShelfId
                };
                _context.UOMs.Add(box);
                await _context.SaveChangesAsync();

                _context.StockMoves.Add(new StockMove
                {
                    UomId = box.Id,
                    FromLocationId = LocationIds.SupplierReturns,
                    ToLocationId = firstStep.LocationId,
                    FromShelfId = null,
                    ToShelfId = dto.ShelfId,
                    Quantity = box.Quantity,
                    MoveType = StockMoveTypes.Move,
                    SourceType = StockMoveSources.None,
                    SourceId = null,
                    Reason = null,
                    MoveDate = DateTime.UtcNow,
                    UserId = CurrentUserId()
                });

                var detail = new MemoRWDetail { MemoId = memo.Id, UomId = box.Id, Tear = dto.Tear };
                _context.MemoRWDetails.Add(detail);
                await _context.SaveChangesAsync();

                await tx.CommitAsync();

                await _auditLogService.LogAsync(
                    User, "AddMemoRWDetail",
                    $"Added box '{box.Barcode}' to memo '{memo.Number}' (MemoId: {memo.Id}, UomId: {box.Id}, Material: {material.MaterialNumber})",
                    HttpContext.Connection.RemoteIpAddress?.ToString());

                return Ok(await ToDetailDto(detail.Id));
            }
            catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlEx && sqlEx.Number is 2601 or 2627)
            {
                await tx.RollbackAsync();
                return Conflict(new { message = "A box with this barcode already exists" });
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        [HttpPut("details/{detailId}")]
        public async Task<IActionResult> UpdateDetail(int detailId, UpdateMemoRWDetailDto dto)
        {
            var detail = await _context.MemoRWDetails
                .Include(d => d.Memo)
                .Include(d => d.Uom).ThenInclude(u => u.Location)
                .FirstOrDefaultAsync(d => d.Id == detailId);
            if (detail is null) return NotFound(new { message = "Detail not found" });

            if (!detail.Memo.IsActive)
                return BadRequest(new { message = "Cannot edit details of an inactive memo" });

            var guard = await CheckEditableAsync(detail.Uom);
            if (guard is not null) return Conflict(new { message = guard });

            if (dto.ShelfId.HasValue)
            {
                var shelf = await _context.Shelves.FirstOrDefaultAsync(s => s.Id == dto.ShelfId.Value && s.IsActive);
                if (shelf is null) return BadRequest(new { message = "Shelf does not exist or is inactive" });
                if (shelf.LocationId != detail.Uom.LocationId)
                    return BadRequest(new { message = "Shelf does not belong to the box's location" });
            }

            await using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                var shelfChanged = detail.Uom.ShelfId != dto.ShelfId;

                detail.Uom.Quantity = dto.Quantity;
                detail.Tear = dto.Tear;

                if (shelfChanged)
                {
                    var oldMove = await _context.StockMoves
                        .FirstOrDefaultAsync(m => m.UomId == detail.UomId);
                    if (oldMove is not null) _context.StockMoves.Remove(oldMove);

                    _context.StockMoves.Add(new StockMove
                    {
                        UomId = detail.UomId,
                        FromLocationId = LocationIds.SupplierReturns,
                        ToLocationId = detail.Uom.LocationId,
                        FromShelfId = detail.Uom.ShelfId,
                        ToShelfId = dto.ShelfId,
                        Quantity = dto.Quantity,
                        MoveType = StockMoveTypes.Move,
                        SourceType = StockMoveSources.None,
                        SourceId = null,
                        Reason = "Edit receiving detail",
                        MoveDate = DateTime.UtcNow,
                        UserId = CurrentUserId()
                    });

                    detail.Uom.ShelfId = dto.ShelfId;
                }

                await _context.SaveChangesAsync();
                await tx.CommitAsync();

                await _auditLogService.LogAsync(
                    User, "UpdateMemoRWDetail",
                    $"Updated detail of memo '{detail.Memo.Number}' (DetailId: {detail.Id}, UomId: {detail.UomId}, Qty: {dto.Quantity}, Tear: {dto.Tear})",
                    HttpContext.Connection.RemoteIpAddress?.ToString());

                return Ok(await ToDetailDto(detail.Id));
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("details/{detailId}")]
        public async Task<IActionResult> DeleteDetail(int detailId)
        {
            var detail = await _context.MemoRWDetails
                .Include(d => d.Memo)
                .Include(d => d.Uom)
                .FirstOrDefaultAsync(d => d.Id == detailId);
            if (detail is null) return NotFound(new { message = "Detail not found" });

            var box = detail.Uom;

            if (!detail.Memo.IsActive)
                return BadRequest(new { message = "Cannot delete details of an inactive memo" });

            if (box.Kind != UOMKinds.Material)
                return Conflict(new { message = "Box is not a material box" });

            if (await _context.TransferOrderLines.AnyAsync(l => l.UomId == box.Id))
                return Conflict(new { message = "Box is referenced on a transfer order and cannot be removed" });

            var initialMoveError = await CheckInitialMoveAsync(box);
            if (initialMoveError is not null)
                return Conflict(new { message = initialMoveError });

            var certOnPo = await _context.CertificatesOfAnalysis
                .Where(c => c.Sample!.UomId == box.Id)
                .Join(_context.PurchaseOrders,
                      c => c.Id,
                      p => p.CertificateOfAnalysisId,
                      (c, p) => c.Id)
                .AnyAsync();
            if (certOnPo)
                return Conflict(new { message = "A certificate from a sample on this box is linked to a purchase order; the box cannot be removed" });

            await using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                var samples = await _context.Samples.Where(s => s.UomId == box.Id).ToListAsync();
                var certs = await _context.CertificatesOfAnalysis
                    .Where(c => samples.Select(s => s.Id).Contains(c.SampleId))
                    .ToListAsync();

                _context.CertificatesOfAnalysis.RemoveRange(certs);
                _context.Samples.RemoveRange(samples);

                var moves = await _context.StockMoves.Where(m => m.UomId == box.Id).ToListAsync();
                _context.StockMoves.RemoveRange(moves);

                _context.MemoRWDetails.Remove(detail);
                _context.UOMs.Remove(box);

                await _context.SaveChangesAsync();
                await tx.CommitAsync();

                await _auditLogService.LogAsync(
                    User, "DeleteMemoRWDetail",
                    $"Deleted detail from memo '{detail.Memo.Number}' (DetailId: {detail.Id}, Barcode: {box.Barcode}, deleted {samples.Count} sample(s), {certs.Count} certificate(s))",
                    HttpContext.Connection.RemoteIpAddress?.ToString());

                return Ok(new { message = "Detail deleted successfully" });
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        [HttpGet("next-barcode")]
        public async Task<IActionResult> NextBarcode([FromQuery] UOMKinds kind = UOMKinds.Material)
        {
            if (kind != UOMKinds.Material)
                return BadRequest(new { message = "Only Material is supported for now" });

            var currentMax = await _context.UOMs
                .Where(u => u.Kind == UOMKinds.Material)
                .Select(u => (long?)u.Number)
                .MaxAsync() ?? 0L;

            return Ok(new NextBarcodeResponseDto { CurrentMaxBarcode = currentMax });
        }
        // GET /api/memorwins/{id}/available-shelves?unoccupiedOnly=false
        // Resolves the material through the memo's PO -> CR -> RFQ chain, then the material's
        // type -> flow -> first FlowStep -> that location's shelves.
        [HttpGet("{id}/available-shelves")]
        public async Task<IActionResult> AvailableShelves(int id, [FromQuery] bool unoccupiedOnly = false)
        {
            var memo = await _context.MemoRWIns
                .Include(m => m.PurchaseOrder).ThenInclude(p => p.ContractReview).ThenInclude(c => c.RFQ)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (memo is null) return NotFound(new { message = "Memo not found" });

            var rfq = memo.PurchaseOrder.ContractReview.RFQ;
            if (rfq is null)
                return BadRequest(new { message = "Purchase order has no linked contract review / RFQ chain" });

            var material = await _context.Materials
                .Include(m => m.MaterialType)
                .FirstOrDefaultAsync(m => m.Id == rfq.MaterialId);
            if (material is null)
                return BadRequest(new { message = "Material from RFQ does not exist" });

            if (material.MaterialType.FlowId is null)
                return BadRequest(new { message = "Material type has no flow configured" });

            var firstStep = await _context.FlowSteps
                .Where(s => s.FlowId == material.MaterialType.FlowId.Value)
                .OrderBy(s => s.OrderIndex)
                .FirstOrDefaultAsync();
            if (firstStep is null)
                return BadRequest(new { message = "Material type's flow has no steps" });

            var query = _context.Shelves
                .Include(s => s.Location).ThenInclude(l => l.Inventory)
                .Where(s => s.LocationId == firstStep.LocationId && s.IsActive);

            if (unoccupiedOnly)
                query = query.Where(s => !_context.UOMs.Any(u => u.ShelfId == s.Id));

            var shelves = await query.OrderBy(s => s.Number).ToListAsync();

            var occupiedIds = await _context.UOMs
                .Where(u => u.ShelfId != null && shelves.Select(s => s.Id).Contains(u.ShelfId!.Value))
                .Select(u => u.ShelfId!.Value)
                .Distinct()
                .ToListAsync();

            var occupiedSet = occupiedIds.ToHashSet();

            return Ok(shelves.Select(s => new neotonic.DTOs.Responses.ShelfResponseDto
            {
                Id = s.Id,
                LocationId = s.LocationId,
                LocationCode = s.Location.Code,
                InventoryCode = s.Location.Inventory?.Code,
                Zone = s.Location.Zone,
                Number = s.Number,
                Label = ShelvesController.ShelfLabel(s.Location, s.Number),
                IsActive = s.IsActive,
                IsOccupied = occupiedSet.Contains(s.Id)
            }).ToList());
        }

        // ============================================================
        // Helpers
        // ============================================================

        private string CurrentUserId() =>
            User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value ?? "unknown";

        private async Task<string?> CheckEditableAsync(UOM box)
        {
            if (await _context.TransferOrderLines.AnyAsync(l => l.UomId == box.Id))
                return "Box is referenced on a transfer order and can no longer be edited";

            return await CheckInitialMoveAsync(box);
        }

        private async Task<string?> CheckInitialMoveAsync(UOM box)
        {
            var moves = await _context.StockMoves.Where(m => m.UomId == box.Id).ToListAsync();
            if (moves.Count != 1)
                return "Box has already moved; it can no longer be edited or removed";

            var m = moves[0];
            if (m.MoveType != StockMoveTypes.Move || m.SourceType != StockMoveSources.None
                || m.FromLocationId != LocationIds.SupplierReturns || m.ToLocationId != box.LocationId)
                return "Box has already moved; it can no longer be edited or removed";

            return null;
        }

        private async Task<MemoRWInResponseDto> ToResponseDto(int id)
        {
            var memo = await _context.MemoRWIns
                .Include(m => m.PurchaseOrder)
                    .ThenInclude(p => p.ContractReview)
                    .ThenInclude(c => c.RFQ)
                    .ThenInclude(r => r.Material)
                .Include(m => m.Worker)
                .Include(m => m.Details).ThenInclude(d => d.Uom).ThenInclude(u => u.Material)
                .Include(m => m.Details).ThenInclude(d => d.Uom).ThenInclude(u => u.Shelf).ThenInclude(s => s!.Location).ThenInclude(l => l.Inventory)
                .FirstAsync(m => m.Id == id);
            var rfqMaterial = memo.PurchaseOrder.ContractReview.RFQ.Material;
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
                MaterialId = rfqMaterial.Id,
                MaterialNumber = rfqMaterial.MaterialNumber,
                MaterialNameEn = rfqMaterial.NameEn,
                MaterialNameAr = rfqMaterial.NameAr,
                Notes = memo.Notes,
                IsActive = memo.IsActive,
                Details = memo.Details.Select(ToDetailDtoStatic).ToList()
            };
        }

        private async Task<MemoRWDetailResponseDto> ToDetailDto(int detailId)
        {
            var d = await _context.MemoRWDetails
                .Include(x => x.Uom).ThenInclude(u => u.Material)
                .Include(x => x.Uom).ThenInclude(u => u.Shelf).ThenInclude(s => s!.Location).ThenInclude(l => l.Inventory)
                .FirstAsync(x => x.Id == detailId);
            return ToDetailDtoStatic(d);
        }

        private static MemoRWDetailResponseDto ToDetailDtoStatic(MemoRWDetail d)
        {
            var box = d.Uom;
            return new MemoRWDetailResponseDto
            {
                Id = d.Id,
                MemoId = d.MemoId,
                UomId = box.Id,
                UomBarcode = box.Barcode,
                MaterialId = box.MaterialId!.Value,
                MaterialNumber = box.Material!.MaterialNumber,
                MaterialNameEn = box.Material.NameEn,
                MaterialNameAr = box.Material.NameAr,
                Quantity = box.Quantity,
                Tear = d.Tear,
                BWeight = box.Quantity + d.Tear,
                ShelfId = box.ShelfId,
                ShelfLabel = ShelfLabel(box.Shelf)
            };
        }

        private static string? ShelfLabel(Shelf? shelf)
        {
            if (shelf is null) return null;
            var zoneLetter = shelf.Location.Zone switch
            {
                InventoryZones.Quarantine => "A",
                InventoryZones.Accepted => "B",
                InventoryZones.Rejected => "C",
                _ => "?"
            };
            return $"{shelf.Location.Inventory!.Code}-{zoneLetter}{shelf.Number}";
        }
    }
}