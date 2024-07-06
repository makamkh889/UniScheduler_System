using System.ComponentModel.DataAnnotations;

 namespace GraduationProject.core.Models
{
    public class Doctor
    {
        [Key]
        public int DoctorId { get; set; }
        [Required]
        public string DoctorName { get; set; }
        [Required]
        public string NationalId { get; set; }
        [Required]
        public string AcademicEmail { get; set; }
        [Required]
        public string Department { get; set; }
        public bool IsDeleted { get; set; } = false;
    }
}
