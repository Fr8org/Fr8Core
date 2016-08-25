/// <reference path="../_all.ts" />
/// <reference path="../../typings/metronic.d.ts" />

/***
Global Directives
***/

'use strict';

app.directive('autoFocus', ['$timeout', function ($timeout) {
    return {
        restrict: 'AC',
        link: function (_scope, _element) {
            $timeout(function () {
                _element[0].focus();
            }, 0);
        }
    };
}]);

app.directive('delayedAutoFocus', ['$timeout', function ($timeout) {
    return {
        restrict: 'AC',
        link: function (_scope, _element) {
            $timeout(function () {
                _element[0].focus();
            }, 500);
        }
    };
}]);

app.filter('parseUrl', () => {
    var urls = /(\b(https?|ftp):\/\/[A-Z0-9+&@#\/%?=~_|!:,.;-]*[-A-Z0-9+&@#\/%=~_|])/gim;

    return (text: string) => {
        if (!angular.isString(text)) {
            return text;
        }
        if (text.match(urls)) {
            var indexOfUrl = text.indexOf(text.match(urls)[0]);
            // if url is inside of a href tag, skip adding href
            if (text.substring(indexOfUrl - 6, indexOfUrl - 1) != "href=") {
                text = text.replace(urls, "<a href=\"$1\" target=\"_blank\">$1</a>");
            }
        }
        return text;
    }
});

app.directive('blockIf', function () {
    return {
        restrict: 'A',
        link: function (_scope, _element, attrs) {
            var expr = attrs['blockIf'];
            _scope.$watch(expr, (value) => {
                if (attrs['class'] === "plan-loading-message" && _scope.$eval(expr) == null) {
                    Metronic.blockUI({ target: _element, animate: true });
                }
                else if (_scope.$eval(expr) === true) {
                    Metronic.blockUI({ target: _element, animate: true });
                }
                else {
                    Metronic.unblockUI(_element);
                }
            });
        }
    };
});



app.directive('fr8Click', ['$parse', '$timeout',($parse: ng.IParseService) => {
    return {
        restrict: 'A',
        require: '^configurationControl',
        compile: ($element: ng.IAugmentedJQuery, attr) => {
            var fn = $parse(attr['fr8Click']);
            return (scope, element: ng.IAugmentedJQuery, attr, cc: dockyard.directives.paneConfigureAction.IConfigurationControlController) => {
                element.on('click', (event) => {
                    if (cc.isThereOnGoingConfigRequest()) {
                        cc.queueClick(element);
                    } else {
                        //lets call callback function immediately
                        scope.$apply(() => {
                            fn(scope, { $event: event });
                        });
                    }
                });
            };
        }
    };
}]);


app.directive("checkboxGroup", function () {
    return {
        restrict: "A",
        scope: {
            array: '=',
            itemObject: '@',
            idField: '@'
        },
        link: function (scope: any, elem, attrs) {
            // Determine initial checked boxes
            if (scope.array.indexOf(scope.$parent[scope.itemObject][scope.idField]) !== -1) {
                elem[0].checked = true;
            }

            // Update array on click
            elem.bind('click', function () {
                var index = scope.array.indexOf(scope.$parent[scope.itemObject][scope.idField]);
                // Add if checked
                if (elem[0].checked) {
                    if (index === -1) scope.array.push(scope.$parent[scope.itemObject][scope.idField]);
                }
                // Remove if unchecked
                else {
                    if (index !== -1) scope.array.splice(index, 1);
                }
                // Sort and update DOM display
                scope.$apply(scope.array.sort(function (a, b) {
                    return a - b
                }));
            });
        }
    }
});

// Web URL State Load Spinner(used on page or content load)
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
])

// Handle global LINK click
app.directive('a',
    function () {
        return {
            restrict: 'E',
            link: function (scope, elem, attrs) {
                if ((<any>attrs).ngClick || (<any>attrs).href === '' || (<any>attrs).href === '#') {
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
            (<any>elem).dropdownHover();
        }
    };
});

// Allows to configure bootstrap container type for an element in state config by setting "data: { containerFluid: true }". Default is simple container.
//remove according to DO-1456
//app.directive('container', ['$state', function ($state: ng.ui.IStateService) {
//    return {
//        link: function (scope: ng.IScope, elem) {
//            scope.$watch(() => $state.current, (newState: ng.ui.IState) => {
//                if (newState.data && newState.data.noContainer) {
//                    elem.removeClass('container');
//                    elem.addClass('container-fluid');
//                } else {
//                    elem.removeClass('container-fluid');
//                    elem.addClass('container');
//                }
//            });
//        }
//    };
//}]);

app.directive('stopClickPropagation', ['$rootScope', ($rootScope) => {
    return {
        link: (scope: ng.IScope, elem: ng.IAugmentedJQuery, attrs) => {
            elem.bind('click', (event) => {
                if (typeof attrs['appendToBody'] !== 'undefined') {
                    angular.element(document.body).trigger(event); // This makes the handlers that are bound to the body to be called, without triggering any child nodes events
                }

                event.stopPropagation();
            });
        }
    };
}]);

// temporary solution to reload configuration when action header is clicked.
app.directive('transferClickConfigurePane', () => {
    return {
        link: (scope: ng.IScope, elem: ng.IAugmentedJQuery) => {
            elem.bind('click', (event) => {
                elem.parent().find('.pane-configure-action').click();
            });
        }
    };
});

app.directive('delayedControl', ['$compile', ($compile: ng.ICompileService) => ({
    scope: {
        currentAction: '=',
        field: '=',
        plan: '=',
        change: '='
    },
    template: '',
    link: (scope: ng.IScope, elem: ng.IAugmentedJQuery, attr: ng.IAttributes) => {

        elem.append("<configuration-control plan='plan' current-action='currentAction' field='field' change='change'></configuration-control>");
        $compile(elem.contents())(scope);
    }
})]);

interface IInputFocusAttributes extends ng.IAttributes {
    inputFocus: string;
}

app.directive('inputFocus', ['$parse', ($parse: ng.IParseService) => {
    return {
        restrict: 'A',
        link: (scope: ng.IScope, element: ng.IAugmentedJQuery, attrs: IInputFocusAttributes) => {
            var prevState = false;
            var model = $parse(attrs.inputFocus);
            scope.$watch(model, (value) => {
                if (value && !prevState) {
                    setTimeout(() => { element.focus(); }, 0);
                }
                prevState = !!value;
            });
        }
    };
}]);

app.directive('compareTo', () => {
    return {
        require: "ngModel",
        scope: {
            otherModelValue: "=compareTo"
        },
        link: function (scope: any, element, attributes, ngModel) {

            ngModel.$validators.compareTo = function (modelValue) {
                return modelValue == scope.otherModelValue;
            };

            scope.$watch("otherModelValue", function () {
                ngModel.$validate();
            });
        }
    };
});


app.directive('stickyFooter', [
    '$timeout',
    function ($timeout) {
        return {
            restrict: 'A',
            link: function (scope, iElement, iAttrs) {
                var stickyFooterWrapper = $(iAttrs.stickyFooter);

                stickyFooterWrapper.parents().css('height', '100%');
                stickyFooterWrapper.css({
                    'min-height': '100%',
                    'height': 'auto'
                });

                // Append a pushing div to the stickyFooterWrapper.
                var stickyFooterPush = $('<div class="push-footer"></div>');
                stickyFooterWrapper.append(stickyFooterPush);

                var setHeights = function () {
                    var height = iElement.outerHeight();
                    stickyFooterPush.height(height);
                    stickyFooterWrapper.css('margin-bottom', -(height));
                };

                $timeout(setHeights, 0);
                $(window).on('resize', setHeights);
            }
        };
    }
]);

app.directive('eventAdd', ['$timeout', '$window', function ($timeout, $window) {
    return {
        restrict: 'A',
        link: function (scope, element) {
            element.on('click', function () {
                if ($window.analytics != null) {
                    $window.analytics.track("Clicked Add Plan Button");
                }
            });
        }
    };
}]);

app.directive('eventRun', ['$timeout', '$window', function ($timeout, $window) {
    return {
        restrict: 'A',
        link: function (scope, element) {
            element.on('click', function () {
                if ($window.analytics != null) {
                    $window.analytics.track("Clicked Run Plan Button");
                }
            });
        }
    };
}]);

app.directive('eventAuthDialog', ['$timeout', '$window', function ($timeout, $window) {
    return {
        restrict: 'A',
        link: function (scope, element) {
            if ($window.analytics != null) {
                $window.analytics.track("Auth Dialog Opened");
            }
        }
    };
}]);

app.directive('eventPlanbuilder', ['$timeout', '$window', function ($timeout, $window) {
    return {
        restrict: 'A',
        link: function (scope, element) {
            if ($window.analytics != null) {
                $window.analytics.page("Visited Page - Plan Builder");
            }
        }
    };
}]);

app.directive('eventPlandashboard', ['$timeout', '$window', function ($timeout, $window) {
    return {
        restrict: 'A',
        link: function (scope, element) {
            if ($window.analytics != null) {
                $window.analytics.page("Visited Page - Plan Dashboard");
            }
        }
    };
}]);

//== scroll grey area of PB vertically and horizontally 
app.directive('pbScrollPane', ['$timeout', '$window', function ($timeout, $window) {
    return {
        restrict: 'A',
        link: function (scope, element) {
            let _validScrollFlag = false;
            let $scroller = null;
            let curSrollTop = 0;
            let curScrollLeft = 0;

            $scroller = (<any>$(element)).kinetic();

            $(element).on('mousedown', function (e) {
                let startObj = e.target,
                    posX = e.pageX,
                    posY = e.pageY;

                curSrollTop = $(element).scrollTop();
                curScrollLeft = $(element).scrollLeft();

                var impossibleObjs = $('#scrollPane .action');                

                _validScrollFlag = true;               

                angular.forEach(impossibleObjs, (elem) => {
                    let w = $(elem).width(),
                        h = $(elem).height(),
                        objPos = $(elem).offset();

                    if (posX >= objPos.left && posX <= objPos.left + w && posY >= objPos.top && posY <= objPos.top + h) {
                        _validScrollFlag = false;                        
                    }
                });

                if (_validScrollFlag) $scroller.kinetic('attach');
                else $scroller.kinetic('detach');
            });            
        }
    };
}]);

app.directive('activityFullHeight', ['$timeout', '$window', function ($timeout, $window) {
    return {
        restrict: 'A',
        link: function (scope: ng.IScope, element) {
            
            angular.element(window).bind('resize', function () {
                setHeight();
            });

            $timeout(setHeight);                   
            
            function setHeight() {                
                var winH = $(window).height();
                var wrapH = winH - 100;

                $(element).find('.page-container').height(wrapH);
                $(element).find('.route-builder-container').height(wrapH);
                $(element).find('.action').height(wrapH - 10);
                $(element).find('.action').css('margin-bottom', '0px');
                $(element).find('.ng-scope').css('margin-top', '0px');
                $('.page-content').removeClass("page-content");                    
            }
        }
    };
}]);
