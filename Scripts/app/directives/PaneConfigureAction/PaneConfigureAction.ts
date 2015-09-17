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
        public action: interfaces.IActionDesignDTO

        constructor(action: interfaces.IActionDesignDTO) {
            // Clone Action to prevent any issues due to possible mutation of source object
            this.action = angular.extend({}, action);
        }
    }

    export class MapFieldsClickedEventArgs {
        action: model.ActionDesignDTO;

        constructor(action: model.ActionDesignDTO) {
            this.action = action;
        }
    }

    export interface IPaneConfigureActionScope extends ng.IScope {
        onActionChanged: (newValue: model.ActionDesignDTO, oldValue: model.ActionDesignDTO, scope: IPaneConfigureActionScope) => void;
        action: interfaces.IActionDesignDTO;
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
        private _currentAction: interfaces.IActionDesignDTO =
        new model.ActionDesignDTO(0, 0, false, 0); //a local immutable copy of current action

        constructor(
            private $rootScope: interfaces.IAppRootScope,
            private ActionService: services.IActionService,
            private crateHelper: services.CrateHelper,
            private $filter: ng.IFilterService
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
                $scope.$watch<interfaces.IActionDesignDTO>((scope: IPaneConfigureActionScope) => scope.action, this.onActionChanged, true);
                $scope.$on(MessageType[MessageType.PaneConfigureAction_Render], <any>angular.bind(this, this.onRender));
                $scope.$on(MessageType[MessageType.PaneConfigureAction_Hide], this.onHide);
                //$scope.$on("OnExitFocus", (event: ng.IAngularEvent, eventArgs: IConfigurationFieldScope) => this.onExitFocus(eventArgs));
                $scope.$on("OnExitFocus", <any>angular.bind(this, this.onExitFocus));

                $scope.mapFields = <(IPaneConfigureActionScope) => void>angular.bind(this, this.mapFields);
            };
        }

        private onActionChanged(newValue: model.ActionDesignDTO, oldValue: model.ActionDesignDTO, scope: IPaneConfigureActionScope) {
            model.ControlsList
        } 

        private onExitFocus(event: ng.IAngularEvent, eventArgs: ExitFocusEventArgs) {
            var scope = <IPaneConfigureActionScope>event.currentScope;

            // Check if this event is defined for the current field
            var fieldName = eventArgs.fieldName;
            var fieldList = scope.currentAction.configurationControls.fields;

            // Find the configuration field object for which the event has fired
            fieldList = <Array<model.ConfigurationField>> this.$filter('filter')(fieldList, { name: fieldName }, true);
            if (fieldList.length == 0 || fieldList[0].events.length == 0) return;
            var field = fieldList[0];

            // Find the onExitFocus event object
            var eventHandlerList = <Array<model.FieldEvent>> this.$filter('filter')(field.events, { Name: 'onExitFocus' }, true);
            if (eventHandlerList.length == 0) return;
            var fieldEvent = eventHandlerList[0];

            
            //this.push(key + ': ' + value);
            //TODO: to be fixed
            //console.log("name : " + value.Name + " -> handler : " + value.Handler);
            //debugger;
            //if (value.Name == "onExitFocus") {
            //    //console.log("events.onExitFocus is NOT null");
            //    //console.log("found onExitFocus -> " + events.onExitFocus);
            //    if (value.Handler == "requestConfig") {
            //        console.log("onExitFocus == requestConfig");
            //        alert(value.Name + " -> " + value.Handler);
            //        // Render Pane Configure Action 
            //        var pcaEventArgs = new RenderEventArgs(this._currentAction);
            //        this.onRender(event, pcaEventArgs);
            //        alert("called onRender on PCA");

            //        //$scope.$broadcast(pca.MessageType[pca.MessageType.PaneConfigureAction_Render], pcaEventArgs);
            //    }
            //}
            // });
        }

        private onRender(event: ng.IAngularEvent, eventArgs: RenderEventArgs) {
            var scope = (<IPaneConfigureActionScope> event.currentScope);

            scope.action = eventArgs.action;

            //for now ignore actions which were not saved in the database
            if (eventArgs.action.isTempId || scope.currentAction == null) return;
            scope.isVisible = true;

            // Get configuration settings template from the server if the current action does not 
            // contain those or user has selected another action template.
            //if (scope.currentAction.crateStorage == null
            //    || scope.currentAction.configurationControls.fields == null
            //    || scope.currentAction.configurationControls.fields.length == 0
            //    || (eventArgs.action.id == this._currentAction.id &&
            //        eventArgs.action.actionTemplateId != this._currentAction.actionTemplateId)) {
            //FOR NOW we're going to simplify things by always checking with this server for a new configuration

            if (eventArgs.action.actionTemplateId > 0) {
                var resource = this.ActionService.configure(scope.action);
                (<any>scope.currentAction).crateStorage = resource;

                // Here we parse look for Crate with ManifestType == 'Standard Configuration Controls'.
                // We parse its contents and put it into currentAction.configurationControls structure.
                var self = this;

                resource.$promise.then(function (res: any) {
                    (<any>scope.currentAction).configurationControls =
                    self.crateHelper.createControlListFromCrateStorage(<model.CrateStorage>res);
                });
            }
            

            // Create a directive-local immutable copy of action so we can detect 
            // a change of actionTemplateId in the currently selected action
            this._currentAction = angular.extend({}, eventArgs.action);
            //debugger;
        }

        private onHide(event: ng.IAngularEvent, eventArgs: RenderEventArgs) {
            (<IPaneConfigureActionScope> event.currentScope).isVisible = false;
        }

        private mapFields(scope: IPaneConfigureActionScope) {
            scope.$emit(
                MessageType[MessageType.PaneConfigureAction_MapFieldsClicked],
                new MapFieldsClickedEventArgs(angular.extend({}, scope.currentAction)) //clone action to prevent msg recipient from modifying orig. object
                );
        }

        //The factory function returns Directive object as per Angular requirements
        public static Factory() {
            var directive = (
                $rootScope: interfaces.IAppRootScope,
                ActionService,
                crateHelper: services.CrateHelper,
                $filter: ng.IFilterService
                ) => {

                return new PaneConfigureAction($rootScope, ActionService, crateHelper, $filter);
            };

            directive['$inject'] = ['$rootScope', 'ActionService', 'CrateHelper', '$filter'];
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