using GraduationProject.core.DTO.UserInput;
using Microsoft.AspNetCore.Identity;

namespace GrdauationProject.EF
{
    public class Registeration
    {
        public Registeration()
        {

        }
        // register the user and return true is success or error list if not and false if there is any error in the data
        public async Task<List<string>> Register(RegisterUserDTO registerUserDTO,
            RoleManager<IdentityRole> roleManager, string role,
            UserManager<ApplicationUser> userManager)
        {
            List<string> ret = new List<string>();

            ApplicationUser AppUser = new ApplicationUser()
            {
                UserName = registerUserDTO.UserName,
                PasswordHash = registerUserDTO.Password
            };

            // Register user
            IdentityResult registerResult = await userManager.CreateAsync(AppUser, registerUserDTO.Password);
            if (registerResult.Succeeded)
            {
                // Check if the role exists, if not, create it
                if (!await roleManager.RoleExistsAsync(role))
                {
                    IdentityRole Role = new IdentityRole()
                    {
                        Name = role
                    };
                    IdentityResult RoleResult = await roleManager.CreateAsync(Role);
                    if (!RoleResult.Succeeded)
                    {
                        foreach (var error in RoleResult.Errors)
                        {
                            ret.Add(error.Description);
                        }
                        return (ret);
                    }
                }
                // Assign the role to the user
                IdentityResult roleAssignmentResult = await userManager.AddToRoleAsync(AppUser, role);
                if (roleAssignmentResult.Succeeded)
                {
                    ret.Add("True");
                    return ret;
                }
                else
                {
                    foreach (var error in roleAssignmentResult.Errors)
                    {
                        ret.Add(error.Description);
                    }
                }

            }
            foreach (var error in registerResult.Errors)
            {
                ret.Add(error.Description);
            }

            return (ret);
        }
    }
}
