/*
    The service enables operations with Process Templates
*/

MetronicApp.factory('ProcessTemplateService', function ($resource) {
    return $resource('/api/ProcessTemplate/:id', {id: '@id'});
});