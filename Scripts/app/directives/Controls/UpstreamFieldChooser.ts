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
        expand: () => void;
    }

    export class UpstreamFieldChooserController {

        static $inject = ['$scope', '$element', '$attrs', 'UpstreamExtractor', '$modal', '$timeout','NgTableParams'];
        constructor($scope: IUpstreamFieldChooser,
            $element: ng.IAugmentedJQuery,
            $attrs: ng.IAttributes,
            UpstreamExtractor: services.UpstreamExtractor,
            $modal: any,
            $timeout: any,
            NgTableParams) {
            var modalInstance;
            $scope.expand = () => {
                $timeout(function () {
                    $(".modal-content").parent().css({

                        'height': () => { return $(".modal-header.ng-scope").height() + 40 }
                    })
                }, 0);
            }
            $scope.openModal = () => {
                getUpstreamFields();
                if ($scope.field.listItems.length !== 0) {
                    modalInstance = $modal.open({
                        animation: true,
                        templateUrl: '/AngularTemplate/UpstreamFieldChooser',
                        scope: $scope,
                        controller: ['$scope', '$modalInstance', function ($scope, $modalInstance) {
                            $timeout(function () {
                                (<any>$(".modal-content"))
                                    .wrap('<div align="center"></div>')
                                    .css({
                                        'overflow': 'hidden',
                                        'max-height': () => { return $(document).height() - 80 }
                                    })
                                    .parent()
                                    .css({
                                        'min-width': () => {
                                            var minWidth = parseInt($(".modal-content").attr("min-width"));
                                            if (angular.isNumber(minWidth) && !isNaN(minWidth) && minWidth !== 0) {
                                                return minWidth;
                                            }
                                            var curWidth = $(".modal-content").width();
                                            return curWidth > 0 ? curWidth : 300;
                                        },
                                        'width': () => {
                                            var minWidth = parseInt($(".modal-content").attr("min-width"));
                                            if (angular.isNumber(minWidth) && !isNaN(minWidth) && minWidth !== 0) {
                                                return minWidth;
                                            }
                                            var curWidth = $(".modal-content").width();
                                            return curWidth > 0 ? curWidth : 300;
                                        },
                                        'min-height': () => { return $(".modal-header").height() },
                                        'max-height': () => {
                                            var curHeight = $(".modal-content").height();
                                            if (curHeight > 85) {
                                                return curHeight;
                                            }
                                        }
                                    })
                                    .resizable().draggable()
                                    .find($(".modal-content"))
                                    .css({
                                        overflow: 'auto',
                                        width: '100%',
                                        height: '100%'
                                    });
                            }, 0);
                        }],
                        resolve: {
                            items: function () {
                                return $scope.field.listItems;
                            }
                        }
                    });
                }
            }
            $scope.setItem = (item) => {
                $scope.field.value = item;
                modalInstance.close($scope.field.value);
                (<any>$(".modal-content")).parent().css({
                    'min-height': 0
                });
                if ($scope.change != null && angular.isFunction($scope.change)) {
                    $scope.change()($scope.field);
                }
            };

            var getUpstreamFields = () => {
                UpstreamExtractor
                    .getAvailableData($scope.currentAction.id, 'NotSet')
                    .then((data: any) => {
                        var listItems: Array<model.DropDownListItem> = [];

                        angular.forEach(<Array<any>>data.incomingFields, it => {
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
    }

    //More detail on creating directives in TypeScript: 
    //http://blog.aaronholmes.net/writing-angularjs-directives-as-typescript-classes/
    class UpstreamFieldChooser implements ng.IDirective {
        public link: (scope: IUpstreamFieldChooser, element: ng.IAugmentedJQuery, attrs: ng.IAttributes) => void;
        public controller: any;

        public template = '<button class="btn btn-primary btn-xs" ng-click="openModal()">Select</button><span>{{field.value}}</span>';
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