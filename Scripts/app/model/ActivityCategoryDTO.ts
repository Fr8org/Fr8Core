module dockyard.model {
    export class ActivityCategoryDTO implements interfaces.IActivityCategoryDTO {
        name: string;
        iconPath: string;
        activities: Array<interfaces.IActivityTemplateVM>

        constructor() {

        }
    }
}