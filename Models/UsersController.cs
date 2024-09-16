using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Login_App.Models
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly LoginDBContext _loginDBContext;
        private readonly IConfiguration _configuration;

        public UsersController(LoginDBContext loginDBContext, IConfiguration configuration)
        {
            _loginDBContext = loginDBContext;
            _configuration = configuration;
        }

        [HttpPost]
        [Route("Registration")]
        public IActionResult Registration([FromBody] UserDTO user)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var res = _loginDBContext.Users.FirstOrDefault(x => x.Email == user.Email);
            if (res == null)
            {
                _loginDBContext.Users.Add(new User
                {
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    Password = user.Password
                });
                _loginDBContext.SaveChanges();
            }
            else
            {
                return BadRequest("User already registered with this email");
            }

            return Ok("User Registered Successfully");
        }

        [HttpPost]
        [Route("Login")]
        public IActionResult Login([FromBody] LoginDTO login)
        {
            var res = _loginDBContext.Users.FirstOrDefault(x => x.Email == login.Email && x.Password == login.Password);
            if (res != null)
            {
                var claims = new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, _configuration["JWT:Subject"]),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim("UserId", res.UserId.ToString()),
                    new Claim("Email", res.Email.ToString())
                };
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Key"]));
                var signIn = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                var token = new JwtSecurityToken(
                    _configuration["JWT:Issuer"],
                    _configuration["JWT:Audience"],
                    claims,
                    notBefore: DateTime.UtcNow,
                    expires: DateTime.UtcNow.AddMinutes(60),
                    signingCredentials: signIn
                );
                string tokenValue = new JwtSecurityTokenHandler().WriteToken(token);
                return Ok(new { Token = tokenValue, User = res });
            }
            return NotFound();
        }

        [HttpGet]
        [Route("GetUsers")]
        public IActionResult GetUsers()
        {
            return Ok(_loginDBContext.Users);
        }

        [Authorize]
        [HttpGet]
        [Route("api/GetUser")]
        public IActionResult GetUser(int id)
        {
            var res = _loginDBContext.Users.FirstOrDefault(x => x.UserId == id);
            if (res != null)
                return Ok(res);
            return NotFound();
        }
    }
}
