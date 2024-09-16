using Microsoft.EntityFrameworkCore;

namespace Login_App.Models
{
    public class LoginDBContext :DbContext
    {
        public LoginDBContext(DbContextOptions<LoginDBContext> options):base(options)
        { 
        }
        public DbSet<User> Users { get; set; }
    }
}
