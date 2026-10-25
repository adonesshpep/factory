using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using neotonic.Models;

public class AppDbContext : IdentityDbContext<AppUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<AuditLog> AuditLogs { get; set; }
    public DbSet<AuditLogRole> AuditLogRoles { get; set; }
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
    public DbSet<AnnualPlan> AnnualPlans { get; set; }
    public DbSet<AnnualPlanDetail> AnnualPlanDetails { get; set; }
    public DbSet<Supplier> Suppliers { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Worker> Workers { get; set; }
    public DbSet<ScaleEqualization> ScaleEqualizations { get; set; }
    public DbSet<Clothe> Clothes { get; set; }
    public DbSet<CleaningRequest> CleaningRequests { get; set; }
    public DbSet<ContractReview> ContractReviews { get; set; }
    public DbSet<SupplierSample> SupplierSamples { get; set; }
    public DbSet<CertificateOfAnalysis> CertificatesOfAnalysis { get; set; }
    public DbSet<PurchaseOrder> PurchaseOrders { get; set; }
    public DbSet<Memo> Memos { get; set; }
    public DbSet<MemoDetail> MemoDetails { get; set; }
    public DbSet<MemoDetailUOM> MemoDetailUOMs { get; set; }
    public DbSet<UOM> UOMs { get; set; }
    public DbSet<WorkOrder> WorkOrders { get; set; }
    public DbSet<WorkOrderDetail> WorkOrderDetails { get; set; }
    public DbSet<WorkOrderDetailUOM> WorkOrderDetailUOMs { get; set; }

    public DbSet<RFQ> RFQs { get; set; }
    public DbSet<MemoRWIn> MemoRWIns { get; set; }
    public DbSet<MemoRWDetail> MemoRWDetails { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ---- Restrict delete behaviors: no Data Annotation equivalent exists for this ----

        modelBuilder.Entity<Material>(entity =>
        {
            entity.ToTable("materials", t =>
                t.HasCheckConstraint("ck_materials_min_max", "[min] >= 0 AND [max] >= [min]"));

            entity.HasOne(m => m.Category).WithMany()
                  .HasForeignKey(m => m.CategoryId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(m => m.MaterialType).WithMany()
                  .HasForeignKey(m => m.MaterialTypeId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Step>(entity =>
        {
            entity.HasOne(s => s.StepType).WithMany()
                  .HasForeignKey(s => s.StepTypeId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(s => s.BoxPath).WithMany()
                  .HasForeignKey(s => s.BoxPathId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Protocol>(entity =>
        {
            entity.HasOne(p => p.StepType).WithMany()
                  .HasForeignKey(p => p.StepTypeId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ProtocolStep>(entity =>
        {
            // Protocol -> ProtocolStep stays Cascade (EF's default for a required
            // relationship), so only the Step side needs explicit config here.
            entity.HasOne(ps => ps.Step)
                  .WithMany()
                  .HasForeignKey(ps => ps.StepId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Machine>(entity =>
        {
            entity.HasOne(m => m.Department).WithMany()
                  .HasForeignKey(m => m.DepartmentId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Scale>(entity =>
        {
            entity.HasOne(s => s.Department).WithMany()
                  .HasForeignKey(s => s.DepartmentId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<AnnualPlan>(entity =>
        {
            entity.HasOne(p => p.Department).WithMany()
                  .HasForeignKey(p => p.DepartmentId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(p => p.MaterialType).WithMany()
                  .HasForeignKey(p => p.MaterialTypeId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<AnnualPlanDetail>(entity =>
        {
            // AnnualPlanDetail -> AnnualPlan stays Cascade (default), only
            // the Material side needs Restrict.
            entity.HasOne(p => p.Material).WithMany()
                  .HasForeignKey(p => p.MaterialId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<Product>(entity =>
        {
            {
                entity.HasOne(m => m.Category).WithMany()
                .HasForeignKey(m => m.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
            }
        });
        modelBuilder.HasSequence<int>("worker_number_seq")
            .StartsAt(1)
            .IncrementsBy(1);

        modelBuilder.Entity<Worker>(entity =>
        {
            entity.Property(w => w.WorkerNumber)
                  .HasDefaultValueSql("NEXT VALUE FOR worker_number_seq")
                  .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore); 
        });
        modelBuilder.Entity<ScaleEqualization>(entity =>
        {
            entity.HasOne(e => e.Scale).WithMany()
                  .HasForeignKey(e => e.ScaleId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Worker).WithMany()
                  .HasForeignKey(e => e.WorkerId)
                  .OnDelete(DeleteBehavior.Restrict);
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
        modelBuilder.Entity<ContractReview>(entity =>
        {
            entity.HasOne(c => c.Supplier).WithMany()
                  .HasForeignKey(c => c.SupplierId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(c => c.Material).WithMany()
                  .HasForeignKey(c => c.MaterialId)
                  .OnDelete(DeleteBehavior.Restrict);

            // NEW: CR now hangs off an RFQ, which hangs off an AnnualPlan.
            entity.HasOne(c => c.RFQ).WithMany()
                  .HasForeignKey(c => c.RFQId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<SupplierSample>(entity =>
        {
            //entity.HasOne(s => s.Supplier).WithMany()
            //      .HasForeignKey(s => s.SupplierId)
            //      .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(s => s.ContractReview).WithMany()
                  .HasForeignKey(s => s.ContractReviewId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<CertificateOfAnalysis>(entity =>
        {
            entity.ToTable("certificates_of_analysis", t =>
            {
                t.HasCheckConstraint("ck_certificates_of_analysis_moisture",
                    "[moisture_percentage] >= 0 AND [moisture_percentage] <= 100");

                // 1 = Accepted, 2 = Rejected (values of AnalysisResults, don't renumber)
                t.HasCheckConstraint("ck_certificates_of_analysis_rejection",
                    "([result] = 1 AND [rejection_reason] IS NULL) OR ([result] = 2 AND [rejection_reason] IS NOT NULL)");
            });

            entity.HasOne(c => c.SupplierSample).WithMany()
                  .HasForeignKey(c => c.SupplierSampleId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<PurchaseOrder>(entity =>
        {
            entity.HasOne(p => p.ContractReview).WithMany()
                  .HasForeignKey(p => p.ContractReviewId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(p => p.CertificateOfAnalysis).WithMany()
                  .HasForeignKey(p => p.CertificateOfAnalysisId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<WorkOrder>(entity =>
        {
            entity.HasMany(w => w.Details).WithOne(d => d.WorkOrder)
                  .HasForeignKey(d => d.WorkOrderId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<WorkOrderDetail>(entity =>
        {
            entity.HasOne(d => d.Material).WithMany()
                  .HasForeignKey(d => d.MaterialId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(d => d.UOMs).WithOne(u => u.WorkOrderDetail)
                  .HasForeignKey(u => u.WorkOrderDetailId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<WorkOrderDetailUOM>(entity =>
        {
            entity.HasOne(u => u.Uom).WithMany()
                  .HasForeignKey(u => u.UOMId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<UOM>(entity =>
        {
            entity.HasOne(u => u.Material).WithMany()
                  .HasForeignKey(u => u.MaterialId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<Memo>(entity =>
        {
            entity.HasOne(m => m.Supplier).WithMany()
                  .HasForeignKey(m => m.SupplierId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(m => m.WorkOrder).WithMany()
                  .HasForeignKey(m => m.WorkOrderId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(m => m.Details).WithOne(d => d.Memo)
                  .HasForeignKey(d => d.MemoId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<MemoDetail>(entity =>
        {
            entity.HasOne(d => d.Material).WithMany()
                  .HasForeignKey(d => d.MaterialId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(d => d.UOMs).WithOne(u => u.MemoDetail)
                  .HasForeignKey(u => u.MemoDetailId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<MemoDetailUOM>(entity =>
        {
            entity.HasOne(u => u.Uom).WithMany()
                  .HasForeignKey(u => u.UOMId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<RFQ>(entity =>
        {
            entity.HasOne(r => r.AnnualPlan).WithMany()
                  .HasForeignKey(r => r.AnnualPlanID)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(r => r.Material).WithMany()
                  .HasForeignKey(r => r.MaterialId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.HasSequence<long>("memo_rw_in_number_seq")
    .StartsAt(1)
    .IncrementsBy(1);

        modelBuilder.Entity<MemoRWIn>(entity =>
        {
            entity.Property(m => m.Number)
                  .HasDefaultValueSql("NEXT VALUE FOR memo_rw_in_number_seq")
                  .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);

            entity.HasOne(m => m.PurchaseOrder).WithMany()
                  .HasForeignKey(m => m.PurchaseOrderId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(m => m.Worker).WithMany()
                  .HasForeignKey(m => m.WorkerId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<MemoRWDetail>(entity =>
        {
            // MemoRWDetail -> MemoRWIn stays Cascade (default for a required relationship)
            entity.HasOne(d => d.Memo).WithMany(m => m.Details)
                  .HasForeignKey(d => d.MemoId)
                  .OnDelete(DeleteBehavior.Cascade);
        });


    }
}