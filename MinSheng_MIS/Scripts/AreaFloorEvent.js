async function AreaFloorEvent(){
    const ASN = document.getElementById("ASN");
    ASN.addEventListener("change", function () {
        if (ASN.value != "") {
            $('#Area').val($("#ASN option:selected").text());
        }
        else {
            $('#Area').val('');
        }
        pushSelect("FSN", `/DropDownList/Floor?ASN=${ASN.value}`);
        $('#Floor').val('');
    });
    const FSN = document.getElementById("FSN");
    FSN.addEventListener("change", function () {
        if (FSN.value != "") {
            $('#Floor').val($("#FSN option:selected").text());
        }
        else {
            $('#Floor').val('');
        }
    });
    await pushSelect("ASN", '/DropDownList/Area');
}