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

                elem.css({
                    'min-width': getOptimalWidth(),
                    'width': getOptimalWidth(),
                    'min-height': getOptimalHeight() 
                }).resizable({ grid: [1, 10000]});
            }
        }
    }

    export interface IResizableScope extends ng.IScope {
        callback: any;
        height: number;
        resize: (events, ui) => void;
    }

    app.directive('activityResize', dockyard.directives.Resizable);
}