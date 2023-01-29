using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;


namespace TestAuth.Data;

internal sealed class AppDbContext : IdentityDbContext<IdentityUser>
{
	public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
	{
		
	}
}
