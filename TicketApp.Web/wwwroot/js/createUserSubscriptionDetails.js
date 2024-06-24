async function createUserSubscriptionDetails(parentNodeId, buyButtonsClassName) {
    try {
        const response = await fetch("/get-user-subscription", {
            method: 'GET',
        });

        if (response.ok) {
            const jsonResponse = await response.json();

            if (jsonResponse.noSubscription) {
                displayNoSubscription(parentNodeId);
            } else {
                showSubscriptionDetails(jsonResponse, parentNodeId, buyButtonsClassName);
            }

        } else {
            console.log("Error: " + response.statusText);
        }
    } catch (error) {
        console.error("An error occurred:", error);
    }
}

function displayNoSubscription(parentNode) {
    let message = document.createElement('p');
    message.innerHTML = 'Buy a subscription right now!';
    parentNode.appendChild(message);
}

function showSubscriptionDetails(subscription, parentNodeId, buyButtonsClassName) {
    let parentNode = document.getElementById(parentNodeId);
    let divElement = document.createElement("div");
    divElement.className = "border rounded mx-auto";
    divElement.style.width = 'fit-content';
    let title = document.createElement('h3');
    title.textContent = 'Your last subscription:';

    let name = document.createElement('p');
    name.innerHTML = 'Name: ' + subscription.name;

    let price = document.createElement('p');
    price.innerHTML = 'Price: ' + subscription.price + ' RON';

    let discount = document.createElement('p');
    discount.innerHTML = 'Discount: ' + subscription.discount + '%';

    let processingD = document.createElement('p');
    let originalDate = new Date(subscription.processingDate);
    let formattedDate = formatDate(originalDate);
    processingD.innerHTML = 'From: ' + formattedDate;

    let status = document.createElement('p');
    status.innerHTML = 'Status: ' + subscription.status;

    divElement.appendChild(title);
    divElement.appendChild(name);
    divElement.appendChild(price);
    divElement.appendChild(discount);
    divElement.appendChild(processingD);
    divElement.appendChild(status);

    if (subscription.status == 'active') {
        var cancelButton = document.createElement('button');
        cancelButton.className = "btn btn-primary"
        cancelButton.textContent = 'Cancel Subscription';
        cancelButton.onclick = () => {
            cancelSubscription();
        }
        divElement.appendChild(cancelButton);
        disableButtons(buyButtonsClassName);
    }
    parentNode.appendChild(divElement);

    function formatDate(date) {
        let day = date.getDate();
        let month = date.getMonth() + 1;
        let year = date.getFullYear();
        let hours = date.getHours();
        let minutes = date.getMinutes();

        day = day < 10 ? '0' + day : day;
        month = month < 10 ? '0' + month : month;
        hours = hours < 10 ? '0' + hours : hours;
        minutes = minutes < 10 ? '0' + minutes : minutes;

        return `${day}.${month}.${year} ${hours}:${minutes}`;
    }
}

function disableButtons(buttonsClassName) {
    let buttonsArray = document.getElementsByClassName(buttonsClassName);
    for(let b in buttonsArray) {
        buttonsArray[b].disabled = true;
    }
}

async function cancelSubscription() {
    try {
        const response = await fetch("/cancel-subscription", {
            method: 'POST',
        });

        if (response.ok) {
            let jsonResponse = await response.json();
            if (jsonResponse.success == true) {
                location.reload();
            }
            else {
                console.log('todo toast');
            }

        } else {
            console.log("Error: " + response.statusText);
        }
    } catch (error) {
        console.error("An error occurred:", error);
    }
}