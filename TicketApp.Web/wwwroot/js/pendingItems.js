async function initButton(fetchUrl, itemId) {
    var response = await fetch(`${fetchUrl}?id=${itemId}`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(itemId),
    });

    if (response.ok) {
        const jsonResponse = await response.json();
        if (jsonResponse.success) {
            location.reload();
        }
        else {
            CreateToast("Operation failed", "Please try again later.", "toastNotification");
        }
    }
}