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
        PaneConfigureAction_ConfigureCallResponseFinished,
        PaneConfigureAction_AuthFailure,
        PaneConfigureAction_ExecutePlan,
        PaneConfigureAction_ConfigureFocusElement,
        PaneConfigureAction_AuthCompleted,
        PaneConfigureAction_DownStreamReconfiguration,
        PaneConfigureAction_UpdateValidationMessages,
        PaneConfigureAction_ResetValidationMessages,
        PaneConfigureAction_ShowAdvisoryMessages,
        PaneConfigureAction_ConfigureStarting
    }

    export class ActionReconfigureEventArgs {
        public action: interfaces.IActivityDTO;

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

    export class ResetValidationMessagesEventArgs {
    }

    export class UpdateValidationMessagesEventArgs {
        public id: string;
        public validationResults: model.ValidationResults;

        constructor(id: string, validationResults: model.ValidationResults) {
            this.id = id;
            this.validationResults = validationResults;
        }
    }
    
    export class ShowAdvisoryMessagesEventArgs {
        public id: string;
        public advisories: model.AdvisoryMessages;

        constructor(id: string, advisories: model.AdvisoryMessages) {
            this.id = id;
            this.advisories = advisories;
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
        isConfigRequestQueued: boolean;
        configControlOperationQueue: { [controlName: string]: Array<paneConfigureAction.ConfigurationControlOperation>; }
        configControlHandles: { [controlName: string]: paneConfigureAction.IConfigurationControlController; };
        configurationWatchUnregisterer: Function;
        mode: string;
        reconfigureChildrenActions: boolean;
        setSolutionMode: () => void;
        currentActiveElement: model.ControlDefinitionDTO;
        collapsed: boolean;
        populateAllActivities: () => void;
        allActivities: Array<model.ActivityEnvelope>;
        view: string;
        plan: model.PlanDTO;
        showAdvisoryPopup: boolean;
        myActivityTemplate: interfaces.IActivityTemplateVM;
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

    export interface IPaneConfigureActionController {
        setJumpTargets: (targets: Array<model.ActivityJumpTarget>) => void;
        isThereOnGoingConfigRequest: () => boolean;
        queueOperation: (controlName: string, operation: paneConfigureAction.ConfigurationControlOperation) => void;
        registerControl: (controlName: string, control: paneConfigureAction.IConfigurationControlController) => void;
    }

    export class PaneConfigureActionController implements IPaneConfigureActionController {

        static $inject = ['$scope', 'ActivityService', 'AuthService', 'ConfigureTrackerService', 'CrateHelper', '$filter',
            '$timeout', '$modal', '$window', '$http', '$q', 'LayoutService', 'ActivityTemplateHelperService'];

        private configLoadingError: boolean = false;
        private ignoreConfigurationChange: boolean = false;

        constructor(private $scope: IPaneConfigureActionScope, private ActivityService: services.IActivityService,
            private AuthService: services.AuthService, private ConfigureTrackerService: services.ConfigureTrackerService,
            private crateHelper: services.CrateHelper, private $filter: ng.IFilterService,
            private $timeout: ng.ITimeoutService, private $modal,
            private $window: ng.IWindowService, private $http: ng.IHttpService,
            private $q: ng.IQService, private LayoutService: services.ILayoutService,
            private ActivityTemplateHelperService: services.IActivityTemplateHelperService)
        {
            $scope.myActivityTemplate = this.ActivityTemplateHelperService.getActivityTemplate($scope.currentAction);
            $scope.collapsed = false;
            $scope.showAdvisoryPopup = false;
            $scope.isConfigRequestQueued = false;
            $scope.configControlOperationQueue = {};
            $scope.configControlHandles = {};
            

            $scope.$on("onChange", <() => void>angular.bind(this, this.onControlChange));
            $scope.$on("onClick", <() => void>angular.bind(this, this.onClickEvent));

            // These are exposed for unit testing.
            $scope.onControlChange = <() => void> angular.bind(this, this.onControlChange);
            $scope.loadConfiguration = <() => void>angular.bind(this, this.loadConfiguration);
            $scope.reloadConfiguration = <() => void>angular.bind(this, this.reloadConfiguration);
            $scope.onConfigurationChanged = <() => void>angular.bind(this, this.onConfigurationChanged);
            $scope.processConfiguration = <() => void>angular.bind(this, this.processConfiguration);
            $scope.setSolutionMode = <() => void>angular.bind(this, this.setSolutionMode);
            $scope.populateAllActivities = <() => void>angular.bind(this, this.populateAllActivities);
            $scope.allActivities = Array<model.ActivityEnvelope>();

            $scope.$on(MessageType[MessageType.PaneConfigureAction_Reconfigure], (event: ng.IAngularEvent, reConfigureActionEventArgs: ActionReconfigureEventArgs) => {
                //this might be a general reconfigure command
                //TODO there shouldn't be a general reconfigure command - we should check it's usage and remove it - note by bahadir
                if (reConfigureActionEventArgs === null || typeof reConfigureActionEventArgs === 'undefined') {
                    this.loadConfiguration();
                    return;
                }

                if (reConfigureActionEventArgs.action.id === $scope.currentAction.id) {
                    this.loadConfiguration();
                }
            });

            $scope.$on(MessageType[MessageType.PaneConfigureAction_AuthCompleted], (event: ng.IAngularEvent, authCompletedEventArgs: AuthenticationCompletedEventArgs) => {
                if (authCompletedEventArgs.action.id === $scope.currentAction.id) {
                    this.loadConfiguration().then(() => {
                        var authCrate = this.crateHelper.findByManifestType(
                            this.$scope.currentAction.crateStorage,
                            'Standard Authentication',
                            true
                        );

                        if (!authCrate || !authCrate.contents || !(<any>authCrate.contents).Revocation) {
                            $scope.$emit(MessageType[MessageType.PaneConfigureAction_DownStreamReconfiguration], new DownStreamReConfigureEventArgs($scope.currentAction));
                        }
                    });
                }
            });

            $scope.$on(MessageType[MessageType.PaneConfigureAction_ResetValidationMessages], (event: ng.IAngularEvent, e: ResetValidationMessagesEventArgs) => {
                this.ignoreConfigurationChange = true;
                crateHelper.resetValidationErrors($scope.currentAction.configurationControls.fields);
            });

            $scope.$on(MessageType[MessageType.PaneConfigureAction_UpdateValidationMessages], (event: ng.IAngularEvent, e: UpdateValidationMessagesEventArgs) => {
                if (e.id === $scope.currentAction.id) {
                    crateHelper.setValidationErrors($scope.currentAction.configurationControls.fields, e.validationResults);
                }
            });

            $scope.$on(MessageType[MessageType.PaneConfigureAction_ReloadAction], (event: ng.IAngularEvent, reloadActionEventArgs: ReloadActionEventArgs) => {
                this.reloadAction(reloadActionEventArgs);
            });

            $scope.$on(MessageType[MessageType.PaneConfigureAction_ConfigureFocusElement], (event: ng.IAngularEvent, configureFocusElementArgs: ConfigureFocusElementArgs) => {
                $scope.currentActiveElement = configureFocusElementArgs.fieldName;
            });

            $scope.$on(MessageType[MessageType.PaneConfigureAction_RenderConfiguration],
                //Allow some time for parent and current action instance to sync
                () => $timeout(() => this.processConfiguration(), 300)
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
                    button.name = 'AuthUnsuccessfulButton';
                    button.events = [onClickEvent];

                    $scope.currentAction.configurationControls = new model.ControlsList();
                    $scope.currentAction.configurationControls.fields = [button];
                }
            );

            // Get configuration settings template from the server if the current action does not contain those       
            // TODO check this     
            if ($scope.currentAction.activityTemplate != null) {
                if ($scope.currentAction.crateStorage == null || !$scope.currentAction.crateStorage.crates.length) {
                    $scope.loadConfiguration();
                } else {
                    $scope.processConfiguration();
                }
            }

            
        }

        public isThereOnGoingConfigRequest(): boolean {
            return this.$scope.isConfigRequestQueued;
        }

        public queueOperation(controlName: string, operation: paneConfigureAction.ConfigurationControlOperation): void {
            if (!this.$scope.configControlOperationQueue[controlName]) {
                this.$scope.configControlOperationQueue[controlName] = [];
            }
            this.$scope.configControlOperationQueue[controlName].push(operation);
        }

        private reloadAction (reloadActionEventArgs: ReloadActionEventArgs): void {
            // Checks whether this reload call for current action
            if (reloadActionEventArgs.action.id !== this.$scope.currentAction.id) {
                return;
            }
            this.$scope.currentAction = <interfaces.IActionVM>reloadActionEventArgs.action;
            this.$scope.loadConfiguration();
            if (this.$scope.currentAction.childrenActivities && this.$scope.currentAction.childrenActivities.length > 0) {
                if (this.$scope.reconfigureChildrenActions) {
                    this.$scope.$emit(MessageType[MessageType.PaneConfigureAction_ChildActionsReconfiguration],
                        new ChildActionReconfigurationEventArgs(this.$scope.currentAction.childrenActivities));
                }
            }
        }

        // The function compares two instances of a configuration control and 
        // determines if user's selection or entered value has changed 
        private controlValuesChanged(control1: model.ControlDefinitionDTO, control2: model.ControlDefinitionDTO) {
                if (control1.name != control2.name) {
                    throw Error("Control1 and control2 represent different controls.");
                }

                if (control1.value != undefined && control1.value != control2.value)
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

        private onConfigurationChanged(newValue: model.ControlsList, oldValue: model.ControlsList) {

            if (this.crateHelper.hasControlListCrate(this.$scope.currentAction.crateStorage)) {
                this.crateHelper.mergeControlListCrate(
                this.$scope.currentAction.configurationControls,
                this.$scope.currentAction.crateStorage,
                this.$scope.view
                );
            }

                this.$scope.currentAction.crateStorage.crateDTO = this.$scope.currentAction.crateStorage.crates; //backend expects crates on CrateDTO field

                this.ActivityService.save(this.$scope.currentAction)
                    .then(() => {
                        
                        if (this.$scope.currentAction.childrenActivities
                            && this.$scope.currentAction.childrenActivities.length > 0) {

                            if (this.$scope.reconfigureChildrenActions) {
                                this.$scope.$emit(MessageType[MessageType.PaneConfigureAction_ChildActionsReconfiguration], new ChildActionReconfigurationEventArgs(this.$scope.currentAction.childrenActivities));
                            }
                        }
                    });
                
                // save request will stop running plans, so FE should know that
                // commented out because of FR-4352, now running plan locks activities configuration
                //if (this.$scope.plan.planState === model.PlanState.Executing) {
                //    this.$scope.plan.planState = model.PlanState.Inactive;
                //}

                // the save request is sent, so we can run the plan
                if (this.$scope.plan.planState === model.PlanState.Saving_Changes) {
                    this.$scope.plan.planState = model.PlanState.Inactive;
                }
            }

        private getControlEventHandler(control: model.ControlDefinitionDTO, eventName: string) {
            if (control.events === null) return;

            var eventHandlerList = <Array<model.ControlEvent>>this.$filter('filter')(control.events, { name: eventName }, true);
            if (typeof eventHandlerList === 'undefined' || eventHandlerList === null || eventHandlerList.length === 0) {
                return null;
            } else {
                return eventHandlerList[0].handler;
            }
        }

        private hasRequestConfigHandler(control: model.ControlDefinitionDTO): boolean {
            var handler = this.getControlEventHandler(control, 'onChange');
            if (handler != null) {
                return handler == 'requestConfig';
            } else
                return false;
        }

        private onControlChange(event: ng.IAngularEvent, eventArgs: ChangeEventArgs) {
            if (this.hasRequestConfigHandler(eventArgs.field)) {
                if (this.crateHelper.hasControlListCrate(this.$scope.currentAction.crateStorage)) {
                    this.crateHelper.mergeControlListCrate(
                        this.$scope.currentAction.configurationControls,
                        this.$scope.currentAction.crateStorage,
                        this.$scope.view
                    );
                }

                this.$scope.currentAction.crateStorage.crateDTO = this.$scope.currentAction.crateStorage.crates; //backend expects crates on CrateDTO field
                var that = this;
                this.$scope.isConfigRequestQueued = true;
                //this delay lets us to process other events before this config request
                //for example a user might type some text to textfield and click a button
                //before triggering change on textfield we should queue click handler
                //with this delay we give them a chance to handle this
                this.$timeout(that.$scope.loadConfiguration, 100);
            }
        }

        private onClickEvent(event: ng.IAngularEvent, eventArgs: ChangeEventArgs) {
            var scope = <IPaneConfigureActionScope>event.currentScope;

            // Find the onClick event object
            if (this.getControlEventHandler(eventArgs.field, 'onClick')) {
                if (eventArgs.field.name === 'AuthUnsuccessfulButton') {
                    this.AuthService.enableAuthentication((<IPaneConfigureActionScope>event.currentScope).currentAction.id);
                }

                if (this.crateHelper.hasControlListCrate(scope.currentAction.crateStorage)) {
                    this.crateHelper.mergeControlListCrate(
                        scope.currentAction.configurationControls,
                        scope.currentAction.crateStorage, scope.view
                    );
                }

                scope.currentAction.crateStorage.crateDTO = scope.currentAction.crateStorage.crates; //backend expects crates on CrateDTO field

                this.loadConfiguration();

                // FR-2488, added by yakov.gnusin.
                // Fixing save/ configure race condition on Continue button click (reproduced in MM solution).
                this.ignoreConfigurationChange = true;
            }
        }

        //only load configuration if there has been a configuration loading error
        private reloadConfiguration() {
            if (this.configLoadingError) {
                this.loadConfiguration();
            }
        }

        private allActivities = Array<model.ActivityEnvelope>();

        private getAllActivities(activities: Array<interfaces.IActivityDTO>) {
            for (var activity of activities) {
                var at = this.ActivityTemplateHelperService.getActivityTemplate(activity);
                this.allActivities.push(new model.ActivityEnvelope(activity, at));
                if (activity.childrenActivities.length > 0) {
                    this.getAllActivities(activity.childrenActivities);
                }
            }
        }

        private populateAllActivities() {
            this.getAllActivities(this.$scope.currentAction.childrenActivities);
            this.$scope.allActivities = this.allActivities;
        }

        // Here we look for Crate with ManifestType == 'Standard UI Controls'.
        // We parse its contents and put it into currentAction.configurationControls structure.
        private loadConfiguration() {

            var deferred = this.$q.defer();
            // Block pane and show pane-level 'loading' spinner
            this.$scope.processing = true;

            if (this.$scope.configurationWatchUnregisterer) {
                this.$scope.configurationWatchUnregisterer();
            }
            
            this.ConfigureTrackerService.configureCallStarted(
                this.$scope.currentAction.id,
                this.$scope.myActivityTemplate.needsAuthentication
            );

            this.$scope.$broadcast(MessageType[MessageType.PaneConfigureAction_ConfigureStarting]);
    
            this.ActivityService.configure(this.$scope.currentAction)
                .then((res: interfaces.IActionVM) => {
                    //lets reset config control handles
                    //they will re register themselves after initializing
                    this.$scope.configControlHandles = {};
                var childActionsDetected = false;

                // Detect OperationalState crate with CurrentClientActionName = 'RunImmediately'.
                if (this.crateHelper.hasCrateOfManifestType(res.crateStorage, 'Operational State')) {
                    var operationalStatus = this.crateHelper.findByManifestType(res.crateStorage, 'Operational State');
                    var contents = <any>operationalStatus.contents;

                    if (contents.CurrentActivityResponse.type === 'ExecuteClientActivity' 
                        && (contents.CurrentActivityResponse.body === 'RunImmediately')) {
                        this.$scope.$emit(MessageType[MessageType.PaneConfigureAction_ExecutePlan]);
                    }
                }
                
                var oldAction = this.$scope.currentAction;
                if (oldAction.label !== res.label) {
                    this.$scope.$emit(MessageType[MessageType.PaneConfigureAction_ActionUpdated], res);
                }

                if (res.childrenActivities && res.childrenActivities.length > 0
                    && (!oldAction.childrenActivities || oldAction.childrenActivities.length < 1)) {
                    // If the directive is used for configuring solutions, the SolutionController would listen to this event 
                    // and redirect user to the RouteBuilder once if is received. It means that solution configuration is complete.
                    // not needed in case of Loop action reconfigure

                    this.$scope.$emit(MessageType[MessageType.PaneConfigureAction_ChildActionsDetected]);
                    childActionsDetected = true;
                }

                this.$scope.reconfigureChildrenActions = false;

                if (this.$scope.currentAction.childrenActivities) {
                    if (angular.toJson(this.$scope.currentAction.childrenActivities) != angular.toJson(res.childrenActivities)) {
                        this.$scope.reconfigureChildrenActions = true;
                        //in case of reconfiguring the solution check the child actions again

                        //not needed in case of Loop action
                        if (this.$scope.currentAction.activityTemplate.name !== "Loop") {
                            this.$scope.$emit(MessageType[MessageType.PaneConfigureAction_ChildActionsDetected]);
                        }
                    }
                }

                this.$scope.currentAction.authTokenId = res.authTokenId;
                this.$scope.currentAction.crateStorage = res.crateStorage;
                this.$scope.currentAction.childrenActivities = res.childrenActivities;

                this.$scope.processConfiguration();
                this.configLoadingError = false;

                // Unblock pane.
                if (!childActionsDetected) {
                    this.$scope.processing = false;
                }

                deferred.resolve(this.$scope.currentAction);
                //lets wait for UI operations to end
                this.$timeout(() => { this.processOperationQueue(); });

            }).catch((result) => {

                if (result.status == 423) {
                    this.$scope.processing = false;
                    deferred.reject(result);
                    return;
                }

                var errorText = 'Something went wrong. Click to retry.';
                if (result.status && result.status >= 400) {
                    // Bad http response
                    errorText = 'Configuration loading error. Click to retry.';
                } else if (result.message) {
                    // Exception was thrown in the code
                    errorText = result.message;
                }
                var control = new model.TextBlock(errorText, 'well well-lg alert-danger');
                this.$scope.currentAction.configurationControls = new model.ControlsList();
                this.$scope.currentAction.configurationControls.fields = [control];
                this.configLoadingError = true;

                // Unblock pane.
                this.$scope.processing = false;
                deferred.reject(result);

                }).finally(() => {
                    this.$scope.isConfigRequestQueued = false;
                    this.ConfigureTrackerService.configureCallFinished(this.$scope.currentAction.id);
                    // emit ConfigureCallResponse for RouteBuilderController be able to reload actions with AgressiveReloadTag
                    this.$scope.$emit(MessageType[MessageType.PaneConfigureAction_ConfigureCallResponse], new CallConfigureResponseEventArgs(this.$scope.currentAction, this.$scope.currentActiveElement));
            });
            return deferred.promise;
        };

        private processOperationQueue(): void {
            for (var controlName in this.$scope.configControlOperationQueue) {
                var controlHandle = this.$scope.configControlHandles[controlName];
                if (controlHandle == null) {
                    //hmm this control is probably deleted
                    //nothing to do
                    continue;
                }
                var operationQueue = this.$scope.configControlOperationQueue[controlName];
                for (var i = 0; i < operationQueue.length; i++) {
                    controlHandle.processOperation(operationQueue[i]);
                }
            }
            this.$scope.configControlOperationQueue = {};
        }

        public registerControl(controlName: string, control: paneConfigureAction.IConfigurationControlController) {
            this.$scope.configControlHandles[controlName] = control;
        }

        public setJumpTargets(targets: Array<model.ActivityJumpTarget>) {
            this.LayoutService.setSiblingStatus(this.$scope.currentAction, false);
            this.LayoutService.setJumpTargets(this.$scope.currentAction, targets);
        }

        timeoutPromise = null;

        private configControlChangeBuffer(newValue: model.ControlsList, oldValue: model.ControlsList) {
            if (!newValue || !newValue.fields) {
                return;
            }

            if (this.ignoreConfigurationChange || this.AuthService.isAuthenticationCanceled(this.$scope.currentAction.id) ) {
                this.ignoreConfigurationChange = false;
                return;
            }

            for (var i = 0; i < newValue.fields.length; i++) {
                if (!this.controlValuesChanged(newValue.fields[i], oldValue.fields[i])) {
                    continue;
                }

                if (this.hasRequestConfigHandler(newValue.fields[i])) {
                    // Don't need to save separately; requestConfig event handler will initiate reconfiguration
                    // which will also save the action
                    return;
                }
            }

            this.$timeout.cancel(this.timeoutPromise);  //does nothing, if timeout alrdy done
            this.$scope.plan.planState = model.PlanState.Saving_Changes; // a control value is changed, the plan should not be run after the change request is sent to server
            this.timeoutPromise = this.$timeout(() => {   //Set timeout to prevent sending more than one save requests for changes lasts less than 1 sec.
                this.$scope.onConfigurationChanged(newValue, oldValue);
            }, 1000);
        }

        private processConfiguration() {
            var that = this;
            // Check if authentication is required.
            if (this.crateHelper.hasCrateOfManifestType(this.$scope.currentAction.crateStorage, 'Standard Authentication')) {
                var authCrate = this.crateHelper
                    .findByManifestType(this.$scope.currentAction.crateStorage, 'Standard Authentication');

                this.$scope.currentAction.configurationControls = new model.ControlsList();

                // startAuthentication($scope.currentAction.id);
                if (!(<any>authCrate.contents).Revocation
                    && !this.AuthService.isAuthenticationCanceled(this.$scope.currentAction.id)) {

                    this.AuthService.enqueue(this.$scope.currentAction);

                    var errorText = 'Please provide credentials to access your desired account.';
                    var control = new model.TextBlock(errorText, '');
                    control.name = 'AuthUnsuccessfulLabel';

                    this.$scope.currentAction.configurationControls.fields = [control];
                } else {
                    this.$scope.currentAction.configurationControls.fields = [];
                    var authCancelled = this.AuthService.isAuthenticationCanceled(this.$scope.currentAction.id);

                    if (!authCancelled) {
                        var errorText = 'Authentication has expired, please try again.';
                        var label = new model.TextBlock(errorText, '');
                        label.name = 'AuthUnsuccessfulLabel';
                        label.class = 'TextBlockClass';

                        this.$scope.currentAction.configurationControls.fields.push(label);
                    }

                    var onClickEvent = new model.ControlEvent();
                    onClickEvent.name = 'onClick';
                    onClickEvent.handler = 'requestConfig';

                    var button = new model.Button(!authCancelled ? 'Authenticate' : 'Authentication unsuccessful, try again');
                    button.name = 'AuthUnsuccessfulButton';
                    button.events = [onClickEvent];

                    this.$scope.currentAction.configurationControls.fields.push(button);
                    this.ignoreConfigurationChange = true;
                }
            } else {
                //let's check if this PCA was opened with a view parameter
                //we normally render StandardConfigurationControls with "Configuration_Controls" label
                //but when PCA opens with view parameter we will render StandardConfigurationControls with given label
                if (this.$scope.view) {
                    this.$scope.currentAction.configurationControls =
                        this.crateHelper.createControlListFromCrateStorage(this.$scope.currentAction.crateStorage, this.$scope.view);
                } else {
                    this.$scope.currentAction.configurationControls =
                    this.crateHelper.createControlListFromCrateStorage(this.$scope.currentAction.crateStorage);
                }
            }

            if (this.crateHelper.hasCrateOfManifestType(this.$scope.currentAction.crateStorage, 'Advisory Messages')) {
                var advisoryCrate = this.crateHelper
                    .findByManifestType(this.$scope.currentAction.crateStorage, 'Advisory Messages');
                var advisoryMessages = (<model.AdvisoryMessages>advisoryCrate.contents);
                if (advisoryMessages && advisoryMessages.advisories.length > 0) {
                    this.$scope.showAdvisoryPopup = true;
                    this.$scope.$emit(MessageType[MessageType.PaneConfigureAction_ShowAdvisoryMessages], new ShowAdvisoryMessagesEventArgs(this.$scope.currentAction.id, advisoryMessages));
                }
            }

            var hasConditionalBranching = _.any(this.$scope.currentAction.configurationControls.fields, (field: model.ControlDefinitionDTO) => {
                return field.type === 'ContainerTransition';
            });

            if (hasConditionalBranching) {
                this.LayoutService.setSiblingStatus(this.$scope.currentAction, false);
            }
            // Before setting up watcher on configuration change, make sure that the first invokation of the handler 
            // is ignored: watcher always triggers after having been set up, and we don't want to handle that 
            // useless call.
            this.ignoreConfigurationChange = true;

            this.$timeout(() => { // let the control list create, we don't want false change notification during creation process
                this.$scope.configurationWatchUnregisterer = this.$scope.$watch<model.ControlsList>(
                () => this.$scope.currentAction.configurationControls,
                <any>angular.bind(that, this.configControlChangeBuffer),
                    true);
            }, 1000);
        } 

        private setSolutionMode() {
            this.$scope.$emit(MessageType[MessageType.PaneConfigureAction_SetSolutionMode]);
        }
    }
}

app.directive('paneConfigureAction', () => {
    return {
        restrict: 'E',
        templateUrl: '/AngularTemplate/PaneConfigureAction',
        controller: dockyard.directives.paneConfigureAction.PaneConfigureActionController,
        scope: {
            currentAction: '=',
            mode: '=',
            plan: '=',
            subPlan: '=',
            view: '@',
            processing: '='
        }
    };
});
