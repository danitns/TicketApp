async function getItemsForShoppingCartAndCreateTableRows(ticketTransactionList) {

    var updatedList = ticketTransactionList;

    const checkoutButton = document.getElementById('checkout-button');
    checkoutButton.onclick = () => {
        checkoutAndShowErrors()
    }

    const updateButton = document.getElementById('update-button');

    let tableRows = document.getElementsByClassName('ticketTableRow');
    for(let row of tableRows){
        const name = row.getElementsByClassName('ticketName')[0].innerText;
        let quantityInput = row.getElementsByClassName('quantityNumberInput')[0];
        quantityInput.oninput = function () {
            updateList(name, quantityInput.value)
        }


    }

    function updateList(name, newValue) {
        const ticket = updatedList.find(o => o.Name == name);
        ticket.Quantity = newValue;

        updateButton.style.display = 'block';
        checkoutButton.style.display = 'none';

        updateButton.onclick = () => {
            let form = new FormData();

            updatedList.forEach((ticket, index) => {
                form.append(`productsIdsInput[${index}].Name`, ticket.Name);
                form.append(`productsIdsInput[${index}].EventName`, ticket.EventName);
                form.append(`productsIdsInput[${index}].Description`, ticket.Description);
                form.append(`productsIdsInput[${index}].Quantity`, ticket.Quantity);
                form.append(`productsIdsInput[${index}].Price`, ticket.Price);
            });
            sendToControllerAndShowErrors(form, newValue);
        }
    }

    async function sendToControllerAndShowErrors(data, newValue) {
        try {
            const response = await fetch("/update-cart", {
                method: 'POST',
                body: data,
            });

            if (response.ok) {
                const jsonResponse = await response.json();

                if (jsonResponse.errors) {
                    showErrorsInSpans(jsonResponse.errors);
                } else {
                    if (newValue == 0)
                        location.reload();
                    updateButton.style.display = 'none';
                    checkoutButton.style.display = 'block';
                }

            } else {
                console.log("Error: " + response.statusText);
            }
        } catch (error) {
            console.error("An error occurred:", error);
        }
    }

    function showErrorsInSpans(errors) {
        let errContainer = document.getElementById('shoppingCartErrors');
        for (let e in errors) {
            var errorDiv = document.createElement('div');

            var text = document.createElement('p');
            if (errors[e] != 0)
                text.innerText = `Max number of tickets you can buy for ticket ${e} is ${errors[e]}`;
            else 
                text.innerText = `The ticket ${e} is out of stock. Please remove it from your cart.`;

            errorDiv.appendChild(text);
            errContainer.appendChild(errorDiv);
        }
    }

    async function checkoutAndShowErrors() {
        try {
            const response = await fetch("/create-checkout-session", {
                method: 'POST',
            });

            if (response.ok) {
                const jsonResponse = await response.json();

                if (jsonResponse.errors) {
                    showErrorsInSpans(jsonResponse.errors);
                } else {
                    location.replace(jsonResponse.url);
                }

            } else {
                console.log("Error: " + response.statusText);
            }
        } catch (error) {
            console.error("An error occurred:", error);
        }
    }
}