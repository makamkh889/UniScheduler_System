

using GraduationProject.core.Filters;
using GraduationProject.core.Repositories;
using GraduationProject.core.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using RouteAttribute = Microsoft.AspNetCore.Mvc.RouteAttribute;
using GraduationProject.core.DataDTO;
using Microsoft.AspNetCore.Authorization;
using System;

namespace GraduationProject.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [HandelError]
    [ServiceFilter(typeof(ValidationFilterAttribute))]
    [Authorize(Roles = "Student")]
    public class StudentController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public StudentController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // get one student infromation
        [HttpGet("/currentStudentData")]
        public async Task<IActionResult> currentStudentData()
        {
            string NationalId = User.Identity.Name;

            Student student = await _unitOfWork.Students.Get(e => e.NationalId == NationalId);
            if (student == null)
            {
                return NotFound(new { message = "Student not found" });
            }
            var PassedStudentCourse = await _unitOfWork.StudentCourses.FindAll(e => e.StudentId == student.StudentId && e.RecordedCourse == "Not");
            var CurrentStudentCourse = await _unitOfWork.StudentCourses.FindAll(e => e.StudentId == student.StudentId && e.RecordedCourse == "Recorded");

            // calculate the gpa
            float GPA = 0.0f, cnt = 0;
            if (PassedStudentCourse.Count() > 0)
            {
                foreach (StudentCourse item in PassedStudentCourse)
                {
                    GPA += item.Degree;
                    cnt++;
                }
                if (cnt > 0)    // student my in the first semster and has no courses
                    GPA /= cnt;
            }

            List<string> CourseNames = new List<string>();
            if (CurrentStudentCourse.Count() > 0)
            {
                foreach (StudentCourse item in CurrentStudentCourse)
                {
                    var departments = await _unitOfWork.Departments.FindAll(e => e.CourseId == item.CourseId);
                    if (departments == null) continue;

                    Course Name = await _unitOfWork.Courses.Get(e => e.Department == departments.First().CourseCode);
                    if (Name != null)
                        CourseNames.Add(Name.CourseName);
                }
            }
            StudentDTO StudentInforamtion = new StudentDTO()
            {
                Name = student.StudentName,
                NationalId = student.NationalId,
                Email = student.AcademicEmail,
                Department = student.Department,
                GPA = GPA,
                CurrentCourses = CourseNames,
                AcademicNumber = student.AcademicNumber
            };
            return Ok(StudentInforamtion);
        }
        // prepare course of each student for register
        [HttpGet("/StuedntAcademicRecordCourses")]
        public async Task<IActionResult> StuedntAcademicRecordCourses()
        {
            // get user name (nationalid) for student from user identitiy
            string NationalId = User.Identity.Name;
            Student Student = await _unitOfWork.Students.Get(E => E.NationalId == NationalId);

            List<StudentCourse> StudentCourses = (List<StudentCourse>)await _unitOfWork.StudentCourses.FindAll(e => e.StudentId == Student.StudentId && e.Degree >= 1);

            List<Department> PreviousCourses = new List<Department>();
            if (StudentCourses == null)
            {
                return BadRequest(new { message = "Student has no cources.." });
            }

            // fetech all prevoius courses
            foreach (var item in StudentCourses)
            {
                // get from Department table since the froign key in table student course is from department table and studetns.
                Department previous = await _unitOfWork.Departments.Get(e => e.CourseId == item.CourseId);
                PreviousCourses.Add(previous);
            }
            // return Ok(PreviousCourses);

            // fetch all courses less than or equal current semester.....
            Dictionary<int, List<Department>> AllCourses = new Dictionary<int, List<Department>>();
            Dictionary<string, string> CourseStatus = new Dictionary<string, string>();
            List<StudentRegisterCourses> FinalCourses = new List<StudentRegisterCourses>();

            for (int i = 1; i <= Student.Semester; i++)
            {
                if (i <= 4)
                {
                    List<Department> CoursesPerSemester =
                    (List<Department>)await _unitOfWork.Departments.FindAll(e => e.Semaster == i);

                    AllCourses[i] = (CoursesPerSemester);
                }
                else
                {
                    List<Department> CoursesPerSemester =
                    (List<Department>)await _unitOfWork.Departments.FindAll(
                        e => e.Semaster == i && e.DepartmentName == Student.Department);

                    AllCourses[i] = (CoursesPerSemester);
                }
            }
            // convert from DepartmentCourses to course to fetech courses data
            Dictionary<int, List<Course>> PreFinalCources = new Dictionary<int, List<Course>>();
            for (int i = 1; i <= Student.Semester; i++)
            {
                List<Department> temp = AllCourses[i];
                List<Course> TempCourse = new List<Course>();
                foreach (var item in temp)
                {
                    Course C = await _unitOfWork.Courses.Get(e => e.CourseCode == item.CourseCode);
                    TempCourse.Add(C);
                }
                PreFinalCources[i] = (TempCourse);
            }

            // fillter the course and prepare current courses for register
            for (int i = 1; i <= Student.Semester; i++)
            {
                List<Course> Temp = PreFinalCources[i];
                foreach (var item in Temp)
                {
                    bool found = false;
                    foreach (var course in PreviousCourses)
                    {
                        if (course.CourseCode == item.CourseCode)
                        {
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        bool check = true;
                        if (item.CoursePrerequisites != null)
                        {
                            List<string> Prerequisites = item.CoursePrerequisites.Split(',').ToList();
                            foreach (var prerequisite in Prerequisites)
                            {
                                if (CourseStatus[prerequisite] == "Failed")
                                    check = false;
                            }
                        }
                        StudentRegisterCourses registerCourses = new StudentRegisterCourses();

                        if (check)
                        {
                            var CourseID = await _unitOfWork.Departments.FindAll(e => e.CourseCode == item.CourseCode);
                            if (CourseID == null)
                                return NotFound(new { message = $"Course with code {item.CourseCode} not found." });


                            // check if the course recorded befor
                            StudentCourse studentCourse =
                             await _unitOfWork.StudentCourses.Get(e => e.CourseId == CourseID.First().CourseId &&
                                    e.StudentId == Student.StudentId && e.RecordedCourse == "Recorded");

                            if (studentCourse == null)
                            {
                                registerCourses.CourseCode = item.CourseCode;
                                registerCourses.CourseName = item.CourseName;
                                if (i < Student.Semester)
                                {
                                    registerCourses.status = true;
                                    CourseStatus[item.CourseCode] = "Failed";
                                }
                                else
                                    registerCourses.status = false;
                                var recordedcourceFromDoctor = await _unitOfWork.DoctorCourseHalls.Get(e => e.Department == Student.Department && e.Coursecode == item.CourseCode);
                                if (recordedcourceFromDoctor != null)
                                    FinalCourses.Add(registerCourses);
                            }
                        }
                        else CourseStatus[item.CourseCode] = "Failed";
                    }
                    else CourseStatus[item.CourseCode] = "Passed";
                }
            }
            return Ok(FinalCourses);
        }

        // enroll the student courses 
        [HttpPost("/StudentRecord")]
        public async Task<IActionResult> StudentRecord(List<string> courses)
        {
            if (courses == null || courses.Count == 0)
                return BadRequest(new { message = "No courses provided." });
            string NationalId = User.Identity.Name;
            Student Student = await _unitOfWork.Students.Get(e => e.NationalId == NationalId);

            List<StudentCourse> StudentRecordedCourses =
               (List<StudentCourse>)await _unitOfWork.StudentCourses.FindAll
               (e => e.StudentId == Student.StudentId && e.RecordedCourse == "Recorded");

            int numOfHours = 0;
            foreach (StudentCourse student in StudentRecordedCourses)
            {
                var code = await _unitOfWork.Departments.FindAll(e => e.CourseId == student.CourseId);
                var Hours = await _unitOfWork.Courses.Get(e => e.CourseCode == code.First().CourseCode);
                numOfHours += Hours.CreditHour;
            }
            List<string> RecordedCourses = new List<string>();
            foreach (var CourseCode in courses)
            {
                Department Course = await _unitOfWork.Departments.Get(e => e.CourseCode == CourseCode);
                if (Course == null)
                    return NotFound(new { message = $"Course with code {CourseCode} not found." });


                // check if the course recorded befor
                StudentCourse studentCourse = await _unitOfWork.StudentCourses.Get(e => e.CourseId == Course.CourseId &&
            e.StudentId == Student.StudentId && e.RecordedCourse == "Recorded");

                if (studentCourse != null)
                {
                    RecordedCourses.Add(CourseCode);
                    continue;
                }

                Course Hour = await _unitOfWork.Courses.Get(e => e.CourseCode == CourseCode);


                numOfHours += Hour.CreditHour;

                StudentCourse StudentCourse = new StudentCourse
                {
                    StudentId = Student.StudentId,
                    CourseId = Course.CourseId,
                    RecordedCourse = "Recorded",
                    Degree = 0
                };

                await _unitOfWork.StudentCourses.Add(StudentCourse);
            }

            if (numOfHours > 18 && Student.Semester < 7)
                return BadRequest(new { message = "Total number of hours exceeds 18." });


            _unitOfWork.Save();
            return Ok(new { message = "Student successfully registered." });
        }

        // praper schedulr for student
        [HttpGet("/GetStudentSchedule")]
        public async Task<IActionResult> GetStudentSchedule()
        {
            // get user name (nationalid) for student from user identitiy
            string NationalId = User.Identity.Name;
            Student student = await _unitOfWork.Students.Get(e => e.NationalId == NationalId);

            List<StudentCourse> RecordedCourses =
               (List<StudentCourse>)await _unitOfWork.StudentCourses.FindAll
               (e => e.StudentId == student.StudentId && e.RecordedCourse == "Recorded");

            if (RecordedCourses == null)
            {
                return BadRequest(new { message = "Student has no courses recorded." });
            }
            List<StudentSechdule> Schedule = new List<StudentSechdule>();

            foreach (var Course in RecordedCourses)
            {
                // get course code from department since the relation in student between student and deprtment course code
                Department department = await _unitOfWork.Departments.Get(e => e.CourseId == Course.CourseId);
                if (department == null) continue;// courese dosn't exist in department table then its error in database

                // get the hall for student with course and department
                DoctorCourseHall studentCourse = await _unitOfWork.DoctorCourseHalls.Get(e => e.IsCompeleted == true &&
                 e.Coursecode == department.CourseCode && e.Department == student.Department);

                if (studentCourse == null) continue;// courese dosn't exist in department table then its error in database

                Doctor doctor = await _unitOfWork.Doctors.Get(e => e.DoctorId == studentCourse.DoctorId);
                var course = await _unitOfWork.Courses.FindAll(e => e.CourseCode == department.CourseCode);
                Hall hall = await _unitOfWork.Halls.Get(e => e.HallId == studentCourse.HallId);
                StudentSechdule s = new StudentSechdule()
                {
                    DoctorName = doctor.DoctorName,
                    CourseCode = course.First().CourseCode,
                    CourseName = course.First().CourseName,
                    Hall = hall.HallName,
                    Day = studentCourse.Day,
                    Time = studentCourse.FirstHour,
                    Department = studentCourse.Department
                };
                Schedule.Add(s);
            }
            return Ok(Schedule);
        }

        [HttpGet("/GetStudentAcademicRecord")]
        public async Task<IActionResult> GetStudentAcademicRecord()
        {
            string NationalId = User.Identity.Name;
            Student student = await _unitOfWork.Students.Get(e => e.NationalId == NationalId);
            if (student == null)
            {
                return Ok(new { message = "Student not found" });
            }
            float GPA = 0.0f, cnt = 0.0f;
            var courses = (List<StudentCourse>)await _unitOfWork.StudentCourses.FindAll(
                e => e.StudentId == student.StudentId && e.RecordedCourse == "Not");
            List<StudentCoursesInFo> results = new List<StudentCoursesInFo>();
            foreach (var item in courses)
            {
                StudentCoursesInFo obj = new StudentCoursesInFo();
                var department = await _unitOfWork.Departments.Get(e => e.CourseId == item.CourseId);
                var course = await _unitOfWork.Courses.FindAll(e => e.CourseCode == department.CourseCode);
                obj.CourseName = course.First().CourseName;
                obj.CourseCode = course.First().CourseCode;
                obj.Level = (department.Semaster + 1) / 2;
                obj.Degree = item.Degree;
                results.Add(obj);
            }
            return Ok(results);
        }

    }
}