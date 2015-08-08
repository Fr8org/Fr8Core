/// <reference path="../_all.ts" />
var dockyard;
(function (dockyard) {
    var interfaces;
    (function (interfaces) {
        (function (ProcessState) {
            ProcessState[ProcessState["Inactive"] = 0] = "Inactive";
            ProcessState[ProcessState["Active"] = 1] = "Active";
        })(interfaces.ProcessState || (interfaces.ProcessState = {}));
        var ProcessState = interfaces.ProcessState;
    })(interfaces = dockyard.interfaces || (dockyard.interfaces = {}));
})(dockyard || (dockyard = {}));
//# sourceMappingURL=iprocesstemplatevm.js.map