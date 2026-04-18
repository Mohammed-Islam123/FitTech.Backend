namespace Membership.Features.Members.UpdateMember;

public record UpdateMemberCommand(UpdateMemberRequest Request, Guid? Id = null);
