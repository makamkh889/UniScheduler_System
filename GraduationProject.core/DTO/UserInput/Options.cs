using GraduationProject.core.Filters;

namespace GraduationProject.core.DTO.UserInput
{
    public class Options
    {
        public string CourseCode { get; set; }
        [TimeDayValidationAttribute]
        public string Option1 { get; set; }
        [TimeDayValidationAttribute]
        public string Option2 { get; set; }
        [TimeDayValidationAttribute]
        public string Option3 { get; set; }
        public string Group { get; set; }
    }
}
