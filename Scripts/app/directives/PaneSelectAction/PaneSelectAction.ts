/// <reference path="../../_all.ts" />

module dockyard.directives.paneSelectAction {
    'use strict';

    export enum MessageType {
        PaneSelectAction_ActionUpdated,
        PaneSelectAction_Render,
        PaneSelectAction_Hide,
        PaneSelectAction_UpdateAction,
        PaneSelectAction_ActionTypeSelected,
        PaneSelectAction_ActionRemoved
    }

    export class ActionTypeSelectedEventArgs {
        public processNodeTemplateId: number;
        public id: number;
        public isTempId: boolean;
        public actionListId: number;
        public actionType: string;
        public actionName: string;

        constructor(
            processNodeTemplateId: number,
            id: number,
            isTempId: boolean,
            actionListId: number,
            actionType: string,
            actionName: string) {

            this.processNodeTemplateId = processNodeTemplateId;
            this.id = id;
            this.isTempId = isTempId;
            this.actionListId = actionListId;
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

    export class RenderEventArgs {
        public processNodeTemplateId: number;
        public id: number;
        public isTempId: boolean;
        public actionListId: number;

        constructor(
            processNodeTemplateId: number,
            id: number,
            isTemp: boolean,
            actionListId: number) {

            this.processNodeTemplateId = processNodeTemplateId;
            this.id = id;
            this.isTempId = isTemp;
            this.actionListId = actionListId;
        }
    }

    export class UpdateActionEventArgs extends ActionEventArgsBase {
        public isTempId: boolean;

        constructor(criteriaId: number, actionId: number, isTempId: boolean) {
            super(criteriaId, actionId);
            this.isTempId = isTempId;
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


    //More detail on creating directives in TypeScript: 
    //http://blog.aaronholmes.net/writing-angularjs-directives-as-typescript-classes/
    class PaneSelectAction implements ng.IDirective {
        public link: (scope: ng.IScope, element: ng.IAugmentedJQuery, attrs: ng.IAttributes) => void;
        public templateUrl = '/AngularTemplate/PaneSelectAction';
        public controller: ($scope: ng.IScope, element: ng.IAugmentedJQuery,
            attrs: ng.IAttributes, $http: ng.IHttpService, urlPrefix: string) => void;
        public scope = {
            action: '='
        };
        public restrict = 'E';

        constructor(
            private $rootScope: interfaces.IAppRootScope
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
                $attrs: ng.IAttributes,
                $http: ng.IHttpService,
                urlPrefix: string) => {

                this.PopulateData($scope, $http, urlPrefix);

                $scope.$watch<model.Action>(
                    (scope: interfaces.IPaneSelectActionScope) => scope.action, this.onActionChanged, true);

                $scope.ActionTypeSelected = () => {
                    var eventArgs = new ActionTypeSelectedEventArgs(
                        $scope.action.processNodeTemplateId,
                        $scope.action.actionId,
                        $scope.action.isTempId,
                        $scope.action.actionListId,
                        $scope.action.actionType,
                        $scope.action.userLabel);
                    $scope.$emit(MessageType[MessageType.PaneSelectAction_ActionTypeSelected], eventArgs);
                }

                $scope.RemoveAction = () => {
                    var afterRemove = function () {
                        $scope.$emit(
                            MessageType[MessageType.PaneSelectAction_ActionRemoved],
                            new ActionRemovedEventArgs($scope.action.actionId, $scope.action.isTempId)
                            );
                    };

                    var self = this;
                    if (!$scope.action.isTempId) {
                        var url = urlPrefix + '/Action/' + $scope.action.actionId;
                        $http.delete(url)
                            .success(function () {
                                afterRemove();
                            });
                    }
                    else {
                        afterRemove();
                    }
                };

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
        }

        private onHide(event: ng.IAngularEvent, eventArgs: RenderEventArgs) {
            (<interfaces.IPaneSelectActionScope> event.currentScope).isVisible = false;
        }

        private onUpdate(event: ng.IAngularEvent, eventArgs: RenderEventArgs) {
            (<any>$).notify("Greetings from Select Action Pane. I've got a message about my neighbor saving its data so I saved my data, too.", "success");
        }

        private PopulateData(
            $scope: interfaces.IPaneSelectActionScope,
            $http: ng.IHttpService,
            urlPrefix: string) {

            $scope.actionTypes = [];

            $http.get(urlPrefix + '/actions/available')
                .then(function (resp) {
                    angular.forEach(resp.data, function (it) {
                        console.log(it);

                        $scope.actionTypes.push(new model.ActionTemplate(it.id, it.actionType, {}));
                    });
                });
        }

        //The factory function returns Directive object as per Angular requirements
        public static Factory() {
            var directive = (
                $rootScope: interfaces.IAppRootScope) => {

                return new PaneSelectAction($rootScope);
            };

            directive['$inject'] = ['$rootScope'];
            return directive;
        }
    }
    app.directive('paneSelectAction', PaneSelectAction.Factory());
}