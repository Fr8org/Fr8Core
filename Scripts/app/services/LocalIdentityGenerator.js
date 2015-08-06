/// <reference path="../_all.ts" />
var dockyard;
(function (dockyard) {
    var services;
    (function (services) {
        var LocalIdentityGenerator = (function () {
            function LocalIdentityGenerator() {
                this._nextId = 0;
            }
            LocalIdentityGenerator.prototype.getNextId = function () {
                return ++this._nextId;
            };
            return LocalIdentityGenerator;
        })();
        services.LocalIdentityGenerator = LocalIdentityGenerator;
    })(services = dockyard.services || (dockyard.services = {}));
})(dockyard || (dockyard = {}));
app.service('LocalIdentityGenerator', dockyard.services.LocalIdentityGenerator);
//# sourceMappingURL=LocalIdentityGenerator.js.map