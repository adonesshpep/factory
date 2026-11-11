//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.Data.SqlClient;
//using Microsoft.EntityFrameworkCore;
//using neotonic.DTOs;
//using neotonic.Helpers;
//using neotonic.Models;
//using neotonic.Services;

//namespace neotonic.Controllers
//{
//    [Route("api/memos")]
//    [ApiController]
//    [Authorize(Policy = "ActiveUser")]
//    public class MemosController : ControllerBase
//    {
//        private readonly AppDbContext _context;
//        private readonly IAuditLogService _auditLogService;

//        public MemosController(AppDbContext context, IAuditLogService auditLogService)
//        {
//            _context = context;
//            _auditLogService = auditLogService;
//        }



//        // POST api/memos/in/create
//        [HttpPost("in/create")]
//        public async Task<IActionResult> CreateMemoIn(CreateMemoInDto dto)
//        {
//            // --- Validate supplier up front.
//            var supplier = await _context.Suppliers.FindAsync(dto.SupplierId);
//            if (supplier is null || !supplier.IsActive)
//                return BadRequest(new { message = "Supplier does not exist or is inactive" });

//            // --- Validate work order if provided.


//            // --- Validate every material referenced, and preload them into a dictionary.
//            var materialIds = dto.Details.Select(d => d.MaterialId).Distinct().ToList();
//            var materials = await _context.Materials
//                .Where(m => materialIds.Contains(m.Id))
//                .ToDictionaryAsync(m => m.Id);

//            foreach (var id in materialIds)
//            {
//                if (!materials.TryGetValue(id, out var material))
//                    return BadRequest(new { message = $"Material {id} does not exist" });
//                if (!material.IsActive)
//                    return BadRequest(new { message = $"Material {id} is inactive" });
//            }

//            // --- Duplicate guard on memo Number (only if caller supplied one).
//            if (dto.Number > 0)
//            {
//                var duplicated = await _context.Memos.AnyAsync(m => m.Number == dto.Number);
//                if (duplicated)
//                    return Conflict(new { message = "A memo with this number already exists" });
//            }

//            // --- Begin transaction. Memo + details + UOMs + join rows are atomic.
//            await using var tx = await _context.Database.BeginTransactionAsync();

//            try
//            {
//                // Auto-number if not provided.
//                long nextMemoNumber = dto.Number;
//                if (nextMemoNumber <= 0)
//                {
//                    var maxNumber = await _context.Memos.Select(m => (long?)m.Number).MaxAsync() ?? 0L;
//                    nextMemoNumber = maxNumber + 1;
//                }

//                // Desktop sentinel: DateTime.MinValue gets normalized to 2000-01-01.
//                var invoiceDate = dto.InvoiceDate;
//                if (invoiceDate.HasValue && invoiceDate.Value.Date == DateTime.MinValue.Date)
//                    invoiceDate = new DateTime(2000, 1, 1);

//                var memo = new Memo
//                {
//                    Number = nextMemoNumber,
//                    MemoDate = dto.MemoDate ?? DateTime.UtcNow,
//                    CreationDate = DateTime.UtcNow,
//                    TypeId = MemoType.In,
//                    InvoiceNumber = string.IsNullOrWhiteSpace(dto.InvoiceNumber) ? null : dto.InvoiceNumber.Trim(),
//                    InvoiceDate = invoiceDate,
//                    SupplierId = supplier.Id,
//                    Notes = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes.Trim()
//                };

//                _context.Memos.Add(memo);
//                await _context.SaveChangesAsync(); // memo.Id populated

//                // Read the next UOM number once for the whole request.
//                // (Desktop re-read Max() on every iteration — slow and racy.)

//                var uomCounter = new Counter((await _context.UOMs.Select(u => (long?)u.Number).MaxAsync() ?? 0L) + 1);
//                foreach (var detailDto in dto.Details)
//                {
//                    var material = materials[detailDto.MaterialId];

//                    // Next box number for this material.
//                    var maxBox = await _context.UOMs
//                        .Where(u => u.MaterialId == material.Id)
//                        .Select(u => (int?)u.BoxNumber)
//                        .MaxAsync() ?? 0;
//                    int nextBoxNumber = maxBox + 1;

//                    var detail = new MemoDetail
//                    {
//                        MemoId = memo.Id,
//                        MaterialId = material.Id,
//                        Quantity = detailDto.Quantity,
//                        SplitQuantity = detailDto.SplitQuantity,
//                        Lot = string.IsNullOrWhiteSpace(detailDto.Lot) ? null : detailDto.Lot.Trim(),
//                        FactoryName = string.IsNullOrWhiteSpace(detailDto.FactoryName) ? null : detailDto.FactoryName.Trim(),
//                        CountryOrigin = string.IsNullOrWhiteSpace(detailDto.CountryOrigin) ? null : detailDto.CountryOrigin.Trim(),
//                        ProductionDate = detailDto.ProductionDate,
//                        ExpiryDate = detailDto.ExpiryDate,
//                        Notes = string.IsNullOrWhiteSpace(detailDto.Notes) ? null : detailDto.Notes.Trim()
//                    };

//                    _context.MemoDetails.Add(detail);
//                    await _context.SaveChangesAsync(); // detail.Id populated

//                    await GenerateUOMsForDetailAsync(detail, material, uomCounter, nextBoxNumber);

//                }

//                await tx.CommitAsync();

//                await _auditLogService.LogAsync(
//                    User,
//                    "CreateMemoIn",
//                    $"Created memo-in '{memo.Number}' (Id: {memo.Id}, Supplier: {supplier.SupplierNumber}, Details: {dto.Details.Count})",
//                    HttpContext.Connection.RemoteIpAddress?.ToString());

//                return Ok(await ToResponseDto(memo.Id));
//            }
//            catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlEx && sqlEx.Number is 2601 or 2627)
//            {
//                await tx.RollbackAsync();
//                return Conflict(new { message = "A memo with this number already exists" });
//            }
//            catch
//            {
//                await tx.RollbackAsync();
//                return StatusCode(500, new
//                {
//                    message = "An unexpected error occurred while updating the memo. Please try again or contact support."
//                });
//            }
//        }


//        // PUT api/memos/in/update/{id}
//        [HttpPut("in/update/{id}")]
//        public async Task<IActionResult> UpdateMemoIn(int id, UpdateMemoInDto dto)
//        {
//            var memo = await _context.Memos
//                .Include(m => m.Details)
//                .FirstOrDefaultAsync(m => m.Id == id);

//            if (memo is null)
//                return NotFound(new { message = "Memo not found" });

//            if (memo.TypeId != MemoType.In)
//                return BadRequest(new { message = "This memo is not an In memo" });

//            if (dto.SupplierId.HasValue)
//            {
//                var supplier = await _context.Suppliers.FindAsync(dto.SupplierId.Value);
//                if (supplier is null || !supplier.IsActive)
//                    return BadRequest(new { message = "Supplier does not exist or is inactive" });
//            }

//            if (dto.WorkOrderId.HasValue)
//            {
//                var wo = await _context.WorkOrders.FindAsync(dto.WorkOrderId.Value);
//                if (wo is null || !wo.IsActive)
//                    return BadRequest(new { message = "Work order does not exist or is inactive" });
//            }

//            if (dto.Number.HasValue && dto.Number.Value != memo.Number)
//            {
//                var duplicated = await _context.Memos.AnyAsync(m => m.Number == dto.Number.Value && m.Id != memo.Id);
//                if (duplicated)
//                    return Conflict(new { message = "A memo with this number already exists" });
//            }

//            await using var tx = await _context.Database.BeginTransactionAsync();

//            try
//            {
//                // ============================================================
//                // STEP 1 — ORPHAN CLEANUP
//                // ============================================================
//                if (dto.Details is not null)
//                {
//                    var sentIds = dto.Details.Where(d => d.Id > 0).Select(d => d.Id).ToList();

//                    bool runOrphanCleanup =
//                        sentIds.Count > 0 ||
//                        dto.Details.Count == 0;

//                    if (runOrphanCleanup)
//                    {
//                        var orphans = await _context.MemoDetails
//                            .Where(d => d.MemoId == memo.Id && !sentIds.Contains(d.Id))
//                            .Include(d => d.Material)
//                            .ToListAsync();

//                        foreach (var orphan in orphans)
//                        {
//                            var orphanJoins = await _context.MemoDetailUOMs
//                                .Where(mdu => mdu.MemoDetailId == orphan.Id)
//                                .ToListAsync();

//                            var orphanUomIds = orphanJoins.Select(j => j.UOMId).Distinct().ToList();
//                            var orphanUoms = await _context.UOMs
//                                .Where(u => orphanUomIds.Contains(u.Id))
//                                .ToListAsync();

//                            if (orphanUoms.Any(u => u.QuantityOut > 0m))
//                            {
//                                await tx.RollbackAsync();
//                                return BadRequest(new
//                                {
//                                    message = $"Material '{orphan.Material.NameEn}' has already been issued out and cannot be removed"
//                                });
//                            }

//                            // Determine which UOMs are non-serialized and need recompute after the join is gone.
//                            // (Serialized UOMs belong to exactly one detail → they get fully removed.)
//                            var nonSerializedUomIds = new HashSet<int>();
//                            foreach (var uom in orphanUoms)
//                            {
//                                var material = await _context.Materials.FindAsync(uom.MaterialId);
//                                if (material is not null && !material.IsSerialized)
//                                    nonSerializedUomIds.Add(uom.Id);
//                            }

//                            // Remove join rows first (FK Restrict on UOM → MemoDetailUOM would block otherwise).
//                            _context.MemoDetailUOMs.RemoveRange(orphanJoins);
//                            await _context.SaveChangesAsync();

//                            // Remove UOMs. Serialized ones go away entirely.
//                            // Non-serialized ones must be recomputed — they may still be referenced by
//                            // other details that share the same (Material, Lot).
//                            foreach (var uom in orphanUoms)
//                            {
//                                if (nonSerializedUomIds.Contains(uom.Id))
//                                {
//                                    // Recompute from remaining joins. If zero, delete the UOM.
//                                    decimal remaining = await _context.MemoDetailUOMs
//                                        .Where(j => j.UOMId == uom.Id)
//                                        .SumAsync(j => (decimal?)j.Quantity) ?? 0m;

//                                    if (remaining <= 0m)
//                                    {
//                                        _context.UOMs.Remove(uom);
//                                    }
//                                    else
//                                    {
//                                        uom.QuantityIn = remaining;
//                                    }
//                                }
//                                else
//                                {
//                                    _context.UOMs.Remove(uom);
//                                }
//                            }

//                            _context.MemoDetails.Remove(orphan);
//                            await _context.SaveChangesAsync();
//                        }
//                    }
//                }

//                // ============================================================
//                // STEP 2 — UPSERT EACH INCOMING DETAIL ROW
//                // ============================================================
//                var uomCounter = new Counter(
//                    (await _context.UOMs.Select(u => (long?)u.Number).MaxAsync() ?? 0L) + 1);

//                if (dto.Details is not null)
//                {
//                    foreach (var incoming in dto.Details)
//                    {
//                        var material = await _context.Materials.FindAsync(incoming.MaterialId);
//                        if (material is null || !material.IsActive)
//                        {
//                            await tx.RollbackAsync();
//                            return BadRequest(new { message = $"Material {incoming.MaterialId} does not exist or is inactive" });
//                        }

//                        MemoDetail? existing = null;
//                        if (incoming.Id > 0)
//                        {
//                            existing = await _context.MemoDetails
//                                .FirstOrDefaultAsync(d => d.Id == incoming.Id && d.MemoId == memo.Id);
//                        }

//                        // ---------- 2a) NEW ROW ----------
//                        if (existing is null)
//                        {
//                            var newDetail = new MemoDetail
//                            {
//                                MemoId = memo.Id,
//                                MaterialId = material.Id,
//                                Quantity = incoming.Quantity,
//                                SplitQuantity = incoming.SplitQuantity,
//                                Lot = string.IsNullOrWhiteSpace(incoming.Lot) ? null : incoming.Lot.Trim(),
//                                FactoryName = string.IsNullOrWhiteSpace(incoming.FactoryName) ? null : incoming.FactoryName.Trim(),
//                                CountryOrigin = string.IsNullOrWhiteSpace(incoming.CountryOrigin) ? null : incoming.CountryOrigin.Trim(),
//                                ProductionDate = incoming.ProductionDate,
//                                ExpiryDate = incoming.ExpiryDate,
//                                UnitId = incoming.UnitId,
//                                Notes = string.IsNullOrWhiteSpace(incoming.Notes) ? null : incoming.Notes.Trim()
//                            };
//                            _context.MemoDetails.Add(newDetail);
//                            await _context.SaveChangesAsync();

//                            var maxBox = await _context.UOMs
//                                .Where(u => u.MaterialId == material.Id)
//                                .Select(u => (int?)u.BoxNumber)
//                                .MaxAsync() ?? 0;
//                            int nextBox = maxBox + 1;

//                            await GenerateUOMsForDetailAsync(newDetail, material, uomCounter, nextBox);
//                            continue;
//                        }

//                        // ---------- 2b) EXISTING ROW ----------
//                        var newLot = string.IsNullOrWhiteSpace(incoming.Lot) ? null : incoming.Lot.Trim();
//                        bool lotChanged = existing.Lot != newLot;

//                        // (i) Serialized + SplitQuantity changed → wipe & regenerate.
//                        if (material.IsSerialized && existing.SplitQuantity != incoming.SplitQuantity)
//                        {
//                            var oldJoins = await _context.MemoDetailUOMs
//                                .Where(mdu => mdu.MemoDetailId == existing.Id)
//                                .ToListAsync();
//                            var oldUomIds = oldJoins.Select(j => j.UOMId).ToList();
//                            var oldUoms = await _context.UOMs
//                                .Where(u => oldUomIds.Contains(u.Id))
//                                .ToListAsync();

//                            if (oldUoms.Any(u => u.QuantityOut > 0m))
//                            {
//                                await tx.RollbackAsync();
//                                return BadRequest(new
//                                {
//                                    message = $"Material '{material.NameEn}' has already been issued out and cannot be edited"
//                                });
//                            }

//                            _context.MemoDetailUOMs.RemoveRange(oldJoins);
//                            _context.UOMs.RemoveRange(oldUoms);
//                            await _context.SaveChangesAsync();

//                            existing.Quantity = 0m;
//                            existing.SplitQuantity = incoming.SplitQuantity;
//                        }

//                        // (ii) Non-serialized + Lot changed → move the detail to a different UOM.
//                        //      This is the fix for the bug in the desktop version.
//                        if (!material.IsSerialized && lotChanged)
//                        {
//                            // Detach from old UOM.
//                            var oldJoin = await _context.MemoDetailUOMs
//                                .Where(mdu => mdu.MemoDetailId == existing.Id)
//                                .FirstOrDefaultAsync();

//                            if (oldJoin is not null)
//                            {
//                                var oldUomId = oldJoin.UOMId;

//                                // Remove the join row first (FK Restrict).
//                                _context.MemoDetailUOMs.Remove(oldJoin);
//                                await _context.SaveChangesAsync();

//                                // Recompute old UOM's QuantityIn from remaining joins.
//                                var oldUom = await _context.UOMs.FindAsync(oldUomId);
//                                if (oldUom is not null)
//                                {
//                                    if (oldUom.QuantityOut > 0m)
//                                    {
//                                        await tx.RollbackAsync();
//                                        return BadRequest(new
//                                        {
//                                            message = $"Material '{material.NameEn}' has already been issued out and cannot have its lot changed"
//                                        });
//                                    }

//                                    decimal remaining = await _context.MemoDetailUOMs
//                                        .Where(j => j.UOMId == oldUom.Id)
//                                        .SumAsync(j => (decimal?)j.Quantity) ?? 0m;

//                                    if (remaining <= 0m)
//                                    {
//                                        // No other details reference this UOM → delete it.
//                                        _context.UOMs.Remove(oldUom);
//                                    }
//                                    else
//                                    {
//                                        oldUom.QuantityIn = remaining;
//                                    }
//                                    await _context.SaveChangesAsync();
//                                }
//                            }
//                            // Note: existing.Lot gets updated below with all the other scalar fields.
//                            // The new UOM is attached further down when QuantityIn is applied.
//                        }

//                        // (iii) Apply scalar updates (Lot, FactoryName, etc.) early.
//                        existing.MaterialId = material.Id;
//                        existing.Lot = newLot;
//                        existing.FactoryName = string.IsNullOrWhiteSpace(incoming.FactoryName) ? null : incoming.FactoryName.Trim();
//                        existing.CountryOrigin = string.IsNullOrWhiteSpace(incoming.CountryOrigin) ? null : incoming.CountryOrigin.Trim();
//                        existing.ProductionDate = incoming.ProductionDate;
//                        existing.ExpiryDate = incoming.ExpiryDate;
//                        existing.UnitId = incoming.UnitId;
//                        existing.Notes = string.IsNullOrWhiteSpace(incoming.Notes) ? null : incoming.Notes.Trim();
//                        await _context.SaveChangesAsync();

//                        // (iv) Adjust UOMs based on quantity change.
//                        if (material.IsSerialized)
//                        {
//                            if (existing.Quantity < incoming.Quantity)
//                            {
//                                decimal delta = incoming.Quantity - existing.Quantity;
//                                decimal split = existing.SplitQuantity == 0m ? 1m : existing.SplitQuantity;
//                                int toAdd = (int)(delta / split);

//                                var maxBox = await _context.UOMs
//                                    .Where(u => u.MaterialId == material.Id)
//                                    .Select(u => (int?)u.BoxNumber)
//                                    .MaxAsync() ?? 0;
//                                int nextBox = maxBox + 1;

//                                for (int i = 0; i < toAdd; i++)
//                                {
//                                    var uom = new UOM
//                                    {
//                                        MaterialId = material.Id,
//                                        PrintCount = 0,
//                                        Lot = existing.Lot,
//                                        QuantityIn = split,
//                                        QuantityOut = 0m,
//                                        QuantityWorkOrder = 0m,
//                                        Barcode = uomCounter.Value.ToString("00000000"),
//                                        Number = uomCounter.Value,
//                                        BoxNumber = nextBox
//                                    };
//                                    uomCounter.Value++;
//                                    nextBox++;

//                                    _context.UOMs.Add(uom);
//                                    await _context.SaveChangesAsync();

//                                    _context.MemoDetailUOMs.Add(new MemoDetailUOM
//                                    {
//                                        UOMId = uom.Id,
//                                        MemoDetailId = existing.Id,
//                                        Quantity = split
//                                    });
//                                    await _context.SaveChangesAsync();
//                                }
//                            }
//                            else if (existing.Quantity > incoming.Quantity)
//                            {
//                                decimal delta = existing.Quantity - incoming.Quantity;

//                                decimal availableToRemove = await _context.MemoDetailUOMs
//                                    .Where(mdu => mdu.MemoDetailId == existing.Id && mdu.Uom.QuantityOut == 0m)
//                                    .SumAsync(mdu => (decimal?)mdu.Quantity) ?? 0m;

//                                if (delta > availableToRemove)
//                                {
//                                    await tx.RollbackAsync();
//                                    return BadRequest(new
//                                    {
//                                        message = $"Material '{material.NameEn}' has already been issued out and cannot be edited"
//                                    });
//                                }

//                                decimal remaining = delta;
//                                while (remaining > 0m)
//                                {
//                                    var candidateJoin = await _context.MemoDetailUOMs
//                                        .Where(mdu => mdu.MemoDetailId == existing.Id && mdu.Uom.QuantityOut == 0m)
//                                        .OrderByDescending(mdu => mdu.Id)
//                                        .FirstOrDefaultAsync();

//                                    if (candidateJoin is null) break;

//                                    var candidateUom = await _context.UOMs.FindAsync(candidateJoin.UOMId);
//                                    if (candidateUom is null) break;

//                                    _context.MemoDetailUOMs.Remove(candidateJoin);
//                                    _context.UOMs.Remove(candidateUom);
//                                    remaining -= candidateUom.QuantityIn;
//                                    await _context.SaveChangesAsync();
//                                }
//                            }

//                            existing.Quantity = incoming.Quantity;
//                            await _context.SaveChangesAsync();
//                        }
//                        else
//                        {
//                            // -- Non-serialized: find-or-create the UOM for (Material, Lot),
//                            //    then recompute its QuantityIn from all joins.
//                            existing.SplitQuantity = 0m;

//                            // If Lot changed, the old join was already removed in step (ii).
//                            // If Lot did NOT change, we need to update the existing join.
//                            MemoDetailUOM? join;
//                            if (lotChanged)
//                            {
//                                // Old join already gone → find-or-create the new UOM below.
//                                join = null;
//                            }
//                            else
//                            {
//                                join = await _context.MemoDetailUOMs
//                                    .Where(mdu => mdu.MemoDetailId == existing.Id)
//                                    .FirstOrDefaultAsync();
//                            }

//                            UOM targetUom;
//                            if (join is null)
//                            {
//                                // Find existing UOM for (Material, Lot), or create one.
//                                targetUom = await _context.UOMs
//                                    .Where(u => u.MaterialId == material.Id && u.Lot == existing.Lot)
//                                    .FirstOrDefaultAsync();

//                                if (targetUom is null)
//                                {
//                                    var maxBox = await _context.UOMs
//                                        .Where(u => u.MaterialId == material.Id)
//                                        .Select(u => (int?)u.BoxNumber)
//                                        .MaxAsync() ?? 0;
//                                    int nextBox = maxBox + 1;

//                                    targetUom = new UOM
//                                    {
//                                        MaterialId = material.Id,
//                                        PrintCount = 0,
//                                        Lot = existing.Lot,
//                                        QuantityIn = 0m,           // computed below
//                                        QuantityOut = 0m,
//                                        QuantityWorkOrder = 0m,
//                                        Barcode = uomCounter.Value.ToString("00000000"),
//                                        Number = uomCounter.Value,
//                                        BoxNumber = nextBox
//                                    };
//                                    uomCounter.Value++;

//                                    _context.UOMs.Add(targetUom);
//                                    await _context.SaveChangesAsync();
//                                }

//                                _context.MemoDetailUOMs.Add(new MemoDetailUOM
//                                {
//                                    UOMId = targetUom.Id,
//                                    MemoDetailId = existing.Id,
//                                    Quantity = incoming.Quantity
//                                });
//                                await _context.SaveChangesAsync();
//                            }
//                            else
//                            {
//                                targetUom = (await _context.UOMs.FindAsync(join.UOMId))!;
//                                join.Quantity = incoming.Quantity;
//                                await _context.SaveChangesAsync();
//                            }

//                            // Recompute QuantityIn from all joins pointing at this UOM.
//                            // This is the self-healing version of the old delta formula.
//                            targetUom.QuantityIn = await _context.MemoDetailUOMs
//                                .Where(j => j.UOMId == targetUom.Id)
//                                .SumAsync(j => (decimal?)j.Quantity) ?? 0m;
//                            await _context.SaveChangesAsync();

//                            existing.Quantity = incoming.Quantity;
//                            await _context.SaveChangesAsync();
//                        }
//                    }
//                }

//                // ============================================================
//                // STEP 3 — HEADER FIELD UPDATES
//                // ============================================================
//                if (dto.Number.HasValue) memo.Number = dto.Number.Value;
//                if (dto.MemoDate.HasValue) memo.MemoDate = dto.MemoDate.Value;
//                if (dto.SupplierId.HasValue) memo.SupplierId = dto.SupplierId.Value;
//                if (dto.InvoiceNumber != null)
//                    memo.InvoiceNumber = string.IsNullOrWhiteSpace(dto.InvoiceNumber) ? null : dto.InvoiceNumber.Trim();
//                if (dto.InvoiceDate.HasValue) memo.InvoiceDate = dto.InvoiceDate.Value;
//                if (dto.WorkOrderId.HasValue) memo.WorkOrderId = dto.WorkOrderId.Value;
//                if (dto.Notes != null)
//                    memo.Notes = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes.Trim();

//                await _context.SaveChangesAsync();
//                await tx.CommitAsync();

//                await _auditLogService.LogAsync(
//                    User,
//                    "UpdateMemoIn",
//                    $"Updated memo-in (Id: {memo.Id}, Number: {memo.Number})",
//                    HttpContext.Connection.RemoteIpAddress?.ToString());

//                return Ok(await ToResponseDto(memo.Id));
//            }
//            catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlEx && sqlEx.Number is 2601 or 2627)
//            {
//                await tx.RollbackAsync();
//                return Conflict(new { message = "A memo with this number already exists" });
//            }
//            catch
//            {
//                await tx.RollbackAsync();
//                return StatusCode(500, new
//                {
//                    message = "An unexpected error occurred while updating the memo. Please try again or contact support."
//                });
//            }
//        }

//        // GET api/memos/index?type=In&page=1&pageSize=20
//        [HttpGet("index")]
//        public async Task<IActionResult> Index(
//            [FromQuery] MemoType? type = null,
//            [FromQuery] int page = 1,
//            [FromQuery] int pageSize = 20)
//        {
//            if (page < 1) page = 1;
//            if (pageSize < 1 || pageSize > 100) pageSize = 20;

//            var query = _context.Memos
//                .Include(m => m.Supplier)
//                .Include(m => m.WorkOrder)
//                .AsQueryable();

//            if (type.HasValue)
//                query = query.Where(m => m.TypeId == type.Value);

//            var ordered = query
//                .OrderByDescending(m => m.MemoDate)
//                .ThenByDescending(m => m.Id);

//            var totalCount = await ordered.CountAsync();

//            // Project via an anonymous shape first, then fill in enum labels in memory.
//            // Enum labels come from [Description], which SQL Server cannot see.
//            var rows = await ordered
//                .Skip((page - 1) * pageSize)
//                .Take(pageSize)
//                .Select(m => new
//                {
//                    m.Id,
//                    m.Number,
//                    m.MemoDate,
//                    m.CreationDate,
//                    m.TypeId,
//                    m.InvoiceNumber,
//                    m.SupplierId,
//                    SupplierNameEn = m.Supplier != null ? m.Supplier.NameEn : null,
//                    SupplierNameAr = m.Supplier != null ? m.Supplier.NameAr : null,
//                    m.WorkOrderId,
//                    WorkOrderNumber = m.WorkOrder != null ? (long?)m.WorkOrder.Number : null,
//                    m.IsActive,
//                    DetailCount = m.Details.Count
//                })
//                .ToListAsync();

//            var items = rows.Select(r =>
//            {
//                var (typeAr, typeEn) = r.TypeId.GetBilingualNames();
//                return new MemoListItemDto
//                {
//                    Id = r.Id,
//                    Number = r.Number,
//                    MemoDate = r.MemoDate,
//                    CreationDate = r.CreationDate,
//                    TypeId = r.TypeId,
//                    TypeNameEn = typeEn,
//                    TypeNameAr = typeAr,
//                    InvoiceNumber = r.InvoiceNumber,
//                    SupplierId = r.SupplierId,
//                    SupplierNameEn = r.SupplierNameEn,
//                    SupplierNameAr = r.SupplierNameAr,
//                    WorkOrderId = r.WorkOrderId,
//                    WorkOrderNumber = r.WorkOrderNumber,
//                    IsActive = r.IsActive,
//                    DetailCount = r.DetailCount
//                };
//            }).ToList();

//            return Ok(new PagedResultDto<MemoListItemDto>
//            {
//                Items = items,
//                TotalCount = totalCount,
//                Page = page,
//                PageSize = pageSize
//            });
//        }

//        // GET api/memos/{id}
//        [HttpGet("{id}")]
//        public async Task<IActionResult> GetOne(int id)
//        {
//            var exists = await _context.Memos.AnyAsync(m => m.Id == id);
//            if (!exists)
//                return NotFound(new { message = "Memo not found" });

//            return Ok(await ToResponseDto(id));
//        }

//        private async Task<MemoResponseDto> ToResponseDto(int memoId)
//        {
//            var memo = await _context.Memos
//                .Include(m => m.Supplier)
//                .Include(m => m.WorkOrder)
//                .Include(m => m.Details)
//                    .ThenInclude(d => d.Material)
//                .Include(m => m.Details)
//                    .ThenInclude(d => d.UOMs)
//                        .ThenInclude(u => u.Uom)
//                .FirstAsync(m => m.Id == memoId);

//            var (typeAr, typeEn) = memo.TypeId.GetBilingualNames();

//            // ---- Totals ----
//            // Computed in memory from the already-loaded graph. Cheap, and it avoids
//            // extra round-trips. If memo sizes ever grow large, move these to SQL
//            // via a `.Select(...).SumAsync()` projection before the .Include chain.
//            var detailList = memo.Details.ToList();

//            int totalUOMCount = detailList.Sum(d => d.UOMs.Count);
//            decimal totalQuantity = detailList.Sum(d => d.Quantity);
//            decimal totalUOMQuantity = detailList.Sum(d => d.UOMs.Sum(u => u.Quantity));

//            return new MemoResponseDto
//            {
//                Id = memo.Id,
//                Number = memo.Number,
//                MemoDate = memo.MemoDate,
//                CreationDate = memo.CreationDate,
//                TypeId = memo.TypeId,
//                TypeNameEn = typeEn,
//                TypeNameAr = typeAr,
//                InvoiceNumber = memo.InvoiceNumber,
//                InvoiceDate = memo.InvoiceDate,
//                SupplierId = memo.SupplierId,
//                SupplierNumber = memo.Supplier?.SupplierNumber,
//                SupplierNameEn = memo.Supplier?.NameEn,
//                SupplierNameAr = memo.Supplier?.NameAr,
//                WorkOrderId = memo.WorkOrderId,
//                WorkOrderNumber = memo.WorkOrder?.Number,
//                Notes = memo.Notes,
//                IsActive = memo.IsActive,

//                TotalDetailCount = detailList.Count,
//                TotalUOMCount = totalUOMCount,
//                TotalQuantity = totalQuantity,
//                TotalUOMQuantity = totalUOMQuantity,

//                Details = detailList.Select(d => new MemoDetailResponseDto
//                {
//                    Id = d.Id,
//                    MemoId = d.MemoId,
//                    MaterialId = d.MaterialId,
//                    MaterialNumber = d.Material.MaterialNumber,
//                    MaterialNameEn = d.Material.NameEn,
//                    MaterialNameAr = d.Material.NameAr,
//                    MaterialIsSerialized = d.Material.IsSerialized,
//                    Quantity = d.Quantity,
//                    SplitQuantity = d.SplitQuantity,
//                    Lot = d.Lot,
//                    FactoryName = d.FactoryName,
//                    CountryOrigin = d.CountryOrigin,
//                    ProductionDate = d.ProductionDate,
//                    ExpiryDate = d.ExpiryDate,
//                    UnitId = d.UnitId,
//                    Notes = d.Notes,
//                    UOMs = d.UOMs.Select(u => new UOMResponseDto
//                    {
//                        Id = u.Uom.Id,
//                        Barcode = u.Uom.Barcode,
//                        BoxNumber = u.Uom.BoxNumber,
//                        PrintCount = u.Uom.PrintCount,
//                        Quantity = u.Quantity
//                    }).ToList()
//                }).ToList()
//            };
//        }
//        // Shared UOM-generation logic for In memos.
//        // Mirrors the desktop's serialized/non-serialized branches.
//        private async Task GenerateUOMsForDetailAsync(
//      MemoDetail detail,
//      Material material,
//      Counter uomCounter,
//      int startBoxNumber)
//        {
//            int nextBox = startBoxNumber;

//            if (material.IsSerialized)
//            {
//                if (detail.SplitQuantity == 0m) detail.SplitQuantity = 1m;
//                int toAdd = (int)(detail.Quantity / detail.SplitQuantity);

//                for (int i = 0; i < toAdd; i++)
//                {
//                    var uom = new UOM
//                    {
//                        MaterialId = material.Id,
//                        PrintCount = 0,
//                        Lot = detail.Lot,
//                        QuantityIn = detail.SplitQuantity,
//                        QuantityOut = 0m,
//                        QuantityWorkOrder = 0m,
//                        Barcode = uomCounter.Value.ToString("00000000"),
//                        Number = uomCounter.Value,
//                        BoxNumber = nextBox
//                    };
//                    uomCounter.Value++;
//                    nextBox++;

//                    _context.UOMs.Add(uom);
//                    await _context.SaveChangesAsync();

//                    _context.MemoDetailUOMs.Add(new MemoDetailUOM
//                    {
//                        UOMId = uom.Id,
//                        MemoDetailId = detail.Id,
//                        Quantity = detail.SplitQuantity
//                    });
//                    await _context.SaveChangesAsync();
//                }
//            }
//            else
//            {
//                var oldUom = await _context.UOMs
//                    .Where(u => u.MaterialId == material.Id && u.Lot == detail.Lot)
//                    .FirstOrDefaultAsync();

//                detail.SplitQuantity = 0m;

//                if (oldUom == null)
//                {
//                    var uom = new UOM
//                    {
//                        MaterialId = material.Id,
//                        PrintCount = 0,
//                        Lot = detail.Lot,
//                        QuantityIn = detail.Quantity,
//                        QuantityOut = 0m,
//                        QuantityWorkOrder = 0m,
//                        Barcode = uomCounter.Value.ToString("00000000"),
//                        Number = uomCounter.Value,
//                        BoxNumber = nextBox
//                    };
//                    uomCounter.Value++;

//                    _context.UOMs.Add(uom);
//                    await _context.SaveChangesAsync();

//                    _context.MemoDetailUOMs.Add(new MemoDetailUOM
//                    {
//                        UOMId = uom.Id,
//                        MemoDetailId = detail.Id,
//                        Quantity = detail.Quantity
//                    });
//                    await _context.SaveChangesAsync();
//                }
//                else
//                {
//                    oldUom.QuantityIn += detail.Quantity;
//                    await _context.SaveChangesAsync();

//                    _context.MemoDetailUOMs.Add(new MemoDetailUOM
//                    {
//                        UOMId = oldUom.Id,
//                        MemoDetailId = detail.Id,
//                        Quantity = detail.Quantity
//                    });
//                    await _context.SaveChangesAsync();
//                }
//            }
//        }
//        // A mutable counter holder. Used because async methods cannot take ref parameters.
//        private sealed class Counter
//        {
//            public long Value { get; set; }

//            public Counter(long start) => Value = start;
//        }
//    }
//}