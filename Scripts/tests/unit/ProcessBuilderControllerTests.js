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
                var _$controllerService, _$scope, _controller, _$state, _actionServiceMock, _$q;
                beforeEach(function () {
                    inject(function ($controller, $rootScope, $q) {
                        _actionServiceMock = new tests.utils.ActionServiceMock($q);
                        _$q = $q;
                        _$scope = tests.utils.Factory.GetProcessBuilderScope($rootScope);
                        _$state = {
                            data: {
                                pageSubTitle: ""
                            }
                        };
                        _controller = $controller("ProcessBuilderController", {
                            $rootScope: $rootScope,
                            $scope: _$scope,
                            stringService: null,
                            LocalIdentityGenerator: null,
                            $state: _$state,
                            ActionService: _actionServiceMock
                        });
                    });
                    spyOn(_$scope, "$broadcast");
                });
                it("When PaneWorkflowDesigner_TemplateSelected is emitted, PaneSelectTemplate_Render should be received", function () {
                    _$scope.$emit(pwd.MessageType[pwd.MessageType.PaneWorkflowDesigner_TemplateSelected], null);
                    expect(_$scope.$broadcast).toHaveBeenCalledWith('PaneSelectTemplate_Render');
                });
                it("When PaneSelectTemplate_ProcessTemplateUpdated is sent, state data (template name) must be updated", function () {
                    var templateName = "testtemplate";
                    var incomingEventArgs = new pst.ProcessTemplateUpdatedEventArgs(1, "testtemplate");
                    _$scope.$emit(pst.MessageType[pst.MessageType.PaneSelectTemplate_ProcessTemplateUpdated], incomingEventArgs);
                    expect(_$state.data.pageSubTitle).toBe(templateName);
                });
                it("When PaneSelectAction_ActionUpdated is sent, PaneWorkflowDesigner_UpdateAction " +
                    "should be received with correct args", function () {
                    var incomingEventArgs = new psa.ActionUpdatedEventArgs(1, 2, true, "testaction"), outgoingEventArgs = new pwd.UpdateActionEventArgs(1, 2, true, "testaction");
                    _$scope.$emit(psa.MessageType[psa.MessageType.PaneSelectAction_ActionUpdated], incomingEventArgs);
                    expect(_$scope.$broadcast).toHaveBeenCalledWith("PaneWorkflowDesigner_UpdateAction", outgoingEventArgs);
                });
                it("When PaneConfigureAction_ActionUpdated is sent, PaneWorkflowDesigner_UpdateAction " +
                    "should be received with correct args", function () {
                    var incomingEventArgs = new pca.ActionUpdatedEventArgs(1, 2, true), outgoingEventArgs = new pwd.UpdateActionEventArgs(1, 2, true, null);
                    _$scope.$emit(pca.MessageType[pca.MessageType.PaneConfigureAction_ActionUpdated], incomingEventArgs);
                    expect(_$scope.$broadcast).toHaveBeenCalledWith('PaneWorkflowDesigner_UpdateAction', outgoingEventArgs);
                });
                it("When PaneSelectAction_ActionTypeSelected is sent, PaneConfigureMapping_Render " +
                    "and PaneConfigureAction_Render should be received with correct args", function () {
                    var incomingEventArgs = new psa.ActionTypeSelectedEventArgs(1, 2, false, "myaction", "myaction"), outgoingEvent1Args = new pcm.RenderEventArgs(1, 2, false), outgoingEvent2Args = new pca.RenderEventArgs(1, 2, false);
                    _$scope.$emit(psa.MessageType[psa.MessageType.PaneSelectAction_ActionTypeSelected], incomingEventArgs);
                    expect(_$scope.$broadcast).toHaveBeenCalledWith("PaneConfigureMapping_Render", outgoingEvent1Args);
                    expect(_$scope.$broadcast).toHaveBeenCalledWith("PaneConfigureAction_Render", outgoingEvent2Args);
                });
                it("When PaneWorkflowDesigner_ActionSelected is sent and selectedAction!=null " +
                    "Save method should be called on ProcessTemplateService", function () {
                    var incomingEventArgs = new pwd.ActionSelectedEventArgs(1, 1);
                    var currentAction = new dockyard.model.Action(1, false);
                    _$scope.currentAction = currentAction;
                    _$scope.$emit(pwd.MessageType[pwd.MessageType.PaneWorkflowDesigner_ActionSelected], incomingEventArgs);
                    expect(_actionServiceMock.save).toHaveBeenCalledWith({ id: currentAction.id }, currentAction, null, null);
                });
                it("When PaneWorkflowDesigner_ActionSelected is sent and selectedAction==null " +
                    "Save method on ProcessTemplateService should NOT be called", function () {
                    var incomingEventArgs = new pwd.CriteriaSelectedEventArgs(1);
                    _$scope.currentAction = null;
                    _$scope.$emit(pwd.MessageType[pwd.MessageType.PaneWorkflowDesigner_ActionSelected], incomingEventArgs);
                    expect(_actionServiceMock.save).not.toHaveBeenCalled();
                });
                it("When PaneWorkflowDesigner_CriteriaSelected is sent and selectedAction!=null " +
                    "Save method should be called on ProcessTemplateService", function () {
                    var incomingEventArgs = new pwd.CriteriaSelectedEventArgs(1);
                    var currentAction = new dockyard.model.Action(1, false);
                    _$scope.currentAction = currentAction;
                    _$scope.$emit(pwd.MessageType[pwd.MessageType.PaneWorkflowDesigner_CriteriaSelected], incomingEventArgs);
                    expect(_actionServiceMock.save).toHaveBeenCalledWith({ id: currentAction.id }, currentAction, null, null);
                });
                it("When PaneWorkflowDesigner_TemplateSelected is sent and selectedAction!=null " +
                    "Save method should be called on ProcessTemplateService", function () {
                    var incomingEventArgs = new pwd.TemplateSelectedEventArgs();
                    var currentAction = new dockyard.model.Action(1, false);
                    _$scope.currentAction = currentAction;
                    _$scope.$emit(pwd.MessageType[pwd.MessageType.PaneWorkflowDesigner_TemplateSelected], incomingEventArgs);
                    expect(_actionServiceMock.save).toHaveBeenCalledWith({ id: currentAction.id }, currentAction, null, null);
                });
            });
        })(controller = tests.controller || (tests.controller = {}));
    })(tests = dockyard.tests || (dockyard.tests = {}));
})(dockyard || (dockyard = {}));
//# sourceMappingURL=ProcessBuilderControllerTests.js.map