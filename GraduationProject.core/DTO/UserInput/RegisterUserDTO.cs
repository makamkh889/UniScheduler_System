using System.ComponentModel.DataAnnotations;

namespace GraduationProject.core.DTO.UserInput
{
    public class RegisterUserDTO
    {
        [Required]
        [MinLength(14)]
        [MaxLength(14)]
        public string UserName { get; set; }
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
