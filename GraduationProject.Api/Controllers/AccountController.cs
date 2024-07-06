using GraduationProject.core.DTO.UserInput;
using GrdauationProject.EF;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace FCI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IConfiguration configuration;
        private readonly UserManager<ApplicationUser> userManager;

        Registeration RegisterRoleObj = new Registeration();

        private readonly RoleManager<IdentityRole> roleManager;

        public AccountController(UserManager<ApplicationUser> userManager,
            IConfiguration Configuration, RoleManager<IdentityRole> roleManager)
        {
            this.userManager = userManager;
            this.configuration = Configuration;
            this.roleManager = roleManager;
        }

        [HttpPost("/Login")]
        public async Task<IActionResult> Login(LoginUserDTO LoginInfo)
        {
            if (ModelState.IsValid)
            {
                var users = userManager.Users.ToList();
                if (users.Count() == 0)
                {
                    // Register  first
                    // admin

                    RegisterUserDTO Data = new RegisterUserDTO()
                    {
                        UserName = LoginInfo.UserName,
                        Password = LoginInfo.UserName + "admin"
                    };

                    List<string> RegisterResult = await RegisterRoleObj.Register(Data, roleManager, "SuperAdmin", userManager);

                    if (RegisterResult.Count() != 1 && RegisterResult[0] != "True")
                        return BadRequest(RegisterResult);
                }
                ApplicationUser? UserFromDB = await userManager.FindByNameAsync(LoginInfo.UserName);
                if (UserFromDB != null)
                {
                    bool Found = await userManager.CheckPasswordAsync(UserFromDB, LoginInfo.Password);
                    if (Found)
                    {
                        // Business Logic: the same user with each login take differn token
                        List<Claim> ClaimList = new List<Claim>();
                        ClaimList.Add(new Claim(ClaimTypes.Name, UserFromDB.UserName));
                        ClaimList.Add(new Claim(ClaimTypes.NameIdentifier, UserFromDB.Id));
                        ClaimList.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));

                        // Add the Roles
                        IList<string> Roles = await userManager.GetRolesAsync(UserFromDB);
                        foreach (string Role in Roles)
                        {
                            ClaimList.Add(new Claim(ClaimTypes.Role, Role));
                        }

                        var SecurityKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(configuration["JWT:SecurityKey"]));

                        SigningCredentials signingCredentials = new SigningCredentials(
                            SecurityKey, SecurityAlgorithms.HmacSha256);

                        // Create Token   
                        JwtSecurityToken MyToken = new JwtSecurityToken(
                            issuer: configuration["JWT:ValidIssuer"],    // Provider Create Token
                            audience: configuration["JWT:ValidAudience"], // Consumer URL
                            expires: DateTime.Now.AddHours(2),
                            claims: ClaimList,
                            signingCredentials: signingCredentials
                            );
                        return Ok(new
                        {
                            Token = new JwtSecurityTokenHandler().WriteToken(MyToken),
                            Expired = MyToken.ValidTo
                        });
                    }
                }
                return Unauthorized("Invalid Account");

            }
            return BadRequest(ModelState);
        }

        
    }
}

