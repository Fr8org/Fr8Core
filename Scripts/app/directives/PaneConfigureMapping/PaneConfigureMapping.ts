module dockyard.directives.paneConfigureMapping {
    export enum MessageType {
        PaneConfigureMapping_ActionUpdated,
        PaneConfigureMapping_Render,
        PaneConfigureMapping_Hide,
        PaneConfigureMapping_UpdateAction   
    }

    export class RenderEventArgs extends RenderEventArgsBase { }
    export class HideEventArgs extends HideEventArgsBase { }

    export class CancelledEventArgs extends CancelledEventArgsBase { }
}