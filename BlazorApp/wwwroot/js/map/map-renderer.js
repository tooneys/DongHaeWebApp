// 지도 렌더링 엔진
export const MapRenderer = {
    init: (elementId, center) => new naver.maps.Map(elementId, {
        center: new naver.maps.LatLng(center.lat, center.lng),
        zoom: 14
    }),

    drawRadius: (map, center, radius) => new naver.maps.Circle({
        map,
        center,
        radius,
        fillColor: 'rgba(25,118,210,0.1)',
        strokeColor: '#1976d2',
        strokeWeight: 2
    })
};
