/// <reference path="../_all.ts" />

module dockyard.directives {

    app.directive('layoutAction', ['LayoutService', (LayoutService: services.LayoutService) => {
        return {
            restrict: 'A',
            link: (scope: any, elem: ng.IAugmentedJQuery) => {
                scope.$watch(() => elem.height(), (newValue) => {
                    scope.envelope.activity.height = newValue;
                    if (newValue > scope.group.height)
                        scope.group.height = newValue;
                    LayoutService.recalculateTop(scope.pSubPlan.actionGroups);
                });
            }
        };
    }]);

    app.directive('layoutActionGroup', ['LayoutService', (LayoutService: services.LayoutService) => {
        return {
            restrict: 'A',
            link: (scope: any, elem: ng.IAugmentedJQuery) => {
            }
        };
    }]);

    // calculates process builder container height depending on the amount of actions
    app.directive('layoutContainer', ['LayoutService', (LayoutService: services.LayoutService) => {
        return {
            restrict: 'A',
            link: (scope: ng.IScope, elem: ng.IAugmentedJQuery) => {
                scope.$watch(() => {
                    var lastChild = elem.children().last();
                    if (lastChild.length) {
                        return lastChild.position().top + elem.scrollTop() + lastChild.height() + 30;
                    }
                    return 0;
                }, (newValue) => {
                    var w = $(window).width();
                    if( w > 400)
                        elem.css('height', newValue);
                });
            }
        };
    }]);
}