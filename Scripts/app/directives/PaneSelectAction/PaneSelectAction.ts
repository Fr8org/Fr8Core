/// <reference path="../../_all.ts" />

module dockyard.directives.paneSelectAction {
    'use strict';

    export enum MessageType {
        PaneSelectAction_ActionUpdated,
        PaneSelectAction_Render,
        PaneSelectAction_Hide,
        PaneSelectAction_UpdateAction,
        PaneSelectAction_ActionTypeSelected
    }

    export class ActionTypeSelectedEventArgs extends ActionEventArgsBase {
        public actionType: string;
        public isTempId: boolean;
        public actionName: string;

        constructor(criteriaId: number, actionId: number, isTempId: boolean, actionType: string, actionName: string) {
            super(criteriaId, actionId);
            this.isTempId = isTempId;
            this.actionType = actionType;
            this.actionName = actionName;
        }
    }

    export class ActionUpdatedEventArgs extends ActionEventArgsBase {
        public isTempId: boolean;
        public actionName: string;

        constructor(criteriaId: number, actionId: number, isTempId: boolean, actionName: string) {
            super(criteriaId, actionId);
            this.isTempId = isTempId;
            this.actionName = actionName;
        }
    }

    export class RenderEventArgs extends ActionEventArgsBase {
        public isTempId: boolean;

        constructor(criteriaId: number, actionId: number, isTemp: boolean) {
            super(criteriaId, actionId);
            this.isTempId = isTemp;
        }
    }

    export class UpdateActionEventArgs extends ActionEventArgsBase {
        public isTempId: boolean;

        constructor(criteriaId: number, actionId: number, isTempId: boolean) {
            super(criteriaId, actionId);
            this.isTempId = isTempId;
        }
    }

    class PaneSelectAction implements ng.IDirective {
        public link: (scope: ng.IScope, element: ng.IAugmentedJQuery, attrs: ng.IAttributes) => void;
        public templateUrl = '/AngularTemplate/PaneSelectAction';
        public controller: ($scope: ng.IScope, element: ng.IAugmentedJQuery, attrs: ng.IAttributes) => void;
        public scope = {};
        public restrict = 'E';

        constructor(
            private $rootScope: interfaces.IAppRootScope,
            private $resource: ng.resource.IResourceService,
            private urlPrefix: any
            ) {

            PaneSelectAction.prototype.link = (
                scope: interfaces.IPaneSelectActionScope,
                element: ng.IAugmentedJQuery,
                attrs: ng.IAttributes) => {

                //Link function goes here
            };

            PaneSelectAction.prototype.controller = (
                $scope: interfaces.IPaneSelectActionScope,
                $element: ng.IAugmentedJQuery,
                $attrs: ng.IAttributes) => {

                this.PupulateData($scope);

                $scope.$watch<model.Action>(
                    (scope: interfaces.IPaneSelectActionScope) => scope.action, this.onActionChanged, true);

                $scope.ActionTypeSelected = () => {
                    var eventArgs = new ActionTypeSelectedEventArgs(
                        $scope.action.criteriaId,
                        $scope.action.id,
                        $scope.action.isTempId,
                        $scope.action.actionType,
                        $scope.action.userLabel);
                    $scope.$emit(MessageType[MessageType.PaneSelectAction_ActionTypeSelected], eventArgs);
                }

                $scope.$on(MessageType[MessageType.PaneSelectAction_Render], this.onRender);
                $scope.$on(MessageType[MessageType.PaneSelectAction_Hide], this.onHide);
                $scope.$on(MessageType[MessageType.PaneSelectAction_UpdateAction], this.onUpdate);
            };
        }

        private onActionChanged(newValue: model.Action, oldValue: model.Action, scope: interfaces.IPaneSelectActionScope) {

        }


        private onRender(event: ng.IAngularEvent, eventArgs: RenderEventArgs) {
            var scope = (<interfaces.IPaneSelectActionScope> event.currentScope);
            scope.isVisible = true;
            scope.action = new model.Action(
                eventArgs.actionId,
                eventArgs.isTempId,
                eventArgs.criteriaId);
        }

        private onHide(event: ng.IAngularEvent, eventArgs: RenderEventArgs) {
            (<interfaces.IPaneSelectActionScope> event.currentScope).isVisible = false;
        }

        private onUpdate(event: ng.IAngularEvent, eventArgs: RenderEventArgs) {
            (<any>$).notify("Greetings from Select Action Pane. I've got a message about my neighbor saving its data so I saved my data, too.", "success");
        }

        private PupulateData($scope: interfaces.IPaneSelectActionScope) {
            // $scope.sampleActionTypes = [
            //     { name: "Action type 1", value: "1" },
            //     { name: "Action type 2", value: "2" },
            //     { name: "Action type 3", value: "3" }
            // ];

            var actionRegistrations = this.$resource(this.urlPrefix + '/actions/available')
                .query(() => {
                    console.log(actionRegistrations);
                    return;
                });
        }

        //The factory function returns Directive object as per Angular requirements
        public static Factory() {
            var directive = (
                $rootScope: interfaces.IAppRootScope,
                $resource: ng.resource.IResourceService,
                urlPrefix: any) => {

                return new PaneSelectAction($rootScope, $resource, urlPrefix);
            };

            directive['$inject'] = ['$rootScope', '$resource', 'urlPrefix'];
            return directive;
        }
    }
    app.directive('paneSelectAction', PaneSelectAction.Factory());
}