namespace FreeLance.Migrations
{
	using System;
	using System.Data.Entity;
	using System.Data.Entity.Migrations;
	using FreeLance.Models;
	using System.Linq;
	using System.Collections.Generic;
	using Microsoft.AspNet.Identity.EntityFramework;
	using Microsoft.AspNet.Identity;

	internal sealed class Configuration : DbMigrationsConfiguration<FreeLance.Models.ApplicationDbContext>
	{
		public Configuration()
		{
			AutomaticMigrationsEnabled = true;
			AutomaticMigrationDataLossAllowed = true;
			ContextKey = "FreeLance.Models.ApplicationDbContext";
		}

		protected override void Seed(FreeLance.Models.ApplicationDbContext context)
		{
			SeedRoles(context);
			SeedUsers(context);
			SeedProblems(context);
			SeedContracts(context);
		}

		private void SeedProblems(FreeLance.Models.ApplicationDbContext context)
		{
			context.ProblemModels.AddOrUpdate(
				p => p.Name,
				new ProblemModels { ProblemId = 1, Name = "Supertask", Description = "Write Operation System.", Status = ProblemStatus.Opened },
				new ProblemModels { ProblemId = 2, Name = "Android design", Description = "Make material design for android application RemindMe.", Status = ProblemStatus.Opened },
				new ProblemModels { Name = "FreeLance site", Description = "Improve FL site.", Status = ProblemStatus.Opened },
				new ProblemModels { Name = "Have a rest", Description = "Enjoy your day.", Status = ProblemStatus.Opened },
				new ProblemModels { Name = "Write Half-life3", Description = "Some text.", Status = ProblemStatus.Opened },
				new ProblemModels { Name = "English translations", Description = "Translate some texts from english to russian.", Status = ProblemStatus.InProgress },
				new ProblemModels { Name = "Implement queue", Description = "Language : ASSEMBLER", Status = ProblemStatus.Closed }
				);
		}

		private void SeedContracts(FreeLance.Models.ApplicationDbContext context)
		{
			ProblemModels problem = new ProblemModels { Name = "Problem with contract", Description = "2-3 workers on this problem", Status = ProblemStatus.InProgress};
			var contracts = new List<ContractModels>();
			contracts.Add(new ContractModels { Details = "First contract", Problem = problem, Status = ContractStatus.Confirmed });
			contracts.Add(new ContractModels { Details = "Second contract", Problem = problem, Status = ContractStatus.InProgress });
			contracts.Add(new ContractModels { Details = "Third contract", Problem = problem, Status = ContractStatus.InProgress });
			problem.Contracts = contracts;
			context.ProblemModels.AddOrUpdate(p => p.Name, problem);
		}

		private void SeedRoles(ApplicationDbContext context)
		{
			context.Roles.AddOrUpdate(r => r.Name, new IdentityRole { Name = "Admin" });
			context.Roles.AddOrUpdate(r => r.Name, new IdentityRole { Name = "Freelancer" });
			context.Roles.AddOrUpdate(r => r.Name, new IdentityRole { Name = "Employer" });
			context.SaveChanges();
		}

		private void SeedUsers(ApplicationDbContext context)
		{
			var userStore = new UserStore<ApplicationUser>(context);
			var userManager = new UserManager<ApplicationUser>(userStore);
			addUser(context, userManager, "admin@ya.ru", "111111", "Admin");
			addUser(context, userManager, "employer@ya.ru", "111111", "Employer");
			addUser(context, userManager, "freelancer@ya.ru", "111111", "Freelancer");
		}

		private void addUser(ApplicationDbContext context, UserManager<ApplicationUser> manager, string email, string password, string role)
		{
			if (!context.Users.Any(t => String.Compare(t.UserName, email) == 0))
			{
				var user = new ApplicationUser { UserName = email, Email = email };
                manager.Create(user, password);
				manager.AddToRole(user.Id, role);
			}
		}
    }
}
