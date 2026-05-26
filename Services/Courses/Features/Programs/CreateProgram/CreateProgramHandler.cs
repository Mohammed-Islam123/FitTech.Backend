using Courses.Common.Security;
using Courses.Domain;
using Courses.Domain.Entities;
using Courses.Domain.Enums;
using ErrorOr;
using Microsoft.EntityFrameworkCore;
using Shared.Events;
using Wolverine;

namespace Courses.Features.Programs.CreateProgram;

/// <description>
/// Coach creates a new program. Submits for admin approval with Pending status.
/// Publishes ProgramCreatedEvent.
/// </description>
public class CreateProgramHandler(
    CoursesDbContext context,
    IUserAccessor userAccessor,
    IMessageBus messageBus)
{
    public async Task<ErrorOr<CreateProgramResponse>> Handle(
        CreateProgramCommand command,
        CancellationToken ct)
    {
        if (!userAccessor.IsCoach)
        {
            return Error.Unauthorized(
                "Program.Unauthorized",
                "Only Coaches can create programs.");
        }

        var currentUserId = userAccessor.UserId;
        if (currentUserId is null)
        {
            return Error.Unauthorized(
                "Program.Unauthorized",
                "Authentication required.");
        }

        var coach = await context.Coaches
            .FirstOrDefaultAsync(c => c.UserId == currentUserId, ct);

        if (coach is null)
        {
            return Error.NotFound(
                "Coach.NotFound",
                "Coach profile not found for the current user.");
        }

        var req = command.Request;

        var program = new ProgramEntity
        {
            Id = Guid.CreateVersion7(),
            CoachId = coach.Id,
            Name = req.Name,
            Description = req.Description,
            Level = req.Level,
            ExerciseType = req.ExerciseType,
            DurationMinutes = req.DurationMinutes,
            StartDate = req.StartDate,
            EndDate = req.EndDate,
            TotalPrice = req.TotalPrice,
            MaxParticipants = req.MaxParticipants,
            PictureUrl = req.PictureUrl,
            Status = ProgramStatus.Pending
        };

        foreach (var slotReq in req.TimeSlots)
        {
            var day = Enum.Parse<CourseDayOfWeek>(slotReq.Day, ignoreCase: true);
            var startTime = TimeOnly.Parse(slotReq.StartTime);
            var endTime = TimeOnly.Parse(slotReq.EndTime);

            program.TimeSlots.Add(new ProgramTimeSlot
            {
                Id = Guid.CreateVersion7(),
                ProgramId = program.Id,
                Day = day,
                StartTime = startTime,
                EndTime = endTime,
                Description = slotReq.Description
            });
        }

        context.Programs.Add(program);
        await context.SaveChangesAsync(ct);

        await messageBus.PublishAsync(new ProgramCreatedEvent(
            ProgramId: program.Id,
            ProgramName: program.Name,
            CoachId: coach.Id,
            CoachName: coach.FullName,
            CreatedAt: program.CreatedAt));

        return new CreateProgramResponse(program.Id);
    }
}
