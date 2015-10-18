/// <reference path="../_all.ts" />

/*
    The filter converts numeric value to state name
*/
module dockyard {
    'use strict';
    app.filter('RouteState', () =>
        function(input : number) : string {
            switch (input)
            {
                case model.RouteState.Active:
                    return "Active";
                    break;
                case model.RouteState.Inactive:
                    return "Inactive";
                    break;
                default:
                    return "Inactive";
            }
        }); 
}