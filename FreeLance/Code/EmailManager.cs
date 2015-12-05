using Postal;

namespace FreeLance.Code
{
	public class EmailManager
	{
		public static void Send(EmailBuilder builder)
		{
			if(builder.SendingCondition())
			{
				Email email = builder.PrepareEmail();
				email.Send();
			}
		}
	}
	
}