module dockyard.model {
    export class ActivityCategories {
        public static monitorId: string = '417DD061-27A1-4DEC-AECD-4F468013FD24';
        public static receiveId: string = '29EFB1D7-A9EA-41C5-AC60-AEF1F520E814';
        public static processId: string = '69FB6D2C-2083-4696-9457-B7B152D358C2';
        public static forwardId: string = 'AFD7E981-A21A-4B05-B0B1-3115A5448F22';
        public static solutionId: string = 'F9DF2AC2-2F10-4D21-B97A-987D46AD65B0';

        public static isPredefinedCategory(id: string): boolean {
            var idUpper = id.toUpperCase();
            return (ActivityCategories.monitorId === idUpper)
                || (ActivityCategories.receiveId === idUpper)
                || (ActivityCategories.processId === idUpper)
                || (ActivityCategories.forwardId === idUpper)
                || (ActivityCategories.solutionId === idUpper);
        }
    }

    export class ActivityCategoryDTO implements interfaces.IActivityCategoryDTO {
        id: string;
        name: string;
        iconPath: string;
        activities: Array<interfaces.IActivityTemplateVM>;

        constructor() {

        }
    }
}