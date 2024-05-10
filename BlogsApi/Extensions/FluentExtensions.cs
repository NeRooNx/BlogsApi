using FluentValidation.Validators;
using FluentValidation;
using System.Text.RegularExpressions;
using FluentValidation.Results;

namespace BlogsApi.Extensions;

public static class FluentExtensions
{
    public static IRuleBuilderOptions<T, string?> IsPassword<T>(this IRuleBuilder<T, string?> ruleBuilder)
        => ruleBuilder.Must(x => PasswordFormat(x)).WithMessage("La contraseña no cumple las condiciones");


    private static bool PasswordFormat(string? password)
    {
        string pattern = "^(?=.*\\d)(?=.*[a-z])(?=.*[A-Z])(?=.*[a-zA-Z]).{8,}$";

        return Regex.Match(password!, pattern).Success;
    }

    public static IDictionary<string, string[]> ToValidationDictionary(this ValidationResult validationResult)
    {
        var errorsDictionary = new Dictionary<string, List<string>>();

        foreach (var error in validationResult.Errors)
        {
            if (!errorsDictionary.ContainsKey(error.PropertyName))
            {
                errorsDictionary.Add(error.PropertyName, new List<string>());
            }

            errorsDictionary[error.PropertyName].Add(error.ErrorMessage);
        }

        return errorsDictionary.ToDictionary(kv => kv.Key, kv => kv.Value.ToArray());
    }

}
