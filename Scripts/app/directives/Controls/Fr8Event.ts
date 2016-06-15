/// <reference path="../../_all.ts" />
module dockyard.directives {
    'use strict';

    export interface IFr8EventScope extends ng.IScope {
        event: any;
        type: string;
        color: string;
        icon: string;
        eventType: string;
        domEl: ng.IAugmentedJQuery;
        collapsed: boolean;
        isCollapsed: boolean;
        toggle: () => void;        
    }

    export function Fr8Event($compile): ng.IDirective {
        var controller = ['$scope', ($scope: IFr8EventScope) => {
            $scope.toggle = function () {
                $scope.isCollapsed = !$scope.isCollapsed;
            }
            let template = `
                    <md-list-item class="md-2-line" ng-class="{'fr8-collapse-collapsed': $scope.isCollapsed}">
                        <div style="overflow:auto" class="md-list-item-text" layout="column"></div>
                        <md-divider ng-hide="$scope.isCollapsed"></md-divider>
                    </md-list-item>`;
            $scope.domEl = $compile(template)($scope);

            // Replace "height: auto" with computed height to be able to animate actual height instead of max-height
            $scope.$watch('isCollapsed', (newValue, oldValue) => {
                if (newValue && !oldValue) {
                    $scope.domEl.css('height', $scope.domEl.height() + 'px');
                } else {
                    $scope.domEl.css('height', 'auto');
                }
            });

            if ($scope.type === 'fr8pusher_generic_success') {
                $scope.color = 'green';
                $scope.eventType = 'Success';
                $scope.icon = 'fa fa-check';
            }
            else if ($scope.type === 'fr8pusher_generic_failure') {
                $scope.color = 'red';
                $scope.eventType = 'Failure';
                $scope.icon = 'fa fa-times';
            }
            else if ($scope.type === 'fr8pusher_activity_execution_info') {
                $scope.eventType = 'Execution';
                $scope.icon = 'fa fa-cogs';
            }
            else if ($scope.type === 'fr8pusher_terminal_event') {
                $scope.eventType = $scope.event.TerminalName+ '-v' + $scope.event.TerminalVersion;
                $scope.icon = 'fa fa-bolt';
            }
        }];

        return {
            templateUrl: '/AngularTemplate/Fr8Event',
            controller: controller,
            transclude: true,
            scope: {
                event: '=',
                type: '@'
            },
            link: (scope: IFr8EventScope,
                element: ng.IAugmentedJQuery,
                attrs: ng.IAttributes,
                transclude: ng.ITranscludeFunction): void => {
                transclude((clone) => {
                    scope.domEl.children('.md-list-item-text').append(clone);
                    element.parents('md-list-item').after(scope.domEl);
                    element.before($compile('<md-icon md-font-icon="fa-chevron-circle-down" class="fa fa-lg md-secondary fr8-collapse-marker"></md-icon>')(scope))
                });
            }
        };
    }
    app.directive('fr8Event', Fr8Event);
    Fr8Event.$inject = ['$compile'];
}
