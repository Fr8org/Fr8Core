/// <reference path="../../_all.ts" />
module dockyard.directives.paneConfigureAction {
    'use strict';

    export enum MessageType {
        PaneConfigureAction_ActionUpdated,
        PaneConfigureAction_Render,
        PaneConfigureAction_Hide,
        PaneConfigureAction_MapFieldsClicked,
        PaneConfigureAction_Cancelled
    }

    export class ActionUpdatedEventArgs extends ActionUpdatedEventArgsBase { }

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

    export interface IPaneConfigureActionScope extends ng.IScope {
        onActionChanged: (newValue: model.ActionDTO, oldValue: model.ActionDTO, scope: IPaneConfigureActionScope) => void;
        action: interfaces.IActionDTO;
        isVisible: boolean;
        currentAction: interfaces.IActionVM;
        configurationControls: ng.resource.IResource<model.ControlsList> | model.ControlsList;
        crateStorage: ng.resource.IResource<model.CrateStorage> | model.CrateStorage;
        mapFields: (scope: IPaneConfigureActionScope) => void;
    }


    export class CancelledEventArgs extends CancelledEventArgsBase { }

    //More detail on creating directives in TypeScript: 
    //http://blog.aaronholmes.net/writing-angularjs-directives-as-typescript-classes/
    class PaneConfigureAction implements ng.IDirective {
        public link: (scope: IPaneConfigureActionScope, element: ng.IAugmentedJQuery, attrs: ng.IAttributes) => void;
        public templateUrl = '/AngularTemplate/PaneConfigureAction';
        public controller: ($scope: IPaneConfigureActionScope, element: ng.IAugmentedJQuery, attrs: ng.IAttributes) => void;
        public scope = {
            currentAction: '='
        };
        public restrict = 'E';
        private _$element: ng.IAugmentedJQuery;
        private _currentAction: interfaces.IActionDTO =
        new model.ActionDTO(0, 0, false, 0); //a local immutable copy of current action
        private configurationWatchUnregisterer: Function;

        constructor(
            private $rootScope: interfaces.IAppRootScope,
            private ActionService: services.IActionService,
            private crateHelper: services.CrateHelper,
            private $filter: ng.IFilterService,
            private $timeout: ng.ITimeoutService
            ) {

            PaneConfigureAction.prototype.link = (
                scope: IPaneConfigureActionScope,
                element: ng.IAugmentedJQuery,
                attrs: ng.IAttributes) => {

                //Link function goes here
            };

            PaneConfigureAction.prototype.controller = (
                $scope: IPaneConfigureActionScope,
                $element: ng.IAugmentedJQuery,
                $attrs: ng.IAttributes) => {
                this._$element = $element;

                //Controller goes here
                $scope.$on(MessageType[MessageType.PaneConfigureAction_Render], <any>angular.bind(this, this.onRender));
                $scope.$on(MessageType[MessageType.PaneConfigureAction_Hide], <any>angular.bind(this, this.onHide));
                $scope.$on("onFieldChange", <any>angular.bind(this, this.onFieldChange));
            };
        }

        private onConfigurationChanged(newValue: model.ControlsList, oldValue: model.ControlsList, scope: IPaneConfigureActionScope) {
            if (!newValue || !newValue.fields || newValue.fields.length == 0) return;

            this.crateHelper.mergeControlListCrate(
                scope.currentAction.configurationControls,
                scope.currentAction.crateStorage
                );
            scope.currentAction.crateStorage.crateDTO = scope.currentAction.crateStorage.crates //backend expects crates on CrateDTO field
            this.ActionService.save({ id: scope.currentAction.id },
                scope.currentAction, null, null);
        }

        private onFieldChange(event: ng.IAngularEvent, eventArgs: ChangeEventArgs) {
            var scope = <IPaneConfigureActionScope>event.currentScope;
            // Check if this event is defined for the current field
            var fieldName = eventArgs.fieldName;
            var fieldList = scope.currentAction.configurationControls.fields;

            // Find the configuration field object for which the event has fired
            fieldList = <Array<model.ConfigurationField>> this.$filter('filter')(fieldList, { name: fieldName }, true);
            if (fieldList.length == 0 || !fieldList[0].events || fieldList[0].events.length == 0) return;
            var field = fieldList[0];

            // Find the onChange event object
            var eventHandlerList = <Array<model.FieldEvent>> this.$filter('filter')(field.events, { name: 'onChange' }, true);
            if (eventHandlerList.length == 0) return;
            var fieldEvent = eventHandlerList[0];

            if (fieldEvent.handler === 'requestConfig') {
                this.crateHelper.mergeControlListCrate(
                    scope.currentAction.configurationControls,
                    scope.currentAction.crateStorage
                    );
                scope.currentAction.crateStorage.crateDTO = scope.currentAction.crateStorage.crates //backend expects crates on CrateDTO field
                this.loadConfiguration(scope, scope.currentAction);
            }
        }

        private onRender(event: ng.IAngularEvent, eventArgs: RenderEventArgs) {
            var scope = (<IPaneConfigureActionScope> event.currentScope);
            if (this.configurationWatchUnregisterer) this.configurationWatchUnregisterer();

            //for now ignore actions which were not saved in the database
            if (eventArgs.action.isTempId) return;
            scope.isVisible = true;

            // Get configuration settings template from the server if the current action does not 
            // contain those or user has selected another action template.
            //if (scope.currentAction.crateStorage == null
            //    || scope.currentAction.configurationControls.fields == null
            //    || scope.currentAction.configurationControls.fields.length == 0
            //    || (eventArgs.action.id == this._currentAction.id &&
            //        eventArgs.action.actionTemplateId != this._currentAction.actionTemplateId)) {
            //FOR NOW we're going to simplify things by always checking with this server for a new configuration

            // Without $timeout the directive's copy of currentAction does not have enough time 
            // to refresh after being assigned newly selected Action on ProcessBuilderController
            // and as a result it contained old action. 
            this.$timeout(() => {
                if (scope.currentAction.activityTemplateId > 0) {
                    this.loadConfiguration(scope, scope.currentAction);
                }            

                // Create a directive-local immutable copy of action so we can detect 
                // a change of actionTemplateId in the currently selected action
                this._currentAction = angular.extend({}, scope.currentAction);
            }, 100);

        }

        // Here we look for Crate with ManifestType == 'Standard Configuration Controls'.
            // We parse its contents and put it into currentAction.configurationControls structure.
        private loadConfiguration(scope: IPaneConfigureActionScope, action: interfaces.IActionDTO) {
            var self = this;
            this.ActionService.configure(action).$promise.then(function (res: any) {               
                scope.currentAction = res;
                (<any>scope.currentAction).configurationControls =
                self.crateHelper.createControlListFromCrateStorage(scope.currentAction.crateStorage);
            });

            if (this.configurationWatchUnregisterer == null) {
                this.$timeout(() => { // let the control list create, we don't want false change notification during creation process
                    this.configurationWatchUnregisterer = scope.$watch<model.ControlsList>((scope: IPaneConfigureActionScope) => scope.currentAction.configurationControls, <any>angular.bind(this, this.onConfigurationChanged), true);
                }, 500);
            }
        }

        private onHide(event: ng.IAngularEvent, eventArgs: RenderEventArgs) {
            (<IPaneConfigureActionScope> event.currentScope).isVisible = false;
            if (this.configurationWatchUnregisterer) this.configurationWatchUnregisterer();
        }

        //The factory function returns Directive object as per Angular requirements
        public static Factory() {
            var directive = (
                $rootScope: interfaces.IAppRootScope,
                ActionService,
                crateHelper: services.CrateHelper,
                $filter: ng.IFilterService,
                $timeout: ng.ITimeoutService
                ) => {

                return new PaneConfigureAction($rootScope, ActionService, crateHelper, $filter, $timeout);
            };

            directive['$inject'] = ['$rootScope', 'ActionService', 'CrateHelper', '$filter', '$timeout'];
            return directive;
        }
    }

    //app.run([
    //    "$httpBackend", "urlPrefix", (httpBackend, urlPrefix) => {

    //        httpBackend
    //            .whenGET("/apimock/Action/configuration/1")
    //            .respond(tests.utils.Fixtures.configurationSettings);
    //    }
    //]);
    app.directive('paneConfigureAction', PaneConfigureAction.Factory());

}