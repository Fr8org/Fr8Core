/// <reference path="../../_all.ts" />
module dockyard.directives.upstreamDataChooser {
    'use strict';
    export interface IUpstreamFieldChooser extends ng.IScope {
        field: model.DropDownList;
        currentAction: model.ActivityDTO;
        upstreamFields: model.FieldDTO[];
        tableParams: any;
        selectedFieldValue: any;
        change: () => (field: model.ControlDefinitionDTO) => void;
        setItem: (item: any) => void;
        selectField: (field: model.FieldDTO) => void;
        openModal: () => void;
        createModal: () => void;
    }

    export class UpstreamFieldChooserController {

        static $inject = ['$scope', '$element', '$attrs', 'UpstreamExtractor', '$modal', 'NgTableParams', 'UIHelperService'];
        constructor($scope: IUpstreamFieldChooser,
            $element: ng.IAugmentedJQuery,
            $attrs: ng.IAttributes,
            UpstreamExtractor: services.UpstreamExtractor,
            $modal: any,
            NgTableParams,
            uiHelperService: services.IUIHelperService) {

            var modalInstance;
            var noActivitiesWithUpstreamFiels = 'This Activity is looking for incoming data from "upstream" activities but can\'t find any right now. Try adding activities to the left of this activity that load or retrieve data from web services. To learn more,<a href= "/documentation/UpstreamCrates.html" target= "_blank" > click here </a><i class="fa fa-question-circle" > </i> ';
            var activitiesNotConfiguredWithUpstreamFields = 'Activities to the left don\'t have "upstream" fields. To learn more,<a href= "/documentation/UpstreamCrates.html" target= "_blank" > click here </a><i class="fa fa-question-circle" > </i>';
            $scope.createModal = () => {
                if ($scope.field.listItems.length !== 0) {
                    modalInstance = $modal.open({
                        animation: true,
                        templateUrl: '/AngularTemplate/UpstreamFieldChooser',
                        scope: $scope,
                        resolve: {
                            items: function () {
                                return $scope.field.listItems;
                            }
                        }
                    });
                }
            }
            $scope.openModal = () => {
                getUpstreamFields().then(() => { 
                    $scope.createModal();
                }, (error) => {
                    var alertMessage = new model.AlertDTO();
                    alertMessage.title = "Notification";
                    alertMessage.body = error.message;
                    alertMessage.isOkCancelVisible = false;
                    uiHelperService.openConfirmationModal(alertMessage);
                });
            }
            $scope.setItem = (item) => {
                $scope.field.value = item;
                modalInstance.close($scope.field.value);
                if ($scope.change != null && angular.isFunction($scope.change)) {
                    $scope.change()($scope.field);
                }
            };

            var getUpstreamFields = () => {
                return UpstreamExtractor
                    .getAvailableData($scope.currentAction.id, 'NotSet')
                    .then((data: dockyard.model.IncomingCratesDTO) => {
                        var listItems: Array<model.DropDownListItem> = [];

                        angular.forEach(data.availableCrates, crate => {
                            angular.forEach(crate.fields, it => {
                                var i, j;
                                var found = false;
                                for (i = 0; i < listItems.length; ++i) {
                                    if (listItems[i].key === it.key) {
                                        found = true;
                                        break;
                                    }
                                }
                                if (!found) {
                                    listItems.push(<model.DropDownListItem>it);
                                }
                            });
                            listItems.sort((x, y) => {
                                if (x.key < y.key) {
                                    return -1;
                                }
                                else if (x.key > y.key) {
                                    return 1;
                                }
                                else {
                                    return 0;
                                }
                            });
                        });
                        if (listItems.length === 0) {
                            var error = '';
                            if (data.availableCrates.length === 0) {
                                error = noActivitiesWithUpstreamFiels;
                            }
                            else {
                                error = activitiesNotConfiguredWithUpstreamFields
                            }
                            throw Error(error);
                        }
                        else {
                            $scope.field.listItems = listItems;
                            $scope.tableParams = new NgTableParams({ count: $scope.field.listItems.length }, { data: $scope.field.listItems, counts: [], groupBy: 'sourceCrateLabel' });
                        }
                    });
            };
        }
    }

    //More detail on creating directives in TypeScript: 
    //http://blog.aaronholmes.net/writing-angularjs-directives-as-typescript-classes/
    class UpstreamFieldChooser implements ng.IDirective {
        public link: (scope: IUpstreamFieldChooser, element: ng.IAugmentedJQuery, attrs: ng.IAttributes) => void;
        public controller: any;

        public template = '<div style="padding:5px 0"><button class="btn btn-primary btn-xs" ng-click="openModal()">Select</button><span>{{field.value}}</span></div>';
        public restrict = 'E';
        public scope = {
            field: '=',
            currentAction: '=',
            change: '&'
        }

        private CrateHelper: services.CrateHelper;

        constructor(CrateHelper: services.CrateHelper) {
            this.CrateHelper = CrateHelper;

            UpstreamFieldChooser.prototype.link = (
                scope: IUpstreamFieldChooser,
                element: ng.IAugmentedJQuery,
                attrs: ng.IAttributes) => {

            },
            UpstreamFieldChooser.prototype.controller = UpstreamFieldChooserController;
        };


        //The factory function returns Directive object as per Angular requirements
        public static Factory() {
            var directive = (CrateHelper: services.CrateHelper) => {
                return new UpstreamFieldChooser(CrateHelper);
            };

            directive['$inject'] = ['CrateHelper'];
            return directive;
        }
    }

    app.directive('upstreamFieldChooser', UpstreamFieldChooser.Factory());
    app.controller('UpstreamFieldChooserController', UpstreamFieldChooserController);
} 