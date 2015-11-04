using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FreeLance.Models;
using Microsoft.AspNet.Identity;
using System.Net;
using System.Data;
using System.Data.Entity;

namespace FreeLance.Controllers
{
	[Authorize]
	public class ChatController : Controller
    {
		private ApplicationDbContext db = new ApplicationDbContext();

		public enum ChatOwner {
			Problem, Contract
		}

		public class ChatVR {
			public string ObjectId { set; get; }
			public ChatOwner Owner { get; set; }
		} 

		public ActionResult ProblemChat(int problemId) {
			return PartialView("CreateChat", new ChatVR {
                ObjectId = problemId.ToString(),
				Owner = ChatOwner.Problem
			});
		}

		public ActionResult ContractChat(int contractId) {
			return PartialView("CreateChat", new ChatVR {
                ObjectId = contractId.ToString(),
				Owner = ChatOwner.Contract
			});
		}

		[HttpPost]
		public ActionResult GetChatMessages(string objectId, ChatOwner owner) {
			try {
				switch (owner) {
					case ChatOwner.Problem:
						return getProblemMessages(Int32.Parse(objectId));
					case ChatOwner.Contract:
						return getContractMessages(Int32.Parse(objectId));
					default:
						return Json(new { Status = "Error", Reason = "Unexpected Owner" });
				}
			} catch (Exception e) {
				return Json(new { Status = "Error", Reason = e.Message });
			}
		}

		[HttpPost]
		public ActionResult SendMessage(string objectId, ChatOwner owner, int? parentId, string text) {
			try {
				switch (owner) {
					case ChatOwner.Problem:
						return sendProblemMessage(Int32.Parse(objectId), parentId, text);
					case ChatOwner.Contract:
						return sendContractMessage(Int32.Parse(objectId), parentId, text);
					default:
						return Json(new { Status = "Error", Reason = "Unexpected Owner" });
				}
			} catch (Exception e) {
				return Json(new { Status = "Error", Reason = e.Message });
			}
		}
		/*
		[HttpPost]
		public ActionResult DeleteMessage(string objectId, ChatOwner owner, int messageId) {
			try {
				switch (owner) {
					case ChatOwner.Problem:
						return deleteProblemMessage(Int32.Parse(objectId), messageId);
					case ChatOwner.Contract:
						return deleteContractMessage(Int32.Parse(objectId), messageId);
					default:
						return Json(new { Status = "Error", Reason = "Unexpected Owner" });
				}
			} catch (Exception e) {
				return Json(new { Status = "Error", Reason = e.Message });
			}
		}*/

		private ActionResult sendProblemMessage(int problemId, int? parentId, string text) {
			ProblemModels problem = db.ProblemModels.Find(problemId);
			string userId = User.Identity.GetUserId();
			if (problem == null || !(problem.Employer.Id == userId || User.IsInRole("Freelancer"))) {
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}
			ProblemChat problemChat = db.ProblemChats.Where(pc => pc.Problem.ProblemId == problem.ProblemId).SingleOrDefault();
			if (problemChat == null) {
				Chat chat = db.Chats.Add(new Chat { });
				problemChat = new Models.ProblemChat { ChatId = chat.Id, Problem = problem };
				db.ProblemChats.Add(problemChat);
				db.SaveChanges();
			}
			return sendMessage(problemChat.ChatId, parentId, text);
		}

		private ActionResult sendContractMessage(int contractId, int? parentId, string text) {
			ContractModels contract = db.ContractModels.Find(contractId);
			string userId = User.Identity.GetUserId();
			if (contract == null || !(contract.Freelancer.Id == userId || contract.Problem.Employer.Id == userId)) {
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}
			ContractChat contractChat = db.ContractChats.Where(ch => ch.Contract.ContractId == contractId).SingleOrDefault();
			if (contractChat == null) {
				Chat chat = db.Chats.Add(new Chat { });
				contractChat = new Models.ContractChat { ChatId = chat.Id, Contract = contract };
				db.ContractChats.Add(contractChat);
				db.SaveChanges();
			}
			return sendMessage(contractChat.ChatId, parentId, text);
		}

		private ActionResult getProblemMessages(int problemId) {
			ProblemModels problem = db.ProblemModels.Find(problemId);
			string userId = User.Identity.GetUserId();
			if (problem == null || !(problem.Employer.Id == userId || User.IsInRole("Freelancer"))) {
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}
			ProblemChat problemChat = db.ProblemChats.Where(pc => pc.Problem.ProblemId == problem.ProblemId).SingleOrDefault();
			int? chatId = null;
			if (problemChat != null) {
				chatId = problemChat.ChatId;
			}
            return getChatMessages(chatId);
		}

		private ActionResult getContractMessages(int contractId) {
			ContractModels contract = db.ContractModels.Find(contractId);
			string userId = User.Identity.GetUserId();
			if (contract == null || !(contract.Freelancer.Id == userId || contract.Problem.Employer.Id == userId)) {
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}
			ContractChat contractChat = db.ContractChats.Where(ch => ch.Contract.ContractId == contractId).SingleOrDefault();
			int? chatId = null;
			if (contractChat != null) {
				chatId = contractChat.ChatId;
			}
			return getChatMessages(chatId);
		}
		/*
		private ActionResult deleteProblemMessage(int problemId, int messageId) {
			ProblemModels problem = db.ProblemModels.Find(problemId);
			string userId = User.Identity.GetUserId();
			if (problem == null || !(problem.Employer.Id == userId || User.IsInRole("Freelancer"))) {
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}
			return deleteChatMessage(problem.ChatId, messageId);
		}

		private ActionResult deleteContractMessage(int contractId, int messageId) {
			ContractModels contract = db.ContractModels.Find(contractId);
			string userId = User.Identity.GetUserId();
			if (contract == null || !(contract.Freelancer.Id == userId || contract.Problem.Employer.Id == userId)) {
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}
			return deleteChatMessage(contract.ChatId, messageId);
		}*/

		private ActionResult sendMessage(int chatId, int? parentId, string text) {
			if (parentId != null) {
				ChatMessage parentMessage = db.ChatMessages.Find(parentId);
				if (parentMessage == null || parentMessage.ChatId != chatId) {
					return Json(new { Status = "Error", Reason = "Invalid parent Id" });
				}
			}
			try {
				ChatMessage msg = db.ChatMessages.Add(new ChatMessage {
					ChatId = chatId,
					ParentId = parentId,
					Content = text,
					User = db.Users.Find(User.Identity.GetUserId()),
					CreationDate = DateTime.Now,
					ModificationDate = null
				});
				db.SaveChanges();
				return Json(new { Status = "Ok", Result = fillMessage(msg) });
			} catch (Exception e) {
				return Json(new { Status = "Error", Reason = e.Message });
			}
		}

		private ActionResult getChatMessages(int? chatId) {
			try {
				return Json(new {
					Status = "Ok",
					Result = db.ChatMessages.Where(msg => msg.ChatId == chatId).OrderBy(msg => msg.CreationDate)
							   .Include(msg => msg.User).Select(fillMessage).ToArray()
				});
			} catch (Exception e) {
				return Json(new { Status = "Error", Reason = e.Message });
			}
		}

		private ActionResult deleteChatMessage(int? chatId, int? messageId) {
			try {
				ChatMessage message = db.ChatMessages.Find(messageId);
				if (message.ChatId != chatId) {
					return Json(new { Status = "Error", Reason = "Invalid Chat Id for ChatMessage" });
				}
				db.ChatMessages.Remove(message);
				db.SaveChanges();
				return Json(new {
					Status = "Ok",
					Result = "Deleted"
				});
			} catch (Exception e) {
				return Json(new { Status = "Error", Reason = e.Message });
			}
		}

		private Object fillMessage(ChatMessage msg) {
			return new {
				Id = msg.Id,
				Author = msg.User.FIO,
				Comment = msg.Content,
				ParentId = msg.ParentId,
				UserAvatar = "/Content/Avatars/default.png",
				CreatedByCurrentUser = msg.User.Id == User.Identity.GetUserId(),
				CanReply = true,
				Date = (Int64) (msg.CreationDate.Subtract(new DateTime(1970, 1, 1, 0, 0, 0))).TotalMilliseconds,
				//Modified = true,
			};
		}
	}
}