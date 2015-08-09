/// <reference path="../../app/_all.ts" />
/// <reference path="../../typings/angularjs/angular-mocks.d.ts" />

module dockyard.tests.controller {
    //Setup aliases
    import pwd = dockyard.directives.paneWorkflowDesigner;
    import pdc = dockyard.directives.paneDefineCriteria;
    import psa = dockyard.directives.paneSelectAction;
    import pca = dockyard.directives.paneConfigureAction;
    import pst = dockyard.directives.paneSelectTemplate;
    import pcm = dockyard.directives.paneConfigureMapping;

    describe("ProcessBuilder Framework message processing", () => {
        beforeEach(module("app"));
        var _$controllerService: ng.IControllerService,
            _$scope: interfaces.IProcessBuilderScope,
            _controller: any,
            _$state: ng.ui.IState;

        beforeEach(() => {
            inject(($controller, $rootScope) => {
                _$scope = tests.mocks.TypedScopeFactory.GetProcessBuilderScope($rootScope);
                _$state = {
                    data: {
                        pageSubTitle: ""
                    }
                };
                _controller = $controller("ProcessBuilderController",
                    {
                        $rootScope: $rootScope,
                        $scope: _$scope,
                        stringService: null,
                        LocalIdentityGenerator: null,
                        $state: _$state
                    });
            });
            spyOn(_$scope, "$broadcast");
        });

        //Below rule number are given per part 3. "Message Processing" of Design Document for DO-781 
        //at https://maginot.atlassian.net/wiki/display/SH/Design+Document+for+DO-781
        //Rules 1, 3 and 4 are bypassed because these events not yet stabilized

        //Rule #2
        it("When PaneWorkflowDesigner_TemplateSelecting is emitted, PaneSelectTemplate_Render should be received", () => {
            _$scope.$emit("PaneWorkflowDesigner_TemplateSelecting", null);
            expect(_$scope.$broadcast).toHaveBeenCalledWith('PaneSelectTemplate_Render');
        });

        //Rule #4

        //Rule #5
        it("When PaneSelectTemplate_ProcessTemplateUpdated is sent, state data (template name) must be updated", () => {
            var templateName = "testtemplate";
            var incomingEventArgs = new pst.ProcessTemplateUpdatedEventArgs(1, "testtemplate");
            _$scope.$emit("PaneSelectTemplate_ProcessTemplateUpdated", incomingEventArgs);
            expect(_$state.data.pageSubTitle).toBe(templateName);
        });

        //Rule #6
        it("When PaneSelectAction_ActionUpdated is sent, PaneWorkflowDesigner_UpdateAction " +
            "should be received with correct args", () => {
                var incomingEventArgs = new psa.ActionUpdatedEventArgs(1, 2, 3, "testaction"),
                    outgoingEventArgs = new pwd.UpdateActionEventArgs(1, 2, 3, "testaction");

                _$scope.$emit("PaneSelectAction_ActionUpdated", incomingEventArgs);

                expect(_$scope.$broadcast).toHaveBeenCalledWith("PaneWorkflowDesigner_UpdateAction", outgoingEventArgs);
            });

        //Rule #7
        it("When PaneConfigureAction_ActionUpdated is sent, PaneWorkflowDesigner_UpdateAction " +
            "should be received with correct args", () => {
                var incomingEventArgs = new pca.ActionUpdatedEventArgs(1, 2, 3),
                    outgoingEventArgs = new pwd.UpdateActionEventArgs(1, 2, 3, null);

                _$scope.$emit("PaneConfigureAction_ActionUpdated", incomingEventArgs);
                expect(_$scope.$broadcast).toHaveBeenCalledWith('PaneWorkflowDesigner_UpdateAction', outgoingEventArgs);
            });

        //Rule #8
        it("When PaneSelectAction_ActionTypeSelected is sent, PaneConfigureMapping_Render " +
            "and PaneConfigureAction_Render should be received with correct args", () => {
                var incomingEventArgs = new psa.ActionTypeSelectedEventArgs(1, 2, 3, 4, "myaction"),
                    outgoingEvent1Args = new pcm.RenderEventArgs(1, 2, false),
                    outgoingEvent2Args = new pca.RenderEventArgs(1, 2, false);

                _$scope.$emit("PaneSelectAction_ActionTypeSelected", incomingEventArgs);

                expect(_$scope.$broadcast).toHaveBeenCalledWith("PaneConfigureMapping_Render", outgoingEvent1Args);
                expect(_$scope.$broadcast).toHaveBeenCalledWith("PaneConfigureAction_Render", outgoingEvent2Args);
            });

        //Rule #9
        it("When PaneConfigureAction_Cancelled is sent, PaneConfigureMapping_Hide " +
            "and PaneSelectAction_Hide should be received with no args", () => {
                var incomingEventArgs = new pca.CancelledEventArgs(1, 2, false);

                _$scope.$emit("PaneConfigureAction_Cancelled", incomingEventArgs);

                expect(_$scope.$broadcast).toHaveBeenCalledWith("PaneConfigureMapping_Hide");
                expect(_$scope.$broadcast).toHaveBeenCalledWith("PaneSelectAction_Hide");
            });
    });
}