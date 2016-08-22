module dockyard.model {
    export class PageDefinition {
        id: number;
        title: string;
        tags: string;
        url: string;
        type: string;
        author: string;
        description: string;
        authorUrl: string;
        pageName: string;

        constructor(name: string) {
            this.title = name;
        }
    }
}