//import "https://cdn.jsdelivr.net/npm/sortablejs@1.15.0/Sortable.min.js"

function CreateDeviceModal() {
    const self = this;
    this.autoCalcRoute = true;
    this.ModalJQ = $(`
    <div class="modal fade sort-list-modal" tabindex="-1" id="createDeviceModal">
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
                    
                    <div class="list-group"></div>
                </div>
                <div class="modal-footer justify-content-center">
                    <button type="button" class="btn btn-back" data-bs-dismiss="modal">取消</button>
                    <button type="button" class="btn btn-search" id="createDeviceModal_save">確定</button>
                </div>
            </div>
        </div>
    </div>
    `);
    this.ModalBs = new bootstrap.Modal(this.ModalJQ[0]);

    this.create = function (search) {
        this.ModalBs.show();

        onSuccess([
            { text: "url-1", value: "1" },
            { text: "url-2", value: "2" },
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

    this.ModalJQ.find("#createDeviceModal_save").click(function () {
        self.save();
        self.ModalBs.hide();
    });

    this.save = () => {
        let name = self.ModalJQ.find(".list-group-item .form-check-input:checked").val();
        $("#FilePath").val('');
        $("#FilePathName").text(name);
        $("#FilePathDelete").removeClass('d-none');
    };

    this.ModalJQ.one("hidden.bs.modal", () => {
        self.ModalBs.dispose();
        self.ModalJQ.remove();
    })

    function listItem(data) {
        let html = `
        <label class="list-group-item">
            <input class="form-check-input me-1" type="checkbox" value="${data.value}">
            ${data.text}
        </label>
        `
        return html;
    }

    return this;
}