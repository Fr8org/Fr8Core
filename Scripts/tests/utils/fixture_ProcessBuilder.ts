module dockyard.tests.utils.fixtures {

    export class ProcessBuilder {
        public static newProcessTemplate = <interfaces.IProcessTemplateVM> {
            id: 1,
            name: "MockProcessTemplate",
            description: "MockProcessTemplate",
            processTemplateState: 1,
            subscribedDocuSignTemplates: [],
            externalEventSubscription: [],
            startingProcessNodeTemplateId: 1
        };

        public static newActionListDTO = <model.ActionList>{
            id: 9,
            actionListType: 1,
            name: "MockImmediate",
            actions: [
                <model.ActionDTO>{
                    id: 1,
                    name: 'Action 1',
                    activityTemplateId: 1,
                    processNodeTemplateId: 1
                },
                <model.ActionDTO>{
                    id: 2,
                    name: 'Action 2',
                    activityTemplateId: 1,
                    processNodeTemplateId: 1
                }
            ]
        };

        public static processBuilderState = new model.ProcessBuilderState();

        public static updatedProcessTemplate = <interfaces.IProcessTemplateVM> {
            'name': 'Updated',
            'description': 'Description',
            'processTemplateState': 1,
            'subscribedDocuSignTemplates': ['58521204-58af-4e65-8a77-4f4b51fef626']
        }

        public static fullProcessTemplate = <interfaces.IProcessTemplateVM> {
            'name': 'Updated',
            'description': 'Description',
            'processTemplateState': 1,
            'subscribedDocuSignTemplates': ['58521204-58af-4e65-8a77-4f4b51fef626'],
            processNodeTemplates: [
                <model.ProcessNodeTemplateDTO>{
                    id: 1,
                    isTempId: false,
                    name: 'Processnode Template 1',
                    actionLists: [
                        ProcessBuilder.newActionListDTO
                    ]
                }]
        }
    }
}
