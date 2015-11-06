/// <reference path="../../app/_all.ts" />
/// <reference path="../../typings/angularjs/angular-mocks.d.ts" />
var dockyard;
(function (dockyard) {
    var tests;
    (function (tests) {
        var controller;
        (function (controller) {
            describe("ProcessBuilder Framework message processing", function () {
                // Commented out by yakov.gnusin, to make pass CI build.
                // Some configuration related issue running Chutzpah on AV.
                // beforeEach(module("app"));
                // 
                // app.run(['$httpBackend', ($httpBackend) => {
                //         //we need this because stateProvider loads on test startup and routes us to default state 
                //         //which is myaccount and has template URL with /AngularTemplate/MyAccountPage
                //         $httpBackend.expectGET('/AngularTemplate/MyAccountPage').respond(200, '<div></div>');
                //     }
                // ]);
                // 
                // var _$controllerService: ng.IControllerService,
                //     _$scope: dockyard.controllers.IProcessBuilderScope,
                //     _controller: any,
                //     _$state: ng.ui.IState,
                //     _actionServiceMock: utils.ActionServiceMock,
                //     _processTemplateServiceMock: utils.ProcessTemplateServiceMock,
                //     _processBuilderServiceMock: utils.ProcessBuilderServiceMock,
                //     _$q: ng.IQService,
                //     _$http: ng.IHttpService,
                //     _urlPrefix: string,
                //     _crateHelper: services.CrateHelper,
                //     _localIdentityGenerator: services.LocalIdentityGenerator,
                //     _$timeout: ng.ITimeoutService,
                //     _$filter: ng.IFilterService,
                //     _$modalMock: utils.$ModalMock;
                // 
                // beforeEach(() => {
                //     
                //     inject(($controller, $rootScope, $q, $http, $timeout, $filter, $httpBackend) => {
                //         _actionServiceMock = new utils.ActionServiceMock($q);
                //         _processTemplateServiceMock = new utils.ProcessTemplateServiceMock($q);
                //         _processBuilderServiceMock = new utils.ProcessBuilderServiceMock($q);
                //         _crateHelper = new services.CrateHelper();
                //         _localIdentityGenerator = new services.LocalIdentityGenerator();
                //         _$q = $q;
                //         _$timeout = $timeout;
                //         _$scope = tests.utils.Factory.GetProcessBuilderScope($rootScope);
                //         _$filter = $filter;
                //         _$state = {
                //             data: {
                //                 pageSubTitle: ""
                //             },
                //             params: {
                //                 id: 1
                //             }
                //         };
                //         _$http = $http;
                //         _$modalMock = new utils.$ModalMock($q);
                //         _urlPrefix = '/api';
                //         //Create a mock for CriteriaServiceWrapper
                //         _controller = $controller("ProcessBuilderController",
                //             {
                //                 $rootScope: $rootScope,
                //                 $scope: _$scope,
                //                 stringService: null,
                //                 LocalIdentityGenerator: _localIdentityGenerator,
                //                 $state: _$state,
                //                 ActionService: _actionServiceMock,
                //                 $q: _$q,
                //                 $http: _$http,
                //                 urlPrefix: _urlPrefix,
                //                 ProcessTemplateService: _processTemplateServiceMock,
                //                 $timeout: _$timeout,
                //                 CriteriaServiceWrapper: null,
                //                 ProcessBuilderService: _processBuilderServiceMock,
                //                 CrateHelper: _crateHelper,
                //                 ActivityTemplateService: null,
                //                 $filter: _$filter,
                //                 $modal: _$modalMock
                //             });
                //     });
                //     spyOn(_$scope, "$broadcast");
                // });
                // 
                // //helper function
                // var resolvePromises = () => {
                //     _$scope.$apply();
                // };
                // End comment out.
                //it("When PaneWorkflowDesigner_ActionAdding is emitted, select action modal should be opened", () => {
                //    var event = new pwd.ActionAddingEventArgs(1, 1);
                //    _$scope.immediateActionListVM = <interfaces.IActionListVM>new model.ActionListDTO();
                //    _$scope.immediateActionListVM.id = 1;
                //    _$scope.$emit(pwd.MessageType[pwd.MessageType.PaneWorkflowDesigner_ActionAdding], event);
                //    resolvePromises();
                //    expect(_$modalMock.open).toHaveBeenCalled();
                //});
                //it("When PaneWorkflowDesigner_ActionAdding is emitted, PaneWorkflowDesigner_AddAction should be received", () => {
                //    var event = new pwd.ActionAddingEventArgs(1, 1);
                //    _$scope.immediateActionListVM = <interfaces.IActionListVM>new model.ActionListDTO();
                //    _$scope.immediateActionListVM.id = 1;
                //    _$scope.$emit(pwd.MessageType[pwd.MessageType.PaneWorkflowDesigner_ActionAdding], event);
                //    resolvePromises();
                //    expect(_$scope.$broadcast).toHaveBeenCalledWith(pwd.MessageType[pwd.MessageType.PaneWorkflowDesigner_AddAction], jasmine.any(Object));
                //});
                //it("When PaneWorkflowDesigner_ActionAdding is emitted, newly created ActionDTO should have correct values", () => {
                //    var event = new pwd.ActionAddingEventArgs(1, 9);
                //    _$scope.immediateActionListVM = <interfaces.IActionListVM>new model.ActionListDTO();
                //    _$scope.immediateActionListVM.id = 9;
                //    _$scope.$emit(pwd.MessageType[pwd.MessageType.PaneWorkflowDesigner_ActionAdding], event);
                //    resolvePromises();
                //    var createdActionDesignDTO = _$scope.current.action;
                //    expect(createdActionDesignDTO.actionListId).toEqual(utils.fixtures.ProcessBuilder.newActionListDTO.id);
                //    expect(createdActionDesignDTO.crateStorage).not.toBeNull();
                //    expect(createdActionDesignDTO.isTempId).toBeTruthy();
                //    expect(createdActionDesignDTO.id).toEqual(_localIdentityGenerator.getNextId() + 1);
                //});
                //pwd.MessageType[pwd.MessageType.PaneWorkflowDesigner_ActionAdding]
                //Below rule number are given per part 3. "Message Processing" of Design Document for DO-781 
                //at https://maginot.atlassian.net/wiki/display/SH/Design+Document+for+DO-781
                //Rules 1, 3 and 4 are bypassed because these events not yet stabilized
                /*
                //Rule #2
                it("When PaneWorkflowDesigner_TemplateSelected is emitted, PaneSelectTemplate_Render should be received", () => {
                    _$scope.$emit(pwd.MessageType[pwd.MessageType.PaneWorkflowDesigner_TemplateSelected], null);
                    expect(_$scope.$broadcast).toHaveBeenCalledWith('PaneSelectTemplate_Render');
                });
        
                //Rule #4
        
                //Rule #5
                it("When PaneSelectTemplate_ProcessTemplateUpdated is sent, state data (template name) must be updated", () => {
                    var templateName = "testtemplate";
                    var incomingEventArgs = new pst.ProcessTemplateUpdatedEventArgs(1, "testtemplate", ['test']);
                    _$scope.$emit(pst.MessageType[pst.MessageType.PaneSelectTemplate_ProcessTemplateUpdated], incomingEventArgs);
                });
                */
                /*
                
                it("When PaneWorkflowDesigner_ActionSelected is sent and selectedAction!=null " +
                    "Save method should be called on ProcessTemplateService", () => {
                        var incomingEventArgs = new pwd.ActionSelectedEventArgs(1, 1, 1, 1);
                        var currentAction = new model.ActionDesignDTO(1, 1, false, 1);
                        _$scope.current.action = <any>currentAction;
        
                        _$scope.$emit(pwd.MessageType[pwd.MessageType.PaneWorkflowDesigner_ActionSelected], incomingEventArgs);
                        expect(_actionServiceMock.save).toHaveBeenCalledWith({ id: currentAction.id}, currentAction, null, null);
                    });
        
                it("When PaneWorkflowDesigner_ActionSelected is sent and selectedAction==null " +
                    "Save method on ProcessTemplateService should NOT be called", () => {
                        var incomingEventArgs = new pwd.CriteriaSelectedEventArgs(1, true);
        
                        _$scope.current.action = null;
        
                        _$scope.$emit(pwd.MessageType[pwd.MessageType.PaneWorkflowDesigner_ActionSelected], incomingEventArgs);
                        expect(_actionServiceMock.save).not.toHaveBeenCalled();
                    });
        
                it("When PaneWorkflowDesigner_ProcessNodeTemplateSelecting is sent and selectedAction!=null " +
                    "Save method should be called on ProcessTemplateService", () => {
                        var incomingEventArgs = new pwd.CriteriaSelectedEventArgs(1, true);
                        var currentAction = new model.ActionDesignDTO(1, 1, false, 1);
                        _$scope.current.action = <any>currentAction;
        
                        _$scope.$emit(pwd.MessageType[pwd.MessageType.PaneWorkflowDesigner_CriteriaSelected], incomingEventArgs);
                        expect(_actionServiceMock.save).toHaveBeenCalledWith({ id: currentAction.id }, currentAction, null, null);
                    });
        
                it("When PaneWorkflowDesigner_TemplateSelected is sent and selectedAction!=null " +
                    "Save method should be called on ProcessTemplateService", () => {
                        var incomingEventArgs = new pwd.TemplateSelectedEventArgs();
                        var currentAction = new model.ActionDesignDTO(1, 1, false, 1);
                        _$scope.current.action = <any>currentAction;
        
                        _$scope.$emit(pwd.MessageType[pwd.MessageType.PaneWorkflowDesigner_TemplateSelected], incomingEventArgs);
                        expect(_actionServiceMock.save).toHaveBeenCalledWith({ id: currentAction.id }, currentAction, null, null);
                    });
                */
            });
        })(controller = tests.controller || (tests.controller = {}));
    })(tests = dockyard.tests || (dockyard.tests = {}));
})(dockyard || (dockyard = {}));
//# sourceMappingURL=ProcessBuilderControllerTests.js.map