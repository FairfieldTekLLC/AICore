"use strict";
const mySpanElement = document.getElementById('ConversationId');
var conversationId = mySpanElement.textContent;

const connection = new signalR.HubConnectionBuilder()
    .withUrl("/chatHub?username=" + conversationId)
    .build();

connection.on("ReceiveMessage", (sender, message) => {
    console.log(`Message from ${sender}: ${message}`);

    if (message == "Close Window")
    {
        $('#staticBackdrop').modal('hide'); 
        
    }

    var li = document.createElement("li");
    document.getElementById("messagesList").appendChild(li);
    // We can assign user-supplied strings to an element's textContent because it
    // is not interpreted as markup. If you're assigning in any other way, you 
    // should be aware of possible script injection concerns.
    li.textContent = `${message}`;
});

connection.start()
    .then(() => console.log("Connected to SignalR server."))
    .catch(err => console.error("Connection error:", err));

function sendMessage(receiver, message) {
    connection.invoke("SendMessage", conversationId, receiver, message)
        .catch(err => console.error("Send error:", err));
}