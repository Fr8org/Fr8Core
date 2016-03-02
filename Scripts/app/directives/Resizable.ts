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
                        'width': function () { return $(elem, this).width(); },
                        'height': '380px',
                        'paddingBottom': '20px',
                        'min-width': '380px',
                        'min-height': '380px'

                    }).resizable()
                    .find(elem)
                    .css({
                        overflow: 'auto',
                        width: '100%',
                        height: '100%'
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