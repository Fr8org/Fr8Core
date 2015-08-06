/// <reference path="../../_all.ts" />
 
module dockyard.directives.paneSelectAction {
    'use strict';

    //More detail on creating directives in TypeScript: 
    //http://blog.aaronholmes.net/writing-angularjs-directives-as-typescript-classes/
    class PaneSelectAction implements ng.IDirective {
        public link: (scope: ng.IScope, element: ng.IAugmentedJQuery, attrs: ng.IAttributes) => void;
        public templateUrl = '/AngularTemplate/PaneSelectAction';
        public controller: ($scope: ng.IScope, element: ng.IAugmentedJQuery, attrs: ng.IAttributes) => void;
        public scope = {};
        public restrict = 'E';

        constructor(private $rootScope: interfaces.IAppRootScope) {
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

                this.PupulateSampleData($scope);

                $scope.$watch<interfaces.IAction>((scope: interfaces.IPaneSelectActionScope) => scope.action, this.onActionChanged, true);
                $scope.$on(MessageType[MessageType.PaneSelectAction_Render], this.onRender);
                $scope.$on(MessageType[MessageType.PaneSelectAction_Hide], this.onHide);
                $scope.$on(MessageType[MessageType.PaneSelectAction_UpdateAction], this.onUpdate);
            };
        }

        private onActionChanged(newValue: interfaces.IAction, oldValue: interfaces.IAction, scope: interfaces.IPaneSelectActionScope) {
            console.log(newValue);
        }

        private onRender(event: ng.IAngularEvent, eventArgs: RenderEventArgs) {
            console.log('render');
            var scope = (<interfaces.IPaneSelectActionScope> event.currentScope);
            scope.isVisible = true;
            scope.action = new model.Action(
                eventArgs.isTempId ? 0 : eventArgs.actionId,
                eventArgs.isTempId ? eventArgs.actionId : 0,
                eventArgs.criteriaId);
        }

        private onHide(event: ng.IAngularEvent, eventArgs: RenderEventArgs) {
            console.log('hide');
            (<interfaces.IPaneSelectActionScope> event.currentScope).isVisible = false;
        }

        private onUpdate(event: ng.IAngularEvent, eventArgs: RenderEventArgs) {
            console.log('update');
            (<any>$).notify("Greetings from Select Action Pane. I've got a message about my neighbor saving its data so I saved my data, too.", "success");
        }

        private PupulateSampleData($scope: interfaces.IPaneSelectActionScope) {
            $scope.sampleActionTypes = [
                { name: "Action type 1", value: "1" },
                { name: "Action type 2", value: "2" },
                { name: "Action type 3", value: "3" }
            ];
        }

        //The factory function returns Directive object as per Angular requirements
        public static Factory() {
            var directive = ($rootScope: interfaces.IAppRootScope) => {
                return new PaneSelectAction($rootScope);
            };

            directive['$inject'] = ['$rootScope'];
            return directive;
        }
    }
    app.directive('paneSelectAction', PaneSelectAction.Factory());
}