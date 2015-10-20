/// <reference path="../_all.ts" />

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

app.directive('layoutContainer', () => {
    return {
        restrict: 'A',
        link: (scope: ng.IScope, elem: ng.IAugmentedJQuery) => {
            scope.$watch(() => {
                var lastChild = elem.children().last();
                return lastChild.length ? lastChild.offset().top + 120 : 0;
            }, (newValue) => {
                elem.css('height', newValue);
            });
        }
    };
});