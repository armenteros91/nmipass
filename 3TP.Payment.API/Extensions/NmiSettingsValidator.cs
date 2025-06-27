using Microsoft.Extensions.Options;
using ThreeTP.Payment.Infrastructure.Services.Nmi;

namespace ThreeTP.Payment.API.Extensions;

public class NmiSettingsValidator : IValidateOptions<NmiSettings>
{
    public ValidateOptionsResult Validate(string name, NmiSettings settings)
    {
        if (settings == null)
        {
            return ValidateOptionsResult.Fail("NmiSettings configuration is missing.");
        }

        if (string.IsNullOrWhiteSpace(settings.BaseUrl))
        {
            return ValidateOptionsResult.Fail("NmiSettings:BaseURL is required.");
        }

        if (!Uri.IsWellFormedUriString(settings.BaseUrl, UriKind.Absolute))
        {
            return ValidateOptionsResult.Fail("NmiSettings:BaseURL must be a valid absolute URL.");
        }

        if (settings.Endpoint == null)
        {
            return ValidateOptionsResult.Fail("NmiSettings:Endpoint section is required.");
        }

        if (string.IsNullOrWhiteSpace(settings.Endpoint.Transaction))
        {
            return ValidateOptionsResult.Fail("NmiSettings:Endpoint:Transaction is required.");
        }

        if (settings.Query == null)
        {
            return ValidateOptionsResult.Fail("NmiSettings:Query section is required.");
        }

        if (string.IsNullOrWhiteSpace(settings.Query.QueryApi))
        {
            return ValidateOptionsResult.Fail("NmiSettings:Query:QueryApi is required.");
        }

        return ValidateOptionsResult.Success;
    }
}