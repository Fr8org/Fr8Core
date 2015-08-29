/// <reference path="../_all.ts" />
/// <reference path="../../typings/metronic.d.ts" />
'use strict';
app.directive('autoFocus', function ($timeout) {
    return {
        restrict: 'AC',
        link: function (_scope, _element) {
            $timeout(function () {
                _element[0].focus();
            }, 0);
        }
    };
});
app.directive("checkboxGroup", function () {
    return {
        restrict: "A",
        scope: {
            array: '=',
            itemObject: '@',
            idField: '@'
        },
        link: function (scope, elem, attrs) {
            if (scope.array.indexOf(scope.$parent[scope.itemObject][scope.idField]) !== -1) {
                elem[0].checked = true;
            }
            elem.bind('click', function () {
                var index = scope.array.indexOf(scope.$parent[scope.itemObject][scope.idField]);
                if (elem[0].checked) {
                    if (index === -1)
                        scope.array.push(scope.$parent[scope.itemObject][scope.idField]);
                }
                else {
                    if (index !== -1)
                        scope.array.splice(index, 1);
                }
                scope.$apply(scope.array.sort(function (a, b) {
                    return a - b;
                }));
            });
        }
    };
});
app.directive('ngSpinnerBar', ['$rootScope',
    function ($rootScope) {
        return {
            link: function (scope, element, attrs) {
                element.addClass('hide');
                $rootScope.$on('$stateChangeStart', function () {
                    element.removeClass('hide');
                    Layout.closeMainMenu();
                });
                $rootScope.$on('$stateChangeSuccess', function () {
                    element.addClass('hide');
                    $('body').removeClass('page-on-load');
                    Layout.setMainMenuActiveLink('match');
                    setTimeout(function () {
                        Metronic.scrollTop();
                    }, $rootScope.settings.layout.pageAutoScrollOnLoad);
                });
                $rootScope.$on('$stateNotFound', function () {
                    element.addClass('hide');
                });
                $rootScope.$on('$stateChangeError', function () {
                    element.addClass('hide');
                });
            }
        };
    }
]);
app.directive('a', function () {
    return {
        restrict: 'E',
        link: function (scope, elem, attrs) {
            if (attrs.ngClick || attrs.href === '' || attrs.href === '#') {
                elem.on('click', function (e) {
                    e.preventDefault();
                });
            }
        }
    };
});
app.directive('dropdownMenuHover', function () {
    return {
        link: function (scope, elem) {
            elem.dropdownHover();
        }
    };
});
//# sourceMappingURL=directives.js.map