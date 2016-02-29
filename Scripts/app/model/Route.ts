module dockyard.model {
    export class RouteDTO {
        id: string;
        isTempId: boolean;
        name: string;
        tag: string;
        description: string;
        routeState: RouteState;
        subscribedDocuSignTemplates: Array<string>;
        externalEventSubscription: Array<number>; 
        startingSubrouteId: number;
        subroutes: Array<SubrouteDTO>;
        visibility: PlanVisibility;
        category: string;
    }

    export enum RouteState {
        Inactive = 1,
        Active = 2
    }

    export enum PlanVisibility {
        Standard = 1,
        Internal = 2
    }
}