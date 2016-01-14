module dockyard.services {
    export class ConfigureTrackerService {
        private _count: number;
        private _inProgressSet: any;

        constructor() {
            this._count = 0;
            this._inProgressSet = {};
        }

        public configureCallStarted(actionId: string) {
            if (!(actionId in this._inProgressSet)) {
                this._inProgressSet[actionId] = true;
                ++this._count;
            }
        }

        public configureCallFinished(actionId: string) {
            if (actionId in this._inProgressSet) {
                delete this._inProgressSet[actionId];
                --this._count;
            }
        }

        public hasPendingConfigureCalls(): boolean {
            return this._count > 0;
        }
    }
}

app.service('ConfigureTrackerService', [dockyard.services.ConfigureTrackerService]);