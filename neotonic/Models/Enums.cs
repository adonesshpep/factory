// Models/Enums.cs  (CHANGED)
// Added: AnalysisResults.Pending = 0, InventoryZones, UOMKinds, LocationKinds.
// Everything else untouched.
using System.ComponentModel;

namespace neotonic.Models
{
    public enum RFQStatus
    {
        [Description("جديد|New")] New = 1,
        [Description("القسم الفني|technical")] Technical = 2,
        [Description("مدير المنشأة|Establishment Manager")] Establishment_Manager = 3,
        [Description("منتهي|Finished")] Finished = 4
    }
    public enum CRStatus
    {
        [Description("جديد|New")] New = 1,
        [Description("القسم الفني|technical")] Technical = 2,
        [Description("مدير المنشأة|Establishment Manager")] Establishment_Manager = 3,
        [Description("منتهي|Finished")] Finished = 4
    }
    public enum AnnualStatuses
    {
        [Description("جديد|New")] New = 1,
        [Description("القسم الفني|technical")] Technical = 2,
        [Description("مدير المنشأة|Establishment Manager")] Establishment_Manager = 3,
        [Description("منتهي|Finished")] Finished = 4
    }
    public enum CleaningTypes
    {
        [Description("قسم|Department")] Department = 1,
        [Description("ميزان|Scale")] Scale = 2,
        [Description("الة|Machine")] Machine = 3,
        [Description("ملابس|Clothe")] Clothe = 4
    }
    public enum ConnectionMethods
    {
        [Description("مباشر|Direct")] Direct = 1,
        [Description("ايميل|Email")] Email = 2,
        [Description("هاتف|Phone")] Phone = 3,
        [Description("صديق|Friend")] Friend = 4
    }
    public enum AnalysisResults
    {
        // 0 = not yet decided. The result is only ever set via the admin result route.
        [Description("معلق|Pending")] Pending = 0,
        [Description("مقبول|Accepted")] Accepted = 1,
        [Description("مرفوض|Rejected")] Rejected = 2
    }
    public enum POStatus
    {
        [Description("معلق|Pending")] Pending = 0,
        [Description("معتمد|Approved")] Approved = 1,
        [Description("ملغي|Cancelled")] Cancelled = 2
    }
    public enum MemoType
    {
        [Description("صادر|Out")] Out = -1,
        [Description("وارد|In")] In = 1
    }
    public enum WorkStatus
    {
        [Description("معلق|Pending")] Pending = 0,
        [Description("قيد التنفيذ|In Progress")] In_Progress = 1,
        [Description("مكتمل|Completed")] Completed = 2,
        [Description("ملغي|Cancelled")] Cancelled = 3
    }
    public enum TransferOrderStatuses
    {
        [Description("جديد|New")] New = 0,
        [Description("تم|Done")] Done = 1,
        [Description("منفذ|Executed")] Executed = 2,
        [Description("ملغي|Cancelled")] Cancelled = 3
    }
    public enum StockMoveTypes
    {
        [Description("نقل|Move")] Move = 1,
        [Description("تقسيم|Split")] Split = 2,
        [Description("نقل يدوي|Manual")] Manual = 3
    }
    public enum StockMoveSources
    {
        [Description("بدون|None")] None = 0,
        [Description("أمر نقل|Transfer order")] TransferOrder = 1,
        [Description("شهادة تحليل|Certificate of analysis")] CertificateOfAnalysis = 2,
        [Description("خطوة تحضير|Preparing step")] PreparingStep = 3
    }
    public enum SampleTypes
    {
        [Description("مورد|Supplier")] Supplier = 1,
        [Description("استلام|Delivery")] Delivery = 2,
        [Description("تحضير|Preparing")] Preparing = 3
    }

    // ---- inventory ----

    // The three zones every real inventory has. No zone for virtual locations.
    public enum InventoryZones
    {
        [Description("حجر|Quarantine")] Quarantine = 1,
        [Description("مقبول|Accepted")] Accepted = 2,
        [Description("مرفوض|Rejected")] Rejected = 3
    }

    // Real location with zones + shelves, or a virtual sink/source.
    public enum LocationKinds
    {
        [Description("مخزون|Inventory")] Inventory = 1,
        [Description("وهمي|Virtual")] Virtual = 2,
        [Description("مؤقت|Transient")] Transient = 3
    }

    // One UOM table serves both material boxes and product boxes.
    public enum UOMKinds
    {
        [Description("مادة|Material")] Material = 1,
        [Description("منتج|Product")] Product = 2
    }
    public enum NotificationRelatedTypes
    {
        DeliverySample = 1,
        CertificatePendingResult = 2,
        CertificateRejected = 3,
        BoxNeedsPutAway = 4,

        PreparingOrderDone = 10,
        ReleaseOrderDone = 11,
        ScrapOrderDone = 12,
        SupplierReturnOrderDone = 13,

        RFQCreated = 20,
        ContractReviewCreated = 21,
        PurchaseOrderCreated = 22,   // reserved
        AnnualPlanCreated = 23       // reserved
    }
    public enum PreparingSteps
    {
        [Description("الغسل|Washing")] Washing = 1,
        [Description("التنقية|Purification")] Purification = 2,
        [Description("التعقيم|Sterilization")] Sterilization = 3,
        [Description("التجفيف|Drying")] Drying = 4
    }
}