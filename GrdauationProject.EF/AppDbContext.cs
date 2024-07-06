using GraduationProject.core.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GrdauationProject.EF
{
    public class AppDbContext :  IdentityDbContext<ApplicationUser>
    {
          public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {


        }


        public DbSet<Admin> Admins { get; set; }
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<Hall> Halls { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<DoctorCourseHall> DoctorCourseHalls { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<StudentCourse> StudentCourses { get; set; }
        public DbSet<CurrentDepartment> CurrentDepartments { get; set; }





        // set constraints on database
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            base.OnModelCreating(modelBuilder);

            // Global query filters for soft delete
            modelBuilder.Entity<Student>().HasQueryFilter(s => !s.IsDeleted);
            modelBuilder.Entity<StudentCourse>().HasQueryFilter(sc => !sc.IsDeleted);
            modelBuilder.Entity<Admin>().HasQueryFilter(s => !s.IsDeleted);
            modelBuilder.Entity<Course>().HasQueryFilter(sc => !sc.IsDeleted);
            modelBuilder.Entity<Doctor>().HasQueryFilter(s => !s.IsDeleted);
            modelBuilder.Entity<DoctorCourseHall>().HasQueryFilter(sc => !sc.IsDeleted);

            /* modelBuilder.Entity<IdentityUserLogin<string>>(entity =>
             {
                 entity.HasKey(e => new { e.LoginProvider, e.ProviderKey });
             });
 */

            // set hall name unique
            modelBuilder.Entity<Hall>()
                 .HasIndex(p => p.HallName).IsUnique();


            // set course code unique for course talbe
            /*modelBuilder.Entity<Course>()
                .HasIndex(p => p.CourseCode).IsUnique();

*/

            // set NationalId for docotr unique
            modelBuilder.Entity<Doctor>()
                .HasIndex(p => p.NationalId).IsUnique();


            // set AcademicEmail for docotr unique
            /*   modelBuilder.Entity<Doctor>()
                   .HasIndex(p => p.AcademicEmail).IsUnique();*/

            // set NationalId for admin unique
            modelBuilder.Entity<Admin>()
                .HasIndex(p => p.NationalId).IsUnique();


            // set AcademicEmail for admin unique
            modelBuilder.Entity<Admin>()
                .HasIndex(p => p.AcademicEmail).IsUnique();

            // set AcademicEmail for stuent unique
            /* modelBuilder.Entity<Student>()
                 .HasIndex(p => p.AcademicEmail).IsUnique();*/

            // set NationalId for student unique
            modelBuilder.Entity<Student>()
                .HasIndex(p => p.NationalId).IsUnique();
                
        }
    }
}
