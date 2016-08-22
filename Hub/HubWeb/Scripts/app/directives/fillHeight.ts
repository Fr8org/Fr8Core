
module dockyard.directives {
    app.directive('fillHeight', ['$window', '$document', '$timeout', function ($window, $document, $timeout) {
        return {
            restrict: 'A',
            scope: {
                footerElementId: '@',
                additionalPadding: '@',
                debounceWait: '@'
            },
            link: function (scope, element, attrs) {
                if (scope.debounceWait === 0) {
                    angular.element($window).on('resize', onWindowResize);
                } else {
                    // allow debounce wait time to be passed in.
                    // if not passed in, default to a reasonable 250ms
                    angular.element($window).on('resize', debounce(onWindowResize, scope.debounceWait || 250));
                }
                $timeout(function () {
                    onWindowResize();
                }, 0);
                

                // returns a fn that will trigger 'time' amount after it stops getting called.
                function debounce(fn, time) {
                    var timeout;
                    // every time this returned fn is called, it clears and re-sets the timeout
                    return function () {
                        var context = this;
                        // set args so we can access it inside of inner function
                        var args = arguments;
                        var later = function () {
                            timeout = null;
                            fn.apply(context, args);
                        };
                        $timeout.cancel(timeout);
                        timeout = $timeout(later, time);
                    };
                }

                function onWindowResize() {
                    var footerElement = angular.element($document[0].getElementById(scope.footerElementId));
                    var footerElementHeight;

                    if (footerElement.length === 1) {
                        footerElementHeight = footerElement[0].offsetHeight
                            + getTopMarginAndBorderHeight(footerElement)
                            + getBottomMarginAndBorderHeight(footerElement);
                    } else {
                        footerElementHeight = 0;
                    }

                    var elementOffsetTop = element[0].offsetTop;
                    var elementBottomMarginAndBorderHeight = getBottomMarginAndBorderHeight(element);

                    var additionalPadding = scope.additionalPadding || 0;

                    var elementHeight = $window.innerHeight
                        - elementOffsetTop
                        - elementBottomMarginAndBorderHeight
                        - footerElementHeight
                        - additionalPadding;
                    element.css('height', elementHeight + 'px');
                }

                function getTopMarginAndBorderHeight(element) {
                    var footerTopMarginHeight = getCssNumeric(element, 'margin-top');
                    var footerTopBorderHeight = getCssNumeric(element, 'border-top-width');
                    return footerTopMarginHeight + footerTopBorderHeight;
                }

                function getBottomMarginAndBorderHeight(element) {
                    var footerBottomMarginHeight = getCssNumeric(element, 'margin-bottom');
                    var footerBottomBorderHeight = getCssNumeric(element, 'border-bottom-width');
                    return footerBottomMarginHeight + footerBottomBorderHeight;
                }

                function getCssNumeric(element, propertyName) {
                    return parseInt(element.css(propertyName), 10) || 0;
                }
            }
        };
    }]);
}