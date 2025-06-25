import { CoordConverter } from './coordinate-service.js';
import MarkerManager from './marker-utils.js';
import { MapRenderer } from './map-renderer.js';
import { DistanceCalculator } from "./distance-utils.js";

window.naverMapUtils = {
    initializeNaverMap: async (dotNetRef, elementId, markers) => {
                
        const perf = { total: 0, conversion: 0, filtering: 0, rendering: 0 };
        const startTotal = performance.now();
        
        try {
            // 1. 현재 위치 획득
            const position = await new Promise(resolve =>
                navigator.geolocation.getCurrentPosition(
                    resolve,
                    () => resolve({ coords: { latitude: 37.5665, longitude: 126.9780 }})
                )
            );

            // 2. 지도 초기화
            const map = MapRenderer.init(elementId, {
                lat: position.coords.latitude,
                lng: position.coords.longitude
            });
            MapRenderer.drawRadius(map, map.getCenter(), 3000);
            
            // 3. 좌표 변환 (Web Worker)
            const startConv = performance.now();
            const converted = await CoordConverter.batchConvert(markers);
            perf.conversion = performance.now() - startConv;

            // 4. 거리 필터링
            const startFilter = performance.now();
            const validMarkers = converted.filter(m =>
                DistanceCalculator.fast(
                    position.coords.latitude,
                    position.coords.longitude,
                    m.lat,
                    m.lng
                ) <= 3
            );
            perf.filtering = performance.now() - startFilter;

            // 5. 마커 렌더링
            const startRender = performance.now();
            validMarkers.forEach(marker => {
                const markerObj = MarkerManager.createMarker(
                    map,
                    new naver.maps.LatLng(marker.lat, marker.lng),
                    marker.isReg
                );
                MarkerManager.addClickHandler(markerObj, () => {
                    dotNetRef.invokeMethodAsync('OnMarkerClicked', JSON.stringify(marker));
                });
            });
            perf.rendering = performance.now() - startRender;

            // 6. 성능 로깅
            perf.total = performance.now() - startTotal;
            console.table(perf);

            return true;

        } catch (error) {
            console.error(`지도 초기화 실패: ${error.stack}`);
            throw error;
        }
    }
};
