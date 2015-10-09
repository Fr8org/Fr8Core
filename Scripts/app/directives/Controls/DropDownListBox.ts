/// <reference path="../../_all.ts" />
module dockyard.directives.dropDownListBox {
    'use strict';

    import pca = dockyard.directives.paneConfigureAction;

    export interface IDropDownListBoxScope extends ng.IScope {
        field: model.DropDownListBoxField;
        change: () => (fieldName: string) => void;
        selectedItem: model.DropDownListItem;
        SetSelectedItem: (item: model.DropDownListItem) => void;
        defaultitem: any;
    }

    //More detail on creating directives in TypeScript: 
    //http://blog.aaronholmes.net/writing-angularjs-directives-as-typescript-classes/
    class DropDownListBox implements ng.IDirective {
        public link: (scope: IDropDownListBoxScope, element: ng.IAugmentedJQuery, attrs: ng.IAttributes) => void;
        public templateUrl = '/AngularTemplate/DropDownListBox';
        public controller: ($scope: IDropDownListBoxScope, element: ng.IAugmentedJQuery, attrs: ng.IAttributes) => void;
        public scope = {
            field: '=',
            change: '&'
        };

        public restrict = 'E';
        private _$element: ng.IAugmentedJQuery;
        private _$scope: IDropDownListBoxScope;

        constructor() {
            DropDownListBox.prototype.link = (
                scope: IDropDownListBoxScope,
                element: ng.IAugmentedJQuery,
                attrs: ng.IAttributes) => {

                //Link function goes here
            };

            DropDownListBox.prototype.controller = (
                $scope: IDropDownListBoxScope,
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
                return new DropDownListBox();
            };

            directive['$inject'] = [];
            return directive;
        }
    }

    app.directive('dropDownListBox', DropDownListBox.Factory());
}