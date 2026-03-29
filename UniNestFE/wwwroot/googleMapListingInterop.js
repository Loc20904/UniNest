// This file handles GoongJS integration for CreateListing, EditListing, and ListingDetail
window.listingGoongMap = {
    map: null,
    markers: {},
    searchMarker: null,
    mapKey: '',
    apiKey: '',
    dotNetRef: null,

    setKeys: function (mapKey, apiKey) {
        this.mapKey = mapKey;
        this.apiKey = apiKey;
    },

    initMap: function (dotNetRef, elementId, lat, lng, zoom) {
        if (!window.goongjs) { console.error("GoongJS library not loaded!"); return; }
        if (!this.mapKey) { console.error("Map Key not set!"); return; }

        this.dotNetRef = dotNetRef;
        goongjs.accessToken = this.mapKey;

        // Cleanup existing map if any
        if (this.map && this.map.getContainer().id === elementId) {
            this.map.remove();
        }

        this.map = new goongjs.Map({
            container: elementId,
            style: 'https://tiles.goong.io/assets/goong_map_web.json',
            center: [lng !== 0 ? lng : 108.2022, lat !== 0 ? lat : 16.0544],
            zoom: zoom || 14
        });

        // Add standard navigation controls
        this.map.addControl(new goongjs.NavigationControl(), 'top-right');

        // Add Geolocate control
        const geolocate = new goongjs.GeolocateControl({
            positionOptions: { enableHighAccuracy: true },
            trackUserLocation: true,
            showUserHeading: true
        });
        this.map.addControl(geolocate);

        // Add initial marker if coordinates are valid
        if (lat !== 0 && lng !== 0) {
            this.updateMarker(lat, lng, "Initial Location");
        }

        // Selection listener
        if (dotNetRef) {
            this.map.on('click', (e) => {
                const clickLat = e.lngLat.lat;
                const clickLng = e.lngLat.lng;
                
                this.updateMarker(clickLat, clickLng, "New Selected Location");
                
                dotNetRef.invokeMethodAsync('OnMapClick', clickLat, clickLng);
            });
        }

        const mapInstance = this.map;
        setTimeout(() => mapInstance.resize(), 400);
    },

    initReadOnlyMap: function (elementId, lat, lng, zoom) {
        this.initMap(null, elementId, lat, lng, zoom || 15);
    },

    updateMarker: function (lat, lng, label) {
        if (!this.map) return;
        
        // Always remove old marker first to avoid re-attachment issues
        if (this.searchMarker) {
            this.searchMarker.remove();
            this.searchMarker = null;
        }

        // Create new marker
        this.searchMarker = new goongjs.Marker({ color: '#f43f5e', draggable: false })
            .setLngLat([lng, lat])
            .addTo(this.map);
        
        if (label) {
            const popup = new goongjs.Popup({ offset: 25 }).setHTML(`<div style="padding: 5px; font-weight: bold; color: #1e293b;">${label}</div>`);
            this.searchMarker.setPopup(popup);
            this.searchMarker.togglePopup();
        }
        
        this.map.flyTo({ center: [lng, lat], zoom: 16, speed: 1.2 });
    },

    addPoiMarkers: function (poiList) {
        if (!this.map || !poiList) return;
        
        poiList.forEach(poi => {
            const el = document.createElement('div');
            el.className = 'university-marker';
            el.style.backgroundColor = poi.type === 'university' ? '#06b5ef' : '#f59e0b';
            el.style.width = '30px';
            el.style.height = '30px';
            el.style.borderRadius = '50%';
            el.style.border = '2px solid white';
            el.style.boxShadow = '0 2px 8px rgba(0,0,0,0.3)';
            el.style.display = 'flex';
            el.style.alignItems = 'center';
            el.style.justifyContent = 'center';
            el.innerHTML = `<i class="material-icons" style="font-size: 16px; color: white;">${poi.type === 'university' ? 'school' : 'shopping_cart'}</i>`;

            new goongjs.Marker(el)
                .setLngLat([poi.lng, poi.lat])
                .setPopup(new goongjs.Popup({ offset: 25 }).setHTML(`<strong style="color: ${el.style.backgroundColor}">${poi.name}</strong>`))
                .addTo(this.map);
        });
    }
};
