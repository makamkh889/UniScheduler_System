using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

 namespace GraduationProject.core.Models
{
    public class StudentCourse
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey(nameof(Student))]
        public int StudentId { get; set; }
        [ForeignKey(nameof(Department))]
        public int CourseId { get; set; }
        public float Degree { get; set; }
        [Required]
        public string? RecordedCourse { get; set; }
        public bool IsDeleted { get; set; } = false;

    }
}
