 $(document)
        .ready(function() {

            $(".loader").hide();
            $(document)
                .ajaxStart(function() {
                    $(".loader").show();
                })
                .ajaxStop(function() {
                    $(".loader").hide();
                });

            $("#accordion")
                .accordion({
                    collapsible: true,
                    active: false,
                    heightStyle: "content"
                });
        });


    function acceptRequest(id) {


        var formdata = new FormData();
        formdata.append('requestid', $("#" + id + "-requestid").val());
        formdata.append('message', $("#" + id + "-ta").val());
        formdata.append('page', $("#page").val());

        $.ajax({
            url: '/Admin/MemberRequests/RequestAccept',
            type: "POST",
            contentType: false, // Not to set any content header
            processData: false, // Not to process data
            data: formdata,
            success: function(result) {
                $("#updateAccordion").html(result);

                //$("#accordion").accordion("destroy");

                $("#accordion")
                    .accordion({
                        collapsible: true,
                        active: false,
                        heightStyle: "content"
                    });
            },
            error: function(err) {
                $("#error").text("Error: " + err.statusText);
                console.log(err);
            }
        });
    }

    function declineRequest(id) {
        var formdata = new FormData();
        formdata.append('message', $("#" + id + "-ta").val());
        formdata.append('requestid', $("#" + id + "-requestid").val());
        formdata.append('page', $("#page").val());

        $.ajax({
            url: '/Admin/MemberRequests/RequestDecline',
            type: "POST",
            contentType: false, // Not to set any content header
            processData: false, // Not to process data
            data: formdata,
            success: function(result) {
                $("#updateAccordion").html(result);

                $("#accordion")
                    .accordion({
                        collapsible: true,
                        active: false,
                        heightStyle: "content"
                    });
            },
            error: function(err) {
                $("#error").text("Error: " + err.statusText);
                console.log(err);
            }
        });

    }

    $("#searchBtn")
        .on('click',
            function() {
                filter();
                return false;
            });
    $("#searchField")
        .on('keydown',
            function() {
                if (event.keyCode == 13) {
                    filter();
                    return false;
                }
            });

    function filter() {
        var formData = new FormData();

        formData.append('search', $('#searchField').val());

        $.ajax({
            url: '/Admin/MemberRequests/Filter',
            type: 'POST',
            async: true,
            contentType: false,
            processData: false,
            data: formData,
            success: function(result) {
                $('#updateAccordion').html(result);

                $("#accordion")
                    .accordion({
                        collapsible: true,
                        active: false,
                        heightStyle: "content"
                    });
            },
            error: function(err) {
                $("#error").text("Error: " + err.statusText);
            }
        });

    }

    $(document)
        .on("click",
            "#Pager a[href]",
            function() {
                var formData = new FormData();
                formData.append('search', $('#searchField').val());
                $.ajax({
                    url: $(this).attr("href"),
                    type: 'POST',
                    async: true,
                    contentType: false,
                    processData: false,
                    data: formData,
                    cache: false,
                    success: function(result) {
                        $('#updateAccordion').html(result);

                        $("#accordion")
                            .accordion({
                                collapsible: true,
                                active: false,
                                heightStyle: "content"
                            });
                    },
                    error: function(err) {
                        $("#error").text("Error: " + err.statusText);
                    }
                });
                return false;
            });
