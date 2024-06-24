function createTicket(nameInputId, priceInputId, noTicketsInputId, descriptionInputId, eventIdInputId, createButtonId) {
    var nameInput = document.getElementById(nameInputId);
    var priceInput = document.getElementById(priceInputId);
    var noTicketsInput = document.getElementById(noTicketsInputId);
    var descriptionInput = document.getElementById(descriptionInputId);
    var eventIdInput = document.getElementById(eventIdInputId);
    var createButton = document.getElementById(createButtonId);

    createButton.onclick = async () => {
        var newTicket = createObject();

        try {
            const response = await fetch("/Ticket/Create", {
                method: "POST",
                body: newTicket
            });

            if (response.ok) {
                const jsonResponse = await response.json();
                if (jsonResponse.errors) {
                    showErrorsInSpans(jsonResponse.errors);
                }
                else {
                    location.reload();
                }

            } else {
                console.log("Error: " + response.statusText);
            }
        } catch (error) {
            console.error("An error occurred:", error);
        }
    }

    function createObject() {
        var objectForm = new FormData();
        objectForm.append('Name', nameInput.value);
        objectForm.append('Price', priceInput.value);
        objectForm.append('Notickets', noTicketsInput.value);
        objectForm.append('Description', descriptionInput.value);
        objectForm.append('EventId', eventIdInput.value);
        return objectForm;
    }

    function showErrorsInSpans(errors) {
        emptySpans();
        for (e in errors) {
            var errorSpan = document.querySelector('span[name="' + errors[e][0] + '"]');
            errorSpan.innerHTML = errors[e][1];
        };

    }

    function emptySpans() {
        var spanElements = document.querySelectorAll('span[id^="ticket"]');
        for (let span of spanElements) {
            span.innerHTML = "";
        }
    }
}