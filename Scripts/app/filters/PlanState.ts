/// <reference path="../_all.ts" />

/*
    The filter converts numeric value to state name
*/
module dockyard {
    'use strict';
    app.filter('PlanState', () =>
        (input : string): string => {
            switch (input)
            {
                case model.PlanState.Executing:
                    return "Executing";
                case model.PlanState.Inactive:
                    return "Inactive";
                case model.PlanState.Active:
                    return "Active";
                default:
                    return "Inactive";
            }
        });
}