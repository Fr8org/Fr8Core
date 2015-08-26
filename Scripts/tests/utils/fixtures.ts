module dockyard.tests.utils {

    export class Fixtures {
        public static newProcessTemplate = <interfaces.IProcessTemplateVM> {
            Name: 'Test',
            Description: 'Description',
            ProcessTemplateState: 1
        };

        public static updatedProcessTemplate = <interfaces.IProcessTemplateVM> {
            'Name': 'Updated',
            'Description': 'Description',
            'ProcessTemplateState': 1,
            'SubscribedDocuSignTemplates': ['58521204-58af-4e65-8a77-4f4b51fef626']
        }
    }
}
