namespace Courses.Features.Coaches.GetCoachClients;

public record GetCoachClientsResponse(
    Guid MemberId,
    string FullName,
    string Email,
    string PhoneNumber,
    DateTime RegistrationDate,
    List<CourseAssignmentResponse> AssignedCourses
);

public record CourseAssignmentResponse(
    Guid ProgramId,
    string ProgramName
);
