var dockyard;
(function (dockyard) {
    var model;
    (function (model) {
        var Field = (function () {
            function Field(key, name) {
                this.key = key;
                this.name = name;
            }
            Field.prototype.clone = function () {
                return new Field(this.key, this.name);
            };
            return Field;
        })();
        model.Field = Field;
    })(model = dockyard.model || (dockyard.model = {}));
})(dockyard || (dockyard = {}));
//# sourceMappingURL=Field.js.map