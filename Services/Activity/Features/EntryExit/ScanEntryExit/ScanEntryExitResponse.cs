namespace Activity.Features.EntryExit.ScanEntryExit;

public record ScanEntryExitResponse(
    bool Success,
    string VerificationType,
    string MemberName,
    int? RemainingSessions,
    ActiveMembershipInfo? ActiveMembership,
    List<ActiveCourseInfo> ActiveCourses
);

public record ActiveMembershipInfo(Guid MembershipId, string PlanName, DateTime? EndDate);

public record ActiveCourseInfo(Guid CourseId, string CourseName);
