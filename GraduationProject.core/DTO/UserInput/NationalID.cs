using GraduationProject.core.Filters;
using System.ComponentModel.DataAnnotations;
namespace GraduationProject.core.DTO.UserInput
{
    public class NationalID
    {
        [Required]
        [MaxLength(14)]
        [MinLength(14)]
        [NationalIdDomainValidationAttribute]
        public string NationalId { get; set; }
    }
}
