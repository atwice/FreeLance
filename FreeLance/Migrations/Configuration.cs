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
			LinkProblemsToEmployers(context);
			AddEmployerFreelancerProblemContractAuto(context, "Employer1", "Freelancer1");
			AddEmployerFreelancerProblemContractAuto(context, "Employer2", "Freelancer2");
			AddEmployerFreelancerProblemContractAuto(context, "Employer2", "Freelancer3");
			AddProblemWithSubscriber(context, "Employer4", "Subscriber1");
			AddProblemWithSubscriber(context, "Employer5", "Subscriber2");
			AddProblemWithSubscriber(context, "Employer6", "Subscriber3");
		}

		private void SeedProblems(FreeLance.Models.ApplicationDbContext context)
		{
			context.ProblemModels.AddOrUpdate(
				p => p.Name,
				new ProblemModels { Name = "Supertask", Description = "Write Operation System.", Status = ProblemStatus.Opened },
				new ProblemModels { Name = "Android design", Description = "Make material design for android application RemindMe.", Status = ProblemStatus.Opened },
				new ProblemModels { Name = "FreeLance site", Description = "Improve FL site.", Status = ProblemStatus.Opened },
				new ProblemModels { Name = "Have a rest", Description = "Enjoy your day.", Status = ProblemStatus.Opened },
				new ProblemModels { Name = "Write Half-life3", Description = "Some text.", Status = ProblemStatus.Opened },
				new ProblemModels { Name = "English translations", Description = "Translate some texts from english to russian.", Status = ProblemStatus.InProgress },
				new ProblemModels { Name = "Implement queue", Description = "Language : ASSEMBLER", Status = ProblemStatus.Closed }
				);
		}

		private ProblemModels addProblem(ApplicationDbContext context, string name, string desc, ProblemStatus status, ApplicationUser employer)
		{
			var problem = new ProblemModels { Name = name, Description = desc, Status = status, Employer = employer };
			context.ProblemModels.AddOrUpdate(p => p.Name, problem);
			return problem;
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

		private ContractModels addContract(ApplicationDbContext context, string details, ContractStatus status, ProblemModels problem, ApplicationUser freelancer)
		{
			var contract = new ContractModels { Details = details, Problem = problem, Status = ContractStatus.Confirmed, Freelancer = freelancer };
			context.ContractModels.AddOrUpdate(p => p.Details, contract);
			return contract;
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
			var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
			addUser(context, userManager, "admin@ya.ru", "111111", "Admin");
			addUser(context, userManager, "employer@ya.ru", "111111", "Employer");
			addUser(context, userManager, "freelancer@ya.ru", "111111", "Freelancer");
		}

		private void AddEmployerFreelancerProblemContractAuto(ApplicationDbContext context, string employerName, string freelancerName)
		{
			try {
				var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
				var employer = addUser(context, userManager, employerName + "@ya.ru", "111111", "Employer");
				var freelancer = addUser(context, userManager, freelancerName + "@ya.ru", "111111", "Freelancer");
				var problem = addProblem(context, "[AUTO] " + employerName, "to " + freelancerName, ProblemStatus.Opened, employer);
				var contract = addContract(context, employerName + " to " + freelancerName, ContractStatus.Opened, problem, freelancer);
			} catch (Exception e)
			{
				System.Diagnostics.Debug.WriteLine("Error! " + e.Message);
			}
		}

		private void AddProblemWithSubscriber(ApplicationDbContext context, string employerName, string freelancerName)
		{
			try{
			var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
			var employer = addUser(context, userManager, employerName + "@ya.ru", "111111", "Employer");
			var freelancer = addUser(context, userManager, freelancerName + "@ya.ru", "111111", "Freelancer");
			var problem = addProblem(context, "[AUTO] " + employerName, "to " + freelancerName, ProblemStatus.Opened, employer);
			var subscription = addSubscription(context, problem, freelancer);
			} catch (Exception e)
			{
                System.Diagnostics.Debug.WriteLine("Error! " + e.Message);
			}
}

		private ApplicationUser addUser(ApplicationDbContext context, UserManager<ApplicationUser> manager, string email, string password, string role)
		{
			if (!context.Users.Any(t => String.Compare(t.UserName, email) == 0))
			{
				var user = new ApplicationUser { UserName = email, Email = email };
                manager.Create(user, password);
				manager.AddToRole(user.Id, role);
				return user;
			}
			return null;
		}

		private SubscriptionModels addSubscription(ApplicationDbContext context, ProblemModels problem, ApplicationUser user)
		{
			var subscription = new SubscriptionModels { Problem = problem, Freelancer = user };
			context.SubscriptionModels.Add(subscription);
			return subscription;
		}

		private void LinkProblemsToEmployers(ApplicationDbContext context)
		{
			var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
			var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
			var userIds = userManager.Users.ToArray();

			bool employerSetted = false;
			bool freelancerSetted = false;
			bool adminSetted = false;

			for (int i = 0; i < userIds.Length; ++i)
			{
				if (userIds[i].Roles.First().RoleId.Equals(roleManager.FindByName("Admin").Id) && !adminSetted) {
					adminSetted = true;
				}
				if (userIds[i].Roles.First().RoleId.Equals(roleManager.FindByName("Employer").Id) && !employerSetted) {
					employerSetted = true;
					var employer = userIds[i];
					foreach (ProblemModels problem in context.ProblemModels.ToArray()) {
						problem.Employer = employer;
					}
				}
				if (userIds[i].Roles.First().RoleId.Equals(roleManager.FindByName("Freelancer").Id) && !freelancerSetted) {
					freelancerSetted = true;
					var freelancer = userIds[i];
					foreach (ContractModels contract in context.ContractModels.ToArray())
					{
						contract.Freelancer = freelancer;
					}
				}
			}
			context.SaveChanges();
		}
    }
}
