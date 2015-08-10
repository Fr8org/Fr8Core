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

// compatibility with 5.9.2029 and previous
if (typeof DayPilotBubble === 'undefined') {
	var DayPilotBubble = DayPilot.BubbleVisible = {};
}

(function() {

    if (typeof DayPilot.Bubble !== 'undefined') {
        return;
    }

    // register the default theme
    (function() {
        if (DayPilot.Global.defaultBubbleCss) {
            return;
        }
        
        var sheet = DayPilot.sheet();
        
        sheet.add(".bubble_default_main", "cursor: default;");
        sheet.add(".bubble_default_main_inner", 'border-radius: 5px;font-size: 12px;padding: 4px;color: #666;background: #eeeeee; background: -webkit-gradient(linear, left top, left bottom, from(#ffffff), to(#eeeeee));background: -webkit-linear-gradient(top, #ffffff 0%, #eeeeee);background: -moz-linear-gradient(top, #ffffff 0%, #eeeeee);background: -ms-linear-gradient(top, #ffffff 0%, #eeeeee);background: -o-linear-gradient(top, #ffffff 0%, #eeeeee);background: linear-gradient(top, #ffffff 0%, #eeeeee);filter: progid:DXImageTransform.Microsoft.Gradient(startColorStr="#ffffff", endColorStr="#eeeeee");border: 1px solid #ccc;-moz-border-radius: 5px;-webkit-border-radius: 5px;border-radius: 5px;-moz-box-shadow:0px 2px 3px rgba(000,000,000,0.3),inset 0px 0px 2px rgba(255,255,255,0.8);-webkit-box-shadow:0px 2px 3px rgba(000,000,000,0.3),inset 0px 0px 2px rgba(255,255,255,0.8);box-shadow:0px 2px 3px rgba(000,000,000,0.3),inset 0px 0px 2px rgba(255,255,255,0.8);');
        sheet.commit();
        
        DayPilot.Global.defaultBubbleCss = true;
    })();

    var DayPilotBubble = {};

    DayPilotBubble.mouseMove = function(ev) {
        if (typeof(DayPilotBubble) === 'undefined') {
            return;
        }
        DayPilotBubble.mouse = DayPilotBubble.mousePosition(ev);
        
        var b = DayPilotBubble.active;
        if (b && b.showPosition) {
            var pos1 = b.showPosition;
            var pos2 = DayPilotBubble.mouse;
            if (pos1.clientX !== pos2.clientX || pos1.clientY !== pos2.clientY) {
                //b.delayedHide();
            }
        }
        
    };

    DayPilotBubble.mousePosition = function(e) {
        var result = DayPilot.page(e);
        result.clientY = e.clientY;
        result.clientX = e.clientX;
        return result;
    };

    DayPilot.Bubble = function(options) {
        this.v = '800';

        var bubble = this;
        //this.object = document.getElementById(id);
        
        // default property values
        this.backgroundColor = "#ffffff";
        this.border = "1px solid #000000";
        this.corners = 'Rounded';
        this.cssOnly = true;
        this.hideAfter = 500;
        this.loadingText = "Loading...";
        this.animated = true;
        this.animation = "fast";
        this.position = "EventTop";
        this.showAfter = 500;
        this.showLoadingLabel = true;
        this.useShadow = true;
        this.zIndex = 10;
        this.cssClassPrefix = "bubble_default";
        
        this.elements = {};
        
        this.callBack = function(args) {
            if (this.aspnet()) {
                WebForm_DoCallback(this.uniqueID, DayPilot.JSON.stringify(args), this.updateView, this, this.callbackError, true);        
            }
            else {
                if (args.calendar.internal.bubbleCallBack) {
                    args.calendar.internal.bubbleCallBack(args, this);
                }
                else {
                    args.calendar.bubbleCallBack(args, this);
                }
            }
        };
        
        this.callbackError = function (result, context) { 
            alert(result); 
        };

        this.updateView = function(result, context) {
            // context should equal to bubble
            if (bubble !== context) {
                throw "Callback object mismatch (internal error)";
            }
            DayPilotBubble.active = bubble;
            if (bubble) {
                if (bubble.elements.inner) {
                    bubble.elements.inner.innerHTML = result;
                }
                bubble.adjustPosition();
                if (!bubble.animated) {
                    bubble.addShadow();
                }
            } 
        };
        
        this.init = function() {
            DayPilot.re(document.body, 'mousemove', DayPilotBubble.mouseMove);
        };
        
        this.aspnet = function() {
            return (typeof WebForm_DoCallback !== 'undefined');
        };
        
        this.rounded = function() {
            return this.corners === 'Rounded';
        };
        
        this.showEvent = function(e, now) {
            //document.title = "e.root:" + e.root;
            var a = new DayPilotBubble.CallBackArgs(e.calendar || e.root, 'Event', e, e.bubbleHtml ? e.bubbleHtml() : null);
            if (now) {
                this.show(a);
            }
            else {
                this.showOnMouseOver(a);
            }
        };
        
        this.showCell = function(cell) {
            var a = new DayPilotBubble.CallBackArgs(cell.calendar || cell.root, 'Cell', cell, cell.staticBubbleHTML ? cell.staticBubbleHTML() : null);
            this.showOnMouseOver(a);
        };
        
        this.showTime = function(time) {
            var a = new DayPilotBubble.CallBackArgs(time.calendar || time.root, 'Time', time, time.staticBubbleHTML ? time.staticBubbleHTML() : null);
            this.showOnMouseOver(a);
        };
        
        this.showResource = function(res) {
            var a = new DayPilotBubble.CallBackArgs(res.calendar || res.root, 'Resource', res, res.staticBubbleHTML ? res.staticBubbleHTML() : null);
            this.showOnMouseOver(a);
        };
        
        this.showHtml = function(html, div) {
            var a = new DayPilotBubble.CallBackArgs(null, 'Html', null, html);
            a.div = div;
            this.show(a);
        };
        
        this.show = function(callbackArgument) {
            var pop = this.animated;
            
            this.showPosition = DayPilotBubble.mouse;

            var id;
            try {           
                id = DayPilot.JSON.stringify(callbackArgument.object);
            }
            catch (e) {
                return; // unable to serialize, it's an invalid event (might have been cleared already)
            }
        
            if (DayPilotBubble.active === this && this.sourceId === id) { // don't show, it's already visible
                return;
            }    
            if (typeof DayPilot.Menu !== 'undefined' && DayPilot.Menu.active) { // don't show the bubble if a menu is active
                return;
            }
            
            // hide whatever might be visible (we are going to show another one)
            DayPilotBubble.hideActive();
                    
            DayPilotBubble.active = this;
            this.sourceId = id;

            var div = document.createElement("div");
            div.setAttribute("unselectable", "on");
            
            //if (!this.showLoadingLabel && !pop) {
            if (!this.showLoadingLabel) {
                div.style.display = 'none';
            }
            
            document.body.appendChild(div);

            div.style.position = 'absolute';
            if (!this.cssOnly) {
                if (this.width) {
                    div.style.width = this.width;
                }
                
                div.style.cursor = 'default';
            }
            else {
                div.className = this._prefixCssClass("_main");
            }
            
            div.style.top = '0px';
            div.style.left = '0px';
            div.style.zIndex = this.zIndex + 1;  
            
            if (pop) {
                div.style.visibility = 'hidden';
            }
            
            div.onclick = function() {
                DayPilotBubble.hideActive();
            };
            
            div.onmousemove = function(e) {
                DayPilotBubble.cancelTimeout();
                var e = e || window.event;
                e.cancelBubble = true;
            };
            div.oncontextmenu = function() { return false; };
            div.onmouseout = this.delayedHide;

            var inner = document.createElement("div");
            div.appendChild(inner);

            if (this.cssOnly) {
                inner.className = this._prefixCssClass("_main_inner");
            }
            else {
                inner.style.padding = '4px';
                if (this.border) {
                    inner.style.border = this.border;
                }
                if (this.rounded()) {
                    inner.style.MozBorderRadius = "5px";
                    inner.style.webkitBorderRadius = "5px";
                    inner.style.borderRadius = "5px";
                }
                inner.style.backgroundColor = this.backgroundColor;
            }

            inner.innerHTML = this.loadingText;
            
            this.elements.div = div;
            this.elements.inner = inner;
            
            var div = this.getDiv(callbackArgument);
            
            
            if (this.position === "EventTop" && div) {
                var margin = 2;
                /*
                var event = callbackArgument.object;
                var calendar = callbackArgument.calendar;
                //var div = calendar.internal.findEventDiv(event);
                if (!div) {
                    return;
                }*/
                var abs = DayPilot.abs(div, true);
                
                this.mouse = DayPilotBubble.mouse;
                this.mouse.x = abs.x;
                this.mouse.y = abs.y;
                this.mouse.h = abs.h + margin;
                this.mouse.w = abs.w;
            }
            else {
                // fix the position to the original location (don't move it in adjustPosition after callback)
                this.mouse = DayPilotBubble.mouse;
            }
            
            if (this.showLoadingLabel && !pop) {
                this.adjustPosition();
                this.addShadow();
            }
            
            if (callbackArgument.staticHTML) {
                this.updateView(callbackArgument.staticHTML, this);
            }
            else if (typeof this.onLoad === 'function') {
                var args = {};
                args.source = callbackArgument.object;
                args.async = false;
                args.loaded = function() {
                    // make sure it's marked as async
                    if (this.async) {
                        bubble.updateView(args.html, bubble);
                    }
                };
                this.onLoad(args);
                
                // not async, show now
                if (!args.async) {
                    bubble.updateView(args.html, bubble);
                }
            }
            else if (this._serverBased(callbackArgument)) {
                this.callBack(callbackArgument);
            }
        };
        
        this.getDiv = function(callbackArgument) {
            if (callbackArgument.div) {
                return callbackArgument.div;
            }
            if (callbackArgument.type === 'Event' && callbackArgument.calendar && callbackArgument.calendar.internal.findEventDiv) {
                return callbackArgument.calendar.internal.findEventDiv(callbackArgument.object);
            }
            
        };
                
        this._prefixCssClass = function(part) {
            var prefix = this.theme || this.cssClassPrefix;
            if (prefix) {
                return prefix + part;
            }
            else {
                return "";
            }
        };
        
        this.loadingElement = null;
        
        this.loadingStart = function(abs) {
        
        };
        
        this.loadingStop = function() {
        
        };
        
        this.adjustPosition = function() {
            var pop = this.animated;
            
            var position = this.position;
            
            var windowPadding = 10; // used for both horizontal and vertical padding if the bubble
        
            if (!this.elements.div) {
                return;
            }
            
            if (!this.mouse) {  // don't ajdust the position
                return;
            }
            
            // invalid coordinates
            if (!this.mouse.x || !this.mouse.y) {
                DayPilotBubble.hideActive();   
                return;         
            }

            var div = this.elements.div;
            
            div.style.display = '';
            var height = div.offsetHeight;
            var width = div.offsetWidth;
            div.style.display = 'none';
            
            var wd = DayPilot.wd();

            var windowWidth = wd.width;
            var windowHeight = wd.height;
            
            if (position === 'Mouse') {
                var pixelsBelowCursor = 22;
                var pixelsAboveCursor = 10;

                var top = 0;
                if (this.mouse.clientY > windowHeight - height + windowPadding) {
                    var offsetY = this.mouse.clientY - (windowHeight - height) + windowPadding;
                    top = (this.mouse.y - height - pixelsAboveCursor);
                }
                else {
                    top = this.mouse.y + pixelsBelowCursor;
                }
                
                if (typeof top === 'number') {
                    div.style.top = Math.max(top, 0) + "px";
                }
                
                if (this.mouse.clientX > windowWidth - width + windowPadding) {
                    var offsetX = this.mouse.clientX - (windowWidth - width) + windowPadding;
                    div.style.left = (this.mouse.x - offsetX) + 'px';
                }
                else {
                    div.style.left = this.mouse.x + 'px';
                }
            }
            else if (position === 'EventTop') {
                var space = 2;
                
                // 1 try to show it above the event
                var top = this.mouse.y - height - space;
                var scrollTop = wd.scrollTop;
                
                // 2 doesn't fit there, try to show it below the event
                if (top < scrollTop) {
                    top = this.mouse.y + this.mouse.h + space;
                }
                
                if (typeof top === 'number') {
                    div.style.top = Math.max(top, 0) + 'px';
                }
                
                var left = this.mouse.x;

                // does it have any effect here? gets updated later                
                if (this.mouse.x + width + windowPadding > windowWidth) {
                    //var offsetX = this.mouse.x - (windowWidth - width) + windowPadding;
                    //left = this.mouse.x - offsetX;
                    left = windowWidth - width - windowPadding;
                }
                
                div.style.left = left + 'px';
                
            }
            
            div.style.display = '';

            if (pop) {
                div.style.display = '';
                
                var original = {};
                original.color = div.firstChild.style.color;
                original.overflow = div.style.overflow;
                
                div.firstChild.style.color = "transparent";
                div.style.overflow = 'hidden';
                
                this.removeShadow();
                
                DayPilot.pop(div, {
                    "finished": function() {
                        div.firstChild.style.color = original.color;
                        div.style.overflow = original.overflow;
                        bubble.addShadow();
                    },
                    "vertical": "bottom",
                    "horizontal": "left",
                    "animation" : bubble.animation
                });
            }
        
        };
        
        this.delayedHide = function() {
            DayPilotBubble.cancelTimeout();
            if (bubble.hideAfter > 0) {
                DayPilotBubble.timeout = window.setTimeout(DayPilotBubble.hideActive, bubble.hideAfter);
                //window.setTimeout("DayPilotBubble.hideActive()", bubble.hideAfter);
            }
        }; 

        this.showOnMouseOver = function (callbackArgument) {
            DayPilotBubble.cancelTimeout();
            
            var delayedShow = function(arg) {
                return function() {
                    bubble.show(arg);
                };
            };

            DayPilotBubble.timeout = window.setTimeout(delayedShow(callbackArgument), this.showAfter);
            //DayPilotBubble.timeout = window.setTimeout(this.clientObjectName + ".show('" + callbackArgument + "')", this.showAfter);
        };

        this.hideOnMouseOut = function() {
            this.delayedHide();
        };
        
        this._serverBased = function(args) {
            if (args.calendar.backendUrl) {  // ASP.NET MVC, Java
                return true;
            }
            if (typeof WebForm_DoCallback === 'function' && this.uniqueID) {  // ASP.NET WebForms
                return true;
            }
            return false;
        };

        
        this.addShadow = function() {
            if (!this.useShadow) {  // shadow is disabled
                return;
            }
            if (this.cssOnly) {
                return;
            }
            if (!this.elements.div) {
                return;
            }
            var div = this.elements.div;
            if (this.shadows && this.shadows.length > 0) {
                this.removeShadow();
            }
            this.shadows = [];
            
            for (var i = 0; i < 5; i++) {
                var shadow = document.createElement('div');
                shadow.setAttribute("unselectable", "on");
                
                shadow.style.position = 'absolute';
                shadow.style.width = div.offsetWidth + 'px';
                shadow.style.height = div.offsetHeight + 'px';
                
                shadow.style.top = div.offsetTop + i + 'px';
                shadow.style.left = div.offsetLeft + i + 'px';
                shadow.style.zIndex = this.zIndex;
                
                shadow.style.filter = 'alpha(opacity:10)';
                shadow.style.opacity = 0.1;
                shadow.style.backgroundColor = '#000000';
                
                if (this.rounded()) {
                    shadow.style.MozBorderRadius = "5px";
                    shadow.style.webkitBorderRadius = "5px";
                    shadow.style.borderRadius = "5px";
                }

                document.body.appendChild(shadow);
                this.shadows.push(shadow);
            }
        };
        
        this.removeShadow = function() {
            if (!this.shadows) {
                return;
            }

            for (var i = 0; i < this.shadows.length; i++) {
                document.body.removeChild(this.shadows[i]);
            }
            this.shadows = [];
        };   
        
        this.removeDiv = function() {
            if (!this.elements.div) {
                return;
            }
            document.body.removeChild(this.elements.div);
            this.elements.div = null;
            
        }; 
        
        if (options) {
            for (var name in options) {
                this[name] = options[name];
            }
        }
        
        this.init();
        
    };


    DayPilotBubble.cancelTimeout = function() {
        if (DayPilotBubble.timeout) {
            window.clearTimeout(DayPilotBubble.timeout);
        }
    };

    DayPilotBubble.hideActive = function() {
        DayPilotBubble.cancelTimeout();
        var bubble = DayPilotBubble.active;
        if (bubble) {
            //bubble.object.style.display = 'none';
            bubble.removeDiv();
            bubble.removeShadow();
        }
        DayPilotBubble.active = null;
    };
    
    DayPilotBubble.CallBackArgs = function(calendar, type, object, staticHTML) {
        this.calendar = calendar;
        this.type = type;
        this.object = object;
        this.staticHTML = staticHTML;
        
        this.toJSON = function() {
            var json = {};
            json.uid = this.calendar.uniqueID;
            //json.v = this.calendar.v;
            json.type = this.type;
            json.object = object;
            //json.staticHTML = staticHTML;
            return json;
        };
    };

    // publish the API 
    
    // (backwards compatibility)    
    DayPilot.BubbleVisible.Bubble = DayPilotBubble.Bubble;
    DayPilot.BubbleVisible.hideActive = DayPilotBubble.hideActive;
    DayPilot.BubbleVisible.cancelTimeout = DayPilotBubble.cancelTimeout;
    
    // current
    DayPilot.Bubble.hideActive = DayPilotBubble.hideActive;

    if (typeof Sys !== 'undefined' && Sys.Application && Sys.Application.notifyScriptLoaded){
       Sys.Application.notifyScriptLoaded();
    }
     
})();