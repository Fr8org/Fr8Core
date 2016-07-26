/// <reference path="../../_all.ts" />
module dockyard.directives {
    'use strict';

    export interface IFr8EventScope extends ng.IScope {
        color: string;
        event: any;
        eventHeader: string;
        eventSubHeader: string;
        eventMessage: string;
        icon: string;
        isCollapsed: boolean;
        toggle: () => void;   
        type: dockyard.enums.NotificationType;
    }

    export function Fr8Event(): ng.IDirective {
        var controller = ['$scope', ($scope: IFr8EventScope) => {
            // Checks whether notification will be shown open or not
            $scope.isCollapsed = true;
            if ($scope.event.Collapsed != null) {
                $scope.isCollapsed = $scope.event.Collapsed;
            }

            $scope.toggle = function () {
                $scope.isCollapsed = !$scope.isCollapsed;
            }

            // Determines notification type and add necessary attributes
            switch ($scope.type) {
                case dockyard.enums.NotificationType.GenericSuccess:
                    $scope.eventHeader = $scope.event.Subject ? $scope.event.Subject : 'Success';
                    $scope.eventSubHeader = null;
                    $scope.eventMessage = $scope.event.Message;
                    $scope.color = 'green';
                    $scope.icon = 'fa-check';
                    break;
                case dockyard.enums.NotificationType.GenericFailure:
                    $scope.eventHeader = 'Failure';
                    $scope.eventSubHeader = null;
                    $scope.eventMessage = $scope.event.Message;
                    $scope.color = 'red';
                    $scope.icon = 'fa-times';
                    break;
                case dockyard.enums.NotificationType.GenericInfo:
                    $scope.eventHeader = 'Executing Activity';
                    $scope.eventSubHeader = $scope.event.ActivityName;
                    $scope.eventMessage = $scope.event.Message;
                    $scope.icon = 'fa-cogs';
                    break;
                case dockyard.enums.NotificationType.TerminalEvent:
                    if ($scope.event.Subject) {
                        $scope.eventHeader = $scope.event.Subject;
                    } else {
                        $scope.eventHeader = $scope.event.TerminalName + '-v' + $scope.event.TerminalVersion;
                        $scope.eventSubHeader = $scope.event.ActivityName + '-v' + $scope.event.ActivityVersion;
                    }
                    $scope.eventMessage = $scope.event.Message;
                    $scope.icon = 'fa-bolt';
                    break;
                case dockyard.enums.NotificationType.ExecutionStopped:
                    $scope.eventHeader = 'Plan Stopped';
                    $scope.eventMessage = $scope.event.Message + ' has been stopped.';
                    $scope.color = 'firebrick';
                    $scope.icon = 'fa-stop';
            }
        }];

        return {
            templateUrl: '/AngularTemplate/Fr8Event',
            controller: controller,
            scope: {
                event: '=',
                type: '='
            }
        };
    }
    app.directive('fr8Event', Fr8Event);
}
