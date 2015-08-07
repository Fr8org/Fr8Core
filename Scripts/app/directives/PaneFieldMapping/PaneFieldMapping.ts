/// <reference path="../../_all.ts" />
/// <reference path="../../../typings/angularjs/angular.d.ts"/>


module dockyard.directives.PaneFieldMapping {
    'use strict';

    class PaneFieldMapping implements ng.IDirective {
        public link: (scope: ng.IScope, element: ng.IAugmentedJQuery, attrs: ng.IAttributes) => void;
        public templateUrl = '/AngularTemplate/PaneFieldMapping';
        public restrict = 'E';

        public static factory() {
            var directive = () => {
                return new PaneFieldMapping();
            };

            directive['$inject'] = ['$rootScope'];
            return directive;
        }
    }
    app.directive('paneFieldMapping', PaneFieldMapping.factory());
}