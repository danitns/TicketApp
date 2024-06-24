function dynamicSelect(firstSelectInputId, secondSelectInputId, firstSelectValue, secondSelectValue) {
    var firstSelectInput = document.getElementById(firstSelectInputId);
    var secondSelectInput = document.getElementById(secondSelectInputId);

    if(firstSelectValue != null)
        firstSelectInput.value = firstSelectValue;

    function updateOptions()  {
        var selectedValue = firstSelectInput.value;
        secondSelectInput.innerHTML = '';
        standardOption = document.createElement('option');
        standardOption.text = 'Select an option';
        standardOption.value = '0';
        standardOption.selected = true;
        secondSelectInput.appendChild(standardOption);

        if (selectedValue) {
            fetch(`/corresponding-event-genres?eventTypeId=${selectedValue}`, {
                method: "get",
            })
                .then(response => response.json())
                .then(data => {
                    for (let i = 0; i < data.length; i++) {
                        var optionFromData = document.createElement('option');
                        optionFromData.value = data[i].value;
                        optionFromData.text = data[i].text;
                        secondSelectInput.appendChild(optionFromData);
                    }
                    if(secondSelectValue != undefined)
                        secondSelectInput.value = secondSelectValue;
                })
                .catch(error => {
                    alert('An error occurred while fetching data.');
                    console.error(error);
                });
        }
    }

    updateOptions();

    firstSelectInput.onchange = () => {
        updateOptions();

    }
}
