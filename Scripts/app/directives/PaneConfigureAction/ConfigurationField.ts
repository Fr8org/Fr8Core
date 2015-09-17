/// <reference path="../../_all.ts" />
module dockyard.directives.paneConfigureAction {
    'use strict';

    enum FieldType {
        textField,
        checkboxField,
        filePicker,
        radioGroupButton,
        dropdownlistField
    }

    export interface IConfigurationFieldScope extends ng.IScope {
        field: model.ConfigurationField;
        OnExitFocus: (radio: model.ConfigurationField) => void;
    }

    //More detail on creating directives in TypeScript: 
    //http://blog.aaronholmes.net/writing-angularjs-directives-as-typescript-classes/
    class ConfigurationField implements ng.IDirective {
        public link: (scope: IConfigurationFieldScope, element: ng.IAugmentedJQuery, attrs: ng.IAttributes) => void;
        public controller: ($scope: IConfigurationFieldScope, element: ng.IAugmentedJQuery, attrs: ng.IAttributes) => void;
        public scope = {
            field: '='
        };
        public templateUrl = '/AngularTemplate/ConfigurationField';
        public restrict = 'E';

        private _$scope: IConfigurationFieldScope;

        constructor() {
            ConfigurationField.prototype.link = (
                $scope: IConfigurationFieldScope,
                $element: ng.IAugmentedJQuery,
                $attrs: ng.IAttributes) => {
            };

            ConfigurationField.prototype.controller = (
                $scope: IConfigurationFieldScope,
                $element: ng.IAugmentedJQuery,
                $attrs: ng.IAttributes) => {

                this._$scope = $scope;
                $scope.OnExitFocus = <(radio: model.ConfigurationField) => void> angular.bind(this, this.OnExitFocus);
                
            };
        }

        //The factory function returns Directive object as per Angular requirements
        public static Factory() {
            var directive = () => {
                return new ConfigurationField();
            };

            directive['$inject'] = [];
            return directive;
        }

        private OnExitFocus(field: model.ConfigurationField, scope: IConfigurationFieldScope) {
            console.log("on exit focus called");
            this._$scope.$emit("OnExitFocus", this._$scope);
        }
    }

    app.directive('configurationField', ConfigurationField.Factory());
}