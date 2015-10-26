/// <reference path="../_all.ts" />

module dockyard.directives {

    app.directive('layoutActionGroup', () => {
        return {
            restrict: 'A',
            link: (scope: any, elem: ng.IAugmentedJQuery) => {
                elem.css('left', scope.group.offsetLeft);
                elem.css('top', scope.group.offsetTop);
                var arrow = angular.element(elem.children()[0]);

                if (arrow.hasClass('action-arrow-bottom')) {
                    arrow.css('top', -scope.group.arrowLength - 60);
                    arrow.css('height', scope.group.arrowLength + 35);
                }
            }
        };
    });

    // calculates process builder container height depending on the amount of actions
    app.directive('layoutContainer', (LayoutService: services.LayoutService) => {
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
                    elem.css('height', newValue);
                });
            }
        };
    });
}