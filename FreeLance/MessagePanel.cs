using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FreeLance {
	public class MessagePanel {
		public struct Message {
			public string Text { get; set; }
			public bool isVisible { get; set; }
		}

		private static Message message;

		public static void SetMessage(string text) {
			message.Text = text;
			message.isVisible = true;
		}

		public static Message GetMessage() {
			Message msg = message;
			message.isVisible = false;
			return msg;
		}
	}
}