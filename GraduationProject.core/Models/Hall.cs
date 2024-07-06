using System.ComponentModel.DataAnnotations;

 namespace GraduationProject.core.Models
{
    public class Hall
    {
        [Key]
        public int HallId { get; set; }
        [Required]
        public string HallName { get; set; }
        [Required]
        public int Capacity { get; set; }
    }
}
