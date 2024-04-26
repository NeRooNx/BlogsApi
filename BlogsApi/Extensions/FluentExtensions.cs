using FluentValidation.Validators;
using FluentValidation;
using System.Text.RegularExpressions;

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

}
