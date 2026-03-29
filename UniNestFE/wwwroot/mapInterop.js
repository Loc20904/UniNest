window.uniNestMap = {
    map: null,
    apiKey: null,
    mapKey: null,
    markers: {},
    geolocateControl: null,
    searchMarker: null,

    toggleTheme: function (isDark) {
        if (isDark) {
            document.documentElement.classList.add('dark');
            localStorage.setItem('theme', 'dark');
        } else {
            document.documentElement.classList.remove('dark');
            localStorage.setItem('theme', 'light');
        }
    },

    searchMarker: null,

    // ------------------------------------------------
    // CALCULATE DISTANCE (USER -> MARKER)
    // ------------------------------------------------
    calculateDistanceTo: function (destLat, destLng) {
        if (!this.apiKey) return;

        // Get current location
        if (navigator.geolocation) {
            navigator.geolocation.getCurrentPosition((position) => {
                const userLat = position.coords.latitude;
                const userLng = position.coords.longitude;

                const origin = `${userLat},${userLng}`;
                const destination = `${destLat},${destLng}`;

                // Use Motorbike by default
                const url = `https://rsapi.goong.io/DistanceMatrix?origins=${origin}&destinations=${destination}&vehicle=bike&api_key=${this.apiKey}`;

                fetch(url)
                    .then(res => res.json())
                    .then(data => {
                        const row = data.rows?.[0]?.elements?.[0];
                        if (row?.status === "OK") {
                            // Call back to Blazor
                            if (this.dotNetRef) {
                                this.dotNetRef.invokeMethodAsync('UpdateDistanceResult', row.distance.text);
                            }
                        } else {
                            if (this.dotNetRef) {
                                this.dotNetRef.invokeMethodAsync('UpdateDistanceResult', "N/A");
                            }
                        }
                    })
                    .catch(err => console.error(err));
            });
        }
    },

    // ------------------------------------------------
    // SET KEYS
    // ------------------------------------------------
    setKeys: function (mapKey, apiKey) {
        this.mapKey = mapKey;
        this.apiKey = apiKey;
        console.log("Goong Keys have been set.");
    },

    // ------------------------------------------------
    // INIT MAP
    // ------------------------------------------------
    initMap: function (dotNetRef, elementId, lat, lng, zoom) {
        if (!window.goongjs) { console.error("Thư viện goong-js chưa load!"); return; }
        if (!this.mapKey) { console.error("Chưa có Map Key!"); return; }

        this.dotNetRef = dotNetRef; // Store reference

        goongjs.accessToken = this.mapKey;

        if (this.map) this.map.remove();

        this.map = new goongjs.Map({
            container: elementId,
            style: 'https://tiles.goong.io/assets/goong_map_web.json',
            center: [lng, lat],
            zoom: zoom
        });

        // Navi Controls
        this.map.addControl(new goongjs.NavigationControl(), 'top-right');

        // ---------------------------
        // GEOLOCATE CONTROL
        // ---------------------------
        this.geolocateControl = new goongjs.GeolocateControl({
            positionOptions: { enableHighAccuracy: true },
            trackUserLocation: true,
            showUserHeading: true
        });

        this.map.addControl(this.geolocateControl);

        const mapInstance = this.map;
        setTimeout(() => mapInstance.resize(), 300);
        mapInstance.on('load', () => mapInstance.resize());
    },

    // ------------------------------------------------
    // TRIGGER FROM BLAZOR
    // ------------------------------------------------
    locateUser: function () {
        if (this.geolocateControl) {
            console.log("Triggering Geolocation...");
            this.geolocateControl.trigger();
        } else {
            console.warn("GeolocateControl chưa được khởi tạo.");
        }
    },

    clearMarkers: function () {
        for (const id in this.markers) {
            this.markers[id].remove();
        }
        this.markers = {};
    },

    // ------------------------------------------------
    // ADD MARKER (ROOM)
    // ------------------------------------------------
    addMarker: function (dotNetRef, data) {
        if (!this.map) return;
        if (this.markers[data.id]) return;

        const popupHTML = `
            <div class="w-[300px] bg-[#121416] border border-[#27353a] rounded-xl overflow-hidden shadow-2xl relative text-left">
                <button onclick="document.querySelector('.mapboxgl-popup').remove()" class="absolute top-2 right-2 w-7 h-7 rounded-full bg-black/60 text-white flex items-center justify-center hover:bg-black/80 transition-colors backdrop-blur-sm z-20">
                    <span class="material-icons-round text-sm" style="font-size: 16px;">close</span>
                </button>
                <div class="relative h-36 w-full bg-cover bg-center bg-gray-700" style="background-image: url('${data.image || "https://via.placeholder.com/300"}');">
                     <div class="absolute bottom-0 left-0 right-0 h-20 bg-gradient-to-t from-[#121416] via-[#121416]/80 to-transparent"></div>
                     ${data.isVerified ? `
                     <div class="absolute top-2 left-2 bg-emerald-500/90 backdrop-blur-md text-white text-[10px] font-bold px-2 py-1 rounded-lg flex items-center gap-1 shadow-lg z-10">
                         <span class="material-icons-round text-[12px]">verified</span>VERIFIED HOST
                     </div>` : ''}
                </div>
                <div class="p-4 -mt-2 relative z-10 text-white">
                    <h3 class="text-base font-bold truncate leading-tight">${data.title}</h3>
                    <div class="flex items-center gap-2 mt-2 text-xs text-secondary-text" style="color: #94a3b8;">
                        <span class="material-icons-round text-[14px] shrink-0">location_on</span>
                        <span class="truncate">${data.address}</span>
                    </div>
                    <div class="flex items-center gap-2 mt-1 text-xs text-emerald-400">
                        <span class="material-icons-round text-[14px] shrink-0">near_me</span>
                        <span class="font-bold" id="dist-${data.id}">Calculating distance...</span>
                    </div>
                    <div class="flex justify-between items-center mt-4 pt-3 border-t border-[#27353a]">
                        <div>
                            <p class="text-[10px] uppercase font-bold tracking-wider" style="color: #64748b;">Price/Month</p>
                            <p class="font-extrabold text-lg" style="color: #06b5ef;">${data.priceStr}</p>
                        </div>
                        <button class="text-xs hover:opacity-90 px-4 py-2 rounded-lg font-bold transition-all shadow-lg" style="background-color: #06b5ef; color: #121416;">
                            View Details
                        </button>
                    </div>
                </div>
            </div>
        `;

        const popup = new goongjs.Popup({ offset: 25, closeButton: false, closeOnClick: true, maxWidth: 'none' })
            .setHTML(popupHTML);

        const marker = new goongjs.Marker({ color: "#ea580c" })
            .setLngLat([data.lng, data.lat])
            .setPopup(popup)
            .addTo(this.map);

        const el = marker.getElement();
        el.style.cursor = "pointer";

        // Click → Tính khoảng cách
        el.addEventListener('click', () => {
            // Goong/Mapbox natively toggles the popup bound to the marker, 
            // so we DO NOT call marker.togglePopup() here, which would immediately close it again!

            // Tính toán thẳng trong JS rồi điền text vào div:
            this.calculateDistanceForMarker(data.id, data.lat, data.lng);

            // Gọi hàm C# nếu cần (như zoom)
            dotNetRef.invokeMethodAsync('OnMarkerClick', data.id);
        });

        this.markers[data.id] = marker;
    },

    calculateDistanceForMarker: function (id, destLat, destLng) {
        if (!this.apiKey) return;
        const distSpan = document.getElementById(`dist-${id}`);
        if (distSpan) distSpan.innerText = "Calculating distance...";

        if (navigator.geolocation) {
            navigator.geolocation.getCurrentPosition((position) => {
                const userLat = position.coords.latitude;
                const userLng = position.coords.longitude;
                const origin = `${userLat},${userLng}`;
                const destination = `${destLat},${destLng}`;
                const url = `https://rsapi.goong.io/DistanceMatrix?origins=${origin}&destinations=${destination}&vehicle=bike&api_key=${this.apiKey}`;

                fetch(url)
                    .then(res => res.json())
                    .then(resData => {
                        const row = resData.rows?.[0]?.elements?.[0];
                        const distEl = document.getElementById(`dist-${id}`);
                        if (distEl) {
                            distEl.innerText = row?.status === "OK" ? row.distance.text : "Distance N/A";
                        }
                    }).catch(e => {
                        const distEl = document.getElementById(`dist-${id}`);
                        if (distEl) distEl.innerText = "Error loading distance";
                    });
            });
        } else {
            const distEl = document.getElementById(`dist-${id}`);
            if (distEl) distEl.innerText = "Location disabled";
        }
    },

    // ------------------------------------------------
    // FORCE OPEN MARKER POPUP
    // ------------------------------------------------
    triggerMarkerPopup: function (id) {
        const marker = this.markers[id];
        if (!marker) {
            console.warn("Marker not found:", id);
            return;
        }

        const popup = marker.getPopup();
        if (!popup.isOpen()) popup.addTo(this.map);

        popup.setLngLat(marker.getLngLat());

        // Tính toán khoảng cách khi bấm từ sidebar
        this.calculateDistanceForMarker(id, marker.getLngLat().lat, marker.getLngLat().lng);
    },

    // ------------------------------------------------
    // GET ROOM DETAILS (GOONG DISTANCE MATRIX)
    // ------------------------------------------------
    getRoomDetails: async function (dotNetRef, lat, lng) {
        if (!this.apiKey) return;

        const origin = `${lat},${lng}`;
        const destination = `16.0738,108.1499`; // DUT

        const modes = [
            { type: 'bike', label: 'motorbike' },
            { type: 'car', label: 'bus' }
        ];

        try {
            const travelPromises = modes.map(m => {
                const url = `https://rsapi.goong.io/DistanceMatrix?origins=${origin}&destinations=${destination}&vehicle=${m.type}&api_key=${this.apiKey}`;
                return fetch(url)
                    .then(res => res.json())
                    .then(data => {
                        const row = data.rows?.[0]?.elements?.[0];
                        if (row?.status === "OK") {
                            return {
                                type: m.label,
                                duration: row.duration.text,
                                distanceVal: row.distance.value
                            };
                        }
                        return { type: m.label, duration: '--' };
                    })
                    .catch(() => ({ type: m.label, duration: '--' }));
            });

            let travelResults = await Promise.all(travelPromises);

            // Estimate walking time if bike distance is available
            const bikeData = travelResults.find(x => x.type === 'motorbike');

            if (bikeData?.distanceVal) {
                const walkMins = Math.ceil(bikeData.distanceVal / 83);
                travelResults.push({ type: 'walking', duration: walkMins + " mins" });
            } else {
                travelResults.push({ type: 'walking', duration: "--" });
            }

            await dotNetRef.invokeMethodAsync('UpdateRoomDetails', {
                travels: travelResults,
                places: []
            });

        } catch (e) {
            console.error("Goong API Error:", e);
        }
    },

    // ------------------------------------------------
    // [MỚI] ADD MARKER TRƯỜNG ĐẠI HỌC
    // ------------------------------------------------
    addUniversityMarker: function (lat, lng, name) {
        if (!this.map) return;

        const el = document.createElement('div');
        el.className = 'university-marker';

        // --- BẮT ĐẦU PHẦN THAY ĐỔI ---
        // Thay span bằng svg icon mũ cử nhân
        el.innerHTML = `
            <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="white" style="width: 20px; height: 20px;">
                <path d="M12 3L1 9l11 6 9-4.91V17h2V9L12 3zM5 13.18v4L12 21l7-3.82v-4L12 17l-7-3.82z"/>
            </svg>
        `;
        // --- KẾT THÚC PHẦN THAY ĐỔI ---

        // style trực tiếp
        Object.assign(el.style, {
            backgroundColor: '#06b5ef',
            width: '36px',
            height: '36px',
            borderRadius: '50%',
            display: 'flex',
            justifyContent: 'center',
            alignItems: 'center',
            cursor: 'pointer',
            boxShadow: '0 4px 10px rgba(0,0,0,0.3)',
            border: '2px solid white',
            zIndex: '10'
        });

        // popup
        const popup = new goongjs.Popup({ offset: 20, closeButton: false })
            .setHTML(`
                <div style="font-weight: bold; font-size: 13px; padding: 4px 8px; color: #06b5ef;">
                    ${name}
                </div>
            `);

        const marker = new goongjs.Marker(el)
            .setLngLat([lng, lat])
            .setPopup(popup)
            .addTo(this.map);

        // hover
        el.addEventListener('mouseenter', () => {
            if (!popup.isOpen()) marker.togglePopup();
        });
        el.addEventListener('mouseleave', () => {
            if (popup.isOpen()) marker.togglePopup();
        });
    },
    addSearchMarker: function (lat, lng, address) {
        if (!this.map) return;

        // Xóa marker tìm kiếm cũ nếu có
        if (this.searchMarker) this.searchMarker.remove();

        const popup = new goongjs.Popup({ offset: 25, closeButton: false })
            .setHTML(`<div style="padding:5px; font-weight:bold;">${address}</div>`);

        this.searchMarker = new goongjs.Marker({ color: "#ef4444" }) // Màu đỏ (Red-500)
            .setLngLat([lng, lat])
            .setPopup(popup)
            .addTo(this.map);

        this.searchMarker.togglePopup(); // Hiện tên luôn
    }
};
// ------------------------------------------------
//Dùng cho CreateListing.razor, Editlisting.razor, ListingDetail.razor (chọn vị trí trên map để lấy lat/lng)
window.mapInterop = {
    initMap: function (dotNetHelper, elementId, initialLat, initialLng) {
        var id = elementId || 'map';

        // Cleanup existing map if it exists
        if (this.maps[id]) {
            this.maps[id].remove();
            delete this.maps[id];
        }

        // Safeguard for Leaflet internal state
        var container = L.DomUtil.get(id);
        if (container && container._leaflet_id) {
            container._leaflet_id = null;
        }

        console.log("Map initialized on " + id + " with:", initialLat, initialLng);

        var startLat = (initialLat && initialLat !== 0) ? initialLat : 16.0544;
        var startLng = (initialLng && initialLng !== 0) ? initialLng : 108.2022;

        var map = L.map(id).setView([startLat, startLng], 14);

        L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
            attribution: '© OpenStreetMap'
        }).addTo(map);

        var marker;

        // Add initial marker if coordinates are provided
        if (initialLat !== 0 || initialLng !== 0) {
            marker = L.marker([startLat, startLng]).addTo(map);
        }

        map.on('click', function (e) {
            var lat = e.latlng.lat;
            var lng = e.latlng.lng;

            // update marker
            if (marker) {
                marker.setLatLng(e.latlng);
            } else {
                marker = L.marker(e.latlng).addTo(map);
            }

            // 🔥 gọi trực tiếp qua dotNetHelper
            if (dotNetHelper) {
                dotNetHelper.invokeMethodAsync("OnMapClick", lat, lng);
            }
        });

        this.maps[id] = map;

        // Force resize for modals
        setTimeout(function () {
            map.invalidateSize();
        }, 400);
    },

    // Store multiple map instances if needed
    maps: {},

    initReadOnlyMap: function (elementId, lat, lng) {
        var id = elementId || 'map';

        // Cleanup existing map if it exists
        if (this.maps[id]) {
            this.maps[id].remove();
            delete this.maps[id];
        }

        // Safeguard for Leaflet internal state
        var container = L.DomUtil.get(id);
        if (container && container._leaflet_id) {
            container._leaflet_id = null;
        }

        var startLat = (lat && lat !== 0) ? lat : 16.0544;
        var startLng = (lng && lng !== 0) ? lng : 108.2022;

        var map = L.map(id).setView([startLat, startLng], 14);

        L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
            attribution: '© OpenStreetMap'
        }).addTo(map);

        if (startLat !== 16.0544 || startLng !== 108.2022) {
            L.marker([startLat, startLng]).addTo(map);
        }

        this.maps[id] = map;

        // Force resize after short delay to handle modal rendering
        setTimeout(function () {
            map.invalidateSize();
        }, 400);
    }
};
