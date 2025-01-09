function addButtonEvent() {
    $("#back").click(function () {
        history.back();
    })

    $("#submit").click(function () {
        save();
    })
}

async function checkAuthority() {
    try {
        const res = await new Promise((resolve, reject) => {
            $.ajax({
                url: '/Account/UserAuthority',
                type: "GET",
                dataType: "json",
                contentType: "application/json;charset=utf-8",
                success: function (res) {
                    resolve(res.Datas.Authority);
                },
                error: function (res) {
                    reject(res);
                },
            });
        });
        //console.log("權限", res);
        window.__authority__ = res;
        return res;
    } catch (error) {
        console.error("權限回傳失敗", error);
        throw error;
    }
}