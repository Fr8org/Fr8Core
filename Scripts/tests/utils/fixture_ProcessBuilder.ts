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

        public static newActionListDTO = <interfaces.IActionListVM>{
            id: 9,
            actionListType: 1,
            name: "MockImmediate"
        };

        public static processBuilderState = new model.ProcessBuilderState();

        public static updatedProcessTemplate = <interfaces.IProcessTemplateVM> {
            'name': 'Updated',
            'description': 'Description',
            'processTemplateState': 1,
            'subscribedDocuSignTemplates': ['58521204-58af-4e65-8a77-4f4b51fef626']
        }

    }
}
