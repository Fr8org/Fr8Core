/// <reference path="../_all.ts" />

module dockyard.services {

    export interface ILocalIdentityGenerator {
        getNextId(): number;
    }

    export class LocalIdentityGenerator implements ILocalIdentityGenerator {
        private _nextId: number;

        constructor() {
            this._nextId = 0;
        }

        // Generating sequential negative numbers, to avoid temporary and global id overlapping.
        getNextId(): number {
            return --this._nextId;
        }
    } 
}

app.service('LocalIdentityGenerator', dockyard.services.LocalIdentityGenerator);
