using System.ComponentModel.DataAnnotations;
namespace GraduationProject.core.Filters
{
    public class EmailDomainValidationAttribute : ValidationAttribute
    {
        private readonly string _allowedDomain;

        public EmailDomainValidationAttribute(string allowedDomain)
        {
            _allowedDomain = allowedDomain;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is string email)
            {
                if (email.EndsWith(_allowedDomain, StringComparison.OrdinalIgnoreCase))
                {
                    return ValidationResult.Success;
                }
                else
                {
                    return new ValidationResult($"Email must end with {_allowedDomain}");
                }
            }

            return new ValidationResult("Invalid email format");
        }
    }

}
