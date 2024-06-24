function CreateArtistMultiselectAndAddForm(selectId, containerId, formValuesContainerId, formFieldName, options) {

    var multiSelect = new MultiselectDropdown(selectId, containerId, formValuesContainerId, formFieldName, options);

    var closeModalButton = document.getElementById('closeArtistModal');
    var createArtistFormButton = document.getElementById('createArtistButton');
    var artistTypes = document.getElementById('artistTypeSelect');
    var initialArtistSelect = document.getElementById(selectId);

    createArtistFormButton.onclick = () => {
        var artistTypeId = artistTypes.value;
        var picture = document.getElementById('artistPicture').files[0];
        var name = document.getElementById('artistName').value;
        var birthdate = document.getElementById('artistBirthdate').value;
        var debut = document.getElementById('artistDebut').value;
        var form = new FormData();
        form.append('ArtistTypeId', artistTypeId);
        form.append('Picture', picture);
        form.append('Name', name);
        form.append('Birthdate', birthdate);
        form.append('Debut', debut);
        form.append('ArtistTypes', null);
        sendToControllerAndAddOptionInSelect(form, name);
    }

    async function sendToControllerAndAddOptionInSelect(data, name) {
        try {
            const response = await fetch("/Artist/Create", {
                method: 'POST',
                body: data,
            });

            if (response.ok) {
                const jsonResponse = await response.json();

                if (jsonResponse.errors) {
                    showErrorsInSpans(jsonResponse.errors);
                } else {
                    var newOption = document.createElement('option');
                    newOption.text = name;
                    newOption.value = jsonResponse.id;
                    initialArtistSelect.appendChild(newOption);
                    var optionForMultiselect = {
                        text: newOption.text,
                        value: newOption.value,
                        selected: true
                    };
                    multiSelect.addOption(optionForMultiselect);
                    closeModalButton.click();
                }

            } else {
                console.log("Error: " + response.statusText);
            }
        } catch (error) {
            console.error("An error occurred:", error);
        }
    }

    function showErrorsInSpans(errors) {
        emptySpans();
        for (e in errors) {
            var errorSpan = document.querySelector('span[name="artist' + errors[e][0] + '"]');
            errorSpan.innerHTML = errors[e][1];
        };

    }

    function emptySpans() {
        var spanElements = document.querySelectorAll('span[id^="artist"]');
        for (let span of spanElements) {
            span.innerHTML = "";
        }
    }
}


