module dockyard.model {
    export class ActionGroup {
        public envelopes: model.ActivityEnvelope[];

        public offsetTop: number = 0;
        public offsetLeft: number = 0;
        public height: number = 300;

        public arrowLength: number = 0;
        public arrowOffsetLeft: number = 0;

        public parentEnvelope: model.ActivityEnvelope;
        public parentId: string;

        constructor(envelopes: model.ActivityEnvelope[], parentEnvelope: model.ActivityEnvelope, parentId: string) {
            this.envelopes = envelopes;
            this.parentEnvelope = parentEnvelope;
            this.parentId = parentId;
        }
    }

    export class ActivityEnvelope {
        public activity: interfaces.IActivityDTO;
        public allowsSiblings: boolean;
        public jumpTargets: Array<ActivityJumpTarget>;
        public activityTemplate: model.ActivityTemplate;

        constructor(activity: interfaces.IActivityDTO, activityTemplate: model.ActivityTemplate) {
            this.activity = activity;
            this.allowsSiblings = true;
            this.activityTemplate = activityTemplate;
        }

    }

    export class ContainerTransitions {
        public static get JumpToActivity(): number { return 0; };
        public static get LaunchAdditionalPlan(): number { return 1; };
        public static get JumpToSubplan(): number { return 2; };
        public static get StopProcessing(): number { return 3; };
        public static get SuspendProcessing(): number { return 4; };
        public static get ProceedToNextActivity(): number { return 5; };
    }

    export class ActivityJumpTarget {
        public TransitionType: number;
        public Target: string;

    }
}