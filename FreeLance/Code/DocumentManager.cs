using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Novacode;

namespace FreeLance.Code
{
    public class DocumentManager
    {
        public void FillWorkTemplate(string username, string fileName)
        {
            using (DocX document = DocX.Load(fileName))
            {
                document.ReplaceText("%%NAME%%", username);
                document.SaveAs("test");
            }
        }

    }
}


