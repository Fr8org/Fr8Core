/// <reference path="../_all.ts" />

/*
    The filter converts numeric value to state name
*/
module dockyard {
    'use strict';
    app.filter('PlanState', () =>
        (input : number): string => {
            switch (input)
            {
                case model.PlanState.Running:
                    return "Running";
                case model.PlanState.Inactive:
                    return "Inactive";
                default:
                    return "Inactive";
            }
        });
}