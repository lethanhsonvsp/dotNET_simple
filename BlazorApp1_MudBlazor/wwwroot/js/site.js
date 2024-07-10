function initMap() {
    // Kết nối tới ROS
    var ros = new ROSLIB.Ros({
        url: 'ws://192.168.137.218:9090'
    });

    // Tạo viewer chính
    var viewer = new windown.ROS2D.Viewer({
        divID: 'map',
        width: 600,
        height: 500
    });

    // Thiết lập map client
    var gridClient = new NAV2D.OccupancyGridClientNav({
        ros: ros,
        rootObject: viewer.scene,
        viewer: viewer,
        severName: "/move_base",
        with0rienation: true
    });

    // Scale canvas để khớp với bản đồ
    gridClient.on('change', function () {
        viewer.scaleToDimensions(gridClient.currentGrid.width, gridClient.currentGrid.height);
    });
}

// Khởi tạo bản đồ khi trang được tải
document.addEventListener("DOMContentLoaded", function () {
    initMap();
});
