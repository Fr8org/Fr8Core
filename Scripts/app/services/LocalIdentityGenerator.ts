/// <reference path="../_all.ts" />

module dockyard.services {

    export interface ILocalIdentityGenerator {
        getNextId(): string;
    }

    export class LocalIdentityGenerator implements ILocalIdentityGenerator {
        private _nextId: number;

        constructor() {
            this._nextId = 0;
        }

        // Generating sequential negative numbers, to avoid temporary and global id overlapping.
        getNextId(): string {
            return (--this._nextId).toString();
        }
    } 
}

app.service('LocalIdentityGenerator', dockyard.services.LocalIdentityGenerator);
