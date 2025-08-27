using System.Text.Json;
using MediatR;
using Microsoft.Extensions.Logging;
using OdixPay.Notifications.Application.Interfaces;
using OdixPay.Notifications.Domain.DTO.Requests;
using OdixPay.Notifications.Domain.Interfaces;

namespace OdixPay.Notifications.Application.UseCases;

public class ValidateTemplateVariablesDTO
{
    public Guid TemplateId { get; set; }
    public IDictionary<string, string> Variables { get; set; }
}

public class ValidateTemplateVariables(INotificationTemplateRepository templateRepository, ILogger<ValidateTemplateVariables> logger) : IUseCase<ValidateTemplateVariablesDTO, Task<bool>>
{
    private readonly INotificationTemplateRepository _templateRepository = templateRepository ?? throw new ArgumentNullException(nameof(templateRepository));
    private readonly ILogger<ValidateTemplateVariables> _logger = logger ?? throw new ArgumentNullException(nameof(logger));


    public async Task<bool> ExecuteAsync(ValidateTemplateVariablesDTO? request, CancellationToken cancellationToken = default)
    {
        if (request == null)
        {
            _logger.LogError("Request for ValidateTemplateVariables is null");
            return false; // Invalid request
        }

        _logger.LogInformation("Validating template variables for template {TemplateId}", request.TemplateId);

        var template = await _templateRepository.GetTemplateByIdAsync(request.TemplateId, cancellationToken);

        if (template == null)
        {
            return false; // Template not found
        }

        if (template.Variables == null || string.IsNullOrWhiteSpace(template.Variables))
        {
            return true; // No variables to validate, considered valid
        }
        // Parse the variables from the template
        try
        {
            var variables = JsonSerializer.Deserialize<Dictionary<string, TemplateVariableStructure>>(template.Variables);

            if (variables == null)
            {
                return false; // Invalid variable format
            }

            _logger.LogInformation("Validating template variables for template: {TemplateId}", JsonSerializer.Serialize(variables));

            _logger.LogInformation("Request Variables: {Variables}", JsonSerializer.Serialize(request.Variables));

            // Check if all required variables are present in the request
            foreach (var variable in variables)
            {
                if (variable.Value.Required &&
                    (!request.Variables.TryGetValue(variable.Key, out var value) || value is not string stringValue || string.IsNullOrEmpty(stringValue)))
                {
                    _logger.LogError("Required variable {VariableName} is missing or empty in the request for template {TemplateId}", variable.Key, request.TemplateId);
                    return false; // Required variable is empty
                }

                // Validate the variable type if necessary
                // if (!IsValidVariableType(variable.Value.Type, request.Variables[variable.Key]))
                // {
                //     return false; // Invalid variable type
                // }

            }

            return true; // All required variables are present and valid
        }
        catch (System.Exception)
        {
            _logger.LogError("Failed to deserialize template variables for template {TemplateId}", request.TemplateId);
            return false;
        }
    }

    private static bool IsValidVariableType(string type, string value)
    {


        return type switch
        {
            "string" => !string.IsNullOrEmpty(value),
            "int" => int.TryParse(value, out _),
            "bool" => bool.TryParse(value, out _),
            "date" => DateTime.TryParse(value, out _),
            "decimal" => decimal.TryParse(value, out _),
            _ => true // Unknown type, consider valid
        };
    }
}