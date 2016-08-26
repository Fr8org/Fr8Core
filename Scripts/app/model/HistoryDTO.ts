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

    export class PagedQueryDTO {
        page: number;
        itemPerPage: number;
        filter: string;
        isCurrentUser: boolean;
        orderBy: string;
        appsOnly: boolean;
    }

    export class PagedResultDTO<T> {
        items: Array<T>;
        currentPage: number;
        totalItemCount: number;
    }
}