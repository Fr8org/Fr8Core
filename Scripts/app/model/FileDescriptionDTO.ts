module dockyard.model {
    export class FileDescriptionDTO implements interfaces.IFileDescriptionDTO {
        id: number;
        originalFileName: string;


        constructor(
            id: number,
            originalFileName: string
        ) {
            this.id = id;
            this.originalFileName = originalFileName;
        }


        clone(): FileDescriptionDTO {
            var result = new FileDescriptionDTO(this.id, this.originalFileName);
            return result;
        }

        static create(dataObject: interfaces.IFileDescriptionDTO): FileDescriptionDTO {
            var result = new FileDescriptionDTO(0, '');
            result.id = dataObject.id;
            result.originalFileName = dataObject.originalFileName;
            return result;
        }
    }
}