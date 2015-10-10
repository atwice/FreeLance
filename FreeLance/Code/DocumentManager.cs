using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FreeLance.Models;
using Novacode;

namespace FreeLance.Code
{
    public class DocumentManager
    {
        public static string fillContractTemplate(ApplicationUser user, LawContractTemplate lawContractTemplate)
        {
            using (DocX doc = DocX.Load(lawContractTemplate.Path))
            {
                string fullNameTemplate = "%ФАМИЛИЯ% %ИМЯ% %ОТЧЕСТВО%";
                string fullNameTemplate2 = "%ФИО ИСПОЛНИТЕЛЯ%";
                string freelancerEmailTemplate = "%EMAIL_ИСПОЛНИТЕЛЯ%";
                string fullName = user.FIO == null ? "У ВАС НЕТ ИМЕНИ" : user.FIO;
                string email = user.UserName;
                doc.ReplaceText(fullNameTemplate, fullName);
                doc.ReplaceText(fullNameTemplate2, fullName);
                doc.ReplaceText(freelancerEmailTemplate, email);
                doc.ReplaceText("%ДАТАСЕГОДНЯ%", DateTime.Today.ToString("dd/MM/yyyy"));
                string contractName = "T" + 0 + "U" + user.Id + ".docx";
                string pathToContract = AppDomain.CurrentDomain.BaseDirectory + "/Files/LawContracts/" + contractName;
                doc.SaveAs(pathToContract);
                return pathToContract;
            }
        }

    }
}


