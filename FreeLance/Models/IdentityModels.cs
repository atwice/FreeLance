using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.ComponentModel.DataAnnotations;

namespace FreeLance.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }

		[Required]
		public bool IsApprovedByCoordinator { get; set; }

		public DocumentPackageModels DocumentPackage { get; set; }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        public static ApplicationDbContext Create()
        {
			return new ApplicationDbContext();
        }

		protected override void OnModelCreating(DbModelBuilder modelBuilder)
		{
			modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();
			base.OnModelCreating(modelBuilder);
		}

		public System.Data.Entity.DbSet<FreeLance.Models.ProblemModels> ProblemModels { get; set; }
		public System.Data.Entity.DbSet<FreeLance.Models.ContractModels> ContractModels { get; set; }
		public System.Data.Entity.DbSet<FreeLance.Models.SubscriptionModels> SubscriptionModels { get; set; }
		public System.Data.Entity.DbSet<FreeLance.Models.DocumentPackageModels> DocumentPackageModels { get; set; }
	}
}