using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using FreeLance.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity.EntityFramework;
using FreeLance.Controllers;

namespace Common.Connections {
	public class AuthorizeEchoConnection : PersistentConnection {
		protected override bool AuthorizeRequest(IRequest request) {
			return request.User != null && request.User.Identity.IsAuthenticated;
		}
	}
}

namespace FreeLance {

	

	[Authorize]
	public class ChatHub : Hub {
		private ApplicationDbContext db = new ApplicationDbContext();

		public void Hello() {
			Clients.All.hello();
		}
		/*
		public void Send(string name, string message) {
			
			// Call the broadcastMessage method to update clients.
			Clients.All.broadcastMessage(name, message);
			
		}
		*/
		/*
		public void SendToChat(string groupName, string value) {
			if( !checkAuthority(groupName) ) {
				throw new UnauthorizedAccessException();
			}
            Clients.Group(groupName).hubMessage(value);
		}
		*/
		public void SendMessage(int chatId, int? parentId, string text) {
			string userId = Context.User.Identity.GetUserId();
            if (!ChatController.HasAccess(userId, chatId)) {
				throw new UnauthorizedAccessException();
			}
			ChatController.ChatResponse response = ChatController.SendMessage(chatId, parentId, userId, text);
			if (response.IsOk) {
				Clients.Group(chatId.ToString()).newMessages(response.Body);
			} else {
				Clients.Client(Context.ConnectionId).errorSendMessage(response.Body);
			}
		}


		public void JoinChat(int chatId) {
			string userId = Context.User.Identity.GetUserId();
			if (!ChatController.HasAccess(userId, chatId)) {
				throw new UnauthorizedAccessException();
			}
			ChatController.ChatResponse response = ChatController.GetChatMessages(chatId, userId);
			if (response.IsOk) {
				Clients.Client(Context.ConnectionId).newMessages(response.Body);
			} else {
				Clients.Client(Context.ConnectionId).errorSendMessage(response.Body);
			}
			Groups.Add(Context.ConnectionId, chatId.ToString());
			//Clients.All.hubMessage(Context.ConnectionId + " joined group " + groupName);
		}

		public void LeaveChat(int chatId) {
			string userId = Context.User.Identity.GetUserId();
			if (!ChatController.HasAccess(userId, chatId)) {
				throw new UnauthorizedAccessException();
			}

			Groups.Remove(Context.ConnectionId, chatId.ToString());
			//Clients.All.hubMessage(Context.ConnectionId + " left group " + groupName);
		}
		/*
		private bool checkAuthority(string objectId, ChatController.ChatOwner owner) {
			int taskId = int.Parse(groupName);
			var userIdentity = Context.User.Identity;

			if( userIdentity == null ) {
				return false;
			}

			ApplicationUser user = null;
			bool isEmployer = false;
			bool isFreelancer = false;

			using (var userStore = new UserStore<ApplicationUser>(db))
			using (var userManager = new ApplicationUserManager(userStore)) {
				user = userManager.FindById(userIdentity.GetUserId());

				isEmployer = userManager.IsInRole(userIdentity.GetUserId(), "Employer");
				isFreelancer = userManager.IsInRole(userIdentity.GetUserId(), "Freelancer");
			}

			if(isEmployer) {
				return db.ProblemModels.Find(taskId).Employer.Id == user.Id;
			} else if( isFreelancer ) {
				return true;
			} else {
				// TODO
				return true;
			}
			
			return true;
		}*/
	}
}