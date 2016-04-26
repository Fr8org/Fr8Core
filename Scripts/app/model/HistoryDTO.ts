module dockyard.model {

    export class HistoryItemDTO {
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
        component: string;
    }

    export class HistoryQueryDTO {
        page: number;
        itemPerPage: number;
        isDescending: boolean;
        filter: string;
        isCurrentUser: boolean;
    }

    export class HistoryResultDTO {
        items: Array<model.HistoryItemDTO>;
        currentPage: number;
        totalItemCount: number;
    }
}