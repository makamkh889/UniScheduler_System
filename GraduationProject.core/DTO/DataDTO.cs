using GraduationProject.core.DTO.UserInput;

namespace GraduationProject.core.DataDTO
{

    public class DoctorDTO : User
    {
        public string Department { get; set; }
        public List<DoctorCourses> CourseNames { get; set; }
    }
    public class DoctorSechdule
    {
        public string Name { get; set; }
        public string Day { get; set; }
        public string Time { get; set; }
        public string Hall { get; set; }
        public string Department { get; set; }

    }
    public class StudentDTO : User
    {
        public string Department { get; set; }
        public string AcademicNumber { get; set; }
        public double GPA { get; set; }
        public List<string> CurrentCourses { get; set; }
    }
    public class StudentSechdule
    {

        public int Level { get; set; }
        public string CourseName { get; set; }
        public string CourseCode { get; set; }
        public string DoctorName { get; set; }
        public string Day { get; set; }
        public string Time { get; set; }
        public string Hall { get; set; }
        public string Department { get; set; }
    }
    public class DoctorCourses
    {
        public string CourseName { get; set; }
        public string CourseCode { get; set; }
        public string Option1 { get; set; }
        public string Option2 { get; set; }
        public string Option3 { get; set; }
        public string Gruop { get; set; }
    }
    public class InValidCourses
    {
        public string CourseName { get; set; }
        public string CourseCode { get; set; }
        public string DoctorName { get; set; }
        public string DoctorEmail { get; set; }
        public string Department { get; set; }
        public string Option1 { get; set; }
        public string Option2 { get; set; }
        public string Option3 { get; set; }
    }
    public class StudentRegisterCourses
    {
        public string CourseName { get; set; }
        public string CourseCode { get; set; }
        public bool status { get; set; }
    }
    public class Levels
    {
        public List<string> Group { get; set; }
        public int Level { get; set; }

    }
    public class StudentCoursesInFo
    {
        public string CourseName { get; set; }
        public string CourseCode { get; set; }
        public int Level { get; set; }
        public float Degree { get; set; }

    }
    public class NewCourse
    {
        public string CourseName { get; set; }
        public string CourseCode { get; set; }
        public string Prerequisites { get; set; }
        public int CreditHour { get; set; }
    }
    public class NewCourseperDepartments
    {
        public string CourseCode { get; set; }
        public string DepartmentName { get; set; }
        public int Semaster { get; set; }
    }
    public class CourseInformation
    {
        public string CourseName { get; set; }
        public string CourseCode { get; set; }
        public string Prerequisites { get; set; }
        public int CreditHour { get; set; }
        public List<NewCourseperDepartments> courses { get; set; }
    }

}
