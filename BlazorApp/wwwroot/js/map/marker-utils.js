// 마커 생성 및 이벤트 관리
const MarkerManager = {
    createIcon: (isReg) => ({
        content: `
      <div style="background:${isReg ? '#1976d2' : '#ff5252'};
           width:24px;height:24px;border-radius:50%;
           border:2px solid white;box-shadow:0 2px 6px rgba(0,0,0,0.3);">
      </div>`,
        size: new naver.maps.Size(24, 24),
        anchor: new naver.maps.Point(12, 12)
    }),

    createMarker: (map, position, isReg) => {
        return new naver.maps.Marker({
            map,
            position,
            icon: MarkerManager.createIcon(isReg)
        });
    },

    addClickHandler: (marker, callback) => {
        naver.maps.Event.addListener(marker, 'click', callback);
    }
};
export default MarkerManager;