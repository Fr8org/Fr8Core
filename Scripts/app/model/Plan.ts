module dockyard.model {
    export class PlanDTO {
        id: string;
        isTempId: boolean;
        name: string;
        tag: string;
        lastUpdated: string;
        description: string;
        planState: PlanState;
        subscribedDocuSignTemplates: Array<string>;
        externalEventSubscription: Array<number>;
        startingSubPlanId: number;
        subPlans: Array<SubPlanDTO>;
        visibility: PlanVisibility;
        category: string;
    }

    export enum PlanState {
        Inactive = 1,
        Active = 2
    }

    export enum PlanVisibility {
        Standard = 1,
        Internal = 2
    }

    export class PlanFullDTO {
        plan: PlanDTO
    }
}