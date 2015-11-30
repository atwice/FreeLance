﻿using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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

		public bool? IsApprovedByCoordinator { get; set; }
		public string FIO { get; set; }
		public virtual DocumentPackageModels DocumentPackage { get; set; }
		public EmailNotificationPolicyModel EmailNotificationPolicy { get; set; }
        public string PhotoPath { get; set; }
        public virtual LawFace LawFace { get; set; }

		public class EmailNotificationPolicyModel
		{
			public bool IsCommentsEnabled { get; set; }
			public bool IsDocumentsEnabled { get; set; }
			public bool IsNewApplicantsEnabled { get; set; }
			public bool IsContractStatusEnabled { get; set; }
		}

	}

	public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        public static ApplicationDbContext Create() => new ApplicationDbContext();

	    protected override void OnModelCreating(DbModelBuilder modelBuilder)
		{
			modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();
			base.OnModelCreating(modelBuilder);
		}

		public System.Data.Entity.DbSet<FreeLance.Models.ProblemModels> ProblemModels { get; set; }
		public System.Data.Entity.DbSet<FreeLance.Models.ContractModels> ContractModels { get; set; }
		public System.Data.Entity.DbSet<FreeLance.Models.SubscriptionModels> SubscriptionModels { get; set; }
		public System.Data.Entity.DbSet<FreeLance.Models.DocumentPackageModels> DocumentPackageModels { get; set; }
        public System.Data.Entity.DbSet<FreeLance.Models.LawFace> LawFaces { get; set; }
        public System.Data.Entity.DbSet<FreeLance.Models.LawContractTemplate> LawContractTemplates { get; set; }
        public System.Data.Entity.DbSet<FreeLance.Models.LawContract> LawContracts { get; set; }
		public System.Data.Entity.DbSet<FreeLance.Models.Chat> Chats { get; set; }
		public System.Data.Entity.DbSet<FreeLance.Models.ContractChat> ContractChats { get; set; }
		public System.Data.Entity.DbSet<FreeLance.Models.ProblemChat> ProblemChats { get; set; }
		public System.Data.Entity.DbSet<FreeLance.Models.ChatMessage> ChatMessages { get; set; }
		public System.Data.Entity.DbSet<FreeLance.Models.ChatUserStatistic> ChatUserStatistics { get; set; }

	}
}