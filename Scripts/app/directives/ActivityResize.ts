/// <reference path="../_all.ts" />

module dockyard.directives {
    'use strict';

    export function Resizable(): ng.IDirective {
        const defaultWidth: number = 300;
        return {
            restrict: 'A',
            link: (scope: IResizableScope, elem, attrs) => {
                elem
                    .wrap('<div/>')
                    .css({ 'overflow': 'hidden' })
                    .parent()
                    .css({
                        'display': 'inline-block',
                        'overflow': 'hidden',
                        'min-width': () => {
                            var minWidth = parseInt(attrs.minwidth);
                            if (angular.isNumber(minWidth) && !isNaN(minWidth) && minWidth !== 0) {
                                return minWidth;
                            }
                            var curWidth = $(elem, this).width();
                            return curWidth > 0 ? curWidth : defaultWidth;
                        },
                        'width': () => {
                            var minWidth = parseInt(attrs.minwidth);
                            if (angular.isNumber(minWidth) && !isNaN(minWidth) && minWidth !== 0) {
                                return minWidth;
                            }
                            var curWidth = $(elem, this).width();
                            return curWidth > 0 ? curWidth : defaultWidth;
                        },
                        'min-height': () => { return $(elem, this).height() + 20; }
                    }).resizable()
                    .find(elem)
                    .css({
                        overflow: 'auto',
                        width: '100%',
                        height: '96%'
                    })
                    ;
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