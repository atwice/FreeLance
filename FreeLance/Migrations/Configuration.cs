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
				new ProblemModels { Name = "�����������", Description = "�������� ��", Type = 0 },
				new ProblemModels { Name = "���������� ��������", Description = "���������� �������� � Photoshop", Type = 1 },
				new ProblemModels { Name = "������� AbbyyFL", Description = "Implement this thing", Type = 0 },
				new ProblemModels { Name = "Get some sleep", Description = "�������", Type = 0 },
				new ProblemModels { Name = "HL3", Description = "�������� HL3", Type = 0 },
				new ProblemModels { Name = "��������� �������� ������", Description = "��������� ������ �� ����. �����", Type = 0 },
				new ProblemModels { Name = "�������� ������� �� C++", Description = "no description", Type = 0 }
				);
		}
	}
}
