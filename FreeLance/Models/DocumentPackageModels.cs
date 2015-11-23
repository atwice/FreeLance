using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace FreeLance.Models
{
	public class DocumentPackageModels
	{

		[Key]
		public int Id { get; set; }
		[Required]
		public virtual ApplicationUser Freelancer { get; set; }
		public bool? IsApproved { get; set; }

		public PassportInfo Passport { get; set; }
		public BankInfo Bank { get; set; }
		public GeneralInfo General { get; set; }
		public Photos Photos { get; set; }
	}

	public class PassportInfo
	{
		public string SerialNumber { get; set; }
		public string Date { get; set; }
		public string Address { get; set; }
		public string OfficeName { get; set; }
		public string OfficeCode { get; set; }
		public string BirthDate { get; set; }
		public string BirthPlace { get; set; }
	}

	public class BankInfo
	{
		public string BIC { get; set; }
		public string CorrespondentAccount { get; set; }
		public string PC { get; set; }
		public string Account { get; set; }
	}

	public class GeneralInfo
	{
		public string Name { get; set; }
		public string Surname { get; set; }
		public string MiddleName { get; set; }
		public string Email { get; set; }
		public string Phone { get; set; }
	}

	public class Photos
	{
		public string PassportFace { get; set; }
		public string PassportRegistration { get; set; }
	}
}