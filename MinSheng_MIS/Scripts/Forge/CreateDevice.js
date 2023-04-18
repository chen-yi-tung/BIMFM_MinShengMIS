function DeviceFileModal() {
    const self = this;
    this.autoCalcRoute = true;
    this.ModalJQ = $(`
    <div class="modal fade sort-list-modal" tabindex="-1" id="DeviceFileModal">
        <div class="modal-dialog modal-dialog-centered">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title">選擇使用手冊</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                </div>
                <div class="modal-body">
                    <div class="text-center w-100" id="modal-spinner">
                        <div class="spinner-border" role="status">
                            <span class="visually-hidden">Loading...</span>
                        </div>
                    </div>
                    
                    <div class="list-group device-file-list-group"></div>
                </div>
                <div class="modal-footer justify-content-center">
                    <button type="button" class="btn btn-back" data-bs-dismiss="modal">取消</button>
                    <button type="button" class="btn btn-search" id="DeviceFileModal_save">確定</button>
                </div>
            </div>
        </div>
    </div>
    `);
    this.ModalBs = new bootstrap.Modal(this.ModalJQ[0]);

    this.create = function (search) {
        console.log(search)

        this.ModalBs.show();

        onSuccess([
            { text: "url-1", value: "1" },
            { text: "localhost_44303_SystemManagement_AuthorityManagement.png", value: "2" },
            { text: "url-3", value: "3" }
        ])

        /* $.ajax({
            url: "/",
            data: JSON.stringify(search),
            type: "POST",
            dataType: "json",
            contentType: "application/json;charset=utf-8",
            success: onSuccess,
            error: (err) => { console.log(err) }
        }) */

        function onSuccess(res) {
            let listGroup = self.ModalJQ.find(".list-group");
            res.forEach(e => {
                listGroup.append(listItem(e));
            });
            listGroup.on("change", ".form-check-input", function (e) {
                $(e.target).parent().siblings().children(".form-check-input").prop("checked", false);
            })
            self.ModalJQ.find("#modal-spinner").remove();
        }
    }

    this.ModalJQ.find("#DeviceFileModal_save").click(function () {
        self.save();
        self.ModalBs.hide();
    });

    this.save = () => {
        let c = self.ModalJQ.find(".list-group-item .form-check-input:checked")
        let name = c.siblings('span').text();
        let val = c.val();
        $("#checkFilePath_val").val(val);
        $("#FilePathName").text(name);
        $("#FilePathDelete").removeClass('d-none');
    };

    this.ModalJQ.one("hidden.bs.modal", () => {
        self.ModalBs.dispose();
        self.ModalJQ.remove();
    })

    function listItem(data) {
        let html = `
        <label class="list-group-item d-flex">
            <input class="form-check-input me-2 flex-shrink-0" type="checkbox" value="${data.value}">
            <span class="text-break">${data.text}</span>
        </label>
        `
        return html;
    }

    return this;
}

function initDrawerLocate() {
    var view = document.querySelector("#PathCanvas");
    var app;
    app = ForgeDraw.init(view, viewer, function () {
        ForgeDraw.setControl(ForgeDraw.Control.DEVICE);
        view.addEventListener("fd.devicepoint.change", function (event) {
            let fos = event.detail;
            console.log("view => fd.devicepoint.change", fos.point);
            if (fos && fos.point) {
                $("#LocationX").text(fos.point.x);
                $("#LocationY").text(fos.point.y);
            }
            else {
                !$("#DialogModal-Error")[0] &&
                    createDialogModal({ id: "DialogModal-Error", inner: "超出模型範圍！" })
                ForgeDraw.stage.off("pointermove", ForgeDraw.stage.onDeviceMoveEvent);
                ForgeDraw.stage.off("pointerup", ForgeDraw.stage.onDeviceUpEvent);

            }
        });
    });
}

function addButtonEvent() {
    $("#back").click(function () {
        history.back();
    })

    $("#checkFilePath").click(function () {
        let System = $("#System").val();
        let SubSystem = $("#SubSystem").val();
        let EName = $("#EName").val();
        let Brand = $("#Brand").val();
        let Model = $("#Model").val();
        if (System && SubSystem && EName && Brand && Model) {
            new DeviceFileModal().create({
                System,
                SubSystem,
                EName,
                Brand,
                Model
            });
        }
        else {
            let label;
            switch (true) {
                case (!System):
                    label = $('label[for="System"]').text();
                    break;
                case (!SubSystem):
                    label = $('label[for="SubSystem"]').text();
                    break;
                case (!EName):
                    label = $('label[for="EName"]').text();
                    break;
                case (!Brand):
                    label = $('label[for="Brand"]').text();
                    break;
                case (!Model):
                    label = $('label[for="Model"]').text();
                    break;
            }
            createDialogModal({ id: "DialogModal-Error", inner: `請輸入${label}！` })
        }
    })

    $("#FilePath").change(function (e) {
        console.log(this.files);
        if (this.files && this.files.length !== 0) {
            let file = this.files[0];
            $("#FilePathName").text(file.name);
            $("#FilePathDelete").removeClass('d-none');
        }
    })

    $("#FilePathDelete").click(function () {
        $("#FilePath").val('');
        $("#FilePathName").text('');
        $("#FilePathDelete").addClass('d-none');
    })

    $("#locate").click(function () {

        if (!getAreaFloor()) return;

        let data = {
            ASN: $("#ASN").val(),
            FSN: $("#FSN").val(),
            PathTitle: ''
        };

        $.ajax({
            url: "/InspectionPlan_Management/AddPlanPath",
            data: JSON.stringify(data),
            type: "POST",
            dataType: "json",
            contentType: "application/json;charset=utf-8",
            success: (res) => {
                console.log(res);
                toggleLocateState(true);
                viewerUrl = window.location.origin + res.PathSample.BIMPath;
                if (viewer) {
                    viewer.loadModel(viewerUrl, { keepCurrentModels: false },
                        (res) => { console.log(res) },
                        (err) => { console.log(err) }
                    );
                }
                else {
                    initializeViewer(initDrawerLocate);
                }
            },
            error: (err) => { console.log(err) }
        })
    })

    $("#submit").click(function () {
        console.log("onclick submit")
        save();
    })
}

function toggleLocateState(bool) {

    $("#locate-draw-area").toggleClass("d-none", !bool);
    $("#locate").toggleClass("d-none", bool);

    if (bool) {
        $("#ASN").on("mousedown", AreaFloorChange);
        $("#FSN").on("mousedown", AreaFloorChange);
    }
    else {
        $("#ASN").off("mousedown");
        $("#FSN").off("mousedown");
    }

    function AreaFloorChange(e) {
        e.preventDefault();
        e.stopPropagation();
        let modal = createDialogModal({
            id: "DialogModal", inner: `確定要更改棟別樓層？這會導致重設設備定位點。`,
            button: [
                { className: "btn btn-cancel", cancel: true, text: "取消" },
                {
                    className: "btn btn-delete", text: "確定", onClick: () => {
                        toggleLocateState(false);
                        modal.hide();
                    }
                },
            ]
        })
    }

}

function getAreaFloor() {
    let area = $("#Area").val();
    let floor = $("#Floor").val();

    $("#current-path-asn").val($("ASN").val());
    $("#current-path-fsn").val($("FSN").val());

    if (area == '' || floor == '') {
        createDialogModal({ id: "DialogModal-Error", inner: `請選擇棟別樓層！` })
        return false;
    }

    $("#current-path-areafloor").text(`${area} ${floor}`);
    return true;
}