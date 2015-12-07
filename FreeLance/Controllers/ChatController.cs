﻿using System;
using System.Linq;
using FreeLance.Code;
using System.Web.Mvc;
using FreeLance.Models;
using Microsoft.AspNet.Identity;
using System.Net;
using System.Data;
using System.Data.Entity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Collections.Generic;

namespace FreeLance.Controllers
{
	[System.Web.Mvc.Authorize]
	public class ChatController : Controller
    {
		private static ApplicationDbContext db = new ApplicationDbContext();

		public class ChatVR {
			public int ChatId { get; set; }
			public string UserName { get; set; }
			public bool CanHide { get; set; }
			public string PhotoPath { get; set; }
		}

		public class ChatUserInfo {
			public int UnreadMessagesCount;
		}

		[NonAction]
		public static ChatUserInfo CalcUserInfo(string userId, int chatId) {
			if (!db.Chats.Where(x => x.Id == chatId).Any()) {
				return null;
			}
			ChatUserStatistic statistic = db.ChatUserStatistics
				.Where(x => x.ChatId == chatId && x.User.Id == userId).SingleOrDefault();
			DateTime lastVisit = DateTime.MinValue;
			if (statistic != null) {
				lastVisit = statistic.LastVisit;
			}
			return new ChatUserInfo {
				UnreadMessagesCount = db.ChatMessages.Where(x => x.ChatId == chatId && x.CreationDate > lastVisit).Count()
			};
		} 

		[NonAction]
		public static int FindProblemChatId(int problemId) {
			ProblemChat problemChat = db.ProblemChats.Where(x => x.Problem.ProblemId == problemId).
                Include(x => x.Chat).SingleOrDefault();
			if (problemChat == null) {
				ProblemModels problem = db.ProblemModels.Find(problemId);
				if (!problem.IsHidden)
				{
					Chat chat = db.Chats.Add(new Chat { Owner = ChatOwner.Problem });
					problemChat = new Models.ProblemChat { Chat = chat, Problem = problem };
					db.ProblemChats.Add(problemChat);
					db.SaveChanges();
				}				
			}
			return problemChat.Chat.Id;
		}

		[NonAction]
		public static int FindContractChatId(int contractId) {
			ContractChat contractChat = db.ContractChats.Where(x => x.Contract.ContractId == contractId)
				.Include(x => x.Chat).SingleOrDefault();
			if (contractChat == null) {
				ContractModels contract = db.ContractModels.Find(contractId);
				Chat chat = db.Chats.Add(new Chat { Owner = ChatOwner.Contract });
				contractChat = new Models.ContractChat { Chat = chat, Contract = contract };
				db.ContractChats.Add(contractChat);
				db.SaveChanges();
			}
			return contractChat.Chat.Id;
		}

		public ActionResult ProblemChat(int problemId) {
			ApplicationUser user = db.Users.Find(User.Identity.GetUserId());
			try {
				return PartialView("CreateChat", new ChatVR
				{
					ChatId = FindProblemChatId(problemId),
					UserName = user.FIO,
					CanHide = checkUserIsInRole(user, "Coordinator"),
					PhotoPath = Utils.GetPhotoUrl(user.PhotoPath)
				});
			} catch (Exception e) {
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest, e.Message);
			}
		}

		public ActionResult ContractChat(int contractId) {
			ApplicationUser user = db.Users.Find(User.Identity.GetUserId());
            try {
				return PartialView("CreateChat", new ChatVR {
					ChatId = FindContractChatId(contractId),
					UserName = user.FIO,
					CanHide = checkUserIsInRole(user, "Coordinator"),
					PhotoPath = Utils.GetPhotoUrl(user.PhotoPath)
				});
			} catch (Exception e) {
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest, e.Message);
			}
		}

		[NonAction]
		public static bool HasAccess(string userId, int chatId) {
			ApplicationUser user = db.Users.Find(userId);
			Chat chat = db.Chats.Find(chatId);
			if (user == null || chat == null) {
				return false;
			}
			switch (chat.Owner) {
				case ChatOwner.Problem:
					return hasAccessToProblem(user, chatId);
				case ChatOwner.Contract:
					return hasAccessToContract(user, chatId);
                default:
					return false;
			}
		}

		[NonAction]
		public static bool CanHideMessages(string userId, int chatId) {
			ApplicationUser user = db.Users.Find(userId);
			Chat chat = db.Chats.Find(chatId);
			if (user == null || chat == null) {
				return false;
			}
			return checkUserIsInRole(user, "Coordinator");
		}

		private static bool checkUserIsInRole(ApplicationUser user, string roleName) {
			ApplicationDbContext context = new ApplicationDbContext();
            IdentityRole role = context.Roles.Where(r => r.Name == roleName).Single();
			return context.Users.Find(user.Id).Roles.Where(r => r.RoleId == role.Id).Any();
		}

		private static bool hasAccessToProblem(ApplicationUser user, int chatId) {
			ProblemChat problemChat = db.ProblemChats.Where(x => x.Chat.Id == chatId).SingleOrDefault();
			if (problemChat == null) {
				return false;
			}
			if (checkUserIsInRole(user, "Freelancer") || checkUserIsInRole(user, "Coordinator") 
					|| problemChat.Problem.Employer.Id == user.Id) {
				return true;
			}
			return false;
		}

		private static bool hasAccessToContract(ApplicationUser user, int chatId) {
			ContractChat contrctChat = db.ContractChats.Where(x => x.Chat.Id == chatId).SingleOrDefault();
			if (contrctChat == null) {
				return false;
			}
			if (checkUserIsInRole(user, "Coordinator") || 
					contrctChat.Contract.Freelancer.Id == user.Id || contrctChat.Contract.Problem.Employer.Id == user.Id) {
				return true;
			}
			return false;
		}

		public class ChatResponse {
			public Object Body { get; set; }
			public bool IsOk { get; set; }
		}

		private static void updateLastVisitDate(int chatId, ApplicationUser user) {
			ChatUserStatistic statistic = db.ChatUserStatistics
				.Where(x => x.ChatId == chatId && x.User.Id == user.Id).SingleOrDefault();
			if (statistic == null) {
				statistic = new ChatUserStatistic { ChatId = chatId, User = user, LastVisit = DateTime.Now };
				db.ChatUserStatistics.Add(statistic);
			} else {
				statistic.LastVisit = DateTime.Now;
			}
			db.SaveChanges();
		}


		private static void SendEmailNotification(int chatId, string userId)
		{
			ContractChat contractChat = db.ContractChats.Where(x => x.Chat.Id == chatId).SingleOrDefault();
			ProblemChat problemChat = db.ProblemChats.Where(x => x.Chat.Id == chatId).SingleOrDefault();

			if (contractChat != null)
			{
				string employerId = contractChat.Contract.Problem.Employer.Id;
				string freelancerId = contractChat.Contract.Freelancer.Id;
				string userNotifyId = (employerId != userId) ? employerId : freelancerId;

				string problemName = contractChat.Contract.Problem.Name;
				int contractId = contractChat.Contract.ContractId;

				EmailManager.Send(new OnNewCommentBuilder(userNotifyId, "contract", problemName, "/Contract/Details/" + contractId.ToString()));
			}

			if (problemChat != null)
			{
				string employerId = problemChat.Problem.Employer.Id;
				List<SubscriptionModels> subscribers = problemChat.Problem.Subscriptions.ToList();

				string problemName = problemChat.Problem.Name;
				int problemId = problemChat.Problem.ProblemId;

				if (employerId != userId)
				{
					EmailManager.Send(new OnNewCommentBuilder(employerId, "problem", problemName, "/Problem/Details/" + problemId.ToString()));
				}

				foreach (var subcriber in subscribers)
				{
					string subscriberId = subcriber.Freelancer.Id;
					if (subscriberId != userId)
					{
						EmailManager.Send(new OnNewCommentBuilder(subscriberId, "problem", problemName, "/Problem/Details/" + problemId.ToString()));
					}
				}
			}
		}


		[NonAction]
		public static ChatResponse SendMessage(int chatId, int? parentId, string userId, string text) {
			if (parentId != null) {
				ChatMessage parentMessage = db.ChatMessages.Find(parentId);
				if (parentMessage == null || parentMessage.ChatId != chatId) {
					return new ChatResponse { Body = new { Status = "Error", Reason = "Invalid parent Id" }, IsOk = false };
				}
			}
			try {
				ChatMessage msg = db.ChatMessages.Add(new ChatMessage {
					ChatId = chatId,
					ParentId = parentId,
					Content = text,
					User = db.Users.Find(userId),
					CreationDate = DateTime.Now,
					ModificationDate = null,
					IsHidden = false
				});
				db.SaveChanges();

				SendEmailNotification(chatId, userId);

				ApplicationUser user = db.Users.Find(userId);
				updateLastVisitDate(chatId, user);
				return new ChatResponse { Body = new { Status = "Ok", Result = new[] { fillMessage(msg) } }, IsOk = true };
			} catch (Exception e) {
				return new ChatResponse { Body = new { Status = "Error", Reason = e.Message }, IsOk = false };
			}
		}

		[NonAction]
		public static ChatResponse GetChatMessages(int chatId, string userId) {
			try {
				ApplicationUser user = db.Users.Find(userId);
				bool showHidden = checkUserIsInRole(user, "Coordinator");
				updateLastVisitDate(chatId, user);
				return new ChatResponse { IsOk = true, Body = new {
					Status = "Ok",
					Result = db.ChatMessages.Where(msg => msg.ChatId == chatId && (!msg.IsHidden || showHidden))
											.OrderBy(msg => msg.CreationDate)
											.Include(msg => msg.User).Select(fillMessage).ToArray()
				}};
			} catch (Exception e) {
				return new ChatResponse { Body = new { Status = "Error", Reason = e.Message }, IsOk = false };
			}
		}
		
		[NonAction]
		public static ChatResponse HideChatMessage(int chatId, int messageId, bool hide) {
			try {
				ChatMessage message = db.ChatMessages.Find(messageId);
				if (message.ChatId != chatId) {
					return new ChatResponse {
						IsOk = false,
						Body = new { Status = "Error", Reason = "Invalid Chat Id for ChatMessage" }
					};
				}
				message.IsHidden = hide;
				db.SaveChanges();
				return new ChatResponse {
					IsOk = true,
					Body = new { Status = "Ok", Hide = hide, MessageId = messageId }
				};
			} catch (Exception e) {
				return new ChatResponse {
					IsOk = false,
					Body = new { Status = "Error", Reason = e.Message }
				};
			}
		}
		
		private static Object fillMessage(ChatMessage msg) {
			return new {
				Id = msg.Id,
				Author = msg.User.FIO,
				Comment = msg.Content,
				ParentId = msg.ParentId,
				UserAvatar = Utils.GetPhotoUrl(msg.User.PhotoPath),
				IsStarred = checkUserIsInRole(msg.User, "Employer") || checkUserIsInRole(msg.User, "Coordinator"),
				CanReply = true,
				Date = (Int64)(msg.CreationDate.Subtract(new DateTime(1970, 1, 1, 0, 0, 0))).TotalMilliseconds,
				Hidden = msg.IsHidden
			};
		}
	}
}