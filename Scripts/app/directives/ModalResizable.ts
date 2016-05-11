/// <reference path="../_all.ts" />

module dockyard.directives {
    'use strict';

    export class ModalResizable implements ng.IDirective {
        restrict = 'A';
        constructor(private $timeout: ng.ITimeoutService) {}
        link = (scope: IModalResizableScope, elem: ng.IAugmentedJQuery, attrs: ng.IAttributes) => {
                    (<any>$(".modal-content"))
                        .wrap('<div align="center"></div>')
                        .css({
                            'overflow': 'hidden',
                            'max-height': () => { return $(document).height() - 80 }
                        })
                        .parent()
                        .css({
                            'min-width': () => {
                                var minWidth = parseInt($(".modal-content").attr("min-width"));
                                if (angular.isNumber(minWidth) && !isNaN(minWidth) && minWidth !== 0) {
                                    return minWidth;
                                }
                                var curWidth = $(".modal-content").width();
                                return curWidth > 0 ? curWidth : 360;
                            },
                            'width': () => {
                                var minWidth = parseInt($(".modal-content").attr("min-width"));
                                if (angular.isNumber(minWidth) && !isNaN(minWidth) && minWidth !== 0) {
                                    return minWidth;
                                }
                                var curWidth = $(".modal-dialog").width();
                                return curWidth > 0 ? curWidth : 360;
                            },
                            'min-height': () => { return $(".modal-header").height() },
                            'max-height': () => {return $(document).height() - 80; }
                        })
                        .resizable().draggable()
                        .find($(".modal-content"))
                        .css({
                            overflow: 'auto',
                            width: '100%',
                            height: '100%'
                    })
                    scope.expand = () => {
                        this.$timeout(function () {
                            $(".modal-content").parent().css({
                                'height': () => { return $(".modal-header.ng-scope").height() + 40 }
                            })
                        }, 0);
                    }
        }
        static factory(): ng.IDirectiveFactory {
            const directive = ($timeout: ng.ITimeoutService) => new ModalResizable($timeout);
            directive.$inject = ['$timeout'];
            return directive;
        }
    }

    export interface IModalResizableScope extends ng.IScope {
        expand: () => void;
    }

    app.directive('modalResizable', dockyard.directives.ModalResizable.factory());
}