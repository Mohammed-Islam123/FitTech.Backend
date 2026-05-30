using Bogus;
using Courses.Domain;
using Courses.Domain.Enums;
using Courses.Features.Coaches.CreateCoach;
using Courses.Features.Programs.CreateProgram;
using Microsoft.EntityFrameworkCore;
using Wolverine;

namespace Courses.Infrastructure.Seed;

public sealed class CoursesSeeder(
    CoursesDbContext context,
    IIdentityServiceClient identityClient,
    IMessageBus messageBus)
{
    private const int CoachSeedCount = 10;
    private const int ProgramSeedCount = 20;

    public async Task SeedAsync(CancellationToken ct)
    {
        if (await context.Coaches.AnyAsync(ct) || await context.Programs.AnyAsync(ct))
        {
            return;
        }

        Randomizer.Seed = new Random(1337);
        var faker = new Faker("en");

        var adminAccessor = new SeedUserAccessor(Guid.NewGuid(), ["Admin"]);
        var coachHandler = new CreateCoachHandler(context, adminAccessor, identityClient, messageBus);

        var coachIds = new List<Guid>(CoachSeedCount);
        for (var i = 0; i < CoachSeedCount; i++)
        {
            var firstName = faker.Name.FirstName();
            var lastName = faker.Name.LastName();
            var email = $"seed.coach{i + 1}@fitteck.com";
            var phone = faker.Phone.PhoneNumber("##########");
            var birthDate = DateOnly.FromDateTime(
                faker.Date.Between(DateTime.UtcNow.AddYears(-50), DateTime.UtcNow.AddYears(-22)));
            var gender = faker.PickRandom<Gender>();

            var request = new CreateCoachRequest(
                firstName,
                lastName,
                email,
                phone,
                birthDate,
                gender,
                faker.Lorem.Sentence(8));

            var result = await coachHandler.Handle(new CreateCoachCommand(request), ct);
            if (result.IsError)
            {
                throw new InvalidOperationException(result.FirstError.Description);
            }

            coachIds.Add(result.Value.CoachId);
        }

        var programsPerCoach = ProgramSeedCount / CoachSeedCount;
        var extraPrograms = ProgramSeedCount % CoachSeedCount;
        var coachIndex = 0;

        foreach (var coachId in coachIds)
        {
            var coach = await context.Coaches
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == coachId, ct);

            if (coach is null)
            {
                throw new InvalidOperationException("Seeded coach could not be loaded for program seeding.");
            }

            var programCount = programsPerCoach + (coachIndex < extraPrograms ? 1 : 0);
            coachIndex++;

            var programHandler = new CreateProgramHandler(
                context,
                new SeedUserAccessor(coach.UserId, ["Coach"]),
                messageBus);

            for (var j = 0; j < programCount; j++)
            {
                var startDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7));
                var endDate = startDate.AddDays(30);

                var timeSlots = BuildTimeSlots(faker);
                var request = new CreateProgramRequest(
                    Name: $"{faker.Commerce.ProductName()} Program",
                    Description: faker.Lorem.Sentence(10),
                    Level: faker.PickRandom(new[] { "Beginner", "Intermediate", "Advanced" }),
                    ExerciseType: faker.PickRandom(new[] { "Strength", "Cardio", "HIIT", "Mobility", "Crossfit" }),
                    DurationMinutes: faker.Random.Int(30, 90),
                    StartDate: startDate,
                    EndDate: endDate,
                    TotalPrice: faker.Random.Decimal(50, 300),
                    MaxParticipants: faker.Random.Int(8, 20),
                    PictureUrl: null,
                    TimeSlots: timeSlots);

                var result = await programHandler.Handle(new CreateProgramCommand(request), ct);
                if (result.IsError)
                {
                    throw new InvalidOperationException(result.FirstError.Description);
                }
            }
        }
    }

    private static List<TimeSlotRequest> BuildTimeSlots(Faker faker)
    {
        var days = Enum.GetNames<CourseDayOfWeek>().ToList();
        days = faker.Random.Shuffle(days).Take(2).ToList();

        return
        [
            new TimeSlotRequest(days[0], "08:00", "09:00", "Morning slot"),
            new TimeSlotRequest(days[1], "18:00", "19:00", "Evening slot")
        ];
    }
}
