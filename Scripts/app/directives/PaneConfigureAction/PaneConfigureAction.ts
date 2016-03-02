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
        PaneConfigureAction_SetSolutionMode,
        PaneConfigureAction_ConfigureCallResponse,
        PaneConfigureAction_AuthFailure,
        PaneConfigureAction_ExecutePlan,
        PaneConfigureAction_ConfigureFocusElement,
        PaneConfigureAction_AuthCompleted,
        PaneConfigureAction_DownStreamReconfiguration
    }

    export class ActionReconfigureEventArgs {
        public action: interfaces.IActivityDTO

        constructor(action: interfaces.IActivityDTO) {
            // Clone Action to prevent any issues due to possible mutation of source object
            this.action = angular.extend({}, action);
        }
    }

    export class AuthenticationCompletedEventArgs extends ActionReconfigureEventArgs{
    }

    export class DownStreamReConfigureEventArgs extends ActionReconfigureEventArgs {
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

    export class ConfigureFocusElementArgs {
        public fieldName: model.ControlDefinitionDTO;

        constructor(fieldName: model.ControlDefinitionDTO) {
            this.fieldName = fieldName;
        }
    }

    export class RenderEventArgs {
        public action: interfaces.IActivityDTO

        constructor(action: interfaces.IActivityDTO) {
            // Clone Action to prevent any issues due to possible mutation of source object
            this.action = angular.extend({}, action);
        }
    }

    export class MapFieldsClickedEventArgs {
        action: model.ActivityDTO;

        constructor(action: model.ActivityDTO) {
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

    export class ActionAuthFailureEventArgs {
        public id: string;

        constructor(id: string) {
            this.id = id;
        }
    }

    export interface IPaneConfigureActionScope extends ng.IScope {
        onConfigurationChanged: (newValue: model.ControlsList, oldValue: model.ControlsList) => void;
        onControlChange: (event: ng.IAngularEvent, eventArgs: ChangeEventArgs) => void;
        processConfiguration: () => void;
        loadConfiguration: () => void;
        reloadConfiguration: () => void;
        currentAction: interfaces.IActionVM;
        configurationControls: ng.resource.IResource<model.ControlsList> | model.ControlsList;
        mapFields: (scope: IPaneConfigureActionScope) => void;
        processing: boolean;
        configurationWatchUnregisterer: Function;
        mode: string;
        reconfigureChildrenActions: boolean;
        setSolutionMode: () => void;
        currentActiveElement: model.ControlDefinitionDTO;
        collapsed: boolean;
    }
    
    export class CancelledEventArgs extends CancelledEventArgsBase { }

    export class ReloadActionEventArgs {
        public action: interfaces.IActivityDTO;
        constructor(action: interfaces.IActivityDTO) {
            this.action = action;
        }
    }

    export class ChildActionReconfigurationEventArgs {
        public actions: Array<interfaces.IActivityDTO>;
        constructor(actions: Array<interfaces.IActivityDTO>) {
            this.actions = actions;
        }
    }

    export class CallConfigureResponseEventArgs {
        public action: interfaces.IActivityDTO;
        public focusElement: model.ControlDefinitionDTO;
        constructor(action: interfaces.IActivityDTO, focusElement: model.ControlDefinitionDTO) {
            this.action = action;
            this.focusElement = focusElement;
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

        private ignoreConfigurationChange = false;
         
        constructor(
            private ActionService: services.IActionService,
            private AuthService: services.AuthService,
            private ConfigureTrackerService: services.ConfigureTrackerService,
            private crateHelper: services.CrateHelper,
            private $filter: ng.IFilterService,
            private $timeout: ng.ITimeoutService,
            private $modal,
            private $window: ng.IWindowService,
            private $http: ng.IHttpService,
            private $q: ng.IQService
        ) {
            PaneConfigureAction.prototype.link = (
                scope: IPaneConfigureActionScope,
                element: ng.IAugmentedJQuery,
                attrs: ng.IAttributes) => {

                //Link function goes here
            };
            
            PaneConfigureAction.prototype.controller = ($scope: IPaneConfigureActionScope, $element: ng.IAugmentedJQuery, $attrs: ng.IAttributes) => {

                var configLoadingError: boolean = false;
                $scope.collapsed = false;

                $scope.$on("onChange", onControlChange);
                $scope.$on("onClick", onClickEvent);

                // These are exposed for unit testing.
                $scope.onControlChange = onControlChange;
                $scope.loadConfiguration = loadConfiguration;
                $scope.reloadConfiguration = reloadConfiguration;
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

                $scope.$on(MessageType[MessageType.PaneConfigureAction_AuthCompleted], (event: ng.IAngularEvent, authCompletedEventArgs: AuthenticationCompletedEventArgs) => {
                    if (authCompletedEventArgs.action.id === $scope.currentAction.id) {
                        loadConfiguration().then(() => {
                            $scope.$emit(MessageType[MessageType.PaneConfigureAction_DownStreamReconfiguration], new DownStreamReConfigureEventArgs($scope.currentAction));
                        });
                    }

                });

                $scope.$on(MessageType[MessageType.PaneConfigureAction_ReloadAction], (event: ng.IAngularEvent, reloadActionEventArgs: ReloadActionEventArgs) => {
                    reloadAction(reloadActionEventArgs);
                });

                $scope.$on(MessageType[MessageType.PaneConfigureAction_ConfigureFocusElement], (event: ng.IAngularEvent, configureFocusElementArgs: ConfigureFocusElementArgs) => {
                    $scope.currentActiveElement = configureFocusElementArgs.fieldName;
                });

                $scope.$on(MessageType[MessageType.PaneConfigureAction_RenderConfiguration],
                    //Allow some time for parent and current action instance to sync
                    () => $timeout(() => processConfiguration(), 300)
                );

                $scope.$on(
                    MessageType[MessageType.PaneConfigureAction_AuthFailure],
                    (event: ng.IAngularEvent, authFailureArgs: ActionAuthFailureEventArgs) => {
                        if (authFailureArgs.id != $scope.currentAction.id) {
                            return;
                        }

                        var onClickEvent = new model.ControlEvent();
                        onClickEvent.name = 'onClick';
                        onClickEvent.handler = 'requestConfig';

                        var button = new model.Button('Authentication unsuccessful, try again');
                        button.name = 'AuthUnsuccessfulLabel';
                        button.events = [onClickEvent];

                        $scope.currentAction.configurationControls = new model.ControlsList();
                        $scope.currentAction.configurationControls.fields = [button];
                    }
                );

                // Get configuration settings template from the server if the current action does not contain those       
                //TODO check this     
                if ($scope.currentAction.activityTemplate != null) {
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
                    if ($scope.currentAction.childrenActivities
                        && $scope.currentAction.childrenActivities.length > 0) {

                        if ($scope.reconfigureChildrenActions) {
                            $scope.$emit(MessageType[MessageType.PaneConfigureAction_ChildActionsReconfiguration], new ChildActionReconfigurationEventArgs($scope.currentAction.childrenActivities));
                        }
                    }
                }

                // The function compares two instances of a configuration control and 
                // determines if user's selection or entered value has changed 
                function controlValuesChanged(control1: model.ControlDefinitionDTO, control2: model.ControlDefinitionDTO) {
                    if (control1.name != control2.name) {
                        throw Error("Control1 and control2 represent different controls.");
                    }

                    if (control1.value != undefined
                        && control1.value != control2.value)
                        return true;

                    if ((<model.CheckBox>control1).selected != undefined
                        && (<model.CheckBox>control1).selected != (<model.CheckBox>control2).selected)
                        return true;

                    if ((<model.DropDownList>control1).selectedKey != undefined
                        && (<model.DropDownList>control1).selectedKey != (<model.DropDownList>control2).selectedKey)
                        return true;

                    if ((<model.TextSource>control1).valueSource != undefined
                        && (<model.TextSource>control1).valueSource != (<model.TextSource>control2).valueSource)
                        return true;

                    return false;
                }

                function onConfigurationChanged(newValue: model.ControlsList, oldValue: model.ControlsList) {
                    if (!newValue || !newValue.fields) {
                        return;
                    }

                    if (this.ignoreConfigurationChange) {
                        this.ignoreConfigurationChange = false;
                        return;
                    }

                    for (var i = 0; i < newValue.fields.length; i++) {
                        if (!controlValuesChanged(newValue.fields[i], oldValue.fields[i])) {
                            continue;
                        }

                        if (hasRequestConfigHandler(newValue.fields[i])) {
                            // Don't need to save separately; requestConfig event handler will initiate reconfiguration
                            // which will also save the action
                            return;
                        }
                    }

                    crateHelper.mergeControlListCrate(
                        $scope.currentAction.configurationControls,
                        $scope.currentAction.crateStorage
                    );
                    $scope.currentAction.crateStorage.crateDTO = $scope.currentAction.crateStorage.crates; //backend expects crates on CrateDTO field
                    ActionService.save({ id: $scope.currentAction.id }, $scope.currentAction, null, null)
                        .$promise
                        .then(() => {
                            if ($scope.currentAction.childrenActivities
                                && $scope.currentAction.childrenActivities.length > 0) {

                                if ($scope.reconfigureChildrenActions) {
                                    $scope.$emit(MessageType[MessageType.PaneConfigureAction_ChildActionsReconfiguration], new ChildActionReconfigurationEventArgs($scope.currentAction.childrenActivities));
                                }
                            }
                        });
                };

                function getControlEventHandler(control: model.ControlDefinitionDTO, eventName: string) {
                    if (control.events === null) return;

                    var eventHandlerList = <Array<model.ControlEvent>>$filter('filter')(control.events, { name: eventName }, true);
                    if (typeof eventHandlerList === 'undefined' || eventHandlerList === null || eventHandlerList.length === 0) {
                        return null;
                    }
                    else {
                        return eventHandlerList[0].handler;
                    }
                }

                function hasRequestConfigHandler(control: model.ControlDefinitionDTO): boolean {
                    var handler = getControlEventHandler(control, 'onChange');
                    if (handler != null) {
                        return handler == 'requestConfig';
                    }
                    else
                        return false; 
                }

                function onControlChange(event: ng.IAngularEvent, eventArgs: ChangeEventArgs) {
                    if (hasRequestConfigHandler(eventArgs.field)) {
                        crateHelper.mergeControlListCrate(
                            $scope.currentAction.configurationControls,
                            $scope.currentAction.crateStorage
                        );
                        $scope.currentAction.crateStorage.crateDTO = $scope.currentAction.crateStorage.crates; //backend expects crates on CrateDTO field
                
                        $scope.loadConfiguration();
                    }
                }

                function onClickEvent(event: ng.IAngularEvent, eventArgs: ChangeEventArgs) {
                    var scope = <IPaneConfigureActionScope>event.currentScope;

                    // Find the onClick event object
                    if (getControlEventHandler(eventArgs.field, 'onClick')) {
                        crateHelper.mergeControlListCrate(
                            scope.currentAction.configurationControls,
                            scope.currentAction.crateStorage
                        );
                        scope.currentAction.crateStorage.crateDTO = scope.currentAction.crateStorage.crates; //backend expects crates on CrateDTO field
                        loadConfiguration();
                    }
                }

                //only load configuration if there has been a configuration loading error
                function reloadConfiguration() {
                    if (configLoadingError) {
                        loadConfiguration();
                    }
                }

                // Here we look for Crate with ManifestType == 'Standard UI Controls'.
                // We parse its contents and put it into currentAction.configurationControls structure.
                function loadConfiguration() {
                    var deferred = $q.defer();
                    // Block pane and show pane-level 'loading' spinner
                    $scope.processing = true;

                    if ($scope.configurationWatchUnregisterer) {
                        $scope.configurationWatchUnregisterer();
                    }

                    ConfigureTrackerService.configureCallStarted(
                        $scope.currentAction.id,
                        $scope.currentAction.activityTemplate.needsAuthentication
                    );

                    ActionService.configure($scope.currentAction).$promise
                        .then((res: interfaces.IActionVM) => {
                            var childActionsDetected = false;

                            // Detect OperationalState crate with CurrentClientActionName = 'RunImmediately'.
                            if (crateHelper.hasCrateOfManifestType(res.crateStorage, 'Operational State')) {
                                var operationalStatus = crateHelper
                                    .findByManifestType(res.crateStorage, 'Operational State');

                                var contents = <any>operationalStatus.contents;

                                if (contents.CurrentActivityResponse.type === 'ExecuteClientActivity'
                                    && (contents.CurrentClientActivityName === 'RunImmediately')
                                    ) {

                                    $scope.$emit(MessageType[MessageType.PaneConfigureAction_ExecutePlan]);
                                }
                            }

                            var oldAction = $scope.currentAction;
                            if (res.childrenActivities && res.childrenActivities.length > 0 && (!oldAction.childrenActivities || oldAction.childrenActivities.length < 1)) {
                                // If the directive is used for configuring solutions,
                                // the SolutionController would listen to this event 
                                // and redirect user to the RouteBuilder once if is received.
                                // It means that solution configuration is complete.
                                
                                // not needed in case of Loop action reconfigure
                                
                                $scope.$emit(MessageType[MessageType.PaneConfigureAction_ChildActionsDetected]);

                                childActionsDetected = true;
                            }

                            $scope.reconfigureChildrenActions = false;

                            if ($scope.currentAction.childrenActivities) {
                                if (angular.toJson($scope.currentAction.childrenActivities) != angular.toJson(res.childrenActivities)) {
                                    $scope.reconfigureChildrenActions = true;
                                    //in case of reconfiguring the solution check the child actions again

                                    //not needed in case of Loop action
                                    if ($scope.currentAction.label !== "Loop") {
                                        $scope.$emit(MessageType[MessageType.PaneConfigureAction_ChildActionsDetected]);
                                    }
                                }
                            }

                            $scope.currentAction.crateStorage = res.crateStorage;
                            $scope.currentAction.childrenActivities = res.childrenActivities;

                            $scope.processConfiguration();
                            configLoadingError = false;

                            // Unblock pane.
                            if (!childActionsDetected) {
                                $scope.processing = false;
                            }

                            deferred.resolve($scope.currentAction);
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
                            configLoadingError = true;

                            // Unblock pane.
                            $scope.processing = false;
                            deferred.reject(result);
                        })
                        .finally(() => {
                            ConfigureTrackerService.configureCallFinished($scope.currentAction.id);
                            // emit ConfigureCallResponse for RouteBuilderController be able to reload actions with AgressiveReloadTag
                            $scope.$emit(MessageType[MessageType.PaneConfigureAction_ConfigureCallResponse], new CallConfigureResponseEventArgs($scope.currentAction, $scope.currentActiveElement));
                        });

                    return deferred.promise;
                };

                function processConfiguration() {
                    var that = this;
                    // Check if authentication is required.
                    if (crateHelper.hasCrateOfManifestType($scope.currentAction.crateStorage, 'Standard Authentication')) {
                        var authCrate = crateHelper
                            .findByManifestType($scope.currentAction.crateStorage, 'Standard Authentication');

                        // startAuthentication($scope.currentAction.id);
                        AuthService.enqueue($scope.currentAction.id);
                    }

                    // if (crateHelper.hasCrateOfManifestType(

                    $scope.currentAction.configurationControls =
                        crateHelper.createControlListFromCrateStorage($scope.currentAction.crateStorage);

                    // Before setting up watcher on configuration change, make sure that the first invokation of the handler 
                    // is ignored: watcher always triggers after having been set up, and we don't want to handle that 
                    // useless call.
                    this.ignoreConfigurationChange = true;

                    $timeout(() => { // let the control list create, we don't want false change notification during creation process
                        $scope.configurationWatchUnregisterer = $scope.$watch<model.ControlsList>(
                            (scope: IPaneConfigureActionScope) => $scope.currentAction.configurationControls,
                            <any>angular.bind(that, $scope.onConfigurationChanged),
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
                        .then(() => loadConfiguration())
                        .catch((result) => {
                            var errorText = 'Authentication unsuccessful. Click to try again.';
                            var control = new model.TextBlock(errorText, 'well well-lg alert-danger');
                            control.name = 'AuthUnsuccessfulLabel';
                            $scope.currentAction.configurationControls = new model.ControlsList();
                            $scope.currentAction.configurationControls.fields = [control];
                        });
                }

                function setSolutionMode() {
                    $scope.$emit(MessageType[MessageType.PaneConfigureAction_SetSolutionMode]);
                }
            };

            PaneConfigureAction.prototype.controller['$inject'] = ['$scope', '$element', '$attrs'];
        }    

        //The factory function returns Directive object as per Angular requirements
        public static Factory() {
            var directive = (
                ActionService,
                AuthService,
                ConfigureTrackerService,
                crateHelper: services.CrateHelper,
                $filter: ng.IFilterService,
                $timeout: ng.ITimeoutService,
                $modal,
                $window: ng.IWindowService,
                $http: ng.IHttpService,
                $q: ng.IQService

            ) => {

                return new PaneConfigureAction(
                    ActionService,
                    AuthService,
                    ConfigureTrackerService,
                    crateHelper,
                    $filter,
                    $timeout,
                    $modal,
                    $window,
                    $http,
                    $q
                );
            };

            directive['$inject'] = [
                'ActionService',
                'AuthService',
                'ConfigureTrackerService',
                'CrateHelper',
                '$filter',
                '$timeout',
                '$modal',
                '$window',
                '$http',
                '$q'
            ];
            return directive;
        }
    }
    app.directive('paneConfigureAction', PaneConfigureAction.Factory());
}