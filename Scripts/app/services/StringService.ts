/// <reference path="../_all.ts" />

/*
    The service implements centralized string storage.
*/

module dockyard.services {
    export interface IStringService {
        plan: dictionary;
    }

    export interface dictionary {
        [key: string]: string
    }

    var strings: IStringService = {
        plan: {
            error404: "Sorry, the Plan was not found. Perhaps it has been deleted.",
            error400: "Some of the specified data were invalid. Please verify your entry and try again.",
            error: "Plan cannot be saved. Please try again in a few minutes."
        }
    };

    app.factory('StringService', () => strings);
}