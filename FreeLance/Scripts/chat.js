( function( $ ) {

// MessageObject
// -photoUrl - url на фотографию человека
// -text - текст сообщения
// -name - имя человека
// -isStarred - отмечать ли иконку звездочкой

// TextObject
// -text - текст
// -style: error || default || none

// Конфиг, с функциями построения дома для сообщений
var DEFAULT_CONFIG = {
	formMessageDom: function( messageObject ) {
		var dom = $("<div val='" + messageObject.id + "' class='chat-message-container row'></div>");

		// Собираем левую часть
		var messageLeft = $("<div class='chat-message-left'><img src='" + messageObject.photoUrl +"'></img></div>");
		if( messageObject.isStarred ) {
			messageLeft.append( "<i class='glyphicon glyphicon-star chat-star'></i>" );
		}
		dom.append( messageLeft );

		var messageRight = $("<div class='chat-message-right'></div>");
		// Имя пользователя
		var name = $("<div class='chat-name'></div>");
		name.text( messageObject.name );
		// Текст сообщения
		var text = $("<div class='chat-text'></div>");
		text.text(messageObject.text);

		// Собираем правую часть
		messageRight.append(name);
		messageRight.append(text);

		// Удаление
		if (messageObject.canHide) {
			var buttonText = messageObject.hide ? 'Show' : 'Hide';
			var buttonId = messageObject.id;
			var hideButton = $("<div class='col-md-12'><button val='" + buttonId + "' class='btn btn-primary pull-right'>" +
				buttonText + "</button></div>");
			hideButton.click(function () {
				messageObject.hideFunction(buttonId, $(this).text() == 'Hide');
			});
			messageRight.append(hideButton);
		}
		dom.append( messageRight );
		return dom;
	},
	formTextDom: function( textObject ) {
		var cssClass = "chat-service-text ";
		if( textObject.style == "error") {
			cssClass+="chat-error";
		} else {
			cssClass+="chat-default";
		}
		var dom = $("<div class='" + cssClass + "'></div>");
		dom.text( textObject.text );
		return dom;
	}
};

function Chat( $selector, config ) {
	var self = this;
	
	// Запоминаем параметры
	self.$selector = $selector;
	self.config = {};
	$.extend( self.config, DEFAULT_CONFIG );
	$.extend( self.config, config );

	self.addMessage = function( messageObject ) {
		self.$selector.append( self.config.formMessageDom( messageObject ) );
	};

	self.addText = function( textObject ) {
		self.$selector.append( self.config.formTextDom( textObject ));
	};

	self.hideMessage = function (messageId, hide, show) {
		var buttonText = hide ? 'Show' : 'Hide';
		self.$selector.find("button[val='" + messageId + "']").text(buttonText);
		if (!show) {
			var vis = hide ? 'hidden' : 'visible';
			$selector.find(".chat-message-container[val='" + messageId + "']").css('visibility', vis);
		}
	};

	self.clear = function () {
		self.$selector.empty();
	}
};

$.fn.extend( {
	chat: function( config ) {
		return new Chat( $( this ), config );
	}
});

} ) ( jQuery );