// 마커 생성 및 이벤트 관리
const MarkerManager = {
    markers: {}, // 마커를 ID로 관리하는 객체

    createIcon: function (isReg, OpticianManage) {

        // OpticianManage: "001" (신규관리점), "002" (관리제외점)
        let color = "#ff5252"; // 기본(회색/빨강)
        if(isReg) color = "#1976d2";
        else if(OpticianManage === "001") color = "#29675b";
        else if (OpticianManage === "002") color = "#9e9e9e";

        return {
            content: `
              <div style="background:${color};
                   width:24px;height:24px;border-radius:50%;
                   border:2px solid white;box-shadow:0 2px 6px rgba(0,0,0,0.3);">
              </div>`,
            size: new naver.maps.Size(24, 24),
            anchor: new naver.maps.Point(12, 12)
        };
    },

    createMarker: (map, position, isReg, OpticianManage, markerId) => {
        const marker = new naver.maps.Marker({
            map,
            position,
            icon: MarkerManager.createIcon(isReg, OpticianManage)
        });

        // 마커를 ID로 저장
        if (markerId) {
            MarkerManager.markers[markerId] = marker;
        }

        return marker;
    },

    getMarker: (markerId) => {
        return MarkerManager.markers[markerId];
    },

    updateMarkerIcon: (markerId, isReg, OpticianManage) => {
        const marker = MarkerManager.getMarker(markerId);

        if (marker) {
            marker.setIcon(MarkerManager.createIcon(isReg, OpticianManage));
        }
    },

    removeMarker: (markerId) => {
        const marker = MarkerManager.getMarker(markerId);
        if (marker) {
            marker.setMap(null); // 지도에서 제거
            delete MarkerManager.markers[markerId]; // 관리 객체에서도 제거
        }
    },

    addClickHandler: (marker, callback) => {
        naver.maps.Event.addListener(marker, 'click', callback);
    }
};
export default MarkerManager;