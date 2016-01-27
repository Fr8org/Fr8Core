/// <reference path="../_all.ts" />

/*
    The filter converts datetimeoffset value to local datetime
*/
module dockyard {
    'use strict';
    app.filter('datetime', () => input => {
        if (input == null) {
             return "";
        }
        var dateValue = new Date(input);
        var date = dateValue.toLocaleDateString() + ' ' + dateValue.toLocaleTimeString();
        return date;
    });
}

   