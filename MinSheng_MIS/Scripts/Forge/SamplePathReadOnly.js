//import '/Scripts/ForgeDraw.js'
//import '/Scripts/datatable.js'
//import '/Scripts/Forge/UpViewer.js'
//import '/Scripts/Forge/ForgeDrawFunction.js'

var view = document.querySelector("#PathCanvas");
var app;
var stage;

function initializeDrawer() {
    ForgeDraw.setDrawSetting("point.contextMenu.button", [{
        name: "詳細",
        onClick: function (event, point) {
            console.log(`point ${point.index} => 詳細`, point);
            createPointDetailModal(point);
        }
    }]);
    ForgeDraw.setDrawSetting("devicePoint.contextMenu.button", [{
        name: "詳細",
        onClick: function (event, point) {
            console.log(`point ${point.index} => 詳細`, point);
            createDevicePointDetailModal(point);
        }
    }]);

    app = ForgeDraw.init(view, viewer, function () {
        ForgeDraw.setControl(ForgeDraw.Control.READONLY);

        createBeacons(fakeData.PathSample.BIMDevices)

        createLinePath(fakeData.PathSampleRecord);


        updatePathDisplay(fakeData.PathSampleOrder);
    });

    window.onresize = function () {
        ForgeDraw.resize();
    }
}