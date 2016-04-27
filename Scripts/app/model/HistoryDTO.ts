module dockyard.model {

    export class HistoryItemDTO {
        id: number;
        activity: string;       
        createDate: Date;
        fr8UserId: string;
        data: string;       
        objectId: string;
        component: string;
        primaryCategory: string;
        secondaryCategory: string;
        status: string;
    }

    export class HistoryQueryDTO {
        page: number;
        itemPerPage: number;
        isDescending: boolean;
        filter: string;
        isCurrentUser: boolean;
    }

    export class HistoryResultDTO<T> {
        items: Array<T>;
        currentPage: number;
        totalItemCount: number;
    }
}