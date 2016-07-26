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

    class FocusOperation {
        constructor(relativeXPath: string, caretPosition: number) {
            this.relativeXPath = relativeXPath;
            this.caretPosition = caretPosition;
        }

        relativeXPath: string;
        caretPosition: number;
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


            $scope.$on(MessageType[MessageType.PaneConfigureAction_ConfigureStarting], (event: ng.IAngularEvent) => {
                this.saveConfigurationControlState();
            });
        }

        public saveConfigurationControlState(): void {
            //let's check if we have a focused element
            var focused = $(':focus', this.$element);
            if (focused.length === 0) {
                //we don't have a focused element
                return;
            }
            var relativeXPath = ConfigurationControlController.getRelativeXPath(focused[0], null).join(" > ");
            var caretPosition = ConfigurationControlController.doGetCaretPosition(focused[0]);
            var focusOperation = new FocusOperation(relativeXPath, <number>caretPosition);
            this.$scope.pca.queueOperation(this.$scope.field.name, new ConfigurationControlOperation("focus", focusOperation));
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

                case "focus":
                    this.processFocusOperation(operation);
                    break;
            }
        }

        private processFocusOperation(operation: ConfigurationControlOperation) {
            var focusOperation = <FocusOperation>operation.data;
            var elementRelativePath = focusOperation.relativeXPath;
            var elementToFocus = this.$element.find(elementRelativePath);
            if (elementToFocus.length === 0) {
                return;
            }
            var targetElement = angular.element(elementToFocus)[0];
            targetElement.focus();
            if(focusOperation.caretPosition){
                ConfigurationControlController.setCaretToPos(targetElement, focusOperation.caretPosition);
            }
        }

        private processClickOperation(operation: ConfigurationControlOperation) {
            var elementRelativePath = operation.data;
            var elementToClick = this.$element.find(elementRelativePath);
            if (elementToClick.length === 0) {
                return;
            }
            angular.element(elementToClick).triggerHandler('click');
        }

        private static setSelectionRange(input, selectionStart, selectionEnd) {
            if (input.setSelectionRange) {
                input.setSelectionRange(selectionStart, selectionEnd);
            }
            else if (input.createTextRange) {
                var range = input.createTextRange();
                range.collapse(true);
                range.moveEnd('character', selectionEnd);
                range.moveStart('character', selectionStart);
                range.select();
            }
        }

        private static setCaretToPos(input, pos) {
            ConfigurationControlController.setSelectionRange(input, pos, pos);
        }

        private static doGetCaretPosition(oField) {
            try {
                // Initialize
                var iCaretPos = 0;
                // IE Support
                if ((<any>document).selection) {
                    // Set focus on the element
                    oField.focus();
                    // To get cursor position, get empty selection range
                    var oSel = (<any>document).selection.createRange();
                    // Move selection start to 0 position
                    oSel.moveStart('character', -oField.value.length);
                    // The caret position is selection length
                    iCaretPos = oSel.text.length;
                }
                // Firefox support
                else if (oField.selectionStart || oField.selectionStart == '0') {
                    iCaretPos = oField.selectionStart;
                }
                // Return results
                return iCaretPos;
            }
            catch (err) {
                return 0;
            }
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
                path.push(node.nodeName.toLowerCase() + (count > 0 ? ":nth-child(" + count + ")" : ''));
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