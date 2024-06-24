async function addTicketsToCart(ticketList, noTicketsColumnName, eventName) {
    let noTicketsColumn = document.getElementsByClassName(noTicketsColumnName);
    
    let form = new FormData();
    for (let index = 0; index < ticketList.length; index++) {
        let numberOfTickets = parseInt(noTicketsColumn[index].value, 10);
        let ticket = ticketList[index];
        var formIndex = 0;
        
        
        if (numberOfTickets > 0) {
            form.append(`tickets[${formIndex}].Name`, ticket.Name);
            form.append(`tickets[${formIndex}].EventName`, eventName);
            form.append(`tickets[${formIndex}].Description`, ticket.Description);
            form.append(`tickets[${formIndex}].Quantity`, numberOfTickets);
            form.append(`tickets[${formIndex}].Price`, ticket.Price);
            formIndex++;
        }
    }

    var response = await fetch("/Transaction/AddTicketsInCart", {
        method: 'POST',
        body: form,
    });

    if (response.ok) {
        const jsonResponse = await response.json();
        if (jsonResponse.errors) {
            showErrorsInTable(jsonResponse.errors);
        }
        else {
            window.location.replace("https://localhost:7034/Transaction/ShoppingCart");
        }
    }

    function showErrorsInTable(errors) {
        for (let e in errors) {
            var tableColumn = document.getElementsByName(`error_${e}`)[0];

            var text = document.createElement('p');
            text.innerText = `Max number of tickets you can buy is ${errors[e]}`

            tableColumn.appendChild(text);
        }
    }
}

