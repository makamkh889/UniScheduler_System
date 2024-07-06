using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace GraduationProject.core.Filters
{
    public class TimeDayValidationAttribute : ValidationAttribute
    {
        private static readonly string[] Days = { "sat", "sun", "mon", "tues", "wed", "thurs" };

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is string input)
            {
                var parts = input.Split(' ');

                if (parts.Length != 2)
                {
                    return new ValidationResult("Input must be in the format 'hh:mm day'.");
                }

                var timePart = parts[0];
                var dayPart = parts[1].ToLower();

                if (!Days.Contains(dayPart))
                {
                    return new ValidationResult($"Day must be one of the following: {string.Join(", ", Days)}.");
                }

                if (!DateTime.TryParseExact(timePart, "hh:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
                {
                    return new ValidationResult("Time must be in the format 'hh:mm day'.");
                }

                return ValidationResult.Success;
            }

            return new ValidationResult("Invalid input format.");
        }
    }
}

