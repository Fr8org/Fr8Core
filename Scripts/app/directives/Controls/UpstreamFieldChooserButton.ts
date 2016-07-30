/// <reference path="../../_all.ts" />
module dockyard.directives.upstreamDataChooser {
    'use strict';
    export interface IUpstreamFieldChooserButtonScope extends ng.IScope {
        field: model.DropDownList;
        currentAction: model.ActivityDTO;
        upstreamFields: model.FieldDTO[];
        tableParams: any;
        selectedFieldValue: any;
        change: () => (field: model.ControlDefinitionDTO) => void;
        selectItem: (field: model.FieldDTO) => void;
        openModal: () => void;
        createModal: () => void;
        getGroupValue: (item: model.FieldDTO) => string;
        isDisabled:boolean;
    }

    import pca = dockyard.directives.paneConfigureAction;

    export class UpstreamFieldChooserButtonController {

        static $inject = ['$scope', '$element', '$attrs', 'UpstreamExtractor', '$modal', 'NgTableParams', 'UIHelperService', '$q'];
        constructor($scope: IUpstreamFieldChooserButtonScope,
            $element: ng.IAugmentedJQuery,
            $attrs: ng.IAttributes,
            UpstreamExtractor: services.UpstreamExtractor,
            $modal: any,
            NgTableParams,
            uiHelperService: services.IUIHelperService,
            $q: ng.IQService) {

            var modalInstance;
            var noActivitiesWithUpstreamFiels = 'This Activity is looking for incoming data from "upstream" activities but can\'t find any right now. Try adding activities to the left of this activity that load or retrieve data from web services. To learn more,<a href= "/documentation/UpstreamCrates.html" target= "_blank" > click here </a><i class="fa fa-question-circle" > </i> ';
            var activitiesNotConfiguredWithUpstreamFields = 'Activities to the left don\'t have "upstream" fields. To learn more,<a href= "/documentation/UpstreamCrates.html" target= "_blank" > click here </a><i class="fa fa-question-circle" > </i>';

            $scope.$on(pca.MessageType[pca.MessageType.PaneConfigureAction_ConfigureCallResponseFinished],
                (event: ng.IAngularEvent, callConfigureResponseEventArgs: pca.CallConfigureResponseEventArgs) => getUpstreamFields());

            $scope.$watch('field.listItems', () => {
                checkAndUpdateUpstreamFields();
            });


            const checkAndUpdateUpstreamFields = () => {
                var isFieldAvailable = $scope.field.listItems.filter((item) => {
                    return item.key === $scope.field.value;
                });
                if (isFieldAvailable.length === 0) {
                            $scope.field.selectedKey = null;
                            $scope.field.value = null;
                            $scope.field.selectedItem = null;
                    
                }
            };

            $scope.createModal = () => {
                if ($scope.field.listItems.length !== 0) {
                    modalInstance = $modal.open({
                        animation: true,
                        templateUrl: '/AngularTemplate/UpstreamFieldChooserDialog',
                        scope: $scope,
                        resolve: {
                            items: function () {
                                return $scope.field.listItems;
                            }
                        }
                    });
                }
            };

            $scope.openModal = () => {
                getUpstreamFields().then(() => {
                    $scope.createModal();
                }, (error) => {
                    var alertMessage = new model.AlertDTO();
                    alertMessage.title = "Notification";
                    alertMessage.body = error;
                    alertMessage.isOkCancelVisible = false;
                    uiHelperService.openConfirmationModal(alertMessage);
                });
            };

            $scope.selectItem = (item) => {
                $scope.field.selectedItem = item;
                $scope.field.value = item.key;
                modalInstance.close($scope.field.value);
                if ($scope.change != null && angular.isFunction($scope.change)) {
                    $scope.change()($scope.field);
                }
            };

            $scope.getGroupValue = (item) => {
                if (item.sourceActivityId == null) {
                    return item.sourceCrateLabel;
                }
                return item.sourceCrateLabel + " (Id - " + item.sourceActivityId + ")";
            };

            var getUpstreamFields = () => {
                return UpstreamExtractor
                    .getAvailableData($scope.currentAction.id, 'NotSet')
                    .then((data: dockyard.model.IncomingCratesDTO) => {
                        var listItems: Array<model.DropDownListItem> = [];

                        angular.forEach(data.availableCrates, crate => {
                            angular.forEach(crate.fields, it => { listItems.push(<model.DropDownListItem>it); });
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
                            return $q.reject(error);
                        }
                        else {
                            $scope.field.listItems = listItems;
                            $scope.tableParams = new NgTableParams({ count: $scope.field.listItems.length }, { data: $scope.field.listItems, counts: [], groupBy: $scope.getGroupValue });
                        }
                    });
            };
        }
    }

    //More detail on creating directives in TypeScript: 
    //http://blog.aaronholmes.net/writing-angularjs-directives-as-typescript-classes/
    class UpstreamFieldChooserButton implements ng.IDirective {
        public link: (scope: IUpstreamFieldChooserButtonScope, element: ng.IAugmentedJQuery, attrs: ng.IAttributes) => void;
        public controller: any;

        public template = '<span><button class="btn btn-primary btn-xs" ng-click="openModal()">incoming data</button><a href="/documentation/UpstreamCrates.html" class="documentation-link"><i class="fa fa-question-circle"></i></a><span></br>{{field.value}}  {{field.selectedItem.sourceCrateLabel}}</span></span>';
        public restrict = 'E';
        public scope = {
            field: '=',
            currentAction: '=',
            change: '&',
            isDisabled:'='
        }

        private CrateHelper: services.CrateHelper;

        constructor(CrateHelper: services.CrateHelper) {
            this.CrateHelper = CrateHelper;

            UpstreamFieldChooserButton.prototype.link = (
                scope: IUpstreamFieldChooserButtonScope,
                element: ng.IAugmentedJQuery,
                attrs: ng.IAttributes) => {

            };
            UpstreamFieldChooserButton.prototype.controller = UpstreamFieldChooserButtonController;
        };


        //The factory function returns Directive object as per Angular requirements
        public static Factory() {
            var directive = (CrateHelper: services.CrateHelper) => {
                return new UpstreamFieldChooserButton(CrateHelper);
            };

            directive['$inject'] = ['CrateHelper'];
            return directive;
        }
    }

    app.directive('upstreamFieldChooserButton', UpstreamFieldChooserButton.Factory());
    app.controller('UpstreamFieldChooserButtonController', UpstreamFieldChooserButtonController);
} 