using FluentValidation;
using Service.Helpers.Exceptions;

namespace Service.Helpers.Validators
{
    public static class ValidationExtensions
    {
        public static async Task EnsureValidAsync<T>(this IValidator<T> validator, T model)
        {
            var result = await validator.ValidateAsync(model);
            if (!result.IsValid)
            {
                throw new BadRequestException(result.Errors.Select(e => e.ErrorMessage).Distinct().ToArray());
            }
        }

        // Mətn boş olmamalıdır və kənar boşluqlar silindikdən sonra limiti keçməməlidir (servis də dəyəri Trim ilə yazır)
        public static IRuleBuilderOptions<T, string?> RequiredText<T>(this IRuleBuilder<T, string?> rule, string name, int maxLength)
        {
            return rule
                .Must(value => !string.IsNullOrWhiteSpace(value)).WithMessage($"Enter the {name}.")
                .Must(value => string.IsNullOrWhiteSpace(value) || value.Trim().Length <= maxLength)
                    .WithMessage($"The {name} can be at most {maxLength} characters.");
        }

        // Boş ola bilər, yazılıbsa limiti keçməməlidir
        public static IRuleBuilderOptions<T, string?> OptionalText<T>(this IRuleBuilder<T, string?> rule, string name, int maxLength)
        {
            return rule
                .Must(value => string.IsNullOrWhiteSpace(value) || value.Trim().Length <= maxLength)
                    .WithMessage($"The {name} can be at most {maxLength} characters.");
        }
    }
}
