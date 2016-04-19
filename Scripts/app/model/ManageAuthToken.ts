module dockyard.model {

    export class ManageAuthToken_TerminalDTO {
        id: number;
        name: string;
        label: string;
        version: string;
        authTokens: Array<ManageAuthToken_AuthTokenDTO>;
        authenticationType: number;
        selectedAuthTokenId: number;

        constructor(
            id: number,
            name: string,
            authTokens: Array<ManageAuthToken_AuthTokenDTO>,
            authenticationType: number,
            selectedAuthTokenId: number) {

            this.id = id;
            this.name = name;
            this.authTokens = authTokens;
            this.authenticationType = authenticationType;
            this.selectedAuthTokenId = selectedAuthTokenId;
        }
    }

    export class ManageAuthToken_AuthTokenDTO {
        id: number;
        externalAccountName: string;
        isMain: boolean;
        isSelected: boolean;

        constructor(
            id: number,
            externalAccountName: string,
            isMain: boolean,
            isSelected: boolean
        ) {
            this.id = id;
            this.externalAccountName = externalAccountName;
            this.isMain = isMain;
            this.isSelected = isSelected;
        }
    }
}