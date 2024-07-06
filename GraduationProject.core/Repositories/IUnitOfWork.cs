using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GraduationProject.core.Models;

namespace GraduationProject.core.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        IBaseRepository<Admin> Admins { get; }
        IBaseRepository<Student> Students { get; }
        IBaseRepository<Doctor> Doctors { get; }
        IBaseRepository<Course> Courses { get; }
        IBaseRepository<DoctorCourseHall> DoctorCourseHalls { get; }
        IBaseRepository<Department> Departments { get; }
        IBaseRepository<CurrentDepartment> CurrentDepartments { get; }
        IBaseRepository<Hall> Halls { get; }
        IBaseRepository<StudentCourse> StudentCourses { get; }

        int Save();
    }
}
