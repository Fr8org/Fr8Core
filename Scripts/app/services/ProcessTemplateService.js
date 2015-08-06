/// <reference path="../_all.ts" />
var dockyard;
(function (dockyard) {
    var services;
    (function (services) {
        app.factory('ProcessTemplateService', ['$resource', function ($resource) {
                return $resource('/api/ProcessTemplate/:id', { id: '@id' });
            }
        ]);
    })(services = dockyard.services || (dockyard.services = {}));
})(dockyard || (dockyard = {}));
//# sourceMappingURL=processtemplateservice.js.map