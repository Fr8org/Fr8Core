/// <reference path="../_all.ts" />

/*
    The filter converts numeric value to state name
*/
module dockyard {
    'use strict';
    app.filter('actionNameFormatter', () =>
        (input : string): string => {
            var str = input.replace(/_+/g, ' ');
            return str;
        });
}