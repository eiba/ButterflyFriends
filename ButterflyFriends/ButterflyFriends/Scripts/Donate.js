var anon = false;
var user = false;
var amount = 0;
var name = "";
var streetadress = "";
var phone = "";
var city = "";
var postcode = "";
var birthnumber = "";
var email = "";
var cardnumber = 0;
var expiration = "";
var cvc = 0;
var token = "";
var description = "";

var subAnon = false;
var subUser = false;
var subId = 0;
var subName = "";
var subStreetadress = "";
var subPhone = "";
var subCity = "";
var subPostcode = "";
var subBirthnumber = "";
var subEmail = "";
var subCardnumber = 0;
var subExpiration = "";
var subCvc = 0;
var subToken = "";
var subDescription = "";

$(function () {
    $("#OnceBirthNumber").tooltip();
});
$(function () {
    $("SubBirthNumber").tooltip();
});

function checkAnonymous() {

    if ($('#OnceAnonymous').is(":checked")) {

        $('#onceContact').css('display', "none");
        $("#OnceUser").prop("checked", false);

    } else {
        $('#onceContact').css('display', "");

    }
}

function checkSubAnonymous() {

    if ($('#SubAnonymous').is(":checked")) {
        $('#SubContact').css('display', "none");
        $("#SubUser").prop("checked", false);

    } else {
        $('#SubContact').css('display', "");

    }
}

function checkUser() {
    if ($('#OnceUser').is(":checked")) {

        $('#onceContact').css('display', "none");
        $("#OnceAnonymous").prop("checked", false);

    } else {
        $('#onceContact').css('display', "");

    }
}

function checkSubUser() {
    if ($('#SubUser').is(":checked")) {
        $('#SubContact').css('display', "none");
        $("#SubAnonymous").prop("checked", false);

    } else {
        $('#SubContact').css('display', "");

    }
}

$("#OnceAmountNext")
    .click(function () {
        if (parseInt($("#AmountOnce").val()) > 0) {
            amount = parseInt($("#AmountOnce").val());
            $('.tabOnce > .active').next('li').find('a').trigger('click');
            $("#OnceAmountError").text("");

        } else {
            $("#OnceAmountError").text("Ugyldig donasjonsmengde");
        }
    });

$("#OnceContactNext")
    .click(function () { //get contact info and validate answers
        if ($('#OnceAnonymous').is(":checked")) {
            anon = true;
            user = false;
            description = $("#OnceDescription").val();
            $('.tabOnce > .active').next('li').find('a').trigger('click');
            $("#OnceContactError").text("");

        } else if ($('#OnceUser').is(":checked")) {
            anon = false;
            user = true;
            description = $("#OnceDescription").val();
            $('.tabOnce > .active').next('li').find('a').trigger('click');
            $("#OnceContactError").text("");
        } else if ($("#OnceEmail").val() !== "" ||
            $("#OnceCity").val() !== "" ||
            $("#OnceName").val() !== "" ||
            $("#OncePhone").val() !== "" ||
            $("#OnceStreet").val() !== "" ||
            $("#OncePostCode").val() !== "" ||
            $("#OnceBirthNumber").val() !== "") {
            var error = "";
            var regTest = /[^\s@@]+@@[^\s@@]+\.[^\s@@]+/; //check for valid email
            if ($("#OnceEmail").val() !== "" && !regTest.test($("#OnceEmail").val())) {
                error += "Ugyldig email \n";

            }
            if ($("#OnceBirthNumber").val() !== "" &&
                (isNaN($("#OnceBirthNumber").val()) || $("#OnceBirthNumber").val().length < 11)) {
                error += "Ugyldig fødselsnummer \n";

            }
            if ($("#OncePostCode").val() !== "" &&
                (isNaN($("#OncePostCode").val()) || $("#OncePostCode").val().length < 4)) {
                error += "Ugyldig postnummber \n";

            }
            if (error !== "") {
                $("#OnceContactError").text(error);
                return;
            } else {
                anon = false;
                email = $("#OnceEmail").val();
                city = $("#OnceCity").val();
                name = $("#OnceName").val();
                phone = $("#OncePhone").val();
                streetadress = $("#OnceStreet").val();
                postcode = $("#OncePostCode").val();
                birthnumber = $("#OnceBirthNumber").val();

                $('.tabOnce > .active').next('li').find('a').trigger('click');
                $("#OnceContactError").text("");
            }
        } else {
            $("#OnceContactError")
                .text("Du må skrive inn kontaktinformasjon, velge å være anonym eller bruke nettsidebrukeren");
        }
    });

$("#SubContactNext")
    .click(function () {
        if ($('#SubAnonymous').is(":checked")) {
            subAnon = true;
            subUser = false;
            subDescription = $("#SubDescription").val();
            $('.tabSub > .active').next('li').find('a').trigger('click');
            $("#SubContactError").text("");

        } else if ($('#SubUser').is(":checked")) {
            subAnon = false;
            subUser = true;
            description = $("#SubDescription").val();
            $('.tabSub > .active').next('li').find('a').trigger('click');
            $("#SubContactError").text("");
        } else if ($("#SubEmail").val() !== "" ||
            $("#SubCity").val() !== "" ||
            $("#SubName").val() !== "" ||
            $("#SubPhone").val() !== "" ||
            $("#SubStreet").val() !== "" ||
            $("#SubPostCode").val() !== "" ||
            $("#SubBirthNumber").val() !== "") {
            var error = "";
            var regTest = /[^\s@@]+@@[^\s@@]+\.[^\s@@]+/;
            if ($("#SubEmail").val() !== "" && !regTest.test($("#SubEmail").val())) {
                error += "Ugyldig email \n";

            }
            if ($("#SubBirthNumber").val() !== "" &&
                (isNaN($("#SubBirthNumber").val()) || $("#SubBirthNumber").val().length < 11)) {
                error += "Ugyldig fødselsnummer \n";

            }
            if ($("#SubPostCode").val() !== "" &&
                    (isNaN($("#SubPostCode").val()) || $("#SubPostCode").val().length < 4)
            ) {
                error += "Ugyldig postnummber \n";

            }
            if (error !== "") {
                $("#SubContactError").text(error);
                return;
            } else {
                subAnon = false;
                subEmail = $("#SubEmail").val();
                subCity = $("#SubCity").val();
                subName = $("#SubName").val();
                subPhone = $("#SubPhone").val();
                subStreetadress = $("#SubStreet").val();
                subPostcode = $("#SubPostCode").val();
                subBirthnumber = $("#SubBirthNumber").val();

                $('.tabSub > .active').next('li').find('a').trigger('click');
                $("#SubContactError").text("");
            }
        } else {
            $("#SubContactError")
                .text("Du må skrive inn kontaktinformasjon, velge å være anonym eller bruke nettsidebrukeren");
        }

    });
$("#SubAmountNext")
    .click(function () {
        if (subId !== 0) {
            $('.tabSub > .active').next('li').find('a').trigger('click');
        } else {
            $("#SubAmountError").text("Du må velge en plan");
        }

    });

function subAmount(id) {
    subId = id;
}

function PrevTab() {
    $('.tabOnce > .active').prev('li').find('a').trigger('click');
}

function PrevTabSub() {
    $('.tabSub > .active').prev('li').find('a').trigger('click');
}

function OnceDonate() {
    var error = "";
    if (!(Stripe.card.validateCardNumber($("#OnceCardNmber").val()))) {

        error += "Ugyldig kortnummer \n";
    }
    if (!(Stripe.card.validateCVC($("#OnceCvc").val()))) {
        error += "Ugyldig CVC \n";
    }
    var expParts = $("#OnceExpiration").val().split('/');
    if (!(Stripe.card.validateExpiry(parseInt(expParts[0]), parseInt('20' + expParts[1])))) {
        error += "Ugyldig utløpsdato";

    }
    if (stripePublic === "") {
        error += "Stripe er ikke konfigurert for nettsiden";
    }

    if (error !== "") {
        $("#OncePaymentError").text(error);
        return;
    } else {
        $("#OncePaymentError").text("");

        cardnumber = $("#OnceCardNmber").val();
        cvc = $("#OnceCvc").val();
        expiration = $("#OnceExpiration").val();

        var paymentInfo = {
            number: cardnumber,
            exp_month: parseInt(expParts[0]),
            exp_year: parseInt('20' + expParts[1]),
            cvc: cvc
        };
        if (name !== "") {
            paymentInfo['name'] = name;

        }
        if (streetadress !== "") {
            paymentInfo['address_line1'] = streetadress;

        }
        if (city !== "") {
            paymentInfo['address_city'] = city;

        }
        if (postcode !== "") {
            paymentInfo['address_zip'] = postcode;

        }
        $(".loader").show(); //show loader as we're working
        Stripe.card.createToken(paymentInfo,
            function (status, response) {
                if (response.error) {
                    $("#OncePaymentError").text(response.error.message);
                    $(".loader").hide(); //failed hide error
                    return;
                } else {
                    $("#OncePaymentError").text("");
                    token = response.id;
                    processPayment();
                }

            });
    }

}

function SubDonate() {
    var error = "";
    if (!(Stripe.card.validateCardNumber($("#SubCardNmber").val()))) { //handle card validation 

        error += "Ugyldig kortnummer \n";
    }
    if (!(Stripe.card.validateCVC($("#SubCvc").val()))) {
        error += "Ugyldig CVC \n";
    }
    var expParts = $("#SubExpiration").val().split('/');
    if (!(Stripe.card.validateExpiry(parseInt(expParts[0]), parseInt('20' + expParts[1])))) {
        error += "Ugyldig utløpsdato";

    }
    if (stripePublic === "") {
        error += "Stripe er ikke konfigurert for nettsiden";
    }

    if (error !== "") {
        $("#SubPaymentError").text(error);
        return;
    } else {
        $("#SubPaymentError").text("");

        subCardnumber = $("#SubCardNmber").val();
        subCvc = $("#SubCvc").val();
        subExpiration = $("#SubExpiration").val();

        var paymentInfo = {
            number: subCardnumber,
            exp_month: parseInt(expParts[0]),
            exp_year: parseInt('20' + expParts[1]),
            cvc: subCvc
        };
        if (subName !== "") {
            paymentInfo['name'] = subName;

        }
        if (subStreetadress !== "") {
            paymentInfo['address_line1'] = subStreetadress;

        }
        if (subCity !== "") {
            paymentInfo['address_city'] = subCity;

        }
        if (subPostcode !== "") {
            paymentInfo['address_zip'] = subPostcode;

        }
        $(".loader").show(); //show loader as we're working
        Stripe.card.createToken(paymentInfo,
            function (status, response) { //get token from stripe
                if (response.error) {
                    $("#SubPaymentError").text(response.error.message);
                    $(".loader").hide(); //failed hide error
                    return;
                } else {
                    $("#SubPaymentError").text("");
                    token = response.id;
                    processSubPayment();
                }

            });
    }

}

function processPayment() { //process one time payment
    var formData = new FormData();
    formData.append("token", token);
    formData.append("type", "once");
    formData.append("amount", amount);
    formData.append("anon", anon);
    formData.append("description", description);
    formData.append("user", user);

    if (!anon) {
        formData.append("email", email);
        formData.append("phone", phone);
        formData.append("city", city);
        formData.append("streetadress", streetadress);
        formData.append("postcode", postcode);
        formData.append("birthnumber", birthnumber);
        formData.append("name", name);
    }

    $.ajax({
        url: '/Home/HandlePayment',
        type: "POST",
        contentType: false, // Not to set any content header
        processData: false, // Not to process data
        data: formData,
        success: function (result) {
            if (result.Success == "false" && result.striperesponse == "true") {
                $("#OncePaymentError").text(JSON.parse(result.Error).error.message);

            } else if (result.Success == "false" && result.striperesponse == "false") {
                $("#OncePaymentError").text(result.Error);
            } else {
                $("#Donations").html(result);
            }

        },
        error: function (err) {
            console.log("error " + err.Error);
        }
    });
}

function processSubPayment() { //process subscription payment
    var formData = new FormData();
    formData.append("token", token);
    formData.append("subId", subId);
    formData.append("anon", subAnon);
    formData.append("description", subDescription);
    formData.append("user", subUser);

    if (!anon) {
        formData.append("email", subEmail);
        formData.append("phone", subPhone);
        formData.append("birthnumber", subBirthnumber);
        formData.append("name", subName);
    }

    $.ajax({
        url: '/Home/HandleSubPayment',
        type: "POST",
        contentType: false, // Not to set any content header
        processData: false, // Not to process data
        data: formData,
        success: function (result) {
            if (result.Success == "false" && result.striperesponse == "true") {
                $("#SubPaymentError").text(JSON.parse(result.Error).error.message);

            } else if (result.Success == "false" && result.striperesponse == "false") {
                $("#SubPaymentError").text(result.Error);
            } else {
                $("#Donations").html(result);
            }

        },
        error: function (err) {
            console.log("error " + err.Error);
        }
    });
}