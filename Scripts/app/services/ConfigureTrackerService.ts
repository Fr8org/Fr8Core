module dockyard.services {
    export class ConfigureTrackerService {
        private _count: number;
        private _needsAuthCount: number;
        private _inProgressSet: any;

        constructor() {
            this._count = 0;
            this._needsAuthCount = 0;
            this._inProgressSet = {};
        }

        public configureCallStarted(actionId: string, needsAuthentication: boolean) {
            if (!(actionId in this._inProgressSet)) {
                this._inProgressSet[actionId] = {
                    needsAuthentication: needsAuthentication
                };

                ++this._count;
                if (needsAuthentication) {
                    ++this._needsAuthCount;
                }
            }
        }

        public configureCallFinished(actionId: string) {
            if (actionId in this._inProgressSet) {
                var needsAuthentication =
                    this._inProgressSet[actionId].needsAuthentication;

                delete this._inProgressSet[actionId];
                --this._count;

                if (needsAuthentication) {
                    --this._needsAuthCount;
                }
            }
        }

        public hasPendingConfigureCalls(): boolean {
            return this._count > 0;
        }

        public hasPendingConfigureAuthCalls(): boolean {
            return this._needsAuthCount > 0;
        }
    }
}

app.service('ConfigureTrackerService', [dockyard.services.ConfigureTrackerService]);