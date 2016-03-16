module dockyard.model {
    export class AlertDTO {
        title: string;
        body: string;
        isOkButtonVisible: boolean = true;
        isOkCancelVisible: boolean = true;
    }
}