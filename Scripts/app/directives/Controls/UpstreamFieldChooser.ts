/// <reference path="../../_all.ts" />
module dockyard.directives.upstreamDataChooser {
    'use strict';
    import upstreamFieldChooserEvents = dockyard.Fr8Events.UpstreamFieldChooser;
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
        ok: () => void;
        cancel: () => void;
    }

    //More detail on creating directives in TypeScript: 
    //http://blog.aaronholmes.net/writing-angularjs-directives-as-typescript-classes/
    class UpstreamFieldChooser implements ng.IDirective {
        public link: (scope: IUpstreamFieldChooser, element: ng.IAugmentedJQuery, attrs: ng.IAttributes) => void;
        public controller: ($scope: IUpstreamFieldChooser, element: ng.IAugmentedJQuery, attrs: ng.IAttributes, UpstreamExtractor: services.UpstreamExtractor, $modal: any) => void;

        public template = '<button class="btn btn-primary btn-xs" ng-click="openModal()">Select</button><span>{{field.value}}</span>';
        public restrict = 'E';
        public scope = {
            field: '=',
            currentAction: '=',
            change: '&'
        }

        private CrateHelper: services.CrateHelper;

        constructor(CrateHelper: services.CrateHelper, NgTableParams) {
            this.CrateHelper = CrateHelper;

            UpstreamFieldChooser.prototype.link = (
                scope: IUpstreamFieldChooser,
                element: ng.IAugmentedJQuery,
                attrs: ng.IAttributes) => {

            }

            UpstreamFieldChooser.prototype.controller = (
                $scope: IUpstreamFieldChooser,
                $element: ng.IAugmentedJQuery,
                $attrs: ng.IAttributes,
                UpstreamExtractor: services.UpstreamExtractor,
                $modal: any            ) => {
                var modalInstance;
                $scope.openModal = () => {
                    getUpstreamFields();
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
                        modalInstance.result.then(function (selectedItem) {
                            $scope.field.value = selectedItem;
                        });
                    }
                }
                $scope.setItem = (item) => {
                    $scope.selectedFieldValue = item;
                };
                $scope.ok = function () {
                    modalInstance.close($scope.selectedFieldValue);
                    if ($scope.change != null && angular.isFunction($scope.change)) {
                        $scope.change()($scope.field);
                    }
                };
                $scope.cancel = function () {
                    modalInstance.dismiss('cancel');
                };
                var getUpstreamFields = () => {
                    UpstreamExtractor
                        .getAvailableData($scope.currentAction.id, 'Field Description')
                        .then((data: any) => {
                            var listItems: Array<model.DropDownListItem> = [];

                            angular.forEach(<Array<any>>data.availableFields, it => {
                                //var fields = <Array<model.FieldDTO>>cm;
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
                            if (listItems.length === 0) {
                                $scope.$emit(<any>upstreamFieldChooserEvents.NO_UPSTREAM_FIELDS, new AlertEventArgs());
                            }
                            else {
                                $scope.field.listItems = listItems;
                                $scope.tableParams = new NgTableParams({ count: 50 }, { data: $scope.field.listItems, counts: [], groupBy: 'label', groupOptions: { isExpanded: false } });   
                            }
                        });
                };
            }
            UpstreamFieldChooser.prototype.controller['$inject'] = ['$scope', '$element', '$attrs', 'UpstreamExtractor', '$modal'];
        };


        //The factory function returns Directive object as per Angular requirements
        public static Factory() {
            var directive = (CrateHelper: services.CrateHelper, NgTableParams) => {
                return new UpstreamFieldChooser(CrateHelper, NgTableParams);
            };

            directive['$inject'] = ['CrateHelper', 'NgTableParams'];
            return directive;
        }
    }

    app.directive('upstreamFieldChooser', UpstreamFieldChooser.Factory());
} 