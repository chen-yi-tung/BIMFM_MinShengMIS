function initializeDrawer(pathID = 1, firstLoad = true) {
    const view = document.querySelector("#PathCanvas");
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
        let pathData = JSON.parse(sessionStorage.getItem(`P1_pathData`))

        createBeacons(pathData.PathSample.Beacon);
        createDevices(pathID);
        createLinePath(pathData.PathSampleRecord);
        updatePathDisplay(pathData.PathSampleOrder, pathID);

        firstLoad && addTooltip(true);
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

                let firstLoad = true
                if (viewer != null) {
                    firstLoad = false
                    DestroyViewerAndForgeDraw()
                }

                const { BIMPath, BeaconPath } = pathData.PathSample
                initializeViewer({
                    BIMPath, BeaconPath,
                    callback: () => {
                        initializeDrawer(pathID, firstLoad)
                    }
                });
            }
        }
    })
})