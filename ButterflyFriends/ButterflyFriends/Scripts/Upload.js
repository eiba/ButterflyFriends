var imgNum = 0;
var imgNumArray = [];
var inside = false;
var n = 0;
var tagObj = {};

function handleFileSelect(evt) {
    var files = evt.target.files; // FileList object

    var list = document.getElementById("list");
    while (list.firstChild) {
        list.removeChild(list.firstChild);
    }
    imgNumArray = [];

    // Loop through the FileList and render image files as thumbnails.
    for (var i = 0, f; f = files[i]; i++) {
        f.num = i;

        // Only process image files.
        if (!f.type.match('image.*')) {
            continue;
        }

        var reader = new FileReader();

        // Closure to capture the file information.
        reader.onload = (function (theFile) {
            return function (e) {
                // Render thumbnail.
                var span = document.createElement('span');
                imgNum = theFile.num;
                span.addClass = "imageDiv";
                span.innerHTML = [
                    '<div class="row"><div class="col-md-8"><div tags="0" class="imgSpan" id="', imgNum + "s",
                    '"><img class="imageShow" id="', imgNum, '" src="', e.target.result,
                    '" title="', escape(theFile.name), '"/></div><textarea class="form-control" id="',
                    imgNum + "ta",
                    '" placeholder="Skriv litt om bildet..."></textarea></div><div class="col-md-4 rightImageDiv"><btn id="', imgNum + "b", '" class="btn btn-primary"><span class="glyphicon glyphicon-open"></span> Last opp</btn><br/><br/><div class="statustxt" id="', imgNum + "t", '">0%</div><div class="progress"><div id="', imgNum + 'p', '" class="progress-bar progress-bar-striped active" role="progressbar" aria-valuenow="40" aria-valuemin="0" aria-valuemax="100" style="width:0%"></div></div><span id="', imgNum + "st", '"></span><br/><btn id="', imgNum + "bf", '" class="btn btn-primary">Detekt ansikter</btn><btn id="', imgNum + "bd", '" class="btn btn-primary" style="margin-left:30px;">Fjern alle tags</btn></div></div><hr>'
                ].join('');
                document.getElementById('list').insertBefore(span, null);

                addHandeler(imgNum);

            };
        })(f);

        // Read in the image file as a data URL.
        reader.readAsDataURL(f);
    }
    $('#buttonAll').attr("disabled", false);
    $('#buttonAll').css({ "display": "" });
}

document.getElementById('files').addEventListener('change', handleFileSelect, false);

function addHandeler(id) {

    $("#" + id + 's')
        .dblclick(function (e) {
            n += 1;
            var parentOffset = $(this).parent().offset();

            var x = (e.pageX - parentOffset.left - 15) - 75;
            var y = (e.pageY - parentOffset.top) - 75;
            var width = 150;
            var height = 150;
            if (x + width > $("#" + id).width()) { //width of box is too much, move inside image
                var overflowX = x + width - ($("#" + id).width());
                x -= overflowX;
            }
            if (y + height > $("#" + id).height()) { //height of tagbox is too much, move inside image
                var overflowY = y + height - ($("#" + id).height());
                y -= overflowY;
            }
            if (x < 0) {
                x = 0;
            }
            if (y < 0) {
                y = 0;
            }


            createTag(id, n, x, y, width, height);

        });

    $("#" + id + "b") //check that file is image and send it
        .on('click',
            function () {
                var fileUpload = $("#files").get(0);
                var files = fileUpload.files;
                if (files[id].type.match('image.*')) {
                    sendFile(id);
                } else {
                    alert("You can only upload images");
                }
            });

    $("#" + id + "bf")
        .on('click',
            function () {
                $('#' + id)
                    .faceDetection({
                        complete: function (faces) {

                            if (faces.length === 0) {
                                $("#" + id + "st").text("Fant ingen ansikter");
                            };
                            for (var i = 0; i < faces.length; i++) {
                                var x = (faces[i].x * faces[i].scaleX);
                                var y = (faces[i].y * faces[i].scaleY) + 5;
                                var height = faces[i].height * faces[i].scaleY + 50;
                                var width = faces[i].width * faces[i].scaleX + 10;

                                createTag(id, n, x, y, width, height);
                                n += 1;
                            }
                        }
                    });
            });
    $("#" + id + "bd")
        .on('click',
            function () {
                $('#' + id + 's')
                    .children('div')
                    .each(function () {
                        document.getElementById(id + "s").removeChild(this);
                    });
            });
}

function createTag(id, n, x, y, width, height) { //creates a tag box

    var div = $('<div class="box" id="' +
            id +
            '' +
            n +
            '"><span onclick="func(this)" class="glyphicon glyphicon-remove-circle"></span></div>')
        .css({
            "left": x + 'px',
            "top": y + 'px',
            "width": width,
            "height": height
        });

    var input = $('<input id="' + id + '' + n + 'p' + '" class="inp tagInput">');
    $("#" + id + 's').append(div);
    div.append(input);

    $("#" + id + "" + n)
        .draggable({
            cursor: "crosshair",
            containment: "#" + id
        });
    $("#" + id + "" + n)
        .resizable({
            containment: "#" + id
        });


    $("#" + id + "" + n)
        .on('click',
            function (e) {
                e.stopPropagation();
                e.preventDefault();
            });

    $("#" + id + "" + n + "p")
        .on('input',
            function () {
                if (!($(this).val())) {
                    return;
                }
                var text = $("#" + id + "" + n + "p").val();
                $.ajax({
                    url: '/Admin/Home/GetUsers',
                    async: true,
                    contentType: false,
                    processData: false,
                    type: "POST",
                    dataType: "json",
                    data: text,
                    xhr: function () { // Custom XMLHttpRequest
                        var myXhr = $.ajaxSettings.xhr();
                        if (myXhr.upload) { // Check if upload property exists

                            // For handling the progress of the upload
                            //myXhr.upload.id = id;
                            //myXhr.upload.addEventListener('progress', progressHandlingFunction, false);

                        }
                        return myXhr;
                    },
                    success: function (result) {

                        var tags = [];
                        for (var i = 0; i < result.length; i++) {
                            var obj = {
                                label: result[i].Name,
                                id: result[i].Id,
                                imgid: result[i].imgId,
                                type: result[i].type,
                                email: result[i].email
                            };
                            tags.push(obj);

                        }
                        $("#" + id + "" + n + "p")
                            .autocomplete({
                                source: tags,
                                select: function (event, ui) {
                                    $("#" + id + "" + n).attr("name", ui.item.label);
                                    $("#" + id + "" + n).attr("nameid", ui.item.id);
                                    $("#" + id + "" + n).attr("type", ui.item.type);
                                    $("#" + id + "" + n).attr("title", ui.item.label);

                                    event.preventDefault();

                                },
                                focus: function (event, ui) {
                                    $("#" + id + "" + n + "p").val(ui.item.label);
                                    $("#" + id + "" + n).attr("name", ui.item.label);
                                    $("#" + id + "" + n).attr("nameid", ui.item.id);
                                    $("#" + id + "" + n).attr("type", ui.item.type);
                                    return false;
                                }
                            });
                        $("#" + id + "" + n + "p").data("ui-autocomplete")._renderItem = function (ul, item) {
                            //render the user list

                            if (item.imgid === 0) {

                                var $li = $('<li>');

                                var $div;
                                if (item.email == "") {
                                    $div =
                                        $('<label class="autoLabel"><div class="row"><div class="col-md-3"><i class="fa fa-question-circle autoQuestionChild" aria-hidden="true"></i></div><div class="col-md-9">' + item.label + '</div></div></label>');

                                } else {
                                    $div =
                                        $('<label class="autoLabel"><div class="row"><div class="col-md-3"><i class="fa fa-question-circle autoQuestion" aria-hidden="true"></i></div><div class="col-md-9">' + item.label + '<br><span class="autoEmailStyle">' + item.email + '</span></div></div></label>');

                                }

                                $li.attr('data-value', item.label);
                                $li.append('<a>');
                                $li.find('a').append($div);

                                return $li.appendTo(ul);
                            }
                            var $li = $('<li>');

                            var $div;
                            if (item.email == "") {
                                $div =
                                    $('<label class="autoLabel"><div class="row"><div class="col-md-3"><img src="../FileAdmin?id=' + item.imgid + '" class="thumbnailShow"></div><div class="col-md-9">' + item.label + '</div></div></label>');
                            } else {
                                $div =
                                    $('<label class="autoLabel"><div class="row"><div class="col-md-3"><img src="../FileAdmin?id=' + item.imgid + '" class="thumbnailShow"></div><div class="col-md-9">' + item.label + '<br><span class="autoEmailStyle">' + item.email + '</span></div></div></label>');

                            }

                            $li.attr('data-value', item.label);
                            $li.append('<a>');
                            $li.find('a').append($div);

                            return $li.appendTo(ul);

                        };
                        $("#" + id + "" + n + "p").autocomplete("search", text);


                    },
                    error: function (err) {
                        alert(err.statusText);
                        console.log(err);
                    }
                });
            });
}

function sendFile(id) {


    // Checking whether FormData is available in browser
    if (window.FormData !== undefined) {

        var fileUpload = $("#files").get(0);
        var files = fileUpload.files;
        if (imgNumArray.indexOf(id) === -1) { //check if already uploaded

            // Create FormData object
            var fileData = new FormData();

            // Looping over all files and add it to FormData object
            //for (var i = 0; i < files.length; i++) {
            fileData.append(files[id].name, files[id]);
            //}
            var originalHeight = $("#" + id).get(0).naturalHeight;
            var originalWidth = $("#" + id).get(0).naturalWidth;
            var displayedWidth = parseInt($("#" + id).css("width"), 10);
            var displayedHeight = parseInt($("#" + id).css("height"), 10);
            var scaleY = displayedHeight / originalHeight;
            var scaleX = displayedWidth / originalWidth;

            var boxesList = [];
            $('#' + id + 's')
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
                        if (originalHeight < 500) {
                            obj.height += 30;
                        }
                        if (obj.name !== $('#' + boxId + "p").val()) {
                            obj.name = $('#' + boxId + "p").val();
                            obj.id = undefined;
                        }

                        boxesList.push(obj);
                    }
                });
            boxesList = JSON.stringify(boxesList);
            fileData.append('tags', boxesList);
            fileData.append("caption", $("#" + id + "ta").val());

            $.ajax({
                url: '/Admin/Home/UploadFiles',
                type: "POST",
                contentType: false, // Not to set any content header
                processData: false, // Not to process data
                data: fileData,
                xhr: function () { // Custom XMLHttpRequest
                    var myXhr = $.ajaxSettings.xhr();
                    if (myXhr.upload) { // Check if upload property exists

                        // For handling the progress of the upload
                        myXhr.upload.id = id;
                        myXhr.upload.addEventListener('progress', progressHandlingFunction, false);

                    }
                    return myXhr;
                },
                success: function (result) {
                    imgNumArray.push(id);
                    $('#' + id + "t").text(result);

                },
                error: function (err) {
                    $('#' + id + "t").text(err.statusText);
                }
            });
        } else {
            $('#' + id + "t").text("Filen er allerede opplastet");
        }
    } else {
        alert("FormData er ikke støttet.");
    }
}

function progressHandlingFunction(e) {
    var id = e.target.id;
    if (e.lengthComputable) {
        var percentage = Math.floor((e.loaded / e.total) * 100);
        //update progressbar percent complete
        $('#' + id + "t").text(percentage + '%');
        $('#' + id + "p").css("width", percentage + '%');
    }
}

$("#buttonAll")
    .on('click',
        function () {
            var fileUpload = $("#files").get(0);
            var files = fileUpload.files;

            for (var i = 0; i < files.length; i++) {

                if (files[i].type.match('image.*')) {
                    sendFile(i);
                } else {
                    alert("Du kan kun laste opp bilder");
                }

            }
        });

function func(item) {
    item.parentNode.parentNode.removeChild(item.parentNode);
}
