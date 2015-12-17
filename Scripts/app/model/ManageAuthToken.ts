module dockyard.model {

    export class ManageAuthToken_TerminalDTO {
        id: number;
        name: string;
        authTokens: Array<ManageAuthToken_AuthTokenDTO>;
        authenticationType: number;

        constructor(
            id: number,
            name: string,
            authTokens: Array<ManageAuthToken_AuthTokenDTO>,
            authenticationType: number) {

            this.id = id;
            this.name = name;
            this.authTokens = authTokens;
            this.authenticationType = authenticationType;
        }
    }

    export class ManageAuthToken_AuthTokenDTO {
        id: number;
        externalAccountName: string;
        isMain: boolean;

        constructor(id: number, externalAccountName: string, isMain: boolean) {
            this.id = id;
            this.externalAccountName = externalAccountName;
            this.isMain = isMain;
        }
    }
}