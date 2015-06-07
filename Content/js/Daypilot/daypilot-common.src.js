/* Copyright 2005 - 2014 Annpoint, s.r.o.
 Use of this software is subject to license terms. 
 http://www.daypilot.org/
 */

if (typeof DayPilot === 'undefined') {
    var DayPilot = {};
}

if (typeof DayPilot.Global === 'undefined') {
    DayPilot.Global = {};
}

(function() {

    if (typeof DayPilot.$ !== 'undefined') {
        return;
    }

    DayPilot.$ = function(id) {
        return document.getElementById(id);
    };

    // mouse offset relative to the positioned element (FF) or relative to element that fired the event (IE)
    // deprecated, replaced by DayPilot.mo2
    // still being used in Month.js, replace carefully
    DayPilot.mo = function(t, ev) {
        ev = ev || window.event;

        if (ev.layerX) {  // Mozilla and others
            var coords = {x: ev.layerX, y: ev.layerY};
            if (!t) {
                return coords;
            }
            return coords;
            //while 

        }
        // this has to be first because IE9 supports layerX but it is not consistent with Mozilla
        if (ev.offsetX) { // IE
            return {x: ev.offsetX, y: ev.offsetY};
        }

        return null;
    };

    DayPilot.isKhtml = (navigator && navigator.userAgent && navigator.userAgent.indexOf("KHTML") !== -1);
    DayPilot.isIE = (navigator && navigator.userAgent && navigator.userAgent.indexOf("MSIE") !== -1);
    DayPilot.isIE7 = (navigator && navigator.userAgent && navigator.userAgent.indexOf("MSIE 7") !== -1);
    DayPilot.isIEQuirks = DayPilot.isIE && (document.compatMode && document.compatMode === "BackCompat");
    
    DayPilot.browser = {};
    DayPilot.browser.ie9 = (navigator && navigator.userAgent && navigator.userAgent.indexOf("MSIE 9") !== -1);  // IE
    DayPilot.browser.ielt9 = (function() {
        var div = document.createElement("div");
        div.innerHTML = "<!--[if lt IE 9]><i></i><![endif]-->";
        var isIeLessThan9 = (div.getElementsByTagName("i").length === 1);
        return isIeLessThan9;
    })();    
    
    DayPilot.mo2 = function(target, ev) {
        ev = ev || window.event;

        // IE
        if (typeof (ev.offsetX) !== 'undefined') {

            var coords = {x: ev.offsetX + 1, y: ev.offsetY + 1};

            if (!target) {
                return coords;
            }

            var current = ev.srcElement;
            while (current && current !== target) {
                if (current.tagName !== 'SPAN') { // hack for DayPilotMonth/IE, hour info on the right side of an event
                    coords.x += current.offsetLeft;
                    if (current.offsetTop > 0) {  // hack for http://forums.daypilot.org/Topic.aspx/879/move_event_bug
                        coords.y += current.offsetTop - current.scrollTop;
                    }
                }

                current = current.offsetParent;
            }

            if (current) {
                return coords;
            }
            return null;
        }

        // FF
        if (typeof (ev.layerX) !== 'undefined') {

            var coords = {x: ev.layerX, y: ev.layerY, src: ev.target};

            if (!target) {
                return coords;
            }
            var current = ev.target;

            // find the positioned offsetParent, the layerX reference
            while (current && current.style.position !== 'absolute' && current.style.position !== 'relative') {
                current = current.parentNode;
                if (DayPilot.isKhtml) { // hack for KHTML (Safari and Google Chrome), used in DPC/event moving
                    coords.y += current.scrollTop;
                }
            }

            while (current && current !== target) {
                coords.x += current.offsetLeft;
                coords.y += current.offsetTop - current.scrollTop;
                current = current.offsetParent;
            }
            if (current) {
                return coords;
            }

            return null;
        }

        return null;
    };

    // mouse offset relative to the specified target
    DayPilot.mo3 = function(target, ev) {
        ev = ev || window.event;

        var page = DayPilot.page(ev);
        if (page) {
            var abs = DayPilot.abs(target);
            return {x: page.x - abs.x, y: page.y - abs.y};
        }

        return DayPilot.mo2(target, ev);
    };

    // mouse coords
    DayPilot.mc = function(ev) {
        if (ev.pageX || ev.pageY) {
            return {x: ev.pageX, y: ev.pageY};
        }
        return {
            x: ev.clientX + document.documentElement.scrollLeft,
            y: ev.clientY + document.documentElement.scrollTop
        };
    };
    
    DayPilot.complete = function(f) {
        if (document.readyState === "complete") {
            f();
            return;
        }
        if (!DayPilot.complete.list) {
            DayPilot.complete.list = [];
            DayPilot.re(document, "readystatechange", function() {
                if (document.readyState === "complete") {
                    for (var i = 0; i < DayPilot.complete.list.length; i++) {
                        var d = DayPilot.complete.list[i];
                        d();
                    }
                    DayPilot.complete.list = [];
                }
            });
        }
        DayPilot.complete.list.push(f);
    };

    // returns pageX, pageY (calculated from clientX if pageX is not available)
    DayPilot.page = function(ev) {
        ev = ev || window.event;
        if (typeof ev.pageX !== 'undefined') {
            return {x: ev.pageX, y: ev.pageY};
        }
        if (typeof ev.clientX !== 'undefined') {
            return {
                x: ev.clientX + document.body.scrollLeft + document.documentElement.scrollLeft,
                y: ev.clientY + document.body.scrollTop + document.documentElement.scrollTop
            };
        }
        // shouldn't happen
        return null;
    };

    // absolute element position on page
    DayPilot.abs = function(element, visible) {
        if (!element) {
            return null;
        }

        var r = {
            x: element.offsetLeft,
            y: element.offsetTop,
            w: element.clientWidth,
            h: element.clientHeight,
            toString: function() {
                return "x:" + this.x + " y:" + this.y + " w:" + this.w + " h:" + this.h;
            }
        };

        if (element.getBoundingClientRect) {
            //var b = element.getBoundingClientRect();
            var b = null;
            try {
                b = element.getBoundingClientRect();
            } catch (e) {
                b = {top: element.offsetTop, left: element.offsetLeft};
            }
            ;
            r.x = b.left;
            r.y = b.top;

            var d = DayPilot.doc();
            r.x -= d.clientLeft || 0;
            r.y -= d.clientTop || 0;

            var pageOffset = DayPilot.pageOffset();
            r.x += pageOffset.x;
            r.y += pageOffset.y;

            if (visible) {
                // use diff, absOffsetBased is not as accurate
                var full = DayPilot.absOffsetBased(element, false);
                var visible = DayPilot.absOffsetBased(element, true);

                r.x += visible.x - full.x;
                r.y += visible.y - full.y;
                r.w = visible.w;
                r.h = visible.h;
            }

            return r;
        }
        else {
            return DayPilot.absOffsetBased(element, visible);
        }

    };

    DayPilot.isArray = function(o) {
        return Object.prototype.toString.call(o) === '[object Array]';
    };
    
    // old implementation of absolute position
    // problems with adjacent float and margin-left in IE7
    // still the best way to calculate the visible part of the element
    DayPilot.absOffsetBased = function(element, visible) {
        var r = {
            x: element.offsetLeft,
            y: element.offsetTop,
            w: element.clientWidth,
            h: element.clientHeight,
            toString: function() {
                return "x:" + this.x + " y:" + this.y + " w:" + this.w + " h:" + this.h;
            }
        };

        while (DayPilot.op(element)) {
            element = DayPilot.op(element);

            r.x -= element.scrollLeft;
            r.y -= element.scrollTop;

            if (visible) {  // calculates the visible part
                if (r.x < 0) {
                    r.w += r.x; // decrease width
                    r.x = 0;
                }

                if (r.y < 0) {
                    r.h += r.y; // decrease height
                    r.y = 0;
                }

                if (element.scrollLeft > 0 && r.x + r.w > element.clientWidth) {
                    r.w -= r.x + r.w - element.clientWidth;
                }

                if (element.scrollTop && r.y + r.h > element.clientHeight) {
                    r.h -= r.y + r.h - element.clientHeight;
                }
            }

            r.x += element.offsetLeft;
            r.y += element.offsetTop;

        }

        var pageOffset = DayPilot.pageOffset();
        r.x += pageOffset.x;
        r.y += pageOffset.y;

        return r;
    };
    
    // window dimensions
    DayPilot.wd = function() {
        var ieQuirks = DayPilot.isIEQuirks;
        
        // don't show the bubble outside of the visible window
        var windowHeight = document.documentElement.clientHeight;
        // fixing http://forums.daypilot.org/Topic.aspx/519/issue_with_bubble_in_ie
        if (ieQuirks) {
            windowHeight = document.body.clientHeight;
        }

        var windowWidth = document.documentElement.clientWidth;
        // fixing http://forums.daypilot.org/Topic.aspx/519/issue_with_bubble_in_ie
        if (ieQuirks) {
            windowWidth = document.body.clientWidth;
        }
        
        var scrollTop = (document.documentElement && document.documentElement.scrollTop) || document.body.scrollTop;
        var scrollLeft = (document.documentElement && document.documentElement.scrollLeft) || document.body.scrollLeft;
        
        var result = {};
        result.width = windowWidth;
        result.height = windowHeight;
        result.scrollTop = scrollTop;
        result.scrollLeft = scrollLeft;
        
        return result;
    };

    // offsetParent, safe access to prevent "Unspecified Error" in IE
    DayPilot.op = function(element) {
        try {
            return element.offsetParent;
        }
        catch (e) {
            return document.body;
        }
    };

    // distance of two points, works with x and y
    DayPilot.distance = function(point1, point2) {
        return Math.sqrt(Math.pow(point1.x - point2.x, 2) + Math.pow(point1.y - point2.y, 2));
    };

    // document element
    DayPilot.doc = function() {
        var de = document.documentElement;
        return (de && de.clientHeight) ? de : document.body;
    };

    DayPilot.pageOffset = function() {
        if (typeof pageXOffset !== 'undefined') {
            return {x: pageXOffset, y: pageYOffset};
        }
        var d = DayPilot.doc();
        return {x: d.scrollLeft, y: d.scrollTop};
    };

    // all children
    DayPilot.ac = function(e, children) {
        if (!children) {
            var children = [];
        }
        for (var i = 0; e.children && i < e.children.length; i++) {
            children.push(e.children[i]);
            DayPilot.ac(e.children[i], children);
        }

        return children;
    };

    DayPilot.indexOf = function(array, object) {
        if (!array || !array.length) {
            return -1;
        }
        for (var i = 0; i < array.length; i++) {
            if (array[i] === object) {
                return i;
            }
        }
        return -1;
    };

    // remove from array
    DayPilot.rfa = function(array, object) {
        var i = DayPilot.indexOf(array, object);
        if (i === -1) {
            return;
        }
        array.splice(i, 1);
    };
    
    DayPilot.sheet = function() {
        var style = document.createElement("style");
        style.setAttribute("type", "text/css");
        if (!style.styleSheet) {   // ie
            style.appendChild(document.createTextNode(""));
        }

        var h = document.head || document.getElementsByTagName('head')[0];
        h.appendChild(style);

        var oldStyle = !! style.styleSheet; // old ie

        var sheet = {};
        sheet.rules = [];
        sheet.commit = function() {
            if (oldStyle) {
                style.styleSheet.cssText = this.rules.join("\n");
            }
        };

        sheet.add = function(selector, rules, index) {
            if (oldStyle) { 
                this.rules.push(selector + "{" + rules + "\u007d");
                return;
            }
            if(style.sheet.insertRule) {
                style.sheet.insertRule(selector + "{" + rules + "\u007d", index);
            }
            else if (style.sheet.addRule) {
                style.sheet.addRule(selector, rules, index);
            }
        };
        return sheet;
    };

/*
    DayPilot.debug = function(msg, append) {

        if (!DayPilot.debugMessages) {
            DayPilot.debugMessages = [];
        }
        DayPilot.debugMessages.push(msg);

        if (typeof console !== 'undefined') {
            console.log(msg);
        }
    };

    DayPilot.debug.show = function() {
        alert("Log:\n" + DayPilot.debugMessages.join("\n"));
    };

    DayPilot.debug.clear = function() {
        DayPilot.debugMessages = [];
    };
  */
 
    DayPilot.Debug = function(calendar) {
        var debug = this;
        
        this.printToBrowserConsole = false;
        this.messages = [];
        this._div = null;
        this.clear = function() {
            this.messages = [];
            if (debug._div) {
                debug._div.innerHTML = '';
            }
        };
        
        this.hide = function() {
            DayPilot.de(debug._div);
            debug._div = null;
        };
        
        this.show = function() {
            if (debug._div) {
                debug.hide();
            }
            
            var ref = calendar.nav.top;

            var div = document.createElement("div");
            div.style.position = "absolute";
            div.style.top = "0px";
            div.style.bottom = "0px";
            div.style.left = "0px";
            div.style.right = "0px";
            div.style.backgroundColor = "black";
            div.style.color = "#ccc";
            div.style.overflow = "auto";
            div.style.webkitUserSelect = 'auto';
            div.style.MozUserSelect = 'all';
            div.onclick = function() {
                debug.hide();
            };
            
            for(var i = 0; i < this.messages.length; i++) {
                var msg = debug.messages[i];
                
                var line = msg._toElement();
                div.appendChild(line);
            }
            
            this._div = div;
            ref.appendChild(div);
        };
        
        this.message = function(text, level) {  // levels: info, warning, error
            var msg = {};
            msg.time = new DayPilot.Date();
            msg.level = level || "debug";
            msg.text = text;
            msg._toElement = function() {
                var line = document.createElement("div");
                line.innerHTML =  msg.time + " (" + msg.level + "): " + msg.text;
                switch (msg.level) {
                    case "error":
                        line.style.color = "red";
                        break;
                    case "warning":
                        line.style.color = "orange";
                        break;
                    case "info":
                        line.style.color = "white";
                        break;
                    case "debug":
                        break;
                }
                return line;
            };
            
            this.messages.push(msg);
            
            if (this.printToBrowserConsole && typeof console !== 'undefined') {
                console.log(msg);
            }
        };
    };

    // register event
    DayPilot.re = function(el, ev, func) {
        if (el.addEventListener) {
            el.addEventListener(ev, func, false);
        } else if (el.attachEvent) {
            el.attachEvent("on" + ev, func);
        }
    };
    // unregister event
    DayPilot.ue = function(el, ev, func) {
        if (el.removeEventListener) {
            el.removeEventListener(ev, func, false);
        } else if (el.detachEvent) {
            el.detachEvent("on" + ev, func);
        }
    };
    // trim
    DayPilot.tr = function(stringToTrim) {
        if (!stringToTrim)
            return '';
        return stringToTrim.replace(/^\s+|\s+$/g, "");
    };
    // date sortable (DateTime.ToString("s"))
    DayPilot.ds = function(d) {
        return DayPilot.Date.toStringSortable(d);
    };
    // get style
    DayPilot.gs = function(el, styleProp) {
        var x = el;
        if (x.currentStyle)
            var y = x.currentStyle[styleProp];
        else if (window.getComputedStyle)
            var y = document.defaultView.getComputedStyle(x, null).getPropertyValue(styleProp);
        if (typeof (y) === 'undefined')
            y = '';
        return y;
    };
    // encode arguments
    DayPilot.ea = function(a) {
        var joined = "";
        for (var i = 0; i < a.length; i++) {
            if (a[i] || typeof (a[i]) === 'number') {
                if (a[i].isDayPilotDate) {
                    a[i] = a[i].toStringSortable();
                }
                else if (a[i].getFullYear) {
                    a[i] = DayPilot.ds(a[i]);
                }
                joined += encodeURIComponent(a[i]);
            }
            if (i + 1 < a.length) {
                joined += '&';
            }
        }
        return joined;
    };

    // html encode
    DayPilot.he = function(str) {
        var result = str.replace(/&/g, "&amp;");
        result = result.replace(/</g, "&lt;");
        result = result.replace(/>/g, "&gt;");
        result = result.replace(/"/g, "&quot;");
        return result;
    };

    // cellIndex
    DayPilot.ci = function(cell) {
        var i = cell.cellIndex;
        if (i && i > 0)
            return i;
        var tr = cell.parentNode;
        var len = tr.cells.length;
        for (i = 0; i < len; i++) {
            if (tr.cells[i] === cell)
                return i;
        }
        return null;
    };

    // make unselectable
    DayPilot.us = function(element) {
        if (element) {
            element.setAttribute("unselectable", "on");
            element.style.MozUserSelect = 'none'; // it's enough for the root
            for (var i = 0; i < element.childNodes.length; i++) {
                if (element.childNodes[i].nodeType === 1) {
                    DayPilot.us(element.childNodes[i]);
                }
            }
        }
    };

    // purge
    // thanks to http://javascript.crockford.com/memory/leak.html
    DayPilot.pu = function(d) {
        //var removed = [];
        //var start = new Date();
        var a = d.attributes, i, l, n;
        if (a) {
            l = a.length;
            for (i = 0; i < l; i += 1) {
                if (!a[i]) {
                    continue;
                }
                n = a[i].name;
                if (typeof d[n] === 'function') {
                    //DayPilot.log.push(d.tagName + "." + n);
                    //removed.push(n);
                    d[n] = null;
                }
            }
        }
        a = d.childNodes;
        if (a) {
            l = a.length;
            for (i = 0; i < l; i += 1) {
                var children = DayPilot.pu(d.childNodes[i]);
                //removed = removed.concat(children);
            }
        }
        //return removed;
    };

    // purge children
    DayPilot.puc = function(d) {
        var a = d.childNodes, i, l;
        if (a) {
            var l = a.length;
            for (i = 0; i < l; i += 1) {
                DayPilot.pu(d.childNodes[i]);
            }
        }
    };

    // delete element
    DayPilot.de = function(e) {
        if (!e) {
            return;
        }
        if (!e.parentNode) {
            return;
        }
        e.parentNode.removeChild(e);
    };

    // get row
    DayPilot.gr = function(cell) {
        var i = 0;
        var tr = cell.parentNode;
        while (tr.previousSibling) {
            tr = tr.previousSibling;
            if (tr.tagName === "TR") {
                i++;
            }
        }
        return i;
    };

    DayPilot.fade = function(element, step, end) {
        //console.log("fade started, step: " + step + ", end: " + end);
        //console.log("current: " + element.opacity);
        var delay = 50;
        var visible = element.style.display !== 'none';
        var fadeIn = step > 0;
        var fadeOut = step < 0;

        if (step === 0) {
            return;
        }

        if (fadeIn && !visible) {
            element.target = parseFloat(element.style.opacity);
            element.opacity = 0; // current, for IE
            element.style.opacity = 0;
            element.style.filter = "alpha(opacity=0)";
            element.style.display = '';
        }
        else if (fadeOut && !element.target) {
            element.target = element.style.opacity;
        }
        else {
            //var current = parseFloat(element.style.opacity);
            var current = element.opacity;
            var updated = Math.floor(10 * (current + step)) / 10;
            if (fadeIn && updated > element.target) {
                updated = element.target;
            }
            if (fadeOut && updated < 0) {
                updated = 0;
            }
            var ie = updated * 100;
            element.opacity = updated;
            element.style.opacity = updated;
            element.style.filter = "alpha(opacity=" + ie + ")";
        }
        if ((fadeIn && (element.opacity >= element.target || element.opacity >= 1)) || (fadeOut && element.opacity <= 0)) {
            element.target = null;
            if (fadeOut) {
                element.style.opacity = element.target;
                element.opacity = element.target;
                var filter = element.target ? "alpha(opacity=" + (element.target * 100) + ")" : null;
                element.style.filter = filter;
                element.style.display = 'none';
            }
            if (end && typeof end === 'function') {
                end();
            }
        }
        else {
            this.messageTimeout = setTimeout(function() {
                DayPilot.fade(element, step, end);
            }, delay);
        }
    };


    // vertical scrollbar width
    DayPilot.sw = function(element) {
        if (!element) {
            return 0;
        }
        return element.offsetWidth - element.clientWidth;
    };
    
    DayPilot.swa = function() {
        var div = document.createElement("div");
        div.style.position = "absolute";
        div.style.top = "-2000px";
        div.style.left = "-2000px";
        div.style.width = '200px';
        div.style.height = '100px';
        div.style.overflow = 'auto';
        
        var inner = document.createElement("div");
        inner.style.width = '300px';
        inner.style.height = '300px';
        div.appendChild(inner);

        document.body.appendChild(div);
        var sw = DayPilot.sw(div);
        document.body.removeChild(div);

        return sw;
    };

    // horizontal scrollbar height
    DayPilot.sh = function(element) {
        if (!element) {
            return 0;
        }
        return element.offsetHeight - element.clientHeight;
    };

    DayPilot.guid = function() {
        var S4 = function() {
            return (((1 + Math.random()) * 0x10000) | 0).toString(16).substring(1);
        };
        return ("" + S4() + S4() + "-" + S4() + "-" + S4() + "-" + S4() + "-" + S4() + S4() + S4());
    };

    // unique array members
    DayPilot.ua = function(array) {
        var u = {}, a = [];
        for (var i = 0, l = array.length; i < l; ++i) {
            if (array[i] in u)
                continue;
            a.push(array[i]);
            u[array[i]] = 1;
        }
        return a;
    };
    
    (function () {
        DayPilot.pop = wave;

        function wave(div, options) {
            var target = {
                w: div.offsetWidth,
                h: div.offsetHeight,
                x: parseInt(div.style.left),
                y: parseInt(div.style.top)
            };
            target.height = div.style.height;
            target.width = div.style.width;
            target.top = div.style.top;
            target.left = div.style.left;
            target.toString = function () { return "w: " + this.w + " h:" + this.h };
            
            var config = {};
            config.finished = null;
            config.vertical = 'center';
            config.horizontal = 'center';
            
            if (options) {
                for (var name in options) {
                    config[name] = options[name];
                }
            }

            div.style.visibility = 'hidden';
            div.style.display = '';

            var animation = options.animation || "fast";

            var plan = createPlan(animation);

            plan.div = div;
            plan.i = 0;
            plan.target = target;
            plan.config = config;

            doStep(plan);
        }
        
        function createPlan(type) {

            var jump = function() {
                var plan = [];
                plan.time = 10;
                var last;

                var step = 0.08;
                last = 0.1;

                for (var i = last; i < 1.2; i += step) {
                    plan.push(i);
                    last = i;
                }

                step = 0.03;

                for (var i = last; i > 0.8; i -= step) {
                    plan.push(i);
                    last = i;
                }

                for (var i = last; i <= 1; i += step) {
                    plan.push(i);
                    last = i;
                }

                return plan;
            };

            var slow = function() {
                var plan = [];
                plan.time = 15;
                var last;

                var step = 0.04;
                last = 0.1;

                for (var i = last; i <= 1; i += step) {
                    plan.push(i);
                    last = i;
                }
                return plan;
            };

            var fast = function() {
                var plan = [];
                plan.time = 9;
                var last;

                var step = 0.04;
                last = 0.1;

                for (var i = last; i <= 1; i += step) {
                    plan.push(i);
                    last = i;
                }
                return plan;
            };
            
            var types = {
                "fast": fast,
                "slow": slow,
                "jump": jump
            };
            
            if (!types[type]) {
                type = "fast";
            }

            return types[type]();
        }

        function doStep(plan) {
            var div = plan.div;

            var pct = plan[plan.i];

            var height = pct * plan.target.h;
            var top;
            switch (plan.config.vertical) {
                case "center":
                    top = plan.target.y - (height - plan.target.h) / 2;
                    break;
                case "top":
                    top = plan.target.y;
                    break;
                case "bottom":
                    top = plan.target.y - (height - plan.target.h);
                    break;
                default:
                    throw "Unexpected 'vertical' value.";
            }

            var width = pct * plan.target.w;
            var left;
            
            switch (plan.config.horizontal) {
                case "left":
                    left = plan.target.x;
                    break;
                case "center":
                    left = plan.target.x - (width - plan.target.w) / 2;
                    break;
                case "right":
                    left = plan.target.x - (width - plan.target.w);
                    break;
                default:
                    throw "Unexpected 'horizontal' value.";
            }
            
            // TODO add scrollLeft
            var wd = DayPilot.wd();
            var bottom = (wd.height + wd.scrollTop) - (top + height);
            if (bottom < 0) {
                top += bottom;
            }
            
            var right = (wd.width) - (left + width);
            if (right < 0) {
                left += right;
            }

            div.style.height = height + "px";
            div.style.top = top + "px";

            div.style.width = width + "px";
            div.style.left = left + "px";

            //div.style.display = '';
            div.style.visibility = 'visible';

            plan.i++;

            if (plan.i < plan.length - 1) {
                setTimeout((function (plan) {
                    return function () {
                        doStep(plan);
                    };
                })(plan), plan.time);
            }
            else {
                // set the original dimensions
                div.style.width = plan.target.width;
                div.style.height = plan.target.height;
                // and position
                div.style.top = plan.target.top;
                div.style.left = plan.target.left;
                
                // callback
                if (typeof plan.config.finished === 'function') {
                    plan.config.finished();
                }
            }
        }


    })();

    DayPilot.Util = {};
    DayPilot.Util.addClass = function(object, name) {
        if (!object) {
            return;
        }
        if (!object.className) {
            object.className = name;
            return;
        }
        var already = new RegExp("(^|\\s)" + name + "($|\\s)");
        if (!already.test(object.className)) {
            object.className = object.className + ' ' + name;
        }
    };
    
    DayPilot.Util.addClassToString = function(str, name) {
        if (!str) {
            return name;
        }
        var already = new RegExp("(^|\\s)" + name + "($|\\s)");
        if (!already.test(str)) {
            return str + ' ' + name;
        }
        else {
            return str;
        }
    };

    DayPilot.Util.removeClassFromString = function(str, name) {
        if (!str) {
            return "";
        }
        var already = new RegExp("(^|\\s)" + name + "($|\\s)");
        return str.replace(already, ' ').replace(/^\s\s*/, '').replace(/\s\s*$/, '');  // trim spaces
    };

    DayPilot.Util.removeClass = function(object, name) {
        if (!object) {
            return;
        }
        var already = new RegExp("(^|\\s)" + name + "($|\\s)");
        object.className = object.className.replace(already, ' ').replace(/^\s\s*/, '').replace(/\s\s*$/, '');  // trim spaces
    };

    DayPilot.Util.props = function(o) {
        var t = [];
        for (a in o) {
            t.push(a);
            t.push(o[a]);
        }
        return t.join("-");
    };
    
    
    DayPilot.Util.propArray = function(props, name) {
        var result = [];
        if (!props || !props.length) {
            return result;
        }

        for (var i = 0; i < props.length; i++) {
            result.push(props[i][name]);
        }
        return result;
    };
    
    DayPilot.Util.updatePropsFromArray = function(props, name, array) {
        for (var i = 0; i < array.length; i++) {
            props[i][name] = array[i];
        }
    };    
    
    DayPilot.Util.copyProps = function(source, target, props) {
        if (!source) {
            return;
        }
        if (typeof props === 'undefined') {
            for (var name in source) {
                target[name] = source[name];
            }
        }
        else {
            for (var i = 0; i < props.length; i++) {
                var name = props[i];
                target[name] = source[name];
            }
        }
    };

    DayPilot.Areas = {};

    DayPilot.Areas.showAreas = function(div, e, ev) {
        if (DayPilot.Global.resizing) {
            return;
        }

        if (DayPilot.Global.moving) {
            return;
        }

        if (DayPilot.Areas.all && DayPilot.Areas.all.length > 0) {
            for (var i = 0; i < DayPilot.Areas.all.length; i++) {
                var d = DayPilot.Areas.all[i];
                if (d !== div) {
                    DayPilot.Areas.hideAreas(d, ev);
                }
            }
        }

        if (div.active) {
            return;
        }
        div.active = {};

        var areas = e.areas;

        if (!areas && e.data && e.data.areas) {
            areas = e.data.areas;
        }

        if (!areas || areas.length === 0) {
            return;
        }

        if (div.areas && div.areas.length > 0) {
            return;
        }
        //if (typeof div.areas == 'undefined') {
        div.areas = [];
        //}

        for (var i = 0; i < areas.length; i++) {
            var area = areas[i];
            if (area.v !== 'Hover') {
                continue;
            }

            var a = DayPilot.Areas.createArea(div, e, area);

            div.areas.push(a);
            div.appendChild(a);

            DayPilot.Areas.all.push(div);
        }
        div.active.children = DayPilot.ac(div);
    };

    DayPilot.Areas.createArea = function(div, e, area) {

        var a = document.createElement("div");
        a.isActiveArea = true;
        a.style.position = "absolute";
        a.setAttribute("unselectable", "on");
        if (typeof area.w !== 'undefined') {
            a.style.width = area.w + "px";
        }
        if (typeof area.h !== 'undefined') {
            a.style.height = area.h + "px";
        }
        if (typeof area.right !== 'undefined') {
            a.style.right = area.right + "px";
        }
        if (typeof area.top !== 'undefined') {
            a.style.top = area.top + "px";
        }
        if (typeof area.left !== 'undefined') {
            a.style.left = area.left + "px";
        }
        if (typeof area.bottom !== 'undefined') {
            a.style.bottom = area.bottom + "px";
        }
        if (typeof area.html !== 'undefined' && area.html) {
            a.innerHTML = area.html;
        }
        ;
        if (area.css) {
            a.className = area.css;
        }
        if (area.action === "ResizeEnd" || area.action === "ResizeStart" || area.action === "Move") {
            if (e.calendar.isCalendar) {
                switch (area.action) {
                    case "ResizeEnd":
                        area.cursor = "s-resize";
                        area.dpBorder = "bottom";
                        break;
                    case "ResizeStart":
                        area.cursor = "n-resize";
                        area.dpBorder = "top";
                        break;
                    case "Move":
                        area.cursor = "move";
                        break;
                }
            }
            if (e.calendar.isScheduler || e.calendar.isMonth) {
                switch (area.action) {
                    case "ResizeEnd":
                        area.cursor = "e-resize";
                        area.dpBorder = "right";
                        break;
                    case "ResizeStart":
                        area.cursor = "w-resize";
                        area.dpBorder = "left";
                        break;
                    case "Move":
                        area.cursor = "move";
                        break;
                }
            }
            a.onmousemove = (function(div, e, area) {
                return function(ev) {
                    var ev = ev || window.event;
                    div.style.cursor = area.cursor;
                    if (area.dpBorder)
                        div.dpBorder = area.dpBorder;
                    ev.cancelBubble = true;
                };
            })(div, e, area);
            a.onmouseout = (function(div, e, area) {
                return function(ev) {
                    div.style.cursor = '';
                };
            })(div, e, area);
        }
        if (area.action === "Bubble" && e.isEvent) {
            a.onmousemove = (function(div, e, area) {
                return function(ev) {
                    if (e.calendar.bubble) {
                        e.calendar.bubble.showEvent(e);
                    }
                };
            })(div, e, area);
            a.onmouseout = (function(div, e, area) {
                return function(ev) {
                    if (typeof DayPilot.Bubble !== "undefined") {
                        //DayPilot.Bubble.hideActive();
                        if (e.calendar.bubble) {
                            e.calendar.bubble.hideOnMouseOut();
                        }
                    }

                };
            })(div, e, area);
        }
        // prevent event moving
        a.onmousedown = (function(div, e, area) {
            return function(ev) {
            	/*
                if (!e.calendar) {
                    return;
                }
                
                var cancel = false;
                if (e.calendar.isMonth) {
                    cancel = true;
                }
                if (e.calendar.isScheduler && e.calendar.moveBy === 'Full') {
                    cancel = true;
                }
                */
                var cancel = true;
                
                if (cancel) {
                    if (area.action === "Move" || area.action === "ResizeEnd" || area.action === "ResizeStart") {
                        return;
                    }
                    ev = ev || window.event;
                    ev.cancelBubble = true;
                }
            };
        })(div, e, area);
        a.onclick = (function(div, e, area) {
            return function(ev) {
                var ev = ev || window.event;
                switch (area.action) {
                    case "JavaScript":
                        var f = area.js;
                        if (typeof f === 'string') {
                            f = eval("0, " + area.js);
                        }
                        if (typeof f === 'function') {
                            f.call(this, e);
                        }
                        break;
                    case "ContextMenu":
                        var m = area.menu;
                        if (typeof m === 'string') {
                            m = eval(m);
                        }
                        if (m && m.show) {
                            m.show(e);
                        }
                        break;
                    case "CallBack":
                        alert("callback not implemented yet, id: " + area.id);
                        break;
                }
                ev.cancelBubble = true;
            };
        })(div, e, area);


        return a;
    };

    DayPilot.Areas.all = [];

    DayPilot.Areas.hideAreas = function(div, ev) {
        if (!div) {
            return;
        }

        if (!div || !div.active) {
            return;
        }

        var active = div.active;
        var areas = div.areas;

        if (active && active.children) {
            var ev = ev || window.event;
            if (ev) {
                var target = ev.toElement || ev.relatedTarget;
                if (~DayPilot.indexOf(active.children, target)) {
                    return;
                }
            }
        }

        if (!areas || areas.length === 0) {
            div.active = null;
            return;
        }

        for (var i = 0; i < areas.length; i++) {
            var a = areas[i];
            div.removeChild(a);
        }

        div.active = null;
        div.areas = [];

        DayPilot.rfa(DayPilot.Areas.all, div);

        active.children = null;
    };

    DayPilot.Areas.hideAll = function(ev) {
        if (!DayPilot.Areas.all || DayPilot.Areas.all.length === 0) {
            return;
        }
        for (var i = 0; i < DayPilot.Areas.all.length; i++) {
            DayPilot.Areas.hideAreas(DayPilot.Areas.all[i], ev);
        }

    };
    
    DayPilot.Action = function(calendar, action, params, data) {
        this.calendar = calendar;
        this.isAction = true;
        this.action = action;
        this.params = params;
        this.data = data;

        this.notify = function() {
            calendar.internal.invokeEvent("Immediate", this.action, this.params, this.data);
        };

        this.auto = function() {
            calendar.internal.invokeEvent("Notify", this.action, this.params, this.data);
        };

        this.queue = function() {
            calendar.queue.add(this);
        };

        this.toJSON = function() {
            var json = {};
            json.name = this.action;
            json.params = this.params;
            json.data = this.data;

            return json;
        };

    };

    DayPilot.Selection = function(start, end, resource, root) {
        this.type = 'selection';
        this.start = start.isDayPilotDate ? start : new DayPilot.Date(start);
        this.end = end.isDayPilotDate ? end : new DayPilot.Date(end);
        this.resource = resource;
        this.root = root;

        this.toJSON = function(key) {
            var json = {};
            json.start = this.start;
            json.end = this.end;
            json.resource = this.resource;

            return json;
        };
    };

    DayPilot.Event = function(data, calendar, part) {
        var e = this;
        this.calendar = calendar;
        this.data = data ? data : {};
        this.part = part ? part : {};

        // backwards compatibility, still accepts id in "value" 
        if (typeof this.data.id === 'undefined') {
            this.data.id = this.data.value;
        }

        var copy = {};
        var synced = ["id", "text", "start", "end", "resource"];

        this.isEvent = true;

        // internal
        this.temp = function() {
            if (copy.dirty) {
                return copy;
            }
            for (var i = 0; i < synced.length; i++) {
                copy[synced[i]] = e.data[synced[i]];
            }
            copy.dirty = true;
            return copy;

        };

        // internal
        this.copy = function() {
            var result = {};
            for (var i = 0; i < synced.length; i++) {
                result[synced[i]] = e.data[synced[i]];
            }
            return result;
        };

        this.commit = function() {
            if (!copy.dirty) {
                return;
            }

            for (var i = 0; i < synced.length; i++) {
                e.data[synced[i]] = copy[synced[i]];
            }

            copy.dirty = false;
        };

        this.dirty = function() {
            return copy.dirty;
        };

        this.id = function(val) {
            if (typeof val === 'undefined') {
                return e.data.id;
            }
            else {
                this.temp().id = val;
            }
        };
        // obsolete, use id() instead
        this.value = function(val) {
            if (typeof val === 'undefined') {
                return e.id();
            }
            else {
                e.id(val);
            }
        };
        this.text = function(val) {
            if (typeof val === 'undefined') {
                return e.data.text;
            }
            else {
                this.temp().text = val;
                this.client.innerHTML(val); // update the HTML automatically
            }
        };
        this.start = function(val) {
            if (typeof val === 'undefined') {
                return new DayPilot.Date(e.data.start);
            }
            else {
                this.temp().start = new DayPilot.Date(val);
            }
        };
        this.end = function(val) {
            if (typeof val === 'undefined') {
                return new DayPilot.Date(e.data.end);
            }
            else {
                this.temp().end = new DayPilot.Date(val);
            }
        };
        this.partStart = function() {
            return new DayPilot.Date(this.part.start);
        };
        this.partEnd = function() {
            return new DayPilot.Date(this.part.end);
        };
        this.row = function() {
            return this.resource();
        };
        
        this.allday = function() {
            if (typeof val === 'undefined') {
                return e.data.allday;
            }
            else {
                this.temp().allday = val;
            }            
        };
        
        // backwards compatibility, 7.3
        this.isAllDay = this.allday;

        this.resource = function(val) {
            if (typeof val === 'undefined') {
                return e.data.resource;
            }
            else { // it's a resource id
                this.temp().resource = val;
            }
        };

        this.recurrent = function() {
            return e.data.recurrent;
        };
        this.recurrentMasterId = function() {
            return e.data.recurrentMasterId;
        };
        this.useBox = function() {
            return this.part.box;
        };
        this.staticBubbleHTML = function() {
            return this.bubbleHtml();
        };
        this.bubbleHtml = function() {
            if (e.cache) {
                return e.cache.bubbleHtml || e.data.bubbleHtml;
            }
            return e.data.bubbleHtml;
        };
        this.tag = function(field) {
            var values = e.data.tag;
            if (!values) {
                return null;
            }
            if (typeof field === 'undefined') {
                return e.data.tag;
            }
            var fields = e.calendar.tagFields;
            var index = -1;
            for (var i = 0; i < fields.length; i++) {
                if (field === fields[i])
                    index = i;
            }
            if (index === -1) {
                throw "Field name not found.";
            }
            return values[index];
        };

        this.client = {};
        this.client.innerHTML = function(val) {
            if (typeof val === 'undefined') {
                if (e.cache && typeof e.cache.html !== "undefined") {
                    return e.cache.html;
                }
                if (typeof e.data.html !== "undefined") {
                    return e.data.html;
                }
                return e.data.text;
            }
            else {
                e.data.html = val;
            }
        };
        
        this.client.html = this.client.innerHTML;
        
        this.client.header = function(val) {
            if (typeof val === 'undefined') {
                return e.data.header;
            }
            else {
                e.data.header = val;
            }
        };
        
        this.client.cssClass = function(val) {
            if (typeof val === 'undefined') {
                return e.data.cssClass;
            }
            else {
                e.data.cssClass = val;
            }
        };
        this.client.toolTip = function(val) {
            if (typeof val === 'undefined') {
                if (e.cache && typeof e.cache.toolTip !== "undefined") {
                    return e.cache.toolTip;
                }
                return typeof e.data.toolTip !== 'undefined' ? e.data.toolTip : e.data.text;
            }
            else {
                e.data.toolTip = val;
            }
        };

        this.client.backColor = function(val) {
            if (typeof val === 'undefined') {
                if (e.cache && typeof e.cache.backColor !== "undefined") {
                    return e.cache.backColor;
                }
                return typeof e.data.backColor !== "undefined" ? e.data.backColor : e.calendar.eventBackColor;
            }
            else {
                e.data.backColor = val;
            }
        };
/*
        this.client.backColor = function(val) {
            if (typeof val === 'undefined') {
                //return typeof e.data.backColor !== "undefined" ? e.data.backColor : e.calendar.eventBackColor;
                return typeof e.data.backColor !== "undefined" ? e.data.backColor : e.calendar.eventBackColor;
            }
            else {
                e.data.backColor = val;
            }
        };
*/
        this.client.borderColor = function(val) {
            if (typeof val === 'undefined') {
                if (e.cache && typeof e.cache.borderColor !== "undefined") {
                    return e.cache.borderColor;
                }
                return typeof e.data.borderColor !== "undefined" ? e.data.borderColor : e.calendar.eventBorderColor;
            }
            else {
                e.data.borderColor = val;
            }
        };

        this.client.barColor = function(val) {
            if (typeof val === 'undefined') {
                if (e.cache && typeof e.cache.barColor !== "undefined") {
                    return e.cache.barColor;
                }
                return typeof e.data.barColor !== "undefined" ? e.data.barColor : e.calendar.durationBarColor;
            }
            else {
                e.data.barColor = val;
            }
        };


        this.client.barVisible = function(val) {
            if (typeof val === 'undefined') {
                if (e.cache && typeof e.cache.barHidden !== "undefined") {
                    return !e.cache.barHidden;
                }
                return e.calendar.durationBarVisible && !e.data.barHidden;
            }
            else {
                e.data.barHidden = !val;
            }
        };

        this.client.contextMenu = function(val) {
            if (typeof val === 'undefined') {
                if (e.oContextMenu) {
                    return e.oContextMenu;
                }
                return (e.data.contextMenu) ? eval(e.data.contextMenu) : null;  // might want to return the default context menu in the future
            }
            else {
                e.oContextMenu = val;
            }
        };

        this.client.moveEnabled = function(val) {
            if (typeof val === 'undefined') {
                if (e.cache && typeof e.cache.moveDisabled !== "undefined") {
                    return !e.cache.moveDisabled;
                }
                return e.calendar.eventMoveHandling !== 'Disabled' && !e.data.moveDisabled;
            }
            else {
                e.data.moveDisabled = !val;
            }
        };

        this.client.resizeEnabled = function(val) {
            if (typeof val === 'undefined') {
                if (e.cache && typeof e.cache.resizeDisabled !== "undefined") {
                    return !e.cache.resizeDisabled;
                }
                return e.calendar.eventResizeHandling !== 'Disabled' && !e.data.resizeDisabled;
            }
            else {
                e.data.resizeDisabled = !val;
            }
        };

        this.client.rightClickEnabled = function(val) {
            if (typeof val === 'undefined') {
                if (e.cache && typeof e.cache.rightClickDisabled !== "undefined") {
                    return !e.cache.rightClickDisabled;
                }
                return e.calendar.rightClickHandling !== 'Disabled' && !e.data.rightClickDisabled;
            }
            else {
                e.data.rightClickDisabled = !val;
            }
        };

        this.client.clickEnabled = function(val) {
            if (typeof val === 'undefined') {
                if (e.cache && typeof e.cache.clickDisabled !== "undefined") {
                    return !e.cache.clickDisabled;
                }
                return e.calendar.clickHandling !== 'Disabled' && !e.data.clickDisabled;
            }
            else {
                e.data.clickDisabled = !val;
            }
        };

        this.client.deleteEnabled = function(val) {
            if (typeof val === 'undefined') {
                if (e.cache && typeof e.cache.deleteDisabled !== "undefined") {
                    return !e.cache.deleteDisabled;
                }
                return e.calendar.eventDeleteHandling !== 'Disabled' && !e.data.deleteDisabled;
            }
            else {
                e.data.deleteDisabled = !val;
            }
        };
        
        this.client.doubleClickEnabled = function(val) {
            if (typeof val === 'undefined') {
                if (e.cache && typeof e.cache.doubleClickDisabled !== "undefined") {
                    return !e.cache.doubleClickDisabled;
                }
                return e.calendar.doubleClickHandling !== 'Disabled' && !e.data.doubleClickDisabled;
            }
            else {
                e.data.doubleClickDisabled = !val;
            }
        };

        this.client.deleteClickEnabled = function(val) {
            if (typeof val === 'undefined') {
                if (e.cache && typeof e.cache.deleteDisabled !== "undefined") {
                    return !e.cache.deleteDisabled;
                }
                return e.calendar.eventDeleteHandling !== 'Disabled' && !e.data.deleteDisabled;
            }
            else {
                e.data.deleteDisabled = !val;
            }
        };

        this.toJSON = function(key) {
            var json = {};
            json.value = this.id(); // still sending it with the old name
            json.id = this.id();
            json.text = this.text();
            json.start = this.start().toJSON();
            json.end = this.end().toJSON();
            json.resource = this.resource();
            json.isAllDay = false;
            json.recurrentMasterId = this.recurrentMasterId();
            json.tag = {};

            if (e.calendar.tagFields) {
                var fields = e.calendar.tagFields;
                for (var i = 0; i < fields.length; i++) {
                    json.tag[fields[i]] = this.tag(fields[i]);
                }
            }

            return json;
        };
    };


    /* JSON objects */

    DayPilot.EventData = function(e) {
        this.value = function() {
            return id;
        };
        this.tag = function() {
            return null;
        };
        this.start = function() {
            return new Date(0);
        };
        this.end = function() {
            return new Date(duration * 1000);
        };
        this.text = function() {
            return text;
        };
        this.isAllDay = function() {
            return false;
        };
        this.allday = this.isAllDay;
    };



    /* XMLHttpRequest */

    DayPilot.request = function(url, callback, postData, errorCallback) {
        var req = DayPilot.createXmlHttp();
        if (!req) {
            return;
        }

        req.open("POST", url, true);
        req.setRequestHeader('Content-type', 'text/plain');
        req.onreadystatechange = function() {
            if (req.readyState !== 4)
                return;
            if (req.status !== 200 && req.status !== 304) {
                if (errorCallback) {
                    errorCallback(req);
                }
                else {
                    if (console) { console.log('HTTP error ' + req.status); }
                }
                return;
            }
            callback(req);
        };
        if (req.readyState === 4) {
            return;
        }
        if (typeof postData === 'object') {
            postData = DayPilot.JSON.stringify(postData);
        }
        req.send(postData);
    };

    DayPilot.createXmlHttp = function() {
        var xmlHttp;
        try {
            xmlHttp = new XMLHttpRequest();
        }
        catch (e) {
            try {
                xmlHttp = new ActiveXObject("Microsoft.XMLHTTP");
            }
            catch (e) {
            }
        }
        return xmlHttp;
    };


    /* Date utils */

    // DayPilot.Date class
    /* Constructor signatures:
     
     -- new DayPilot.Date(date, isLocal)
     date - JavaScript Date object
     isLocal - true if the local time should be taken from date, otherwise GMT base is used
     
     -- new DayPilot.Date() - returns now, using local date
     
     -- new DayPilot.Date(string)
     string - date in ISO 8601 format, e.g. 2009-01-01T00:00:00
     
     */
    DayPilot.Date = function(date, isLocal) {

        if (typeof date === 'undefined') {  // date not set, use NOW
            this.isDayPilotDate = true; // allow class detection
            this.d = DayPilot.Date.fromLocal();
            this.ticks = this.d.getTime();
            this.value = this.toStringSortable();
            return;
        }

        if (date.isDayPilotDate) { // it's already DayPilot.Date object, return it (no copy)
            return date;
        }

        var cache = DayPilot.Date.Cache.Ctor;
        if (cache[date]) {
            return cache[date];
        }

        if (typeof date === "string") {
            var result = DayPilot.Date.fromStringSortable(date);
            cache[date] = result;
            return result;
        }

        if (!date.getFullYear) {  // it's not a date object, fail
            throw "date parameter is not a Date object: " + date;
        }

        if (isLocal) {  // if the date passed should be read as local date
            this.isDayPilotDate = true; // allow class detection
            this.d = DayPilot.Date.fromLocal(date);
            this.ticks = this.d.getTime();
        }
        else {  // should be read as GMT
            this.isDayPilotDate = true; // allow class detection
            this.d = date;
            this.ticks = this.d.getTime();
        }
        this.value = this.toStringSortable();
    };

    DayPilot.Date.Cache = {};
    DayPilot.Date.Cache.Parsing = {};
    DayPilot.Date.Cache.Ctor = {};

/*
    DayPilot.Date.prototype.toJSON = function() {
        return this.value;
    };
*/
    DayPilot.Date.prototype.addDays = function(days) {
        return new DayPilot.Date(DayPilot.Date.addDays(this.d, days));
    };

    DayPilot.Date.prototype.addHours = function(hours) {
        return this.addTime(hours * 60 * 60 * 1000);
    };

    DayPilot.Date.prototype.addMilliseconds = function(millis) {
        return this.addTime(millis);
    };

    DayPilot.Date.prototype.addMinutes = function(minutes) {
        return this.addTime(minutes * 60 * 1000);
    };

    DayPilot.Date.prototype.addMonths = function(months) {
        return new DayPilot.Date(DayPilot.Date.addMonths(this.d, months));
    };

    DayPilot.Date.prototype.addSeconds = function(seconds) {
        return this.addTime(seconds * 1000);
    };

    DayPilot.Date.prototype.addTime = function(ticks) {
        return new DayPilot.Date(DayPilot.Date.addTime(this.d, ticks));
    };

    DayPilot.Date.prototype.addYears = function(years) {
        var n = this.clone();
        n.d.setUTCFullYear(this.getYear() + years);
        return n;
    };

    DayPilot.Date.prototype.clone = function() {
        return new DayPilot.Date(DayPilot.Date.clone(this.d));
    };

    DayPilot.Date.prototype.dayOfWeek = function() {
        return this.d.getUTCDay();
    };
    
    DayPilot.Date.prototype.getDayOfWeek = function() {
        return this.d.getUTCDay();
    };

    DayPilot.Date.prototype.daysInMonth = function() {
        return DayPilot.Date.daysInMonth(this.d);
    };

    DayPilot.Date.prototype.dayOfYear = function() {
        return Math.ceil((this.getDatePart().getTime() - this.firstDayOfYear().getTime()) / 86400000) + 1;
    };

    DayPilot.Date.prototype.equals = function(another) {
        if (another === null) {
            return false;
        }
        if (another.isDayPilotDate) {
            return DayPilot.Date.equals(this.d, another.d);
        }
        else {
            throw "The parameter must be a DayPilot.Date object (DayPilot.Date.equals())";
        }
    };

    DayPilot.Date.prototype.firstDayOfMonth = function() {
        var utc = DayPilot.Date.firstDayOfMonth(this.getYear(), this.getMonth() + 1);
        return new DayPilot.Date(utc);
    };

    DayPilot.Date.prototype.firstDayOfYear = function() {
        var year = this.getYear();
        var d = new Date();
        d.setUTCFullYear(year, 0, 1);
        d.setUTCHours(0);
        d.setUTCMinutes(0);
        d.setUTCSeconds(0);
        d.setUTCMilliseconds(0);
        return new DayPilot.Date(d);
    };

    DayPilot.Date.prototype.firstDayOfWeek = function(weekStarts) {
        var utc = DayPilot.Date.firstDayOfWeek(this.d, weekStarts);
        return new DayPilot.Date(utc);
    };

    DayPilot.Date.prototype.getDay = function() {
        return this.d.getUTCDate();
    };

    DayPilot.Date.prototype.getDatePart = function() {
        return new DayPilot.Date(DayPilot.Date.getDate(this.d));
    };

    DayPilot.Date.prototype.getYear = function() {
        return this.d.getUTCFullYear();
    };

    DayPilot.Date.prototype.getHours = function() {
        return this.d.getUTCHours();
    };

    DayPilot.Date.prototype.getMilliseconds = function() {
        return this.d.getUTCMilliseconds();
    };

    DayPilot.Date.prototype.getMinutes = function() {
        return this.d.getUTCMinutes();
    };

    DayPilot.Date.prototype.getMonth = function() {
        return this.d.getUTCMonth();
    };

    DayPilot.Date.prototype.getSeconds = function() {
        return this.d.getUTCSeconds();
    };

    DayPilot.Date.prototype.getTotalTicks = function() {
        return this.getTime();
    };

    // undocumented
    DayPilot.Date.prototype.getTime = function() {
        /*
         if (typeof this.ticks !== 'number') {
         throw "Uninitialized DayPilot.Date (internal error)";
         }*/
        return this.ticks;
    };

    DayPilot.Date.prototype.getTimePart = function() {
        return DayPilot.Date.getTime(this.d);
    };

    DayPilot.Date.prototype.lastDayOfMonth = function() {
        var utc = DayPilot.Date.lastDayOfMonth(this.getYear(), this.getMonth() + 1);
        return new DayPilot.Date(utc);
    };

    DayPilot.Date.prototype.weekNumber = function() {
        var first = this.firstDayOfYear();
        var days = (this.getTime() - first.getTime()) / 86400000;
        return Math.ceil((days + first.dayOfWeek() + 1) / 7);
    };

    DayPilot.Date.prototype.local = function() {
        if (typeof this.offset === 'undefined') {
            return new DayPilot.Date(this.d);
        }
        return this.addMinutes(this.offset);
    };

    // ISO 8601
    DayPilot.Date.prototype.weekNumberISO = function() {
        var thursdayFlag = false;
        var dayOfYear = this.dayOfYear();

        var startWeekDayOfYear = this.firstDayOfYear().dayOfWeek();
        var endWeekDayOfYear = this.firstDayOfYear().addYears(1).addDays(-1).dayOfWeek();
        //int startWeekDayOfYear = new DateTime(date.getYear(), 1, 1).getDayOfWeekOrdinal();
        //int endWeekDayOfYear = new DateTime(date.getYear(), 12, 31).getDayOfWeekOrdinal();

        if (startWeekDayOfYear === 0) {
            startWeekDayOfYear = 7;
        }
        if (endWeekDayOfYear === 0) {
            endWeekDayOfYear = 7;
        }

        var daysInFirstWeek = 8 - (startWeekDayOfYear);

        if (startWeekDayOfYear === 4 || endWeekDayOfYear === 4) {
            thursdayFlag = true;
        }

        var fullWeeks = Math.ceil((dayOfYear - (daysInFirstWeek)) / 7.0);

        var weekNumber = fullWeeks;

        if (daysInFirstWeek >= 4) {
            weekNumber = weekNumber + 1;
        }

        if (weekNumber > 52 && !thursdayFlag) {
            weekNumber = 1;
        }

        if (weekNumber === 0) {
            weekNumber = this.firstDayOfYear().addDays(-1).weekNumberISO(); //weekNrISO8601(new DateTime(date.getYear() - 1, 12, 31));
        }

        return weekNumber;

    };

    DayPilot.Date.prototype.toDateLocal = function() {
        return DayPilot.Date.toLocal(this.d);
    };

    DayPilot.Date.prototype.toJSON = function() {
        return this.value;
    };

    // formatting and languages needed here
    DayPilot.Date.prototype.toString = function(pattern, locale) {
        if (typeof pattern === 'undefined') {
            return this.toStringSortable();
        }
        return new Pattern(pattern, locale).print(this);
    };

    DayPilot.Date.prototype.toStringSortable = function() {
        return DayPilot.Date.toStringSortable(this.d);
    };

    /* static functions, return DayPilot.Date object */


    // returns null if parsing was not successful
    DayPilot.Date.parse = function(str, pattern) {
        var p = new Pattern(pattern);
        return p.parse(str);
    };

    DayPilot.Date.fromStringSortable = function(string) {
        var result;
        //var datetime = /^(\d{4})-(\d{2})-(\d{2})T(\d{2}):(\d{2}):(\d{2})(?:(\+|-)(\d{2}):(\d{2}))?$/;
        //var date = /^(\d{4})-(\d{2})-(\d{2})$/;

        //var isValidDateTime = datetime.test(string);
        //var isValidDate = date.test(string);
        //var isValid = isValidDateTime || isValidDate;

        // 2010-01-01  --- 10
        // 
        // 2010-01-01T00:00:00  --- 19
        // 2010-01-01T00:00:00+00:00   --- 25

        if (!string) {
            throw "Can't create DayPilot.Date from empty string";
        }

        var len = string.length;
        var date = len === 10;
        var datetime = len = 19;
        var datetimetz = len === 25;

        if (!date && !datetime && !datetimetz) {
            throw "Invalid string format (use '2010-01-01', '2010-01-01T00:00:00', or '2010-01-01T00:00:00+00:00'.";
        }

        if (DayPilot.Date.Cache.Parsing[string]) {
            return DayPilot.Date.Cache.Parsing[string];
        }

        var year = string.substring(0, 4);
        var month = string.substring(5, 7);
        var day = string.substring(8, 10);

        var d = new Date();
        d.setUTCFullYear(year, month - 1, day);

        if (date) {
            d.setUTCHours(0);
            d.setUTCMinutes(0);
            d.setUTCSeconds(0);
            d.setUTCMilliseconds(0);
            result = new DayPilot.Date(d);
            DayPilot.Date.Cache.Parsing[string] = result;
            return result;
        }

        var hours = string.substring(11, 13);
        var minutes = string.substring(14, 16);
        var seconds = string.substring(17, 19);

        d.setUTCHours(hours);
        d.setUTCMinutes(minutes);
        d.setUTCSeconds(seconds);
        d.setUTCMilliseconds(0);
        var result = new DayPilot.Date(d);

        if (datetime) {
            DayPilot.Date.Cache.Parsing[string] = result;
            return result;
        }

        var tzdir = string[20];
        var tzhours = string.substring(21, 23);
        var tzminutes = string.substring(24);
        var tzoffset = tzhours * 60 + tzminutes;
        if (tzdir === "-") {
            tzoffset = -tzoffset;
        }
        result = result.addMinutes(-tzoffset); // get UTC base
        result.offset = offset; // store offset

        DayPilot.Date.Cache.Parsing[string] = result;
        return result;
    };

    /* internal functions, all operate with GMT base of the date object 
     (except of DayPilot.Date.fromLocal()) */

    DayPilot.Date.addDays = function(date, days) {
        var d = new Date();
        d.setTime(date.getTime() + days * 24 * 60 * 60 * 1000);
        return d;
    };

    DayPilot.Date.addMinutes = function(date, minutes) {
        var d = new Date();
        d.setTime(date.getTime() + minutes * 60 * 1000);
        return d;
    };

    DayPilot.Date.addMonths = function(date, months) {
        if (months === 0)
            return date;

        var y = date.getUTCFullYear();
        var m = date.getUTCMonth() + 1;

        if (months > 0) {
            while (months >= 12) {
                months -= 12;
                y++;
            }
            if (months > 12 - m) {
                y++;
                m = months - (12 - m);
            }
            else {
                m += months;
            }
        }
        else {
            while (months <= -12) {
                months += 12;
                y--;
            }
            if (m <= months) {  // 
                y--;
                m = 12 - (months + m);
            }
            else {
                m = m + months;
            }
        }

        var d = DayPilot.Date.clone(date);
        d.setUTCFullYear(y);
        d.setUTCMonth(m - 1);

        return d;
    };

    DayPilot.Date.addTime = function(date, time) {
        var d = new Date();
        d.setTime(date.getTime() + time);
        return d;
    };

    DayPilot.Date.clone = function(original) {
        var d = new Date();
        return DayPilot.Date.dateFromTicks(original.getTime());
    };


    // rename candidate: diffDays
    DayPilot.Date.daysDiff = function(first, second) {
        if (first.getTime() > second.getTime()) {
            return null;
        }

        var i = 0;
        var fDay = DayPilot.Date.getDate(first);
        var sDay = DayPilot.Date.getDate(second);

        while (fDay < sDay) {
            fDay = DayPilot.Date.addDays(fDay, 1);
            i++;
        }

        return i;
    };

    DayPilot.Date.daysInMonth = function(year, month) {  // accepts also: function(date)
        if (year.getUTCFullYear) { // it's a date object
            month = year.getUTCMonth() + 1;
            year = year.getUTCFullYear();
        }

        var m = [31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31];
        if (month !== 2)
            return m[month - 1];
        if (year % 4 !== 0)
            return m[1];
        if (year % 100 === 0 && year % 400 !== 0)
            return m[1];
        return m[1] + 1;
    };

    DayPilot.Date.daysSpan = function(first, second) {
        if (first.getTime() === second.getTime()) {
            return 0;
        }

        var diff = DayPilot.Date.daysDiff(first, second);

        if (DayPilot.Date.equals(second, DayPilot.Date.getDate(second))) {
            diff--;
        }

        return diff;
    };

    DayPilot.Date.diff = function(first, second) { // = first - second
        if (!(first && second && first.getTime && second.getTime)) {
            throw "Both compared objects must be Date objects (DayPilot.Date.diff).";
        }

        return first.getTime() - second.getTime();
    };

    DayPilot.Date.equals = function(first, second) {
        return first.getTime() === second.getTime();
    };

    DayPilot.Date.fromLocal = function(localDate) {
        if (!localDate) {
            localDate = new Date();
        }

        var d = new Date();
        d.setUTCFullYear(localDate.getFullYear(), localDate.getMonth(), localDate.getDate());
        d.setUTCHours(localDate.getHours());
        d.setUTCMinutes(localDate.getMinutes());
        d.setUTCSeconds(localDate.getSeconds());
        d.setUTCMilliseconds(localDate.getMilliseconds());
        return d;
    };

    DayPilot.Date.firstDayOfMonth = function(year, month) {
        var d = new Date();
        d.setUTCFullYear(year, month - 1, 1);
        d.setUTCHours(0);
        d.setUTCMinutes(0);
        d.setUTCSeconds(0);
        d.setUTCMilliseconds(0);
        return d;
    };

    DayPilot.Date.firstDayOfWeek = function(d, weekStarts) {
        var day = d.getUTCDay();
        while (day !== weekStarts) {
            d = DayPilot.Date.addDays(d, -1);
            day = d.getUTCDay();
        }
        return d;
    };


    // rename candidate: fromTicks
    DayPilot.Date.dateFromTicks = function(ticks) {
        var d = new Date();
        d.setTime(ticks);
        return d;
    };

    // rename candidate: getDatePart
    DayPilot.Date.getDate = function(original) {
        var d = DayPilot.Date.clone(original);
        d.setUTCHours(0);
        d.setUTCMinutes(0);
        d.setUTCSeconds(0);
        d.setUTCMilliseconds(0);
        return d;
    };

    DayPilot.Date.getStart = function(year, month, weekStarts) {  // gets the first days of week where the first day of month occurs
        var fdom = DayPilot.Date.firstDayOfMonth(year, month);
        d = DayPilot.Date.firstDayOfWeek(fdom, weekStarts);
        return d;
    };

    // rename candidate: getTimePart
    DayPilot.Date.getTime = function(original) {
        var date = DayPilot.Date.getDate(original);

        return DayPilot.Date.diff(original, date);
    };

    // rename candidate: toHourString
    DayPilot.Date.hours = function(date, use12) {

        var minute = date.getUTCMinutes();
        if (minute < 10)
            minute = "0" + minute;


        var hour = date.getUTCHours();
        //if (hour < 10) hour = "0" + hour;

        if (use12) {
            var am = hour < 12;
            var hour = hour % 12;
            if (hour === 0) {
                hour = 12;
            }
            var suffix = am ? "AM" : "PM";
            return hour + ':' + minute + ' ' + suffix;
        }
        else {
            return hour + ':' + minute;
        }
    };

    DayPilot.Date.lastDayOfMonth = function(year, month) {
        var d = DayPilot.Date.firstDayOfMonth(year, month);
        var length = DayPilot.Date.daysInMonth(year, month);
        d.setUTCDate(length);
        return d;
    };

    DayPilot.Date.max = function(first, second) {
        if (first.getTime() > second.getTime()) {
            return first;
        }
        else {
            return second;
        }
    };

    DayPilot.Date.min = function(first, second) {
        if (first.getTime() < second.getTime()) {
            return first;
        }
        else {
            return second;
        }
    };

    DayPilot.Date.today = function() {
        var relative = new Date();
        var d = new Date();
        d.setUTCFullYear(relative.getFullYear());
        d.setUTCMonth(relative.getMonth());
        d.setUTCDate(relative.getDate());

        return d;
    };

    DayPilot.Date.toLocal = function(date) {
        if (!date) {
            date = new Date();
        }

        var d = new Date();
        d.setFullYear(date.getUTCFullYear(), date.getUTCMonth(), date.getUTCDate());
        d.setHours(date.getUTCHours());
        d.setMinutes(date.getUTCMinutes());
        d.setSeconds(date.getUTCSeconds());
        d.setMilliseconds(date.getUTCMilliseconds());
        return d;
    };


    DayPilot.Date.toStringSortable = function(date) {
        if (date.isDayPilotDate) {
            return date.toStringSortable();
        }

        var d = date;
        var second = d.getUTCSeconds();
        if (second < 10)
            second = "0" + second;
        var minute = d.getUTCMinutes();
        if (minute < 10)
            minute = "0" + minute;
        var hour = d.getUTCHours();
        if (hour < 10)
            hour = "0" + hour;
        var day = d.getUTCDate();
        if (day < 10)
            day = "0" + day;
        var month = d.getUTCMonth() + 1;
        if (month < 10)
            month = "0" + month;
        var year = d.getUTCFullYear();

        if (year <= 0) {
            throw "The minimum year supported is 1.";
        }
        if (year < 10) {
            year = "000" + year;
        }
        else if (year < 100) {
            year = "00" + year;
        }
        else if (year < 1000) {
            year = "0" + year;
        }

        return year + "-" + month + "-" + day + 'T' + hour + ":" + minute + ":" + second;
    };

    var Pattern = function(pattern, locale) {
        if (typeof locale === "string") {
            locale = DayPilot.Locale.find(locale);
        }
        var locale = locale || DayPilot.Locale.US;
        var all = [
            {"seq": "yyyy", "expr": "[0-9]{4,4\u007d", "str": function(d) {
                    return d.getYear();
                }},
            {"seq": "MMMM", "expr": "[a-z]*", "str": function(d) {
                    var r = locale.monthNames[d.getMonth()];
                    return r;
                }},
            {"seq": "MMM", "expr": "[a-z]*", "str": function(d) {
                    var r = locale.monthNamesShort[d.getMonth()];
                    return r;
                }},
            {"seq": "MM", "expr": "[0-9]{2,2\u007d", "str": function(d) {
                    var r = d.getMonth() + 1;
                    return r < 10 ? "0" + r : r;
                }},
            {"seq": "M", "expr": "[0-9]{1,2\u007d", "str": function(d) {
                    var r = d.getMonth() + 1;
                    return r;
                }},
            {"seq": "dddd", "expr": "[a-z]*", "str": function(d) {
                    var r = locale.dayNames[d.getDayOfWeek()];
                    return r;
                }},
            {"seq": "ddd", "expr": "[a-z]*", "str": function(d) {
                    var r = locale.dayNamesShort[d.getDayOfWeek()];
                    return r;
                }},
            {"seq": "dd", "expr": "[0-9]{2,2\u007d", "str": function(d) {
                    var r = d.getDay();
                    return r < 10 ? "0" + r : r;
                }},
            {"seq": "d", "expr": "[0-9]{1,2\u007d", "str": function(d) {
                    var r = d.getDay();
                    return r;
                }},
            {"seq": "m", "expr": "[0-9]{1,2\u007d", "str": function(d) {
                    var r = d.getMinutes();
                    return r;
                }},
            {"seq": "mm", "expr": "[0-9]{2,2\u007d", "str": function(d) {
                    var r = d.getMinutes();
                    return r < 10 ? "0" + r : r;
                }},
            {"seq": "H", "expr": "[0-9]{1,2\u007d", "str": function(d) {
                    var r = d.getHours();
                    return r;
                }},
            {"seq": "HH", "expr": "[0-9]{2,2\u007d", "str": function(d) {
                    var r = d.getHours();
                    return r < 10 ? "0" + r : r;
                }},
            {"seq": "h", "expr": "[0-9]{1,2\u007d", "str": function(d) {
                    var hour = d.getHours();
                    var hour = hour % 12;
                    if (hour === 0) {
                        hour = 12;
                    }
                    return hour;
                }},
            {"seq": "hh", "expr": "[0-9]{2,2\u007d", "str": function(d) {
                    var hour = d.getHours();
                    var hour = hour % 12;
                    if (hour === 0) {
                        hour = 12;
                    }
                    var r = hour;
                    return r < 10 ? "0" + r : r;
                }},
            {"seq": "tt", "expr": "(AM|PM)", "str": function(d) {
                    var hour = d.getHours();
                    var am = hour < 12;
                    return am ? "AM" : "PM";
                }},
            {"seq": "s", "expr": "[0-9]{1,2\u007d", "str": function(d) {
                    var r = d.getSeconds();
                    return r;
                }},
            {"seq": "ss", "expr": "[0-9]{2,2\u007d", "str": function(d) {
                    var r = d.getSeconds();
                    return r < 10 ? "0" + r : r;
                }}
        ];

        var escapeRegex = function(text) {
            return text.replace(/[-[\]{}()*+?.,\\^$|#\s]/g, "\\$&");
        };

        this.init = function() {
            this.year = this.findSequence("yyyy");
            this.month = this.findSequence("MM") || this.findSequence("M");
            this.day = this.findSequence("dd") || this.findSequence("d");

            this.hours = this.findSequence("HH") || this.findSequence("H");
            this.minutes = this.findSequence("mm") || this.findSequence("m");
            this.seconds = this.findSequence("ss") || this.findSequence("s");
        };

        this.findSequence = function(seq) {

            var index = pattern.indexOf(seq);
            if (index === -1) {
                return null;
            }
            return {
                "findValue": function(input) {
                    var prepared = escapeRegex(pattern);
                    for (var i = 0; i < all.length; i++) {
                        var len = all[i].length;
                        var pick = (seq === all[i].seq);
                        //var expr = "";
                        var expr = all[i].expr;
                        if (pick) {
                            expr = "(" + expr + ")";
                        }
                        prepared = prepared.replace(all[i].seq, expr);
                    }
                    
                    try {
                        var r = new RegExp(prepared);
                        var array = r.exec(input);
                        if (!array) {
                            return null;
                        }
                        return parseInt(array[1]);
                    }
                    catch (e) {
                        throw "unable to create regex from: " + prepared;
                    }
                }
            };
        };

        this.print = function(date) {
            // always recompiles the pattern

            var find = function(t) {
                for (var i = 0; i < all.length; i++) {
                    if (all[i].seq === t) {
                        return all[i];
                    }
                }
                return null;
            };

            var eos = pattern.length <= 0;
            var pos = 0;
            var components = [];

            while (!eos) {
                var rem = pattern.substring(pos);
                var matches = /(.)\1*/.exec(rem);
                if (matches && matches.length > 0) {
                    var match = matches[0];
                    var q = find(match);
                    if (q) {
                        components.push(q);
                    }
                    else {
                        components.push(match);
                    }
                    pos += match.length;
                    eos = pattern.length <= pos;
                }
                else {
                    eos = true;
                }
            }

            // resolve placeholders
            for (var i = 0; i < components.length; i++) {
                var c = components[i];
                if (typeof c !== 'string') {
                    components[i] = c.str(date);
                }
            }

            return components.join("");
        };



        this.parse = function(input) {

            var year = this.year.findValue(input);
            if (!year) {
                return null; // unparseable
            }

            var month = this.month.findValue(input);
            var day = this.day.findValue(input);

            var hours = this.hours ? this.hours.findValue(input) : 0;
            var minutes = this.minutes ? this.minutes.findValue(input) : 0;
            var seconds = this.seconds ? this.seconds.findValue(input) : 0;

            var d = new Date();
            d.setUTCFullYear(year, month - 1, day);
            d.setUTCHours(hours);
            d.setUTCMinutes(minutes);
            d.setUTCSeconds(seconds);
            d.setUTCMilliseconds(0);

            return new DayPilot.Date(d);
        };

        this.init();

    };

    DayPilot.Action = function(calendar, action, params, data) {
        this.calendar = calendar;
        this.isAction = true;
        this.action = action;
        this.params = params;
        this.data = data;

        this.notify = function() {
            calendar.invokeEvent("Immediate", this.action, this.params, this.data);
        };

        this.auto = function() {
            calendar.invokeEvent("Notify", this.action, this.params, this.data);
        };

        this.queue = function() {
            calendar.queue.add(this);
        };


        this.toJSON = function() {
            var json = {};
            json.name = this.action;
            json.params = this.params;
            json.data = this.data;

            return json;
        };

    };

    DayPilot.Locale = function(id, config) {
        this.id = id;
        this.dayNames = ["Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"];
        this.dayNamesShort = ["Su", "Mo", "Tu", "We", "Th", "Fr", "Sa"];
        this.monthNames = ["January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"];
        this.monthNamesShort  = ['Jan','Feb','Mar','Apr','May','Jun','Jul','Aug','Sep','Oct','Nov','Dec'];
        this.datePattern = "M/d/yyyy";
        this.timePattern = "H:mm";
        this.dateTimePattern = "M/d/yyyy H:mm";
        this.timeFormat = "Clock12Hours";
        this.weekStarts = 0; // Sunday

        if (config) {
            for (var name in config) {
                this[name] = config[name];
            }
        }
    };

    DayPilot.Locale.all = {};

    DayPilot.Locale.find = function(id) {
        if (!id) {
            return null;
        }
        var normalized = id.toLowerCase();
        if (normalized.length > 2) {
            normalized[2] = '-';
        }
        return DayPilot.Locale.all[normalized];
    };
    
    DayPilot.Locale.register = function(locale) {
        DayPilot.Locale.all[locale.id] = locale;
    };

    DayPilot.Locale.register(new DayPilot.Locale('ca-es', {'dayNames':['diumenge','dilluns','dimarts','dimecres','dijous','divendres','dissabte'],'dayNamesShort':['dg','dl','dt','dc','dj','dv','ds'],'monthNames':['gener','febrer','març','abril','maig','juny','juliol','agost','setembre','octubre','novembre','desembre',''],'monthNamesShort':['gen.','febr.','març','abr.','maig','juny','jul.','ag.','set.','oct.','nov.','des.',''],'timePattern':'H:mm','datePattern':'dd/MM/yyyy','dateTimePattern':'dd/MM/yyyy H:mm','timeFormat':'Clock24Hours','weekStarts':1}));
    DayPilot.Locale.register(new DayPilot.Locale('cs-cz', {'dayNames':['nedìle','pondìlí','úterý','støeda','ètvrtek','pátek','sobota'],'dayNamesShort':['ne','po','út','st','èt','pá','so'],'monthNames':['leden','únor','bøezen','duben','kvìten','èerven','èervenec','srpen','záøí','øíjen','listopad','prosinec',''],'monthNamesShort':['I','II','III','IV','V','VI','VII','VIII','IX','X','XI','XII',''],'timePattern':'H:mm','datePattern':'d. M. yyyy','dateTimePattern':'d. M. yyyy H:mm','timeFormat':'Clock24Hours','weekStarts':1}));
    DayPilot.Locale.register(new DayPilot.Locale('da-dk', {'dayNames':['sondag','mandag','tirsdag','onsdag','torsdag','fredag','lordag'],'dayNamesShort':['so','ma','ti','on','to','fr','lo'],'monthNames':['januar','februar','marts','april','maj','juni','juli','august','september','oktober','november','december',''],'monthNamesShort':['jan','feb','mar','apr','maj','jun','jul','aug','sep','okt','nov','dec',''],'timePattern':'HH:mm','datePattern':'dd-MM-yyyy','dateTimePattern':'dd-MM-yyyy HH:mm','timeFormat':'Clock24Hours','weekStarts':1}));
    DayPilot.Locale.register(new DayPilot.Locale('de-at', {'dayNames':['Sonntag','Montag','Dienstag','Mittwoch','Donnerstag','Freitag','Samstag'],'dayNamesShort':['So','Mo','Di','Mi','Do','Fr','Sa'],'monthNames':['Jänner','Februar','März','April','Mai','Juni','Juli','August','September','Oktober','November','Dezember',''],'monthNamesShort':['Jän','Feb','Mär','Apr','Mai','Jun','Jul','Aug','Sep','Okt','Nov','Dez',''],'timePattern':'HH:mm','datePattern':'dd.MM.yyyy','dateTimePattern':'dd.MM.yyyy HH:mm','timeFormat':'Clock24Hours','weekStarts':1}));
    DayPilot.Locale.register(new DayPilot.Locale('de-ch', {'dayNames':['Sonntag','Montag','Dienstag','Mittwoch','Donnerstag','Freitag','Samstag'],'dayNamesShort':['So','Mo','Di','Mi','Do','Fr','Sa'],'monthNames':['Januar','Februar','März','April','Mai','Juni','Juli','August','September','Oktober','November','Dezember',''],'monthNamesShort':['Jan','Feb','Mrz','Apr','Mai','Jun','Jul','Aug','Sep','Okt','Nov','Dez',''],'timePattern':'HH:mm','datePattern':'dd.MM.yyyy','dateTimePattern':'dd.MM.yyyy HH:mm','timeFormat':'Clock24Hours','weekStarts':1}));
    DayPilot.Locale.register(new DayPilot.Locale('de-de', {'dayNames':['Sonntag','Montag','Dienstag','Mittwoch','Donnerstag','Freitag','Samstag'],'dayNamesShort':['So','Mo','Di','Mi','Do','Fr','Sa'],'monthNames':['Januar','Februar','März','April','Mai','Juni','Juli','August','September','Oktober','November','Dezember',''],'monthNamesShort':['Jan','Feb','Mrz','Apr','Mai','Jun','Jul','Aug','Sep','Okt','Nov','Dez',''],'timePattern':'HH:mm','datePattern':'dd.MM.yyyy','dateTimePattern':'dd.MM.yyyy HH:mm','timeFormat':'Clock24Hours','weekStarts':1}));
    DayPilot.Locale.register(new DayPilot.Locale('de-lu', {'dayNames':['Sonntag','Montag','Dienstag','Mittwoch','Donnerstag','Freitag','Samstag'],'dayNamesShort':['So','Mo','Di','Mi','Do','Fr','Sa'],'monthNames':['Januar','Februar','März','April','Mai','Juni','Juli','August','September','Oktober','November','Dezember',''],'monthNamesShort':['Jan','Feb','Mrz','Apr','Mai','Jun','Jul','Aug','Sep','Okt','Nov','Dez',''],'timePattern':'HH:mm','datePattern':'dd.MM.yyyy','dateTimePattern':'dd.MM.yyyy HH:mm','timeFormat':'Clock24Hours','weekStarts':1}));
    DayPilot.Locale.register(new DayPilot.Locale('en-au', {'dayNames':['Sunday','Monday','Tuesday','Wednesday','Thursday','Friday','Saturday'],'dayNamesShort':['Su','Mo','Tu','We','Th','Fr','Sa'],'monthNames':['January','February','March','April','May','June','July','August','September','October','November','December',''],'monthNamesShort':['Jan','Feb','Mar','Apr','May','Jun','Jul','Aug','Sep','Oct','Nov','Dec',''],'timePattern':'h:mm tt','datePattern':'d/MM/yyyy','dateTimePattern':'d/MM/yyyy h:mm tt','timeFormat':'Clock12Hours','weekStarts':1}));
    DayPilot.Locale.register(new DayPilot.Locale('en-ca', {'dayNames':['Sunday','Monday','Tuesday','Wednesday','Thursday','Friday','Saturday'],'dayNamesShort':['Su','Mo','Tu','We','Th','Fr','Sa'],'monthNames':['January','February','March','April','May','June','July','August','September','October','November','December',''],'monthNamesShort':['Jan','Feb','Mar','Apr','May','Jun','Jul','Aug','Sep','Oct','Nov','Dec',''],'timePattern':'h:mm tt','datePattern':'yyyy-MM-dd','dateTimePattern':'yyyy-MM-dd h:mm tt','timeFormat':'Clock12Hours','weekStarts':0}));
    DayPilot.Locale.register(new DayPilot.Locale('en-gb', {'dayNames':['Sunday','Monday','Tuesday','Wednesday','Thursday','Friday','Saturday'],'dayNamesShort':['Su','Mo','Tu','We','Th','Fr','Sa'],'monthNames':['January','February','March','April','May','June','July','August','September','October','November','December',''],'monthNamesShort':['Jan','Feb','Mar','Apr','May','Jun','Jul','Aug','Sep','Oct','Nov','Dec',''],'timePattern':'HH:mm','datePattern':'dd/MM/yyyy','dateTimePattern':'dd/MM/yyyy HH:mm','timeFormat':'Clock24Hours','weekStarts':1}));
    DayPilot.Locale.register(new DayPilot.Locale('en-us', {'dayNames':['Sunday','Monday','Tuesday','Wednesday','Thursday','Friday','Saturday'],'dayNamesShort':['Su','Mo','Tu','We','Th','Fr','Sa'],'monthNames':['January','February','March','April','May','June','July','August','September','October','November','December',''],'monthNamesShort':['Jan','Feb','Mar','Apr','May','Jun','Jul','Aug','Sep','Oct','Nov','Dec',''],'timePattern':'h:mm tt','datePattern':'M/d/yyyy','dateTimePattern':'M/d/yyyy h:mm tt','timeFormat':'Clock12Hours','weekStarts':0}));
    DayPilot.Locale.register(new DayPilot.Locale('es-es', {'dayNames':['domingo','lunes','martes','miércoles','jueves','viernes','sábado'],'dayNamesShort':['D','L','M','X','J','V','S'],'monthNames':['enero','febrero','marzo','abril','mayo','junio','julio','agosto','septiembre','octubre','noviembre','diciembre',''],'monthNamesShort':['ene.','feb.','mar.','abr.','may.','jun.','jul.','ago.','sep.','oct.','nov.','dic.',''],'timePattern':'H:mm','datePattern':'dd/MM/yyyy','dateTimePattern':'dd/MM/yyyy H:mm','timeFormat':'Clock24Hours','weekStarts':1}));
    DayPilot.Locale.register(new DayPilot.Locale('es-mx', {'dayNames':['domingo','lunes','martes','miércoles','jueves','viernes','sábado'],'dayNamesShort':['do.','lu.','ma.','mi.','ju.','vi.','sá.'],'monthNames':['enero','febrero','marzo','abril','mayo','junio','julio','agosto','septiembre','octubre','noviembre','diciembre',''],'monthNamesShort':['ene.','feb.','mar.','abr.','may.','jun.','jul.','ago.','sep.','oct.','nov.','dic.',''],'timePattern':'hh:mm tt','datePattern':'dd/MM/yyyy','dateTimePattern':'dd/MM/yyyy hh:mm tt','timeFormat':'Clock12Hours','weekStarts':0}));
    DayPilot.Locale.register(new DayPilot.Locale('eu-es', {'dayNames':['igandea','astelehena','asteartea','asteazkena','osteguna','ostirala','larunbata'],'dayNamesShort':['ig','al','as','az','og','or','lr'],'monthNames':['urtarrila','otsaila','martxoa','apirila','maiatza','ekaina','uztaila','abuztua','iraila','urria','azaroa','abendua',''],'monthNamesShort':['urt.','ots.','mar.','api.','mai.','eka.','uzt.','abu.','ira.','urr.','aza.','abe.',''],'timePattern':'H:mm','datePattern':'yyyy/MM/dd','dateTimePattern':'yyyy/MM/dd H:mm','timeFormat':'Clock24Hours','weekStarts':1}));
    DayPilot.Locale.register(new DayPilot.Locale('fi-fi', {'dayNames':['sunnuntai','maanantai','tiistai','keskiviikko','torstai','perjantai','lauantai'],'dayNamesShort':['su','ma','ti','ke','to','pe','la'],'monthNames':['tammikuu','helmikuu','maaliskuu','huhtikuu','toukokuu','kesäkuu','heinäkuu','elokuu','syyskuu','lokakuu','marraskuu','joulukuu',''],'monthNamesShort':['tammi','helmi','maalis','huhti','touko','kesä','heinä','elo','syys','loka','marras','joulu',''],'timePattern':'H:mm','datePattern':'d.M.yyyy','dateTimePattern':'d.M.yyyy H:mm','timeFormat':'Clock24Hours','weekStarts':1}));
    DayPilot.Locale.register(new DayPilot.Locale('fr-be', {'dayNames':['dimanche','lundi','mardi','mercredi','jeudi','vendredi','samedi'],'dayNamesShort':['di','lu','ma','me','je','ve','sa'],'monthNames':['janvier','février','mars','avril','mai','juin','juillet','aout','septembre','octobre','novembre','décembre',''],'monthNamesShort':['janv.','févr.','mars','avr.','mai','juin','juil.','aout','sept.','oct.','nov.','déc.',''],'timePattern':'HH:mm','datePattern':'dd-MM-yy','dateTimePattern':'dd-MM-yy HH:mm','timeFormat':'Clock24Hours','weekStarts':1}));
    DayPilot.Locale.register(new DayPilot.Locale('fr-ch', {'dayNames':['dimanche','lundi','mardi','mercredi','jeudi','vendredi','samedi'],'dayNamesShort':['di','lu','ma','me','je','ve','sa'],'monthNames':['janvier','février','mars','avril','mai','juin','juillet','aout','septembre','octobre','novembre','décembre',''],'monthNamesShort':['janv.','févr.','mars','avr.','mai','juin','juil.','aout','sept.','oct.','nov.','déc.',''],'timePattern':'HH:mm','datePattern':'dd.MM.yyyy','dateTimePattern':'dd.MM.yyyy HH:mm','timeFormat':'Clock24Hours','weekStarts':1}));
    DayPilot.Locale.register(new DayPilot.Locale('fr-fr', {'dayNames':['dimanche','lundi','mardi','mercredi','jeudi','vendredi','samedi'],'dayNamesShort':['di','lu','ma','me','je','ve','sa'],'monthNames':['janvier','février','mars','avril','mai','juin','juillet','aout','septembre','octobre','novembre','décembre',''],'monthNamesShort':['janv.','févr.','mars','avr.','mai','juin','juil.','aout','sept.','oct.','nov.','déc.',''],'timePattern':'HH:mm','datePattern':'dd/MM/yyyy','dateTimePattern':'dd/MM/yyyy HH:mm','timeFormat':'Clock24Hours','weekStarts':1}));
    DayPilot.Locale.register(new DayPilot.Locale('fr-lu', {'dayNames':['dimanche','lundi','mardi','mercredi','jeudi','vendredi','samedi'],'dayNamesShort':['di','lu','ma','me','je','ve','sa'],'monthNames':['janvier','février','mars','avril','mai','juin','juillet','aout','septembre','octobre','novembre','décembre',''],'monthNamesShort':['janv.','févr.','mars','avr.','mai','juin','juil.','aout','sept.','oct.','nov.','déc.',''],'timePattern':'HH:mm','datePattern':'dd/MM/yyyy','dateTimePattern':'dd/MM/yyyy HH:mm','timeFormat':'Clock24Hours','weekStarts':1}));
    DayPilot.Locale.register(new DayPilot.Locale('gl-es', {'dayNames':['domingo','luns','martes','mércores','xoves','venres','sábado'],'dayNamesShort':['do','lu','ma','mé','xo','ve','sá'],'monthNames':['xaneiro','febreiro','marzo','abril','maio','xuno','xullo','agosto','setembro','outubro','novembro','decembro',''],'monthNamesShort':['xan','feb','mar','abr','maio','xuno','xul','ago','set','out','nov','dec',''],'timePattern':'H:mm','datePattern':'dd/MM/yyyy','dateTimePattern':'dd/MM/yyyy H:mm','timeFormat':'Clock24Hours','weekStarts':1}));
    DayPilot.Locale.register(new DayPilot.Locale('it-it', {'dayNames':['domenica','lunedi','martedi','mercoledi','giovedi','venerdi','sabato'],'dayNamesShort':['do','lu','ma','me','gi','ve','sa'],'monthNames':['gennaio','febbraio','marzo','aprile','maggio','giugno','luglio','agosto','settembre','ottobre','novembre','dicembre',''],'monthNamesShort':['gen','feb','mar','apr','mag','giu','lug','ago','set','ott','nov','dic',''],'timePattern':'HH:mm','datePattern':'dd/MM/yyyy','dateTimePattern':'dd/MM/yyyy HH:mm','timeFormat':'Clock24Hours','weekStarts':1}));
    DayPilot.Locale.register(new DayPilot.Locale('it-ch', {'dayNames':['domenica','lunedi','martedi','mercoledi','giovedi','venerdi','sabato'],'dayNamesShort':['do','lu','ma','me','gi','ve','sa'],'monthNames':['gennaio','febbraio','marzo','aprile','maggio','giugno','luglio','agosto','settembre','ottobre','novembre','dicembre',''],'monthNamesShort':['gen','feb','mar','apr','mag','giu','lug','ago','set','ott','nov','dic',''],'timePattern':'HH:mm','datePattern':'dd.MM.yyyy','dateTimePattern':'dd.MM.yyyy HH:mm','timeFormat':'Clock24Hours','weekStarts':1}));
    DayPilot.Locale.register(new DayPilot.Locale('ja-jp', {'dayNames':['???','???','???','???','???','???','???'],'dayNamesShort':['?','?','?','?','?','?','?'],'monthNames':['1?','2?','3?','4?','5?','6?','7?','8?','9?','10?','11?','12?',''],'monthNamesShort':['1','2','3','4','5','6','7','8','9','10','11','12',''],'timePattern':'H:mm','datePattern':'yyyy/MM/dd','dateTimePattern':'yyyy/MM/dd H:mm','timeFormat':'Clock24Hours','weekStarts':0}));
    DayPilot.Locale.register(new DayPilot.Locale('nb-no', {'dayNames':['sondag','mandag','tirsdag','onsdag','torsdag','fredag','lordag'],'dayNamesShort':['so','ma','ti','on','to','fr','lo'],'monthNames':['januar','februar','mars','april','mai','juni','juli','august','september','oktober','november','desember',''],'monthNamesShort':['jan','feb','mar','apr','mai','jun','jul','aug','sep','okt','nov','des',''],'timePattern':'HH:mm','datePattern':'dd.MM.yyyy','dateTimePattern':'dd.MM.yyyy HH:mm','timeFormat':'Clock24Hours','weekStarts':1}));
    DayPilot.Locale.register(new DayPilot.Locale('nl-nl', {'dayNames':['zondag','maandag','dinsdag','woensdag','donderdag','vrijdag','zaterdag'],'dayNamesShort':['zo','ma','di','wo','do','vr','za'],'monthNames':['januari','februari','maart','april','mei','juni','juli','augustus','september','oktober','november','december',''],'monthNamesShort':['jan','feb','mrt','apr','mei','jun','jul','aug','sep','okt','nov','dec',''],'timePattern':'HH:mm','datePattern':'d-M-yyyy','dateTimePattern':'d-M-yyyy HH:mm','timeFormat':'Clock24Hours','weekStarts':1}));
    DayPilot.Locale.register(new DayPilot.Locale('nl-be', {'dayNames':['zondag','maandag','dinsdag','woensdag','donderdag','vrijdag','zaterdag'],'dayNamesShort':['zo','ma','di','wo','do','vr','za'],'monthNames':['januari','februari','maart','april','mei','juni','juli','augustus','september','oktober','november','december',''],'monthNamesShort':['jan','feb','mrt','apr','mei','jun','jul','aug','sep','okt','nov','dec',''],'timePattern':'H:mm','datePattern':'d/MM/yyyy','dateTimePattern':'d/MM/yyyy H:mm','timeFormat':'Clock24Hours','weekStarts':1}));
    DayPilot.Locale.register(new DayPilot.Locale('nn-no', {'dayNames':['sondag','mandag','tysdag','onsdag','torsdag','fredag','laurdag'],'dayNamesShort':['so','ma','ty','on','to','fr','la'],'monthNames':['januar','februar','mars','april','mai','juni','juli','august','september','oktober','november','desember',''],'monthNamesShort':['jan','feb','mar','apr','mai','jun','jul','aug','sep','okt','nov','des',''],'timePattern':'HH:mm','datePattern':'dd.MM.yyyy','dateTimePattern':'dd.MM.yyyy HH:mm','timeFormat':'Clock24Hours','weekStarts':1}));
    DayPilot.Locale.register(new DayPilot.Locale('pt-br', {'dayNames':['domingo','segunda-feira','terça-feira','quarta-feira','quinta-feira','sexta-feira','sábado'],'dayNamesShort':['D','S','T','Q','Q','S','S'],'monthNames':['janeiro','fevereiro','março','abril','maio','junho','julho','agosto','setembro','outubro','novembro','dezembro',''],'monthNamesShort':['jan','fev','mar','abr','mai','jun','jul','ago','set','out','nov','dez',''],'timePattern':'HH:mm','datePattern':'dd/MM/yyyy','dateTimePattern':'dd/MM/yyyy HH:mm','timeFormat':'Clock24Hours','weekStarts':0}));
    DayPilot.Locale.register(new DayPilot.Locale('pl-pl', {'dayNames':['niedziela','poniedzia³ek','wtorek','œroda','czwartek','pi¹tek','sobota'],'dayNamesShort':['N','Pn','Wt','Œr','Cz','Pt','So'],'monthNames':['styczeñ','luty','marzec','kwiecieñ','maj','czerwiec','lipiec','sierpieñ','wrzesieñ','paŸdziernik','listopad','grudzieñ',''],'monthNamesShort':['sty','lut','mar','kwi','maj','cze','lip','sie','wrz','paŸ','lis','gru',''],'timePattern':'HH:mm','datePattern':'yyyy-MM-dd','dateTimePattern':'yyyy-MM-dd HH:mm','timeFormat':'Clock24Hours','weekStarts':1}));
    DayPilot.Locale.register(new DayPilot.Locale('pt-pt', {'dayNames':['domingo','segunda-feira','terça-feira','quarta-feira','quinta-feira','sexta-feira','sábado'],'dayNamesShort':['D','S','T','Q','Q','S','S'],'monthNames':['janeiro','fevereiro','março','abril','maio','junho','julho','agosto','setembro','outubro','novembro','dezembro',''],'monthNamesShort':['jan','fev','mar','abr','mai','jun','jul','ago','set','out','nov','dez',''],'timePattern':'HH:mm','datePattern':'dd/MM/yyyy','dateTimePattern':'dd/MM/yyyy HH:mm','timeFormat':'Clock24Hours','weekStarts':0}));
    DayPilot.Locale.register(new DayPilot.Locale('ru-ru', {'dayNames':['???????????','???????????','???????','?????','???????','???????','???????'],'dayNamesShort':['??','??','??','??','??','??','??'],'monthNames':['??????','???????','????','??????','???','????','????','??????','????????','???????','??????','???????',''],'monthNamesShort':['???','???','???','???','???','???','???','???','???','???','???','???',''],'timePattern':'H:mm','datePattern':'dd.MM.yyyy','dateTimePattern':'dd.MM.yyyy H:mm','timeFormat':'Clock24Hours','weekStarts':1}));
    DayPilot.Locale.register(new DayPilot.Locale('sk-sk', {'dayNames':['nede¾a','pondelok','utorok','streda','štvrtok','piatok','sobota'],'dayNamesShort':['ne','po','ut','st','št','pi','so'],'monthNames':['január','február','marec','apríl','máj','jún','júl','august','september','október','november','december',''],'monthNamesShort':['1','2','3','4','5','6','7','8','9','10','11','12',''],'timePattern':'H:mm','datePattern':'d.M.yyyy','dateTimePattern':'d.M.yyyy H:mm','timeFormat':'Clock24Hours','weekStarts':1}));
    DayPilot.Locale.register(new DayPilot.Locale('sv-se', {'dayNames':['söndag','mandag','tisdag','onsdag','torsdag','fredag','lördag'],'dayNamesShort':['sö','ma','ti','on','to','fr','lö'],'monthNames':['januari','februari','mars','april','maj','juni','juli','augusti','september','oktober','november','december',''],'monthNamesShort':['jan','feb','mar','apr','maj','jun','jul','aug','sep','okt','nov','dec',''],'timePattern':'HH:mm','datePattern':'yyyy-MM-dd','dateTimePattern':'yyyy-MM-dd HH:mm','timeFormat':'Clock24Hours','weekStarts':1}));
    DayPilot.Locale.register(new DayPilot.Locale('zh-cn', {'dayNames':['???','???','???','???','???','???','???'],'dayNamesShort':['?','?','?','?','?','?','?'],'monthNames':['??','??','??','??','??','??','??','??','??','??','???','???',''],'monthNamesShort':['1?','2?','3?','4?','5?','6?','7?','8?','9?','10?','11?','12?',''],'timePattern':'H:mm','datePattern':'yyyy/M/d','dateTimePattern':'yyyy/M/d H:mm','timeFormat':'Clock24Hours','weekStarts':1}));

    DayPilot.Locale.US = DayPilot.Locale.find("en-us");

    DayPilot.Switcher = function () {

        var This = this;

        this.views = [];
        this.switchers = [];
        this.navigator = {};
        
        this.selectedClass = null;

        this.active = null;

        this.day = new DayPilot.Date();

        this.navigator.updateMode = function (mode) {
            var control = This.navigator.control;
            if (!control) {
                return;
            }
            control.selectMode = mode;
            control.select(This.day);
        };

        this.addView = function (spec, options) {
            var element;
            if (typeof spec === 'string') {
                element = document.getElementById(spec);
                if (!element) {
                	throw "Element not found: " + spec;
                }
            }
            else {  // DayPilot object, DOM element
                element = spec;
            }
            
            var control = element;

            var view = {};
            view.isView = true;
            view.id = control.id;
            view.control = control;
            view.options = options || {};
            view.hide = function () {
                if (control.hide) {
                    control.hide();
                }
                else if (control.nav && control.nav.top) {
                    control.nav.top.style.display = 'none';
                }
                else {
                    control.style.display = 'none';
                }
            };
            view.sendNavigate = function(date) {
                var serverBased = (function() {
                    if (control.backendUrl) {  // ASP.NET MVC, Java
                        return true;
                    }
                    if (typeof WebForm_DoCallback === 'function' && control.uniqueID) {  // ASP.NET WebForms
                        return true;
                    }
                    return false;
                })();
                if (serverBased) {
                    //console.log("server based, sending navigate");
                    if (control.commandCallBack) {
                        control.commandCallBack("navigate", { "day": date });
                    }
                }
            };
            view.show = function () {
                This._hideViews();
                if (control.show) {
                    control.show();
                }
                else if (control.nav && control.nav.top) {
                    control.nav.top.style.display = '';
                }
                else {
                    control.style.display = '';
                }
            };
            view.selectMode = function () { // for navigator
                if (view.options.navigatorSelectMode) {
                    return view.options.navigatorSelectMode;
                }
                    
                if (control.isCalendar) {
                    switch (control.viewType) {
                        case "Day":
                            return "day";
                        case "Week":
                            return "week";
                        case "WorkWeek":
                            return "week";
                        default:
                            return "day";
                    }
                }
                else if (control.isMonth) {
                    switch (control.viewType) {
                        case "Month":
                            return "month";
                        case "Weeks":
                            return "week";
                        default:
                            return "day";
                    }
                }
                return "day";
            };

            this.views.push(view);
            
            return view;
        };

        this.addButton = function (id, control) {
            var element;
            if (typeof id === 'string') {
                element = document.getElementById(id);
                if (!element) {
                	throw "Element not found: " + id;
                }
            }
            else {
                element = id;
            }

            var view = this._findViewByControl(control);
            if (!view) {
                view = this.addView(control);
            }

            var switcher = {};
            switcher.isSwitcher = true;
            switcher.element = element;
            switcher.id = element.id;
            switcher.view = view;
            switcher.onClick = function (ev) {

                This.show(switcher);
                This._select(switcher);

                ev = ev || window.event;
                if (ev) {
                    ev.preventDefault ? ev.preventDefault() : ev.returnValue = false;
                }
                
            };

            DayPilot.re(element, 'click', switcher.onClick);
            
            this.switchers.push(switcher);
            
            return switcher;
        };

        this.select = function(id) {
            var switcher = this._findSwitcherById(id);
            if (switcher) {
                switcher.onClick();
            }
            else if (this.switchers.length > 0) {
                this.switchers[0].onClick();
            }
        };
        
        this._findSwitcherById = function(id) {
            for (var i = 0; i < this.switchers.length; i++) {
                var switcher = this.switchers[i];
                if (switcher.id === id) {
                    return switcher;
                }
            }
            return null;
        };
        
        this._select = function(switcher) {
            if (!this.selectedClass) {
                return;
            }
            
            for (var i = 0; i < this.switchers.length; i++) {
                var s = this.switchers[i];
                DayPilot.Util.removeClass(s.element, this.selectedClass);
            }
            DayPilot.Util.addClass(switcher.element, this.selectedClass);
        };

        this.addNavigator = function (control) {
            //this.navigator = {};
            This.navigator.control = control;

            control.timeRangeSelectedHandling = "JavaScript";
            control.onTimeRangeSelected = function() {
                var start, end, day;
                if (control.api === 1) {
                    start = arguments[0];
                    end = arguments[1];
                    day = arguments[2];
                }
                else {
                    var args = arguments[0];
                    start = args.start;
                    end = args.end;
                    day = args.day;
                }
                This.day = day;
                This.active.sendNavigate(This.day);
                if (This.onTimeRangeSelected) {
                    var args = {};
                    args.start = start;
                    args.end = end;
                    args.day = day;
                    args.target = This.active.control;
                    This.onTimeRangeSelected(args);
                }
                //This.active.control.commandCallBack("navigate", { "day": This.day });
            };
        };

        this.show = function (el) {
            var view, switcher;
            if (el.isSwitcher) {
                switcher = el;
                view = switcher.view;
            }
            else {
                view = el.isView ? el : this._findViewByControl(el);
                if (this.active === view) {
                    return;
                }
            }
            
            if (This.onSelect) {
                var args = {};
                //args.switcher = switcher;
                args.source = switcher ? switcher.element : null;
                args.target = view.control;
                
                This.onSelect(args);
                // TODO add preventDefault
            }
            
            this.active = view;
            view.show();

            var mode = view.selectMode();
            This.navigator.updateMode(mode);

            This.active.sendNavigate(this.day);
        };

        this._findViewByControl = function (control) {
            for (var i = 0; i < this.views.length; i++) {
                if (this.views[i].control === control) {
                    return this.views[i];
                }
            }
            return null;
        };

        this._hideViews = function () {
            //var controls = [dp_day, dp_week, dp_month];
            for (var i = 0; i < this.views.length; i++) {
                this.views[i].hide();
            }
        };
    };
    
    var Splitter = function(id) {
        var This = this;

        this.id = id;
        //this.count = 3;
        this.widths = [];
        this.titles = [];
        this.height = null;
        //this.height = 20;
        this.splitterWidth = 3;
        //this.color = "#000000";
        //this.opacity = 60;
        //this.padding = '0px 2px 0px 2px';
        this.css = {};
        this.css.title = null;
        this.css.titleInner = null;
        this.css.splitter = null;

        // internal
        this.blocks = [];
        this.drag = {};

        // callback
        this.updated = function() {};
        this.updating = function() {};

        this.init = function() {
            var div;

            if (!id) {
                throw "error: id not provided";
            }
            else if (typeof id === 'string') {
                div = document.getElementById(id);
            }
            else if (id.appendChild) {
                div = id;
            }
            else {
                throw "error: invalid object provided";
            }

            this.div = div;
            this.blocks = [];

            for (var i = 0; i < this.widths.length; i++) {
                var s = document.createElement("div");
                s.style.display = "inline-block";
                if (This.height !== null) {
                    s.style.height = This.height + "px";
                }
                else {
                    s.style.height = "100%";
                }
                s.style.width = (this.widths[i] - this.splitterWidth) + "px";
                s.style.display.overflow = 'hidden';
                s.style.verticalAlign = "top";
                s.style.position = "relative";
                s.setAttribute("unselectable", "on");
                s.className = this.css.title;
                div.appendChild(s);

                var inner = document.createElement("div");
                inner.innerHTML = this.titles[i];
                inner.setAttribute("unselectable", "on");
                inner.className = this.css.titleInner;
                s.appendChild(inner);
                
                var handle = document.createElement("div");
                handle.style.display = "inline-block";
                
                //handle.style.top = "0px";
                //handle.style.left = "0px";
                //handle.style.float = "left";
                //handle.style.height = this.height + "px";
                if (This.height !== null) {
                    handle.style.height = This.height + "px";
                }
                else {
                    handle.style.height = "100%";
                }
                handle.style.width = this.splitterWidth + "px";
                handle.style.position = "relative";

                handle.appendChild(document.createElement("div"));
                /*
                handle.style.backgroundColor = this.color;
                if (this.opacity >= 0 && this.opacity <= 100) {
                    handle.style.opacity = this.opacity / 100;
                    handle.style.filter = "alpha(opacity=" + this.opacity + ")";
                }*/
                handle.style.cursor = "col-resize";
                handle.setAttribute("unselectable", "on");
                handle.className = this.css.splitter;

                var data = {};
                data.index = i;
                data.width = this.widths[i];

                handle.data = data;

                handle.onmousedown = function(ev) {
                    This.drag.start = DayPilot.page(ev);
                    This.drag.data = this.data;
                    This.div.style.cursor = "col-resize";
                    //document.body.style.cursor = "col-resize";
                    ev = ev || window.event;
                    ev.preventDefault ? ev.preventDefault() : ev.returnValue = false;
                };

                div.appendChild(handle);

                var block = {};
                block.section = s;
                block.handle = handle;
                this.blocks.push(block);
            }

            this.registerGlobalHandlers();
        }; // Init

        // resets the initial value
        this.updateWidths = function() {
            for (var i = 0; i < this.blocks.length; i++) {
                var block = this.blocks[i];
                var width = this.widths[i];
                block.handle.data.width = width;

                this._updateWidth(i);
            }
        };

        this._updateWidth = function(i) {
            var block = this.blocks[i];
            var width = this.widths[i];
            block.section.style.width = (width - this.splitterWidth) + "px";
        };

        this.totalWidth = function() {
            var t = 0;
            for (var i = 0; i < this.widths.length; i++) {
                t += this.widths[i];
            }
            return t;
        };

        this.gMouseMove = function(ev) {
            if (!This.drag.start) {
                return;
            }

            var data = This.drag.data;

            var now = DayPilot.page(ev);
            var delta = now.x - This.drag.start.x;
            var i = data.index;

            This.widths[i] = data.width + delta;
            This._updateWidth(i);

            // callback
            var params = {};
            params.widths = this.widths;
            params.index = data.index;

            This.updating(params);
        };

        this.gMouseUp = function(ev) {
            if (!This.drag.start) {
                return;
            }
            This.drag.start = null;
            document.body.style.cursor = "";
            This.div.style.cursor = "";

            var data = This.drag.data;
            data.width = This.widths[data.index];

            // callback
            var params = {};
            params.widths = this.widths;
            params.index = data.index;

            This.updated(params);

        };

        this.registerGlobalHandlers = function() {
            DayPilot.re(document, 'mousemove', this.gMouseMove);
            DayPilot.re(document, 'mouseup', this.gMouseUp);
        };
    };

    DayPilot.Splitter = Splitter;    
    
})();

/* JSON */
// thanks to http://www.json.org/js.html


// declares DayPilot.JSON.stringify()
DayPilot.JSON = {};

(function() {
    function f(n) {
        return n < 10 ? '0' + n : n;
    }

    if (typeof Date.prototype.toJSON2 !== 'function') {

        Date.prototype.toJSON2 = function(key) {
            return this.getUTCFullYear() + '-' +
                    f(this.getUTCMonth() + 1) + '-' +
                    f(this.getUTCDate()) + 'T' +
                    f(this.getUTCHours()) + ':' +
                    f(this.getUTCMinutes()) + ':' +
                    f(this.getUTCSeconds()) + '';
        };

        String.prototype.toJSON =
                Number.prototype.toJSON =
                Boolean.prototype.toJSON = function(key) {
            return this.valueOf();
        };
    }

    var cx = /[\u0000\u00ad\u0600-\u0604\u070f\u17b4\u17b5\u200c-\u200f\u2028-\u202f\u2060-\u206f\ufeff\ufff0-\uffff]/g,
            escapeable = /[\\\"\x00-\x1f\x7f-\x9f\u00ad\u0600-\u0604\u070f\u17b4\u17b5\u200c-\u200f\u2028-\u202f\u2060-\u206f\ufeff\ufff0-\uffff]/g,
            gap,
            indent,
            meta = {
        '\b': '\\b',
        '\t': '\\t',
        '\n': '\\n',
        '\f': '\\f',
        '\r': '\\r',
        '"': '\\"',
        '\\': '\\\\'
    },
    rep;

    function quote(string) {
        escapeable.lastIndex = 0;
        return escapeable.test(string) ?
                '"' + string.replace(escapeable, function(a) {
            var c = meta[a];
            if (typeof c === 'string') {
                return c;
            }
            return '\\u' + ('0000' + a.charCodeAt(0).toString(16)).slice(-4);
        }) + '"' :
                '"' + string + '"';
    }

    function str(key, holder) {
        var i,
                k,
                v,
                length,
                mind = gap,
                partial,
                value = holder[key];
        if (value && typeof value === 'object' && typeof value.toJSON2 === 'function') {
            value = value.toJSON2(key);
        }
        else if (value && typeof value === 'object' && typeof value.toJSON === 'function' && !value.ignoreToJSON) {
            value = value.toJSON(key);
        }
        if (typeof rep === 'function') {
            value = rep.call(holder, key, value);
        }
        switch (typeof value) {
            case 'string':
                return quote(value);
            case 'number':
                return isFinite(value) ? String(value) : 'null';
            case 'boolean':
            case 'null':
                return String(value);
            case 'object':
                if (!value) {
                    return 'null';
                }
                gap += indent;
                partial = [];
                if (typeof value.length === 'number' &&
                        !value.propertyIsEnumerable('length')) {
                    length = value.length;
                    for (i = 0; i < length; i += 1) {
                        partial[i] = str(i, value) || 'null';
                    }
                    v = partial.length === 0 ? '[]' :
                            gap ? '[\n' + gap +
                            partial.join(',\n' + gap) + '\n' +
                            mind + ']' :
                            '[' + partial.join(',') + ']';
                    gap = mind;
                    return v;
                }
                if (rep && typeof rep === 'object') {
                    length = rep.length;
                    for (i = 0; i < length; i += 1) {
                        k = rep[i];
                        if (typeof k === 'string') {
                            v = str(k, value);
                            if (v) {
                                partial.push(quote(k) + (gap ? ': ' : ':') + v);
                            }
                        }
                    }
                } else {
                    for (k in value) {
                        if (Object.hasOwnProperty.call(value, k)) {
                            v = str(k, value);
                            if (v) {
                                partial.push(quote(k) + (gap ? ': ' : ':') + v);
                            }
                        }
                    }
                }
                v = (partial.length === 0) ? '{\u007D' :
                        gap ? '{\n' + gap + partial.join(',\n' + gap) + '\n' +
                        mind + '\u007D' : '{' + partial.join(',') + '\u007D';
                gap = mind;
                return v;
        }
    }

    if (typeof DayPilot.JSON.stringify !== 'function') {
        DayPilot.JSON.stringify = function(value, replacer, space) {
            var i;
            gap = '';
            indent = '';
            if (typeof space === 'number') {
                for (i = 0; i < space; i += 1) {
                    indent += ' ';
                }
            } else if (typeof space === 'string') {
                indent = space;
            }
            rep = replacer;
            if (replacer && typeof replacer !== 'function' && (typeof replacer !== 'object' || typeof replacer.length !== 'number')) {
                throw new Error('JSON.stringify');
            }
            return str('', {'': value});
        };
    }

    if (typeof Sys !== 'undefined' && Sys.Application && Sys.Application.notifyScriptLoaded) {
        Sys.Application.notifyScriptLoaded();
    }

})();
