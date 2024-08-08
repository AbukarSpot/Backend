
using Microsoft.EntityFrameworkCore;
using ProjectModels;

namespace DatabaseContex {

    public class ProjectContext: DbContext {

        public IConfiguration _config { get; set; }

        public DbSet<Customer> Customer { get; set; }
        public DbSet<User> User { get; set; }
        public DbSet<Orders> Orders { get; set; }
        public ProjectContext() {}
        public ProjectContext(DbContextOptions<ProjectContext> options)
        : base(options)
        {}
    }
}