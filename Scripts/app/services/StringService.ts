/// <reference path="../_all.ts" />

/*
    The service implements centralized string storage.
*/

module dockyard.services {
    export interface IStringService {
        processTemplate: dictionary;
    }

    export interface dictionary {
        [key: string]: string
    }

    var strings: IStringService = {
        processTemplate: {
            error404: "Sorry, the Route was not found. Perhaps it has been deleted.",
            error400: "Some of the specified data were invalid. Please verify your entry and try again.",
            error: "Route cannot be saved. Please try again in a few minutes."
        }
    };

    app.factory('StringService', () => strings);
}