var dockyard;
(function (dockyard) {
    var tests;
    (function (tests) {
        var utils;
        (function (utils) {
            var fixtures;
            (function (fixtures) {
                var ProcessBuilder = (function () {
                    function ProcessBuilder() {
                    }
                    ProcessBuilder.newProcessTemplate = {
                        name: 'Test',
                        description: 'Description',
                        processTemplateState: 1
                    };
                    ProcessBuilder.updatedProcessTemplate = {
                        'name': 'Updated',
                        'description': 'Description',
                        'processTemplateState': 1,
                        'subscribedDocuSignTemplates': ['58521204-58af-4e65-8a77-4f4b51fef626']
                    };
                    return ProcessBuilder;
                })();
                fixtures.ProcessBuilder = ProcessBuilder;
            })(fixtures = utils.fixtures || (utils.fixtures = {}));
        })(utils = tests.utils || (tests.utils = {}));
    })(tests = dockyard.tests || (dockyard.tests = {}));
})(dockyard || (dockyard = {}));
//# sourceMappingURL=fixtures.js.map