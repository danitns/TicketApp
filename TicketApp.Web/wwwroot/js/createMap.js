"use strict";

const icons = {
    TheatreComplex: {
        icon: '../img/icons/theatreIcon40.png',
    },
    Cinema: {
        icon: '../img/icons/movieIcon40.png',
    },
    ArtGallery: {
        icon: '../img/icons/artIcon40.png'
    },
    Museum: {
        icon: '../img/icons/museumIcon40.png'
    },
    Club: {
        icon: '../img/icons/partyIcon40.png'
    },
    ComedyClub: {
        icon: '../img/icons/standupIcon40.png'
    },
    ConcertHall: {
        icon: '../img/icons/concertIcon40.png'
    },
    Stadium: {
        icon: '../img/icons/stadium48.png'
    }

};

var markersArrayWithInfoWindows = [];
var seenCoordinates = {};

async function fetchDataAndAddMarkersToMap(map, eventTypeOption, omsInstance, eventTypeFilter, startDateFilter) {
    var bounds = map.getBounds();
    clearOutsideOverlays(bounds);

    var swPoint = bounds.getSouthWest();
    var nePoint = bounds.getNorthEast();

    var swLat = swPoint.lat();
    var swLng = swPoint.lng();
    var neLat = nePoint.lat();
    var neLng = nePoint.lng();


    var response = await
        fetch(`/map-locations?option=${eventTypeOption}&swLat=${swLat}&swLng=${swLng}&neLat=${neLat}&neLng=${neLng}&eventTypeFilter=${eventTypeFilter}&startDateFilter=${startDateFilter}`, {
            method: "get",
        })
            .then(response => response.json())
            .then(data => {
                var mapLocations = data;

                clearFilteredMarkers(mapLocations);

                var infoWindow = new google.maps.InfoWindow;

                for (let i = 0; i < mapLocations.length; i++) {
                    var locationTypeName = mapLocations[i].locationTypeName;
                    locationTypeName = locationTypeName.replace(/\s/g, '');
                    var iconPath = icons[locationTypeName].icon;

                    createMarkerAndAddItToMap(iconPath, mapLocations[i], infoWindow);

                }
            })
            .catch(error => {
                alert('An error occurred while fetching data.');
                console.error(error);
            });



    function createMarkerAndAddItToMap(pathToIcon, fetchedLocation, iw) {
        var contentString = createContentString(fetchedLocation);

        var marker = marker = new google.maps.Marker({
            position: { lat: fetchedLocation.latitude, lng: fetchedLocation.longitude },
            icon: pathToIcon,
        });

        var coordHash = calculateCoordinateHash(marker);



        if (seenCoordinates[coordHash] == null) {
            seenCoordinates[coordHash] = 1;

            marker.setMap(map);

            google.maps.event.addListener(marker, 'spider_click', function (e) {
                iw.setContent(contentString);
                iw.open(map, marker);
            });

            var markerWithIw = {
                Marker: marker,
                InfoWindow: iw
            };

            markersArrayWithInfoWindows.push(markerWithIw);

            omsInstance.addMarker(marker);
        }
    }

    function createContentString(location) {
        var contentString = `<div class="info-window-div w-100 text-center">`

        var locationHeader = `<div class="info-window-header"> <h1>${location.name}</h1> </div>`;

        var eventsDetails = `<div class="info-window-details p-2">`



        for (var l in location.events) {
            let originalDate = new Date(location.events[l].startDate);
            let formattedStartDate = formatDate(originalDate);
            originalDate = new Date(location.events[l].endDate);
            let formattedEndDate = formatDate(originalDate);
            eventsDetails += 
            `<div class="info-window-event-body p-1 row">
                <div class="border rounded col h-100">
                    <div class="row h-100">
                        <div class="col col-6 p-1 h-100">
                            <div class="h-100">
                                <img style="width: 100%; height: 100%; object-fit: contain" src="data:image/png;base64,${location.events[l].pictureContent}" />
                            </div>
                        </div> 
                        <div class="col col-6 h-100">
                            <div class="row">
                                <h6>${location.events[l].name}</h6>
                            </div>
                            <div class="row">
                                <div> Start Date: ${formattedStartDate}</div >
                            </div>
                            <div class="row">
                                <div> End Date: ${formattedEndDate}</div >
                            </div>
                            <div class="row">
                                <div> Event Type: ${location.events[l].eventTypeName}</div >
                            </div>
                            <div class="row">
                                <div><a href="../Event/Details/${location.events[l].id}" class="btn btn-primary">Event page</a> </div >
                            </div>
                        </div>
                    </div>
                </div>
            </div>`;
        }

        eventsDetails += `</div>`;
        contentString += locationHeader + eventsDetails + `</div>`;
        return contentString;
    }

    function clearOutsideOverlays(mapBounds) {
        for (var i = markersArrayWithInfoWindows.length - 1; i >= 0; i--) {
            if (!mapBounds.contains(markersArrayWithInfoWindows[i].Marker.getPosition())) {
                markersArrayWithInfoWindows[i].Marker.setMap(null);

                var coordHash = calculateCoordinateHash(markersArrayWithInfoWindows[i].Marker);
                if (seenCoordinates[coordHash]) {
                    seenCoordinates[coordHash] = null;
                }

                markersArrayWithInfoWindows.splice(i, 1);
            }
        }
    }

    function clearFilteredMarkers(mapData) {
        
        for (var i = markersArrayWithInfoWindows.length - 1; i >= 0; i--) {
            var shouldRemove = true;

            var lat = markersArrayWithInfoWindows[i].Marker.getPosition().lat();
            var lng = markersArrayWithInfoWindows[i].Marker.getPosition().lng();

            for (var j = 0; j < mapData.length; j++) {
                if (mapData[j].latitude == lat && mapData[j].longitude == lng)
                    shouldRemove = false;
            }

            if (shouldRemove) {
                markersArrayWithInfoWindows[i].Marker.setMap(null);

                var coordHash = calculateCoordinateHash(markersArrayWithInfoWindows[i].Marker);
                if (seenCoordinates[coordHash]) {
                    seenCoordinates[coordHash] = null;
                }

                markersArrayWithInfoWindows.splice(i, 1);
            }
        }
    }

    function calculateCoordinateHash(marker) {
        var coordinatesHash = [marker.getPosition().lat(),
        marker.getPosition().lng()].join('');
        return coordinatesHash.replace(".", "").replace(",", "").replace("-", "");
    }
}


function addFiltersToMap(filtersContainerId, eventTypeSelectId, startDatePickerId, filterButtonId) {
    var filtersContainer = document.getElementById(filtersContainerId);

    var startDatePicker = document.createElement('input');
    startDatePicker.className = 'form-control';
    startDatePicker.type = 'datetime-local';
    startDatePicker.id = startDatePickerId;

    const dt = new Date();
    dt.setMinutes(dt.getMinutes() - dt.getTimezoneOffset());

    startDatePicker.value = dt.toISOString().slice(0, 16);

    var eventTypeSelect = document.createElement('select');
    eventTypeSelect.id = eventTypeSelectId;
    eventTypeSelect.className = 'form-select';

    var option = document.createElement('option');
    option.value = 'all';
    option.text = 'All';
    option.selected = true;
    eventTypeSelect.appendChild(option);

    var filterButton = document.createElement('button');
    filterButton.type = 'button';
    filterButton.id = filterButtonId;
    filterButton.innerHTML = 'Filter';
    filterButton.className = 'btn btn-primary';

    filtersContainer.style.display = "none";

    filtersContainer.appendChild(eventTypeSelect);
    filtersContainer.appendChild(startDatePicker)
    filtersContainer.appendChild(filterButton); 
}


async function addOptionsInEventTypeSelect(eventTypeSelectId) {
    var eventTypeSelect = document.getElementById(eventTypeSelectId);

    var response = await
    fetch(`/map-eventTypes?option=${optionForEvents}`, {
        method: "get",
    })
        .then(response => response.json())
        .then(data => {

            for (var d in data) {
                var option = document.createElement('option');
                option.value = data[d].value;
                option.text = data[d].text;

                eventTypeSelect.appendChild(option);
            }
        })
        .catch(error => {
            alert('An error occurred while fetching data.');
            console.error(error);
        });
}

function panToCurrentLocation(map) {
    const locationButton = document.createElement("button");
    locationButton.classList.add("btn");
    locationButton.classList.add("btn-primary");

    locationButton.textContent = "Pan to Current Location";
    locationButton.classList.add("custom-map-control-button");
    map.controls[google.maps.ControlPosition.TOP_CENTER].push(locationButton);
    locationButton.addEventListener("click", () => {
        if (navigator.geolocation) {
            navigator.geolocation.getCurrentPosition(
                (position) => {
                    const pos = {
                        lat: position.coords.latitude,
                        lng: position.coords.longitude,
                    };
                    map.setCenter(pos);
                },
                () => {
                    handleLocationError(true, map.getCenter());
                },
            );
        } else {
            handleLocationError(false, map.getCenter());
        }
    });
    locationButton.click();
}

function handleLocationError(browserHasGeolocation, pos) {
    var infoWindowForGeoLocation = new google.maps.InfoWindow(); 
    infoWindowForGeoLocation.setPosition(pos);
    infoWindowForGeoLocation.setContent(
        browserHasGeolocation
            ? "Error: The Geolocation service failed."
            : "Error: Your browser doesn't support geolocation.",
    );
    infoWindowForGeoLocation.open(map);
}



function initMap() {


    const map = new google.maps.Map(document.getElementById("gmp-map"), {
        center: {
            lat: 44.4249267578125,
            lng: 26.087175369262695
        },
        disableDefaultUI: true,
        zoom: 12,
        mapId: mapThemeId


    });

    var oms = new OverlappingMarkerSpiderfier(map, {
        markersWontMove: true,
        markersWontHide: true,
        basicFormatEvents: true
    });

    var eventTypeSelectId = 'eventTypeSelect';
    var startDatePickerId = 'startDatePicker';
    var filterButtonId = 'filterButton';

    addFiltersToMap('filtersContainer', eventTypeSelectId, startDatePickerId, filterButtonId);
    addOptionsInEventTypeSelect(eventTypeSelectId);

    var eventTypeSelect = document.getElementById(eventTypeSelectId);
    var startDatePicker = document.getElementById(startDatePickerId);
    var filterButton = document.getElementById(filterButtonId);
    var filtersContainer = document.getElementById('filtersContainer');

    map.controls[google.maps.ControlPosition.RIGHT_TOP].push(filtersContainer); 

    panToCurrentLocation(map);

    filterButton.onclick = () => {
        fetchDataAndAddMarkersToMap(map, optionForEvents, oms, eventTypeSelect.value, startDatePicker.value);
    }

    google.maps.event.addListener(map, 'idle', function () {
        fetchDataAndAddMarkersToMap(map, optionForEvents, oms, eventTypeSelect.value, startDatePicker.value);
    });

    google.maps.event.addListenerOnce(map, 'tilesloaded', function () {
        filtersContainer.style.display = "block";
    });


}



function formatDate(date) {
    let day = date.getDate();
    let month = date.getMonth() + 1;
    let year = date.getFullYear();
    let hours = date.getHours();
    let minutes = date.getMinutes();

    // Ensure two-digit format
    day = day < 10 ? '0' + day : day;
    month = month < 10 ? '0' + month : month;
    hours = hours < 10 ? '0' + hours : hours;
    minutes = minutes < 10 ? '0' + minutes : minutes;

    return `${day}.${month}.${year} ${hours}:${minutes}`;
}