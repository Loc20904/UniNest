window.uniNestMap = {
    map: null, // Biến map toàn cục để lưu trữ instance
    markers: [], // Lưu danh sách marker để quản lý

    // 1. Hàm load script (không đổi)
    loadGoogleMapsScript: function (apiKey) {
        return new Promise((resolve, reject) => {
            if (window.google && window.google.maps) {
                resolve();
                return;
            }
            var script = document.createElement("script");
            script.src = `https://maps.googleapis.com/maps/api/js?key=${apiKey}&callback=initMapPlaceholder&v=weekly`;
            script.async = true;
            script.defer = true;
            document.head.appendChild(script);

            window.initMapPlaceholder = function () {
                resolve();
            };
        });
    },

    // 2. Hàm khởi tạo Map
    initMap: function (elementId, lat, lng, zoom) {
        var mapOptions = {
            center: { lat: parseFloat(lat), lng: parseFloat(lng) },
            zoom: zoom,
            mapTypeId: google.maps.MapTypeId.ROADMAP
        };

        var mapElement = document.getElementById(elementId);
        if (mapElement) {
            // Gán vào biến this.map để dùng lại ở hàm khác
            this.map = new google.maps.Map(mapElement, mapOptions);
            console.log("Map initialized success!");
        } else {
            console.error("Không tìm thấy thẻ div id=" + elementId);
        }
    },

    // 3. Hàm thêm Marker (SỬA LẠI CHỖ NÀY)
    addMarker: function (lat, lng, title, price, address) {
        if (!this.map) {
            console.error("Map chưa khởi tạo, không thể add marker!");
            return;
        }

        console.log(`Đang vẽ marker: ${title} tại [${lat}, ${lng}]`); // Log để debug

        var position = { lat: parseFloat(lat), lng: parseFloat(lng) };

        var marker = new google.maps.Marker({
            position: position,
            map: this.map, // QUAN TRỌNG: Phải trỏ đúng vào biến map đã tạo
            title: title,
            animation: google.maps.Animation.DROP
        });

        // Tạo info window khi click vào marker
        var infoWindow = new google.maps.InfoWindow({
            content: `<div style="color:black; padding:5px">
                        <b>${title}</b><br/>
                        <span>Giá: ${price}</span><br/>
                        <small>${address}</small>
                      </div>`
        });

        marker.addListener("click", () => {
            infoWindow.open(this.map, marker);
        });

        this.markers.push(marker);
    }
};