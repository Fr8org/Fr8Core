module dockyard.model {
    export class ContainerDTO{
        id: string;
        name: string;
        routeId: number;
        containerState: number;
        currentRouteNodeId: string;
        nextRouteNodeId: string;
        lastUpdated: string;
        createDate: string;
    }

    export enum ContainerState {
        Unstarted = 1,
        Executing = 2,
        WaitingForTerminal = 3,
        Completed = 4,
        Failed = 5
    }
}