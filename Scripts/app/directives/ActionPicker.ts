/// <reference path="../_all.ts" />

module dockyard.directives {
    'use strict';

    import psa = dockyard.directives.paneSelectAction;
    export function ActionPicker(): ng.IDirective {
        return {
            restrict: 'E',
            templateUrl: '/AngularTemplate/ActionPicker',
            controller: ['$scope', 'WebServiceService',
                function (
                    $scope: IActionPickerScope,
                    webServiceService: services.IWebServiceService
                    ) {

                    $scope.actionCategories = [
                        { id: 1, name: "Monitor", description: "Learn when something happen", icon: "eye" },
                        { id: 2, name: "Get", description: "In-process Crates from a web service", icon: "download" },
                        { id: 3, name: "Process", description: "Carry out work on a Container", icon: "recycle" },
                        { id: 4, name: "Forward", description: "Send Crates to a web service", icon: "share" }];
                    $scope.activeCategory = NaN;
                    $scope.activeTerminal = NaN;

                    $scope.setActive = <() => void> function (actionCategoryId) {
                        $scope.activeCategory == actionCategoryId ? $scope.activeCategory = NaN : $scope.activeCategory = actionCategoryId;
                        $scope.webServiceActionList = webServiceService.getActivities([$scope.activeCategory]);
                        $scope.activeTerminal = NaN;
                        console.log($scope.webServiceActionList);
                    };

                    $scope.setActiveAction = <() => void> function (action, group) {
                        $scope.activeCategory = NaN;
                        $scope.activeCategory = NaN

                        if (group == undefined) {
                            group = null;
                        }

                        var eventArgs = new psa.ActivityTypeSelectedEventArgs(action, group);
                        $scope.$emit(psa.MessageType[psa.MessageType.PaneSelectAction_ActivityTypeSelected], eventArgs);

                    };

                    $scope.deactivateTerminal = <() => void> function () {
                        $scope.activeTerminal = NaN
                    };

                    $scope.setActiveTerminal = <() => void> function (index) {
                        $scope.activeTerminal = index;
                    };
                }
            ]
        }
    }

    export interface IActionPickerScope extends ng.IScope {
        webServiceActionList: Array<model.WebServiceActionSetDTO>;
        actionCategories: any;
        activeCategory: any;
        activeTerminal: any;
        setActive: () => void;
        setActiveTerminal: () => void;
        deactivateTerminal: () => void;
        setActiveAction: () => void;
    }
}

app.directive('actionPicker', dockyard.directives.ActionPicker);