using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;

namespace FreeLance {

	public class ChatHub : Hub {
		public void Hello() {
			Clients.All.hello();
		}
		public void Send(string name, string message) {
			
			// Call the broadcastMessage method to update clients.
			Clients.All.broadcastMessage(name, message);
			
		}

		public void SendToGroup(string groupName, string value) {
			Clients.Group(groupName).hubMessage(value);
		}

		public void JoinGroup(string groupName, string connectionId) {
			if (string.IsNullOrEmpty(connectionId)) {
				connectionId = Context.ConnectionId;
			}
			Groups.Add(connectionId, groupName);
			Clients.All.hubMessage(connectionId + " joined group " + groupName);
		}

		public void LeaveGroup(string groupName, string connectionId) {
			if (string.IsNullOrEmpty(connectionId)) {
				connectionId = Context.ConnectionId;
			}

			Groups.Remove(connectionId, groupName);
			Clients.All.hubMessage(connectionId + " left group " + groupName);
		}

	}
}