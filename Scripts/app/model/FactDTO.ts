module dockyard.model {
    export class FactDTO{
        id: number;
        objectId: string;
        bookerId: string;
        customerId: string;
        primaryCategory: string;
        secondaryCategory: string;
        activity: string;
        data: string;
        status: string;
        lastUpdated: string;
        createDate: string;
        createdByID: string;
        priority: number;
        discriminator: string;       
    }   
}