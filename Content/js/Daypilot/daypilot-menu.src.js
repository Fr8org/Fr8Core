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
if (typeof DayPilotMenu === 'undefined') {
    var DayPilotMenu = DayPilot.MenuVisible = {};
}

(function() {

    if (typeof DayPilot.Menu !== 'undefined') {
        return;
    }

    (function registerDefaultTheme() {
        if (DayPilot.Global.defaultMenuCss) {
            return;
        }
        
        var sheet = DayPilot.sheet();
        
        sheet.add(".menu_default_main", "font-family: Tahoma, Arial, Sans-Serif;font-size: 12px;border: 1px solid #dddddd;background-color: white;padding: 0px;cursor: default;background-image: url(data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAB4AAAABCAIAAABG0om7AAAAKXRFWHRDcmVhdGlvbiBUaW1lAHBvIDEwIDUgMjAxMCAyMjozMzo1OSArMDEwMGzy7+IAAAAHdElNRQfaBQoUJAesj4VUAAAACXBIWXMAAA7DAAAOwwHHb6hkAAAABGdBTUEAALGPC/xhBQAAABVJREFUeNpj/P//PwO1weMnT2RlZAAYuwX/4oA3BgAAAABJRU5ErkJggg==);background-repeat: repeat-y;border-radius: 5px;-moz-box-shadow:0px 2px 3px rgba(000,000,000,0.3),inset 0px 0px 2px rgba(255,255,255,0.8);-webkit-box-shadow:0px 2px 3px rgba(000,000,000,0.3),inset 0px 0px 2px rgba(255,255,255,0.8);box-shadow:0px 2px 3px rgba(000,000,000,0.3),inset 0px 0px 2px rgba(255,255,255,0.8);");
        sheet.add(".menu_default_title", "background-color: #f2f2f2;border-bottom: 1px solid gray;padding: 4px 4px 4px 37px;");
        sheet.add(".menu_default_main a", "padding: 2px 2px 2px 35px;color: black;text-decoration: none;cursor: default;");
        sheet.add(".menu_default_main a img", "margin-left: 6px;margin-top: 2px;");
        sheet.add(".menu_default_main a span", "display: block;height: 20px;line-height: 20px;width: 150px; overflow:hidden;padding-left: 2px;padding-right: 20px;");
        sheet.add(".menu_default_main a:hover", 'background: #eeeeee;background: -webkit-gradient(linear, left top, left bottom, from(#efefef), to(#e6e6e6));background: -webkit-linear-gradient(top, #efefef 0%, #e6e6e6);background: -moz-linear-gradient(top, #efefef 0%, #e6e6e6);background: -ms-linear-gradient(top, #efefef 0%, #e6e6e6);background: -o-linear-gradient(top, #efefef 0%, #e6e6e6);background: linear-gradient(top, #efefef 0%, #e6e6e6);filter: progid:DXImageTransform.Microsoft.Gradient(startColorStr="#efefef", endColorStr="#e6e6e6");');
        sheet.add(".menu_default_main div div", "border-top: 1px solid #dddddd;margin-top: 2px;margin-bottom: 2px;margin-left: 28px;");
        sheet.commit();
        
        DayPilot.Global.defaultMenuCss = true;
    })();

    var DayPilotMenu = {};

    DayPilotMenu.mouse = null;
    DayPilotMenu.menu = null;
    DayPilotMenu.clickRegistered = false;

    DayPilot.Menu = function(items) {
        var menu = this;
        
        this.v = '800';
        this.zIndex = 10;  // more than 10,001 used by ModalPopupExtender
        this.useShadow = true;
        this.cssClassPrefix = "menu_default";
        this.cssOnly = true;
        this.menuTitle = null;
        this.showMenuTitle = false;
        this.ref = null; // ref object, used for position

        if (items && DayPilot.isArray(items)) {
            this.items = items;
        }

        this.show = function(e, submenu) {
            var value = null;
            if (typeof e.id === 'string' || typeof e.id === 'number') {
                value = e.id;
            }
            else if (typeof e.id === 'function') {
                value = e.id();
            }
            else if (typeof e.value === 'function') {
                value = e.value();
            }

            if (typeof(DayPilot.Bubble) !== 'undefined') { // hide any bubble if active
                DayPilot.Bubble.hideActive();
            }

            if (!submenu) {
                DayPilotMenu.menuClean();
            }
            
            // clear old data
            this.submenu = null;

            if (DayPilotMenu.mouse === null) // not possible to execute before mouse move (TODO)
                return;

            var div = document.createElement("div");
            div.style.position = "absolute";
            //div.style.top = coords.y + "px";
            //div.style.left = coords.x + "px";
            div.style.top = "0px";
            div.style.left = "0px";
            div.style.display = 'none';
            div.style.overflow = 'hidden';
            div.style.zIndex = this.zIndex + 1;
            div.className = this.applyCssClass('main');
            div.onclick = function() {
                this.parentNode.removeChild(this);
            };

            if (!this.items || this.items.length === 0) {
                throw "No menu items defined.";
            }

            if (this.showMenuTitle) {
                var title = document.createElement("div");
                title.innerHTML = this.menuTitle;
                title.className = this.applyCssClass("title");
                div.appendChild(title);
            }

            for (var i = 0; i < this.items.length; i++) {
                var mi = this.items[i];
                var item = document.createElement("div");
                //item.style.position = 'relative';

                if (typeof mi === 'undefined') {
                    continue;
                }
                if (mi.text === '-') {
                    var separator = document.createElement("div");
                    item.appendChild(separator);
                }
                else {
                    var link = document.createElement("a");
                    link.style.position = 'relative';
                    link.style.display = "block";
                    if (mi.href) {
                        link.href = mi.href.replace(/\x7B0\x7D/gim, value); // for NavigateUrl actions;
                        if (mi.target) {
                            link.setAttribute("target", mi.target);
                        }
                    }
                    else if (mi.onclick) {
                        link.item = mi;
                        link.onclick = mi.onclick;
                    }
                    else if (mi.command) {
                        var assign = function(mi, link) {
                            return function() {
                                var source = link.source;
                                var item = mi;
                                item.action = item.action ? item.action : 'CallBack';
                                var cal = source.calendar || source.root;
                                switch (source.type) {
                                    case 'resource':
                                        cal.internal.resourceHeaderMenuClick(item.command, source, item.action);
                                        return;
                                    case 'selection':
                                        cal.internal.timeRangeMenuClick(item.command, source, item.action);
                                        return;
                                    default:
                                        cal.internal.eventMenuClick(item.command, source, item.action);
                                        return;
                                }
                            };
                        };
                        link.onclick = assign(mi, link);
                    }
                    link.source = e;

                    var span = document.createElement("span");
                    span.innerHTML = mi.text;
                    link.appendChild(span);

                    if (mi.image) {
                        var image = document.createElement("img");
                        image.src = mi.image;
                        image.style.position = 'absolute';
                        image.style.top = '0px';
                        image.style.left = '0px';

                        link.appendChild(image);
                    }
                    
                    var assignOnMouseOver = function(mi, link) {
                        return function() {
                            var source = link.source;
                            var item = mi;
                            
                            // some delay
                            setTimeout(function() {
                                if (menu.submenu && menu.submenu.item === item) {  // already visible
                                    return;
                                }
                                
                                if (menu.submenu && menu.submenu.item !== item) {  // hide other
                                    menu.submenu.menu.hide();
                                    menu.submenu = null;
                                }

                                if (!item.items) {  // no submenu for this item
                                    return;
                                }
                                
                                var options = menu.cloneOptions();
                                options.items = item.items;
                                
                                menu.submenu = {};
                                menu.submenu.menu = new DayPilot.Menu(options);
                                menu.submenu.menu.show(source, true);
                                menu.submenu.item = item;
                            }, 500);
                        };
                    };
                        
                    //if (mi.items) {
                        link.onmouseover = assignOnMouseOver(mi, link);
                    //}

                    item.appendChild(link);
                }

                div.appendChild(item);

            }

            div.onclick = function(e) {
                window.setTimeout(function() {
                    DayPilotMenu.menuClean();
                }, 100);
            };
            div.onmousedown = function(e) {
                if (!e)
                    var e = window.event;
                e.cancelBubble = true;
                if (e.stopPropagation)
                    e.stopPropagation();
            };
            div.oncontextmenu = function() {
                return false;
            };

            document.body.appendChild(div);

            div.style.display = '';
            var height = div.clientHeight;
            var width = div.offsetWidth;
            div.style.display = 'none';

            // don't show the menu outside of the visible window
            var windowHeight = document.documentElement.clientHeight;
            //        windowHeight = document.body.clientHeight;

            if (!this.ref) {
                // don't show it exactly under the cursor
                var x = DayPilotMenu.mouse.x + 1;
                var y = DayPilotMenu.mouse.y + 1;

                if (DayPilotMenu.mouse.clientY > windowHeight - height && windowHeight !== 0) {
                    var offsetY = DayPilotMenu.mouse.clientY - (windowHeight - height) + 5;
                    div.style.top = (y - offsetY) + 'px';
                }
                else {
                    div.style.top = y + 'px';
                }

                var windowWidth = document.documentElement.clientWidth;

                if (DayPilotMenu.mouse.clientX > windowWidth - width && windowWidth !== 0) {
                    var offsetX = DayPilotMenu.mouse.clientX - (windowWidth - width) + 5;
                    div.style.left = (x - offsetX) + 'px';
                }
                else {
                    div.style.left = x + 'px';
                }
            }
            else {
                var pos = DayPilot.abs(this.ref);
                var height = this.ref.offsetHeight;
                
                div.style.left = pos.x + "px";
                div.style.top = (pos.y + height) + "px";
                
            }
            div.style.display = '';

            this.addShadow(div);
            this.div = div;

            if (!submenu) {
                DayPilot.Menu.active = this;
            }

            return;

        };

        this.applyCssClass = function(part) {
            var prefix = this.theme || this.cssClassPrefix;
            var sep = (this.cssOnly ? "_" : "");
            if (prefix) {
                return prefix + sep + part;
            }
            else {
                return "";
            }
        };
        
        this.cloneOptions = function() {
            var options = {};
            var properties = ['cssOnly', 'cssClassPrefix', 'useShadow', 'zIndex'];
            
            for(var i = 0; i < properties.length; i++) {
                var p = properties[i];
                options[p] = this[p];
            }
            
            return options;
        };

        this.hide = function() {
            if (this.submenu) {
                this.submenu.menu.hide();
            }
            
            this.removeShadow();
            if (this.div && this.div.parentNode === document.body) {
                document.body.removeChild(this.div);
            }
        };
        
        // detects the mouse position, use when creating menu right before opening (.show)
        this.init = function(ev) {
            DayPilotMenu.mouseMove(ev);
        };
        
        this.addShadow = function(object) {
            if (!this.useShadow || this.cssOnly) {  // shadow is disabled
                return;
            }
            if (!object) {
                return;
            }
            if (this.shadows && this.shadows.length > 0) {
                this.removeShadow();
            }
            this.shadows = [];

            for (var i = 0; i < 5; i++) {
                var shadow = document.createElement('div');

                shadow.style.position = 'absolute';
                shadow.style.width = object.offsetWidth + 'px';
                shadow.style.height = object.offsetHeight + 'px';

                shadow.style.top = object.offsetTop + i + 'px';
                shadow.style.left = object.offsetLeft + i + 'px';
                shadow.style.zIndex = this.zIndex;

                shadow.style.filter = 'alpha(opacity:10)';
                shadow.style.opacity = 0.1;
                shadow.style.backgroundColor = '#000000';

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

        var options = DayPilot.isArray(items) ? null : items;
        if (options) {
            for (var name in options) {
                this[name] = options[name];
            }
        }

        DayPilot.re(document.body, 'mousemove', DayPilotMenu.mouseMove);
        if (!DayPilotMenu.clickRegistered) {
            DayPilot.re(document, 'mousedown', DayPilotMenu.menuClean);
            DayPilotMenu.clickRegistered = true;
        }
    };

    DayPilotMenu.menuClean = function(ev) {
        if (typeof(DayPilot.Menu.active) === 'undefined')
            return;

        if (DayPilot.Menu.active) {
            DayPilot.Menu.active.hide();
            DayPilot.Menu.active = null;
        }

    };
    
    DayPilotMenu.mouseMove = function(ev) {
        if (typeof(DayPilotMenu) === 'undefined')
            return;
        DayPilotMenu.mouse = DayPilotMenu.mousePosition(ev);
    };
    
    DayPilotMenu.mousePosition = function(e) {
        var posx = 0;
        var posy = 0;
        if (!e)
            var e = window.event;
        if (e.pageX || e.pageY) {
            posx = e.pageX;
            posy = e.pageY;
        }
        else if (e.clientX || e.clientY) {
            posx = e.clientX + document.body.scrollLeft + document.documentElement.scrollLeft;
            posy = e.clientY + document.body.scrollTop + document.documentElement.scrollTop;
        }

        var result = {};
        result.x = posx;
        result.y = posy;
        result.clientY = e.clientY;
        result.clientX = e.clientX;
        return result;
    };

    // publish the API 

    // (backwards compatibility)    
    //DayPilot.MonthVisible.dragStart = DayPilotMonth.dragStart;
    DayPilot.MenuVisible.Menu = DayPilotMenu.Menu;

    // current
    //DayPilot.Menu = DayPilotMenu.Menu;

    if (typeof Sys !== 'undefined' && Sys.Application && Sys.Application.notifyScriptLoaded) {
        Sys.Application.notifyScriptLoaded();
    }


})();
