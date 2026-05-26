using Courses.Common.Security;
using Courses.Domain;
using Courses.Domain.Enums;
using ErrorOr;
using Microsoft.EntityFrameworkCore;
using Shared.Events;
using Wolverine;

namespace Courses.Features.Programs.RejectProgram;

public class RejectProgramHandler(
    CoursesDbContext context,
    IUserAccessor userAccessor,
    IMessageBus messageBus)
{
    public async Task<ErrorOr<RejectProgramResponse>> Handle(
        RejectProgramCommand command,
        CancellationToken ct)
    {
        if (!userAccessor.IsAdmin)
        {
            return Error.Unauthorized(
                "Program.Unauthorized",
                "Only Administrators can reject program requests.");
        }

        var program = await context.Programs
            .Include(p => p.Coach)
            .FirstOrDefaultAsync(p => p.Id == command.ProgramId, ct);

        if (program is null)
        {
            return Error.NotFound("Program.NotFound", "Program not found.");
        }

        if (program.Status != ProgramStatus.Pending)
        {
            return Error.Conflict(
                "Program.AlreadyReviewed",
                $"Program has already been {program.Status.ToString().ToLowerInvariant()}.");
        }

        program.Status = ProgramStatus.Rejected;
        program.ReviewedAt = DateTime.UtcNow;
        await context.SaveChangesAsync(ct);

        await messageBus.PublishAsync(new ProgramRejectedEvent(
            ProgramId: program.Id,
            ProgramName: program.Name,
            CoachId: program.CoachId,
            CoachName: program.Coach.FullName,
            RejectedAt: program.ReviewedAt!.Value));

        return new RejectProgramResponse(program.Id, program.Status.ToString());
    }
}
