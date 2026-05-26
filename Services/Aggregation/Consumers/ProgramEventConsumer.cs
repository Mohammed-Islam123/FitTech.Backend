using Aggregation.Domain;
using Shared.Events;

namespace Aggregation.Consumers;

public class ProgramEventConsumer
{
    public Task Handle(ProgramAcceptedEvent evt)
    {
        return Task.CompletedTask;
    }
}
