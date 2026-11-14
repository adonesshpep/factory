namespace neotonic.Models.HR
{
    public record FeedbackQuestion(int Number, string TextAr, string TextEn);

    // Questions of form HR/S08/F09 (issue 01, 1/6/2025)
    public static class FeedbackQuestions
    {
        public static readonly IReadOnlyList<FeedbackQuestion> All = new List<FeedbackQuestion>
        {
            new(1,  "مستوى الرضى عن جو العمل؟",
                    "Level of satisfaction with the work environment?"),
            new(2,  "مستوى العلاقة مع المدير المباشر؟",
                    "Level of the relationship with your direct manager?"),
            new(3,  "مستوى الرضى عن دور الإدارة في حل أي خلاف (مظلمة) أو شكوى، إن وجدت؟",
                    "Level of satisfaction with management's role in resolving any dispute (grievance) or complaint, if any?"),
            new(4,  "مستوى الأمان الوظيفي (الاستمرارية)؟",
                    "Level of job security (continuity)?"),
            new(5,  "هل تقوم الشركة باتخاذ الإجراءات التصحيحية المناسبة لأي اقتراح أو تطوير تقدمت به، إن وجد؟",
                    "Does the company take appropriate corrective action on any suggestion or improvement you submitted, if any?"),
            new(6,  "مستوى الحوافز التي تقدمها الشركة من أجل المحافظة على جودة العمل والمنتج؟",
                    "Level of incentives the company provides to maintain the quality of work and product?"),
            new(7,  "مستوى برنامج التدريب العام والخاص (في القسم) في الشركة؟",
                    "Level of the general and department-specific training program in the company?"),
            new(8,  "هل آلية وطريقة العمل التي تقوم بها مناسبة ومتطورة؟",
                    "Is the work method you follow suitable and up to date?"),
            new(9,  "هل قدراتك وطموحاتك تتناسب مع مهام العمل الموكلة إليك؟",
                    "Do your abilities and ambitions match the tasks assigned to you?"),
            new(10, "مستوى التعاون والعمل بروح الفريق؟",
                    "Level of cooperation and teamwork?")
        };
    }
}