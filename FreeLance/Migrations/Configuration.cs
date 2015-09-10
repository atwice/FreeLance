namespace FreeLance.Migrations
{
	using System;
	using System.Data.Entity;
	using System.Data.Entity.Migrations;
	using FreeLance.Models;
	using System.Linq;

	internal sealed class Configuration : DbMigrationsConfiguration<FreeLance.Models.ApplicationDbContext>
	{
		public Configuration()
		{
			AutomaticMigrationsEnabled = true;
			ContextKey = "FreeLance.Models.ApplicationDbContext";
		}

		protected override void Seed(FreeLance.Models.ApplicationDbContext context)
		{
			SeedProblems(context);
		}

		private void SeedProblems(FreeLance.Models.ApplicationDbContext context)
		{
			context.ProblemModels.AddOrUpdate(
				p => p.Name,
				new ProblemModels { Name = "Supertask", Description = "Write Operation System.", Type = ProblemStatus.Opened },
				new ProblemModels { Name = "Android design", Description = "Make material design for android application RemindMe.", Type = ProblemStatus.Opened },
				new ProblemModels { Name = "FreeLance site", Description = "Improve FL site.", Type = ProblemStatus.Opened },
				new ProblemModels { Name = "Have a rest", Description = "Enjoy your day.", Type = ProblemStatus.Opened },
				new ProblemModels { Name = "Write Half-life3", Description = "Some text.", Type = ProblemStatus.Opened },
				new ProblemModels { Name = "English translations", Description = "Translate some texts from english to russian.", Type = ProblemStatus.InProgress },
				new ProblemModels { Name = "Implement queue", Description = "Language : ASSEMBLER", Type = ProblemStatus.Closed }
				);
		}
	}
}
