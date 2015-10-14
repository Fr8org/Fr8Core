/// <reference path="../../_all.ts" />
module dockyard.directives.dropDownListBox {
    'use strict';

    import pca = dockyard.directives.paneConfigureAction;

    export interface IOldDropDownListBoxScope extends ng.IScope {
        field: model.DropDownListControlDefinitionDTO;
        change: () => (fieldName: string) => void;
        selectedItem: model.DropDownListItem;
        SetSelectedItem: (item: model.DropDownListItem) => void;
        defaultitem: any;
    }

    //More detail on creating directives in TypeScript: 
    //http://blog.aaronholmes.net/writing-angularjs-directives-as-typescript-classes/
    class OldDropDownListBox implements ng.IDirective {
        public link: (scope: IOldDropDownListBoxScope, element: ng.IAugmentedJQuery, attrs: ng.IAttributes) => void;
        public templateUrl = '/AngularTemplate/OldDropDownListBox';
        public controller: ($scope: IOldDropDownListBoxScope, element: ng.IAugmentedJQuery, attrs: ng.IAttributes) => void;
        public scope = {
            field: '=',
            change: '&'
        };

        public restrict = 'E';
        private _$element: ng.IAugmentedJQuery;
        private _$scope: IOldDropDownListBoxScope;

        constructor() {
            OldDropDownListBox.prototype.link = (
                scope: IOldDropDownListBoxScope,
                element: ng.IAugmentedJQuery,
                attrs: ng.IAttributes) => {

                //Link function goes here
            };

            OldDropDownListBox.prototype.controller = (
                $scope: IOldDropDownListBoxScope,
                $element: ng.IAugmentedJQuery,
                $attrs: ng.IAttributes) => {

                $scope.selectedItem = null;
                $scope.SetSelectedItem = function (item: model.DropDownListItem) {
                    $scope.field.value = item.Value;
                    $scope.selectedItem = item;

                    // Invoike onChange event handler
                    if ($scope.change != null && angular.isFunction($scope.change)) {
                        $scope.change()($scope.field.name);
                    }
                };

                var FindAndSetSelectedItem = function () {
                    for (var i = 0; i < $scope.field.listItems.length; i++) {
                        if ($scope.field.value == $scope.field.listItems[i].Value) {
                            $scope.selectedItem = $scope.field.listItems[i];
                            break;
                        }
                    }
                };

                FindAndSetSelectedItem();
                $scope.defaultitem = null;
            };
        }

        //The factory function returns Directive object as per Angular requirements
        public static Factory() {
            var directive = () => {
                return new OldDropDownListBox();
            };

            directive['$inject'] = [];
            return directive;
        }
    }

    app.directive('oldDropDownListBox', OldDropDownListBox.Factory());
}