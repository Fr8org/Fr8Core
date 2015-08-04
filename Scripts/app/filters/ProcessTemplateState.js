/// <reference path="../_all.ts" />
/*
    The filter converts numeric value to state name
*/
var dockyard;
(function (dockyard) {
    'use strict';
    app.filter('ProcessTemplateState', function () {
        return function (input) {
            switch (input) {
                case dockyard.interfaces.ProcessState.Active:
                    return "Active";
                    break;
                case dockyard.interfaces.ProcessState.Inactive:
                    return "Inactive";
                    break;
                default:
                    return "Inactive";
            }
        };
    });
})(dockyard || (dockyard = {}));
//# sourceMappingURL=processtemplatestate.js.map