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
        button
    }

    export class ConfigurationControlOperation {

        constructor(type: string, data: any) {
            this.type = type;
            this.data = data;
        }

        public type: string;
        public data: any;
    }

    export class ChangeEventArgs {
        constructor(field: model.ControlDefinitionDTO) {
            this.field = field;
        }

        public field: model.ControlDefinitionDTO;
    }

    export interface IConfigurationControlScope extends ng.IScope {
        currentAction: interfaces.IActionVM;
        field: model.ControlDefinitionDTO;
        plan: any;
        subPlan: any;
        //change made for MetaControlContainer with delayed control
        change: any;
        onChange: (radio: model.ControlDefinitionDTO) => void;
        onClick: (event: any) => void;
        isDisabled: boolean;
        pca: paneConfigureAction.IPaneConfigureActionController;
        cc: IConfigurationControlController;
    }

    export interface IConfigurationControlController {
        isThereOnGoingConfigRequest: () => boolean;
        queueClick: (element: ng.IAugmentedJQuery) => void;
        processOperation: (operation: ConfigurationControlOperation) => void;
    }

    export class ConfigurationControlController implements IConfigurationControlController {

        static $inject = ['$scope', '$element', '$attrs'];

        constructor(private $scope: IConfigurationControlScope, private $element: ng.IAugmentedJQuery) {
            $scope.cc = this;
            $scope.onChange = (event: any) => {
                // we need this thing ↓ because of delayed controls
                if ($scope.change) {
                    $scope.change({ target: true });
                    return;
                }

                var field: model.ControlDefinitionDTO;
                if (!!event.target === true) {
                    // If called by DOM event (for standard fields), get field from scope
                    field = $scope.field;
                }
                else {
                    // If called by custom field, it is assumed that field is supplied as the argument
                    field = event;
                }
                // Resetting validation errors on client side. If there is error, it will arise with follow-up configuration
                field.errorMessage = null;

                $scope.$emit("onChange", new ChangeEventArgs(field));
            };

            $scope.onClick = (event: any) => {
                var field: model.ControlDefinitionDTO;

                if (!!event.target === true) {
                    // If called by DOM event (for standard fields), get field that received the event
                    field = $scope.field;
                }
                else {
                    // If called by custom field, it is assumed that field name is supplied as the argument
                    field = event;
                }

                $scope.$emit("onClick", new ChangeEventArgs(field));
            };
        }

        public isThereOnGoingConfigRequest(): boolean {
            return this.$scope.pca.isThereOnGoingConfigRequest();
        }

        //on configure requests configuration controls get destroyed
        //this click event will be lost
        //we are telling PCA to queue this operation
        //and notify us after we are recreated
        public queueClick(element: ng.IAugmentedJQuery): void {
            var relativeXPath = ConfigurationControlController.getRelativeXPath(element[0], null).join(" > ");
            this.$scope.pca.queueOperation(this.$scope.field.name, new ConfigurationControlOperation("click", relativeXPath));
        }

        //after we are initialized
        //PCA will call this function with our old operations
        public processOperation(operation: ConfigurationControlOperation) {
            switch (operation.type) {
                case "click":
                    this.processClickOperation(operation);
                break;
            }
        }

        private processClickOperation(operation: ConfigurationControlOperation) {
            var elementRelativePath = operation.data;
            var elementToClick = this.$element.find(elementRelativePath);
            angular.element(elementToClick).triggerHandler('click');
        }

        private static getRelativeXPath(node: HTMLElement, path) {
            path = path || [];
            if (node.parentNode && !$(node.parentNode).hasClass("configuration-control-form-group")) {
                path = ConfigurationControlController.getRelativeXPath(<HTMLElement>node.parentNode, path);
            }
            if (node.previousSibling) {
                var count: number = 1;
                var sibling = node.previousSibling;
                do {
                    if (sibling.nodeType === 1 && sibling.nodeName === node.nodeName) { count++; }
                    sibling = sibling.previousSibling;
                } while (sibling);
                if (count === 1) { count = null; }
            } else if (node.nextSibling) {
                var sibling = node.nextSibling;
                do {
                    var count: number = 0;
                    if (sibling.nodeType === 1 && sibling.nodeName === node.nodeName) {
                        count = 1;
                        sibling = null;
                    } else {
                        sibling = sibling.previousSibling;
                    }
                } while (sibling);
            }

            if (node.nodeType === 1) {
                path.push(node.nodeName.toLowerCase() + (node.id ? "[@id='" + node.id + "']" : count > 0 ? "[" + count + "]" : ''));
            }
            return path;
        }
    }


    app.directive('configurationControl', () => {
        return {
            restrict: 'E',
            templateUrl: '/AngularTemplate/ConfigurationControl',
            require: '^paneConfigureAction',
            controller: ConfigurationControlController,
            link: (scope: IConfigurationControlScope, element: ng.IAugmentedJQuery, attrs: ng.IAttributes, pca: paneConfigureAction.IPaneConfigureActionController) => {
                scope.pca = pca;
                scope.pca.registerControl(scope.field.name, scope.cc);
            },
            scope: {
                currentAction: '=',
                field: '=',
                plan: '=',
                subPlan: '=',
                change: '=',
                isDisabled: '='
            }
        };
    });

    // A simple filter to format a string as a valid HTML identifier
    // per http://www.w3.org/TR/html4/types.html#type-id 
    app.filter('validId', function () {
        return input => {
            if (input) {
                return input.replace(/^[^a-zA-Z]/, 'a').replace(/[^\w\d\-_\.]/g, '-');
            }
        };
    });
}