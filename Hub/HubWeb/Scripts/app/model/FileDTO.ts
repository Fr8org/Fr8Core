module dockyard.model {
    export class FileDTO {
        id: number;
        originalFileName: string;
        cloudStorageUrl: string;
        tags: string;
    }
} 