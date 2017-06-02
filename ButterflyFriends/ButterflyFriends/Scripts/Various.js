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


function showSendGrid() {
    sendGridVue.showModal = true;
}

function showRecaptcha() {
    recaptchaVue.showModal = true;
}

function showAbout() {
    aboutVue.showModal = true;
}

function showStripe() {
    stripeVue.showModal = true;
}

function showFacebook() {
    facebookVue.showModal = true;
}

function showDisqus() {
    disqusVue.showModal = true;
}

function showTwitter() {
    twitterVue.showModal = true;
}

document.getElementById('file').addEventListener('change', handleFileSelect, false);
document.getElementById('carouselFile').addEventListener('change', handleCarouselFileSelect, false);
document.getElementById('backgroundFile').addEventListener('change', handleBackgroundFileSelect, false);


function handleFileSelect(evt) {
    var files = evt.target.files; // FileList object
    $('#bar').text('0%');
    $('#bar').css("width", '0%');
    $("#prog").hide();
    $("#TermsSuccess").text("");
    for (var i = 0, f; f = files[i]; i++) {
        if (!f.type.match('pdf.*')) {
            $("#TermsError").text("Filen må være et pdf dokument");
            document.getElementById("file").value = "";
            continue;
        }
        $("#TermsError").text("");


    }
}

function handleCarouselFileSelect(evt) {
    var files = evt.target.files; // FileList object
    $('#carouselBar').text('0%');
    $('#carouselBar').css("width", '0%');
    $("#carouselProg").hide();
    $("#CarouselSuccess").text("");
    for (var i = 0, f; f = files[i]; i++) {
        if (!f.type.match('image.*') && !f.type.match('video.*')) {
            $("#CarouselError").text("Filene må være bilder eller videoer");
            continue;
        }
        $("#CarouselError").text("");


    }
}

function handleBackgroundFileSelect(evt) {
    var files = evt.target.files; // FileList object
    $('#backgroundBar').text('0%');
    $('#backgroundBar').css("width", '0%');
    $("#backgroundProg").hide();
    $("#BackgroundSuccess").text("");
    for (var i = 0, f; f = files[i]; i++) {
        if (!f.type.match('image.*')) {
            $("#BackgroundError").text("Filen må være et bilde");
            continue;
        }
        $("#BackgroundError").text("");


    }
}

function uploadPDF() {
    var formData = new FormData();

    var fileUpload = $("#file").get(0);
    var file = fileUpload.files;
    if (file.length === 0) {
        $("#TermsError").text("Du må velge en fil først");
        return;
    }

    for (var i = 0, f; f = file[i]; i++) {
        if (!f.type.match('pdf.*')) {
            $("#TermsError").text("Filen må være et pdf dokument");
            document.getElementById("file").value = "";
            return;
        }
        $("#TermsError").text("");
        $("#TermsSuccess").text("");
    }
    $("#prog").show();
    formData.append(file[0].name, file[0]);

    $.ajax({
        url: '/Admin/Various/TermsUpload',
        async: true,
        contentType: false,
        processData: false,
        type: "POST",
        data: formData,
        xhr: function () { // Custom XMLHttpRequest
            var myXhr = $.ajaxSettings.xhr();
            if (myXhr.upload) { // Check if upload property exists

                // For handling the progress of the upload
                myXhr.upload.addEventListener('progress', progressHandlingFunction, false);

            }
            return myXhr;
        },
        success: function (result) {
            $('#TermsOfUserUpdate').html(result);
            document.getElementById('file').addEventListener('change', handleFileSelect, false);

        },
        error: function (err) {
            $("#TermsError").text(err.statusText);
            $("#TermsSuccess").text("");
        }
    });
}

function uploadCarousel() {
    var formData = new FormData();

    var fileUpload = $("#carouselFile").get(0);
    var file = fileUpload.files;
    if (file.length === 0) {
        $("#CarouselError").text("Du må velge en fil først");
        return;
    }

    for (var i = 0, f; f = file[i]; i++) {
        if (!f.type.match('image.*') && !f.type.match('video.*')) {
            $("#CarouselError").text("Filene må være bilder og videoer");
            document.getElementById("carouselFile").value = "";
            return;
        }
        $("#CarouselError").text("");
        $("#CarouselSuccess").text("");
        formData.append(file[i].name, file[i]);
        console.log(file[i]);

    }
    $("#carouselProg").show();

    $.ajax({
        url: '/Admin/Various/CarouselUpload',
        async: true,
        contentType: false,
        processData: false,
        type: "POST",
        data: formData,
        xhr: function () { // Custom XMLHttpRequest
            var myXhr = $.ajaxSettings.xhr();
            if (myXhr.upload) { // Check if upload property exists

                // For handling the progress of the upload
                myXhr.upload.addEventListener('progress', carouselProgressHandlingFunction, false);

            }
            return myXhr;
        },
        success: function (result) {
            $('#CarouselUpdate').html(result);
            document.getElementById('carouselFile').addEventListener('progress', handleFileSelect, false);

        },
        error: function (err) {
            $("#CarouselError").text(err.statusText);
            $("#CarouselSuccess").text("");
        }
    });
}

function uploadBackground() {
    var formData = new FormData();

    var fileUpload = $("#backgroundFile").get(0); //get file from file element
    var file = fileUpload.files;
    if (file.length === 0) {
        $("#BackgroundError").text("Du må velge en fil først");
        return;
    }

    for (var i = 0, f; f = file[i]; i++) {
        if (!f.type.match('image.*')) {
            $("#BackgroundError").text("Filen må være et bilde!");
            document.getElementById("backgroundFile").value = "";
            return;
        }
        $("#backgroundError").text("");
        $("#backgroundSuccess").text("");
        formData.append(file[i].name, file[i]);
        console.log(file[i]);

    }
    $("#backgroundProg").show();

    $.ajax({
        url: '/Admin/Various/BackgroundUpload',
        async: true,
        contentType: false,
        processData: false,
        type: "POST",
        data: formData,
        xhr: function () { // Custom XMLHttpRequest
            var myXhr = $.ajaxSettings.xhr();
            if (myXhr.upload) { // Check if upload property exists

                // For handling the progress of the upload
                myXhr.upload.addEventListener('progress', backgroundProgressHandlingFunction, false);

            }
            return myXhr;
        },
        success: function (result) {
            $('#BackgroundUpdate').html(result);
            document.getElementById('carouselFile').addEventListener('progress', handleFileSelect, false);

        },
        error: function (err) {
            $("#BackgroundError").text(err.statusText);
            $("#BackgroundSuccess").text("");
        }
    });
}

function progressHandlingFunction(e) {
    if (e.lengthComputable) {
        var percentage = Math.floor((e.loaded / e.total) * 100);
        //update progressbar percent complete
        $('#bar').text(percentage + '%');
        $('#bar').css("width", percentage + '%');
    }
}


function carouselProgressHandlingFunction(e) {
    if (e.lengthComputable) {
        var percentage = Math.floor((e.loaded / e.total) * 100);
        //update progressbar percent complete
        $('#carouselBar').text(percentage + '%');
        $('#carouselBar').css("width", percentage + '%');
    }
}

function backgroundProgressHandlingFunction(e) {
    if (e.lengthComputable) {
        var percentage = Math.floor((e.loaded / e.total) * 100);
        //update progressbar percent complete
        $('#backgroundBar').text(percentage + '%');
        $('#backgroundBar').css("width", percentage + '%');
    }
}

function enableCarousel(enable) {
    var formData = new FormData();
    formData.append('enable', enable);
    $.ajax({
        url: '/Admin/Various/EnableCarousel',
        async: true,
        contentType: false,
        processData: false,
        type: "POST",
        data: formData,
        success: function (result) {
            $('#CarouselUpdate').html(result);
        },
        error: function (err) {
            $("#CarouselError").text(err.statusText);
            $("#CarouselSuccess").text("");
        }
    });
}

function enableBackground(enable) {
    var formData = new FormData();
    formData.append('enable', enable);
    $.ajax({
        url: '/Admin/Various/EnableBackground',
        async: true,
        contentType: false,
        processData: false,
        type: "POST",
        data: formData,
        success: function (result) {
            $('#BackgroundUpdate').html(result);
        },
        error: function (err) {
            $("#BackgroundError").text(err.statusText);
            $("#BackgroundSuccess").text("");
        }
    });
}

function enableTwitter(enable) {
    var formData = new FormData();
    formData.append('enable', enable);
    $.ajax({
        url: '/Admin/Various/EnableTwitter',
        async: true,
        contentType: false,
        processData: false,
        type: "POST",
        data: formData,
        success: function (result) {
            $('#updateTwitter').html(result);
        },
        error: function (err) {
            $("#TwitterError").text(err.statusText);
            $("#TwitterSuccess").text("");
        }
    });
}

function enableFacebook(enable) {
    var formData = new FormData();
    formData.append('enable', enable);
    $.ajax({
        url: '/Admin/Various/EnableFacebook',
        async: true,
        contentType: false,
        processData: false,
        type: "POST",
        data: formData,
        success: function (result) {
            $('#updateFacebook').html(result);
        },
        error: function (err) {
            $("#FacebookError").text(err.statusText);
            $("#FacebookSuccess").text("");
        }
    });
}

function enableDisqus(enable) {
    var formData = new FormData();
    formData.append('enable', enable);
    $.ajax({
        url: '/Admin/Various/EnableDisqus',
        async: true,
        contentType: false,
        processData: false,
        type: "POST",
        data: formData,
        success: function (result) {
            $('#updateDisqus').html(result);
        },
        error: function (err) {
            $("#DisqusError").text(err.statusText);
            $("#DisqusSuccess").text("");
        }
    });
}

function enableTerms(enable) {
    var formData = new FormData();
    formData.append('enable', enable);
    $.ajax({
        url: '/Admin/Various/EnableTerms',
        async: true,
        contentType: false,
        processData: false,
        type: "POST",
        data: formData,
        success: function (result) {
            $('#TermsOfUserUpdate').html(result);
        },
        error: function (err) {
            $("#TermsError").text(err.statusText);
            $("#TermsSuccess").text("");
        }
    });
}

function enableStripe(enable) {
    var formData = new FormData();
    formData.append('enable', enable);
    $.ajax({
        url: '/Admin/Various/EnableStripe',
        async: true,
        contentType: false,
        processData: false,
        type: "POST",
        data: formData,
        success: function (result) {
            $('#updateStripe').html(result);
        },
        error: function (err) {
            $("#StripeError").text(err.statusText);
            $("#StripeSuccess").text("");
        }
    });
}

function enableSendgrid(enable) {
    var formData = new FormData();
    formData.append('enable', enable);
    $.ajax({
        url: '/Admin/Various/EnableSendgrid',
        async: true,
        contentType: false,
        processData: false,
        type: "POST",
        data: formData,
        success: function (result) {
            $('#upadteSendGrid').html(result);
        },
        error: function (err) {
            $("#SendgridError").text(err.statusText);
            $("#SendgridSuccess").text("");
        }
    });
}

function enableRecaptcha(enable) {
    var formData = new FormData();
    formData.append('enable', enable);
    $.ajax({
        url: '/Admin/Various/EnableRecaptcha',
        async: true,
        contentType: false,
        processData: false,
        type: "POST",
        data: formData,
        success: function (result) {
            $('#updateRecaptcha').html(result);
        },
        error: function (err) {
            $("#RecaptchaError").text(err.statusText);
            $("#RecaptchaSuccess").text("");
        }
    });
}