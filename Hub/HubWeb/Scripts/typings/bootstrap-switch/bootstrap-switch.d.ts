// Type definitions for bootstrap-switch 3.3.2
// Project: https://github.com/nostalgiaz/bootstrap-switch
// Definitions by: Benoit Robin (werddomain@gmail.com)
// Definitions: <Not Listed>
 
 
/// <reference path="../jquery/jquery.d.ts"/>
 
declare module bootstrapSwitch {



    interface IbootstrapSwitchOptions {
        /**
         * The checkbox state
         *
         * default: false
         */
        state?: boolean;
 
        /**
         * The checkbox size. null, 'mini', 'small', 'normal', 'large'
         *
         * default: null
         */
        size?: string;
 
        /**
         * Animate the switch.
         *
         *default: true
         */
        animate?: boolean;
 
        /**
         * Disable state
         *
         * default: false
         */
        disabled?: boolean;
 
        /**
         * Readonly state
         *
         * default: false
         */
        readonly?: boolean;
 
        /**
         * Indeterminate state
         *
         * default: false
         */
        indeterminate?: boolean;
 
        /**
        * Inverse switch direction
        *
        * default: false
        */
        inverse?: boolean;
 
        /**
      * Allow this radio button to be unchecked by the user
      *
      * default: false
      */
        radioAllOff?: boolean;
 
        /**
      * Color of the left side of the switch. 'primary', 'info', 'success', 'warning', 'danger', 'default'
      *
      * default: 'primary'
      */
        onColor?: string;
 
        /**
      * Color of the right side of the switch. 'primary', 'info', 'success', 'warning', 'danger', 'default'
      *
      * default: 'default'
      */
        offColor?: string;
 
        /**
      * Text of the left side of the switch
      *
      * default: 'ON'
      */
        onText?: string;
 
        /**
      * Text of the right side of the switch
      *
      * default: 'OFF'
      */
        offText?: string;
 
        /**
      * Width of the left and right sides in pixels. ('auto' or Number)
      *
      * default: 'auto'
      */
        handleWidth?: any;
 
        /**
        * Width of the center handle in pixels. ('auto' or Number)
        *
        * default: 'auto;'
        */
        labelWidth?: any;
 
        /**
        * Global class prefix
        *
        * default: 'bootstrap-switch'
        */
        baseClass?: string;
 
 
        /**
        * Inverse switch direction. String | Array
        *
        * default: 'wrapper'
        */
        wrapperClass?: any;
 
 
        /**
        * Callback function to execute on initialization
        *
        */
        onInit?: (event: JQueryEventObject, state: boolean) => void;
 
        /**
     * Callback function to execute on initialization
     *
     */
        onSwitchChange?: (event: JQueryEventObject, state: boolean) => void;


    }

}

interface JQuery {
    /**
     * Create the Bootstrap-Switch
     *
     * @param options Options of the Bootstrap-Switch.
     */
    bootstrapSwitch(options?: bootstrapSwitch.IbootstrapSwitchOptions): any;

    bootstrapSwitch(method: string, param: any, param2: any): any;


}