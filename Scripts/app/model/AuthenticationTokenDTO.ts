module dockyard.model {

    export class AuthenticationTokenTerminalDTO {
        name: string;
        label: string;
        version: string;
        authTokens: Array<AuthenticationTokenDTO>;
        authenticationType: number;
        selectedAuthTokenId: number;

        constructor(
            name: string,
            authTokens: Array<AuthenticationTokenDTO>,
            authenticationType: number,
            selectedAuthTokenId: number) {

            this.name = name;
            this.authTokens = authTokens;
            this.authenticationType = authenticationType;
            this.selectedAuthTokenId = selectedAuthTokenId;
        }
    }

    export class AuthenticationTokenDTO {
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