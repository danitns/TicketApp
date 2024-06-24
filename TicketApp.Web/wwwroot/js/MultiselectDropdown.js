function MultiselectDropdown(selectId, containerId, formValuesContainerId, formFieldName, options) {
    let select = document.createElement("select");

    let formValuesContainer = document.getElementById(formValuesContainerId);

    let placeholderOption = document.createElement("option");
    placeholderOption.disabled = true;
    placeholderOption.selected = true;
    placeholderOption.value = "";
    placeholderOption.text = "Select..."
    select.appendChild(placeholderOption);
    select.className = "form-select";
    select.setAttribute("id", selectId);

    options.forEach(o => {
        let option = document.createElement("option");
        option.value = o.value;
        option.text = o.text;
        select.appendChild(option);
    });

    this.addOption = (newOption) => {
        options.push(newOption);
        updateSelectedOptionsDiv();
        updateAvailableOptions();
    }

    select.onchange = (e) => {
        options.forEach(o => o.selected = o.value == e.currentTarget.value ? true : o.selected);
        updateSelectedOptionsDiv();
        updateAvailableOptions();
    }

    let updateSelectedOptionsDiv = () => {
        selectedOptionsContainer.innerHTML = null;
        select.value = "";
        options.filter(o => o.selected).forEach(o => {
            let selectedOptionSpan = document.createElement("span");
            selectedOptionSpan.className = "rounded"
            selectedOptionSpan.textContent = o.text;
            selectedOptionSpan.value = o.value;
            selectedOptionSpan.onclick = e => {
                options.forEach(o => o.selected = o.value == e.currentTarget.value ? false : o.selected);
                updateSelectedOptionsDiv();
                updateAvailableOptions();
            }

            selectedOptionsContainer.appendChild(selectedOptionSpan);
        });
    }


    let updateAvailableOptions = () => {
        var dropdownOptions = select.getElementsByTagName('option');
        formValuesContainer.innerHTML = null;
        for(let i = 0; i < dropdownOptions.length; i++) {
            var correspondingOption = options.filter(co => co.value == dropdownOptions[i].value)[0];
            if(correspondingOption != undefined && correspondingOption.selected) {
                dropdownOptions[i].disabled = true;
                dropdownOptions[i].style.display = 'none';

                let valueInput = document.createElement("input");
                valueInput.type = "hidden";
                valueInput.name = formFieldName;
                valueInput.value = correspondingOption.value;
                formValuesContainer.appendChild(valueInput);
            }
            else{
                dropdownOptions[i].disabled = false;
                dropdownOptions[i].style.display = 'block';
            }
        }
    }

    let wrapper = document.createElement("div");
    wrapper.className = "dropdown-multiple";
    wrapper.appendChild(select);


    let selectedOptionsContainer = document.createElement("div");
    selectedOptionsContainer.className = "dropdown-multiple-selected-options rounded";
    wrapper.appendChild(selectedOptionsContainer);

    let container = document.getElementById(containerId);
    container.appendChild(wrapper);

    
    updateSelectedOptionsDiv();
    updateAvailableOptions();
}