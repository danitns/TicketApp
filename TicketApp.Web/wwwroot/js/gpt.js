var chatHeader = document.getElementsByClassName('chat-header')[0];
chatHeader.onclick = () => {
    var chatBody = document.getElementsByClassName('chat-body')[0];
    if (chatBody.style.display == 'none')
        chatBody.style.display = 'block';
    else chatBody.style.display = 'none';
}

$(document).click(function (event) {
    if ($(event.target).hasClass('chat-option')) {
        let text = $(event.target).text();
        toggleOptions()
        addMessageInContainer(text, 'chat-request');
        gptResponse(text);
    }
});


async function gptResponse(prompt) {

    var response = await fetch("/gpt-response", {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(prompt)
    });

    if (response.ok) {
        const jsonResponse = await response.json();
        if (jsonResponse.success) {
            addMessageInContainer(jsonResponse.success, 'chat-response');
            toggleOptions();
        }
    }
}

function toggleOptions() {
    var optionContainer = document.getElementById('chat-options-container');
    if (optionContainer.style.display == 'none')
        optionContainer.style.display = 'block';
    else optionContainer.style.display = 'none';
}

function addMessageInContainer(data, className) {
    var containerBody = document.getElementById('chat-replies-container');
    var response = document.createElement('div');
    response.innerHTML = data;
    response.className = className + " border rounded m-2 p-2";
    containerBody.appendChild(response);
}
