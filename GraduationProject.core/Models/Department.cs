using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

 namespace GraduationProject.core.Models
{
    public class Department
    {

        [Key]
        public int CourseId { get; set; }
        [Required]
        public string CourseCode { get; set; }
        [Required]
        public string DepartmentName { get; set; }
        [Required]
        public int Semaster { get; set; }
        public bool IsDeleted { get; set; } = false;
    }
}

