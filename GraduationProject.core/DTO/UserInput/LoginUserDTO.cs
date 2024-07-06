using GraduationProject.core.Filters;
using System.ComponentModel.DataAnnotations;

namespace GraduationProject.core.DTO.UserInput
{
    public class LoginUserDTO
    {
        [Required]
        [MinLength(14)]
        [MaxLength(14)]
        [NationalIdDomainValidationAttribute]
        public string UserName { get; set; }
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
