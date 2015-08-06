var dockyard;
(function (dockyard) {
    var model;
    (function (model) {
        var Condition = (function () {
            function Condition(field, operator, value) {
                this.field = field;
                this.operator = operator;
                this.value = value;
            }
            Condition.prototype.validate = function () {
                this.valueError = !this.value;
            };
            Condition.prototype.clone = function () {
                var result = new Condition(this.field, this.operator, this.value);
                result.valueError = this.valueError;
                return result;
            };
            return Condition;
        })();
        model.Condition = Condition;
    })(model = dockyard.model || (dockyard.model = {}));
})(dockyard || (dockyard = {}));
//# sourceMappingURL=Condition.js.map