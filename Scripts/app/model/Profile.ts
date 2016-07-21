module dockyard.model {
    export class ProfileDTO {
        public id: string;
        public name: string;
        constructor(id: string, name: string) {
            this.id = id;
            this.name = name;
        }
    }
}
 