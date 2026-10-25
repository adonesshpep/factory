using System.ComponentModel;

namespace neotonic.Models
{
    public enum RFQStatus
    {
        [Description("جديد|New")]
        New = 1,
        [Description("القسم الفني|technical")]
        Technical = 2,
        [Description("مدير المنشأة|Establishment Manager")]
        Establishment_Manager = 3
    }
    public enum CRStatus
    {
        [Description("جديد|New")]
        New = 1,
        [Description("القسم الفني|technical")]
        Technical = 2,
        [Description("مدير المنشأة|Establishment Manager")]
        Establishment_Manager = 3

    }
    public enum AnnualStatuses
    {
        [Description("جديد|New")]
        New = 1,
        [Description("القسم الفني|technical")]
        Technical = 2,
        [Description("مدير المنشأة|Establishment Manager")]
        Establishment_Manager = 3

    }
    public enum CleaningTypes
    {
        [Description("قسم|Department")]
        Department = 1,
        [Description("ميزان|Scale")]
        Scale = 2,
        [Description("الة|Machine")]
        Machine = 3,
        [Description("ملابس|Clothe")]
        Clothe = 4
    }
    public enum ConnectionMethods
    {
        [Description("مباشر|Direct")]
        Direct = 1,
        [Description("ايميل|Email")]
        Email = 2,
        [Description("هاتف|Phone")]
        Phone = 3,
        [Description("صديق|Friend")]
        Friend=4
    }
    public enum AnalysisResults
    {
        [Description("مقبول|Accepted")]
        Accepted = 1,
        [Description("مرفوض|Rejected")]
        Rejected = 2
    }
    public enum POStatus
    {
        [Description("معلق|Pending")]
        Pending = 0,
        [Description("معتمد|Approved")]
        Approved = 1,
        [Description("ملغي|Cancelled")]
        Cancelled = 2
    }
    public enum MemoType
    {
        // Stored as int in the DB. Matches the desktop app's 2-row type table.
        [Description("صادر|Out")]
        Out = -1,
        [Description("وارد|In")]
        In = 1
    }
    public enum WorkStatus
    {
        [Description("معلق|Pending")]
        Pending = 0,
        [Description("قيد التنفيذ|In Progress")]
        In_Progress = 1,
        [Description("مكتمل|Completed")]
        Completed = 2,
        [Description("ملغي|Cancelled")]
        Cancelled = 3
    }
}
