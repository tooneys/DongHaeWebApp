// 거리 계산 유틸리티
export const DistanceCalculator = {
    precise: (lat1, lng1, lat2, lng2) => {
        const R = 6371;
        const dLat = (lat2 - lat1) * Math.PI / 180;
        const dLng = (lng2 - lng1) * Math.PI / 180;
        const a =
            Math.sin(dLat/2) * Math.sin(dLat/2) +
            Math.cos(lat1 * Math.PI / 180) * Math.cos(lat2 * Math.PI / 180) *
            Math.sin(dLng/2) * Math.sin(dLng/2);
        return R * 2 * Math.atan2(Math.sqrt(a), Math.sqrt(1 - a));
    },

    fast: (lat1, lng1, lat2, lng2) => {
        const dx = 111.3 * (lat2 - lat1);
        const dy = 111.3 * Math.cos((lat1 * Math.PI)/180) * (lng2 - lng1);
        return Math.sqrt(dx*dx + dy*dy);
    }
};
