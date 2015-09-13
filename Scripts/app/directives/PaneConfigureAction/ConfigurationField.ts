/// <reference path="../../_all.ts" />
module dockyard.directives.paneConfigureAction {
    'use strict';

    enum FieldType {
        textField,
        checkboxField,
        filePicker,
        radioGroupButton
    }

    interface IConfigurationFieldScope extends ng.IScope {
        field: model.ConfigurationField;
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
                debugger;
                $scope.$on('onblur', <any>angular.bind(this, this.onBlur));
                
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

        public onBlur(scope: IConfigurationFieldScope) {
            debugger;
            scope.$emit('onExitFocus', scope.field);
                //new MapFieldsClickedEventArgs(angular.extend({}, scope.field)) //clone action to prevent msg recipient from modifying orig. object
                //);
        }
    }

    app.directive('configurationField', ConfigurationField.Factory());
}