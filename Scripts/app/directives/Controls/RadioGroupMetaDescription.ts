/// <reference path="../../_all.ts" />

module dockyard.directives.radioGroupMetaDescription {
    'use strict';

    import pca = dockyard.directives.paneConfigureAction;

    export interface IRadioGroupMetaDescriptionScope extends ng.IScope {
        field: model.RadioGroupMetaDescriptionDTO;
        removeOptionDescription: (index: number) => void;
        addOptionDescription: () => void;
    }

    export function RadioGroupMetaDescription(): ng.IDirective {
        var controller = ['$scope', 
            ($scope: IRadioGroupMetaDescriptionScope) => {

                $scope.removeOptionDescription = (index: number) => {
                    $scope.field.remove(index);
                };

                $scope.addOptionDescription = () => {
                    
                }
            }
        ];

        return {
            restrict: 'E',
            templateUrl: '/AngularTemplate/RadioGroupMetaDescription',
            controller: controller,
            scope: {
                field: '='
            }
        };
    }

    app.directive('dropDownListBox', RadioGroupMetaDescription);
}