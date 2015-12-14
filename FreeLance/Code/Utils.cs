using System.Web;
using System.Web.Mvc;
using System.Web.WebPages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FreeLance.Models;
using Novacode;

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
