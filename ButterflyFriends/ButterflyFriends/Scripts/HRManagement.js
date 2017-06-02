$(document)
    .ready(function () {
        $(".loader").hide();
        $(document)
            .ajaxStart(function () {
                $(".loader").show();
            })
            .ajaxStop(function () {
                $(".loader").hide();
            });

        $(document)
            .on("click",
                "#employeePager a[href]",
                function () {
                    var formData = new FormData();
                    formData.append('search', $('#EmployeesearchField').val());
                    formData.append('active', $('#employeeActive input:radio:checked').val());
                    formData.append('order', $('#EmployeeOrderFilter input:radio:checked').val());
                    formData.append('filter', $("#EmployeeSelected option:selected").val());
                    formData.append('employeePhone', $('#employeePhone').val());
                    formData.append('EmployeeStreetadress', $('#EmployeeStreetadress').val());
                    formData.append('EmployeeZip', $('#EmployeeZip').val());
                    formData.append('EmployeeCity', $('#EmployeeCity').val());
                    formData.append('EmployeeCounty', $('#EmployeeCounty').val());
                    formData.append('EmployeePosition', $('#EmployeePosition').val());
                    formData.append('employeeAccountNumber', $('#employeeAccountNumber').val());
                    $.ajax({
                        url: $(this).attr("href"),
                        type: 'POST',
                        async: true,
                        contentType: false,
                        processData: false,
                        data: formData,
                        cache: false,
                        success: function (result) {
                            $('#updateEmployees').html(result);
                        }
                    });
                    return false;
                });

        $(document)
            .on("click",
                "#childrenPager a[href]",
                function () {
                    var formData = new FormData();
                    formData.append('search', $('#ChildsearchField').val());
                    formData.append('active', $('#ChildActive input:radio:checked').val());
                    formData.append('order', $('#ChildOrderFilter input:radio:checked').val());
                    formData.append('filter', $("#ChildSelected option:selected").val());
                    formData.append('ChildDoB', $('#ChildDoB').val());
                    formData.append('ChildSponsor', $('#ChildSponsor').val());
                    $.ajax({
                        url: $(this).attr("href"),
                        type: 'POST',
                        async: true,
                        contentType: false,
                        processData: false,
                        data: formData,
                        cache: false,
                        success: function (result) {
                            $('#updateChildren').html(result);
                        }
                    });
                    return false;
                });

        $(document)
            .on("click",
                "#sponsorPager a[href]",
                function () {
                    var formData = new FormData();
                    formData.append('search', $('#SponsorsearchField').val());
                    formData.append('active', $('#SponsorActive input:radio:checked').val());
                    formData.append('order', $('#SponsorOrderFilter input:radio:checked').val());
                    formData.append('filter', $("#SponsorSelected option:selected").val());
                    formData.append('SponsorPhone', $('#SponsorPhone').val());
                    formData.append('SponsorStreetadress', $('#SponsorStreetadress').val());
                    formData.append('SponsorZip', $('#SponsorZip').val());
                    formData.append('SponsorCity', $('#SponsorCity').val());
                    formData.append('SponsorCounty', $('#SponsorCounty').val());

                    $.ajax({
                        url: $(this).attr("href"),
                        type: 'POST',
                        async: true,
                        contentType: false,
                        processData: false,
                        data: formData,
                        cache: false,
                        success: function (result) {
                            $('#updateSponsors').html(result);
                        }
                    });
                    return false;
                });
    });

//employees
function filterEmployees() {
    var formData = new FormData();
    formData.append('search', $('#EmployeesearchField').val());
    formData.append('active', $('#employeeActive input:radio:checked').val());
    formData.append('order', $('#EmployeeOrderFilter input:radio:checked').val());
    formData.append('filter', $("#EmployeeSelected option:selected").val());
    formData.append('employeePhone', $('#employeePhone').val());
    formData.append('EmployeeStreetadress', $('#EmployeeStreetadress').val());
    formData.append('EmployeeZip', $('#EmployeeZip').val());
    formData.append('EmployeeCity', $('#EmployeeCity').val());
    formData.append('EmployeeCounty', $('#EmployeeCounty').val());
    formData.append('EmployeePosition', $('#EmployeePosition').val());
    formData.append('employeeAccountNumber', $('#employeeAccountNumber').val());


    $.ajax({
        url: '/Admin/HRManagement/FilterEmployees',
        type: 'POST',
        async: true,
        contentType: false,
        processData: false,
        data: formData,
        success: function (result) {
            $('#updateEmployees').html(result);
        }
    });
}

$("#employeeReset")
    .on('click',
        function () {
            event.stopPropagation();
            $('#EmployeesearchField').val('');

            $('#employeePhone').val('');
            $('#EmployeeStreetadress').val('');
            $('#EmployeeZip').val('');
            $('#EmployeeCity').val('');
            $('#EmployeeCounty').val('');
            $('#EmployeePosition').val('');
            $('#employeeAccountNumber').val('');
            document.getElementById("EmployeeDefault").selected = "true";
            $("#EmployeeOrderDefault").prop("checked", true);
            $("#EmployeeDefaultRadio").prop("checked", true);
        });

$("#EmployeesearchField")
    .on('keydown',
        function () {
            if (event.keyCode == 13) {
                filterEmployees();
                return false;
            }
        });

//sponsors
function filterSponsors() {
    var formData = new FormData();
    formData.append('search', $('#SponsorsearchField').val());
    formData.append('active', $('#SponsorActive input:radio:checked').val());
    formData.append('order', $('#SponsorOrderFilter input:radio:checked').val());
    formData.append('filter', $("#SponsorSelected option:selected").val());
    formData.append('SponsorPhone', $('#SponsorPhone').val());
    formData.append('SponsorStreetadress', $('#SponsorStreetadress').val());
    formData.append('SponsorZip', $('#SponsorZip').val());
    formData.append('SponsorCity', $('#SponsorCity').val());
    formData.append('SponsorCounty', $('#SponsorCounty').val());


    $.ajax({
        url: '/Admin/HRManagement/FilterSponsors',
        type: 'POST',
        async: true,
        contentType: false,
        processData: false,
        data: formData,
        success: function (result) {
            $('#updateSponsors').html(result);
        }
    });
}

$("#SponsorReset")
    .on('click',
        function () {
            event.stopPropagation();
            $('#SponsorsearchField').val('');

            $('#SponsorPhone').val('');
            $('#SponsorStreetadress').val('');
            $('#SponsorZip').val('');
            $('#SponsorCity').val('');
            $('#SponsorCounty').val('');
            $('#SponsorPosition').val('');
            $('#SponsorAccountNumber').val('');
            document.getElementById("SponsorDefault").selected = "true";
            $("#SponsorOrderDefault").prop("checked", true);
            $("#SponsorDefaultRadio").prop("checked", true);
        });

$("#SponsorsearchField")
    .on('keydown',
        function () {
            if (event.keyCode == 13) {
                filterSponsors();
                return false;
            }
        });

//child 
$(function () {
    $("#ChildDoB")
        .datepicker({
            dateFormat: "dd.mm.yy"
        });
});

function filterChildren() {
    var formData = new FormData();
    formData.append('search', $('#ChildsearchField').val());
    formData.append('active', $('#ChildActive input:radio:checked').val());
    formData.append('order', $('#ChildOrderFilter input:radio:checked').val());
    formData.append('filter', $("#ChildSelected option:selected").val());

    formData.append('ChildDoB', $('#ChildDoB').val());
    formData.append('ChildSponsor', $('#ChildSponsor').val());


    $.ajax({
        url: '/Admin/HRManagement/FilterChildren',
        type: 'POST',
        async: true,
        contentType: false,
        processData: false,
        data: formData,
        success: function (result) {
            $('#updateChildren').html(result);
        }
    });
}

$("#ChildReset")
    .on('click',
        function () {
            event.stopPropagation();
            $('#ChildsearchField').val('');

            $('#ChildDoB').val('');
            $('#ChildSponsor').val('');

            document.getElementById("ChildDefault").selected = "true";
            $("#ChildOrderDefault").prop("checked", true);
            $("#ChildDefaultRadio").prop("checked", true);
        });

$("#ChildsearchField")
    .on('keydown',
        function () {
            if (event.keyCode == 13) {
                filterChildren();
                return false;
            }
        });