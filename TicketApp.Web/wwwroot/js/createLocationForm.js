var createLocationForm = document.getElementById('locationForm')
var createLocationFormButton = document.getElementById('createLocationButton');
var initialLocationSelect = document.getElementById('initialLocationSelect');
var closeModal = document.getElementById('closeLocationModal');

var marker = null;
var locationAddress = null;

function initMap() { //todo searchbar
    const map = new google.maps.Map(document.getElementById("mapForLocation"), {
        center: {
            lat: 44.4249267578125,
            lng: 26.087175369262695
        },
        disableDefaultUI: true,
        zoom: 12

    });

    const input = document.getElementById("pac-input");
    const searchBox = new google.maps.places.SearchBox(input);

    map.addListener("bounds_changed", () => {
        searchBox.setBounds(map.getBounds());
    });

    searchBox.addListener("places_changed", () => {
        const places = searchBox.getPlaces();

        if (places.length == 0) {
            return;
        }
        const bounds = new google.maps.LatLngBounds();

        places.forEach((place) => {
            if (!place.geometry || !place.geometry.location) {
                console.log("Returned place contains no geometry");
                return;
            }
            if (place.geometry.viewport) {
                bounds.union(place.geometry.viewport);
            } else {
                bounds.extend(place.geometry.location);
            }
        });
        map.fitBounds(bounds);

    });




    const geocoder = new google.maps.Geocoder();

    google.maps.event.addListener(map, 'click', function (event) {
        marker = placeOrReplaceMarker(event.latLng, marker);
        geocodeLatLng(geocoder, event.latLng);
    });

    function placeOrReplaceMarker(location, myMarker) {
        if(myMarker != null)
            myMarker.setMap(null);
        const mrk = new google.maps.Marker({
            position: location,
            map: map
        });
        return mrk;
    }  
}

createLocationFormButton.onclick = () => { 
    var name = document.getElementById('locationName').value;
    var locationTypeId = document.getElementById('locationLocationTypeId').value;
    if (marker != null) {
        var latitude = marker.getPosition().lat();
        var longitude = marker.getPosition().lng();
    }
    else {
        var latitude = 91;
        var longitude = 181;
    }
    

    var objectForm = new FormData();
    objectForm.append('Name', name);
    objectForm.append('LocationTypeId',locationTypeId)
    objectForm.append('Address', locationAddress);
    objectForm.append('Latitude', latitude);
    objectForm.append('Longitude', longitude);

    sendToControllerAndAddOptionInSelect(objectForm, name);
}

async function sendToControllerAndAddOptionInSelect(data, name) {
    try {
        const response = await fetch("/Location/Create", {
            method: "POST",
            body: data
        });

        if (response.ok) {
            const jsonResponse = await response.json();


            if (jsonResponse.errors) {
                showErrorsInSpans(jsonResponse.errors);
            } else {
                var newOption = document.createElement('option');
                newOption.text = name;
                newOption.value = jsonResponse.id;
                initialLocationSelect.appendChild(newOption);
                newOption.selected = true;
                closeModal.click();
            }

        } else {
            console.log("Error: " + response.statusText);
        }
    } catch (error) {
        console.error("An error occurred:", error);
    }
}


async function geocodeLatLng(geocoder, latlng) {

    geocoder
        .geocode({ location: latlng })
        .then((response) => {
            if (response.results[0]) {
                locationAddress = response.results[0].formatted_address;
            } else {
                window.alert("No results found");
            }
        })
        .catch((e) => window.alert("Geocoder failed due to: " + e));
}

function showErrorsInSpans(errors) {
    debugger;
    for (e in errors) {
        var errorSpan = document.querySelector('span[name="location' + errors[e][0] + '"]');
        errorSpan.innerHTML = errors[e][1];
    };

}