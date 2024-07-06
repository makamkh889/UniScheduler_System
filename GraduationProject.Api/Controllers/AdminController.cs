
using GraduationProject.core.DataDTO;
using GraduationProject.core.DTO.UserInput;
using GraduationProject.core.Filters;
using GraduationProject.core.Models;
using GraduationProject.core.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Drawing.Charts;
using GrdauationProject.EF;
using System.Security.Cryptography;
using System.Text.RegularExpressions;



namespace GraduationProject.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [HandelError]
    [ServiceFilter(typeof(ValidationFilterAttribute))]
    //[Authorize(Roles = "Admin,SuperAdmin")]
    public class AdminController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        Registeration RegisterRoleObj = new Registeration();

        public UserManager<ApplicationUser> userManager { get; }
        public RoleManager<IdentityRole> roleManager { get; }

        public AdminController(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _unitOfWork = unitOfWork;
            this.userManager = userManager;
            this.roleManager = roleManager;
        }

        #region    ------- Admin Managment -------
        [HttpGet("/currentAdminData")]
        public async Task<IActionResult> currentAdminData()
        {
            string NationalId = User.Identity.Name;
            Admin admin = await _unitOfWork.Admins.Get(e => e.NationalId == NationalId);
            if (admin == null)
            {
                admin.AdminName = "Super Admin";
                admin.AcademicEmail = "Academic Email";
                admin.NationalId = NationalId;
            }
            User data = new User()
            {
                Name = admin.AdminName,
                Email = admin.AcademicEmail,
                NationalId = admin.NationalId
            };
            return Ok(data);
        }
        // add new admin
        [HttpPost("/AddAdmin")]
        public async Task<IActionResult> AddAdmin(User admin)
        {
            var existingAdmin = await _unitOfWork.Admins.Get(e => e.NationalId == admin.NationalId);
            if (existingAdmin != null)
            {
                return BadRequest(new { message = "National ID already exists." });
            }

            if (await _unitOfWork.Admins.Get(e => e.AcademicEmail == admin.Email) != null)
            {
                return BadRequest(new { message = "Academic Email already exists." });
            }
            Admin newAdmin = new()
            {
                AdminName = admin.Name,
                NationalId = admin.NationalId,
                AcademicEmail = admin.Email,
            };

            await _unitOfWork.Admins.Add(newAdmin);
            _unitOfWork.Save();

            //Register admin
            RegisterUserDTO Data = new RegisterUserDTO()
            {
                UserName = admin.NationalId,
                Password = admin.NationalId + "admin"
            };
            List<string> RegisterResult = await RegisterRoleObj.Register(Data, roleManager, "Admin", userManager);

            if (RegisterResult.Count() == 1 && RegisterResult[0] == "True")
                return Ok(new { message = "Admin added succesfully..." });

            return BadRequest(RegisterResult);
            return Ok(new { message = "Admin added succesfully.." });
        }

        // Get all admins
        [HttpGet("/GetAdmins")]
        public async Task<IActionResult> GetAdmins()
        {
            var Admins = await _unitOfWork.Admins.GetAll();
            return Ok(Admins);
        }

        [HttpPut("/EditAdmin")]
        public async Task<IActionResult> EditAdmin(User admin)
        {
            Admin existingAdmin = await _unitOfWork.Admins.Get(e => e.NationalId == admin.NationalId);
            if (existingAdmin == null)
            {
                return NotFound(new { message = "National ID not found." });
            }

            existingAdmin.AdminName = admin.Name;
            existingAdmin.AcademicEmail = admin.Email;

            _unitOfWork.Admins.Update(existingAdmin);
            _unitOfWork.Save();

            return Ok(new { message = "Admin edited successfully." });
        }


        [HttpDelete("/DeleteAdmin")]
        public async Task<IActionResult> DeleteAdmin(NationalID adminID)
        {
            Admin admin = await _unitOfWork.Admins.Get(e => e.NationalId == adminID.NationalId);
            if (admin == null)
                return NotFound(new { message = "Admin not found." });

            // ناقص اتاكد ان الرول دى بتاعت الادمن ادمن مش سوبر ادمن  واعمله هارد اما لو سوبر مينفعش يتمسح

            _unitOfWork.Admins.HardDelete(admin);
            _unitOfWork.Save();
            return Ok(new { message = "Admin Deleted." });

        }

        #endregion

        #region  ------- Current departments ---------

        [HttpPost("/AddNewDepartmentOrGroup")]
        public async Task<IActionResult> AddNewDepartmentOrGroup(string Name, int Level)
        {
            List<CurrentDepartment> departments = (List<CurrentDepartment>)await _unitOfWork.CurrentDepartments.GetAll();
            foreach (var item in departments)
            {
                if (item.Name == Name && item.Level == Level)
                {
                    return Ok(new { message = $"The {Name} department arleady exist." });
                }
            }
            CurrentDepartment currentDepartment = new CurrentDepartment()
            {
                Name = Name,
                Level = Level
            };
            await _unitOfWork.CurrentDepartments.Add(currentDepartment);
            _unitOfWork.Save();

            return Ok(new { meassage = "New department added." });
        }

        #endregion

        #region ------ Doctor Managment -------
        [HttpPost("/AddDoctor")]
        public async Task<IActionResult> AddDoctor(UserDoctor doctor)
        {
            var existingDoctor = await _unitOfWork.Doctors.Get(e => e.NationalId == doctor.NationalId);
            if (existingDoctor != null)
            {
                return BadRequest(new { message = "National ID already exists." });
            }

            Doctor newDoctor = new()
            {
                DoctorName = doctor.Name,
                AcademicEmail = doctor.Email,
                Department = doctor.Department,
                NationalId = doctor.NationalId
            };

            await _unitOfWork.Doctors.Add(newDoctor);
            _unitOfWork.Save();

            RegisterUserDTO Data = new RegisterUserDTO()
            {
                UserName = doctor.NationalId,
                Password = doctor.NationalId + "doctor"
            };

            List<string> RegisterResult = await RegisterRoleObj.Register(Data, roleManager, "Doctor", userManager);

            if (RegisterResult.Count() == 1 && RegisterResult[0] == "True")
                return Ok(new { message = "Doctor added succesfully..." });

            return BadRequest(RegisterResult);
            return Ok(new { message = "Doctor added succesfully..." });
        }

        // get information about all doctors
        [HttpGet("/GetAllDocotrs")]
        public async Task<IActionResult> GetAllDoctors()
        {

            // get all doctor nationalId
            List<Doctor> doctors = (List<Doctor>)await _unitOfWork.Doctors.GetAll();
            List<DoctorDTO> result = new List<DoctorDTO>();

            // get information foreach doctor
            foreach (var doctor in doctors)
            {
                // get doctor courses
                var DoctorCourses = await _unitOfWork.DoctorCourseHalls.FindAll(e => e.DoctorId == doctor.DoctorId);

                // set doctor courses data
                List<DoctorCourses> courseNames = new List<DoctorCourses>();

                foreach (var item in DoctorCourses)
                {
                    Course tempCourse = await _unitOfWork.Courses.Get(a => a.CourseId == item.CourseId);
                    if (tempCourse != null)
                    {
                        DoctorCourses Data = new DoctorCourses();
                        Data.CourseName = tempCourse.CourseName;
                        Data.Option1 = item.Option1;
                        Data.Option2 = item.Option2;
                        Data.Option3 = item.Option3;
                        Data.CourseCode = item.Coursecode;
                        Data.Gruop = item.Department;
                        courseNames.Add(Data);
                    }
                }

                DoctorDTO doctorDTO = new DoctorDTO();

                doctorDTO.Name = doctor.DoctorName;
                doctorDTO.NationalId = doctor.NationalId;
                doctorDTO.Department = doctor.Department;
                doctorDTO.Email = doctor.AcademicEmail;
                doctorDTO.CourseNames = courseNames;

                result.Add(doctorDTO);
            }

            return Ok(result);
        }


        // update  docotr options for courses
        [HttpPut("/UpdateDoctorOptions")]
        public async Task<IActionResult> UpdateDoctorOptions(DoctorOptions options)
        {
            Course course = await _unitOfWork.Courses.Get(e => e.CourseCode == options.CourseCode);
            if (course == null)
            {
                return NotFound(new { message = "Course not found" });
            }
            Doctor doctor = await _unitOfWork.Doctors.Get(e => e.NationalId == options.NationalId);
            if (doctor == null)
            {
                return NotFound(new { message = "Doctor not found" });
            }
            var department = await _unitOfWork.CurrentDepartments.FindAll(e => e.Name == options.Group);
            if (department.IsNullOrEmpty())
            {
                return NotFound(new { message = "Invalid department ot group" });
            }

            // if the course  is exist  find the course with course code and departmetn
            DoctorCourseHall doctorCourse = await _unitOfWork.DoctorCourseHalls.Get(e => e.Coursecode == options.CourseCode && e.Department == options.Group);

            if (doctorCourse == null)
            {
                return NotFound(new { message = $"Doctor with national id = {options.NationalId} dosn't register this course, please register the course first." });
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

        // update  docotr options for courses
        [HttpPost("/AddDoctorOptions")]
        public async Task<IActionResult> AddDoctorOptions(DoctorOptions options)
        {

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

            Doctor doctor = await _unitOfWork.Doctors.Get(e => e.NationalId == options.NationalId);
            if (doctor == null)
            {
                return NotFound(new { message = "Doctor not found" });
            }

            Course course = await _unitOfWork.Courses.Get(e => e.CourseCode == options.CourseCode);

            if (course == null)
            {
                return NotFound(new { message = "Course not found" });
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

            var newDoctorCourse = new DoctorCourseHall
            {
                Coursecode = options.CourseCode,
                CourseId = course.CourseId,
                DoctorId = doctor.DoctorId,
                Option1 = options.Option1,
                Option2 = options.Option2,
                Option3 = options.Option3,
                Department = options.Group
            };
            await _unitOfWork.DoctorCourseHalls.Add(newDoctorCourse);
            _unitOfWork.Save();
            return Ok(new { message = "Done Succesfully..." });

        }

        [HttpDelete("/DeleteDoctoroption")]
        public async Task<IActionResult> DeleteDoctoroption(NationalID NationalID, string courseCode, string department)
        {
            Doctor doctor = await _unitOfWork.Doctors.Get(e => e.NationalId == NationalID.NationalId);
            if (doctor == null)
            {
                return NotFound(new { message = "Doctor not found" });
            }
            Course course = await _unitOfWork.Courses.Get(e => e.CourseCode == courseCode);

            if (course == null)
            {
                return NotFound(new { message = "Course not found" });
            }
            var departments = await _unitOfWork.CurrentDepartments.FindAll(e => e.Name == department);
            if (departments.IsNullOrEmpty())
            {
                return NotFound(new { message = "Invalid department ot group" });
            }
            DoctorCourseHall doctorCourse = await _unitOfWork.DoctorCourseHalls.Get(e => e.Coursecode == courseCode && e.Department == department);
            _unitOfWork.DoctorCourseHalls.HardDelete(doctorCourse);
            _unitOfWork.Save();
            return Ok(new { message = "Doctor Options for this course deleted.." });
        }
        // doctor soft delete
        [HttpDelete("/DeleteDoctor")]
        public async Task<IActionResult> DeleteDoctor(NationalID NationalID)
        {
            Doctor doctor = await _unitOfWork.Doctors.Get(e => e.NationalId == NationalID.NationalId);
            if (doctor == null)
            {
                return NotFound(new { message = "Doctor not found" });
            }
            doctor.IsDeleted = true;
            doctor = _unitOfWork.Doctors.SoftDelete(doctor);

            List<DoctorCourseHall> Doctors =
             (List<DoctorCourseHall>)await _unitOfWork.DoctorCourseHalls.FindAll(e => e.DoctorId == doctor.DoctorId);

            foreach (DoctorCourseHall item in Doctors)
            {
                _unitOfWork.DoctorCourseHalls.HardDelete(item);
            }
            _unitOfWork.Save();
            return Ok(new { message = "Doctor deleted." });
        }

        #endregion

        #region -----------Send Email -------------------

        [HttpPost("/SendEmail")]

        public async Task<IActionResult> SendEmail(string email, string subject, string body)
        {
            EmailModel emailModel = new EmailModel();
            email = "sana.20377387@compit.aun.edu.eg";
            string result = emailModel.SendEmailTo(email, subject, emailModel.CustomBody(body));
            return Ok(result);
        }

        #endregion




        #region------------------------ Start Dealing With Hall ----------------- 

        // add new hall 
        [HttpPost("/AddNewHall")]
        public async Task<IActionResult> AddNewHall(string HallName, int Capacity)
        {
            Hall hall = new Hall();
            hall.HallName = HallName;
            hall.Capacity = Capacity;
            await _unitOfWork.Halls.Add(hall);
            _unitOfWork.Save();
            return Ok(new { message = "New hall added succesfully." });
        }
        [HttpGet("/GetAllHall")]
        public async Task<IActionResult> GetAllHall()
        {
            return (IActionResult)await _unitOfWork.Halls.GetAll();
        }

        // update hall name for specific course 
        // الفانكشن دى بتبقى كتعديل على الجدول تعديل المدرج
        [HttpPut("/UpdataHall")]
        public async Task<IActionResult> UpdataHall(string courseCode, string hallName, string department)
        {
            Course Course = await _unitOfWork.Courses.Get(e => e.CourseCode == courseCode);
            if (Course == null)
            {
                return NotFound(new { message = "Course Dosen't exist." });
            }

            Hall Hall = await _unitOfWork.Halls.Get(e => e.HallName == hallName);
            if (Hall == null)
            {
                return NotFound(new { message = "Hall not found." });
            }

            DoctorCourseHall doctorCourseHall =
            await _unitOfWork.DoctorCourseHalls.Get(e => e.CourseId == Course.CourseId && e.Department == department);

            if (doctorCourseHall == null)
            {
                return NotFound(new { message = "Course dosen't registerd." });
            }

            // get all courses which has the same hall in the same day and match at least one of the hours of any course
            List<DoctorCourseHall> AllCourses =
                (List<DoctorCourseHall>)await _unitOfWork.DoctorCourseHalls.FindAll(e => e.HallId == Hall.HallId
                    && e.Day == doctorCourseHall.Day && e.FirstHour == doctorCourseHall.FirstHour);

            if (AllCourses.Count() > 0)
            {
                return BadRequest(new { message = "There is another lecture in the same hall in the same time." });
            }

            doctorCourseHall.HallId = Hall.HallId;
            _unitOfWork.DoctorCourseHalls.Update(doctorCourseHall);
            _unitOfWork.Save();

            return Ok(new { message = "Hall updated succesfully..." });
        }

        #endregion


        #region--------------------------------Start Dealing With Student------------------------------------

        //  add  one student  each time

        [HttpPost("/AddStudent")]
        public async Task<IActionResult> AddStudent(UserStudent student)
        {
            var existingstudent = await _unitOfWork.Students.Get(e => e.NationalId == student.NationalId);
            if (existingstudent != null)
            {
                return BadRequest(new { message = "National ID already exists." });
            }

            if (await _unitOfWork.Departments.FindAll(e => e.DepartmentName == student.Department) == null)
            {
                return BadRequest(new { message = "incorrect department." });
            }


            Student newStudent = new()
            {

                StudentName = student.Name,
                AcademicEmail = student.Email,
                NationalId = student.NationalId,
                AcademicNumber = student.AcademicNumber,
                Department = student.Department,
                Level = student.Level,
                Semester = student.Semester,
            };

            await _unitOfWork.Students.Add(newStudent);
            _unitOfWork.Save();


            //Register student
            RegisterUserDTO Data = new RegisterUserDTO()
            {
                UserName = student.NationalId,
                Password = student.NationalId + "student"
            };

            List<string> RegisterResult = await RegisterRoleObj.Register(Data, roleManager, "Student", userManager);

            if (RegisterResult.Count() == 1 && RegisterResult[0] == "True")
                return Ok(new { meaasge = "Student added succesfully..." });

            return BadRequest(RegisterResult);
            return Ok(new { message = "Student added succesfully..." });
        }


        // insert student from excel sheet

        [HttpPost("/uploadExcelsheetForStudent")]
        public async Task<IActionResult> UploadExcelsheetForStudent(IFormFile file)
        {
            using var mem = new MemoryStream();
            await file.CopyToAsync(mem);

            var data = new List<List<string>>();
            List<Student> invalidStudents = new List<Student>();

            using (var document = SpreadsheetDocument.Open(mem, false))
            {
                var workbookPart = document.WorkbookPart;
                var sheets = workbookPart.Workbook.Descendants<DocumentFormat.OpenXml.Spreadsheet.Sheet>();
                List<string> studentProperties = new List<string>();
                List<string> FinalResult = new List<string>();


                foreach (var sheet in sheets)
                {
                    var worksheetPart = (WorksheetPart)workbookPart.GetPartById(sheet.Id);
                    var sheetData = worksheetPart.Worksheet.Elements<SheetData>().First();
                    bool check = false;

                    foreach (var row in sheetData.Elements<Row>())
                    {
                        var obj = new List<string>();

                        foreach (var cell in row.Elements<Cell>())
                        {
                            var value = GetCellValue(workbookPart, cell);
                            obj.Add(value);
                        }

                        if (!check) studentProperties = obj;
                        else
                        {
                            // Add student to database
                            var result = setStudentValue(obj, studentProperties);
                            var student = result[0] as Student;
                            var courses = result[1] as Dictionary<string, float>;

                            if (student == null || courses == null)
                            {
                                return BadRequest(new { message = "Invalid student data" });
                            }

                            // vlaid student data
                            var existingstudent = await _unitOfWork.Students.Get(e => e.NationalId == student.NationalId);
                            var existingdepartment = await _unitOfWork.CurrentDepartments.FindAll(e => e.Name == student.Department && e.Level == student.Level);
                            bool compelete = false;
                            if (Regex.IsMatch(student.NationalId, @"^\d+$") == false || student.NationalId.Length != 14)
                            {
                                return BadRequest(new { Message = $"Invalid National Id of student {student.NationalId}" });
                            }
                            if (student.AcademicEmail.EndsWith("@compit.aun.edu.eg") == false)
                            {
                                return BadRequest(new { Message = $"Invalid Academic Email for student{student.StudentName}" });
                            }
                            if (existingstudent == null && existingdepartment != null)
                            {
                                await _unitOfWork.Students.Add(student);
                                if (student == null) return BadRequest(student);

                                _unitOfWork.Save();
                                compelete = true;
                                // register student for login
                                //Register student
                                RegisterUserDTO Data = new RegisterUserDTO()
                                {
                                    UserName = student.NationalId,
                                    Password = student.NationalId + "student"
                                };

                                List<string> RegisterResult = await RegisterRoleObj.Register(Data, roleManager, "Student", userManager);

                                if (RegisterResult.Count != 1 && RegisterResult[0] != "True")
                                    return BadRequest(RegisterResult);

                            }

                            if (compelete)
                            {
                                // register student courses
                                foreach (var Kvp in courses)
                                {
                                    Department courseId = new Department();
                                    var someCourses = await _unitOfWork.Departments.FindAll(e => e.CourseCode == Kvp.Key);
                                    if (someCourses.Count() != 1)
                                    {
                                        courseId = await _unitOfWork.Departments.Get(e => e.CourseCode == Kvp.Key && e.DepartmentName == student.Department);
                                    }
                                    else if (someCourses.Count() == 1)
                                    {
                                        courseId = await _unitOfWork.Departments.Get(e => e.CourseCode == Kvp.Key);
                                    }
                                    Student studentId = await _unitOfWork.Students.Get(e => e.NationalId == student.NationalId);
                                    // return BadRequest(studentId);
                                    if (courseId != null && studentId != null)
                                    {

                                        var Studentcourses = new StudentCourse();
                                        Studentcourses.StudentId = studentId.StudentId;
                                        Studentcourses.CourseId = courseId.CourseId;
                                        Studentcourses.Degree = Kvp.Value;
                                        Studentcourses.RecordedCourse = "Not";
                                        await _unitOfWork.StudentCourses.Add(Studentcourses);
                                        if (Studentcourses == null) return BadRequest(studentId);
                                        _unitOfWork.Save();
                                    }
                                }
                            }
                            else invalidStudents.Add(student);
                        }
                        check = true;
                    }
                }
            }
            return Ok(invalidStudents);
        }



        // Helper method to get cell value
        private string GetCellValue(WorkbookPart workbookPart, Cell cell)
        {
            SharedStringTablePart stringTablePart = workbookPart.GetPartsOfType<SharedStringTablePart>().FirstOrDefault();
            if (cell.DataType != null && cell.DataType == CellValues.SharedString)
            {
                if (stringTablePart != null)
                {
                    SharedStringItem[] items = stringTablePart.SharedStringTable.Elements<SharedStringItem>().ToArray();
                    if (int.TryParse(cell.InnerText, out int index))
                    {
                        if (index < items.Length)
                        {
                            return items[index].InnerText;
                        }
                    }
                }
            }
            return cell.InnerText;
        }

        private static List<object> setStudentValue(List<string> data, List<string> student_data)
        {
            Student student = new Student();

            var courses = new Dictionary<string, float> { };
            var result = new List<object>();

            for (int j = 0; j < data.Count; j++)
            {

                switch (student_data[j])
                {
                    case "StudentName":
                        student.StudentName = data[j];
                        break;
                    case "AcademicNumber":

                        student.AcademicNumber = data[j];
                        break;
                    case "NationalId":
                        student.NationalId = data[j];
                        break;
                    case "Department":
                        student.Department = data[j];
                        break;
                    case "AcademicEmail":
                        student.AcademicEmail = data[j];
                        break;
                    case "Level":
                        student.Level = int.Parse(data[j]);
                        break;
                    case "Semester":
                        student.Semester = int.Parse(data[j]);
                        break;
                    default:
                        courses[student_data[j]] = float.Parse(data[j]);
                        break;
                }
            }
            result.Add(student);
            result.Add(courses);

            return result;
        }

        // get one student infromation
        [HttpGet("/GetStudent")]
        public async Task<IActionResult> GetStudent(string AcademicNumber)
        {
            Student student = await _unitOfWork.Students.Get(e => e.AcademicNumber == AcademicNumber);
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
                GPA = Math.Round(GPA, 2),
                CurrentCourses = CourseNames,
                AcademicNumber = student.AcademicNumber
            };
            return Ok(StudentInforamtion);
        }

        // get all Student per one semster in any department or groub
        [HttpGet("/GetStudents")]
        public async Task<IActionResult> GetStudents(int Semster, string Department)
        {
            List<Student> Students = (List<Student>)await _unitOfWork.Students.FindAll(e => e.Semester == Semster && e.Department == Department);
            List<StudentDTO> Result = new List<StudentDTO>();

            // loop for each student 
            foreach (Student student in Students)
            {
                var PassedStudentCourse = await _unitOfWork.StudentCourses.FindAll(e => e.StudentId == student.StudentId && e.RecordedCourse == "Not");
                var CurrentStudentCourse = await _unitOfWork.StudentCourses.FindAll(e => e.StudentId == student.StudentId && e.RecordedCourse == "Recorded");

                // calculate the gpa
                float GPA = 0.0f, cnt = 0;


                foreach (StudentCourse item in PassedStudentCourse)
                {
                    GPA += item.Degree;
                    cnt++;
                }
                if (cnt > 0)    // student my in the first semster and has no courses
                    GPA /= cnt;

                List<string> CourseNames = new List<string>();
                foreach (StudentCourse item in CurrentStudentCourse)
                {
                    Department Departments = await _unitOfWork.Departments.Get(e => e.CourseId == item.CourseId);
                    Course Name = await _unitOfWork.Courses.Get(e => e.CourseCode == Departments.CourseCode);
                    CourseNames.Add(Name.CourseName);
                }
                StudentDTO StudentInforamtion = new StudentDTO()
                {
                    Name = student.StudentName,
                    NationalId = student.NationalId,
                    Email = student.AcademicEmail,
                    Department = student.Department,
                    GPA = GPA,
                    CurrentCourses = CourseNames,
                };
                Result.Add(StudentInforamtion);

            }
            return Ok(Result);
        }



        // delete one student
        [HttpDelete("/DeleteStudent")]
        public async Task<IActionResult> DeleteStudent(string AcademicNumber)
        {

            var student = await _unitOfWork.Students.Get(e => e.AcademicNumber == AcademicNumber);
            if (student == null)
            {
                return NotFound(new { message = "student not found." });
            }
            student = _unitOfWork.Students.SoftDelete(student);

            List<StudentCourse> StudentCourse =
             (List<StudentCourse>)await _unitOfWork.StudentCourses.FindAll(E => E.StudentId == student.StudentId);

            foreach (var studentCourse in StudentCourse)
            {
                _unitOfWork.StudentCourses.SoftDelete(studentCourse);
            }
            _unitOfWork.Save();
            return Ok(new { message = "Student Deleted." });
        }

        #endregion



        #region-------------------------Schedule--------------------------

        [HttpGet("/Schedule")]
        public async Task<IActionResult> Schedule()
        {
            List<InValidCourses> InValidDates = new List<InValidCourses>();

            // get all courses for schedule
            List<DoctorCourseHall> AllCourses =
             (List<DoctorCourseHall>)await _unitOfWork.DoctorCourseHalls.FindAll(e => e.IsCompeleted == false);

            foreach (var item in AllCourses)
            {
                string day = "null", Hour = "null";
                int Hall = -1;
                int HallId1 = await Check(item.Option1, item.Department, item.DoctorId);
                int HallId2 = await Check(item.Option2, item.Department, item.DoctorId);
                int HallId3 = await Check(item.Option3, item.Department, item.DoctorId);

                if (HallId1 != -1)
                {
                    string[] op = item.Option1.Split(' ');
                    Hour = op[0];
                    day = op[1];
                    Hall = HallId1;
                }
                else if (HallId2 != -1)
                {
                    string[] op = item.Option2.Split(' ');
                    Hour = op[0];
                    day = op[1];
                    Hall = HallId2;
                }
                else if (HallId3 != -1)
                {
                    string[] op = item.Option3.Split(' ');
                    Hour = op[0];
                    day = op[1];
                    Hall = HallId3;
                }
                else
                {
                    Doctor doctor = await _unitOfWork.Doctors.Get(e => e.DoctorId == item.DoctorId);
                    var course = await _unitOfWork.Courses.FindAll(e => e.CourseCode == item.Coursecode);
                    InValidCourses obj = new InValidCourses();
                    obj.Option1 = item.Option1;
                    obj.Option2 = item.Option2;
                    obj.Option3 = item.Option3;
                    obj.Department = item.Department;
                    obj.CourseCode = item.Coursecode;
                    obj.DoctorName = doctor.DoctorName;
                    obj.CourseName = course.First().CourseName;
                    obj.DoctorEmail = doctor.AcademicEmail;
                    InValidDates.Add(obj);
                    EmailModel emailModel = new EmailModel();
                    emailModel.SendEmailTo("sana.20377387@compit.aun.edu.eg", emailModel.DoctorSubject, emailModel.DoctorMessage(obj));
                }

                if (Hall != -1)
                {
                    item.Day = day;
                    item.FirstHour = Hour;
                    item.HallId = Hall;
                    item.IsCompeleted = true;
                    _unitOfWork.DoctorCourseHalls.Update(item);
                    _unitOfWork.Save();
                }
            }
            if (InValidDates.Count() == 0)
                return Ok(new { message = "All Courses Schedule succesfully." });
            return Ok(InValidDates);
        }
        // validate an option and return the hall id if there is free hall
        async Task<int> Check(string option, string department, int doctorId)
        {
            string[] op = option.Split(' ');
            string time = op[0];
            string day = op[1];

            // list all courses for the same department and check if there is any match with the current course
            List<DoctorCourseHall> CompeletedCourses =
            (List<DoctorCourseHall>)await _unitOfWork.DoctorCourseHalls.FindAll(
                e => e.Department == department && e.Day == day && e.FirstHour == time && e.IsCompeleted == true);
            if (CompeletedCourses.Count() > 0)
                return -1;

            // list all courses for the same Doctor and check if there is any match with the current course
            List<DoctorCourseHall> DoctorCourses =
            (List<DoctorCourseHall>)await _unitOfWork.DoctorCourseHalls.FindAll(
                e => e.DoctorId == doctorId && e.Day == day && e.FirstHour == time && e.IsCompeleted == true);
            if (DoctorCourses.Count() > 0)
                return -1;

            // list all the recorded hall in the same time and day
            List<Hall> Halls = (List<Hall>)await _unitOfWork.Halls.GetAll();

            List<DoctorCourseHall> MatchedCourses =
            (List<DoctorCourseHall>)await _unitOfWork.DoctorCourseHalls.FindAll(
                e => e.Day == day && e.FirstHour == time && e.IsCompeleted == true);

            // check if there is a free hall
            Dictionary<int, bool> ExistHall = new Dictionary<int, bool>();
            foreach (var item in MatchedCourses)
            {
                ExistHall[item.HallId] = true;
            }
            int HallId = -1;
            foreach (var item in Halls)
            {
                if (ExistHall.ContainsKey(item.HallId) == false)
                {
                    HallId = item.HallId;
                    break;
                }
            }

            return HallId;

        }
        [HttpGet("/GetGroupBerLevel")]
        public async Task<IActionResult> GetGroupBerLevel()
        {

            List<Levels> res = new List<Levels>();
            var Temp = await _unitOfWork.CurrentDepartments.GetAll();
            Dictionary<int, List<string>> temp = new Dictionary<int, List<string>>();
            temp[1] = new List<string>();
            temp[2] = new List<string>();
            temp[3] = new List<string>();
            temp[4] = new List<string>();
            foreach (var item in Temp)
            {
                temp[item.Level].Add(item.Name);
            }

            res.Add(new Levels() { Level = 1, Group = temp[1] });
            res.Add(new Levels() { Level = 2, Group = temp[2] });
            res.Add(new Levels() { Level = 3, Group = temp[3] });
            res.Add(new Levels() { Level = 4, Group = temp[4] });
            return Ok(temp);

        }
        // [HttpGet("/GetSchedule")]

        // public async Task<IActionResult> GetSchedule()
        // {
        //     List<StudentSechdule> Sechdule = new List<StudentSechdule>();
        //     List<DoctorCourseHall> Courses =
        //     (List<DoctorCourseHall>)await _unitOfWork.DoctorCourseHalls.FindAll(e => e.IsCompeleted == true);
        //     foreach (var course in Courses)
        //     {
        //         var tempCourse = await _unitOfWork.Courses.FindAll(e => e.CourseCode == course.Coursecode);
        //         Doctor tempDoctor = await _unitOfWork.Doctors.Get(e => e.DoctorId == course.DoctorId);
        //         Hall TempHall = await _unitOfWork.Halls.Get(e => e.HallId == course.HallId);
        //         Department level = await _unitOfWork.Departments.Get(e => e.CourseCode == course.Coursecode && e.DepartmentName == course.Department);
        //         StudentSechdule studentSechdule = new StudentSechdule()
        //         {
        //             Level = (level.Semaster / 2),
        //             CourseCode = tempCourse.First().CourseCode,
        //             CourseName = tempCourse.First().CourseName,
        //             DoctorName = tempDoctor.DoctorName,
        //             Day = course.Day,
        //             Hall = TempHall.HallName,
        //             Time = course.FirstHour,
        //             Department = course.Department
        //         };
        //         Sechdule.Add(studentSechdule);
        //     }
        //     return Ok(Sechdule);
        // }





        [HttpGet("/GetSchedule")]
        public async Task<IActionResult> GetSchedule()
        {
            List<StudentSechdule> Sechdule = new List<StudentSechdule>();
            List<DoctorCourseHall> Courses = (List<DoctorCourseHall>)await _unitOfWork.DoctorCourseHalls.FindAll(e => e.IsCompeleted == true);

            foreach (var course in Courses)
            {
                var tempCourse = await _unitOfWork.Courses.FindAll(e => e.CourseCode == course.Coursecode);
                if (tempCourse == null || !tempCourse.Any())
                {
                    continue;
                }

                Doctor tempDoctor = await _unitOfWork.Doctors.Get(e => e.DoctorId == course.DoctorId);
                if (tempDoctor == null)
                {
                    continue;
                }

                Hall TempHall = await _unitOfWork.Halls.Get(e => e.HallId == course.HallId);
                if (TempHall == null)
                {
                    continue;
                }
                Department level = new Department();
                var groups = await _unitOfWork.CurrentDepartments.FindAll(e => e.Name == course.Department);

                if (groups.First().Level == 1)
                {
                    level = await _unitOfWork.Departments.Get(e => e.CourseCode == course.Coursecode && e.DepartmentName == "first");

                }
                else if (groups.First().Level == 2)
                {
                    level = await _unitOfWork.Departments.Get(e => e.CourseCode == course.Coursecode && e.DepartmentName == "second");

                }
                else
                {
                    level = await _unitOfWork.Departments.Get(e => e.CourseCode == course.Coursecode && e.DepartmentName == course.Department);
                }
                if (level == null)
                {
                    continue;
                }

                StudentSechdule studentSechdule = new StudentSechdule()
                {
                    Level = ((1 + level.Semaster) / 2),
                    CourseCode = tempCourse.First().CourseCode,
                    CourseName = tempCourse.First().CourseName,
                    DoctorName = tempDoctor.DoctorName,
                    Day = course.Day,
                    Hall = TempHall.HallName,
                    Time = course.FirstHour,
                    Department = course.Department
                };

                Sechdule.Add(studentSechdule);
            }

            return Ok(Sechdule);

        }

        #endregion



        [HttpGet("/FillData")]
        public async Task<IActionResult> FillData()
        {
            string[] times = new string[]
            {
        "08:00 sun", "10:00 sun", "12:00 sun", "02:00 sun", "04:00 sun", "06:00 sun",
        "08:00 mon", "10:00 mon", "12:00 mon", "02:00 mon", "04:00 mon", "06:00 mon",
        "08:00 tues", "10:00 tues", "12:00 tues", "02:00 tues", "04:00 tues", "06:00 tues",
        "08:00 wed", "10:00 wed", "12:00 wed", "02:00 wed", "04:00 wed", "06:00 wed",
        "08:00 thurs", "10:00 thurs", "12:00 thurs", "02:00 thurs", "04:00 thurs", "06:00 thurs"
            };

            var doctors = (List<Doctor>)await _unitOfWork.Doctors.GetAll();
            var indextime = 0;
            var timelength = times.Count();
            var indexdoctor = 0;
            var doctorlength = doctors.Count();
            List<Department> Courses = new List<Department>();

            for (var i = 1; i < 5; i++)
            {
                var groups = (List<CurrentDepartment>)await _unitOfWork.CurrentDepartments.FindAll(e => e.Level == i);
                int currentindex = indexdoctor;

                foreach (var items in groups)
                {
                    if (i <= 2)
                    {
                        Courses = (List<Department>)await _unitOfWork.Departments.FindAll(e => e.Semaster == (i * 2));
                    }
                    else
                    {
                        Courses = (List<Department>)await _unitOfWork.Departments.FindAll(e => e.Semaster == (i * 2) && e.DepartmentName == items.Name);
                    }

                    currentindex = indexdoctor;

                    foreach (var course in Courses)
                    {
                        var id = await _unitOfWork.Courses.FindAll(e => e.CourseCode == course.CourseCode);
                        if (id == null || !id.Any())
                        {
                            continue;
                        }

                        DoctorCourseHall options = new DoctorCourseHall();
                        var doctorCourse = await _unitOfWork.DoctorCourseHalls.FindAll(e => e.Coursecode == course.CourseCode && e.Department == items.Name);
                        if (doctorCourse != null && doctorCourse.Any())
                        {
                            continue;
                        }

                        options.Coursecode = course.CourseCode;
                        options.CourseId = id.First().CourseId;
                        options.DoctorId = doctors[currentindex].DoctorId;
                        options.Department = items.Name;
                        options.Option1 = times[indextime];
                        indextime = (indextime + 1) % timelength;
                        options.Option2 = times[indextime];
                        indextime = (indextime + 1) % timelength;
                        options.Option3 = times[indextime];
                        indextime = (indextime + 1) % timelength;

                        _unitOfWork.DoctorCourseHalls.Add(options);
                        _unitOfWork.Save();
                        currentindex = (currentindex + 1) % doctorlength;
                    }
                }
                indexdoctor = currentindex;
            }

            return Ok();
        }



        [HttpGet("/getData")]
        public async Task<IActionResult> getData()
        {
            var ddd = await _unitOfWork.Doctors.GetAll();
            foreach (var d in ddd)
            {
                ApplicationUser? UserFromDB = await userManager.FindByNameAsync(d.NationalId);
                if (UserFromDB != null) continue;
                RegisterUserDTO Data = new RegisterUserDTO()
                {
                    UserName = d.NationalId,
                    Password = d.NationalId + "doctor"
                };

                List<string> RegisterResult = await RegisterRoleObj.Register(Data, roleManager, "Doctor", userManager);
            }

            return Ok();

        }

        [HttpPost("/AddnewCourse")] // pre code1,code2,code3
        public async Task<IActionResult> AddNewCourse(NewCourse course)
        {
            var existingCourse = await _unitOfWork.Courses.FindAll(e => e.CourseCode == course.CourseCode);
            if (existingCourse.Count() > 0)
            {
                return BadRequest(new { message = "Course already exists" });
            }
            Course data = new Course()
            {
                CourseName = course.CourseName,
                CourseCode = course.CourseCode,
                CreditHour = course.CreditHour,
                CoursePrerequisites = course.Prerequisites,
                Department = "-"
            };
            _unitOfWork.Courses.Add(data);
            _unitOfWork.Save();
            return Ok(new { message = "Course added successfully" });

        }
        [HttpDelete("/DeleteCourse")]
        public async Task<IActionResult> DeleteCourse(string CourseCode)
        {
            var departments = (List<Department>)await _unitOfWork.Departments.FindAll(e => e.CourseCode == CourseCode);
            foreach (var department in departments)
            {
                var Students = (List<StudentCourse>)await _unitOfWork.StudentCourses.FindAll(e => e.CourseId == department.CourseId && e.RecordedCourse != "Not");
                foreach (var student in Students)
                {
                    _unitOfWork.StudentCourses.HardDelete(student);
                }
                _unitOfWork.Departments.HardDelete(department);
            }
            var courses = (List<DoctorCourseHall>)await _unitOfWork.DoctorCourseHalls.FindAll(e => e.Coursecode == CourseCode);
            foreach (var item in courses)
            {
                _unitOfWork.DoctorCourseHalls.HardDelete(item);
            }
            var course = await _unitOfWork.Courses.Get(e => e.CourseCode == CourseCode);
            _unitOfWork.Courses.HardDelete(course);
            _unitOfWork.Save();

            return Ok(new { message = "Course deleted successfully" });
        }
        [HttpPost("/AddnewDepartmentCourse")] // first second 
        public async Task<IActionResult> AddnewDepartmentCourse(NewCourseperDepartments course)
        {
            var existingCourse = await _unitOfWork.Departments.FindAll(e => e.CourseCode == course.CourseCode && e.Semaster == course.Semaster);
            if (existingCourse.Count() > 0)
            {
                return BadRequest(new { message = "Course already exists" });
            }
            Department data = new Department()
            {
                CourseCode = course.CourseCode,
                Semaster = course.Semaster,
                DepartmentName = course.DepartmentName
            };
            _unitOfWork.Departments.Add(data);
            _unitOfWork.Save();
            return Ok(new { message = "Course added successfully" });

        }

        [HttpDelete("/DeleteDepartmentCourse")]// first second 
        public async Task<IActionResult> DeleteDepartmentCourse(string CourseCode, string DepartmentName)
        {
            var department = await _unitOfWork.Departments.Get(e => e.CourseCode == CourseCode && e.DepartmentName == DepartmentName);


            var Students =
             (List<StudentCourse>)await _unitOfWork.StudentCourses.FindAll(
                e => e.CourseId == department.CourseId && e.RecordedCourse != "Not");
            foreach (var student in Students)
            {
                _unitOfWork.StudentCourses.HardDelete(student);
            }
            var courses = new List<DoctorCourseHall>();
            if (DepartmentName == "first" || DepartmentName == "second")
                courses = (List<DoctorCourseHall>)await _unitOfWork.DoctorCourseHalls.FindAll(e => e.Coursecode == CourseCode);
            else
                courses = (List<DoctorCourseHall>)await _unitOfWork.DoctorCourseHalls.FindAll(
                    e => e.Coursecode == CourseCode && e.Department == DepartmentName);

            foreach (var item in courses)
            {
                _unitOfWork.DoctorCourseHalls.HardDelete(item);
            }
            _unitOfWork.Departments.HardDelete(department);
            _unitOfWork.Save();
            return Ok(new { message = "Course deleted successfully" });
        }
        [HttpGet("/CourseInfo")]
        public async Task<IActionResult> CourseInfo()
        {
            List<CourseInformation> data = new List<CourseInformation>();
            var courses = (List<Course>)await _unitOfWork.Courses.GetAll();
            foreach (var course in courses)
            {
                var departments =
                  (List <Department>)await _unitOfWork.Departments.FindAll(e => e.CourseCode == course.CourseCode);

                List<NewCourseperDepartments> res = new List<NewCourseperDepartments>();
                foreach (var department in departments)
                {
                    NewCourseperDepartments temp = new NewCourseperDepartments();
                    temp.CourseCode = department.CourseCode;
                    temp.DepartmentName = department.DepartmentName;
                    temp.Semaster = department.Semaster;
                    res.Add(temp);
                }
                CourseInformation info = new CourseInformation()
                {
                    CourseName = course.CourseName,
                    CourseCode = course.CourseCode,
                    Prerequisites = course.CoursePrerequisites,
                    CreditHour = course.CreditHour,
                    courses = res
                };
                data.Add(info);
            }
            return Ok(data);
        }

    }
}