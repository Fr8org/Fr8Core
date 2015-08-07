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

        getNextId(): number {
            return ++this._nextId;
        }
    } 
}

app.service('LocalIdentityGenerator', dockyard.services.LocalIdentityGenerator);
