using System.ComponentModel.DataAnnotations;

namespace GraduationProject.core.Filters
{
    public class LevelSemesterValidationAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var levelProperty = validationContext.ObjectType.GetProperty("Level");
            if (levelProperty == null)
            {
                return new ValidationResult("Level property not found.");
            }

            var levelValue = (int)levelProperty.GetValue(validationContext.ObjectInstance, null);

            if (value is int semesterValue)
            {
                if (levelValue == 1 && (semesterValue == 1 || semesterValue == 2))
                {
                    return ValidationResult.Success;
                }

                if (levelValue == 2 && (semesterValue == 3 || semesterValue == 4))
                {
                    return ValidationResult.Success;
                }

                if (levelValue == 3 && (semesterValue == 5 || semesterValue == 6))
                {
                    return ValidationResult.Success;
                }

                if (levelValue == 4 && (semesterValue == 7 || semesterValue == 8))
                {
                    return ValidationResult.Success;
                }

                return new ValidationResult($"For Level {levelValue}, Semester must be within the specified range.");
            }

            return new ValidationResult("Invalid Semester value.");
        }
    }
}

