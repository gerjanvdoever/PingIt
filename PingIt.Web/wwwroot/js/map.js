// wwwroot/js/map.js

// Global state
window._pendingIncidents = [];
window._mapReady = false;
window._apiKey = null;

// Called by Blazor as soon as the C# has the API key
window.loadGoogleMaps = function (apiKey, incidents) {
    console.log("loadGoogleMaps(): starting with apiKey:", apiKey, "incidents:", incidents);

    // Store the API key and incidents globally
    window._apiKey = apiKey;
    window._pendingIncidents = Array.isArray(incidents) ? incidents : [];

    // If Google Maps is already loaded, initialize immediately
    if (window.google && window.google.maps && window.google.maps.Map) {
        console.log("loadGoogleMaps(): Google Maps already loaded, initializing...");
        window.initMap();
        return;
    }

    // Check if script is already being loaded
    if (document.getElementById("gmaps-script")) {
        console.log("loadGoogleMaps(): Script already exists, waiting for load...");
        return;
    }

    // Create and load the Google Maps script
    console.log("loadGoogleMaps(): Creating Google Maps script tag...");
    const script = document.createElement("script");
    script.id = "gmaps-script";
    script.src = `https://maps.googleapis.com/maps/api/js?key=${encodeURIComponent(apiKey)}&callback=initMap&libraries=geometry`;
    script.async = true;
    script.defer = true;

    script.onerror = function (error) {
        console.error("loadGoogleMaps(): Failed to load Google Maps API", error);
        // You might want to show an error message to the user here
    };

    script.onload = function () {
        console.log("loadGoogleMaps(): Google Maps script loaded successfully");
    };

    document.head.appendChild(script);
};

// This function is called by Google Maps API when it's ready
window.initMap = function () {
    console.log("initMap(): Starting map initialization...");

    // Double-check that Google Maps is actually available
    if (!window.google || !window.google.maps) {
        console.error("initMap(): Google Maps API not available!");
        setTimeout(() => window.initMap(), 100); // Retry after 100ms
        return;
    }

    const mapElement = document.getElementById("map");
    if (!mapElement) {
        console.error("initMap(): '#map' element not found in DOM");
        return;
    }

    try {
        console.log("initMap(): Creating map instance...");
        window.map = new google.maps.Map(mapElement, {
            center: { lat: 52.3676, lng: 4.9041 }, // Amsterdam
            zoom: 10,
            mapTypeId: google.maps.MapTypeId.ROADMAP,
        });

        window._mapReady = true;
        console.log("initMap(): Map created successfully");

        // Now render the incidents
        window.renderIncidentMarkers();

    } catch (err) {
        console.error("initMap(): Error creating map:", err);
        window._mapReady = false;
    }
};

// Function to render incident markers
window.renderIncidentMarkers = function () {
    console.log("renderIncidentMarkers(): Starting with incidents:", window._pendingIncidents);

    // Check if Google Maps and map are ready
    if (!window.google || !window.google.maps) {
        console.error("renderIncidentMarkers(): Google Maps API not loaded");
        return;
    }

    if (!window.map) {
        console.error("renderIncidentMarkers(): Map not initialized");
        return;
    }

    const incidents = window._pendingIncidents || [];

    // Clear existing markers
    if (window.markers && Array.isArray(window.markers)) {
        console.log("renderIncidentMarkers(): Clearing existing markers");
        window.markers.forEach((marker) => {
            if (marker && marker.setMap) {
                marker.setMap(null);
            }
        });
    }
    window.markers = [];

    // If no incidents, we're done
    if (!Array.isArray(incidents) || incidents.length === 0) {
        console.log("renderIncidentMarkers(): No incidents to display");
        return;
    }

    console.log(`renderIncidentMarkers(): Processing ${incidents.length} incidents`);

    // Create bounds to fit all markers
    const bounds = new google.maps.LatLngBounds();
    let validMarkerCount = 0;

    incidents.forEach((incident, idx) => {
        try {
            // Extract coordinates - handle different possible property names
            const lat = incident.latitude || incident.Latitude;
            const lng = incident.longitude || incident.Longitude;
            const title = incident.title || incident.Title || `Incident ${idx + 1}`;
            const description = incident.description || incident.Description || "No description";

            console.log(`renderIncidentMarkers(): Processing incident ${idx}:`, { lat, lng, title });

            const latNum = parseFloat(lat);
            const lngNum = parseFloat(lng);

            if (isNaN(latNum) || isNaN(lngNum)) {
                console.warn(`renderIncidentMarkers(): Invalid coordinates for incident ${idx}:`, { lat, lng, incident });
                return; // Skip this incident
            }

            // Create marker
            const marker = new google.maps.Marker({
                position: { lat: latNum, lng: lngNum },
                map: window.map,
                title: title,
            });

            window.markers.push(marker);
            validMarkerCount++;

            // Create info window if there's content
            if (title || description) {
                const infoWindow = new google.maps.InfoWindow({
                    content: `
                        <div style="max-width: 200px;">
                            <h4 style="margin: 0 0 10px 0;">${title}</h4>
                            <p style="margin: 0;">${description}</p>
                        </div>
                    `,
                });

                marker.addListener("click", () => {
                    // Close other info windows
                    if (window.currentInfoWindow) {
                        window.currentInfoWindow.close();
                    }
                    infoWindow.open(window.map, marker);
                    window.currentInfoWindow = infoWindow;
                });
            }

            // Extend bounds
            bounds.extend(marker.getPosition());

        } catch (markerErr) {
            console.error(`renderIncidentMarkers(): Error creating marker for incident ${idx}:`, markerErr, incident);
        }
    });

    // Fit map to show all markers
    if (validMarkerCount > 0) {
        try {
            window.map.fitBounds(bounds);

            // Prevent over-zooming for single marker
            if (validMarkerCount === 1) {
                const listener = google.maps.event.addListenerOnce(window.map, 'bounds_changed', function () {
                    if (window.map.getZoom() > 15) {
                        window.map.setZoom(15);
                    }
                });
            }
        } catch (boundsErr) {
            console.error("renderIncidentMarkers(): Error fitting bounds:", boundsErr);
        }
    }

    console.log(`renderIncidentMarkers(): Successfully created ${validMarkerCount} markers`);
};

// Utility function to update incidents from Blazor
window.updateMapIncidents = function (incidents) {
    console.log("updateMapIncidents(): Updating with new incidents:", incidents);
    window._pendingIncidents = Array.isArray(incidents) ? incidents : [];

    if (window._mapReady) {
        window.renderIncidentMarkers();
    } else {
        console.log("updateMapIncidents(): Map not ready yet, incidents will be rendered when map initializes");
    }
};

// Add error handling for the global scope
window.addEventListener('error', function (e) {
    if (e.message && e.message.includes('google')) {
        console.error("Global error related to Google Maps:", e);
    }
});