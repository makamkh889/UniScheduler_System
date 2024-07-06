
using GraduationProject.core.Models;
using GraduationProject.core.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrdauationProject.EF.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;

        public IBaseRepository<Hall> Halls { get; private set; }
        public IBaseRepository<Admin> Admins { get; private set; }
        public IBaseRepository<Course> Courses { get; private set; }
        public IBaseRepository<Doctor> Doctors { get; private set; }
        public IBaseRepository<Student> Students { get; private set; }
        public IBaseRepository<Department> Departments { get; private set; }
        public IBaseRepository<StudentCourse> StudentCourses { get; private set; }
        public IBaseRepository<DoctorCourseHall> DoctorCourseHalls { get; private set; }
        public IBaseRepository<CurrentDepartment> CurrentDepartments { get; private set; }

        public UnitOfWork(AppDbContext context)
        {
            _context = context;

            Halls = new BaseRepository<Hall>(_context);
            Admins = new BaseRepository<Admin>(_context);
            Doctors = new BaseRepository<Doctor>(_context);
            Students = new BaseRepository<Student>(_context);
            Courses = new BaseRepository<Course>(_context);
            Departments = new BaseRepository<Department>(_context);
            StudentCourses = new BaseRepository<StudentCourse>(_context);
            DoctorCourseHalls = new BaseRepository<DoctorCourseHall>(_context);
            CurrentDepartments = new BaseRepository<CurrentDepartment>(_context);
        }

        public int Save()
        {
            return _context.SaveChanges();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
