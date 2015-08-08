/*
    This module declares a few methods and functions from the Metronic theme.
*/

declare module Metronic {
    export function getAssetsPath(): string;
    export function initComponents(): void;
    export function initAjax() : void;
}

declare module Layout {
    export function initHeader(): void;
    export function initFooter(): void;
}
declare module Demo {
    export function init(): void;
}