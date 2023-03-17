//import "datatable.js"

function DataDetailModal(url) {
    let ModalJQ, ModalBs;
    const sn = [
        { text: "計畫編號", value: "IPSN" },
    ]
    $.getJSON(url, function (data) {
        const html = `
        <div class="modal fade data-detail-modal" tabindex="-1">
            <div class="modal-dialog modal-lg modal-dialog-centered">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title">${title}</h5>
                        <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                    </div>
                    <div class="modal-body">
                        ${createTableInner()}
                    </div>
                    <div class="modal-footer justify-content-center">
                        <a type="button" class="btn btn-search" href="" target="_blank">定位</a>
                    </div>
                </div>
            </div>
        </div>
        `;
        ModalJQ = $(html);
        ModalBs = new bootstrap.Modal(ModalJQ[0]);

        ModalBs.show();
    })


}