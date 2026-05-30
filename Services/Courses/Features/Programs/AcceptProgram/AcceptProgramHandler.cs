using Courses.Common.Security;
using Courses.Domain;
using Courses.Domain.Entities;
using Courses.Domain.Enums;
using ErrorOr;
using Microsoft.EntityFrameworkCore;
using Shared.Events;
using Wolverine;

namespace Courses.Features.Programs.AcceptProgram;

/// <description>
/// Admin accepts a program creation request. Generates sessions between start and end dates.
/// </description>
public class AcceptProgramHandler(
    CoursesDbContext context,
    IUserAccessor userAccessor,
    IMessageBus messageBus)
{
    public async Task<ErrorOr<AcceptProgramResponse>> Handle(
        AcceptProgramCommand command,
        CancellationToken ct)
    {
        if (!userAccessor.IsAdmin)
        {
            return Error.Unauthorized(
                "Program.Unauthorized",
                "Only Administrators can accept program requests.");
        }

        var program = await context.Programs
            .Include(p => p.TimeSlots)
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

        program.Status = ProgramStatus.Accepted;
        program.ReviewedAt = DateTime.UtcNow;

        foreach (var slot in program.TimeSlots)
        {
            var date = program.StartDate;
            while (date <= program.EndDate)
            {
                if (date.DayOfWeek == MapDayOfWeek(slot.Day))
                {
                    program.Sessions.Add(new Session
                    {
                        Id = Guid.CreateVersion7(),
                        ProgramId = program.Id,
                        TimeSlotId = slot.Id,
                        Date = date,
                        StartTime = slot.StartTime,
                        EndTime = slot.EndTime
                    });
                }
                date = date.AddDays(1);
            }
        }

        await context.SaveChangesAsync(ct);

        await messageBus.PublishAsync(new ProgramAcceptedEvent(
            ProgramId: program.Id,
            ProgramName: program.Name,
            CoachId: program.CoachId,
            CoachName: $"{program.Coach.FirstName} {program.Coach.LastName}",
            AcceptedAt: program.ReviewedAt!.Value));

        return new AcceptProgramResponse(program.Id, program.Status.ToString());
    }

    private static System.DayOfWeek MapDayOfWeek(CourseDayOfWeek day) => day switch
    {
        CourseDayOfWeek.Sunday => System.DayOfWeek.Sunday,
        CourseDayOfWeek.Monday => System.DayOfWeek.Monday,
        CourseDayOfWeek.Tuesday => System.DayOfWeek.Tuesday,
        CourseDayOfWeek.Wednesday => System.DayOfWeek.Wednesday,
        CourseDayOfWeek.Thursday => System.DayOfWeek.Thursday,
        CourseDayOfWeek.Friday => System.DayOfWeek.Friday,
        CourseDayOfWeek.Saturday => System.DayOfWeek.Saturday,
        _ => throw new ArgumentOutOfRangeException(nameof(day))
    };
}
