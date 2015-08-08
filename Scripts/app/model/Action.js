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
            return Action;
        })();
        model.Action = Action;
    })(model = dockyard.model || (dockyard.model = {}));
})(dockyard || (dockyard = {}));
//# sourceMappingURL=Action.js.map