using Courses.Common.Security;
using Courses.Domain;
using ErrorOr;
using Microsoft.EntityFrameworkCore;

namespace Courses.Features.Programs.GetProgramRequest;

public class GetProgramRequestHandler(CoursesDbContext context, IUserAccessor userAccessor)
{
    public async Task<ErrorOr<GetProgramRequestResponse>> Handle(
        GetProgramRequestQuery query,
        CancellationToken ct)
    {
        if (!userAccessor.IsAdmin)
        {
            return Error.Unauthorized(
                "Program.Unauthorized",
                "Only Administrators can review program requests.");
        }

        var program = await context.Programs
            .AsNoTracking()
            .Include(p => p.Coach)
            .Include(p => p.TimeSlots)
            .FirstOrDefaultAsync(p => p.Id == query.ProgramId, ct);

        if (program is null)
        {
            return Error.NotFound("Program.NotFound", "Program not found.");
        }

        return new GetProgramRequestResponse(
            program.Id,
            program.Name,
            program.Description,
            program.Level,
            program.ExerciseType,
            program.DurationMinutes,
            program.StartDate,
            program.EndDate,
            program.TotalPrice,
            program.MaxParticipants,
            program.PictureUrl,
            program.Status.ToString(),
            $"{program.Coach.FirstName} {program.Coach.LastName}",
            program.TimeSlots.Select(t => new TimeSlotResponse(
                t.Id,
                t.Day.ToString(),
                t.StartTime.ToString("HH:mm"),
                t.EndTime.ToString("HH:mm"),
                t.Description)).ToList()
        );
    }
}
