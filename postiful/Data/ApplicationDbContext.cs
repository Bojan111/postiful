using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using postiful.Core.Entities.PinterestEntitiy;

public class ApplicationDbContext: IdentityDbContext{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options):base(options){

    }

    public DbSet<Pinterest> Pinterests { get; set; }
}