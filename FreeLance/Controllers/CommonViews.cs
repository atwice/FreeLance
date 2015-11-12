using FreeLance.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FreeLance.Controllers
{
	public class FreelancerViewModel
	{
		private ApplicationDbContext db = new ApplicationDbContext();

		public String Name { get; set; }
		public string Id { get; set; }
		public string Email { get; set; }
		public int ClosedContractsCount { get; set; }
		public int OpenContractsCount { get; set; }
		public decimal Rate { get; set; }

		public FreelancerViewModel(string id)
		{
			ApplicationUser freelancer = db.Users.Find(id);
			List<SmallContractInfoModel> contracts = db.ContractModels
				.Where(
					c => c.Freelancer.Id == id)
				.Select(
				c => new SmallContractInfoModel
				{
					Rate = c.Rate,
					Status = c.Status
				})
				.ToList();

			Rate = 0;
			Name = freelancer.FIO;
			Email = freelancer.Email;
			ClosedContractsCount = 0;
			OpenContractsCount = 0;
			Id = id;
			decimal rate = 0;
			int cancelByFreelancer = 0;
			foreach (var contract in contracts)
			{
				if (contract.Status == ContractStatus.Closed
					|| contract.Status == ContractStatus.Failed
					|| contract.Status == ContractStatus.СancelledByEmployer
					|| contract.Status == ContractStatus.ClosedNotPaid)
				{
					rate += contract.Rate;
					ClosedContractsCount += 1;
				}
				else if (contract.Status == ContractStatus.InProgress ||
				  contract.Status == ContractStatus.Opened)
				{
					OpenContractsCount += 1;
				}
				else if (contract.Status == ContractStatus.СancelledByFreelancer)
				{
					cancelByFreelancer += 1;
				}
			}
			if (ClosedContractsCount != 0)
			{
				Rate = rate / ClosedContractsCount;
			}
			ClosedContractsCount += cancelByFreelancer;
		}
    }

	public class SmallContractInfoModel
	{
		public ContractStatus Status { get; set; }
		public decimal Rate { get; set; }
	}

	
}