/// <reference path="../_all.ts" />
 
module dockyard.directives {
    'use strict';

    //More detail on creating directives in TypeScript: 
    //http://blog.aaronholmes.net/writing-angularjs-directives-as-typescript-classes/
    class PaneSelectAction implements ng.IDirective {
        public link: (scope: ng.IScope, element: ng.IAugmentedJQuery, attrs: ng.IAttributes) => void;
        public templateUrl = '/AngularTemplate/PaneSelectAction';
        public controller: ($scope: ng.IScope, element: ng.IAugmentedJQuery, attrs: ng.IAttributes) => void;
        public scope = {
            criteriaList: '='
        };
        public restrict = 'E';

        constructor(private $rootScope: interfaces.IAppRootScope) {
            PaneSelectAction.prototype.link = (
                scope: interfaces.ISelectActionPaneScope,
                element: ng.IAugmentedJQuery,
                attrs: ng.IAttributes) => {

                //Link function goes here
            };

            PaneSelectAction.prototype.controller = (
                $scope: interfaces.ISelectActionPaneScope,
                $element: ng.IAugmentedJQuery,
                $attrs: ng.IAttributes) => {

                $scope.$watch((scope: interfaces.ISelectActionPaneScope) => scope.actionTypeId, this.onActionTypeChanged, true);
            };


        }

        public onActionTypeChanged(newValue: number, oldValue: number, scope: interfaces.ISelectActionPaneScope) {

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