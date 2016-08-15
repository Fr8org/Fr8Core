/// <reference path="../_all.ts" />

module dockyard.directives {
    'use strict';

    export function Resizable(): ng.IDirective {
        const defaultWidth: number = 300;
        const defaultHeight: number = 400;
        return {
            restrict: 'A',
            link: (scope: IResizableScope, elem, attrs) => {
                function getOptimalWidth() {
                    var minWidth = parseInt(attrs.minWidth);
                    if (angular.isNumber(minWidth) && !isNaN(minWidth) && minWidth !== 0) {
                        return minWidth;
                    }
                    var curWidth = $(elem, this).width();
                    return curWidth > defaultWidth ? curWidth : defaultWidth;
                };

                function getOptimalHeight() {
                    var minHeight = parseInt(attrs.minHeight);
                    if (angular.isNumber(minHeight) && !isNaN(minHeight) && minHeight !== 0) {
                        return minHeight;
                    }
                    var curHeight = $(elem, this).outerHeight(true);
                    return curHeight > defaultHeight ? curHeight : defaultHeight;
                };

                // Setting title-bar style
                function setTitleSize(width) {
                    var titleObj = $(elem).find(".ellipsis h2");
                    var titleStr = $(titleObj).html();

                    if (typeof titleStr !== typeof undefined) {
                        titleStr = titleStr.trim();
                        let strnum = titleStr.length;
                        let fontSize = 20;
                        let titleWrappedWidth = $(titleObj).width();
                        let expectedWidth = strnum * 10;
                        let cssAttr = {
                            'font-size': '20px',
                            'white-space': 'normal'
                        };

                        if (expectedWidth <= titleWrappedWidth) {
                            $(titleObj).css(cssAttr);
                            return true;
                        }

                        let expectedRatio = titleWrappedWidth / expectedWidth;
                        let fixedFontSize = Math.round(expectedRatio * fontSize);

                        expectedWidth /= 2;
                        if (expectedWidth <= titleWrappedWidth) {
                            fixedFontSize = 20;
                        } else if (fixedFontSize < 15) {
                             expectedRatio = titleWrappedWidth / expectedWidth;
                             fixedFontSize = Math.round(expectedRatio * fontSize);

                             if (fixedFontSize < 15)
                                 fixedFontSize = 15;                            
                        }

                        cssAttr = {
                            'font-size': fixedFontSize + 'px',
                            'white-space': 'normal'
                        };

                        $(titleObj).css(cssAttr);
                    }

                };

                var suspectedWidth = getOptimalWidth();
                elem.css({
                    'min-width': suspectedWidth,
                    'width': suspectedWidth,
                    'min-height': getOptimalHeight()
                }).resizable({ grid: [1, 10000] });

                //listen event of loading finished 
                scope.$on('titleLoadingFinished', function () {
                    setTitleSize(suspectedWidth);
                });

                angular.element(window).bind('resize', function () {
                    var w = $(elem).find(".md-toolbar-tools .ellipsis").width();
                    setTitleSize(w);
                });
            }
        }
    }

    export interface IResizableScope extends ng.IScope {
        callback: any;
        height: number;
        resize: (events, ui) => void;
        flagOneEmit: boolean;
    }

    app.directive('activityResize', dockyard.directives.Resizable);
}