module dockyard.model {
    export class PlanDTO {
        id: string;
        isTempId: boolean;
        name: string;
        tag: string;
        lastUpdated: string;
        description: string;
        planState: string;
        subscribedDocuSignTemplates: Array<string>;
        externalEventSubscription: Array<number>;
        startingSubPlanId: number;
        subPlans: Array<SubPlanDTO>;
        visibility: { hidden: boolean, public: boolean };
        category: string;
    }

    export class PlanState {
        static Inactive = "Inactive";
        static Executing = "Executing";
        static Saving_Changes = "Saving_Changes";
        static Active = "Active";
    }

    export enum PlanVisibility {
        Standard = 1,
        Internal = 2
    }

    export class PlanFullDTO {
        plan: PlanDTO
    }

    export class PlanQueryDTO {
        id: string;
        page: number;
        planPerPage: number;
        status:string;
        category: string;
        orderBy: string;
        isDescending: boolean;
        filter: string;
        appsOnly: boolean;
    }

    export class PlanResultDTO {
        plans: Array<interfaces.IPlanVM>;
        currentPage: number;
        totalPlanCount: number;
    }

    // TODO: implement FE PlanTemplateDTO
    //export class PlanTemplateDTO {
    //    id: string;        
    //    name: string;
    //    startingPlanNodeDescriptionId: string;        
    //    description: string;
        
    //    PlanNodeDescriptions: Array<PlanNodeDescriptionDTO>;
    //}
}