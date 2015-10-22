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

		public OnNewCommentBuilder(string userId)
		{
			userTo = db.Users.Find(userId);
		}

		public Email PrepareEmail()
		{
			dynamic email = new Email("Example");
			email.To = userTo.Email;

			return email;
		}
	}

}