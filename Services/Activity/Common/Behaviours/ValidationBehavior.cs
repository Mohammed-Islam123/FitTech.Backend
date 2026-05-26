using FluentValidation;
using Microsoft.Extensions.Logging;

namespace Activity.Common.Behaviours;

public class ValidationBehavior
{
    private readonly ILogger<ValidationBehavior> _logger;
    public ValidationBehavior(ILogger<ValidationBehavior> logger) => _logger = logger;

    public async Task BeforeAsync(object message, IEnumerable<IValidator> validators)
    {
        var messageType = message.GetType();
        var validator = validators.FirstOrDefault(x => x.CanValidateInstancesOfType(messageType));
        if (validator is null) return;
        var context = new ValidationContext<object>(message);
        var result = await validator.ValidateAsync(context);
        if (!result.IsValid) throw new ValidationException(result.Errors);
    }
}
