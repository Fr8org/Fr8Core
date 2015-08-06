/// <reference path="../_all.ts" />

/*
    The filter converts numeric value to state name
*/
module dockyard {
    'use strict';
    app.filter('ProcessTemplateState', () =>
        function(input : number) : string {
            switch (input)
            {
                case interfaces.ProcessState.Active:
                    return "Active";
                    break;
                case interfaces.ProcessState.Inactive:
                    return "Inactive";
                    break;
                default:
                    return "Inactive";
            }
        });
}