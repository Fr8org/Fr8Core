/// <reference path="../../_all.ts" />
module dockyard.directives {
    'use strict';

    // --------------------------------------------------------------------------------
    // Template for SelectActivity modal.
    // --------------------------------------------------------------------------------
    const SelectActivityTemplate =
        '<div class="modal-header">\
            <h4>Please, select activity from the list</h4>\
        </div>\
        <div class="modal-body">\
            <div style="overflow-y: auto; max-height: 500px">\
            </div>\
        </div>';

    // --------------------------------------------------------------------------------
    // ActivityChooser directive.
    // Main directive for ActivityChooser control.
    // --------------------------------------------------------------------------------
    export function ActivityChooser(): ng.IDirective {
        return {
            restrict: 'E',
            templateUrl: '/AngularTemplate/ActivityChooser',
            scope: {
                field: '=',
                currentAction: '=',
                plan: '='
            },
            controller: ['$scope', '$modal',
                function (
                    $scope: IActivityChooserScope,
                    $modal: any
                ) {
                    $scope.selectActivity = () => {
                        $modal.open({
                            template: SelectActivityTemplate,
                            controller: 'ACSelectActivityController'
                        });
                    };
                }
            ]
        }
    }

    // --------------------------------------------------------------------------------
    // IActivityChooserScope interface.
    // Scope for ActivityChooser directive.
    // --------------------------------------------------------------------------------
    interface IActivityChooserScope extends ng.IScope {
        plan: model.PlanDTO;
        currentAction: model.ActivityDTO;
        field: model.ActivityChooser;

        selectActivity: () => void;
    }

    // --------------------------------------------------------------------------------
    // ActivityChooserSelectActivityController controller.
    // Controller for handling SelectActivity modal triggered by ActivityChooser directive.
    // --------------------------------------------------------------------------------
    export var ACSelectActivityController = [
        '$scope',
        '$http',
        function ($scope: IACSelectActivityControllerScope, $http: ng.IHttpService) {
            // Perform HTTP-request to extract activity-templates from Hub.
        }
    ];

    // --------------------------------------------------------------------------------
    // IACSelectActivityControllerScope interface.
    // Scope for ACSelectActivityController controller.
    // --------------------------------------------------------------------------------
    interface IACSelectActivityControllerScope extends ng.IScope {

    }
}

app.directive('activityChooser', dockyard.directives.ActivityChooser);
app.controller('ACSelectActivityController', dockyard.directives.ACSelectActivityController);
