using GraduationProject.core.Filters;
using System.ComponentModel.DataAnnotations;

namespace GraduationProject.core.DTO.UserInput
{
    public class User: NationalID
    {
        [Required]
        public string Name { get; set; }
        [Required]
        [EmailDomainValidation("@compit.aun.edu.eg")]
        public string Email { get; set; }
    }
}
