async function checkForGift(containerId) {
    var response = await fetch(`/available-gift`, {
        method: 'GET',
        headers: {
            'Content-Type': 'application/json'
        }
    });

    if (response.ok) {
        const jsonResponse = await response.json();
        if (jsonResponse.success == true) {
            createButton(containerId);
        }
    }

    function createButton(containerId) {
        var buttonDiv = document.createElement('div');
        buttonDiv.className = "border rounded text-center";
        var text = document.createElement('p');
        text.innerText = "You have a gift available from your subscription.";
        text.style.marginBottom = 0;
        text.className = "p-1";


        var button = document.createElement('button');
        button.className = "btn btn-primary";
        button.innerHTML = 'Redeem your free gift';
        button.onclick = () => {
            getGift();
        }

        buttonDiv.appendChild(text);
        buttonDiv.appendChild(button);
        buttonDiv.style.width = 'fit-content';
        container = document.getElementById(containerId);
        container.appendChild(buttonDiv);
    }

    async function getGift() {
        var response = await fetch(`/free-gift`, {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json'
            }
        });

        if (response.ok) {
            const jsonResponse = await response.json();
            if (jsonResponse.success) {
                location.replace(jsonResponse.success);
            }
        }
    }
}