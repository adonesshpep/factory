//using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.EntityFrameworkCore.Metadata;
//using neotonic.Models;

//public class AppDbContext : IdentityDbContext<AppUser>
//{
//    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

//    public DbSet<AuditLog> AuditLogs { get; set; }
//    public DbSet<AuditLogRole> AuditLogRoles { get; set; }
//    public DbSet<Category> Categories { get; set; }
//    public DbSet<MaterialType> MaterialTypes { get; set; }
//    public DbSet<Material> Materials { get; set; }
//    public DbSet<StepType> StepTypes { get; set; }
//    public DbSet<Step> Steps { get; set; }
//    public DbSet<Protocol> Protocols { get; set; }
//    public DbSet<ProtocolStep> ProtocolSteps { get; set; }
//    public DbSet<Department> Departments { get; set; }
//    public DbSet<Machine> Machines { get; set; }
//    public DbSet<Scale> Scales { get; set; }
//    public DbSet<LabEquipment> LabEquipments { get; set; }
//    public DbSet<BoxPath> BoxPaths { get; set; }
//    public DbSet<AnnualPlan> AnnualPlans { get; set; }
//    public DbSet<AnnualPlanDetail> AnnualPlanDetails { get; set; }
//    public DbSet<Supplier> Suppliers { get; set; }
//    public DbSet<Product> Products { get; set; }
//    public DbSet<Worker> Workers { get; set; }
//    public DbSet<ScaleEqualization> ScaleEqualizations { get; set; }
//    public DbSet<Clothe> Clothes { get; set; }
//    public DbSet<CleaningRequest> CleaningRequests { get; set; }
//    public DbSet<ContractReview> ContractReviews { get; set; }
//    public DbSet<CertificateOfAnalysis> CertificatesOfAnalysis { get; set; }
//    public DbSet<PurchaseOrder> PurchaseOrders { get; set; }
//    public DbSet<Memo> Memos { get; set; }
//    public DbSet<MemoDetail> MemoDetails { get; set; }
//    public DbSet<MemoDetailUOM> MemoDetailUOMs { get; set; }
//    public DbSet<UOM> UOMs { get; set; }
//    public DbSet<WorkOrder> WorkOrders { get; set; }
//    public DbSet<WorkOrderDetail> WorkOrderDetails { get; set; }
//    public DbSet<WorkOrderDetailUOM> WorkOrderDetailUOMs { get; set; }

//    public DbSet<RFQ> RFQs { get; set; }
//    public DbSet<MemoRWIn> MemoRWIns { get; set; }
//    public DbSet<MemoRWDetail> MemoRWDetails { get; set; }

//    // ---- inventory ----
//    public DbSet<Location> Locations { get; set; }
//    public DbSet<StockMove> StockMoves { get; set; }
//    public DbSet<PreparingOrder> PreparingOrders { get; set; }
//    public DbSet<ReleaseOrder> ReleaseOrders { get; set; }
//    public DbSet<TransferOrder> TransferOrders { get; set; }
//    public DbSet<TransferOrderLine> TransferOrderLines { get; set; }
//    public DbSet<Sample> Samples { get; set; }   // replaces SupplierSamples

//    protected override void OnModelCreating(ModelBuilder modelBuilder)
//    {
//        base.OnModelCreating(modelBuilder);

//        // ---- Restrict delete behaviors: no Data Annotation equivalent exists for this ----

//        modelBuilder.Entity<Material>(entity =>
//        {
//            entity.ToTable("materials", t =>
//                t.HasCheckConstraint("ck_materials_min_max", "[min] >= 0 AND [max] >= [min]"));

//            entity.HasOne(m => m.Category).WithMany()
//                  .HasForeignKey(m => m.CategoryId)
//                  .OnDelete(DeleteBehavior.Restrict);

//            entity.HasOne(m => m.MaterialType).WithMany()
//                  .HasForeignKey(m => m.MaterialTypeId)
//                  .OnDelete(DeleteBehavior.Restrict);
//        });

//        modelBuilder.Entity<Step>(entity =>
//        {
//            entity.HasOne(s => s.StepType).WithMany()
//                  .HasForeignKey(s => s.StepTypeId)
//                  .OnDelete(DeleteBehavior.Restrict);

//            entity.HasOne(s => s.BoxPath).WithMany()
//                  .HasForeignKey(s => s.BoxPathId)
//                  .OnDelete(DeleteBehavior.Restrict);
//        });

//        modelBuilder.Entity<Protocol>(entity =>
//        {
//            entity.HasOne(p => p.StepType).WithMany()
//                  .HasForeignKey(p => p.StepTypeId)
//                  .OnDelete(DeleteBehavior.Restrict);
//        });

//        modelBuilder.Entity<ProtocolStep>(entity =>
//        {
//            // Protocol -> ProtocolStep stays Cascade (EF's default for a required
//            // relationship), so only the Step side needs explicit config here.
//            entity.HasOne(ps => ps.Step)
//                  .WithMany()
//                  .HasForeignKey(ps => ps.StepId)
//                  .OnDelete(DeleteBehavior.Restrict);
//        });

//        modelBuilder.Entity<Machine>(entity =>
//        {
//            entity.HasOne(m => m.Department).WithMany()
//                  .HasForeignKey(m => m.DepartmentId)
//                  .OnDelete(DeleteBehavior.Restrict);
//        });

//        modelBuilder.Entity<Scale>(entity =>
//        {
//            entity.HasOne(s => s.Department).WithMany()
//                  .HasForeignKey(s => s.DepartmentId)
//                  .OnDelete(DeleteBehavior.Restrict);
//        });

//        modelBuilder.Entity<AnnualPlan>(entity =>
//        {
//            entity.HasOne(p => p.Department).WithMany()
//                  .HasForeignKey(p => p.DepartmentId)
//                  .OnDelete(DeleteBehavior.Restrict);

//            entity.HasOne(p => p.MaterialType).WithMany()
//                  .HasForeignKey(p => p.MaterialTypeId)
//                  .OnDelete(DeleteBehavior.Restrict);
//        });

//        modelBuilder.Entity<AnnualPlanDetail>(entity =>
//        {
//            // AnnualPlanDetail -> AnnualPlan stays Cascade (default), only
//            // the Material side needs Restrict.
//            entity.HasOne(p => p.Material).WithMany()
//                  .HasForeignKey(p => p.MaterialId)
//                  .OnDelete(DeleteBehavior.Restrict);
//        });

//        modelBuilder.Entity<Product>(entity =>
//        {
//            entity.HasOne(m => m.Category).WithMany()
//                  .HasForeignKey(m => m.CategoryId)
//                  .OnDelete(DeleteBehavior.Restrict);
//        });

//        modelBuilder.HasSequence<int>("worker_number_seq")
//            .StartsAt(1)
//            .IncrementsBy(1);

//        modelBuilder.Entity<Worker>(entity =>
//        {
//            entity.Property(w => w.WorkerNumber)
//                  .HasDefaultValueSql("NEXT VALUE FOR worker_number_seq")
//                  .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
//        });

//        modelBuilder.Entity<ScaleEqualization>(entity =>
//        {
//            entity.HasOne(e => e.Scale).WithMany()
//                  .HasForeignKey(e => e.ScaleId)
//                  .OnDelete(DeleteBehavior.Restrict);

//            entity.HasOne(e => e.Worker).WithMany()
//                  .HasForeignKey(e => e.WorkerId)
//                  .OnDelete(DeleteBehavior.Restrict);
//        });

//        modelBuilder.Entity<CleaningRequest>(entity =>
//        {
//            entity.ToTable("cleaning_requests", t => t.HasCheckConstraint(
//                "ck_cleaning_requests_target",
//                "([cleaning_type] = 1 AND [TargetDepartmentId] IS NOT NULL AND [ScaleId] IS NULL AND [MachineId] IS NULL AND [ClotheId] IS NULL) OR " +
//                "([cleaning_type] = 2 AND [ScaleId] IS NOT NULL AND [TargetDepartmentId] IS NULL AND [MachineId] IS NULL AND [ClotheId] IS NULL) OR " +
//                "([cleaning_type] = 3 AND [MachineId] IS NOT NULL AND [TargetDepartmentId] IS NULL AND [ScaleId] IS NULL AND [ClotheId] IS NULL) OR " +
//                "([cleaning_type] = 4 AND [ClotheId] IS NOT NULL AND [TargetDepartmentId] IS NULL AND [ScaleId] IS NULL AND [MachineId] IS NULL)"));

//            entity.HasOne(r => r.Department).WithMany()
//                  .HasForeignKey(r => r.DepartmentId).OnDelete(DeleteBehavior.Restrict);

//            entity.HasOne(r => r.Protocol).WithMany()
//                  .HasForeignKey(r => r.ProtocolId).OnDelete(DeleteBehavior.Restrict);

//            entity.HasOne(r => r.TargetDepartment).WithMany()
//                  .HasForeignKey(r => r.TargetDepartmentId).OnDelete(DeleteBehavior.Restrict);

//            entity.HasOne(r => r.Scale).WithMany()
//                  .HasForeignKey(r => r.ScaleId).OnDelete(DeleteBehavior.Restrict);

//            entity.HasOne(r => r.Machine).WithMany()
//                  .HasForeignKey(r => r.MachineId).OnDelete(DeleteBehavior.Restrict);

//            entity.HasOne(r => r.Clothe).WithMany()
//                  .HasForeignKey(r => r.ClotheId).OnDelete(DeleteBehavior.Restrict);
//        });

//        modelBuilder.Entity<ContractReview>(entity =>
//        {
//            entity.HasOne(c => c.Supplier).WithMany()
//                  .HasForeignKey(c => c.SupplierId)
//                  .OnDelete(DeleteBehavior.Restrict);

//            // TODO(purchasing redesign): delete this relation when MaterialId is removed from
//            // ContractReview (the material will come from the RFQ).
//            entity.HasOne(c => c.Material).WithMany()
//                  .HasForeignKey(c => c.MaterialId)
//                  .OnDelete(DeleteBehavior.Restrict);

//            // CR hangs off an RFQ, which hangs off an AnnualPlan.
//            entity.HasOne(c => c.RFQ).WithMany()
//                  .HasForeignKey(c => c.RFQId)
//                  .OnDelete(DeleteBehavior.Restrict);
//        });

//        // Sample = former SupplierSample. Supplier sample -> contract review, delivery sample -> UOM.
//        modelBuilder.Entity<Sample>(entity =>
//        {
//            entity.ToTable("samples", t => t.HasCheckConstraint("ck_samples_link",
//                "([sample_type] = 1 AND [ContractReviewId] IS NOT NULL AND [UomId] IS NULL) OR " +
//                "([sample_type] = 2 AND [UomId] IS NOT NULL AND [ContractReviewId] IS NULL)"));

//            entity.HasOne(s => s.ContractReview).WithMany()
//                  .HasForeignKey(s => s.ContractReviewId)
//                  .OnDelete(DeleteBehavior.Restrict);

//            entity.HasOne(s => s.Uom).WithMany(u => u.Samples)
//                  .HasForeignKey(s => s.UomId)
//                  .OnDelete(DeleteBehavior.Restrict);
//        });

//        modelBuilder.Entity<CertificateOfAnalysis>(entity =>
//        {
//            entity.ToTable("certificates_of_analysis", t =>
//            {
//                t.HasCheckConstraint("ck_certificates_of_analysis_moisture",
//                    "[moisture_percentage] >= 0 AND [moisture_percentage] <= 100");

//                // 1 = Accepted, 2 = Rejected (values of AnalysisResults, don't renumber)
//                t.HasCheckConstraint("ck_certificates_of_analysis_rejection",
//                    "([result] = 1 AND [rejection_reason] IS NULL) OR ([result] = 2 AND [rejection_reason] IS NOT NULL)");
//            });

//            entity.HasOne(c => c.Sample).WithMany()
//                  .HasForeignKey(c => c.SampleId)
//                  .OnDelete(DeleteBehavior.Restrict);
//        });

//        modelBuilder.Entity<PurchaseOrder>(entity =>
//        {
//            entity.HasOne(p => p.ContractReview).WithMany()
//                  .HasForeignKey(p => p.ContractReviewId)
//                  .OnDelete(DeleteBehavior.Restrict);

//            entity.HasOne(p => p.CertificateOfAnalysis).WithMany()
//                  .HasForeignKey(p => p.CertificateOfAnalysisId)
//                  .OnDelete(DeleteBehavior.Restrict);
//        });

//        modelBuilder.Entity<WorkOrder>(entity =>
//        {
//            entity.HasMany(w => w.Details).WithOne(d => d.WorkOrder)
//                  .HasForeignKey(d => d.WorkOrderId)
//                  .OnDelete(DeleteBehavior.Cascade);
//        });

//        modelBuilder.Entity<WorkOrderDetail>(entity =>
//        {
//            entity.HasOne(d => d.Material).WithMany()
//                  .HasForeignKey(d => d.MaterialId)
//                  .OnDelete(DeleteBehavior.Restrict);

//            entity.HasMany(d => d.UOMs).WithOne(u => u.WorkOrderDetail)
//                  .HasForeignKey(u => u.WorkOrderDetailId)
//                  .OnDelete(DeleteBehavior.Cascade);
//        });

//        modelBuilder.Entity<WorkOrderDetailUOM>(entity =>
//        {
//            entity.HasOne(u => u.Uom).WithMany()
//                  .HasForeignKey(u => u.UOMId)
//                  .OnDelete(DeleteBehavior.Restrict);
//        });

//        // UOM: material (existing) + location, split parent (inventory).
//        // Reservation is NOT a UOM column: it is the sum of Done transfer order lines.
//        modelBuilder.Entity<UOM>(entity =>
//        {
//            entity.ToTable("uoms", t => t.HasCheckConstraint("ck_uoms_quantity", "[quantity] >= 0"));

//            entity.HasOne(u => u.Material).WithMany()
//                  .HasForeignKey(u => u.MaterialId)
//                  .OnDelete(DeleteBehavior.Restrict);

//            entity.HasOne(u => u.Location).WithMany()
//                  .HasForeignKey(u => u.LocationId)
//                  .OnDelete(DeleteBehavior.Restrict);

//            // self reference: must be Restrict on SQL Server
//            entity.HasOne(u => u.Parent).WithMany(u => u.Children)
//                  .HasForeignKey(u => u.ParentUomId)
//                  .OnDelete(DeleteBehavior.Restrict);
//        });

//        modelBuilder.Entity<Memo>(entity =>
//        {
//            entity.HasOne(m => m.Supplier).WithMany()
//                  .HasForeignKey(m => m.SupplierId)
//                  .OnDelete(DeleteBehavior.Restrict);

//            entity.HasOne(m => m.WorkOrder).WithMany()
//                  .HasForeignKey(m => m.WorkOrderId)
//                  .OnDelete(DeleteBehavior.Restrict);

//            entity.HasMany(m => m.Details).WithOne(d => d.Memo)
//                  .HasForeignKey(d => d.MemoId)
//                  .OnDelete(DeleteBehavior.Cascade);
//        });

//        modelBuilder.Entity<MemoDetail>(entity =>
//        {
//            entity.HasOne(d => d.Material).WithMany()
//                  .HasForeignKey(d => d.MaterialId)
//                  .OnDelete(DeleteBehavior.Restrict);

//            entity.HasMany(d => d.UOMs).WithOne(u => u.MemoDetail)
//                  .HasForeignKey(u => u.MemoDetailId)
//                  .OnDelete(DeleteBehavior.Cascade);
//        });

//        modelBuilder.Entity<MemoDetailUOM>(entity =>
//        {
//            entity.HasOne(u => u.Uom).WithMany()
//                  .HasForeignKey(u => u.UOMId)
//                  .OnDelete(DeleteBehavior.Restrict);
//        });

//        modelBuilder.Entity<RFQ>(entity =>
//        {
//            entity.HasOne(r => r.AnnualPlan).WithMany()
//                  .HasForeignKey(r => r.AnnualPlanID)
//                  .OnDelete(DeleteBehavior.Restrict);

//            entity.HasOne(r => r.Material).WithMany()
//                  .HasForeignKey(r => r.MaterialId)
//                  .OnDelete(DeleteBehavior.Restrict);
//        });

//        modelBuilder.HasSequence<long>("memo_rw_in_number_seq")
//            .StartsAt(1)
//            .IncrementsBy(1);

//        modelBuilder.Entity<MemoRWIn>(entity =>
//        {
//            entity.Property(m => m.Number)
//                  .HasDefaultValueSql("NEXT VALUE FOR memo_rw_in_number_seq")
//                  .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);

//            entity.HasOne(m => m.PurchaseOrder).WithMany()
//                  .HasForeignKey(m => m.PurchaseOrderId)
//                  .OnDelete(DeleteBehavior.Restrict);

//            entity.HasOne(m => m.Worker).WithMany()
//                  .HasForeignKey(m => m.WorkerId)
//                  .OnDelete(DeleteBehavior.Restrict);
//        });

//        modelBuilder.Entity<MemoRWDetail>(entity =>
//        {
//            entity.ToTable("memo_rw_details", t => t.HasCheckConstraint("ck_memo_rw_details_tear", "[tear] >= 0"));

//            // MemoRWDetail -> MemoRWIn stays Cascade (default for a required relationship)
//            entity.HasOne(d => d.Memo).WithMany(m => m.Details)
//                  .HasForeignKey(d => d.MemoId)
//                  .OnDelete(DeleteBehavior.Cascade);

//            // one box per detail, and a box belongs to at most one detail
//            entity.HasOne(d => d.Uom).WithOne(u => u.MemoRWDetail)
//                  .HasForeignKey<MemoRWDetail>(d => d.UomId)
//                  .OnDelete(DeleteBehavior.Restrict);
//        });

//        // ================= inventory =================

//        modelBuilder.Entity<Location>().HasData(
//            new Location { Id = LocationIds.Receiving, Code = LocationCodes.Receiving, NameEn = "Receiving", NameAr = "الاستلام", SortOrder = 1, IsActive = true },
//            new Location { Id = LocationIds.Accepted, Code = LocationCodes.Accepted, NameEn = "Accepted", NameAr = "المقبول", SortOrder = 2, IsActive = true },
//            new Location { Id = LocationIds.Rejected, Code = LocationCodes.Rejected, NameEn = "Rejected", NameAr = "المرفوض", SortOrder = 3, IsActive = true },
//            new Location { Id = LocationIds.Prepared, Code = LocationCodes.Prepared, NameEn = "Prepared", NameAr = "المحضّر", SortOrder = 4, IsActive = true },
//            new Location { Id = LocationIds.Released, Code = LocationCodes.Released, NameEn = "Released", NameAr = "المصروف للإنتاج", SortOrder = 5, IsActive = true },
//            new Location { Id = LocationIds.Products, Code = LocationCodes.Products, NameEn = "Products", NameAr = "المنتجات", SortOrder = 6, IsActive = true });

//        modelBuilder.Entity<StockMove>(entity =>
//        {
//            entity.ToTable("stock_moves", t => t.HasCheckConstraint("ck_stock_moves_quantity", "[quantity] > 0"));

//            entity.HasOne(m => m.Uom).WithMany()
//                  .HasForeignKey(m => m.UomId)
//                  .OnDelete(DeleteBehavior.Restrict);

//            entity.HasOne(m => m.FromLocation).WithMany()
//                  .HasForeignKey(m => m.FromLocationId)
//                  .OnDelete(DeleteBehavior.Restrict);

//            entity.HasOne(m => m.ToLocation).WithMany()
//                  .HasForeignKey(m => m.ToLocationId)
//                  .OnDelete(DeleteBehavior.Restrict);
//        });

//        modelBuilder.Entity<PreparingOrder>(entity =>
//        {
//            entity.HasOne(p => p.Protocol).WithMany()
//                  .HasForeignKey(p => p.ProtocolId)
//                  .OnDelete(DeleteBehavior.Restrict);

//            // one transfer per order; deleting the order deletes its transfer and lines
//            entity.HasOne(p => p.TransferOrder).WithOne(t => t.PreparingOrder)
//                  .HasForeignKey<TransferOrder>(t => t.PreparingOrderId)
//                  .OnDelete(DeleteBehavior.Cascade);
//        });

//        modelBuilder.Entity<ReleaseOrder>(entity =>
//        {
//            entity.HasOne(r => r.TransferOrder).WithOne(t => t.ReleaseOrder)
//                  .HasForeignKey<TransferOrder>(t => t.ReleaseOrderId)
//                  .OnDelete(DeleteBehavior.Cascade);
//        });

//        modelBuilder.Entity<TransferOrder>(entity =>
//        {
//            entity.ToTable("transfer_orders", t =>
//            {
//                t.HasCheckConstraint("ck_transfer_orders_locations", "[FromLocationId] <> [ToLocationId]");

//                // exactly one owner, same pattern as CleaningRequest
//                t.HasCheckConstraint("ck_transfer_orders_owner",
//                    "([PreparingOrderId] IS NOT NULL AND [ReleaseOrderId] IS NULL) OR ([PreparingOrderId] IS NULL AND [ReleaseOrderId] IS NOT NULL)");
//            });

//            entity.HasOne(o => o.FromLocation).WithMany()
//                  .HasForeignKey(o => o.FromLocationId)
//                  .OnDelete(DeleteBehavior.Restrict);

//            entity.HasOne(o => o.ToLocation).WithMany()
//                  .HasForeignKey(o => o.ToLocationId)
//                  .OnDelete(DeleteBehavior.Restrict);

//            entity.HasOne(o => o.Material).WithMany()
//                  .HasForeignKey(o => o.MaterialId)
//                  .OnDelete(DeleteBehavior.Restrict);
//        });

//        modelBuilder.Entity<TransferOrderLine>(entity =>
//        {
//            entity.ToTable("transfer_order_lines", t => t.HasCheckConstraint("ck_transfer_order_lines_quantity", "[quantity] > 0"));

//            entity.HasOne(l => l.TransferOrder).WithMany(o => o.Lines)
//                  .HasForeignKey(l => l.TransferOrderId)
//                  .OnDelete(DeleteBehavior.Cascade);

//            entity.HasOne(l => l.Uom).WithMany()
//                  .HasForeignKey(l => l.UomId)
//                  .OnDelete(DeleteBehavior.Restrict);
//        });
//    }
//}
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using neotonic.Models;

public class AppDbContext : IdentityDbContext<AppUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // ---- identity / audit ----
    public DbSet<AuditLog> AuditLogs { get; set; }
    public DbSet<AuditLogRole> AuditLogRoles { get; set; }

    // ---- master data ----
    public DbSet<Category> Categories { get; set; }
    public DbSet<MaterialType> MaterialTypes { get; set; }
    public DbSet<Material> Materials { get; set; }
    public DbSet<StepType> StepTypes { get; set; }
    public DbSet<Step> Steps { get; set; }
    public DbSet<Protocol> Protocols { get; set; }
    public DbSet<ProtocolStep> ProtocolSteps { get; set; }
    public DbSet<Department> Departments { get; set; }
    public DbSet<Machine> Machines { get; set; }
    public DbSet<Scale> Scales { get; set; }
    public DbSet<LabEquipment> LabEquipments { get; set; }
    public DbSet<BoxPath> BoxPaths { get; set; }
    public DbSet<Supplier> Suppliers { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Worker> Workers { get; set; }
    public DbSet<Clothe> Clothes { get; set; }

    // ---- business ----
    public DbSet<AnnualPlan> AnnualPlans { get; set; }
    public DbSet<AnnualPlanDetail> AnnualPlanDetails { get; set; }
    public DbSet<ScaleEqualization> ScaleEqualizations { get; set; }
    public DbSet<CleaningRequest> CleaningRequests { get; set; }
    public DbSet<ContractReview> ContractReviews { get; set; }
    public DbSet<CertificateOfAnalysis> CertificatesOfAnalysis { get; set; }
    public DbSet<PurchaseOrder> PurchaseOrders { get; set; }
    public DbSet<RFQ> RFQs { get; set; }

    // ---- legacy (kept, ignored) ----
    public DbSet<Memo> Memos { get; set; }
    public DbSet<MemoDetail> MemoDetails { get; set; }
    public DbSet<MemoDetailUOM> MemoDetailUOMs { get; set; }
    public DbSet<WorkOrder> WorkOrders { get; set; }
    public DbSet<WorkOrderDetail> WorkOrderDetails { get; set; }
    public DbSet<WorkOrderDetailUOM> WorkOrderDetailUOMs { get; set; }

    // ---- receiving ----
    public DbSet<MemoRWIn> MemoRWIns { get; set; }
    public DbSet<MemoRWDetail> MemoRWDetails { get; set; }
    public DbSet<Sample> Samples { get; set; }

    // ---- inventory ----
    public DbSet<Inventory> Inventories { get; set; }
    public DbSet<Location> Locations { get; set; }
    public DbSet<Shelf> Shelves { get; set; }
    public DbSet<Flow> Flows { get; set; }
    public DbSet<FlowStep> FlowSteps { get; set; }
    public DbSet<UOM> UOMs { get; set; }
    public DbSet<StockMove> StockMoves { get; set; }
    public DbSet<TransferOrder> TransferOrders { get; set; }
    public DbSet<TransferOrderLine> TransferOrderLines { get; set; }
    public DbSet<PreparingOrder> PreparingOrders { get; set; }
    public DbSet<ReleaseOrder> ReleaseOrders { get; set; }
    public DbSet<ScrapOrder> ScrapOrders { get; set; }
    public DbSet<SupplierReturnOrder> SupplierReturnOrders { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<PreparingOrderTransferLink> PreparingOrderTransferLinks { get; set; }
    public DbSet<WashingReport> WashingReports { get; set; }
    public DbSet<PurificationReport> PurificationReports { get; set; }
    public DbSet<SterilizationReport> SterilizationReports { get; set; }
    public DbSet<DryingReport> DryingReports { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ============================================================
        // existing business model — delete behaviors (unchanged)
        // ============================================================

        modelBuilder.Entity<Material>(entity =>
        {
            entity.ToTable("materials", t =>
                t.HasCheckConstraint("ck_materials_min_max", "[min] >= 0 AND [max] >= [min]"));

            entity.HasOne(m => m.Category).WithMany()
                  .HasForeignKey(m => m.CategoryId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(m => m.MaterialType).WithMany()
                  .HasForeignKey(m => m.MaterialTypeId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<MaterialType>(entity =>
            entity.HasOne(t => t.Flow).WithMany()
                  .HasForeignKey(t => t.FlowId).OnDelete(DeleteBehavior.Restrict));

        modelBuilder.Entity<Step>(entity =>
        {
            entity.HasOne(s => s.StepType).WithMany()
                  .HasForeignKey(s => s.StepTypeId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(s => s.BoxPath).WithMany()
                  .HasForeignKey(s => s.BoxPathId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(s => s.VirtualLocation).WithMany()
         .HasForeignKey(s => s.VirtualLocationId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Protocol>(entity =>
            entity.HasOne(p => p.StepType).WithMany()
                  .HasForeignKey(p => p.StepTypeId).OnDelete(DeleteBehavior.Restrict));

        modelBuilder.Entity<ProtocolStep>(entity =>
            entity.HasOne(ps => ps.Step).WithMany()
                  .HasForeignKey(ps => ps.StepId).OnDelete(DeleteBehavior.Restrict));

        modelBuilder.Entity<Machine>(entity =>
            entity.HasOne(m => m.Department).WithMany()
                  .HasForeignKey(m => m.DepartmentId).OnDelete(DeleteBehavior.Restrict));

        modelBuilder.Entity<Scale>(entity =>
            entity.HasOne(s => s.Department).WithMany()
                  .HasForeignKey(s => s.DepartmentId).OnDelete(DeleteBehavior.Restrict));

        modelBuilder.Entity<AnnualPlan>(entity =>
        {
            entity.HasOne(p => p.Department).WithMany()
                  .HasForeignKey(p => p.DepartmentId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(p => p.MaterialType).WithMany()
                  .HasForeignKey(p => p.MaterialTypeId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<AnnualPlanDetail>(entity =>
            entity.HasOne(p => p.Material).WithMany()
                  .HasForeignKey(p => p.MaterialId).OnDelete(DeleteBehavior.Restrict));

        modelBuilder.Entity<Product>(entity =>
            entity.HasOne(m => m.Category).WithMany()
                  .HasForeignKey(m => m.CategoryId).OnDelete(DeleteBehavior.Restrict));

        modelBuilder.HasSequence<int>("worker_number_seq").StartsAt(1).IncrementsBy(1);
        modelBuilder.Entity<Worker>(entity =>
            entity.Property(w => w.WorkerNumber)
                  .HasDefaultValueSql("NEXT VALUE FOR worker_number_seq")
                  .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore));

        modelBuilder.Entity<ScaleEqualization>(entity =>
        {
            entity.HasOne(e => e.Scale).WithMany()
                  .HasForeignKey(e => e.ScaleId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Worker).WithMany()
                  .HasForeignKey(e => e.WorkerId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<CleaningRequest>(entity =>
        {
            entity.ToTable("cleaning_requests", t => t.HasCheckConstraint(
                "ck_cleaning_requests_target",
                "([cleaning_type] = 1 AND [TargetDepartmentId] IS NOT NULL AND [ScaleId] IS NULL AND [MachineId] IS NULL AND [ClotheId] IS NULL) OR " +
                "([cleaning_type] = 2 AND [ScaleId] IS NOT NULL AND [TargetDepartmentId] IS NULL AND [MachineId] IS NULL AND [ClotheId] IS NULL) OR " +
                "([cleaning_type] = 3 AND [MachineId] IS NOT NULL AND [TargetDepartmentId] IS NULL AND [ScaleId] IS NULL AND [ClotheId] IS NULL) OR " +
                "([cleaning_type] = 4 AND [ClotheId] IS NOT NULL AND [TargetDepartmentId] IS NULL AND [ScaleId] IS NULL AND [MachineId] IS NULL)"));

            entity.HasOne(r => r.Department).WithMany()
                  .HasForeignKey(r => r.DepartmentId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(r => r.Protocol).WithMany()
                  .HasForeignKey(r => r.ProtocolId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(r => r.TargetDepartment).WithMany()
                  .HasForeignKey(r => r.TargetDepartmentId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(r => r.Scale).WithMany()
                  .HasForeignKey(r => r.ScaleId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(r => r.Machine).WithMany()
                  .HasForeignKey(r => r.MachineId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(r => r.Clothe).WithMany()
                  .HasForeignKey(r => r.ClotheId).OnDelete(DeleteBehavior.Restrict);
        });

        // ============================================================
        // purchasing — material no longer stored on the contract review
        // ============================================================

        modelBuilder.Entity<ContractReview>(entity =>
        {
            entity.HasOne(c => c.Supplier).WithMany()
                  .HasForeignKey(c => c.SupplierId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(c => c.RFQ).WithMany()
                  .HasForeignKey(c => c.RFQId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<RFQ>(entity =>
        {
            entity.HasOne(r => r.AnnualPlan).WithMany()
                  .HasForeignKey(r => r.AnnualPlanID).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(r => r.Material).WithMany()
                  .HasForeignKey(r => r.MaterialId).OnDelete(DeleteBehavior.Restrict);
        });

        // ============================================================
        // samples (one table for supplier + delivery) / certificates
        // ============================================================

        modelBuilder.Entity<Sample>(entity =>
        {
            entity.ToTable("samples", t => t.HasCheckConstraint("ck_samples_link",
                "([sample_type] = 1 AND [ContractReviewId] IS NOT NULL AND [UomId] IS NULL) OR " +
                "([sample_type] = 2 AND [UomId] IS NOT NULL AND [ContractReviewId] IS NULL)"));

            entity.HasOne(s => s.ContractReview).WithMany()
                  .HasForeignKey(s => s.ContractReviewId).OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(s => s.Uom).WithMany(u => u.Samples)
                  .HasForeignKey(s => s.UomId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<CertificateOfAnalysis>(entity =>
        {
            entity.ToTable("certificates_of_analysis", t =>
            {
                t.HasCheckConstraint("ck_certificates_of_analysis_moisture",
                    "[moisture_percentage] >= 0 AND [moisture_percentage] <= 100");

                // Pending=0/Accepted=1 have no reason; Rejected=2 requires one.
                t.HasCheckConstraint("ck_certificates_of_analysis_rejection",
                    "([result] = 0 AND [rejection_reason] IS NULL) OR " +
                    "([result] = 1 AND [rejection_reason] IS NULL) OR " +
                    "([result] = 2 AND [rejection_reason] IS NOT NULL)");
            });

            entity.HasOne(c => c.Sample).WithMany()
                  .HasForeignKey(c => c.SampleId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PurchaseOrder>(entity =>
        {
            entity.HasOne(p => p.ContractReview).WithMany()
                  .HasForeignKey(p => p.ContractReviewId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(p => p.CertificateOfAnalysis).WithMany()
                  .HasForeignKey(p => p.CertificateOfAnalysisId).OnDelete(DeleteBehavior.Restrict);
        });

        // ============================================================
        // legacy memo-in / work order — kept, configured exactly as before
        // ============================================================

        modelBuilder.Entity<WorkOrder>(entity =>
        {
            entity.HasMany(w => w.Details).WithOne(d => d.WorkOrder)
                  .HasForeignKey(d => d.WorkOrderId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<WorkOrderDetail>(entity =>
        {
            entity.HasOne(d => d.Material).WithMany()
                  .HasForeignKey(d => d.MaterialId).OnDelete(DeleteBehavior.Restrict);
            entity.HasMany(d => d.UOMs).WithOne(u => u.WorkOrderDetail)
                  .HasForeignKey(u => u.WorkOrderDetailId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<WorkOrderDetailUOM>(entity =>
            entity.HasOne(u => u.Uom).WithMany()
                  .HasForeignKey(u => u.UOMId).OnDelete(DeleteBehavior.Restrict));

        modelBuilder.Entity<Memo>(entity =>
        {
            entity.HasOne(m => m.Supplier).WithMany()
                  .HasForeignKey(m => m.SupplierId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(m => m.WorkOrder).WithMany()
                  .HasForeignKey(m => m.WorkOrderId).OnDelete(DeleteBehavior.Restrict);
            entity.HasMany(m => m.Details).WithOne(d => d.Memo)
                  .HasForeignKey(d => d.MemoId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<MemoDetail>(entity =>
        {
            entity.HasOne(d => d.Material).WithMany()
                  .HasForeignKey(d => d.MaterialId).OnDelete(DeleteBehavior.Restrict);
            entity.HasMany(d => d.UOMs).WithOne(u => u.MemoDetail)
                  .HasForeignKey(u => u.MemoDetailId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<MemoDetailUOM>(entity =>
            entity.HasOne(u => u.Uom).WithMany()
                  .HasForeignKey(u => u.UOMId).OnDelete(DeleteBehavior.Restrict));

        // ============================================================
        // receiving (memo RW in)
        // ============================================================

        modelBuilder.HasSequence<long>("memo_rw_in_number_seq").StartsAt(1).IncrementsBy(1);

        modelBuilder.Entity<MemoRWIn>(entity =>
        {
            entity.Property(m => m.Number)
                  .HasDefaultValueSql("NEXT VALUE FOR memo_rw_in_number_seq")
                  .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);

            entity.HasOne(m => m.PurchaseOrder).WithMany()
                  .HasForeignKey(m => m.PurchaseOrderId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(m => m.Worker).WithMany()
                  .HasForeignKey(m => m.WorkerId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<MemoRWDetail>(entity =>
        {
            entity.ToTable("memo_rw_details", t =>
                t.HasCheckConstraint("ck_memo_rw_details_tear", "[tear] >= 0"));

            entity.HasOne(d => d.Memo).WithMany(m => m.Details)
                  .HasForeignKey(d => d.MemoId).OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.Uom).WithOne(u => u.MemoRWDetail)
                  .HasForeignKey<MemoRWDetail>(d => d.UomId).OnDelete(DeleteBehavior.Restrict);
        });

        // ============================================================
        // inventory master: inventory / location / shelf / flow
        // ============================================================

        modelBuilder.Entity<Inventory>().HasData(
            new Inventory { Id = InventoryIds.Receiving, Code = InventoryCodes.Receiving, NameEn = "Receiving", NameAr = "الاستلام", SortOrder = 1, IsActive = true },
            new Inventory { Id = InventoryIds.Preparing, Code = InventoryCodes.Preparing, NameEn = "Preparing", NameAr = "التحضير", SortOrder = 2, IsActive = true },
            new Inventory { Id = InventoryIds.Filling, Code = InventoryCodes.Filling, NameEn = "Filling", NameAr = "التعبئة", SortOrder = 3, IsActive = true },
            new Inventory { Id = InventoryIds.ProductsProduced, Code = InventoryCodes.ProductsProduced, NameEn = "Products produced", NameAr = "المنتجات المنتجة", SortOrder = 4, IsActive = true },
            new Inventory { Id = InventoryIds.ProductsReadyToSell, Code = InventoryCodes.ProductsReadyToSell, NameEn = "Products ready to sell", NameAr = "المنتجات الجاهزة", SortOrder = 5, IsActive = true });

        modelBuilder.Entity<Location>(entity =>
        {
            entity.ToTable("locations", t => t.HasCheckConstraint("ck_locations_inventory_zone",
    "([kind] = 1 AND [InventoryId] IS NOT NULL AND [zone] IS NOT NULL) OR " +
    "([kind] = 2 AND [InventoryId] IS NULL AND [zone] IS NULL) OR " +
    "([kind] = 3 AND [InventoryId] IS NULL AND [zone] IS NULL)"));

            entity.HasOne(l => l.Inventory).WithMany(i => i.Locations)
                  .HasForeignKey(l => l.InventoryId).OnDelete(DeleteBehavior.Restrict);

            entity.HasData(LocationSeeds.All);
        });

        modelBuilder.Entity<Shelf>(entity =>
        {
            // Principal key so UOM(ShelfId, LocationId) can FK to Shelf(Id, LocationId).
            entity.HasAlternateKey(s => new { s.Id, s.LocationId })
                  .HasName("ak_shelves_id_location");

            entity.HasOne(s => s.Location).WithMany(l => l.Shelves)
                  .HasForeignKey(s => s.LocationId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Flow>(entity =>
        {
            entity.HasMany(f => f.Steps).WithOne(s => s.Flow)
                  .HasForeignKey(s => s.FlowId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<FlowStep>(entity =>
        {
            entity.HasOne(s => s.Location).WithMany()
                  .HasForeignKey(s => s.LocationId).OnDelete(DeleteBehavior.Restrict);
        });

        // ============================================================
        // UOM — single table, Kind, shelf, split parent
        // ============================================================

        modelBuilder.HasSequence<long>("uom_material_seq").StartsAt(1).IncrementsBy(1);
        modelBuilder.HasSequence<long>("uom_product_seq").StartsAt(1).IncrementsBy(1);

        modelBuilder.Entity<UOM>(entity =>
        {
            entity.ToTable("uoms", t =>
            {
                t.HasCheckConstraint("ck_uoms_quantity", "[quantity] >= 0");

                // Kind Material(1) -> MaterialId set, ProductId null
                // Kind Product(2)  -> ProductId set,  MaterialId null
                t.HasCheckConstraint("ck_uoms_kind_link",
                    "([kind] = 1 AND [MaterialId] IS NOT NULL AND [ProductId] IS NULL) OR " +
                    "([kind] = 2 AND [ProductId] IS NOT NULL AND [MaterialId] IS NULL)");
            });

            entity.HasOne(u => u.Material).WithMany()
                  .HasForeignKey(u => u.MaterialId).OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(u => u.Product).WithMany()
                  .HasForeignKey(u => u.ProductId).OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(u => u.Location).WithMany()
                  .HasForeignKey(u => u.LocationId).OnDelete(DeleteBehavior.Restrict);

            // Composite FK: the shelf a box sits on must belong to the box's own location.
            entity.HasOne(u => u.Shelf).WithMany()
                  .HasForeignKey(u => new { u.ShelfId, u.LocationId })
                  .HasPrincipalKey(s => new { s.Id, s.LocationId })
                  .OnDelete(DeleteBehavior.Restrict);

            // Self reference for split children — Restrict, SQL Server requires it.
            entity.HasOne(u => u.Parent).WithMany(u => u.Children)
                  .HasForeignKey(u => u.ParentUomId).OnDelete(DeleteBehavior.Restrict);
        });

        // ============================================================
        // stock moves
        // ============================================================

        modelBuilder.Entity<StockMove>(entity =>
        {
            entity.ToTable("stock_moves", t =>
                t.HasCheckConstraint("ck_stock_moves_quantity", "[quantity] > 0"));

            entity.HasOne(m => m.Uom).WithMany()
                  .HasForeignKey(m => m.UomId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(m => m.FromLocation).WithMany()
                  .HasForeignKey(m => m.FromLocationId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(m => m.ToLocation).WithMany()
                  .HasForeignKey(m => m.ToLocationId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(m => m.FromShelf).WithMany()
                  .HasForeignKey(m => m.FromShelfId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(m => m.ToShelf).WithMany()
                  .HasForeignKey(m => m.ToShelfId).OnDelete(DeleteBehavior.Restrict);
        });

        // ============================================================
        // transfer engine + business orders
        // ============================================================

        modelBuilder.Entity<TransferOrder>(entity =>
        {
            entity.ToTable("transfer_orders", t =>
            {
                t.HasCheckConstraint("ck_transfer_orders_locations", "[FromLocationId] <> [ToLocationId]");
            });

            entity.HasOne(o => o.FromLocation).WithMany()
                  .HasForeignKey(o => o.FromLocationId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(o => o.ToLocation).WithMany()
                  .HasForeignKey(o => o.ToLocationId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(o => o.Material).WithMany()
                  .HasForeignKey(o => o.MaterialId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<Notification>().ToTable("notifications");

        modelBuilder.Entity<TransferOrderLine>(entity =>
        {
            entity.ToTable("transfer_order_lines", t =>
                t.HasCheckConstraint("ck_transfer_order_lines_quantity", "[quantity] > 0"));

            entity.HasOne(l => l.TransferOrder).WithMany(o => o.Lines)
                  .HasForeignKey(l => l.TransferOrderId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(l => l.Uom).WithMany()
                  .HasForeignKey(l => l.UomId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PreparingOrder>(entity =>
        {
            entity.HasOne(p => p.Protocol).WithMany()
                  .HasForeignKey(p => p.ProtocolId).OnDelete(DeleteBehavior.Restrict);

            // The order owns the transfer now: one transfer per preparing order,
            // enforced by a unique index on the FK.
            entity.HasOne(p => p.TransferOrder).WithOne()
                  .HasForeignKey<PreparingOrder>(p => p.TransferOrderId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(p => p.TransferOrderId)
                  .IsUnique()
                  .HasFilter("[TransferOrderId] IS NOT NULL")
                  .HasDatabaseName("ix_preparing_orders_transfer_order");
        });
        modelBuilder.Entity<PreparingOrderTransferLink>(entity =>
        {
            entity.HasOne(l => l.PreparingOrder).WithMany()
                  .HasForeignKey(l => l.PreparingOrderId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(l => l.TransferOrder).WithMany()
                  .HasForeignKey(l => l.TransferOrderId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<WashingReport>(entity =>
        {
            entity.HasOne(r => r.PreparingOrder).WithMany()
                  .HasForeignKey(r => r.PreparingOrderId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(r => r.Uom).WithMany()
                  .HasForeignKey(r => r.UomId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(r => r.StockMove).WithMany()
                  .HasForeignKey(r => r.StockMoveId).OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(r => r.PreparingOrderId).HasDatabaseName("ix_washing_reports_prep_order");
            entity.HasIndex(r => r.UomId).HasDatabaseName("ix_washing_reports_uom");
        });
        modelBuilder.Entity<DryingReport>(entity =>
        {
            entity.HasOne(r => r.PreparingOrder).WithMany()
                  .HasForeignKey(r => r.PreparingOrderId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(r => r.Uom).WithMany()
                  .HasForeignKey(r => r.UomId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(r => r.StockMove).WithMany()
                  .HasForeignKey(r => r.StockMoveId).OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(r => r.PreparingOrderId).HasDatabaseName("ix_drying_reports_prep_order");
            entity.HasIndex(r => r.UomId).HasDatabaseName("ix_drying_reports_uom");
        });
        modelBuilder.Entity<PurificationReport>(entity =>
        {
            entity.HasOne(r => r.PreparingOrder).WithMany()
                  .HasForeignKey(r => r.PreparingOrderId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(r => r.Uom).WithMany()
                  .HasForeignKey(r => r.UomId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(r => r.StockMove).WithMany()
                  .HasForeignKey(r => r.StockMoveId).OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(r => r.PreparingOrderId).HasDatabaseName("ix_purification_reports_prep_order");
            entity.HasIndex(r => r.UomId).HasDatabaseName("ix_purification_reports_uom");
        });
        modelBuilder.Entity<SterilizationReport>(entity =>
        {
            entity.HasOne(r => r.PreparingOrder).WithMany()
                  .HasForeignKey(r => r.PreparingOrderId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(r => r.Uom).WithMany()
                  .HasForeignKey(r => r.UomId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(r => r.StockMove).WithMany()
                  .HasForeignKey(r => r.StockMoveId).OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(r => r.PreparingOrderId).HasDatabaseName("ix_sterilization_reports_prep_order");
            entity.HasIndex(r => r.UomId).HasDatabaseName("ix_sterilization_reports_uom");
        });
        modelBuilder.Entity<ReleaseOrder>(entity =>
        {
            entity.HasOne(r => r.TransferOrder).WithOne()
                  .HasForeignKey<ReleaseOrder>(r => r.TransferOrderId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(r => r.TransferOrderId)
                  .IsUnique()
                  .HasFilter("[TransferOrderId] IS NOT NULL")
                  .HasDatabaseName("ix_release_orders_transfer_order");
        });
        modelBuilder.Entity<ScrapOrder>(entity =>
        {
            entity.HasOne(o => o.TransferOrder).WithOne()
                  .HasForeignKey<ScrapOrder>(o => o.TransferOrderId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(o => o.TransferOrderId)
                  .IsUnique()
                  .HasFilter("[TransferOrderId] IS NOT NULL")
                  .HasDatabaseName("ix_scrap_orders_transfer_order");
        });

        modelBuilder.Entity<SupplierReturnOrder>(entity =>
        {
            entity.HasOne(o => o.Supplier).WithMany()
                  .HasForeignKey(o => o.SupplierId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(o => o.TransferOrder).WithOne()
                  .HasForeignKey<SupplierReturnOrder>(o => o.TransferOrderId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(o => o.TransferOrderId)
                  .IsUnique()
                  .HasFilter("[TransferOrderId] IS NOT NULL")
                  .HasDatabaseName("ix_supplier_return_orders_transfer_order");
        });
        modelBuilder.Entity<Flow>().HasData(
    new Flow { Id = 1, NameEn = "Organic", NameAr = "عضوي", IsActive = true },
    new Flow { Id = 2, NameEn = "Chemical", NameAr = "كيميائي", IsActive = true },
    new Flow { Id = 3, NameEn = "Filling", NameAr = "تعبئة", IsActive = true });

        modelBuilder.Entity<FlowStep>().HasData(
            // Organic: Receiving -> Preparing -> Products produced -> Ready to sell
            new FlowStep { Id = 1, FlowId = 1, LocationId = LocationIds.ReceivingQuarantine, OrderIndex = 1 },
            new FlowStep { Id = 2, FlowId = 1, LocationId = LocationIds.PreparingQuarantine, OrderIndex = 2 },
            new FlowStep { Id = 3, FlowId = 1, LocationId = LocationIds.ProductsProducedQuarantine, OrderIndex = 3 },
            new FlowStep { Id = 4, FlowId = 1, LocationId = LocationIds.ProductsReadyToSellQuarantine, OrderIndex = 4 },

            // Chemical: Preparing -> Products produced -> Ready to sell
            new FlowStep { Id = 5, FlowId = 2, LocationId = LocationIds.PreparingQuarantine, OrderIndex = 1 },
            new FlowStep { Id = 6, FlowId = 2, LocationId = LocationIds.ProductsProducedQuarantine, OrderIndex = 2 },
            new FlowStep { Id = 7, FlowId = 2, LocationId = LocationIds.ProductsReadyToSellQuarantine, OrderIndex = 3 },

            // Filling: Filling -> Products produced -> Ready to sell
            new FlowStep { Id = 8, FlowId = 3, LocationId = LocationIds.FillingQuarantine, OrderIndex = 1 },
            new FlowStep { Id = 9, FlowId = 3, LocationId = LocationIds.ProductsProducedQuarantine, OrderIndex = 2 },
            new FlowStep { Id = 10, FlowId = 3, LocationId = LocationIds.ProductsReadyToSellQuarantine, OrderIndex = 3 });
    }

}
// Seed rows for locations, built from the stable ids in LocationIds.
public static class LocationSeeds
{
    public static readonly Location[] All =
    {
        Zone(LocationIds.ReceivingQuarantine,  InventoryIds.Receiving,           InventoryZones.Quarantine, InventoryCodes.Receiving,           "Receiving quarantine",            "حجر الاستلام",            1),
        Zone(LocationIds.ReceivingAccepted,    InventoryIds.Receiving,           InventoryZones.Accepted,   InventoryCodes.Receiving,           "Receiving accepted",              "مقبول الاستلام",          2),
        Zone(LocationIds.ReceivingRejected,    InventoryIds.Receiving,           InventoryZones.Rejected,   InventoryCodes.Receiving,           "Receiving rejected",              "مرفوض الاستلام",          3),
        Zone(LocationIds.PreparingQuarantine,  InventoryIds.Preparing,           InventoryZones.Quarantine, InventoryCodes.Preparing,           "Preparing quarantine",            "حجر التحضير",             4),
        Zone(LocationIds.PreparingAccepted,    InventoryIds.Preparing,           InventoryZones.Accepted,   InventoryCodes.Preparing,           "Preparing accepted",              "مقبول التحضير",           5),
        Zone(LocationIds.PreparingRejected,    InventoryIds.Preparing,           InventoryZones.Rejected,   InventoryCodes.Preparing,           "Preparing rejected",              "مرفوض التحضير",           6),
        Zone(LocationIds.FillingQuarantine,    InventoryIds.Filling,             InventoryZones.Quarantine, InventoryCodes.Filling,             "Filling quarantine",              "حجر التعبئة",             7),
        Zone(LocationIds.FillingAccepted,      InventoryIds.Filling,             InventoryZones.Accepted,   InventoryCodes.Filling,             "Filling accepted",                "مقبول التعبئة",           8),
        Zone(LocationIds.FillingRejected,      InventoryIds.Filling,             InventoryZones.Rejected,   InventoryCodes.Filling,             "Filling rejected",                "مرفوض التعبئة",           9),
        Zone(LocationIds.ProductsProducedQuarantine, InventoryIds.ProductsProduced, InventoryZones.Quarantine, InventoryCodes.ProductsProduced, "Products produced quarantine", "حجر المنتجات المنتجة", 10),
        Zone(LocationIds.ProductsProducedAccepted,   InventoryIds.ProductsProduced, InventoryZones.Accepted,   InventoryCodes.ProductsProduced, "Products produced accepted",   "مقبول المنتجات المنتجة", 11),
        Zone(LocationIds.ProductsProducedRejected,   InventoryIds.ProductsProduced, InventoryZones.Rejected,   InventoryCodes.ProductsProduced, "Products produced rejected",   "مرفوض المنتجات المنتجة", 12),
        Zone(LocationIds.ProductsReadyToSellQuarantine, InventoryIds.ProductsReadyToSell, InventoryZones.Quarantine, InventoryCodes.ProductsReadyToSell, "Ready to sell quarantine", "حجر الجاهز", 13),
        Zone(LocationIds.ProductsReadyToSellAccepted,   InventoryIds.ProductsReadyToSell, InventoryZones.Accepted,   InventoryCodes.ProductsReadyToSell, "Ready to sell accepted",   "مقبول الجاهز", 14),
        Zone(LocationIds.ProductsReadyToSellRejected,   InventoryIds.ProductsReadyToSell, InventoryZones.Rejected,   InventoryCodes.ProductsReadyToSell, "Ready to sell rejected",   "مرفوض الجاهز", 15),

        Virtual(LocationIds.Consumed,        "VIRT-CONSUMED", "Consumed",         "مستهلك",            16),
        Virtual(LocationIds.Scrap,           "VIRT-SCRAP",    "Scrap",            "تالف",              17),
        Virtual(LocationIds.SupplierReturns, "VIRT-SUPPLIER", "Supplier returns", "مرتجع للمورد",      18),
        Virtual(LocationIds.ProductionFloor, "VIRT-PROD",     "Production floor", "أرضية الإنتاج",     19),
        Transient(LocationIds.VirtualWashing,       "V-WASH",  "Washing",       "الغسل",        11),
        Transient(LocationIds.VirtualPurification,  "V-PURIF", "Purification",  "التنقية",      12),
        Transient(LocationIds.VirtualSterilization, "V-STER",  "Sterilization", "التعقيم",      13),
        Transient(LocationIds.VirtualDrying,        "V-DRY",   "Drying",        "التجفيف",      14),
    };

    private static Location Zone(int id, int inv, InventoryZones zone, string invCode,
                                 string en, string ar, int order) => new()
                                 {
                                     Id = id,
                                     Code = $"{invCode}-{zone switch { InventoryZones.Quarantine => "QUAR", InventoryZones.Accepted => "ACC", _ => "REJ" }}",
                                     NameEn = en,
                                     NameAr = ar,
                                     Kind = LocationKinds.Inventory,
                                     InventoryId = inv,
                                     Zone = zone,
                                     SortOrder = order,
                                     IsActive = true
                                 };

    private static Location Virtual(int id, string code, string en, string ar, int order) => new()
    {
        Id = id,
        Code = code,
        NameEn = en,
        NameAr = ar,
        Kind = LocationKinds.Virtual,
        InventoryId = null,
        Zone = null,
        SortOrder = order,
        IsActive = true
    };
    private static Location Transient(int id, string code, string en, string ar, int order) => new()
    {
        Id = id,
        Code = code,
        NameEn = en,
        NameAr = ar,
        Kind = LocationKinds.Transient,
        InventoryId = null,
        Zone = null,
        SortOrder = order,
        IsActive = true
    };
}