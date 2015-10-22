using Postal;

namespace FreeLance.Code
{
	public class EmailManager
	{
		public static void Send(EmailBuilder builder)
		{
			Email email = builder.PrepareEmail();
			email.Send();
		}
	}
	
}