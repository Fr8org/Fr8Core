/// <reference path="../_all.ts" />
/// <reference path="../../typings/metronic.d.ts" />
/***
Global Directives
***/
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
app.directive('blockIf', function () {
    return {
        restrict: 'A',
        link: function (_scope, _element, attrs) {
            var expr = attrs['blockIf'];
            _scope.$watch(expr, function (value) {
                if (_scope.$eval(expr) === true) {
                    Metronic.blockUI({ target: _element, animate: true });
                }
                else {
                    Metronic.unblockUI(_element);
                }
            });
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
            // Determine initial checked boxes
            if (scope.array.indexOf(scope.$parent[scope.itemObject][scope.idField]) !== -1) {
                elem[0].checked = true;
            }
            // Update array on click
            elem.bind('click', function () {
                var index = scope.array.indexOf(scope.$parent[scope.itemObject][scope.idField]);
                // Add if checked
                if (elem[0].checked) {
                    if (index === -1)
                        scope.array.push(scope.$parent[scope.itemObject][scope.idField]);
                }
                else {
                    if (index !== -1)
                        scope.array.splice(index, 1);
                }
                // Sort and update DOM display
                scope.$apply(scope.array.sort(function (a, b) {
                    return a - b;
                }));
            });
        }
    };
});
// Route State Load Spinner(used on page or content load)
app.directive('ngSpinnerBar', ['$rootScope',
    function ($rootScope) {
        return {
            link: function (scope, element, attrs) {
                // by defult hide the spinner bar
                element.addClass('hide'); // hide spinner bar by default
                // display the spinner bar whenever the route changes(the content part started loading)
                $rootScope.$on('$stateChangeStart', function () {
                    element.removeClass('hide'); // show spinner bar
                    Layout.closeMainMenu();
                });
                // hide the spinner bar on rounte change success(after the content loaded)
                $rootScope.$on('$stateChangeSuccess', function () {
                    element.addClass('hide'); // hide spinner bar
                    $('body').removeClass('page-on-load'); // remove page loading indicator
                    Layout.setMainMenuActiveLink('match'); // activate selected link in the sidebar menu
                    // auto scorll to page top
                    setTimeout(function () {
                        Metronic.scrollTop(); // scroll to the top on content load
                    }, $rootScope.settings.layout.pageAutoScrollOnLoad);
                });
                // handle errors
                $rootScope.$on('$stateNotFound', function () {
                    element.addClass('hide'); // hide spinner bar
                });
                // handle errors
                $rootScope.$on('$stateChangeError', function () {
                    element.addClass('hide'); // hide spinner bar
                });
            }
        };
    }
]);
// Handle global LINK click
app.directive('a', function () {
    return {
        restrict: 'E',
        link: function (scope, elem, attrs) {
            if (attrs.ngClick || attrs.href === '' || attrs.href === '#') {
                elem.on('click', function (e) {
                    e.preventDefault(); // prevent link click for above criteria
                });
            }
        }
    };
});
// Handle Dropdown Hover Plugin Integration
app.directive('dropdownMenuHover', function () {
    return {
        link: function (scope, elem) {
            elem.dropdownHover();
        }
    };
});
// Allows to configure bootstrap container type for an element in state config by setting "data: { containerFluid: true }". Default is simple container.
app.directive('container', ['$state', function ($state) {
        return {
            link: function (scope, elem) {
                scope.$watch(function () { return $state.current; }, function (newState) {
                    if (newState.data && newState.data.noContainer) {
                        elem.removeClass('container');
                        elem.addClass('container-fluid');
                    }
                    else {
                        elem.removeClass('container-fluid');
                        elem.addClass('container');
                    }
                });
            }
        };
    }]);
app.directive('stopClickPropagation', function () {
    return {
        link: function (scope, elem) {
            elem.bind('click', function (event) {
                event.stopPropagation();
            });
        }
    };
});
// temporary solution to reload configuration when action header is clicked.
app.directive('transferClickConfigurePane', function () {
    return {
        link: function (scope, elem) {
            elem.bind('click', function (event) {
                elem.parent().find('.pane-configure-action').click();
                //angular.element(elem.siblings()[0]).click();
            });
        }
    };
});
//# sourceMappingURL=directives.js.map