/// <reference path="../../_all.ts" />

module dockyard.directives.timePicker {
    'use strict';

    export interface ITimePickerScope extends ng.IScope {
        duration: model.Duration;
        pickerChanged: any;
    }

    //More detail on creating directives in TypeScript: 
    //http://blog.aaronholmes.net/writing-angularjs-directives-as-typescript-classes/
    class TimePicker implements ng.IDirective {
        public templateUrl = '/AngularTemplate/TimePicker';
        public link: (scope: ITimePickerScope, element: ng.IAugmentedJQuery, attrs: ng.IAttributes) => void;
        public controller: ($scope: ITimePickerScope, element: ng.IAugmentedJQuery, attrs: ng.IAttributes, $compile: ng.ICompileService) => void;
        public restrict = 'E';
        public scope = {
            duration: '='
        }

        constructor() {

            TimePicker.prototype.link = (
                scope: ITimePickerScope,
                element: ng.IAugmentedJQuery,
                attrs: ng.IAttributes ) => {
            }

            TimePicker.prototype.controller = (
                $scope: ITimePickerScope,
                $element: ng.IAugmentedJQuery,
                $attrs: ng.IAttributes, $compile: ng.ICompileService) => {

                var totalTime = 0;
                var containerEle = angular.element($element).clone();
                var mainInputReplacer = containerEle.children('.bdp-input');
                var picker = containerEle.children("#picker");

                $scope.pickerChanged = function () {
                    if (isNaN($scope.duration.minutes) || isNaN($scope.duration.hours) || isNaN($scope.duration.days))
                        return;
                    updateTotalTime();
                    init();
                }

                function updateTotalTime() {
                    totalTime = $scope.duration.minutes * 60 + $scope.duration.hours * 60 * 60 + $scope.duration.days * 24 * 60 * 60;
                }

                function init() {
                    if (totalTime === 0)
                        updateTotalTime();
                    var total = totalTime;
                    total = Math.floor(total / 60);
                    var minutes = total % 60;
                    total = Math.floor(total / 60);
                    var hours = total % 24;
                    var days = Math.floor(total / 24);
                    if (isNaN(days) || isNaN(hours) || isNaN(minutes))
                        return;
                    $scope.duration.days = days;
                    $scope.duration.hours = hours;
                    $scope.duration.minutes = minutes;
                }

                init();

                var isPopoverVisible = false;

                var showPopover = function (event) {
                    angular.element(this).popover("show");
                    angular.element(this).siblings(".popover").on("mouseleave", function () {
                        isPopoverVisible = false;
                    });
                    angular.element(this).siblings(".popover").on("mouseenter", function () {
                        isPopoverVisible = true;
                    });
                }

                var hidePopover = function (event) {
                    var _this = this ? this : angular.element($element).find('.popover');
                    if (isPopoverVisible === false) {
                        angular.element(_this).popover('hide');
                    } else {
                        angular.element(_this).focus();
                    }
                };

                angular.element('.bdp-input').popover({
                    placement: 'bottom',
                    trigger: 'manual',
                    html: true,
                    content: $compile(picker.html())($scope),
                }).click(showPopover).blur(hidePopover);

                $scope.$watch(function () {
                    return $element.offset().top
                }, function (outOfBorder) {
                    if (outOfBorder) {
                        hidePopover(null);
                    }
                });
            }

            TimePicker.prototype.controller['$inject'] = ['$scope', '$element', '$attrs', '$compile'];
        };

        //The factory function returns Directive object as per Angular requirements
        public static Factory() {
            var directive = () => {
                return new TimePicker();
            };

            directive['$inject'] = [];
            return directive;
        }
    }

    app.directive('timePicker', TimePicker.Factory());
}