/// <reference path="../../_all.ts" />
var dockyard;
(function (dockyard) {
    var directives;
    (function (directives) {
        var paneConfigureAction;
        (function (paneConfigureAction) {
            'use strict';
            var FieldType;
            (function (FieldType) {
                FieldType[FieldType["textField"] = 0] = "textField";
                FieldType[FieldType["checkboxField"] = 1] = "checkboxField";
                FieldType[FieldType["filePicker"] = 2] = "filePicker";
                FieldType[FieldType["radioGroupButton"] = 3] = "radioGroupButton";
                FieldType[FieldType["dropdownlistField"] = 4] = "dropdownlistField";
                FieldType[FieldType["textBlockField"] = 5] = "textBlockField";
                FieldType[FieldType["routing"] = 6] = "routing";
            })(FieldType || (FieldType = {}));
            var ChangeEventArgs = (function () {
                function ChangeEventArgs(fieldName) {
                    this.fieldName = fieldName;
                }
                return ChangeEventArgs;
            })();
            paneConfigureAction.ChangeEventArgs = ChangeEventArgs;
            //More detail on creating directives in TypeScript: 
            //http://blog.aaronholmes.net/writing-angularjs-directives-as-typescript-classes/
            var ConfigurationField = (function () {
                function ConfigurationField() {
                    var _this = this;
                    this.scope = {
                        currentAction: '=',
                        field: '='
                    };
                    this.templateUrl = '/AngularTemplate/ConfigurationField';
                    this.restrict = 'E';
                    ConfigurationField.prototype.link = function ($scope, $element, $attrs) {
                    };
                    ConfigurationField.prototype.controller = function ($scope, $element, $attrs) {
                        _this._$scope = $scope;
                        $scope.onFieldChange = angular.bind(_this, _this.onFieldChange);
                    };
                }
                //The factory function returns Directive object as per Angular requirements
                ConfigurationField.Factory = function () {
                    var directive = function () {
                        return new ConfigurationField();
                    };
                    directive['$inject'] = [];
                    return directive;
                };
                ConfigurationField.prototype.onFieldChange = function (event) {
                    var fieldName;
                    if (!!event.target === true) {
                        // If called by DOM event (for standard fields), get field name
                        // Get name of field that received the event
                        fieldName = event.target.attributes.getNamedItem('data-field-name').value;
                    }
                    else {
                        // If called by custom field, it is assumed that field name is suppied as the argument
                        fieldName = event;
                    }
                    this._$scope.$emit("onFieldChange", new ChangeEventArgs(fieldName));
                };
                return ConfigurationField;
            })();
            app.directive('configurationField', ConfigurationField.Factory());
        })(paneConfigureAction = directives.paneConfigureAction || (directives.paneConfigureAction = {}));
    })(directives = dockyard.directives || (dockyard.directives = {}));
})(dockyard || (dockyard = {}));
//# sourceMappingURL=configurationfield.js.map