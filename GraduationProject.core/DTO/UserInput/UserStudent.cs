using GraduationProject.core.Filters;
using System.ComponentModel.DataAnnotations;

namespace GraduationProject.core.DTO.UserInput
{
    public class UserStudent : User
    {

        public string AcademicNumber { get; set; }
        public string Department { get; set; }
        [Range(1, 4, ErrorMessage = "Level must be between {1} and {4}.")]
        public int Level { get; set; }
        [Range(1, 8, ErrorMessage = "Semseter must be between {1} and {8}.")]
        [LevelSemesterValidation(ErrorMessage = "Invalid Semester for the given Level.")]
        public int Semester { get; set; }
    }
}
