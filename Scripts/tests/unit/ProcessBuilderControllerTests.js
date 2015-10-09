/// <reference path="../../app/_all.ts" />
/// <reference path="../../typings/angularjs/angular-mocks.d.ts" />
var dockyard;
(function (dockyard) {
    var tests;
    (function (tests) {
        var controller;
        (function (controller) {
            describe("ProcessBuilder Framework message processing", function () {
                beforeEach(module("app"));
                app.run(['$httpBackend', function ($httpBackend) {
                        $httpBackend.expectGET('/AngularTemplate/MyAccountPage').respond(200, '<div></div>');
                    }
                ]);
                var _$controllerService, _$scope, _controller, _$state, _actionServiceMock, _processTemplateServiceMock, _actionListServiceMock, _processBuilderServiceMock, _$q, _$http, _urlPrefix, _crateHelper, _localIdentityGenerator, _$timeout, _$filter, _$modalMock;
                beforeEach(function () {
                    inject(function ($controller, $rootScope, $q, $http, $timeout, $filter, $httpBackend) {
                        _actionServiceMock = new tests.utils.ActionServiceMock($q);
                        _processTemplateServiceMock = new tests.utils.ProcessTemplateServiceMock($q);
                        _actionListServiceMock = new tests.utils.ActionListServiceMock($q);
                        _processBuilderServiceMock = new tests.utils.ProcessBuilderServiceMock($q);
                        _crateHelper = new dockyard.services.CrateHelper();
                        _localIdentityGenerator = new dockyard.services.LocalIdentityGenerator();
                        _$q = $q;
                        _$timeout = $timeout;
                        _$scope = tests.utils.Factory.GetProcessBuilderScope($rootScope);
                        _$filter = $filter;
                        _$state = {
                            data: {
                                pageSubTitle: ""
                            },
                            params: {
                                id: 1
                            }
                        };
                        _$http = $http;
                        _$modalMock = new tests.utils.$ModalMock($q);
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
                            ActivityTemplateService: null,
                            $filter: _$filter,
                            $modal: _$modalMock
                        });
                    });
                    spyOn(_$scope, "$broadcast");
                });
                var resolvePromises = function () {
                    _$scope.$apply();
                };
            });
        })(controller = tests.controller || (tests.controller = {}));
    })(tests = dockyard.tests || (dockyard.tests = {}));
})(dockyard || (dockyard = {}));
//# sourceMappingURL=ProcessBuilderControllerTests.js.map