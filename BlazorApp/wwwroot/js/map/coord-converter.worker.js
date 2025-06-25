// 1. proj4js 로드
importScripts('https://cdnjs.cloudflare.com/ajax/libs/proj4js/2.8.1/proj4.js');

// 2. 좌표계 정의
proj4.defs(
    "EPSG:5174",
    "+proj=tmerc +lat_0=38 +lon_0=127.0028902777778 +k=1 +x_0=200000 +y_0=500000 +ellps=bessel +towgs84=-115.8,474.99,674.11,1.16,-2.31,-1.63,6.43 +units=m +no_defs"
);
proj4.defs(
    "EPSG:4326",
    "+proj=longlat +ellps=WGS84 +datum=WGS84 +no_defs"
);

// 3. 오류 처리 강화
self.onmessage = (e) => {
    const inputMarkers = e.data;
    const results = inputMarkers.map(marker => {
        try {
            // 0. proj4 라이브러리 로드 검사
            if (!proj4) throw new Error('proj4 라이브러리 로드 실패');
            
            // 1. 좌표값 유효성 검사
            const x = Number(marker.geoX);
            const y = Number(marker.geoY);

            if (!Number.isFinite(x) || !Number.isFinite(y)) {
                throw new Error(`Invalid coordinates: ${x}, ${y}`);
            }

            // 2. 좌표 변환
            const [lng, lat] = proj4("EPSG:5174", "EPSG:4326", [x, y]);

            return {
                ...marker,
                lng: Number(lng.toFixed(6)),  // 소수점 6자리 제한
                lat: Number(lat.toFixed(6))
            };

        } catch (error) {
            console.error('변환 실패 마커:', marker, error);
            return null;
        }
    }).filter(Boolean);  // null 제거

    self.postMessage(results);
};
