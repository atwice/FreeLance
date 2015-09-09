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
				new ProblemModels { Name = "Суперзадача", Description = "Написать ОС", Type = 0 },
				new ProblemModels { Name = "Нарисовать картинку", Description = "Нарисовать картинку в Photoshop", Type = 1 },
				new ProblemModels { Name = "Сделать AbbyyFL", Description = "Implement this thing", Type = 0 },
				new ProblemModels { Name = "Get some sleep", Description = "Поспать", Type = 0 },
				new ProblemModels { Name = "HL3", Description = "Написать HL3", Type = 0 },
				new ProblemModels { Name = "Выполнить домашнюю работу", Description = "Выполнить работу по англ. языку", Type = 0 },
				new ProblemModels { Name = "Написать очередь на C++", Description = "no description", Type = 0 }
				);
		}
	}
}
