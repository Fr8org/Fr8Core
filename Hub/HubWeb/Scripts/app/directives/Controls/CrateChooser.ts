/// <reference path="../../_all.ts" />
module dockyard.directives.crateChooser {
    'use strict';

    export interface ICrateChooserScope extends ng.IScope {
        field: model.CrateChooser;
        change: () => (field: model.ControlDefinitionDTO) => void;
        selectCrate: () => void;
        onChange: any;
        currentAction: model.ActivityDTO;
    }

    export function CrateChooser(): ng.IDirective {
        var controller = ['$scope', '$modal', 'UpstreamExtractor', (
            $scope: ICrateChooserScope,
            $modal: any,
            UpstreamExtractor: services.UpstreamExtractor
        ) => {
            var onCratesSelected = (selectedCrates: Array<model.CrateDescriptionDTO>) => {
                $scope.onChange();
            };

            $scope.onChange = () => {
                if ($scope.change != null && angular.isFunction($scope.change)) {
                    $scope.change()($scope.field);
                }
            };

            $scope.selectCrate = () => {
                var displayModal = () => {
                    var modalInstance = $modal.open({
                        animation: true,
                        templateUrl: 'TextTemplate-CrateChooserSelectionModal',
                        controller: 'CrateChooser__CrateSelectorModalController',
                        size: 'm',
                        resolve: {
                            'crateDescriptions': () => $scope.field.crateDescriptions,
                            'singleSelection': () => $scope.field.singleManifestOnly,
                            'allowedManifestTypes': () => $scope.field.allowedManifestTypes
                        }
                    });

                    modalInstance.result.then(onCratesSelected);
                };

                if ($scope.field.requestUpstream) {
                    UpstreamExtractor.getAvailableData($scope.currentAction.id, 'NotSet')
                        .then((data: model.IncomingCratesDTO) => {
                            var availableManifests = [];
                            if ($scope.field.allowedManifestTypes) {
                                availableManifests = $scope.field.allowedManifestTypes;
                                for (var i = 0; i < availableManifests.length; i++) {
                                    availableManifests[i] = availableManifests[i].toLowerCase();
                                }
                            }
                            var descriptions = <Array<model.CrateDescriptionDTO>>[];
                            angular.forEach(data.availableCrates, (cd) => {
                                if (availableManifests.length === 0 ||
                                    availableManifests.indexOf(cd.manifestType.toLowerCase()) >= 0) {
                                    descriptions.push(cd);
                                }
                            });

                            $scope.field.crateDescriptions = descriptions;
                            displayModal();
                        });
                }
                else {
                    displayModal();
                }
            };
        }];

        return {
            restrict: 'E',
            templateUrl: '/AngularTemplate/CrateChooser',
            controller: controller,
            scope: {
                field: '=',
                change: '&',
                currentAction: '='
            }
        };
    }

    app.directive('crateChooser', CrateChooser);


    app.controller('CrateChooser__CrateSelectorModalController', ['$scope', '$modalInstance', 'crateDescriptions', 'singleSelection', ($scope: any, $modalInstance: any, crateDescriptions: Array<model.CrateDescriptionDTO>, singleSelection: boolean): void => {

        $scope.tpl = '<crate-node single-selection="' + (singleSelection === true ? 'true' : 'false')+'"></crate-node>';

        var categories = [];
        var findCategory = (crateDescription: model.CrateDescriptionDTO) => {
            for (var i = 0; i < categories.length; i++) {
                if (crateDescription.manifestId === categories[i].manifestId) {
                    return categories[i];
                }
            }
            return null;
        };


        for (var i = 0; i < crateDescriptions.length; i++) {
            var category = findCategory(crateDescriptions[i]);
            if (category == null) {
                category = { label: crateDescriptions[i].label, isCategory: true, children: [] };
                categories.push(category);
            }

            category.children.push(crateDescriptions[i]);
        }

        $scope.crateDescriptions = categories;

        $scope.ok = () => {
            var selections = [];
            for (var i = 0; i < crateDescriptions.length; i++) {
                if (crateDescriptions[i].selected) {
                    selections.push(crateDescriptions[i]);
                }
            }
            $modalInstance.close(selections);
        };

        $scope.cancel = () => {
            $modalInstance.dismiss();
        };

    }]);


    app.directive('crateNode', ['ivhTreeviewMgr', (ivhTreeviewMgr) => {
        return {
            restrict: 'E',
            templateUrl: 'TextTemplate-CrateNode',
            link: (scope, element, attrs) => {
                var singleSelection: boolean = attrs.singleSelection === 'true';
                element.on('click', () => {
                    if (scope.node.isCategory) {
                        return;
                    }
                    var currentState = scope.node.selected;
                    if (singleSelection) {
                        //deselect previous selections first
                        ivhTreeviewMgr.deselectAll(scope.trvw.root());
                    }

                    ivhTreeviewMgr.select(scope.trvw.root(), scope.node, !currentState);
                    scope.$apply();
                });
            }
        };
    }]);
}