module dockyard.model {

    export class ManageAuthToken_TerminalDTO {
        id: number;
        name: string;
        authTokens: Array<ManageAuthToken_AuthTokenDTO>;

        constructor(
            id: number,
            name: string,
            authTokens: Array<ManageAuthToken_AuthTokenDTO>) {

            this.id = id;
            this.name = name;
            this.authTokens = authTokens;
        }
    }

    export class ManageAuthToken_AuthTokenDTO {
        id: number;
        externalAccountName: string;

        constructor(id: number, externalAccountName: string) {
            this.id = id;
            this.externalAccountName = externalAccountName;
        }
    }
}