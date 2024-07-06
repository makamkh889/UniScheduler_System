using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

 namespace GraduationProject.core.Models
{
    public class Course
    {
        [Key]
        public int CourseId { get; set; }
        [Required]
        public string CourseName { get; set; }
        [Required]
        public string CourseCode { get; set; }
        [Required]
        public int CreditHour { get; set; }
        public string? CoursePrerequisites { get; set; }
        [Required]
        public string Department { get; set; }
        public bool IsDeleted { get; set; } = false;
    }
}
