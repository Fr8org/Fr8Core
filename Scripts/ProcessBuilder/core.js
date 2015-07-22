var Core = { };

(function (ns) {

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

    ns.delegate = function (func, context) {
        return function () {
            return func.apply(context, arguments);
        };
    };

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

    ns.extend = function (obj, methods) {
        var propName;
        for (propName in methods) {
            if (methods.hasOwnProperty(propName)) {
                obj.prototype[propName] = methods[propName];
            }
        }
    };

    ns.apply = function (target, source) {
        var propName;
        for (propName in source) {
            if (source.hasOwnProperty(propName)) {
                target[propName] = source[propName];
            }
        }
    };

    ns.undefined = function (value) {
        return (typeof value === 'undefined');
    };

    ns.value = function (item, path) {
        var tokens = path.split('.');
        var i;
        for (i = 0; i < tokens.length; ++i) {
            if (!item) { return null; }
            item = item[tokens[i]];
        }

        return item;
    };

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

    ns.createAsync = function () {
        if (arguments.length === 0) {
            throw new Error('PlatformUI.createAsync(<ctorFn>, <arg0>, ..., <argN>)');
        }

        var ctorFn = arguments[0];

        var boundFn = ctorFn.bind.apply(ctorFn, arguments);
        var obj = new boundFn();

        var result = $.Deferred();
        obj.initAsync().done(function () { result.resolve(obj); });

        return result;
    };

    ns.CoreObject = ns.class({
        constructor: function () {
            this._events = {};
        },

        init: function () {
        },

        initAsync: function () {
            return false;
        },

        on: function (event, handler) {
            if (!this._events[event]) {
                this._events[event] = [];
            }

            this._events[event].push(handler);
        },

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