module dockyard.model {
    export class OrganizationDTO {
        public id: number;
        public name: string;
        public themeName: string;
        public backgroundColor: string;
        public logoUrl: string;
        constructor(id: number, name: string, themeName: string, backgroundColor: string, logoUrl: string) {
            this.id = id;
            this.name = name;
            this.themeName = themeName;
            this.backgroundColor = backgroundColor;
            this.logoUrl = logoUrl;
        }
    }
    
}
 