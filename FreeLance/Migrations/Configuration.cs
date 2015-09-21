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
	using System.Data.Entity.Validation;

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
			//if (System.Diagnostics.Debugger.IsAttached == false)
			//	System.Diagnostics.Debugger.Launch();
			SeedRoles(context);
			SeedUsers(context);
			SeedProblems(context);
			SeedContracts(context);
			AddEmployerFreelancerProblemContractAuto(context, "Employer1", "Freelancer1");
			AddEmployerFreelancerProblemContractAuto(context, "Employer2", "Freelancer2");
			AddEmployerFreelancerProblemContractAuto(context, "Employer2", "Freelancer3");
			AddProblemWithSubscriber(context, "Employer4", "Subscriber1");
			AddProblemWithSubscriber(context, "Employer5", "Subscriber2");
			AddProblemWithSubscriber(context, "Employer6", "Subscriber3");
		}

		private void SeedProblems(FreeLance.Models.ApplicationDbContext context)
		{
			var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
			var employer = addUser(context, userManager, "super@ya.ru", "111111", "Employer");
			addProblem(context, "Supertask", "Write Operation System.", ProblemStatus.Opened, employer);
			addProblem(context, "Android design", "Make material design for android application RemindMe.", ProblemStatus.Opened, employer);
			addProblem(context, "FreeLance site", "Improve FL site.", ProblemStatus.Opened, employer);
			addProblem(context, "Have a rest", "Enjoy your day..", ProblemStatus.Opened, employer);
			addProblem(context, "Write Half-life3", "Some text.", ProblemStatus.Opened, employer);
			addProblem(context, "English translations", "Translate some texts from english to russian.", ProblemStatus.Opened, employer);
			addProblem(context, "Implement queue", "Language : ASSEMBLER", ProblemStatus.Opened, employer);
		}

		private ProblemModels addProblem(ApplicationDbContext context, string name, string desc, ProblemStatus status, ApplicationUser employer)
		{
			var problem = new ProblemModels { Name = name, Description = desc, Status = status, Employer = employer };
			context.ProblemModels.AddOrUpdate(p => p.Name, problem);
			return problem;
		}

		private void SeedContracts(FreeLance.Models.ApplicationDbContext context)
		{
			var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
			var freelancer = addUser(context, userManager, "contractfreelancer@ya.ru", "111111", "Freelancer");
			var employer = addUser(context, userManager, "contractemployer@ya.ru", "111111", "Employer");
			var problem = addProblem(context, "Problem with coantract", "2-3 workers on this problem", ProblemStatus.Opened, employer);
			var contracts = new List<ContractModels>();
			contracts.Add(addContract(context, "First contract", ContractStatus.Opened, problem, freelancer));
			contracts.Add(addContract(context, "Second contract", ContractStatus.Opened, problem, freelancer));
			contracts.Add(addContract(context, "Third contract", ContractStatus.Opened, problem, freelancer));
			problem.Contracts = contracts;
			context.ProblemModels.AddOrUpdate(p => p.Name, problem);
			context.SaveChanges();
		}

		private ContractModels addContract(ApplicationDbContext context, string details, ContractStatus status, ProblemModels problem, ApplicationUser freelancer)
		{
			var contract = new ContractModels { Details = details, Problem = problem, Status = status, Freelancer = freelancer };
			context.ContractModels.AddOrUpdate(p => p.Details, contract);
			return contract;
		}

		private void SeedRoles(ApplicationDbContext context)
		{
			context.Roles.AddOrUpdate(r => r.Name, new IdentityRole { Name = "Admin" });
			context.Roles.AddOrUpdate(r => r.Name, new IdentityRole { Name = "Freelancer" });
			context.Roles.AddOrUpdate(r => r.Name, new IdentityRole { Name = "Employer" });
			context.Roles.AddOrUpdate(r => r.Name, new IdentityRole { Name = "Incognito" });
			context.Roles.AddOrUpdate(r => r.Name, new IdentityRole { Name = "Coordinator" });
			context.SaveChanges();
		}

		private void SeedUsers(ApplicationDbContext context)
		{
			var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
			addUser(context, userManager, "admin@ya.ru", "111111", "Admin");
			addUser(context, userManager, "employer@ya.ru", "111111", "Employer");
			addUser(context, userManager, "freelancer@ya.ru", "111111", "Freelancer");
            addUser(context, userManager, "incognito@ya.ru", "111111", "Incognito");
			addUser(context, userManager, "coordinator@ya.ru", "111111", "Coordinator");
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
			var user = manager.FindByEmail(email);
			if (user != null)
			{
				return user;
			}
			user = new ApplicationUser { UserName = email, Email = email };
            manager.Create(user, password);
			manager.AddToRole(user.Id, role);
			return user;
		}

		private SubscriptionModels addSubscription(ApplicationDbContext context, ProblemModels problem, ApplicationUser user)
		{
			var subscription = new SubscriptionModels { Problem = problem, Freelancer = user };
			context.SubscriptionModels.Add(subscription);
			return subscription;
		}
    }
}
