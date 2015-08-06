/// <reference path="../_all.ts" />
 
module dockyard.directives {
    'use strict';

    export function PaneDefineCriteria(): ng.IDirective {
        return {
            restrict: 'E',
            templateUrl: '/AngularTemplate/PaneDefineCriteria',
            scope: {
                criteria: '=',
                fields: '=',
                removeCriteria: '&onRemoveCriteria'
            }
        };
    }
}

app.directive('paneDefineCriteria', dockyard.directives.PaneDefineCriteria);
