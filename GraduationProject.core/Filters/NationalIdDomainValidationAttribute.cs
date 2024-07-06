using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace GraduationProject.core.Filters
{
    public class NationalIdDomainValidationAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is string NationalID)
            {
                // Regular expression to check if the string consists only of digits
                if (Regex.IsMatch(NationalID, @"^\d+$"))
                {
                    return ValidationResult.Success;
                }
                else
                {
                    return new ValidationResult("NationalID must consist of digits only.");
                }
            }

            return new ValidationResult("Invalid NationalID format.");
        }
    }
}

