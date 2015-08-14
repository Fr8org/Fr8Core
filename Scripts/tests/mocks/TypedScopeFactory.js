var dockyard;
(function (dockyard) {
    var tests;
    (function (tests) {
        var mocks;
        (function (mocks) {
            var TypedScopeFactory = (function () {
                function TypedScopeFactory() {
                }
                TypedScopeFactory.GetProcessBuilderScope = function (rootScope) {
                    var scope = rootScope.$new();
                    scope.processTemplateId = 0;
                    scope.criteria = null;
                    scope.fields = null;
                    return scope;
                };
                return TypedScopeFactory;
            })();
            mocks.TypedScopeFactory = TypedScopeFactory;
        })(mocks = tests.mocks || (tests.mocks = {}));
    })(tests = dockyard.tests || (dockyard.tests = {}));
})(dockyard || (dockyard = {}));
//# sourceMappingURL=TypedScopeFactory.js.map