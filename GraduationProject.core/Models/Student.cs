using System.ComponentModel.DataAnnotations;

 namespace GraduationProject.core.Models
{
    public class Student
    {

        [Key]
        public int StudentId { get; set; }
        [Required]
        public string AcademicNumber { get; set; }
        [Required]
        public string StudentName { get; set; }
        [Required]
        public string NationalId { get; set; }
        [Required]
        public string AcademicEmail { get; set; }
        [Required]
        public string Department { get; set; } // for first and second level if there is a group then the department is the group 
        [Required]
        public int Level { get; set; }
        [Required]
        public int Semester { get; set; }
        public bool IsDeleted { get; set; } = false;
    }
}
