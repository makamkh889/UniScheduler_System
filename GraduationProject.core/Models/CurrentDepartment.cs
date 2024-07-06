using System.ComponentModel.DataAnnotations;
 namespace GraduationProject.core.Models
{
    public class CurrentDepartment
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public int Level { get; set; }

    }
}
