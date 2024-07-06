

using GraduationProject.core.Filters;
using GraduationProject.core.Repositories;
using GraduationProject.core.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using RouteAttribute = Microsoft.AspNetCore.Mvc.RouteAttribute;
using GraduationProject.core.DataDTO;
using GraduationProject.core.DTO.UserInput;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;

namespace GraduationProject.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [HandelError]
    [ServiceFilter(typeof(ValidationFilterAttribute))]
    [Authorize(Roles = "Doctor")]
    public class DoctorController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        string[] Days = { "sat", "sun", "mon", "teus", "wed", "thurs" };
        public DoctorController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet("/currentDoctorData")]
        public async Task<IActionResult> currentDoctorData()
        {
            string NationalId = User.Identity.Name;
            Doctor doctor = await _unitOfWork.Doctors.Get(e => e.NationalId == NationalId);
            DoctorDTO data = new DoctorDTO()
            {
                Name = doctor.DoctorName,
                Email = doctor.AcademicEmail,
                Department = doctor.Department,
                CourseNames = null
            };
            return Ok(data);
        }

        // enroll the courses with it's options
        [HttpPost("/EnrollDocotrCoruses")]
        public async Task<IActionResult> EnrollDocotrCoruses(Options options)
        {
            // Get docotr name (national id) from user identitiy
            string DoctorNationaId = User.Identity.Name;
            // Check if options object is null
            if (options == null)
            {
                return BadRequest(new { message = "Invalid request data" });
            }

            // if the course  is exist  find the course with course code and departmetn

            DoctorCourseHall doctorCourse = await _unitOfWork.DoctorCourseHalls.Get(e => e.Coursecode == options.CourseCode && e.Department == options.Group);

            if (doctorCourse != null)
            {
                return BadRequest(new { message = $"Course {options.CourseCode} already registered" });
            }

            Doctor doctor = await _unitOfWork.Doctors.Get(e => e.NationalId == DoctorNationaId);
            if (doctor == null)
            {
                return NotFound(new { message = "Doctor not found" });
            }

            // check if the Course in the correct semester and level
            var CourseDefualtSemester = await _unitOfWork.Departments.Get(e => e.CourseCode == options.CourseCode);
            if (CourseDefualtSemester == null)
            {
                return NotFound(new { message = "Course not found" });
            }
            int level = (CourseDefualtSemester.Semaster + 1) / 2;
            var DeparmentDefualtSemester = await _unitOfWork.CurrentDepartments.Get(e => e.Name == options.Group && e.Level == level);
            if (DeparmentDefualtSemester == null)
            {
                return BadRequest(new { message = "Course not in the correct level or semester" });
            }

            Course course = await _unitOfWork.Courses.Get(e => e.CourseCode == options.CourseCode);

            if (course == null)
            {
                return NotFound(new { message = "Course not found" });
            }
            var department = await _unitOfWork.CurrentDepartments.FindAll(e => e.Name == options.Group);
            if (department.IsNullOrEmpty())
            {
                return NotFound(new { message = "Invalid department or group" });
            }

            if (await ValidOptions(options.Option1) == false)
            {
                return BadRequest($"Invalid {options.Option1}");
            }

            if (await ValidOptions(options.Option2) == false)
            {
                return BadRequest($"Invalid {options.Option2}");
            }

            if (await ValidOptions(options.Option3) == false)
            {
                return BadRequest($"Invalid {options.Option3}");
            }

            doctorCourse = new DoctorCourseHall();
            doctorCourse.DoctorId = doctor.DoctorId;
            doctorCourse.CourseId = course.CourseId;
            doctorCourse.Option1 = options.Option1;
            doctorCourse.Option2 = options.Option2;
            doctorCourse.Option3 = options.Option3;
            doctorCourse.Department = options.Group;
            doctorCourse.Coursecode = options.CourseCode;

            await _unitOfWork.DoctorCourseHalls.Add(doctorCourse);
            _unitOfWork.Save();
            return Ok("Doctor enrolled succesfully.");
        }

        async Task<bool> ValidOptions(string option)
        {
            string[] op = option.Split(' ');
            string day = op[1];
            string time = op[0];
            if ((time[0] != '0' && time[0] != '1') || (time[1] < '0' || time[1] > '9') || time[2] != ':' ||
                (time[3] < '0' || time[3] > '9') || (time[4] < '0' || time[4] > '9') || Days.Contains(day) == false)
                return false;

            return true;

        }

        // update  docotr options for courses
        [HttpPut("/UpdateOptions")]
        public async Task<IActionResult> UpdateOptions(Options options)
        {

            // Get docotr name (national id) from user identitiy
            string DoctoeNationaId = User.Identity.Name;

            Course course = await _unitOfWork.Courses.Get(e => e.CourseCode == options.CourseCode);
            if (course == null)
            {
                return NotFound(new { message = "Course not found" });
            }
            Doctor doctor = await _unitOfWork.Doctors.Get(e => e.NationalId == DoctoeNationaId);
            if (doctor == null)
            {
                return NotFound(new { message = "Doctor not found" });
            }
            var department = await _unitOfWork.CurrentDepartments.FindAll(e => e.Name == options.Group);
            if (department.IsNullOrEmpty())
            {
                return NotFound(new { message = "Invalid department or group" });
            }

            // if the course  is exist  find the course with course code and departmetn
            DoctorCourseHall doctorCourse = await _unitOfWork.DoctorCourseHalls.Get(e => e.Coursecode == options.CourseCode && e.Department == options.Group);

            if (doctorCourse == null)
            {
                return NotFound(new { message = $"Doctor with national id = {DoctoeNationaId} dosn't register this course, please register the course first." });
            }

            if (options.Option1 != null)
                doctorCourse.Option1 = options.Option1;

            if (options.Option2 != null)
                doctorCourse.Option2 = options.Option2;

            if (options.Option3 != null)
                doctorCourse.Option3 = options.Option3;

            _unitOfWork.DoctorCourseHalls.Update(doctorCourse);
            _unitOfWork.Save();

            return Ok(new { message = "Course updated successfully." });

        }


        // get all doctor coueses
        [HttpGet("/GetDoctorCourses")]
        public async Task<IActionResult> GetDoctorCourses()
        {
            // Get docotr name (national id) from user identitiy
            string NationalId = User.Identity.Name;
            Doctor doctor = await _unitOfWork.Doctors.Get(e => e.NationalId == NationalId);

            if (doctor == null)
            {
                return NotFound(new { message = "Doctor not found" });
            }

            // get doctor courses
            List<DoctorCourseHall> DoctorCourses =
             (List<DoctorCourseHall>)await _unitOfWork.DoctorCourseHalls.FindAll(e => e.DoctorId == doctor.DoctorId);

            //  get course names;
            List<DoctorCourses> CourseNames = new List<DoctorCourses>();

            foreach (var item in DoctorCourses)
            {
                Course TempCourse = await _unitOfWork.Courses.Get(e => e.CourseId == item.CourseId);
                if (TempCourse != null)
                {
                    DoctorCourses Data = new DoctorCourses();
                    Data.CourseName = TempCourse.CourseName;
                    Data.CourseCode = TempCourse.CourseCode;
                    Data.Gruop = item.Department;
                    Data.Option1 = item.Option1;
                    Data.Option2 = item.Option2;
                    Data.Option3 = item.Option3;
                    CourseNames.Add(Data);
                }
            }
            if (CourseNames.IsNullOrEmpty())
                return Ok(new { message = "There is no courses." });
            return Ok(CourseNames);
        }



        // get docotr sechdule
        [HttpGet("/GetDoctorSechdule")]
        public async Task<IActionResult> GetDoctorSechdule()
        {
            // Get docotr name (national id) from user identitiy
            string NationalId = User.Identity.Name;

            Doctor doctor = await _unitOfWork.Doctors.Get(e => e.NationalId == NationalId);

            if (doctor == null)
            {
                return NotFound(new { message = "Doctor not found" });
            }

            // get doctor courses
            List<DoctorCourseHall> DoctorCourses =
             (List<DoctorCourseHall>)await _unitOfWork.DoctorCourseHalls.FindAll(e => e.DoctorId == doctor.DoctorId);
            List<DoctorSechdule> result = new List<DoctorSechdule>();

            // set the data about each course 
            foreach (DoctorCourseHall item in DoctorCourses)
            {
                Course Course = await _unitOfWork.Courses.Get(e => e.CourseId == item.CourseId && e.CourseCode == item.Coursecode);
                Hall Hall = await _unitOfWork.Halls.Get(e => e.HallId == item.HallId);

                if (Course != null && item.IsCompeleted == true)
                {
                    DoctorSechdule temp = new DoctorSechdule();
                    temp.Name = Course.CourseName;
                    temp.Time = item.FirstHour;
                    temp.Day = item.Day;
                    temp.Hall = Hall.HallName;
                    temp.Department = item.Department;

                    result.Add(temp);
                }

            }
            return Ok(result);

        }
        [HttpGet("/GetNumberOfStudent")]
        public async Task<IActionResult> GetNumberOfStudent(string Code)
        {
            int cnt = 0;
            string NationalId = User.Identity.Name;
            var docotr = await _unitOfWork.Doctors.Get(e => e.NationalId == NationalId);
            var courses = (List<DoctorCourseHall>) await _unitOfWork.DoctorCourseHalls.FindAll
            (e => e.Coursecode == Code && e.DoctorId == docotr.DoctorId);

            foreach (var item in courses)
            {
                var checkForCourse = await _unitOfWork.Departments.Get(e => e.CourseCode == Code);
                int level = (checkForCourse.Semaster + 1) / 2;
                Department course = new Department();
                if (level == 1)
                    course = await _unitOfWork.Departments.Get(e => e.CourseCode == Code && e.DepartmentName == "first");
                else if (level == 2)
                    course = await _unitOfWork.Departments.Get(e => e.CourseCode == Code && e.DepartmentName == "second");
                else
                    course = await _unitOfWork.Departments.Get(e => e.CourseCode == Code && e.DepartmentName == item.Department);

                var stutdents = await _unitOfWork.StudentCourses.FindAll(e => e.CourseId == course.CourseId && e.RecordedCourse != "Not");
                cnt += stutdents.Count();
            }
            return Ok(cnt);

        }
    }
}