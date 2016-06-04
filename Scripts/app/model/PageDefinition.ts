module dockyard.model {
    export class PageDefinition {
        title: string;
        tags: Array<string>;
        url: string;
        type: string;
        author: string;
        description: string;
        authorUrl: string;
        pageName: string;

        constructor(name: string, url: string, pageName: string) {
            this.title = name;
            this.url = url;
            this.pageName = pageName;
        }
    }
}