var dockyard;
(function (dockyard) {
    var tests;
    (function (tests) {
        var utils;
        (function (utils) {
            //The class contains methods to create mocks for complex objects
            var Factory = (function () {
                function Factory() {
                }
                //Creates a mock for ProcessBuilderController $scope
                Factory.GetProcessBuilderScope = function (rootScope) {
                    var scope = rootScope.$new();
                    scope.processTemplateId = 0;
                    scope.processNodeTemplates = null;
                    scope.fields = null;
                    return scope;
                };
                return Factory;
            })();
            utils.Factory = Factory;
            /*
                Mock for ActionService
            */
            var ActionServiceMock = (function () {
                function ActionServiceMock($q) {
                    this.get = jasmine.createSpy('get');
                    this.save = jasmine.createSpy('save');
                    this.configure = jasmine.createSpy('configure');
                    if ($q) {
                        this.save.and.callFake(function () {
                            var def = $q.defer();
                            def.resolve();
                            def.promise.$promise = def.promise;
                            return def.promise;
                        });
                    }
                }
                return ActionServiceMock;
            })();
            utils.ActionServiceMock = ActionServiceMock;
            var ProcessTemplateServiceMock = (function () {
                function ProcessTemplateServiceMock($q) {
                    this.get = jasmine.createSpy('get').and.callFake(function () {
                        var def = $q.defer();
                        def.resolve(utils.fixtures.ProcessBuilder.newProcessTemplate);
                        def.promise.$promise = def.promise;
                        return def.promise;
                    });
                    this.getFull = jasmine.createSpy('getFull').and.callFake(function () {
                        var def = $q.defer();
                        def.resolve(utils.fixtures.ProcessBuilder.fullProcessTemplate);
                        def.promise.$promise = def.promise;
                        return def.promise;
                    });
                }
                return ProcessTemplateServiceMock;
            })();
            utils.ProcessTemplateServiceMock = ProcessTemplateServiceMock;
            var ProcessBuilderServiceMock = (function () {
                function ProcessBuilderServiceMock($q) {
                    this.save = jasmine.createSpy('save').and.callFake(function () {
                        var def = $q.defer();
                        def.resolve();
                        def.promise.$promise = def.promise;
                        return def.promise;
                    });
                    this.saveCurrent = jasmine.createSpy('saveCurrent').and.callFake(function () {
                        var def = $q.defer();
                        def.resolve(utils.fixtures.ProcessBuilder.processBuilderState);
                        def.promise.$promise = def.promise;
                        return def.promise;
                    });
                }
                return ProcessBuilderServiceMock;
            })();
            utils.ProcessBuilderServiceMock = ProcessBuilderServiceMock;
            var $ModalMock = (function () {
                function $ModalMock($q) {
                    this.open = jasmine.createSpy('open').and.returnValue({
                        result: $q.when(utils.fixtures.ActivityTemplate.activityTemplateDO)
                    });
                }
                return $ModalMock;
            })();
            utils.$ModalMock = $ModalMock;
        })(utils = tests.utils || (tests.utils = {}));
    })(tests = dockyard.tests || (dockyard.tests = {}));
})(dockyard || (dockyard = {}));
//# sourceMappingURL=mocks.js.map