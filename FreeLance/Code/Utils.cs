using System.Web.WebPages;

namespace FreeLance.Code
{
	public class Utils
	{
		public static string GetPhotoUrl(string pathFromUser)
		{
			var imgSrc = pathFromUser;
			if (imgSrc.IsEmpty())
			{
				imgSrc = "/Files/avatar.jpg";
			}
			return imgSrc;
		}
	}

}
