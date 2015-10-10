/// <reference path="../../_all.ts" />

module dockyard.directives {
    'use strict';

    export function FieldList(): ng.IDirective {
        return {
            restrict: 'E',
            templateUrl: '/AngularTemplate/FieldList',
            scope: {
                field: '='
            },
            controller: ['$scope', 
                function ($scope: IFieldListScope) {
                }
            ]
        }
    }

    export interface IFieldListScope extends ng.IScope {
        field: any;
    }
}

app.directive('fieldList', dockyard.directives.FieldList); 