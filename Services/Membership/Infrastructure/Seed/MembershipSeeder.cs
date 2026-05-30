using Bogus;
using Membership.Domain;
using Membership.Domain.Enums;
using Membership.Features.Members.CreateMember;
using Membership.Features.Members.UpdateMyProfile;
using Membership.Features.Plans.CreatePlan;
using Microsoft.EntityFrameworkCore;
using Wolverine;

namespace Membership.Infrastructure.Seed;

public sealed class MembershipSeeder(
    MembershipDbContext context,
    IIdentityServiceClient identityClient,
    IMessageBus messageBus)
{
    private const int MemberSeedCount = 50;

    public async Task SeedAsync(CancellationToken ct)
    {
        await SeedPlansAsync(ct);
        await SeedMembersAsync(ct);
    }

    private async Task SeedPlansAsync(CancellationToken ct)
    {
        if (await context.SubscriptionPlans.AnyAsync(ct))
        {
            return;
        }

        var adminAccessor = new SeedUserAccessor(Guid.NewGuid(), ["Admin"]);
        var handler = new CreatePlanHandler(context, adminAccessor);
        var plans = BuildPlanRequests();

        foreach (var plan in plans)
        {
            var result = await handler.Handle(new CreatePlanCommand(plan), ct);
            if (result.IsError)
            {
                throw new InvalidOperationException(result.FirstError.Description);
            }
        }
    }

    private async Task SeedMembersAsync(CancellationToken ct)
    {
        if (await context.Members.AnyAsync(ct))
        {
            return;
        }

        var planIds = await context.SubscriptionPlans
            .AsNoTracking()
            .Select(p => p.Id)
            .ToListAsync(ct);

        if (planIds.Count == 0)
        {
            throw new InvalidOperationException("No subscription plans available for seeding members.");
        }

        Randomizer.Seed = new Random(1337);
        var faker = new Faker("en");

        var memberHandler = new CreateMemberHandler(context, identityClient, messageBus);

        for (var i = 0; i < MemberSeedCount; i++)
        {
            var firstName = faker.Name.FirstName();
            var lastName = faker.Name.LastName();
            var email = $"seed.member{i + 1}@fitteck.com";
            var phone = faker.Phone.PhoneNumber("##########");
            var birthDate = DateOnly.FromDateTime(
                faker.Date.Between(DateTime.UtcNow.AddYears(-50), DateTime.UtcNow.AddYears(-18)));
            var gender = faker.PickRandom<Gender>();
            var cardUid = $"CARD-{(i + 1):D4}";
            var planId = planIds[i % planIds.Count];

            var request = new CreateMemberRequest
            {
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                PhoneNumber = phone,
                DateOfBirth = birthDate,
                Gender = gender,
                PlanId = planId,
                CardUid = cardUid,
                MedicalCertificate = null,
                ProfilePicture = null
            };

            var result = await memberHandler.Handle(new CreateMemberCommand(request), ct);
            if (result.IsError)
            {
                throw new InvalidOperationException(result.FirstError.Description);
            }

            var member = await context.Members
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == result.Value.MemberId, ct);

            if (member is null)
            {
                throw new InvalidOperationException("Seeded member could not be loaded for profile seeding.");
            }

            var profileHandler = new UpdateMyProfileHandler(
                context,
                identityClient,
                new SeedUserAccessor(member.UserId, ["Member"]));

            var goals = faker.Lorem.Sentence(6);
            var profileResult = await profileHandler.Handle(
                new UpdateMyProfileCommand(new UpdateMyProfileRequest(null, goals, null)),
                ct);

            if (profileResult.IsError)
            {
                throw new InvalidOperationException(profileResult.FirstError.Description);
            }
        }
    }

    private static List<CreatePlanRequest> BuildPlanRequests()
    {
        return
        [
            new("Starter Monthly", "Basic monthly access", 25, 1, DurationUnit.Months, null, ["GymFloor"]),
            new("Standard Monthly", "Standard monthly access", 40, 1, DurationUnit.Months, null, ["GymFloor", "Classes"]),
            new("Premium Monthly", "Premium monthly access", 60, 1, DurationUnit.Months, null, ["All"]),
            new("Annual Basic", "Annual plan with discount", 250, 12, DurationUnit.Months, null, ["GymFloor"]),
            new("Annual Premium", "Annual premium plan", 550, 12, DurationUnit.Months, null, ["All"]),
            new("10-Session Pack", "Pack of 10 sessions", 80, null, null, 10, ["Classes"]),
            new("20-Session Pack", "Pack of 20 sessions", 140, null, null, 20, ["Classes"]),
            new("Student Monthly", "Discounted student plan", 30, 1, DurationUnit.Months, null, ["GymFloor"]),
            new("Weekend Access", "Weekend-only access", 20, 1, DurationUnit.Months, null, ["GymFloor"]),
            new("Family Monthly", "Family plan", 90, 1, DurationUnit.Months, null, ["GymFloor", "Classes"])
        ];
    }
}
