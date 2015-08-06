var dockyard;
(function (dockyard) {
    var model;
    (function (model) {
        var Action = (function () {
            function Action(id, tempId, criteriaId) {
                this.criteriaId = criteriaId;
                this.id = id;
                this.tempId = tempId;
            }
            Action.prototype.clone = function () {
                var result = new Action(this.id, this.tempId, this.criteriaId);
                result.name = this.name;
                result.actionTypeId = this.actionTypeId;
                return result;
            };
            return Action;
        })();
        model.Action = Action;
    })(model = dockyard.model || (dockyard.model = {}));
})(dockyard || (dockyard = {}));
//# sourceMappingURL=Action.js.map