/// <reference path="../../_all.ts" />
module dockyard.directives.paneConfigureAction {
    'use strict';

    enum FieldType {
        textField,
        checkboxField,
        filePicker,
        radioGroupButton,
        dropdownlistField,
        textBlockField,
        routing,
    }

    export class ChangeEventArgs {
        constructor(fieldName: string) {
            this.fieldName = fieldName;
        }

        public fieldName: string;
    }

    export interface IConfigurationFieldScope extends ng.IScope {
        field: model.ControlDefinitionDTO;
        onChange: (radio: model.ControlDefinitionDTO) => void;
    }

    //More detail on creating directives in TypeScript: 
    //http://blog.aaronholmes.net/writing-angularjs-directives-as-typescript-classes/
    class ConfigurationField implements ng.IDirective {
        public link: (scope: IConfigurationFieldScope, element: ng.IAugmentedJQuery, attrs: ng.IAttributes) => void;
        public controller: ($scope: IConfigurationFieldScope, element: ng.IAugmentedJQuery, attrs: ng.IAttributes) => void;
        public scope = {
            currentAction: '=',
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
                $scope.onChange = <(radio: model.ControlDefinitionDTO) => void> angular.bind(this, this.onChange);

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

        private onChange(event: any) {
            var fieldName: string;

            if (!!event.target === true) {
                // If called by DOM event (for standard fields), get field name
                // Get name of field that received the event
                fieldName = event.target.attributes.getNamedItem('data-field-name').value;
            }
            else {
                // If called by custom field, it is assumed that field name is suppied as the argument
                fieldName = event;
            }

            this._$scope.$emit("onChange", new ChangeEventArgs(fieldName));
        }
    }

    app.directive('configurationField', ConfigurationField.Factory());

    // A simple filter to format a string as a valid HTML identifier
    // per http://www.w3.org/TR/html4/types.html#type-id 
    app.filter('validId', function () {
        return function (input) {
            if (input) {
                return input.replace(/^[^a-zA-Z]/, 'a').replace(/[^\w\d\-_\.:]/g, '-');
            }
        }
    });

}