using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

 namespace GraduationProject.core.Models
{
    public class DoctorCourseHall
    {
        [Key]
        public int Id { get; set; }
        [ForeignKey(nameof(Doctor))]
        public int DoctorId { get; set; }

        [ForeignKey(nameof(Course))]
        public int CourseId { get; set; }

        [ForeignKey(nameof(Hall))]
        public int HallId { get; set; }
        public string? Day { get; set; }
        public string? FirstHour { get; set; }
        public string? SecondHour { get; set; }
        public bool IsCompeleted { get; set; }

        public string? Option1 { get; set; }

        public string? Option2 { get; set; }

        public string? Option3 { get; set; }

        public string? Department { get; set; }
        public string Coursecode {  get; set; }
        public bool IsDeleted { get; set; } = false;
    }
}
