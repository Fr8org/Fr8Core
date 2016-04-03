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
                <div ng-if="!selectedWebService">\
                    <div ng-repeat="webService in webServiceActivities" ng-click="selectWebService(webService)" style="margin: 0 0 5px 0; cursor: pointer;">\
                        <img ng-src="{{webService.webServiceIconPath}}" style="width: 48px; height: 48px;" />\
                        <span style="font-size: 1.3em">{{webService.webServiceName}}</span>\
                    </div>\
                </div>\
                <div ng-if="selectedWebService">\
                    <div ng-repeat="activity in selectedWebService.activities">\
                        <span style="font-size: 1.3em;">{{activity.name}}</span>\
                    </div>\
                </div>\
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
            var _reloadData = () => {
                $scope.webServiceActivities = [];

                $http.post('/api/webservices/activities', null)
                    .then((res) => {
                        var webServiceActivities = <Array<model.WebServiceActionSetDTO>>res.data;
                        angular.forEach(webServiceActivities, (webServiceActivity) => {
                            $scope.webServiceActivities.push(webServiceActivity);
                        });
                    });
            };

            // Perform web-service selection.
            $scope.selectWebService = (webService: model.WebServiceActionSetDTO) => {
                $scope.selectedWebService = webService;
            };

            // Force reload data.
            _reloadData();
        }
    ];

    // --------------------------------------------------------------------------------
    // IACSelectActivityControllerScope interface.
    // Scope for ACSelectActivityController controller.
    // --------------------------------------------------------------------------------
    interface IACSelectActivityControllerScope extends ng.IScope {
        webServiceActivities: Array<model.WebServiceActionSetDTO>;
        selectedWebService: model.WebServiceActionSetDTO;

        selectWebService: (webService: model.WebServiceActionSetDTO) => void;
    }
}

app.directive('activityChooser', dockyard.directives.ActivityChooser);
app.controller('ACSelectActivityController', dockyard.directives.ACSelectActivityController);
