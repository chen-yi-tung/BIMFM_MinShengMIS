var view = document.querySelector("#PathCanvas");
var app;

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
        let pathData = JSON.parse(sessionStorage.getItem(`P1_pathData`))

        createBeacons(pathData.PathSample.Beacon);
        createDevices(1);
        createLinePath(pathData.PathSampleRecord);
        updatePathDisplay(pathData.PathSampleOrder, 1);

        addTooltip(true);
    });
}

window.addEventListener('load', function () {
    pathGroup.setSetting({
        startButton: {
            id: "path-search",
            icon: "icon-search",
            tooltip: "查看此路線",
            onClick: (e) => {
                let pathID = $(e.target).closest(".sample-path-group").attr("data-path-id");
                let pathData = JSON.parse(sessionStorage.getItem(`P${pathID}_pathData`))
                $("#current-path-title").val(pathData.PathSample.PathTitle);
                loadModel(
                    window.location.origin + pathData.PathSample.BIMPath,
                    pathID,
                    loadPath)
            }
        }
    })
})