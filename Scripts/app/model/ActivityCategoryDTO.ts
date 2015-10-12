module dockyard.model {
    export class ActivityCategoryDTO implements interfaces.IActivityCategoryDTO {
        name: string;
        activities: Array<interfaces.IActivityTemplateVM>

        constructor() {

        }
    }
}