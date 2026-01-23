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
    addMarker: function (dotNetRef, id, lat, lng, title, price, address) {
        if (!this.map) return;

        // Return if marker already exists to avoid duplicates
        if (this.markers[id]) return;

        const popupContent = `
            <div style="font-family: sans-serif; padding: 5px; min-width: 150px;">
                <h3 style="margin: 0 0 5px 0; font-weight: bold; font-size: 14px;">${title}</h3>
                <p style="margin: 0 0 5px 0; font-size: 12px; color: #555;">${address}</p> 
                <p style="margin: 0; font-weight: bold; color: #ea580c;">${price}</p>
            </div>
        `;

        // ... rest of function ...

        const popup = new goongjs.Popup({ offset: 25, closeButton: false }).setHTML(popupContent);

        const marker = new goongjs.Marker({ color: "#ea580c" })
            .setLngLat([lng, lat])
            .setPopup(popup)
            .addTo(this.map);

        const el = marker.getElement();
        el.style.cursor = "pointer";

        // Hover: chỉ mở popup nếu đang đóng
        el.addEventListener("mouseenter", () => {
            if (!popup.isOpen()) marker.togglePopup();
        });
        el.addEventListener("mouseleave", () => {
            if (popup.isOpen()) marker.togglePopup();
        });

        // Click → gọi C# để mở popup bên Blazor
        el.addEventListener('click', () => {
            dotNetRef.invokeMethodAsync('OnMarkerClick', id);
        });

        this.markers[id] = marker;
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
