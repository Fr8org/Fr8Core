/*
    The service implements centralized string storage.
*/

MetronicApp.factory('StringService', function () {
    var strings = {};

    strings.processTemplate = {
        error404: "Sorry, the Process Template was not found. Perhaps it has been deleted.",
        error400: "Some of the specified data were invalid. Please verify your entry and try again.",
        error: "Process Template cannot be saved. Please try again in a few minutes."
    }

    return strings;
});