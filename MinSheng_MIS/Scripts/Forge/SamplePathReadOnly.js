//import '/Scripts/ForgeDraw.js'
//import '/Scripts/datatable.js'
//import '/Scripts/Forge/UpViewer.js'
//import '/Scripts/Forge/ForgeDrawFunction.js'


function initializeDrawer(data) {
    var view = document.querySelector("#PathCanvas");
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

    ForgeDraw.init(view, viewer, function () {
        ForgeDraw.setControl(ForgeDraw.Control.READONLY);
        createBeacons(data.PathSample.BIMDevices)
        createLinePath(data.PathSampleRecord);
        updatePathDisplay(data.PathSampleOrder);
    });

    window.addEventListener("resize", function () { ForgeDraw.resize(); })
}