/// <reference path="../_all.ts" />

/*
    The filter converts numeric value to state name
*/
module dockyard {
    'use strict';
    app.filter('PlanState', () =>
        function (input: number): string {
            switch (input) {
                case model.PlanState.Active:
                    return "Active";
                    break;
                case model.PlanState.Inactive:
                    return "Inactive";
                    break;
                default:
                    return "Inactive";
            }
        });
}