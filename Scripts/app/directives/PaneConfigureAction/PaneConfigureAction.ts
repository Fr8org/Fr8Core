/// <reference path="../../_all.ts" />
module dockyard.directives.paneConfigureAction {
    'use strict';

    export enum MessageType {
        PaneConfigureAction_ActionUpdated,
        PaneConfigureAction_ActionRemoved,
        PaneConfigureAction_Reconfigure,
        PaneConfigureAction_RenderConfiguration,
        PaneConfigureAction_ChildActionsDetected,
        PaneConfigureAction_ChildActionsReconfiguration,
        PaneConfigureAction_ReloadAction,
        PaneConfigureAction_SetSolutionMode
    }

    export class ActionReconfigureEventArgs {
        public action: interfaces.IActionDTO

        constructor(action: interfaces.IActionDTO) {
            // Clone Action to prevent any issues due to possible mutation of source object
            this.action = angular.extend({}, action);
        }
    }

    export class ActionUpdatedEventArgs extends ActionUpdatedEventArgsBase { }

    export class InternalAuthenticationArgs {
        public activityTemplateId: number;

        constructor(activityTemplateId: number) {
            this.activityTemplateId = activityTemplateId;
        }
    }

    export class ExternalAuthenticationArgs {
        public activityTemplateId: number;

        constructor(activityTemplateId: number) {
            this.activityTemplateId = activityTemplateId;
        }
    }

    export class RenderEventArgs {
        public action: interfaces.IActionDTO

        constructor(action: interfaces.IActionDTO) {
            // Clone Action to prevent any issues due to possible mutation of source object
            this.action = angular.extend({}, action);
        }
    }

    export class MapFieldsClickedEventArgs {
        action: model.ActionDTO;

        constructor(action: model.ActionDTO) {
            this.action = action;
        }
    }

    export class ActionRemovedEventArgs {
        public id: number;
        public isTempId: boolean;

        constructor(id: number, isTempId: boolean) {
            this.id = id;
            this.isTempId = isTempId;
        }
    }

    export interface IPaneConfigureActionScope extends ng.IScope {
        onConfigurationChanged: (newValue: model.ControlsList, oldValue: model.ControlsList) => void;
        onControlChange: (event: ng.IAngularEvent, eventArgs: ChangeEventArgs) => void;
        processConfiguration: () => void;
        loadConfiguration: () => void;
        currentAction: interfaces.IActionVM;
        configurationControls: ng.resource.IResource<model.ControlsList> | model.ControlsList;
        mapFields: (scope: IPaneConfigureActionScope) => void;
        processing: boolean;
        configurationWatchUnregisterer: Function;
        mode: string;
        reconfigureChildrenActions: boolean;
        setSolutionMode: () => void;
    }


    export class CancelledEventArgs extends CancelledEventArgsBase { }

    export class ReloadActionEventArgs {
        public action: interfaces.IActionDTO;
        constructor(action: interfaces.IActionDTO) {
            this.action = action;
        }
    }

    export class ChildActionReconfigurationEventArgs {
        public actions: Array<interfaces.IActionDTO>;
        constructor(actions: Array<interfaces.IActionDTO>) {
            this.actions = actions;
        }
    }

    //More detail on creating directives in TypeScript: 
    //http://blog.aaronholmes.net/writing-angularjs-directives-as-typescript-classes/
    class PaneConfigureAction implements ng.IDirective {
        public link: (scope: IPaneConfigureActionScope, element: ng.IAugmentedJQuery, attrs: ng.IAttributes) => void;
        public templateUrl = '/AngularTemplate/PaneConfigureAction';
        public controller: ($scope: IPaneConfigureActionScope, element: ng.IAugmentedJQuery, attrs: ng.IAttributes) => void;
        public scope = {
            currentAction: '=',
            mode: '='
        };
        public restrict = 'E';

        constructor(
            private ActionService: services.IActionService,
            private crateHelper: services.CrateHelper,
            private $filter: ng.IFilterService,
            private $timeout: ng.ITimeoutService,
            private $modal,
            private $window: ng.IWindowService,
            private $http: ng.IHttpService
        ) {
            PaneConfigureAction.prototype.link = (
                scope: IPaneConfigureActionScope,
                element: ng.IAugmentedJQuery,
                attrs: ng.IAttributes) => {

                //Link function goes here
            };

            PaneConfigureAction.prototype.controller = function (
                $scope: IPaneConfigureActionScope,
                $element: ng.IAugmentedJQuery,
                $attrs: ng.IAttributes) {

                $scope.$on("onChange", onControlChange);
                $scope.$on("onClick", onClickEvent);

                // These are exposed for unit testing.
                $scope.onControlChange = onControlChange;
                $scope.loadConfiguration = loadConfiguration;
                $scope.onConfigurationChanged = onConfigurationChanged;
                $scope.processConfiguration = processConfiguration;
                $scope.setSolutionMode = setSolutionMode;

                $scope.$on(MessageType[MessageType.PaneConfigureAction_Reconfigure], (event: ng.IAngularEvent, reConfigureActionEventArgs: ActionReconfigureEventArgs) => {
                    //this might be a general reconfigure command
                    //TODO there shouldn't be a general reconfigure command - we should check it's usage and remove it - note by bahadir
                    if (reConfigureActionEventArgs === null || typeof reConfigureActionEventArgs === 'undefined') {
                        loadConfiguration();
                        return;
                    }

                    if (reConfigureActionEventArgs.action.id === $scope.currentAction.id) {
                        loadConfiguration();
                    }

                });

                $scope.$on(MessageType[MessageType.PaneConfigureAction_ReloadAction], (event: ng.IAngularEvent, reloadActionEventArgs: ReloadActionEventArgs) => {
                    reloadAction(reloadActionEventArgs);
                });

                $scope.$on(MessageType[MessageType.PaneConfigureAction_RenderConfiguration],
                    //Allow some time for parent and current action instance to sync
                    () => $timeout(() => processConfiguration(), 300)
                );

                // Get configuration settings template from the server if the current action does not contain those             
                if ($scope.currentAction.activityTemplateId > 0) {
                    if ($scope.currentAction.crateStorage == null || !$scope.currentAction.crateStorage.crates.length) {
                        $scope.loadConfiguration();
                    } else {
                        $scope.processConfiguration();
                    }
                }

                function reloadAction(reloadActionEventArgs: ReloadActionEventArgs) {
                    //is this a reload call for me?
                    if (reloadActionEventArgs.action.id !== $scope.currentAction.id) {
                        return;
                    }
                    $scope.currentAction = <interfaces.IActionVM>reloadActionEventArgs.action;
                    $scope.processConfiguration();
                    if ($scope.currentAction.childrenActions
                        && $scope.currentAction.childrenActions.length > 0) {

                        if ($scope.reconfigureChildrenActions) {
                            $scope.$emit(MessageType[MessageType.PaneConfigureAction_ChildActionsReconfiguration], new ChildActionReconfigurationEventArgs($scope.currentAction.childrenActions));
                        }
                    }
                }

                function onConfigurationChanged(newValue: model.ControlsList, oldValue: model.ControlsList) {
                    if (!newValue || !newValue.fields) {
                         return;
                    }
                    crateHelper.mergeControlListCrate(
                        $scope.currentAction.configurationControls,
                        $scope.currentAction.crateStorage
                    );
                    $scope.currentAction.crateStorage.crateDTO = $scope.currentAction.crateStorage.crates; //backend expects crates on CrateDTO field
                    ActionService.save({ id: $scope.currentAction.id }, $scope.currentAction, null, null)
                        .$promise
                        .then(() => {
                            if ($scope.currentAction.childrenActions
                                && $scope.currentAction.childrenActions.length > 0) {

                                if ($scope.reconfigureChildrenActions) {
                                    $scope.$emit(MessageType[MessageType.PaneConfigureAction_ChildActionsReconfiguration], new ChildActionReconfigurationEventArgs($scope.currentAction.childrenActions));
                                }
                            }
                    });
                };

                function onControlChange(event: ng.IAngularEvent, eventArgs: ChangeEventArgs) {

                    var field = eventArgs.field;
                    if (field.events === null) return;
                    // Find the onChange event object
                    var eventHandlerList = <Array<model.ControlEvent>>$filter('filter')(field.events, { name: 'onChange' }, true);
                    if (eventHandlerList.length == 0) return;
                    var fieldEvent = eventHandlerList[0];

                    if (fieldEvent.handler === 'requestConfig') {
                        crateHelper.mergeControlListCrate(
                            $scope.currentAction.configurationControls,
                            $scope.currentAction.crateStorage
                        );
                        $scope.currentAction.crateStorage.crateDTO = $scope.currentAction.crateStorage.crates; //backend expects crates on CrateDTO field
                
                        $scope.loadConfiguration();
                    }
                    if (fieldEvent.handler === 'focusConfig') {
                        var cccc = $scope.currentAction.crateStorage;
                        angular.forEach(cccc.crates, function (value, keys) {
                            if (value.label == "Configuration_Controls") {
                                var textArea;
                                var dropDown;
                                var textAreaIndex;
                                angular.forEach(value.contents, function (val, key) {
                                    angular.forEach(val, function (i, j) {
                                        if (i.type == "TextArea") {
                                            textArea = i.value;
                                            textAreaIndex = j;
                                        }
                                        if (i.type == "DropDownList") {
                                            dropDown = i.selectedKey;
                                            val[textAreaIndex].value = val[textAreaIndex].value + '[' + dropDown + ']';
                                        }
                                    });
                                });
                            }
                        });
                        

                        crateHelper.mergeControlListCrate(
                            $scope.currentAction.configurationControls,
                            $scope.currentAction.crateStorage
                            );
                        $scope.currentAction.crateStorage.crateDTO = $scope.currentAction.crateStorage.crates //backend expects crates on CrateDTO field
                
                        $scope.loadConfiguration();
                    }
                }

                function onClickEvent(event: ng.IAngularEvent, eventArgs: ChangeEventArgs) {
                    var scope = <IPaneConfigureActionScope>event.currentScope;
                    var field = eventArgs.field;

                    // Find the onChange event object
                    var eventHandlerList = <Array<model.ControlEvent>>$filter('filter')(field.events, { name: 'onClick' }, true);
                    if (!eventHandlerList || eventHandlerList.length == 0) {
                        return;
                    }
                    else {
                        var fieldEvent = eventHandlerList[0];
                        if (fieldEvent.handler != null) {
                            crateHelper.mergeControlListCrate(
                                scope.currentAction.configurationControls,
                                scope.currentAction.crateStorage
                            );
                            scope.currentAction.crateStorage.crateDTO = scope.currentAction.crateStorage.crates; //backend expects crates on CrateDTO field
                            loadConfiguration();
                        }
                    }
                } 

                // Here we look for Crate with ManifestType == 'Standard UI Controls'.
                // We parse its contents and put it into currentAction.configurationControls structure.
                function loadConfiguration() {
                    // Block pane and show pane-level 'loading' spinner
                    $scope.processing = true;

                    if ($scope.configurationWatchUnregisterer) {
                        $scope.configurationWatchUnregisterer();
                    }

                    ActionService.configure($scope.currentAction).$promise
                        .then((res: interfaces.IActionVM) => {
                            if (res.childrenActions && res.childrenActions.length > 0) {
                                // If the directive is used for configuring solutions,
                                // the SolutionController would listen to this event 
                                // and redirect user to the RouteBuilder once if is received.
                                // It means that solution configuration is complete. 
                                $scope.$emit(MessageType[MessageType.PaneConfigureAction_ChildActionsDetected]);
                            }

                            $scope.reconfigureChildrenActions = false;

                            if ($scope.currentAction.childrenActions) {
                                if (angular.toJson($scope.currentAction.childrenActions) != angular.toJson(res.childrenActions)) {
                                    $scope.reconfigureChildrenActions = true;
                                }
                            }

                            $scope.currentAction.crateStorage = res.crateStorage;
                            $scope.currentAction.childrenActions = res.childrenActions;

                            $scope.processConfiguration();
                        })
                        .catch((result) => {
                            var errorText = 'Something went wrong. Click to retry.';
                            if (result.status && result.status >= 400) {
                                // Bad http response
                                errorText = 'Configuration loading error. Click to retry.';
                            } else if (result.message) {
                                // Exception was thrown in the code
                                errorText = result.message;
                            }
                            var control = new model.TextBlock(errorText, 'well well-lg alert-danger');
                            $scope.currentAction.configurationControls = new model.ControlsList();
                            $scope.currentAction.configurationControls.fields = [control];
                        })
                        .finally(() => {
                            // Unblock pane
                            $scope.processing = false;
                        });
                };

                function processConfiguration() {
                    // Check if authentication is required.
                    if (crateHelper.hasCrateOfManifestType($scope.currentAction.crateStorage, 'Standard Authentication')) {
                        var authCrate = crateHelper
                            .findByManifestType($scope.currentAction.crateStorage, 'Standard Authentication');

                        startAuthentication($scope.currentAction.id);

                        // TODO: remove this.
                        // var authMS = <any>authCrate.contents;

                        // TODO: remove this.
                        // // Dockyard auth mode.
                        // if (authMS.Mode == 1 || authMS.Mode == 3) {
                        //     startInternalAuthentication($scope.currentAction.id, authMS.Mode);
                        // }
                        // 
                        // // External auth mode.                           
                        // else {
                        //     // self.$window.open(authMS.Url, '', 'width=400, height=500, location=no, status=no');
                        //     startExternalAuthentication($scope.currentAction.id);
                        // }
                    }

                    $scope.currentAction.configurationControls =
                        crateHelper.createControlListFromCrateStorage($scope.currentAction.crateStorage);

                    $timeout(() => { // let the control list create, we don't want false change notification during creation process
                        $scope.configurationWatchUnregisterer = $scope.$watch<model.ControlsList>(
                            (scope: IPaneConfigureActionScope) => $scope.currentAction.configurationControls,
                            $scope.onConfigurationChanged,
                            true);
                    }, 1000);
                }

                function startAuthentication(actionId: string) {
                    var modalScope = <any>$scope.$new(true);
                    modalScope.actionIds = [actionId];

                    $modal.open({
                        animation: true,
                        templateUrl: '/AngularTemplate/AuthenticationDialog',
                        controller: 'AuthenticationDialogController',
                        scope: modalScope
                    })
                    .result
                    .then(() => loadConfiguration());
                }

                // TODO: remove this.
                // function startInternalAuthentication(actionId: string, mode: number) {
                //     var self = this;
                // 
                //     var modalScope = <any>$scope.$new(true);
                //     modalScope.actionId = actionId;
                //     modalScope.mode = mode;
                //     
                //     $modal.open({
                //         animation: true,
                //         templateUrl: '/AngularTemplate/InternalAuthentication',
                //         controller: 'InternalAuthenticationController',
                //         scope: modalScope
                //     })
                //     .result
                //     .then(() => loadConfiguration());
                // }

                // TODO: remove this.
                // function startExternalAuthentication(actionId: string) {
                //     var self = this;
                //     var childWindow;
                // 
                //     var messageListener = function (event) {
                //         if (!event.data || event.data != 'external-auth-success') {
                //             return;
                //         }
                // 
                //         childWindow.close();
                //         loadConfiguration();
                //     };
                // 
                //     $http
                //         .get('/api/authentication/initial_url?id=' + actionId)
                //         .then(res => {
                //             var url = (<any>res.data).url;
                //             childWindow = $window.open(url, 'AuthWindow', 'width=400, height=500, location=no, status=no');
                //             window.addEventListener('message', messageListener);
                // 
                //             var isClosedHandler = function () {
                //                 if (childWindow.closed) {
                //                     window.removeEventListener('message', messageListener);
                //                 }
                //                 else {
                //                     setTimeout(isClosedHandler, 500);
                //                 }
                //             };
                //             setTimeout(isClosedHandler, 500);
                //         });
                // }

                function setSolutionMode() {
                    $scope.$emit(MessageType[MessageType.PaneConfigureAction_SetSolutionMode]);

                }
            }
        }    

        //The factory function returns Directive object as per Angular requirements
        public static Factory() {
            var directive = (
                ActionService,
                crateHelper: services.CrateHelper,
                $filter: ng.IFilterService,
                $timeout: ng.ITimeoutService,
                $modal,
                $window: ng.IWindowService,
                $http: ng.IHttpService
            ) => {

                return new PaneConfigureAction(ActionService, crateHelper, $filter, $timeout, $modal, $window, $http);
            };

            directive['$inject'] = ['ActionService', 'CrateHelper', '$filter', '$timeout', '$modal', '$window', '$http'];
            return directive;
        }
    }
    app.directive('paneConfigureAction', PaneConfigureAction.Factory());
}