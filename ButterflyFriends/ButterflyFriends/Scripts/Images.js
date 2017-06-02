Vue.component('modal',
{
    template: '#DeleteModal',
    props: {
        show: {
            type: Boolean,
            required: true,
            twoWay: true
        }
    },
    methods: {
        close: function () {
            this.show = false;
        },
        submit: function () {
            deleteImage($("#deleteId").val());
            this.show = false;
        }
    },
    ready: function () {
        document.addEventListener("keydown",
            (e) => {
                if (this.show && e.keyCode == 27) {

                    this.close();
                } else if (this.show && e.keyCode == 13) {
                    this.submit();
                }


            });
    }
});

var deleteV = new Vue({
    el: '#deletePrompt',
    data: {
        showModal: false
    }
});

function filterImages() {
    var formData = new FormData();
    formData.append('search', $('#searchField').val());
    formData.append('active', $('#Active input:radio:checked').val());
    formData.append('order', $('#OrderFilter input:radio:checked').val());
    formData.append('filter', $("#Selected option:selected").val());
    formData.append('filename', $("#Filename").val());

    formData.append('date', $('#Date').val());
    formData.append('user', $('#Sponsor').val());
    formData.append('child', $('#Child').val());


    $.ajax({
        url: '/Admin/Home/FilterImages',
        type: 'POST',
        async: true,
        contentType: false,
        processData: false,
        data: formData,
        success: function (result) {
            $('#images').html(result);
        },
        error: function (err) {
            $("#error").text(err.statusMessage);
        }
    });
}

$("#Reset")
    .on('click',
        function () {
            event.stopPropagation();
            $('#searchField').val('');

            $('#Date').val('');
            $('#Sponsor').val('');
            $('#Child').val('');

            document.getElementById("Default").selected = "true";
            $("#OrderDefault").prop("checked", true);
            $("#DefaultRadio").prop("checked", true);
        });

$("#searchField")
    .on('keydown',
        function () {
            if (event.keyCode == 13) {
                filterImages();
                return false;
            }
        });
$(document)
    .ready(function () {

        $(document)
            .ajaxStart(function () {
                $(".loader").show();
            })
            .ajaxStop(function () {
                $(".loader").hide();
            });
    });

$(document) //pager function, move in list corresponding to number clicked
    .on("click",
        "#Pager a[href]",
        function () {
            var formData = new FormData();
            formData.append('search', $('#searchField').val());
            formData.append('active', $('#Active input:radio:checked').val());
            formData.append('order', $('#OrderFilter input:radio:checked').val());
            formData.append('filter', $("#Selected option:selected").val());
            formData.append('filename', $("#Filename").val());

            formData.append('date', $('#Date').val());
            formData.append('user', $('#Sponsor').val());
            formData.append('child', $('#Child').val());
            $.ajax({
                url: $(this).attr("href"),
                type: 'POST',
                async: true,
                contentType: false,
                processData: false,
                data: formData,
                cache: false,
                success: function (result) {
                    $('#images').html(result);
                },
                error: function (err) {
                    $("#error").text(err.statusMessage);
                }
            });
            return false;
        });
$(function () {
    $("#Date")
        .datepicker({
            dateFormat: "dd.mm.yy"
        });
});

function imagePublish(id) { //choose to publish or not publish image
    var formData = new FormData();
    formData.append("id", id);
    formData.append('search', $('#searchField').val());
    formData.append('active', $('#Active input:radio:checked').val());
    formData.append('order', $('#OrderFilter input:radio:checked').val());
    formData.append('filter', $("#Selected option:selected").val());

    formData.append('date', $('#Date').val());
    formData.append('user', $('#Sponsor').val());
    formData.append('child', $('#Child').val());
    formData.append('page', $("#page").val());
    formData.append('filename', $("#Filename").val());
    $.ajax({
        url: '/Admin/Home/PublishImage',
        type: 'POST',
        async: true,
        contentType: false,
        processData: false,
        data: formData,
        success: function (result) {
            $('#images').html(result);
        },
        error: function (err) {
            $("#error").text(err.statusMessage);
        }
    });
}

function deleteImage(id) {
    var formData = new FormData();
    formData.append("id", id);
    formData.append('search', $('#searchField').val());
    formData.append('active', $('#Active input:radio:checked').val());
    formData.append('order', $('#OrderFilter input:radio:checked').val());
    formData.append('filter', $("#Selected option:selected").val());

    formData.append('date', $('#Date').val());
    formData.append('user', $('#Sponsor').val());
    formData.append('child', $('#Child').val());
    formData.append('page', $("#page").val());
    formData.append('filename', $("#Filename").val());
    $.ajax({
        url: '/Admin/Home/DeleteImage',
        type: 'POST',
        async: true,
        contentType: false,
        processData: false,
        data: formData,
        success: function (result) {
            $('#images').html(result);
        },
        error: function (err) {
            $("#error").text(err.statusMessage);
        }
    });
}

function deletePrompt(id) {
    $("#deleteId").val(id);
    deleteV.showModal = true;

}

function imageUpdate(id, imgId) { //update image, append filter values to form
    var formData = new FormData();
    formData.append("id", id);
    formData.append('search', $('#searchField').val());
    formData.append('active', $('#Active input:radio:checked').val());
    formData.append('order', $('#OrderFilter input:radio:checked').val());
    formData.append('filter', $("#Selected option:selected").val());

    formData.append('date', $('#Date').val());
    formData.append('user', $('#Sponsor').val());
    formData.append('child', $('#Child').val());
    formData.append('page', $("#page").val());
    formData.append('filename', $("#Filename").val());
    formData.append('caption', $("#" + imgId + "ta").val());

    var originalHeight = $("#" + imgId).get(0).naturalHeight; //scale boxes
    var originalWidth = $("#" + imgId).get(0).naturalWidth;
    var displayedWidth = parseInt($("#" + imgId).css("width"), 10);
    var displayedHeight = parseInt($("#" + imgId).css("height"), 10);
    var scaleY = displayedHeight / originalHeight;
    var scaleX = displayedWidth / originalWidth;

    var boxesList = [];
    $('#' + imgId + 'd')
        .children('div')
        .each(function () {
            var boxId = this.getAttribute("id");

            if ($('#' + boxId + "p").val() !== "") {
                var div = $('#' + boxId);
                var obj = {
                    "id": this.getAttribute("nameid"),
                    "name": this.getAttribute("name"),
                    "type": this.getAttribute("type"),
                    "x": parseInt(parseFloat(div.css("left")) / scaleX),
                    "y": parseInt(parseFloat(div.css("top")) / scaleY),
                    "height": parseInt(parseFloat(div.css("height")) / scaleY),
                    "width": parseInt(parseFloat(div.css("width")) / scaleX)
                };
                if (obj.name !== $('#' + boxId + "p").val()) {
                    obj.name = $('#' + boxId + "p").val();
                    obj.id = undefined;
                }

                boxesList.push(obj);
            }
        });
    boxesList = JSON.stringify(boxesList);
    formData.append('tags', boxesList);

    $.ajax({
        url: '/Admin/Home/EditImage',
        type: 'POST',
        async: true,
        contentType: false,
        processData: false,
        data: formData,
        success: function (result) {
            $('#images').html(result);
        },
        error: function (err) {
            $("#error").text(err.statusMessage);
        }
    });
}