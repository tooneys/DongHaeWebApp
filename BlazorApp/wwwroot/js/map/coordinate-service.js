// coordinate-service.js

// 단일 변환: 메인 스레드에서 proj4 직접 사용
// 대량 변환: Web Worker를 통한 비동기 처리

// 1. proj4js가 메인 스레드에서 필요하다면 아래 importScripts는 필요 없음
//    <script src="https://cdnjs.cloudflare.com/ajax/libs/proj4js/2.8.0/proj4.js"></script> 를 HTML에 추가하세요.
//    또는 npm/yarn으로 설치 후 import 하세요.

export const CoordConverter = (() => {
    // 캐시: 반복 변환 성능 향상
    const CACHE = new Map();
    const CACHE_KEY = (x, y) => `${x}_${y}`;

    // 좌표값 유효성 검사
    const isValidCoordinate = (x, y) =>
        typeof x === 'number' && typeof y === 'number' && Number.isFinite(x) && Number.isFinite(y);

    // 단일 변환 (메인 스레드)
    function convertEpsg5174ToWgs84(x, y) {
        if (!isValidCoordinate(x, y)) {
            throw new Error(`Invalid coordinates: ${x}, ${y}`);
        }
        const key = CACHE_KEY(x, y);
        if (CACHE.has(key)) return CACHE.get(key);

        // proj4는 index.html 등에서 전역으로 로드되어 있다고 가정
        const [lng, lat] = proj4("EPSG:5174", "EPSG:4326", [x, y]);
        const result = { lng: Number(lng.toFixed(6)), lat: Number(lat.toFixed(6)) };
        CACHE.set(key, result);
        return result;
    }

    // 대량 변환 (Web Worker)
    function batchConvert(markers) {
        return new Promise((resolve, reject) => {
            // 워커 경로는 실제 프로젝트 구조에 맞게 조정
            let worker;
            try {
                worker = new Worker('js/map/coord-converter.worker.js');
            } catch (err) {
                reject(new Error('좌표 변환 워커 로드 실패: ' + err.message));
                return;
            }

            worker.onmessage = (e) => {
                resolve(e.data);
                worker.terminate();
            };
            worker.onerror = (err) => {
                reject(new Error('좌표 변환 워커 오류: ' + err.message));
                worker.terminate();
            };

            // 유효한 좌표만 워커로 전달
            const validMarkers = markers.filter(m =>
                isValidCoordinate(Number(m.geoX), Number(m.geoY))
            );
            worker.postMessage(validMarkers);
        });
    }

    return {
        convertEpsg5174ToWgs84,
        batchConvert
    };
})();
