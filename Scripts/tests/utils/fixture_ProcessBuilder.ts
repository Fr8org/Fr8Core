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

        public static newContainier = <interfaces.IContainerVM> {
            id: 1,
            name: "TestTemplate{0B6944E1-3CC5-45BA-AF78-728FFBE57358}",
            processTemplateId: 1,
            containerState: 2,
            currentActivityId: null,
            nextActivityId: null
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
            subroutes: [
                <model.ProcessNodeTemplateDTO>{
                    id: 1,
                    isTempId: false,
                    name: 'Processnode Template 1',
                    actions: [
                        <model.ActionDTO> {
                            id: 1,
                            name: 'Action 1',
                            activityTemplateId: 1,
                            parentActivityId: 1
                        },
                        <model.ActionDTO>{
                            id: 2,
                            name: 'Action 2',
                            activityTemplateId: 1,
                            parentActivityId: 1
                        }
                    ]
                }]
        }
    }
}
