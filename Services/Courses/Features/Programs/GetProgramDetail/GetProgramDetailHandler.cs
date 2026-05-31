using Courses.Domain;
using ErrorOr;
using Microsoft.EntityFrameworkCore;

namespace Courses.Features.Programs.GetProgramDetail;

/// <description>
/// Returns full details of a specific program including time slots and coach info.
/// </description>
public class GetProgramDetailHandler(CoursesDbContext context)
{
    public async Task<ErrorOr<GetProgramDetailResponse>> Handle(
        Guid programId,
        CancellationToken ct)
    {
        var program = await context.Programs
            .AsNoTracking()
            .Include(p => p.Coach)
            .Include(p => p.TimeSlots)
            .Include(p => p.Enrollments)
            .FirstOrDefaultAsync(p => p.Id == programId, ct);

        if (program is null)
        {
            return Error.NotFound(
                "Program.NotFound",
                "The specified program does not exist.");
        }

        var spotsLeft = program.MaxParticipants - program.Enrollments.Count;

        var timeSlots = program.TimeSlots
            .Select(t => new ProgramTimeSlotResponse(
                t.Id,
                t.Day.ToString(),
                t.StartTime.ToString("HH:mm"),
                t.EndTime.ToString("HH:mm"),
                t.Description))
            .ToList();

        return new GetProgramDetailResponse(
            Id: program.Id,
            Name: program.Name,
            Price: program.TotalPrice,
            Description: program.Description,
            SpotsLeft: spotsLeft,
            Capacity: program.MaxParticipants,
            CoachId: program.CoachId,
            CoachName: $"{program.Coach.FirstName} {program.Coach.LastName}",
            Level: program.Level,
            ExerciseType: program.ExerciseType,
            DurationMinutes: program.DurationMinutes,
            TimeSlots: timeSlots);
    }
}
