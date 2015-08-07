/// <reference path="../../_all.ts" />
module dockyard.directives.paneConfigureAction {
    'use strict';

    export enum MessageType {
        PaneConfigureAction_ActionUpdated,
        PaneConfigureAction_Render,
        PaneConfigureAction_Hide,
        PaneConfigureAction_Cancelled
    }

    export class ActionUpdatedEventArgs {
        public criteriaId: number;
        public actionId: number;
        public actionTempId: number;
        public processTemplateId: number;

        constructor(criteriaId: number, actionId: number, actionTempId: number, processTemplateId: number) {
            this.actionId = actionId;
            this.criteriaId = criteriaId;
            this.actionTempId = actionTempId;
            this.processTemplateId = processTemplateId
        }
    }

    export class RenderEventArgs {
        public criteriaId: number;
        public actionId: number;
        public isTempId: boolean;
        public processTemplateId: number;

        constructor(criteriaId: number, actionId: number, isTempId: boolean, processTemplateId: number) {
            this.actionId = actionId;
            this.criteriaId = criteriaId;
            this.isTempId = isTempId;
            this.processTemplateId = processTemplateId
        }
    }

    export class CancelledEventArgs {
        public criteriaId: number;
        public actionId: number;
        public isTempId: boolean;
        public processTemplateId: number;

        constructor(criteriaId: number, actionId: number, isTemp: boolean, processTemplateId: number) {
            this.actionId = actionId;
            this.criteriaId = criteriaId;
            this.isTempId = isTemp;
            this.processTemplateId = processTemplateId
        }
    }

    //More detail on creating directives in TypeScript: 
    //http://blog.aaronholmes.net/writing-angularjs-directives-as-typescript-classes/
    class PaneConfigureAction implements ng.IDirective {
        public link: (scope: ng.IScope, element: ng.IAugmentedJQuery, attrs: ng.IAttributes) => void;
        public templateUrl = '/AngularTemplate/PaneConfigureAction';
        public controller: ($scope: ng.IScope, element: ng.IAugmentedJQuery, attrs: ng.IAttributes) => void;
        public scope = {};
        public restrict = 'E';

        constructor(private $rootScope: interfaces.IAppRootScope) {
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

                //Template function goes here

                $scope.cancel = function (event) {
                    $scope.isVisible = false;
                    var eventArgs = new CancelledEventArgs(
                        $scope.action.criteriaId,
                        $scope.action.id > 0 ? $scope.action.id : $scope.action.tempId,
                        $scope.action.id < 0, 0);
                    $scope.$emit(MessageType[MessageType.PaneConfigureAction_Cancelled], eventArgs);
                }

                $scope.save = function (event) {
                    var eventArgs = new ActionUpdatedEventArgs($scope.action.criteriaId, $scope.action.id, $scope.action.tempId, 0);
                    $scope.$emit(MessageType[MessageType.PaneConfigureAction_ActionUpdated], eventArgs);
                    (<any>$).notify("Thank you, Action saved!", "success");
                }

                $scope.$watch<interfaces.IAction>((scope: interfaces.IPaneConfigureActionScope) => scope.action, this.onActionChanged, true);
                $scope.$on(MessageType[MessageType.PaneConfigureAction_Render], this.onRender);
                $scope.$on(MessageType[MessageType.PaneConfigureAction_Hide], this.onHide);
            };
        }

        private onActionChanged(newValue: interfaces.IAction, oldValue: interfaces.IAction, scope: interfaces.IPaneConfigureActionScope) {

        }

        private onRender(event: ng.IAngularEvent, eventArgs: RenderEventArgs) {
            var scope = (<interfaces.IPaneConfigureActionScope> event.currentScope);
            scope.isVisible = true;
            scope.action = new model.Action(
                eventArgs.isTempId ? 0 : eventArgs.actionId,
                eventArgs.isTempId ? eventArgs.actionId : 0,
                eventArgs.criteriaId);
        }

        private onHide(event: ng.IAngularEvent, eventArgs: RenderEventArgs) {
            (<interfaces.IPaneConfigureActionScope> event.currentScope).isVisible = false;
        }

        //The factory function returns Directive object as per Angular requirements
        public static Factory() {
            var directive = ($rootScope: interfaces.IAppRootScope) => {
                return new PaneConfigureAction($rootScope);
            };

            directive['$inject'] = ['$rootScope'];
            return directive;
        }
    }
    app.directive('paneConfigureAction', PaneConfigureAction.Factory());
}