// Core object oriented helper functions for JS.
var Core = {};

(function (ns) {

    // Get DOM element.
    // Parameters:
    //     arg: - following arguments are valid:
    //         1. '#elementId'
    //         2. jQuery object
    //         3. DOM element
    //         4. any object that contains getElement() method.
    ns.element = function (arg) {
        if (typeof arg !== 'undefined') {
            if (typeof arg === 'string') {
                if (arg.length > 0) {
                    if (arg[0] !== '#')
                        return document.getElementById(arg);
                    else
                        return document.getElementById(arg.substring(1));
                }
            }

            else if (arg instanceof jQuery && arg.length > 0)
                return arg[0];

            else if (typeof arg.tagName !== 'undefined')
                return arg;

            else if (typeof arg.getElement !== 'undefined')
                return arg.getElement();
        }

        throw 'Invalid argument';
    };

    // Create wrapper over function to be called from other 'this' context.
    // Parameters:
    //     func - function to be wrapped
    //     context - another context
    ns.delegate = function (func, context) {
        return function () {
            return func.apply(context, arguments);
        };
    };

    // Create 'namespace' object.
    // Parameters:
    //     path - dot-separated namespace path.
    ns.ns = function (path) {
        var tokens = path.split('.');
        var i;
        var obj = window;
        for (i = 0; i < tokens.length; ++i) {
            if (!obj[tokens[i]]) {
                obj[tokens[i]] = {};
            }

            obj = obj[tokens[i]];
        }

        return obj;
    };

    // Define class.
    // Overloads:
    //     Core.class(methods):
    //         Parameters:
    //             methods - object that defines class methods.
    //     Core.class(baseClass, methods):
    //         Parameters:
    //             baseClass - base class to derive from
    //             methods - object that defines class methods.
    ns.class = function () {
        if (arguments.length === 0) {
            throw new Error('PlatformUI.class(<methods>) or PlatformUI.class(<parent>, <methods>)');
        }

        var constructorFn;

        if (arguments.length === 1) {
            constructorFn = arguments[0].constructor || function () { };
            ns.extend(constructorFn, arguments[0]);
        }
        else {
            constructorFn = arguments[1].constructor || function () { };
            ns.inherit(arguments[0], constructorFn, arguments[1]);
        }

        return constructorFn;
    };

    // Inherit one class from another.
    // Parameters:
    //     parent - base class constructor function.
    //     child - derived class constructor function.
    //     methods - object that defines class methods.
    ns.inherit = function (parent, child, methods) {
        var construct = function () { };
        construct.prototype = parent.prototype;
        child.prototype = new construct();
        child.prototype.constructor = child;
        child.super = parent.prototype;

        if (methods) {
            ns.extend(child, methods);
        }
    };

    // Extend object prototype with defined set of methods.
    // Parameters:
    //     obj - object to extend.
    //     methods - object that defines class methods.
    ns.extend = function (obj, methods) {
        var propName;
        for (propName in methods) {
            if (methods.hasOwnProperty(propName)) {
                obj.prototype[propName] = methods[propName];
            }
        }
    };

    // Extend object with properties.
    // Parameters:
    //     target - object to extend
    //     source - object that contains extension properties.
    ns.apply = function (target, source) {
        var propName;
        for (propName in source) {
            if (source.hasOwnProperty(propName)) {
                target[propName] = source[propName];
            }
        }
    };

    // Check value is undefined.
    ns.undefined = function (value) {
        return (typeof value === 'undefined');
    };

    // Get object value or nested value.
    // Parameters:
    //     item - object that contains properties or nested propertes.
    //     path - dot-separated property name.
    ns.value = function (item, path) {
        var tokens = path.split('.');
        var i;
        for (i = 0; i < tokens.length; ++i) {
            if (!item) { return null; }
            item = item[tokens[i]];
        }

        return item;
    };

    // Set object value or nested value.
    // Parameters:
    //     item - object that contains properties or nested properties.
    //     path - dot-separated property name.
    //     value - value to set.
    ns.setValue = function (item, path, value) {
        var tokens = path.split('.');
        var i;
        for (i = 0; i < tokens.length; ++i) {
            if (i < tokens.length - 1) {
                if (!item[tokens[i]]) {
                    item[tokens[i]] = {};
                }

                item = item[tokens[i]];
            }
            else {
                item[tokens[i]] = value;
            }
        }
    };


    // Create instance of CoreObject derived class.
    // Equals to var o = new CoreObject(); o.init();
    // Overloads:
    //     Core.create(ctorFn, params...):
    //         ctorFn - constructor function of class.
    //         params - parameters for constructor.
    ns.create = function () {
        if (arguments.length === 0) {
            throw new Error('PlatformUI.create(<ctorFn>, <arg0>, ..., <argN>)');
        }

        var ctorFn = arguments[0];

        var boundFn = ctorFn.bind.apply(ctorFn, arguments);
        var obj = new boundFn();
        obj.init();

        return obj;
    };


    // CoreObject class.
    // Implements publisher/subscriber pattern.
    // Allows other objects and functions to subscribe to certain events.
    ns.CoreObject = ns.class({
        constructor: function () {
            this._events = {};
        },

        // Init method, allows to properly initialize wigdets when inheriting widgets one from another.
        init: function () {
        },

        // Subscribe to event.
        // Parameters:
        //     event - event name.
        //     handler - event handler function.
        on: function (event, handler) {
            if (!this._events[event]) {
                this._events[event] = [];
            }

            this._events[event].push(handler);
        },

        // Unsubscribe from event.
        // Parameters:
        //     event - event name.
        //     handler - event handler function.
        un: function (event, handler) {
            if (!this._events[event]) {
                return;
            }

            var i;
            for (i = 0; i < this._events[event].length; ++i) {
                if (this._events[event][i] === handler) {
                    this._events[event].splice(i, 1);
                    --i;
                }
            }
        },

        // Fire event.
        // Overloads:
        //     fire(eventName, params...):
        //         eventName - name of the event to fire.
        //         params - parameters to pass to event handler functions.
        fire: function () {
            var event = arguments[0];

            if (!event || !this._events[event]) {
                return;
            }

            var i;
            var args = [];
            for (i = 1; i < arguments.length; ++i) {
                args.push(arguments[i]);
            }

            for (i = 0; i < this._events[event].length; ++i) {
                this._events[event][i].apply(this, args);
            }
        }
    });

}(Core));