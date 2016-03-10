/// <reference path="../../_all.ts" />
module dockyard.directives.containerTransition {
    'use strict';

    interface IContainerTransitionScope extends ng.IScope {
        route: model.Route;
        
    }

    //More detail on creating directives in TypeScript: 
    //http://blog.aaronholmes.net/writing-angularjs-directives-as-typescript-classes/
    export function ContainerTransition(): ng.IDirective {
        
        var controller = ['$scope', '$element', '$attrs', ($scope: IContainerTransitionScope, element: ng.IAugmentedJQuery, attrs: ng.IAttributes) => {
            
            
        }];

        //The factory function returns Directive object as per Angular requirements
        return {
            restrict: 'E',
            templateUrl: '/AngularTemplate/ContainerTransition',
            controller: controller,
            scope: {
                route: '='
            }
        };
    }

    app.directive('containerTransition', ContainerTransition);
}