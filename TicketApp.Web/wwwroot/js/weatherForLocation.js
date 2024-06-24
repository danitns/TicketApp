async function fetchDataAndCreateWidget(locationId, startDate, endDate) {
    try {
        const response = await fetch(`/location-coordinates?id=${locationId}`, {
            method: 'GET',
        });

        if (response.ok) {
            const jsonResponse = await response.json();

            if (jsonResponse.lng) {
                let coordinates = jsonResponse;
                fetchWeatherDataAndCreateWidgets(coordinates, startDate, endDate);
            }

        } else {
            console.log("Error: " + response.statusText);
        }
    } catch (error) {
        console.error("An error occurred:", error);
    }
}

async function fetchWeatherDataAndCreateWidgets(coordinates, startDate, endDate) {
    const dateRange = getDatesBetween(startDate, endDate);

    for (const date of dateRange) {
        await createWidget(coordinates, date);
    }
}

function getDatesBetween(startDate, endDate) {
    const dates = [];
    const currentDate = new Date(startDate);

    while (currentDate <= new Date(endDate)) {
        dates.push(currentDate.toISOString().split('T')[0]);
        currentDate.setDate(currentDate.getDate() + 1);
    }

    return dates;
}

async function createWidget(coordinates, date){
    try {
        const response = await fetch(`https://api.weatherapi.com/v1/future.json?key=3c6d00b18ef84766b2e71432230409&q=${coordinates.lat},${coordinates.lng}&dt=${date}`, {
            method: 'GET',
        });

        if (response.ok) {
            const jsonResponse = await response.json();

            const weatherData = jsonResponse

            const forecast = weatherData.forecast.forecastday[0];

            const dayContainer = document.createElement('div');
            dayContainer.className = "widget-container border rounded text-center p-2 m-1";

            const dateElement = document.createElement('div');
            dateElement.className = "widget-date";
            dateElement.textContent = formatDate(forecast.date);

            const temperatureElement = document.createElement('div');
            temperatureElement.className = "widget-temperature";
            temperatureElement.textContent = `Temperature: ${forecast.day.avgtemp_c} C`;


            const precipitationElement = document.createElement('div');
            precipitationElement.className = "widget-precipitation";
            precipitationElement.textContent = `Precipitation: ${forecast.day.totalprecip_mm} mm`;


            const iconElement = document.createElement('div');
            iconElement.className = ("widget-icon");
            iconElement.innerHTML = `<img src="${forecast.day.condition.icon}" alt="Weather Icon">`;

            dayContainer.appendChild(iconElement);
            dayContainer.appendChild(dateElement);
            dayContainer.appendChild(temperatureElement);
            dayContainer.appendChild(precipitationElement);
            document.getElementById('widgets-container').appendChild(dayContainer);

            
        } else {
            console.log("Error: " + response.statusText);
        }
    } catch (error) {
        console.error("An error occurred:", error);
    }
}
function formatDate(dateString) {
    const options = { year: 'numeric', month: '2-digit', day: '2-digit' };
    const formattedDate = new Date(dateString).toLocaleDateString('en-GB', options);
    return formattedDate;
}

