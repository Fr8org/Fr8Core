/*
    This module declares a few methods and functions from the Metronic theme.
*/

declare module Metronic {
    export function getAssetsPath(): string;
    export function initComponents(): void;
    export function initAjax(): void;
    export function scrollTop(): void;
    export function blockUI(options?: UIBlockOptions | any): void;
    export function unblockUI(el: JQuery): void;
    export function startPageLoading(options?: PageLoadingOptions): void;
    export function stopPageLoading(): void;

    class UIBlockOptions {
        target: JQuery | any;
        animate: Boolean;
        overlayColor: string;
        iconOnly: boolean;
        textOnly: boolean;
        boxed: boolean;
        message: string;
    }

    class PageLoadingOptions {
        message: string;
        animate: boolean;
    }
}

declare module Layout {
    export function initHeader(): void;
    export function initFooter(): void;
    export function setMainMenuActiveLink(id: string): void;
    export function closeMainMenu(): void;
}
declare module Demo {
    export function init(): void;
}

