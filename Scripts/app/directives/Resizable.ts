/// <reference path="../_all.ts" />

module dockyard.directives {
    'use strict';

    export function Resizable(): ng.IDirective {
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
                        'min-width': function () {
                            if (angular.isNumber(parseInt(attrs.minwidth)) && !isNaN(parseInt(attrs.minwidth))) {
                                return parseInt(attrs.minwidth);
                            }
                            return $(elem, this).width();
                        },
                        'min-height': function () { return $(elem, this).height() + 20; },
                    }).resizable()
                    .find(elem)
                    .css({
                        overflow: 'auto',
                        width: '100%',
                        height: '96%'
                    });
            }
        }
    }

    export interface IResizableScope extends ng.IScope {
        callback: any;
        height: number;
        resize: (events, ui) => void;
    }

    app.directive('resizable', dockyard.directives.Resizable);
}