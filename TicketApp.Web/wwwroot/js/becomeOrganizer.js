var organizerButton = document.getElementById('organizer-button');
organizerButton.onclick = () => {
    updateRole();
};

async function updateRole() {
    var response = await fetch("/UserAccount/BecomeOrganizer", {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        }
    });

    if (response.ok) {
        const jsonResponse = await response.json();
        if (jsonResponse.success) {
            CreateToast("Request sent", "Your request has been sent. You will be contacted soon by an admin.", "toastNotification", "organizerRequestToast");
            $('.toast').toast('show');
            var becomeOrganizerDiv = document.getElementById('become-organizer-div');
            becomeOrganizerDiv.style.display = 'none';
        }
    }
}