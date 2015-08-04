'use strict';

app.directive('criteriaPane', function () {
    return {
        restrict: 'E',
        templateUrl: '/AngularTemplate/CriteriaPane',
        scope: {
            criteria: '=',
            fields: '=',
            removeCriteria: '&onRemoveCriteria'
        }
    };
});
