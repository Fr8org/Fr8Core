module dockyard.model {
    export class FileDTO implements interfaces.IFileDTO {
        id: number;
        originalFileName: string;


        constructor(
            id: number,
            originalFileName: string
        ) {
            this.id = id;
            this.originalFileName = originalFileName;
        }


        clone(): FileDTO {
            var result = new FileDTO(this.id, this.originalFileName);
            return result;
        }

        static create(dataObject: interfaces.IFileDTO): FileDTO {
            var result = new FileDTO(0, '');
            result.id = dataObject.id;
            result.originalFileName = dataObject.originalFileName;
            return result;
        }
    }
}