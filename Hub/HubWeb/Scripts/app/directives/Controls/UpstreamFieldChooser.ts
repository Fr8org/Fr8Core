/// <reference path="../../_all.ts" />
module dockyard.directives.textSource {
    'use strict';

    export interface IUpstreamFieldChooserScope extends ng.IScope {
        currentAction: model.ActivityDTO;
        field: model.UpstreamFieldChooser;
        change: () => (field: model.ControlDefinitionDTO) => void;
        onChange: any;
        onFocus: any;
        uniqueDirectiveId: number;
        isFocused: boolean;
        onUpStreamChange: any;
    }

    //Setup aliases
    import pca = dockyard.directives.paneConfigureAction;
    import ddl = dockyard.directives.dropDownListBox;
    import planEvents = dockyard.Fr8Events.Plan;

    export function UpstreamFieldChooser(): ng.IDirective {
        
        var uniqueDirectiveId = 1;
        var controller = ['$scope', 'UIHelperService', ($scope: IUpstreamFieldChooserScope, uiHelperService: services.IUIHelperService) => {

            $scope.uniqueDirectiveId = ++uniqueDirectiveId;
            $scope.onChange = (fieldName: string) => {
                if ($scope.change != null && angular.isFunction($scope.change)) {
                    $scope.change()($scope.field);

                    $scope.isFocused = false;
                    $scope.$emit(pca.MessageType[pca.MessageType.PaneConfigureAction_ConfigureFocusElement],
                        new pca.ConfigureFocusElementArgs(null));
                }
            };

            var alertMessage = new model.AlertDTO();
            alertMessage.title = "Notification";
            alertMessage.body = 'There are no upstream fields available right now. To learn more,<a href= "/documentation/UpstreamCrates.html" target= "_blank" > click here </a><i class="fa fa-question-circle" > </i>';

            $scope.$on(<any>planEvents.ON_FIELD_FOCUS, function(event, args:pca.CallConfigureResponseEventArgs) {
                if ($scope.field.name === args.focusElement.name) {
                    $scope.isFocused = true;
                } else {
                    $scope.isFocused = false;
                }
            });

            // $scope.$on(ddl.MessageType[ddl.MessageType.DropDownListBox_NoRecords], function (event, args: AlertEventArgs) {
            //     if ($scope.field.listItems.length === 0) {
            //         uiHelperService.openConfirmationModal(alertMessage);
            //     }
            // });

            $scope.onFocus = (fieldname) => {
                $scope.$emit(pca.MessageType[pca.MessageType.PaneConfigureAction_ConfigureFocusElement],
                    new pca.ConfigureFocusElementArgs($scope.field));
            };

            $scope.onUpStreamChange = (field) => {
                $scope.field.selectedKey = field.value;
                $scope.field.selectedItem = field.selectedItem;
                $scope.onFocus(field);
                $scope.onChange(field);
            };
        }];


        var link = function ($scope, $element, attrs) {

            if ($scope.currentAction) {
                let controls = <Array<model.ControlDefinitionDTO>>$scope.currentAction.configurationControls.fields;

                //crutch, it would be nice to write test for this.
                // what happens here: we show lable 'Avaliable fields'  only for first textSource element in textsource elements block
                let index = _.findIndex(controls, f => f.type.toLowerCase() === 'textsource');
                if (index > -1) {
                    (<any>controls[index]).groupLabel = true;

                    let tailControls = _.rest(controls, index + 1);

                    while (index > -1) {
                        index = _.findIndex(tailControls, f => f.type.toLowerCase() === 'textsource');
                        tailControls = _.rest(tailControls);

                        if (index > 0) {
                            (<any>controls[index]).groupLabel = true;
                            tailControls = _.rest(controls, index + 1);
                        }
                    }
                }        
            }           

            //watch function for programatically setting focus on html element
            //$scope.$watch("isFocused", function (currentValue, previousValue) {
                //if (currentValue === true && !previousValue) {
                //    var theElement = $element.find("input[type='text']")[0];
                //    theElement.focus();
                //    $scope.field.valueSource = 'specific';
                //} 
            //});
        }

        return {
            restrict: 'E',
            templateUrl: '/AngularTemplate/UpstreamFieldChooser',
            controller: controller,
            link: link,
            scope: {
                currentAction: '=',
                field: '=',
                change: '&',
                isFocused : "=?"
            }
        };
    }

    app.directive('upstreamFieldChooser', UpstreamFieldChooser);
}