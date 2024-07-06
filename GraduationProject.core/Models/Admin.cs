using System.ComponentModel.DataAnnotations;
 namespace GraduationProject.core.Models
{
    public class Admin
    {

        [Key]
        public int AdminId { get; set; }
        [Required]
        public string AdminName { get; set; }
        [Required]
        public string NationalId { get; set; }
        [Required]
        public string AcademicEmail { get; set; }
        public bool IsDeleted { get; set; } = false;

    }
}
