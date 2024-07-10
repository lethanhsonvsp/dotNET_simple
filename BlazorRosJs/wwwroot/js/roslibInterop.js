(function () {
    var ros;
    var reconnectInterval = 1000; // 1 second

    function connectToRos(rosbridgeUrl) {
        ros = new ROSLIB.Ros({
            url: rosbridgeUrl
        });

        if (ros) {
            ros.on('connection', function () {
                console.log('Connected to websocket server.');
                updateRosStatus('Connected to Ros!', 'green');
                invokeDotNetMethod('OnRosConnected');
            });

            ros.on('error', function (error) {
                console.log('Error connecting to websocket server: ', error);
                updateRosStatus('Error!', 'red');
                invokeDotNetMethod('OnRosError', error.message);
            });

            ros.on('close', function () {
                console.log('Connection to websocket server closed.');
                updateRosStatus('Disconnected!', 'red');
                invokeDotNetMethod('OnRosClosed');
                setTimeout(function () {
                    console.log('Attempting to reconnect...');
                    connectToRos(rosbridgeUrl);
                }, reconnectInterval);
            });
        } else {
            console.error('ROS object is undefined.');
            updateRosStatus('ROS object is undefined!', 'red');
        }
    }

    function updateRosStatus(statusText, color) {
        var statusElement = document.getElementById('ros-status');
        if (statusElement) {
            statusElement.innerText = statusText;
            statusElement.style.color = color;
        }
    }

    function invokeDotNetMethod(methodName, arg) {
        if (arg !== undefined) {
            DotNet.invokeMethodAsync('BlazorWithRoslibjs', methodName, arg);
        } else {
            DotNet.invokeMethodAsync('BlazorWithRoslibjs', methodName);
        }
    }

    function subscribeTopic(topicName, messageType, callback) {
        var topic = new ROSLIB.Topic({
            ros: ros,
            name: topicName,
            messageType: messageType
        });

        topic.subscribe(function (message) {
            var messageStr = JSON.stringify(message);
            invokeDotNetMethod(callback, messageStr);
        });
    }

    function initMap() {
        if (typeof ROS2D === 'undefined' || typeof ros === 'undefined') {
            console.error('ROS2D or ros is not defined');
            return;
        }

        // Create the main viewer.
        var viewer = new ROS2D.Viewer({
            divID: 'map',
            width: 640,
            height: 480,
            context2dOptions: { willReadFrequently: true }
        });

        // Setup the map client.
        var gridClient = new NAV2D.OccupancyGridClientNav({
            ros: ros,
            rootObject: viewer.scene,
            viewer: viewer,
            serverName: "/move_base",
            withOrientation: true,
        });
    }

    window.initMap = initMap;
    window.connectToRos = connectToRos;
    window.subscribeTopic = subscribeTopic;
})();
