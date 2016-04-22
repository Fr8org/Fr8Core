module dockyard.model {
    export class IncidentDTO {
        id: number;
        activity: string;       
        createDate: Date;
        fr8UserId: string;
        data: string;       
        is_high_priority: boolean;
        lastUpdated: Date;       
        objectId: string;
        primaryCategory: string;
        priority: number;
        secondaryCategory: string;
        status: string;
    }
}