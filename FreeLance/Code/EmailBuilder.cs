using Postal;
using FreeLance.Models;

namespace FreeLance.Code
{

	public interface EmailBuilder
	{
		Email PrepareEmail();
	}

	public class OnNewCommentBuilder : EmailBuilder
	{
		private ApplicationUser userTo;
		private ApplicationDbContext db = new ApplicationDbContext();
		private string type;
		private string name;
		private string link;

		/// <param name="type">Can be "contract" or "problem". It define for what a comment was added.</param>
		/// <param name="name">Name of a problem or a contract.</param>
		/// <param name="userId">Define an addressee.</param>
		public OnNewCommentBuilder(string userId, string _type, string _name, string _link)
		{
			userTo = db.Users.Find(userId);
			type = _type;
			name = _name;
			link = _link;
		}

		public Email PrepareEmail()
		{
			dynamic email = new Email("OnNewComment");
			email.To = userTo.Email;
			switch (type)
			{
				case "contract":
					email.Type = "контракту";
					break;
				case "problem":
					email.Type = "задаче";
					break;
			}
			email.Name = name;
			email.Link = link;

			return email;
		}
	}


	public class OnStatusChangeBuilder : EmailBuilder
	{
		private ApplicationUser userTo;
		private ApplicationDbContext db = new ApplicationDbContext();
		private ContractStatus previousStatus;
		private ContractStatus currentStatus;
		private string name;
		private string link;


		/// <param name="name">Name of a problem or a contract.</param>
		/// <param name="userId">Define addressee.</param>
		public OnStatusChangeBuilder(string userId, string _name, string _link,
			ContractStatus _previousStatus, ContractStatus _currentStatus)
		{
			userTo = db.Users.Find(userId);
			name = _name;
			link = _link;
			previousStatus = _previousStatus;
			currentStatus = _currentStatus;
		}

		public Email PrepareEmail()
		{
			dynamic email = new Email("OnStatusChange");
			email.To = userTo.Email;
			email.Name = name;
			email.Link = link;
			email.CurrentStatus = currentStatus;
			email.PreviousStatus = previousStatus;

			return email;
		}
	}

	public class OnNewApplicationBuilder : EmailBuilder
	{
		private ApplicationUser userTo;
		private ApplicationDbContext db = new ApplicationDbContext();
		private string type;
		private string link;


		/// <param name="type">Can be "employerNew", "freelancerNew" or "document".</param>
		/// <param name="userId">Define addressee.</param>
		public OnNewApplicationBuilder(string userId, string _type, string _link)
		{
			userTo = db.Users.Find(userId);
			type = _type;
			link = _link;
		}

		public Email PrepareEmail()
		{
			dynamic email = new Email("OnNewApplication");
			email.To = userTo.Email;
			switch (type)
			{
				case "employerNew":
					email.Type = "одобрение работодателя";
					break;
				case "freelancerNew":
					email.Type = "одобрение фрилансера";
					break;
				case "document":
					email.Type = "одобрение документов";
					break;
			}
			email.Link = link;

			return email;
		}
	}
}