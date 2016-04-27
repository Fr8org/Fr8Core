module dockyard.model {

    export class IncidentDTO extends HistoryItemDTO {
        priority: number;
        isHighPriority: boolean;
    }

}