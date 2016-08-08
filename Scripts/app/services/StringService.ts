/// <reference path="../_all.ts" />

/*
    The service implements centralized string storage.
*/

module dockyard.services {
    export interface IStringService {
        terminal: dictionary;
    }

    export interface dictionary {
        [key: string]: string
    }

    var strings: IStringService = {
        terminal: {
            error404: "The terminal was not found. Please make sure that you have the permissions to access it.",
            error400: "Some of the specified data were invalid. Please verify your entry and try again.",
            error: "Terminal cannot be saved. Please try again in a few minutes.",
            localhost_dev: " cannot contain the string 'localhost'. Please correct the URL and try again.",
            localhost_prod: " cannot contain the string 'localhost'. Please correct the URL and try again."

        }
    };

    app.factory('StringService', () => strings);
}