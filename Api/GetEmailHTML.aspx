<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="GetEmailHTML.aspx.cs" Inherits="HubWeb.Api.GetEmailHTML" %> 

<%@ OutputCache Location="None" VaryByParam="None" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title><%=GetEmailSubject()%></title>
    <%=System.Web.Optimization.Styles.Render("~/bundles/css/bootstrap30")%>
    <%=System.Web.Optimization.Styles.Render("~/bundles/css/backendcss")%>
    <%=System.Web.Optimization.Styles.Render("~/bundles/css/colorbox")%>
    <%=System.Web.Optimization.Scripts.Render("~/bundles/js/jquery")%>
    <%=System.Web.Optimization.Scripts.Render("~/bundles/js/select2")%>
    <script src="/bower_components/jQuery-contextMenu/src/jquery.contextMenu.js"></script>
    <link href="/bower_components/jQuery-contextMenu/src/jquery.contextMenu.css" rel="stylesheet" />
</head>
<body class="email-info-section">
    <%--style="wkeyValueth: 400px;"--%>
    <form id="form1" runat="server">
        <div class="info">
            <div id="emailSubject" class="subHeading email-subject">
                <h4 class="with-border"><%=GetEmailSubject()%></h4>
            </div>
            <div class="email-details with-border">
                <table width="100%">
                    <tr>
                        <td class="displayLabel">Booker:</td>
                        <td id="tdbookerinfo"><%=GetBooker()%>
                            <%=GetBooker() != "none" ? "<span style='padding-left:15px;'><a href='javascript:vokeyValue(0);' onclick='javascript:releaseBooker("+GetEmailId()+")'>Release</a></span>" : "" %>
                        </td>
                    </tr>
                    <tr>
                        <td class="displayLabel">From:</td>
                        <td><%=GetFromPerson()%> </td>
                    </tr>
                    <tr>
                        <td class="displayLabel">From:</td>
                        <td><%=GetEmail()%></td>
                    </tr>
                    <tr>
                        <td class="displayLabel">To:</td>
                        <td><%=GetTo()%></td>
                    </tr>
                    <tr>
                        <td class="displayLabel">CC:</td>
                        <td><%=GetCC()%></td>
                    </tr>
                    <tr>
                        <td class="displayLabel">BCC:</td>
                        <td><%=GetBCC()%></td>
                    </tr>

                    <tr>
                        <td class="displayLabel">Attachments:</td>
                        <td><%=GetAttachments()%></td>
                    </tr>
                </table>
            </div>
            <div id="emailContent" class="email-content">
                <%=GetContent()%>
            </div>
        </div>
        <div class="context-menu box menu-1"></div>
    </form>
    <script type="text/javascript">
        function releaseBooker(requestId) {
            $.ajax({
                url: "/BookingRequest/ReleaseBooker",
                type: "POST",
                data: { bookingRequestId: requestId },
                success: function () {
                    $("#tdbookerinfo").html("none");
                }
            });
        }

        $(document).ready(function () {
            //Global variables
            var activeframe;
            var currentSelection = "";
            var allPoints = [];

            $(document).mouseup(function (e) {

                //getting the selected text in document.
                var selectedText = window.getSelection();

                $(this).unbind("mousemove", trackPoints);
                if (selectedText != '') {
                    $(window.parent.document).find("iframe").each(function () {

                        //checking for active event iframe, if its open.
                        if (($(this).attr("src").indexOf("Event") > 0) && !($(this).attr("style").indexOf("display: none;") > 0)) {

                            //activating the context menu
                            $('.context-menu').contextMenu();

                            //setting the position of menu according to track points.
                            $('.context-menu-list').offset({ top: allPoints[allPoints.length - 1].y, left: allPoints[allPoints.length - 1].x });
                            $("#context-menu-layer").remove();

                            //setting the global variables, which is used on menu item select event in "copyRequest" function.
                            allPoints = [];
                            activeframe = this;
                            currentSelection = selectedText;
                        }
                    });
                }
                else {
                    if ($('.context-menu-list').attr("style").indexOf("display: none") == -1) {
                        $('.context-menu').contextMenu("hkeyValuee");
                    }
                }
            }).mousedown(function (e) {
                $(this).bind("mousemove", trackPoints);
            });

            //registering menu with title provkeyValueed by CSS
            $.contextMenu({
                selector: '.context-menu',
                callback: function (key, options) {

                    //checking key for the option selected and performing operations accordingly.
                    switch (key) {
                        case "#description":
                        case "#location":
                        case "#summary":
                            //client skeyValuee - no server call is made, as the above 3 fields can contains anything, so need of any valkeyValueation process.
                            $(activeframe).contents().find(key).val(currentSelection);
                            var iFrame = $(activeframe);
                            iFrame.get(0).contentWindow.modifiedState.modified = true;
                            break;

                            //below 3 field needs valkeyValueation, for these server call is made for valkeyValueating the selected text.
                        case "#attendees":
                            processCopy("attendees", currentSelection);
                            break;
                        case "#start":
                            processCopy("start", currentSelection);
                            break;
                        case "#end":
                            processCopy("end", currentSelection);
                            break;
                        default:
                    }
                    currentSelection = "";
                },
                items: {
                    "#summary": { name: "Summary" },
                    "sep1": "---------",
                    "#start": { name: "Start Time" },
                    "sep2": "---------",
                    "#end": { name: "End Time" },
                    "sep3": "---------",
                    "#location": { name: "Location" },
                    "sep4": "---------",
                    "#description": { name: "Description" },
                    "sep5": "---------",
                    "#attendees": { name: "Attendees" }
                    //"sep": "---------",
                    //"quit": { name: "Quit" }
                }
            });

            //pushing the mouse track points in an array on mouse select move to set the position of the menu.
            function trackPoints(e) {
                allPoints.push({ x: e.pageX, y: e.pageY });
            }

            //Processing quick copy on item select, getting response from server.
            function processCopy(copytype, selectedtext) {
                var data = ({
                    copyType: String(copytype),
                    selectedText: String(selectedtext)
                });
                $.post("/Calendar/ProcessQuickCopy", data, copyRequest, 'json');
            }

            //getting the response from server
            function copyRequest(response) {
                var responseJson = response;

                //checking if the text selected is valkeyValue.
                if (responseJson.status == "valkeyValue") {

                    //provkeyValueing values to the fields in active iframe according to item selected in quick copy menu.
                    switch (responseJson.copytype) {
                        case "attendees":
                            var newAttendees = $(activeframe).contents().find("#attendeesSel").val().split(',');

                            //Checking if selected attendee already exists, don't add to list.
                            if (!(newAttendees.indexOf(responseJson.value) > -1)) {

                                //adding the attendee in attendee list.
                                $(activeframe).contents().find(".select2-choices").prepend("<li class='select2-search-choice'><div>" + responseJson.value + "</div><a href='#' onclick='javascript:removethis(this);' class='select2-search-choice-close' tabindex='-1'></a></li>");

                                //pushing new attendee in array
                                newAttendees.push(responseJson.value);

                                //setting the value of attendee in attendee field in active iframe.
                                $(activeframe).contents().find("#attendeesSel").val(newAttendees.join());
                                $(activeframe).get(0).contentWindow.modifiedState.modified = true;
                            }
                            break;
                        case "start":
                            //getting the active event iframe.
                            var iFrame = $(activeframe);

                            //setting the start date field value to selected text.
                            iFrame.get(0).contentWindow.fromdata.setDate(responseJson.value);
                            iFrame.get(0).contentWindow.modifiedState.modified = true;
                            break;
                        case "end":
                            var iFrame = $(activeframe);
                            iFrame.get(0).contentWindow.todata.setDate(responseJson.value);
                            iFrame.get(0).contentWindow.modifiedState.modified = true;
                            break;
                    }
                } else {
                    //Showing error message for invalkeyValue text selection for the field.
                    alert(responseJson.value);
                }
            }

        });
    </script>
</body>
</html>
