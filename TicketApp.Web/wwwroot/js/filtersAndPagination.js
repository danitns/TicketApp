function filtersAndPagination(nextPageButtonId, previousPageButtonId, currentPageInputId, filterButtonId, eventTypeInputId, eventGenreInputId, startDateInputId, endDateInputId) {
    var nextPageButton = document.getElementById(nextPageButtonId);
    var previousPageButton = document.getElementById(previousPageButtonId);
    var filterButton = document.getElementById(filterButtonId);
    var eventTypeInput = document.getElementById(eventTypeInputId);
    var eventGenreInput = document.getElementById(eventGenreInputId);
    var startDateInput = document.getElementById(startDateInputId);
    var endDateInput = document.getElementById(endDateInputId);
    var currentPage = document.getElementById(currentPageInputId);
    var pageControl = document.getElementById('pageControl');

    setCurrentFiltersAndPage();

    filterButton.onclick = () => {
        let queryString = `./Index?FilterEventModel.EventTypeId=${eventTypeInput.value}&FilterEventModel.EventGenreId=${eventGenreInput.value}&FilterEventModel.StartDate=${startDateInput.value}&FilterEventModel.EndDate=${endDateInput.value}&FilterEventModel.CurrentPage=${pageControl.value}`;
        filterButton.href = queryString;
        filterButton.click;
    }

    nextPageButton.onclick = () => {
        pageControl.value = 1;
        filterButton.click();
    }

    previousPageButton.onclick = () => {
        pageControl.value = -1;
        filterButton.click();
    }

    async function setCurrentFiltersAndPage() {
        var response = await fetch("/get-filters", {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json'
            }
        });

        if (response.ok) {
            const jsonResponse = await response.json();
            if (jsonResponse.error) {
                //toast
            }
            else {
                dynamicSelect(eventTypeInputId, eventGenreInputId, jsonResponse.eventTypeId, jsonResponse.eventGenreId);

                if (jsonResponse.startDate != null) {
                    let jsStartDate = new Date(jsonResponse.startDate);

                    let year = jsStartDate.getFullYear();
                    let month = String(jsStartDate.getMonth() + 1).padStart(2, '0');
                    let day = String(jsStartDate.getDate()).padStart(2, '0');
                    let formattedStartDate = year + "-" + month + "-" + day;

                    startDateInput.value = formattedStartDate;
                }

                if (jsonResponse.endDate != null) {
                    let jsEndDate = new Date(jsonResponse.startDate);

                    let year = jsEndDate.getFullYear();
                    let month = String(jsEndDate.getMonth() + 1).padStart(2, '0');
                    let day = String(jsEndDate.getDate()).padStart(2, '0');
                    let formattedEndDate = year + "-" + month + "-" + day;

                    endDateInput.value = formattedEndDate;
                }
                

                currentPage.innerHTML = jsonResponse.currentPage;



                if (currentPage.innerText == "1") {
                    previousPageButton.disabled = true;
                }
                else {
                    previousPageButton.disabled = false;
                }
            }
        }
    }
}

function formatDate(dateString) {
    const options = { year: 'numeric', month: '2-digit', day: '2-digit' };
    const formattedDate = new Date(dateString).toLocaleDateString('en-GB', options);
    return formattedDate;
}