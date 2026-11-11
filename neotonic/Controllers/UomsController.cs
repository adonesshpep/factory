using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using neotonic.DTOs;
using neotonic.DTOs.Responses;
using neotonic.Helpers;
using neotonic.Models;
using neotonic.Services;

namespace neotonic.Controllers
{
    [Route("api/uoms")]
    [ApiController]
    [Authorize(Policy = "ActiveUser")]
    public class UomsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IAuditLogService _auditLogService;

        public UomsController(AppDbContext context, IAuditLogService auditLogService)
        {
            _context = context;
            _auditLogService = auditLogService;
        }

        // ---------------------------------------------------------------
        // GET /api/uoms/index
        // ---------------------------------------------------------------
        [HttpGet("index")]
        public async Task<IActionResult> Index(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] int? locationId = null,
            [FromQuery] int? materialId = null,
            [FromQuery] int? productId = null,
            [FromQuery] int? shelfId = null,
            [FromQuery] bool? unShelved = null,
            [FromQuery] UOMKinds? kind = null)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;

            var query = _context.UOMs
                .Include(u => u.Location).ThenInclude(l => l.Inventory)
                .Include(u => u.Shelf).ThenInclude(s => s!.Location).ThenInclude(l => l.Inventory)
                .Include(u => u.Material)
                .Include(u => u.Product)
                .AsQueryable();

            if (locationId.HasValue) query = query.Where(u => u.LocationId == locationId.Value);
            if (materialId.HasValue) query = query.Where(u => u.MaterialId == materialId.Value);
            if (productId.HasValue) query = query.Where(u => u.ProductId == productId.Value);
            if (shelfId.HasValue) query = query.Where(u => u.ShelfId == shelfId.Value);
            if (unShelved == true) query = query.Where(u => u.ShelfId == null);
            if (kind.HasValue) query = query.Where(u => u.Kind == kind.Value);

            var ordered = query.OrderBy(u => u.Id);
            var totalCount = await ordered.CountAsync();
            var boxes = await ordered.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return Ok(new PagedResultDto<UomListItemDto>
            {
                Items = boxes.Select(ToListItem).ToList(),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            });
        }
        [HttpGet("by-barcode/{barcode}")]
        public async Task<IActionResult> GetByBarcode(string barcode)
        {
            var trimmed = barcode.Trim();

            var box = await _context.UOMs
                .Include(u => u.Location).ThenInclude(l => l.Inventory)
                .Include(u => u.Shelf).ThenInclude(s => s!.Location).ThenInclude(l => l.Inventory)
                .Include(u => u.Material)
                .Include(u => u.Product)
                .FirstOrDefaultAsync(u => u.Barcode == trimmed);

            if (box is null) return NotFound(new { message = "Box not found" });

            var dto = ToBox(box);

            var origin = await _context.GetAsync(box.Id);
            if (origin is not null)
            {
                dto.SupplierId = origin.SupplierId;
                dto.SupplierNumber = origin.SupplierNumber;
                dto.SupplierNameEn = origin.SupplierNameEn;
                dto.SupplierNameAr = origin.SupplierNameAr;
                dto.MemoRWInId = origin.MemoRWInId;
                dto.MemoRWInNumber = origin.MemoRWInNumber;
            }

            var cert = await _context.CertificatesOfAnalysis
                .Where(c => c.Sample.UomId == box.Id && c.IsActive)
                .OrderByDescending(c => c.AnalysisDate).ThenByDescending(c => c.Id)
                .Select(c => new { c.Id, c.Number, c.Result })
                .FirstOrDefaultAsync();

            if (cert is not null)
            {
                var (resultAr, resultEn) = cert.Result.GetBilingualNames();
                dto.ActiveCertificateId = cert.Id;
                dto.ActiveCertificateNumber = cert.Number;
                dto.ActiveCertificateResult = cert.Result;
                dto.ActiveCertificateResultNameEn = resultEn;
                dto.ActiveCertificateResultNameAr = resultAr;
            }

            return Ok(dto);
        }
        // ---------------------------------------------------------------
        // GET /api/uoms/needs-putaway?locationId=...
        // ---------------------------------------------------------------
        [HttpGet("needs-putaway")]
        public async Task<IActionResult> NeedsPutAway([FromQuery] int locationId)
        {
            var location = await _context.Locations
                .Include(l => l.Inventory)
                .FirstOrDefaultAsync(l => l.Id == locationId);
            if (location is null) return NotFound(new { message = "Location not found" });
            if (location.Kind != LocationKinds.Inventory)
                return BadRequest(new { message = "Virtual locations have no shelves" });

            var boxes = await _context.UOMs
                .Where(u => u.LocationId == locationId && u.ShelfId == null)
                .Include(u => u.Material)
                .Include(u => u.Product)
                .ToListAsync();

            if (boxes.Count == 0) return Ok(new List<NeedsPutAwayItemDto>());

            var boxIds = boxes.Select(b => b.Id).ToList();

            // Last move per box.
            var lastMovePerBox = (await _context.StockMoves
                    .Where(m => boxIds.Contains(m.UomId))
                    .ToListAsync())
                .GroupBy(m => m.UomId)
                .ToDictionary(g => g.Key, g => g.OrderByDescending(m => m.MoveDate).ThenByDescending(m => m.Id).First());

            // Collect certificate ids and transfer order ids referenced by those moves.
            var certIds = lastMovePerBox.Values
                .Where(m => m.SourceType == StockMoveSources.CertificateOfAnalysis && m.SourceId.HasValue)
                .Select(m => m.SourceId!.Value).Distinct().ToList();

            var transferIds = lastMovePerBox.Values
                .Where(m => m.SourceType == StockMoveSources.TransferOrder && m.SourceId.HasValue)
                .Select(m => m.SourceId!.Value).Distinct().ToList();

            var certNumbers = await _context.CertificatesOfAnalysis
                .Where(c => certIds.Contains(c.Id))
                .Select(c => new { c.Id, c.Number })
                .ToDictionaryAsync(x => x.Id, x => x.Number);

            // Transfer order numbers are owned by the business orders now (FK on the order side).
            var transferNumbers = new Dictionary<int, string?>();

            if (transferIds.Count > 0)
            {
                var prepNumbers = await _context.PreparingOrders
                    .Where(p => p.TransferOrderId != null && transferIds.Contains(p.TransferOrderId!.Value))
                    .Select(p => new { p.TransferOrderId, p.Number })
                    .ToListAsync();
                foreach (var p in prepNumbers)
                    transferNumbers[p.TransferOrderId!.Value] = p.Number;

                var relNumbers = await _context.ReleaseOrders
                    .Where(r => r.TransferOrderId != null && transferIds.Contains(r.TransferOrderId!.Value))
                    .Select(r => new { r.TransferOrderId, r.Number })
                    .ToListAsync();
                foreach (var r in relNumbers)
                    transferNumbers[r.TransferOrderId!.Value] = r.Number;
            }

            var items = boxes.Select(box =>
            {
                lastMovePerBox.TryGetValue(box.Id, out var lastMove);

                string reason;
                int? sourceId = null;
                string? sourceNumber = null;

                if (lastMove is null)
                {
                    reason = "No move history";
                }
                else if (lastMove.SourceType == StockMoveSources.CertificateOfAnalysis)
                {
                    reason = "Certificate result moved the box to this zone";
                    sourceId = lastMove.SourceId;
                    sourceNumber = lastMove.SourceId.HasValue ? certNumbers.GetValueOrDefault(lastMove.SourceId.Value) : null;
                }
                else if (lastMove.SourceType == StockMoveSources.TransferOrder)
                {
                    reason = "Transfer order finished here";
                    sourceId = lastMove.SourceId;
                    sourceNumber = lastMove.SourceId.HasValue ? transferNumbers.GetValueOrDefault(lastMove.SourceId.Value) : null;
                }
                else if (lastMove.SourceType == StockMoveSources.None && lastMove.FromLocationId == LocationIds.SupplierReturns)
                {
                    reason = "Received without a shelf";
                }
                else
                {
                    reason = "Moved manually; no shelf";
                }

                return new NeedsPutAwayItemDto
                {
                    UomId = box.Id,
                    Barcode = box.Barcode,
                    Kind = box.Kind,
                    MaterialId = box.MaterialId,
                    MaterialNumber = box.Material?.MaterialNumber,
                    MaterialNameEn = box.Material?.NameEn,
                    MaterialNameAr = box.Material?.NameAr,
                    ProductId = box.ProductId,
                    ProductNumber = box.Product?.ProductNumber,
                    ProductNameEn = box.Product?.NameEn,
                    ProductNameAr = box.Product?.NameAr,
                    Quantity = box.Quantity,
                    LocationId = box.LocationId,
                    Reason = reason,
                    SourceId = sourceId,
                    SourceNumber = sourceNumber
                };
            }).ToList();

            return Ok(items);
        }

        // ---------------------------------------------------------------
        // PUT /api/uoms/{id}/shelf  (put-away)
        // ---------------------------------------------------------------
        [HttpPut("{id}/shelf")]
        public async Task<IActionResult> PutAway(int id, PutAwayUomDto dto)
        {
            var box = await _context.UOMs
                .Include(u => u.Location)
                .Include(u => u.Shelf)
                .FirstOrDefaultAsync(u => u.Id == id);
            if (box is null) return NotFound(new { message = "Box not found" });

            if (box.Location.Kind != LocationKinds.Inventory)
                return BadRequest(new { message = "Virtual locations have no shelves" });

            var shelf = await _context.Shelves.FirstOrDefaultAsync(s => s.Id == dto.ShelfId && s.IsActive);
            if (shelf is null) return BadRequest(new { message = "Shelf does not exist or is inactive" });

            if (shelf.LocationId != box.LocationId)
                return BadRequest(new { message = "Shelf does not belong to the box's location" });

            if (box.ShelfId == shelf.Id)
                return BadRequest(new { message = "Box is already on that shelf" });

            var oldShelfId = box.ShelfId;

            await using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                _context.StockMoves.Add(new StockMove
                {
                    UomId = box.Id,
                    FromLocationId = box.LocationId,
                    ToLocationId = box.LocationId,
                    FromShelfId = oldShelfId,
                    ToShelfId = shelf.Id,
                    Quantity = box.Quantity,
                    MoveType = StockMoveTypes.Manual,
                    SourceType = StockMoveSources.None,
                    SourceId = null,
                    Reason = "Put away",
                    MoveDate = DateTime.UtcNow,
                    UserId = CurrentUserId()
                });

                box.ShelfId = shelf.Id;
                await _context.SaveChangesAsync();
                await tx.CommitAsync();
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }

            await _auditLogService.LogAsync(
                User,
                "PutAwayUom",
                $"Put away box '{box.Barcode}' (UomId: {box.Id}) on shelf {shelf.LocationId}/{shelf.Number} (from shelf {oldShelfId?.ToString() ?? "none"})",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(await ToBoxDto(id));
        }

        // ---------------------------------------------------------------
        // Helpers
        // ---------------------------------------------------------------

        private string CurrentUserId() =>
            User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value ?? "unknown";

        private async Task<UomBoxDto> ToBoxDto(int id)
        {
            var box = await _context.UOMs
                .Include(u => u.Location).ThenInclude(l => l.Inventory)
                .Include(u => u.Shelf).ThenInclude(s => s!.Location).ThenInclude(l => l.Inventory)
                .Include(u => u.Material)
                .Include(u => u.Product)
                .FirstAsync(u => u.Id == id);
            return ToBox(box);
        }

        private static UomBoxDto ToBox(UOM box) => new()
        {
            Id = box.Id,
            Barcode = box.Barcode,
            Number = box.Number,
            Kind = box.Kind,
            MaterialId = box.MaterialId,
            MaterialNumber = box.Material?.MaterialNumber,
            MaterialNameEn = box.Material?.NameEn,
            MaterialNameAr = box.Material?.NameAr,
            ProductId = box.ProductId,
            ProductNumber = box.Product?.ProductNumber,
            ProductNameEn = box.Product?.NameEn,
            ProductNameAr = box.Product?.NameAr,
            Quantity = box.Quantity,
            LocationId = box.LocationId,
            LocationCode = box.Location.Code,
            LocationNameEn = box.Location.NameEn,
            LocationNameAr = box.Location.NameAr,
            InventoryId = box.Location.InventoryId,
            InventoryCode = box.Location.Inventory?.Code,
            Zone = box.Location.Zone,
            ShelfId = box.ShelfId,
            ShelfLabel = ShelfLabel(box.Shelf),
            ParentUomId = box.ParentUomId
        };

        private static UomListItemDto ToListItem(UOM box) => new()
        {
            Id = box.Id,
            Barcode = box.Barcode,
            Kind = box.Kind,
            MaterialId = box.MaterialId,
            MaterialNumber = box.Material?.MaterialNumber,
            MaterialNameEn = box.Material?.NameEn,
            MaterialNameAr = box.Material?.NameAr,
            ProductId = box.ProductId,
            ProductNumber = box.Product?.ProductNumber,
            ProductNameEn = box.Product?.NameEn,
            ProductNameAr = box.Product?.NameAr,
            Quantity = box.Quantity,
            LocationId = box.LocationId,
            LocationCode = box.Location.Code,
            InventoryId = box.Location.InventoryId,
            InventoryCode = box.Location.Inventory?.Code,
            Zone = box.Location.Zone,
            ShelfId = box.ShelfId,
            ShelfLabel = ShelfLabel(box.Shelf)
        };

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