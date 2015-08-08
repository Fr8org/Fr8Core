/// <reference path="../_all.ts" />
/*
    The service implements centralized string storage.
*/
var dockyard;
(function (dockyard) {
    var services;
    (function (services) {
        var strings = {
            processTemplate: {
                error404: "Sorry, the Process Template was not found. Perhaps it has been deleted.",
                error400: "Some of the specified data were invalid. Please verify your entry and try again.",
                error: "Process Template cannot be saved. Please try again in a few minutes."
            }
        };
        app.factory('StringService', function () { return strings; });
    })(services = dockyard.services || (dockyard.services = {}));
})(dockyard || (dockyard = {}));
//# sourceMappingURL=stringservice.js.map