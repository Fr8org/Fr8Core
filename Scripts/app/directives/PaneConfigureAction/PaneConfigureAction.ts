/// <reference path="../../_all.ts" />
module dockyard.directives.paneConfigureAction {
    'use strict';

    export enum MessageType {
        PaneConfigureAction_ActionUpdated,
        PaneConfigureAction_Render,
        PaneConfigureAction_Hide,
        PaneConfigureAction_Cancelled
    }

    export class ActionUpdatedEventArgs extends ActionUpdatedEventArgsBase { }

    export class RenderEventArgs {
        public processNodeTemplateId: number;
        public id: number;
        public isTempId: boolean;
        public actionListId: number;

        constructor(
            processNodeTemplateId: number,
            id: number,
            isTempId: boolean,
            actionListId: number) {

            this.actionListId = actionListId;
            this.id = id;
            this.isTempId = isTempId;
        }
    }

    export interface IPaneConfigureActionScope extends ng.IScope {
        onActionChanged: (newValue: model.Action, oldValue: model.Action, scope: IPaneConfigureActionScope) => void;
        action: model.Action;
        isVisible: boolean;
        currentAction: interfaces.IActionVM;
        configurationSettings: ng.resource.IResource<model.ConfigurationSettings> | model.ConfigurationSettings;
        cancel: (event: ng.IAngularEvent) => void;
        save: (event: ng.IAngularEvent) => void;
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

        constructor(private $rootScope: interfaces.IAppRootScope, private ActionService: services.IActionService) {
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
                $scope.isVisible = true;

                $scope.$watch<model.Action>((scope: IPaneConfigureActionScope) => scope.action, this.onActionChanged, true);
                $scope.$on(MessageType[MessageType.PaneConfigureAction_Render], <any>angular.bind(this, this.onRender));
                $scope.$on(MessageType[MessageType.PaneConfigureAction_Hide], this.onHide);

                //TODO: this is test code, remove later
                $scope.currentAction = <interfaces.IActionVM> { actionId: 1, isTempId: false };
                $scope.$broadcast(MessageType[MessageType.PaneConfigureAction_Render], new RenderEventArgs(1, 2, false, 1));
            };
        }

        private onActionChanged(newValue: model.Action, oldValue: model.Action, scope: IPaneConfigureActionScope) {
            model.ConfigurationSettings
        }

        private onRender(event: ng.IAngularEvent, eventArgs: RenderEventArgs) {
            var scope = (<IPaneConfigureActionScope> event.currentScope);
            scope.action = new model.Action(
                eventArgs.processNodeTemplateId,
                eventArgs.id,
                eventArgs.isTempId,
                eventArgs.actionListId
                );

            //for now ignore actions which were not saved in the database
            if (eventArgs.isTempId || scope.currentAction == null) return;
            scope.isVisible = true;


            if (scope.currentAction.configurationSettings == null
                || scope.currentAction.configurationSettings.fields == null
                || scope.currentAction.configurationSettings.fields.length == 0) {

                (<any>scope.currentAction).configurationSettings = this.ActionService.getConfigurationSettings({ id: 1 });  //TODO supply real actionRegistrationId 
            }
        }

        private onHide(event: ng.IAngularEvent, eventArgs: RenderEventArgs) {
            (<IPaneConfigureActionScope> event.currentScope).isVisible = false;
        }

        //The factory function returns Directive object as per Angular requirements
        public static Factory() {
            var directive = ($rootScope: interfaces.IAppRootScope, ActionService) => {
                return new PaneConfigureAction($rootScope, ActionService);
            };

            directive['$inject'] = ['$rootScope', 'ActionService'];
            return directive;
        }
    }

    app.run([
        "$httpBackend", "urlPrefix", (httpBackend, urlPrefix) => {
            var configuration = {
                "fields":
                [
                    {
                        "type": "textField",
                        "name": "connection_string",
                        "required": true,
                        "value": "",
                        "fieldLabel": "SQL Connection String"
                    },
                    {
                        "type": "textField",
                        "name": "query",
                        "required": true,
                        "value": "",
                        "fieldLabel": "Custom SQL Query"
                    },
                    {
                        "type": "checkboxField",
                        "name": "log_transactions",
                        "selected": false,
                        "fieldLabel": "Log All Transactions?"
                    },
                    {
                        "type": "checkboxField",
                        "name": "log_transactions1",
                        "selected": false,
                        "fieldLabel": "Log Some Transactions?"
                    },
                    {
                        "type": "checkboxField",
                        "name": "log_transactions2",
                        "selected": false,
                        "fieldLabel": "Log No Transactions?"
                    },
                    {
                        "type": "checkboxField",
                        "name": "log_transactions3",
                        "selected": false,
                        "fieldLabel": "Log Failed Transactions?"
                    }
                ]
            };

            httpBackend
                .whenGET("/apimock/Action/configuration/1")
                .respond(configuration);
        }
    ]);
    app.directive('paneConfigureAction', PaneConfigureAction.Factory());

}