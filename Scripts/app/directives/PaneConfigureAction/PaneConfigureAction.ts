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

    export class CancelledEventArgs extends CancelledEventArgsBase { }

    enum FieldType {
        textField,
        checkboxField
    }

    //More detail on creating directives in TypeScript: 
    //http://blog.aaronholmes.net/writing-angularjs-directives-as-typescript-classes/
    class PaneConfigureAction implements ng.IDirective {
        public link: (scope: interfaces.IPaneConfigureActionScope, element: ng.IAugmentedJQuery, attrs: ng.IAttributes) => void;
        public templateUrl = '/AngularTemplate/PaneConfigureAction';
        public controller: ($scope: interfaces.IPaneConfigureActionScope, element: ng.IAugmentedJQuery, attrs: ng.IAttributes) => void;
        public scope = {
            currentAction: '='
        };
        public restrict = 'E';
        private _$element: ng.IAugmentedJQuery;

        constructor(private $rootScope: interfaces.IAppRootScope, private ActionService: services.IActionService) {
            PaneConfigureAction.prototype.link = (
                scope: interfaces.IPaneConfigureActionScope,
                element: ng.IAugmentedJQuery,
                attrs: ng.IAttributes) => {

                //Link function goes here
            };

            PaneConfigureAction.prototype.controller = (
                $scope: interfaces.IPaneConfigureActionScope,
                $element: ng.IAugmentedJQuery,
                $attrs: ng.IAttributes) => {
                this._$element = $element;

                //Controller goes here
                $scope.isVisible = true;

                $scope.$watch<model.Action>((scope: interfaces.IPaneConfigureActionScope) => scope.action, this.onActionChanged, true);
                $scope.$on(MessageType[MessageType.PaneConfigureAction_Render], <any>angular.bind(this, this.onRender));
                $scope.$on(MessageType[MessageType.PaneConfigureAction_Hide], this.onHide);

                //TODO: this is test code, remove later
                $scope.currentAction = <interfaces.IActionVM> { id: 1, isTempId: false };
                $scope.$broadcast(MessageType[MessageType.PaneConfigureAction_Render], new RenderEventArgs(1, 2, false, 1));
            };
        }

        private onActionChanged(newValue: model.Action, oldValue: model.Action, scope: interfaces.IPaneConfigureActionScope) {

        }

        private onRender(event: ng.IAngularEvent, eventArgs: RenderEventArgs) {
            var scope = (<interfaces.IPaneConfigureActionScope> event.currentScope);
            scope.action = new model.Action(
                eventArgs.processNodeTemplateId,
                eventArgs.id,
                eventArgs.isTempId,
                eventArgs.actionListId
                );

            //for now ignore actions which were not saved in the database
            if (eventArgs.isTempId || scope.currentAction == null) return;
            scope.isVisible = true;
            //TODO supply real actionRegistrationId 
            scope.configurationSettings = this.ActionService.getConfigurationSettings({ id: 1 });
            scope.configurationSettings.$promise.then((result) => {
                console.log(result);
                this.renderFields(result);
            })
        }

        private renderFields(configurationSettings: model.ConfigurationSettings) {
            for (var field of configurationSettings.fields) {
                switch (field.type) {
                    case FieldType[FieldType.textField]:
                        dockyard.model.TextField.prototype.render.call(field, this._$element);
                        break;
                    case FieldType[FieldType.checkboxField]:
                        dockyard.model.CheckboxField.prototype.render.call(field, this._$element);
                        break;
                    default:
                        Error("Unsupported field type: " + field.type);
                }
            }
        }

        private onHide(event: ng.IAngularEvent, eventArgs: RenderEventArgs) {
            (<interfaces.IPaneConfigureActionScope> event.currentScope).isVisible = false;
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
                        "type": "checkboxField",
                        "name": "log_transactions",
                        "selected": false,
                        "fieldLabel": "Log All Transactions?"
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