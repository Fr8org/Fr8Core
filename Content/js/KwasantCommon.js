
var CONTROLLER_NAME = 'Calendar';

var squelchErrors = [];

$(document).ajaxError(function (event, request, settings) {
    if (squelchErrors.indexOf(request) != -1)
        return;
    
    var text = "Error requesting page: " + settings.url;
    if (settings.data != undefined && settings.data != '')
        text += "?" + settings.data;
    text += ". Status: " + request.status + " " + request.statusText;

    $('#ajaxErrors').show();
    $("#ajaxErrors").append("<li>" + text + "</li>");
});

function getConfiguration() {

    var configFilePath = getBaseCalendarURL();

    var retValue = '';

    $.get(configFilePath,
        function (txt, status, jqXHR) {
            $("#txtConfigValue").val(txt);
        },
        "text"
    );

    return retValue;
}

function getURL(key) {
    var configValue = $("#txtConfigValue").val();

    var retValue = '';
    var lines = configValue.split("\n");

    for (var i = 0, len = lines.length; i < len; i++) {
        var arrLine = lines[i].split('=');
        if ($.trim(arrLine[0]) == $.trim(key)) {
            retValue = arrLine[1];
            break;
        }
    }

    return retValue;
}

var guid = (function () {
    function s4() {
        return Math.floor((1 + Math.random()) * 0x10000)
                   .toString(16)
                   .substring(1);
    }
    return function () {
        return s4() + s4() + '-' + s4() + '-' + s4() + '-' +
               s4() + '-' + s4() + s4() + s4();
    };
})();

function isEmail(email) {
    var regex = /^[a-zA-Z0-9.!#$%&'*+\/=?^_`{|}~-]+@[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?(?:\.[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?)*$/;
    return regex.test(email);
}

function getURLParameter(name) {
    return decodeURIComponent((new RegExp('[?|&]' + name + '=' + '([^&;]+?)(&|#|;|$)').exec(location.search) || [, ""])[1].replace(/\+/g, '%20')) || null;
}

function close(saved) {
    if (saved === undefined || saved == null)
        saved = false;
    Kwasant.IFrame.CloseMe(saved);
}

/*** Check Email Validation ***/
function getValidEmailAddress(attendeesSel) {
    $(attendeesSel).select2({
        createSearchChoice: function (term) {
            return { id: term, text: term };
        },
        validateFormat: function (term) {
            if (isValidEmail(term)) {
                return null;
            }
            return 'Invalid Email';
        },
        multiple: true,
        data: [],
        width: '100%',
    });
}

function isValidEmail(term) {
    var atIndex = term.indexOf('@');
    var dotIndex = term.lastIndexOf('.');
    if (atIndex > 0 //We need something before the at sign
        && dotIndex > atIndex + 1 //We need a dot, and it should have at least one character between the at and the dot
        && dotIndex < term.length - 1 //The dot can't be at the end
    )
        return true;
    return false;
}

function convertToDateString(date) {
    var hour, meridiem;
    var datevalue = new Date(date);
    hour = datevalue.getHours();
    if (hour < 12)
    { meridiem = "AM"; }

    else
    { meridiem = "PM"; }

    if (hour === 0)
    { hour = 12; }

    if (hour > 12)
    { hour = hour - 12; }

    return (datevalue.getMonth() + 1) + "/" + datevalue.getDate() + "/" + datevalue.getFullYear() + " " + fmtZero(hour) + ":" + fmtZero(datevalue.getMinutes()) + " " + meridiem + " ";
}

fmtZero = function (str)
{ return ('0' + str).slice(-2); }


function autoResizeTextArea(e) {
    $(e).css({ 'height': 'auto', 'overflow-y': 'hidden' }).height(e.scrollHeight);
}

function displayNotification(message) {
    var notification = $('#notificationGroup', window.parent.document);
    $('#notificationMessage', window.parent.document).html(message);
    notification.fadeIn('slow');
    setTimeout(function ()
    { notification.fadeOut('slow'); }

    , 5 * 1000);
}

//This function clear all text input fields, need to provide collection of elements need to be cleared, calling example : resetFields($("input:text"));
function resetFields(fieldsToClear) {
    $(fieldsToClear).each(function () {
        $(this).val("");
    });
}

function minutesToStr(minutes) {
    var sign = '';
    if (minutes < 0) {
        sign = '-';
    }

    var hours = leftPad(Math.floor(Math.abs(minutes) / 60));
    var minutes = leftPad(Math.abs(minutes) % 60);

    return sign + hours + 'hrs ' + minutes + 'min';

}

function leftPad(number) {
    return ((number < 10 && number >= 0) ? '0' : '') + number;
}

function ShowBookerOwnershipAlert(bookingRequestBooker, Id) {
    if (alert("TYou can't do that action because Booker " + bookingRequestBooker + " \n has this item CheckedOut.")) {
        $.ajax({
            url: "/BookingRequest/ProcessBookerChange",
            type: "GET",
            data: { bookingRequestId: Id },
            success: function (response) {
                alert(response);
            }
        });
    }
}

function SubmitNegotiationForm(spinner, negotiation, callback) {
     $.ajax({
        type: "POST",
        dataType: 'json',
        contentType: 'application/json',
        url: '/Negotiation/ProcessSubmittedForm',
        data: JSON.stringify(negotiation)
    })
    .success(callback)
    .fail(function () {
        alert('An error occured on the server. Your changes have not been saved.');
    })
    .always(function () {
        if (spinner !== null) {
            spinner.hide();
        }
    });
}