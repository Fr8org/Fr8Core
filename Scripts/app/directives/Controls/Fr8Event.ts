/// <reference path="../../_all.ts" />
module dockyard.directives {
    'use strict';

    export interface IFr8EventScope extends ng.IScope {
        event: any;
        type: string;
        color: string;
        icon: string;
        eventType: string;
        isCollapsed: boolean;
        toggle: () => void;        
    }

    export function Fr8Event(): ng.IDirective {
        var controller = ['$scope', ($scope: IFr8EventScope) => {
            if ($scope.event.Collapsed != null) {
                $scope.isCollapsed = $scope.event.Collapsed;
            }
            else {
                $scope.isCollapsed = true;
            }
            $scope.toggle = function () {
                $scope.isCollapsed = !$scope.isCollapsed;
            }
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
            scope: {
                event: '=',
                type: '@'
            }
        };
    }
    app.directive('fr8Event', Fr8Event);
}
