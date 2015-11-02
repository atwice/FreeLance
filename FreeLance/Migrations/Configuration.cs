using System.Linq.Expressions;

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
	using System.IO;

	internal sealed class Configuration : DbMigrationsConfiguration<FreeLance.Models.ApplicationDbContext>
	{

		private static readonly Random random = new Random();

		private static double RandomNumberBetween(double minValue, double maxValue)
		{
			var next = random.NextDouble();

			return minValue + (next * (maxValue - minValue));
		}

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
			SeedLawContractTemplates(context);
			AddEmployerFreelancerProblemContractAuto(context, "Employer1", "Freelancer1");
			AddEmployerFreelancerProblemContractAuto(context, "Employer2", "Freelancer2");
			AddEmployerFreelancerProblemContractAuto(context, "Employer", "Freelancer3");
			AddProblemWithSubscriber(context, "Employer4", "Subscriber1");
			AddProblemWithSubscriber(context, "Employer5", "Subscriber1");
			AddProblemWithSubscriber(context, "Employer6", "Subscriber1");
			AddProblemWithSubscriber(context, "Employer5", "Subscriber2");
			AddProblemWithSubscriber(context, "Employer6", "Subscriber3");
			AddClosedContracts(context);
			AddVeryBadProblem(context);
		}

		private void SeedProblems(FreeLance.Models.ApplicationDbContext context)
		{
			var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
			var employer = addUser(context, userManager, "super", "super@ya.ru", "111111", "Employer");
			addProblem(context, "Supertask", "Write Operation System.", ProblemStatus.Opened, employer);
			addProblem(context, "Android design", "Make material design for android application RemindMe.", ProblemStatus.Opened, employer);
			addProblem(context, "FreeLance site", "Improve FL site.", ProblemStatus.Opened, employer);
			addProblem(context, "Have a rest", "Enjoy your day..", ProblemStatus.Opened, employer);
			addProblem(context, "Write Half-life3", "Some text.", ProblemStatus.Opened, employer);
			addProblem(context, "English translations", "Translate some texts from english to russian.", ProblemStatus.Opened, employer);
			addProblem(context, "Implement queue", "Language : ASSEMBLER", ProblemStatus.Opened, employer);
		}


	    private void SeedLawContractTemplates(FreeLance.Models.ApplicationDbContext context)
	    {
            LawFace lawFace1 = new LawFace {Name = "Abbyy Production"};
	        LawFace lawFace2 = new LawFace {Name = "Abbyy Lingvo"};
			var rootPath = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.FullName + "\\App_Data\\LawContractTemplates\\";
            LawContractTemplate template1 = new LawContractTemplate {
	            LawFace = lawFace1,
	            Name = "Basic Document",
				Path = rootPath  + "template1.docx",
                Active = true
			};
            LawContractTemplate template2 = new LawContractTemplate
            {
                LawFace = lawFace1,
                Name = "Old Document",
				Path = rootPath  + "template2.docx",
                Active = true
			};
            LawContractTemplate template3 = new LawContractTemplate
            {
                LawFace = lawFace2,
                Name = "Actual Document",
				Path = rootPath + "template3.docx",
                Active = true
            };

            context.LawFaces.AddOrUpdate(p => p.Name, lawFace1);
            context.LawFaces.AddOrUpdate(p => p.Name, lawFace2);
            context.LawContractTemplates.AddOrUpdate(p => p.Path, template1);
            context.LawContractTemplates.AddOrUpdate(p => p.Path, template2);
            context.LawContractTemplates.AddOrUpdate(p => p.Path, template3);
        }

		private ProblemModels addProblem(ApplicationDbContext context, string name, string desc, ProblemStatus status, ApplicationUser employer)
		{
			Random rnd = new Random();
			String additionalDescription = " Although moreover mistaken kindness me feelings do be marianne. Son over own nay with tell they cold upon are. Cordial village and settled she ability law herself. Finished why bringing but sir bachelor unpacked any thoughts. Unpleasing unsatiable particular inquietude did nor sir. Get his declared appetite distance his together now families. Friends am himself at on norland it viewing. Suspected elsewhere you belonging continued commanded she. Do commanded an shameless we disposing do.Indulgence ten remarkably nor are impression out. Power is lived means oh every in we quiet. Remainder provision an in intention.Saw supported too joy promotion engrossed propriety.Me till like it sure no sons. ";
            var problem = new ProblemModels {
				Name = name,
				SmallDescription = desc + additionalDescription,
				Description = desc + additionalDescription, Status = status,
				Employer = employer,
				CreationDate = DateTime.Now.AddDays(rnd.Next(1, 40)).AddHours(5),
				Cost = (decimal)RandomNumberBetween(10.1, 5000.0)
			};
			context.ProblemModels.AddOrUpdate(p => p.Name, problem);
			return problem;
		}

		private void SeedContracts(FreeLance.Models.ApplicationDbContext context)
		{
			var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
			var freelancer = addUser(context, userManager, "contractfreelancer", "contractfreelancer@ya.ru", "111111", "Freelancer");
			var employer = addUser(context, userManager, "contractemployer", "contractemployer@ya.ru", "111111", "Employer");
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
			Random rnd = new Random();
			var contract = new ContractModels { Details = details, Problem = problem, Status = status, Freelancer = freelancer,
				CreationDate = DateTime.Now.AddDays(rnd.Next(1, 40)).AddHours(5),
				EndingDate = DateTime.Now.AddDays(rnd.Next(50, 100)).AddHours(5),
				Comment ="Comment about freelancer's work", Rate=rnd.Next(1,6),
				Cost = (decimal) RandomNumberBetween(10.1, 5000.0)
			};
			context.ContractModels.AddOrUpdate(p => p.Details, contract);
			return contract;
		}

		private void SeedRoles(ApplicationDbContext context)
		{
			context.Roles.AddOrUpdate(r => r.Name, new IdentityRole { Name = "Admin" });
			context.Roles.AddOrUpdate(r => r.Name, new IdentityRole { Name = "Freelancer" });
			context.Roles.AddOrUpdate(r => r.Name, new IdentityRole { Name = "Employer" });
			context.Roles.AddOrUpdate(r => r.Name, new IdentityRole { Name = "Incognito" });
			context.Roles.AddOrUpdate(r => r.Name, new IdentityRole { Name = "Trash" });
			context.Roles.AddOrUpdate(r => r.Name, new IdentityRole { Name = "Coordinator" });
			context.SaveChanges();
		}

		private void SeedUsers(ApplicationDbContext context)
		{
			var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
			addUser(context, userManager, "admin", "admin@ya.ru", "111111", "Admin");
			addUser(context, userManager, "employer", "employer@ya.ru", "111111", "Employer");
			addUser(context, userManager, "freelancer", "freelancer@ya.ru", "111111", "Freelancer");
            addUser(context, userManager, "incognito", "incognito@ya.ru", "111111", "Incognito");
			addUser(context, userManager, "incognito2", "incognito2@ya.ru", "111111", "Incognito");
			addUser(context, userManager, "withoutDocuments", "withoutDocuments@ya.ru", "111111", "Incognito");
			addUser(context, userManager, "coordinator", "coordinator@ya.ru", "111111", "Coordinator");
		}

		private void AddEmployerFreelancerProblemContractAuto(ApplicationDbContext context, string employerName, string freelancerName)
		{
			try {
				var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
				var employer = addUser(context, userManager, employerName, employerName + "@ya.ru", "111111", "Employer", true);
				var freelancer = addUser(context, userManager, freelancerName, freelancerName + "@ya.ru", "111111", "Freelancer");
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
			var employer = addUser(context, userManager, employerName, employerName + "@ya.ru", "111111", "Employer");
			var freelancer = addUser(context, userManager, freelancerName, freelancerName + "@ya.ru", "111111", "Freelancer");
			var problem = addProblem(context, "[AUTO] " + employerName, "to " + freelancerName, ProblemStatus.Opened, employer);
			var subscription = addSubscription(context, problem, freelancer);
			} catch (Exception e)
			{
                System.Diagnostics.Debug.WriteLine("Error! " + e.Message);
			}
}

		private ApplicationUser addUser(ApplicationDbContext context, UserManager<ApplicationUser> manager, string FIO,
			string email, string password, string role, bool isApprovedByCoordinator = false)
		{
			var user = manager.FindByEmail(email);
			if (user != null)
			{
				return user;
			}
			user = new ApplicationUser { UserName = email, Email = email, FIO = FIO, IsApprovedByCoordinator = isApprovedByCoordinator };
			user.EmailNotificationPolicy = new ApplicationUser.EmailNotificationPolicyModel { IsCommentsEnabled = true, IsContractStatusEnabled = true, IsDocumentsEnabled = true, IsNewApplicantsEnabled = true };
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

		private void AddClosedContracts(ApplicationDbContext context)
		{
			try
			{
				var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
				var employer = addUser(context, userManager, "employer", "employer@ya.ru", "111111", "Employer");
				var freelancer = addUser(context, userManager, "freelancer", "freelancer@ya.ru", "111111", "Freelancer");
				var problem = addProblem(context, "Problem with closed contracts", "description", ProblemStatus.Opened, employer);
				addContract(context, "closed contract1", ContractStatus.Closed, problem, freelancer);
				addContract(context, "closed contract2", ContractStatus.Closed, problem,
				addUser(context, userManager, "freelancer1", "freelancer1@ya.ru", "111111", "Freelancer"));
				addContract(context, "closed contract3", ContractStatus.Closed, problem,
				addUser(context, userManager, "freelancer2", "freelancer2@ya.ru", "111111", "Freelancer"));
			}
			catch (Exception e)
			{
				System.Diagnostics.Debug.WriteLine("Error! " + e.Message);
			}
		}

		private void AddVeryBadProblem(ApplicationDbContext context)
		{
			try
			{
				var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
			var employer = addUser(context, userManager, "employer", "employer@ya.ru", "111111", "Employer");
			var freelancer = addUser(context, userManager, "freelancer", "freelancer@ya.ru", "111111", "Freelancer");
			var problem = addProblem(context, "Problem with closed contracts", "Anyone who reads Old and Middle English literary texts will be familiar with the mid-brown volumes of the EETS, with the symbol of Alfred's jewel embossed on the front cover. Most of the works attributed to King Alfred or to Aelfric, along with some of those by bishop Wulfstan and much anonymous prose and verse from the pre-Conquest period, are to be found within the Society's three series; all of the surviving medieval drama, most of the Middle English romances, much religious and secular prose and verse including the English works of John Gower, Thomas Hoccleve and most of Caxton's prints all find their place in the publications. Without EETS editions, study of medieval English texts would hardly be possible.", ProblemStatus.Opened, employer);
			addContract(context, "As its name states, EETS was begun as a 'club', and it retains certain features of that even now. It has no physical location, or even office, no paid staff or editors, but books in the Original Series are published in the first place to satisfy subscriptions paid by individuals or institutions. This means that there is need for a regular sequence of new editions, normally one or two per year; achieving that sequence can pose problems for the Editorial Secretary, who may have too few or too many texts ready for publication at any one time. Details on a separate sheet explain how individual (but not institutional) members can choose to take certain back volumes in place of the newly published volumes against their subscriptions. On the same sheet are given details about the very advantageous discount available to individual members on all back numbers. In 1970 a Supplementary Series was begun, a series which only appears occasionally (it currently has 24 volumes within it); some of these are new editions of texts earlier appearing in the main series. Again these volumes are available at publication and later at a substantial discount to members. All these advantages can only be obtained through the Membership Secretary (the books are sent by post); they are not available through bookshops, and such bookstores as carry EETS books have only a very limited selection of the many published.", ContractStatus.Closed, problem, freelancer);
			}
			catch (Exception e)
			{
				System.Diagnostics.Debug.WriteLine("Error! " + e.Message);
			}
		}
	}
}
