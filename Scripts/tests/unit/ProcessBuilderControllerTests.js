/// <reference path="../../app/_all.ts" />
/// <reference path="../../typings/angularjs/angular-mocks.d.ts" />
var dockyard;
(function (dockyard) {
    var tests;
    (function (tests) {
        var controller;
        (function (controller) {
            var pwd = dockyard.directives.paneWorkflowDesigner;
            var psa = dockyard.directives.paneSelectAction;
            describe("ProcessBuilder Framework message processing", function () {
                beforeEach(module("app"));
                app.run(['$httpBackend', function ($httpBackend) {
                        $httpBackend.expectGET('/AngularTemplate/ProcessTemplateList').respond(200, '<div></div>');
                    }
                ]);
                var _$controllerService, _$scope, _controller, _$state, _actionServiceMock, _processTemplateServiceMock, _actionListServiceMock, _processBuilderServiceMock, _$q, _$http, _urlPrefix, _crateHelper, _localIdentityGenerator, _$timeout;
                beforeEach(function () {
                    inject(function ($controller, $rootScope, $q, $http, $timeout) {
                        _actionServiceMock = new tests.utils.ActionServiceMock($q);
                        _processTemplateServiceMock = new tests.utils.ProcessTemplateServiceMock($q);
                        _actionListServiceMock = new tests.utils.ActionListServiceMock($q);
                        _processBuilderServiceMock = new tests.utils.ProcessBuilderServiceMock($q);
                        _crateHelper = new dockyard.services.CrateHelper();
                        _localIdentityGenerator = new dockyard.services.LocalIdentityGenerator();
                        _$q = $q;
                        _$timeout = $timeout;
                        _$scope = tests.utils.Factory.GetProcessBuilderScope($rootScope);
                        _$state = {
                            data: {
                                pageSubTitle: ""
                            },
                            params: {
                                id: 1
                            }
                        };
                        _$http = $http;
                        _urlPrefix = '/api';
                        _controller = $controller("ProcessBuilderController", {
                            $rootScope: $rootScope,
                            $scope: _$scope,
                            stringService: null,
                            LocalIdentityGenerator: _localIdentityGenerator,
                            $state: _$state,
                            ActionService: _actionServiceMock,
                            $q: _$q,
                            $http: _$http,
                            urlPrefix: _urlPrefix,
                            ProcessTemplateService: _processTemplateServiceMock,
                            $timeout: _$timeout,
                            CriteriaServiceWrapper: null,
                            ProcessBuilderService: _processBuilderServiceMock,
                            ActionListService: _actionListServiceMock,
                            CrateHelper: _crateHelper,
                            ActivityTemplateService: null
                        });
                    });
                    spyOn(_$scope, "$broadcast");
                });
                var resolvePromises = function () {
                    _$scope.$apply();
                };
                it("When PaneWorkflowDesigner_TemplateSelected is emitted, PaneSelectAction_Hide should be received", function () {
                    _$scope.$emit(pwd.MessageType[pwd.MessageType.PaneWorkflowDesigner_TemplateSelected], null);
                    resolvePromises();
                    expect(_$scope.$broadcast).toHaveBeenCalledWith(psa.MessageType[psa.MessageType.PaneSelectAction_Hide]);
                });
                it("When PaneWorkflowDesigner_ActionAdding is emitted, PaneWorkflowDesigner_AddAction should be received", function () {
                    var event = new pwd.ActionAddingEventArgs(1, 1);
                    _$scope.$emit(pwd.MessageType[pwd.MessageType.PaneWorkflowDesigner_ActionAdding], event);
                    resolvePromises();
                    expect(_$scope.$broadcast).toHaveBeenCalledWith(pwd.MessageType[pwd.MessageType.PaneWorkflowDesigner_AddAction], jasmine.any(Object));
                });
                it("When PaneWorkflowDesigner_ActionAdding is emitted, newly created ActionDesignDTO should have correct values", function () {
                    var event = new pwd.ActionAddingEventArgs(1, 1);
                    _$scope.$emit(pwd.MessageType[pwd.MessageType.PaneWorkflowDesigner_ActionAdding], event);
                    resolvePromises();
                    var createdActionDesignDTO = _$scope.current.action;
                    expect(createdActionDesignDTO.actionListId).toEqual(tests.utils.fixtures.ProcessBuilder.newActionListDTO.id);
                    expect(createdActionDesignDTO.crateStorage).not.toBeNull();
                    expect(createdActionDesignDTO.isTempId).toBeTruthy();
                    expect(createdActionDesignDTO.id).toEqual(_localIdentityGenerator.getNextId() + 1);
                });
            });
        })(controller = tests.controller || (tests.controller = {}));
    })(tests = dockyard.tests || (dockyard.tests = {}));
})(dockyard || (dockyard = {}));
//# sourceMappingURL=ProcessBuilderControllerTests.js.map