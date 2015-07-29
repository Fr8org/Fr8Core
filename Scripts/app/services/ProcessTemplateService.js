/*
    The service enables operations with Process Templates
*/

app.factory('ProcessTemplateService', function ($resource) {
    return $resource('/api/ProcessTemplate/:id', {id: '@id'});
});