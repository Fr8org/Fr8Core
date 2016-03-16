/// <reference path="../_all.ts" />

/*
    The filter converts numeric value to state name
*/
module dockyard {
    'use strict';
    app.filter('RouteState', () =>
        (input : number): string => {
            switch (input)
            {
            case model.RouteState.Active:
                return "Active";
            case model.RouteState.Inactive:
                return "Inactive";
            default:
                return "Inactive";
            }
        }); 
}