/// <reference path="../_all.ts" />

module dockyard.model {
    export class SubordinateSubplan {
        constructor(subPlanId: string, activityId: string) {
            this.subPlanId = subPlanId;
            this.activityId = activityId;
        }

        public subPlanId: string;
        public activityId: string;
    }
}
