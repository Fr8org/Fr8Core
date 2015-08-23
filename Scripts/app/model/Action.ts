module dockyard.model {
    export class Action {
        id: number;
        isTempId: boolean;
        processNodeTemplateId: number;
        actionListType: ActionListType;
        actionType: string;
        configurationSettings: string;
        fieldMappingSettings: string;
        userLabel: string

        constructor(
            id: number,
            isTempId: boolean,
            processNodeTemplateId: number,
            actionListType: ActionListType
        ) {
            this.id = id;
            this.isTempId = isTempId;
            this.processNodeTemplateId = processNodeTemplateId;
            this.actionListType = actionListType;
        }

        clone(): Action {
            var result = new Action(this.id, this.isTempId,
                this.processNodeTemplateId, this.actionListType);
            result.userLabel = this.userLabel;
            result.actionType = this.actionType;

            result.userLabel = this.userLabel;
            return result;
        }
    }
}