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
            var pca = dockyard.directives.paneConfigureAction;
            var pst = dockyard.directives.paneSelectTemplate;
            var pcm = dockyard.directives.paneConfigureMapping;
            describe("ProcessBuilder Framework message processing", function () {
                beforeEach(module("app"));
                app.run(['$httpBackend',
                    function ($httpBackend) {
                        $httpBackend.whenGET().passThrough();
                    }
                ]);
                var _$controllerService, _$scope, _controller, _$state, _actionServiceMock, _$q, _$http, _urlPrefix;
                beforeEach(function () {
                    inject(function ($controller, $rootScope, $q, $http) {
                        _actionServiceMock = new tests.utils.ActionServiceMock($q);
                        _$q = $q;
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
                            LocalIdentityGenerator: null,
                            $state: _$state,
                            ActionService: _actionServiceMock,
                            $http: _$http,
                            urlPrefix: _urlPrefix
                        });
                    });
                    spyOn(_$scope, "$broadcast");
                });
                it("When PaneWorkflowDesigner_TemplateSelected is emitted, PaneSelectTemplate_Render should be received", function () {
                    _$scope.$emit(pwd.MessageType[pwd.MessageType.PaneWorkflowDesigner_TemplateSelecting], null);
                    expect(_$scope.$broadcast).toHaveBeenCalledWith('PaneSelectTemplate_Render');
                });
                it("When PaneSelectTemplate_ProcessTemplateUpdated is sent, state data (template name) must be updated", function () {
                    var templateName = "testtemplate";
                    var incomingEventArgs = new pst.ProcessTemplateUpdatedEventArgs(1, "testtemplate", ['test']);
                    _$scope.$emit(pst.MessageType[pst.MessageType.PaneSelectTemplate_ProcessTemplateUpdated], incomingEventArgs);
                });
                it("When PaneSelectAction_ActionTypeSelected is sent, " +
                    "PaneConfigureAction_Render should be received with correct args", function () {
                    var incomingEventArgs = new psa.ActionTypeSelectedEventArgs(new dockyard.model.ActionDesignDTO(1, 2, false, 3)), outgoingEvent1Args = new pcm.RenderEventArgs(1, 2, false), outgoingEvent2Args = new pca.RenderEventArgs(new dockyard.model.ActionDesignDTO(1, 2, false, 3));
                    _$scope.$emit(psa.MessageType[psa.MessageType.PaneSelectAction_ActionTypeSelected], incomingEventArgs);
                    expect(_$scope.$broadcast).toHaveBeenCalledWith("PaneConfigureAction_Render", outgoingEvent2Args);
                });
                it("When PaneConfigureAction_MapFieldsClicked is sent, " +
                    "PaneConfigureMapping_Render should be received with correct args", function () {
                    var incomingEventArgs = new pca.MapFieldsClickedEventArgs(new dockyard.model.ActionDesignDTO(1, 1, false, 1)), outgoingEvent1Args = new pcm.RenderEventArgs(1, 1, false);
                    _$scope.$emit(pca.MessageType[pca.MessageType.PaneConfigureAction_MapFieldsClicked], incomingEventArgs);
                    expect(_$scope.$broadcast).toHaveBeenCalledWith("PaneConfigureMapping_Render", outgoingEvent1Args);
                });
                it("When PaneWorkflowDesigner_ActionSelected is sent and selectedAction!=null " +
                    "Save method should be called on ProcessTemplateService", function () {
                    var incomingEventArgs = new pwd.ActionSelectingEventArgs(1, 1, 1);
                    var currentAction = new dockyard.model.ActionDesignDTO(1, 1, false, 1);
                    _$scope.currentAction = currentAction;
                    _$scope.$emit(pwd.MessageType[pwd.MessageType.PaneWorkflowDesigner_ActionSelecting], incomingEventArgs);
                    expect(_actionServiceMock.save).toHaveBeenCalledWith({ id: currentAction.id }, currentAction, null, null);
                });
                it("When PaneWorkflowDesigner_ActionSelected is sent and selectedAction==null " +
                    "Save method on ProcessTemplateService should NOT be called", function () {
                    var incomingEventArgs = new pwd.CriteriaSelectedEventArgs(1, true);
                    _$scope.currentAction = null;
                    _$scope.$emit(pwd.MessageType[pwd.MessageType.PaneWorkflowDesigner_ActionSelecting], incomingEventArgs);
                    expect(_actionServiceMock.save).not.toHaveBeenCalled();
                });
                it("When PaneWorkflowDesigner_ProcessNodeTemplateSelecting is sent and selectedAction!=null " +
                    "Save method should be called on ProcessTemplateService", function () {
                    var incomingEventArgs = new pwd.CriteriaSelectedEventArgs(1, true);
                    var currentAction = new dockyard.model.ActionDesignDTO(1, 1, false, 1);
                    _$scope.currentAction = currentAction;
                    _$scope.$emit(pwd.MessageType[pwd.MessageType.PaneWorkflowDesigner_CriteriaSelected], incomingEventArgs);
                    expect(_actionServiceMock.save).toHaveBeenCalledWith({ id: currentAction.id }, currentAction, null, null);
                });
                it("When PaneWorkflowDesigner_TemplateSelected is sent and selectedAction!=null " +
                    "Save method should be called on ProcessTemplateService", function () {
                    var incomingEventArgs = new pwd.TemplateSelectingEventArgs();
                    var currentAction = new dockyard.model.ActionDesignDTO(1, 1, false, 1);
                    _$scope.currentAction = currentAction;
                    _$scope.$emit(pwd.MessageType[pwd.MessageType.PaneWorkflowDesigner_TemplateSelecting], incomingEventArgs);
                    expect(_actionServiceMock.save).toHaveBeenCalledWith({ id: currentAction.id }, currentAction, null, null);
                });
            });
        })(controller = tests.controller || (tests.controller = {}));
    })(tests = dockyard.tests || (dockyard.tests = {}));
})(dockyard || (dockyard = {}));
//# sourceMappingURL=ProcessBuilderControllerTests.js.map