using FluentValidation;
using Microsoft.Extensions.Logging;

namespace Courses.Common.Behaviours;

public class ValidationBehavior
{
    private readonly ILogger<ValidationBehavior> _logger;

    public ValidationBehavior(ILogger<ValidationBehavior> logger)
    {
        _logger = logger;
    }

    public async Task BeforeAsync(object message, IEnumerable<IValidator> validators)
    {
        var messageType = message.GetType();
        _logger.LogInformation("Starting validation for message type: {MessageType}", messageType.Name);

        var validator = validators.FirstOrDefault(x =>
            x.CanValidateInstancesOfType(messageType));

        if (validator is null)
        {
            _logger.LogInformation("No validator found for message type: {MessageType}", messageType.Name);
            return;
        }

        var context = new ValidationContext<object>(message);
        var result = await validator.ValidateAsync(context);

        if (!result.IsValid)
        {
            _logger.LogWarning("Validation failed for message type: {MessageType}. Errors: {@Errors}",
                messageType.Name, result.Errors);
            throw new ValidationException(result.Errors);
        }

        _logger.LogInformation("Validation completed successfully for message type: {MessageType}", messageType.Name);
    }
}
