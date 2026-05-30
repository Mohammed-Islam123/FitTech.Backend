using ErrorOr;
using Wolverine;

namespace Membership.Features.Plans.ListPlans;

public record ListPlansResponseWrapper(ErrorOr<List<ListPlansResponse>> Value) : IResponse;
