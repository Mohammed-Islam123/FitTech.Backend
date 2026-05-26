namespace Courses.Features.Programs.GetProgramMembers;

public record GetProgramMembersResponse(
    Guid MemberId,
    string FullName,
    string Email,
    DateTime EnrolledAt
);
