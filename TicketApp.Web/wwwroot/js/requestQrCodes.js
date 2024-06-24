function requestQrCodesAndFillDiv(buttonId, ticketName, eventId, containerId, eventName, location, startDate, endDate, description, price, eventPicture, printButtonId) {
    const printButton = document.getElementById(printButtonId);
    const button = document.getElementById(buttonId);
    button.onclick = () => {
        let container = document.getElementById(containerId);

        if (container.childNodes.length === 0) {
            fetchData(ticketName, eventId, containerId, eventName, location, startDate, endDate, description, price, eventPicture, printButton);
        }
        else {
            printButton.disabled = false;
            let loadingCircle = printButton.getElementsByClassName("spinner-border")[0];
            loadingCircle.style.display = "none";
        }
    }

    async function fetchData(ticketName, eventId, containerId, eventName, location, startDate, endDate, description, price, eventPicture, printButton) {
        let response = await fetch(`/get-qr-codes?ticketName=${ticketName}&eventId=${eventId}`, {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json'
            }
        });

        if (response.ok) {
            const jsonResponse = await response.json();
            if (jsonResponse) {
                fillDiv(jsonResponse, containerId, ticketName, eventName, location, startDate, endDate, description, price, eventPicture);
                printButton.classList.remove('disabled');
                let loadingCircle = printButton.getElementsByClassName("spinner-border")[0];
                loadingCircle.style.display = "none";
            }
        }
    }
}



function fillDiv(qrCodes, containerId, ticketName, eventName, locationName, startDate, endDate, description, ticketPrice, eventPicture) {

    var container = document.getElementById(containerId);

    for (let i in qrCodes) {
        let elementRow = document.createElement('div');
        elementRow.className = "row m-1 border rounded";

        let imageCol = document.createElement('div');
        imageCol.className = "col-3 p-1";
        let eventImgString = `<img src="${eventPicture}" class="ticketEventPicture"></img>`;
        imageCol.innerHTML = eventImgString;
        elementRow.appendChild(imageCol);

        let detailsCol = document.createElement('div');
        detailsCol.className = "col-6";

        let hRow = document.createElement('div');
        hRow.className = "row text-center";
        let h4 = document.createElement('h4');
        h4.textContent = "EVENT TICKET";
        let h3 = document.createElement('h3');
        h3.textContent = ticketName;
        hRow.appendChild(h4);
        hRow.appendChild(h3);
        detailsCol.appendChild(hRow);

        let centerDetailsRow = document.createElement('div');
        centerDetailsRow.className = 'row';
        let detailsLeftColumn = document.createElement('div');
        detailsLeftColumn.className = 'col-6';
        let pEventName = document.createElement('p');
        pEventName.innerText = `Event name: ${eventName}`;

        let pLocationName = document.createElement('p');
        pLocationName.innerText = `Location name: ${locationName}`;

        let pStartDate = document.createElement('p');
        pStartDate.innerText = `Start date: ${startDate}`;

        let pEndDate = document.createElement('p');
        pEndDate.innerText = `End date: ${endDate}`;

        detailsLeftColumn.appendChild(pEventName);
        detailsLeftColumn.appendChild(pLocationName);
        detailsLeftColumn.appendChild(pStartDate);
        detailsLeftColumn.appendChild(pEndDate);
        centerDetailsRow.appendChild(detailsLeftColumn);

        let detailsRightColumn = document.createElement('div');
        detailsRightColumn.className = 'col-6';

        let pDescription = document.createElement('p');
        pDescription.innerText = `Description: ${description}`;

        let pPrice = document.createElement('p');
        pPrice.innerText = `Price: ${ticketPrice} RON`;

        detailsRightColumn.appendChild(pDescription);
        detailsRightColumn.appendChild(pPrice);
        centerDetailsRow.appendChild(detailsRightColumn);

        detailsCol.appendChild(centerDetailsRow);

        elementRow.appendChild(detailsCol);

        let qrCodeDiv = document.createElement('div');
        qrCodeDiv.className = "col-3 my-auto";
        let qrCodeImg = document.createElement('img');
        qrCodeImg.src = qrCodes[i];
        qrCodeImg.style.maxWidth = '100%';
        qrCodeImg.style.maxHeight = '100%';
        qrCodeDiv.appendChild(qrCodeImg);

        elementRow.appendChild(qrCodeDiv);
        container.appendChild(elementRow);
    }
}

//<div class="row">
//    <div class="col-3">
//        <img class="ticketEventPicture" src="data:image/png;base64,@Convert.ToBase64String(item.EventPicture)" />
//    </div>
//    <div class="col-6">
//        <div class="row text-center">
//            <h4>EVENT TICKET</h4>
//            <h3>@item.Name</h3>
//        </div>
//        <div class="row">
//            <div class="col-6">
//                <p>Event name: @item.EventName</p>
//                <p>Location: @item.Location</p>
//                <p>Start date: @item.StartDate.ToString("dd-MM-yyyy hh:mm")</p>
//                <p>End Date: @item.EndDate.ToString("dd-MM-yyyy hh:mm")</p>
//            </div>
//            <div class="col-6">
//                <p>Description:@item.Description</p>
//                <p>Price: @item.Price.ToString("0.") RON</p>
//            </div>
//            <button class="print btn btn-outline-primary">Save ticket</button>
//        </div>
//    </div>

//    <div class="col-3 my-auto">
//        <img src="@ticketTransaction" style="max-width: 100%; max-height: 100%" />
//    </div>

//</div>