using System.ComponentModel.DataAnnotations;

namespace GraduationProject.core.DTO.UserInput
{
    public class CourseData
    {
        public string CourseName { get; set; }
        [Required]
        public string CourseCode { get; set; }
        [Required]
        [Range(1, 3, ErrorMessage = "CreditHour must be between {1} and {3}.")]
        public int CreditHour { get; set; }
        public List<string>? Prerequisites { get; set; }
        [Required]
        public string Department { get; set; }
        [Required]
        [Range(1, 8, ErrorMessage = "Semseter must be between {1} and {8}.")]
        public int Semester { get; set; }
    }
}
