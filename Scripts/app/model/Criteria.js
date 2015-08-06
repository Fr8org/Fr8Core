var dockyard;
(function (dockyard) {
    var model;
    (function (model) {
        (function (CriteriaExecutionMode) {
            CriteriaExecutionMode[CriteriaExecutionMode["WithConditions"] = 1] = "WithConditions";
            CriteriaExecutionMode[CriteriaExecutionMode["WithoutConditions"] = 2] = "WithoutConditions";
        })(model.CriteriaExecutionMode || (model.CriteriaExecutionMode = {}));
        var CriteriaExecutionMode = model.CriteriaExecutionMode;
        var Criteria = (function () {
            function Criteria(id, isTempId, name, executionMode) {
                this.id = id;
                this.isTempId = isTempId;
                this.name = name;
                this.executionMode = executionMode;
                this.actions = [];
                this.conditions = [];
            }
            Criteria.prototype.clone = function () {
                var result = new Criteria(this.id, this.isTempId, this.name, this.executionMode);
                this.actions.forEach(function (it) { result.actions.push(it.clone()); });
                this.conditions.forEach(function (it) { result.conditions.push(it.clone()); });
                return result;
            };
            return Criteria;
        })();
        model.Criteria = Criteria;
    })(model = dockyard.model || (dockyard.model = {}));
})(dockyard || (dockyard = {}));
//# sourceMappingURL=Criteria.js.map