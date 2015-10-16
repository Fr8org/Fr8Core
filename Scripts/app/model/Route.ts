module dockyard.model {
    export class RouteDTO {
        id: number;
        isTempId: boolean;
        name: string;
        description: string;
        routeState: RouteState;
        subscribedDocuSignTemplates: Array<string>;
        externalEventSubscription: Array<number>; 
        startingProcessNodeTemplateId: number;
        subroutes: Array<ProcessNodeTemplateDTO>
    }

    export enum RouteState {
        Inactive = 1,
        Active = 2
    }
}