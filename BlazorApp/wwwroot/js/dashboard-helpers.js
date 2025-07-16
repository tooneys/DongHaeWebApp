window.blazorHelpers = {
    optimizeScroll: function (element) {
        if (element) {
            // 스크롤 성능 최적화
            element.style.willChange = 'scroll-position';
            element.style.transform = 'translateZ(0)';

            // 스크롤 이벤트 스로틀링
            let scrollTimeout;
            element.addEventListener('scroll', function () {
                if (scrollTimeout) {
                    clearTimeout(scrollTimeout);
                }
                scrollTimeout = setTimeout(() => {
                    element.style.willChange = 'auto';
                }, 150);
            });
        }
    },

    // 메모리 사용량 모니터링
    getMemoryUsage: function () {
        if (performance.memory) {
            return {
                used: performance.memory.usedJSHeapSize,
                total: performance.memory.totalJSHeapSize,
                limit: performance.memory.jsHeapSizeLimit
            };
        }
        return null;
    }
};
