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

    export class ExitFocusEventArgs
    {
        constructor(fieldName: string)
        {
            this.fieldName = fieldName;
        }

        public fieldName: string;
    }

    export interface IConfigurationFieldScope extends ng.IScope {
        field: model.ConfigurationField;
        onFieldChange: (radio: model.ConfigurationField) => void;
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
                $scope.onFieldChange = <(radio: model.ConfigurationField) => void> angular.bind(this, this.onFieldChange);
                
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

        private onFieldChange(event: JQueryMouseEventObject) {
            //Get name of field that received the event
            debugger;
            var fieldName = event.target.attributes.getNamedItem('data-field-name').value,
                eventArgs = new ExitFocusEventArgs(fieldName)
            this._$scope.$emit("onFieldChange", eventArgs);
        }
    }

    app.directive('configurationField', ConfigurationField.Factory());
}