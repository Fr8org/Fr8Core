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
if (typeof DayPilotCalendar === 'undefined') {
    var DayPilotCalendar = DayPilot.CalendarVisible = {};
}

(function() {

    if (typeof DayPilot.Calendar !== 'undefined') {
        return;
    }
    
    // register the default theme
    (function() {
        if (DayPilot.Global.defaultCalendarCss) {
            return;
        }
        
        var sheet = DayPilot.sheet();
        
        sheet.add(".calendar_default_main", "border: 1px solid #999;font-family: Tahoma, Arial, sans-serif; font-size: 12px;");
        sheet.add(".calendar_default_rowheader_inner,.calendar_default_cornerright_inner,.calendar_default_corner_inner,.calendar_default_colheader_inner,.calendar_default_alldayheader_inner", "color: #666;background: #eee;");
        sheet.add(".calendar_default_cornerright_inner", "position: absolute;top: 0px;left: 0px;bottom: 0px;right: 0px;	border-bottom: 1px solid #999;");
        sheet.add(".calendar_default_rowheader_inner", "font-size: 16pt;text-align: right; position: absolute;top: 0px;left: 0px;bottom: 0px;right: 0px;border-right: 1px solid #999;border-bottom: 1px solid #999;");
        sheet.add(".calendar_default_corner_inner", "position: absolute;top: 0px;left: 0px;bottom: 0px;right: 0px;border-right: 1px solid #999;border-bottom: 1px solid #999;");
        sheet.add(".calendar_default_rowheader_minutes", "font-size:10px;vertical-align: super;padding-left: 2px;padding-right: 2px;");
        sheet.add(".calendar_default_colheader_inner", "text-align: center; position: absolute;top: 0px;left: 0px;bottom: 0px;right: 0px;border-right: 1px solid #999;border-bottom: 1px solid #999;");
        sheet.add(".calendar_default_cell_inner", "position: absolute;top: 0px;left: 0px;bottom: 0px;right: 0px;border-right: 1px solid #ddd;border-bottom: 1px solid #ddd; background: #f9f9f9;");
        sheet.add(".calendar_default_cell_business .calendar_default_cell_inner", "background: #fff");
        sheet.add(".calendar_default_alldayheader_inner", "text-align: center;position: absolute;top: 0px;left: 0px;bottom: 0px;right: 0px;border-right: 1px solid #999;border-bottom: 1px solid #999;");
        sheet.add(".calendar_default_message", "opacity: 0.9;filter: alpha(opacity=90);	padding: 10px; color: #ffffff;background: #ffa216;");
        sheet.add(".calendar_default_alldayevent_inner,.calendar_default_event_inner", 'color: #666; border: 1px solid #999;'); // border-top: 4px solid #1066a8;
        sheet.add(".calendar_default_event_bar", "top: 0px;bottom: 0px;left: 0px;width: 4px;background-color: #9dc8e8;");
        sheet.add(".calendar_default_event_bar_inner", "position: absolute;width: 4px;background-color: #1066a8;");
        sheet.add(".calendar_default_alldayevent_inner,.calendar_default_event_inner", 'background: #fff;background: -webkit-gradient(linear, left top, left bottom, from(#ffffff), to(#eeeeee));background: -webkit-linear-gradient(top, #ffffff 0%, #eeeeee);background: -moz-linear-gradient(top, #ffffff 0%, #eeeeee);background: -ms-linear-gradient(top, #ffffff 0%, #eeeeee);background: -o-linear-gradient(top, #ffffff 0%, #eeeeee);background: linear-gradient(top, #ffffff 0%, #eeeeee);filter: progid:DXImageTransform.Microsoft.Gradient(startColorStr="#ffffff", endColorStr="#eeeeee");');
        sheet.add(".calendar_default_selected .calendar_default_event_inner", "background: #ddd;");
        sheet.add(".calendar_default_alldayevent_inner", "position: absolute;top: 2px;bottom: 2px;left: 2px;right: 2px;padding: 2px;margin-right: 1px;font-size: 12px;");
        sheet.add(".calendar_default_event_withheader .calendar_default_event_inner", "padding-top: 15px;");
        sheet.add(".calendar_default_event", "cursor: default;");
        sheet.add(".calendar_default_event_inner", "position: absolute;overflow: hidden;top: 0px;bottom: 0px;left: 0px;right: 0px;padding: 2px 2px 2px 6px;font-size: 12px;");
        sheet.add(".calendar_default_shadow_inner", "background-color: #666666;	opacity: 0.5;filter: alpha(opacity=50);height: 100%;-moz-border-radius: 5px;-webkit-border-radius: 5px;border-radius: 5px;");
        sheet.commit();
        
        // trying to define event height using css
        //sheet.add(".calendar_default_header_height", "height:50px");
        //sheet.add(".calendar_default_alldayevent_height", "height:50px");
        
        DayPilot.Global.defaultCalendarCss = true;
    })();

    DayPilot.Calendar = function(id) {
        this.v = '800';

        var isConstructor = false;
        if (this instanceof DayPilot.Calendar && !this.__constructor) {
            isConstructor = true;
            this.__constructor = true;
        }

        if (!isConstructor) {
            throw "DayPilot.Calendar() is a constructor and must be called as 'var c = new DayPilot.Calendar(id);'";
        }

        var calendar = this;
        this.uniqueID = null;

        this.id = id;
        this.isCalendar = true;
        this.api = 2;

        this.clientName = id;
        this.clientState = {};

        this._cache = {};
        this._cache.pixels = {};
        this._cache.events = [];

        this.elements = {};
        this.elements.events = [];

        this.nav = {};

        this.events = {};

        this.hideUntilInit = true;

        // potentially leaking a bit but significantly faster in IE
        this.fasterDispose = true;

        this.allDayEventBorderColor = "#000000";
        this.allDayEventFontFamily = 'Tahoma';
        this.allDayEventFontSize = '8pt';
        this.allDayEventFontColor = "#000000";
        this.allDayEventHeight = 25;
        this.allowEventOverlap = true;
        this.allowMultiSelect = true;
        this.autoRefreshCommand = 'refresh';
        this.autoRefreshEnabled = false;
        this.autoRefreshInterval = 60;
        this.autoRefreshMaxCount = 20;
        this.borderColor = "#000000";
        this.businessBeginsHour = 9;
        this.businessEndsHour = 18;
        this.cellBackColor = "#FFFFD5";
        this.cellBackColorNonBusiness = "#FFF4BC";
        this.cellBorderColor = "#999999";
        this.cellHeight = 20;
        this.cellDuration = 30;
        this.columnMarginRight = 5;
        this.cornerBackColor = "#ECE9D8";
        this.crosshairColor = 'Gray';
        this.crosshairOpacity = 20;
        this.crosshairType = "Header";
        this.cssOnly = true;
        this.dayBeginsHour = 0;
        this.dayEndsHour = 24;
        this.days = 1;
        this.deleteImageUrl = null;  // todo
        this.durationBarColor = 'blue';
        this.durationBarVisible = true;
        this.durationBarWidth = 5;
        this.durationBarImageUrl = null;  // todo
        this.eventArrangement = "SideBySide";
        this.eventBackColor = '#ffffff';
        this.eventBorderColor = "#000000";
        this.eventFontFamily = 'Tahoma';
        this.eventFontSize = '8pt';
        this.eventFontColor = "#000000";
        this.eventSelectColor = 'blue';
        this.headerFontSize = '10pt';
        this.headerFontFamily = 'Tahoma';
        this.headerFontColor = "#000000";
        this.headerHeight = 20;
        this.headerLevels = 1;
        this.height = 300;
        this.heightSpec = 'BusinessHours';
        this.hideFreeCells = false;
        this.hourHalfBorderColor = "#F3E4B1";
        this.hourBorderColor = "#EAD098";
        this.hourFontColor = "#000000";
        this.hourFontFamily = "Tahoma";
        this.hourFontSize = "16pt";
        this.hourNameBackColor = "#ECE9D8";
        this.hourNameBorderColor = "#ACA899";
        this.hourWidth = 45;
        this.initScrollPos = 0;
        this.loadingLabelText = "Loading...";
        this.loadingLabelVisible = true;
        this.loadingLabelBackColor = "orange";
        this.loadingLabelFontColor = "#ffffff";
        this.loadingLabelFontFamily = "Tahoma";
        this.loadingLabelFontSize = "10pt";
        this.locale = "en-us";
        this.messageHideAfter = 5000;
        this.moveBy = "Full";
        this.notifyCommit = 'Immediate'; // or 'Queue'
        this.numberFormat = null;
        this.roundedCorners = false;
        this.rtl = false;
        this.selectedColor = "#316AC5";
        this.shadow = 'Fill';
        this.showToolTip = true;
        this.showAllDayEvents = false;
        this.showAllDayEventStartEnd = true;
        this.showHeader = true;
        this.showHours = true;
        this.startDate = new DayPilot.Date().getDatePart();
        this.cssClassPrefix = "calendar_default";
        this.timeFormat = 'Auto';
        this.timeHeaderCellDuration = 60;
        this.useEventBoxes = 'Always';
        this.useEventSelectionBars = false;
        this.viewType = 'Days';

        this.eventClickHandling = 'Enabled';
        this.eventDoubleClickHandling = 'Enabled';
        this.eventRightClickHandling = 'ContextMenu';
        this.eventDeleteHandling = 'Disabled';
        this.eventEditHandling = 'Update';
        this.eventHoverHandling = 'Bubble';
        this.eventResizeHandling = 'Update';
        this.eventMoveHandling = 'Update';
        this.eventSelectHandling = 'Update';
        this.headerClickHandling = 'Enabled';
        this.timeRangeSelectedHandling = 'Enabled';
        this.timeRangeDoubleClickHandling = "Enabled";

        // temporary        
        this.transparent = false;

        this.separateEventsTable = true;

        this.autoRefreshCount = 0;
        this.doubleClickTimeout = 300;
        
        this._browser = {};
        
        this._browser.ie = (navigator && navigator.userAgent && navigator.userAgent.indexOf("MSIE") !== -1);  // IE
        this._browser.ie9 = (navigator && navigator.userAgent && navigator.userAgent.indexOf("MSIE 9") !== -1);  // IE
        this._browser.ielt9 = (function() {
            var div = document.createElement("div");
            div.innerHTML = "<!--[if lt IE 9]><i></i><![endif]-->";
            var isIeLessThan9 = (div.getElementsByTagName("i").length === 1);
            return isIeLessThan9;
        })();

        this._browser.ff = (navigator && navigator.userAgent && navigator.userAgent.indexOf("Firefox") !== -1);
        this._browser.opera105 = (function() {
            if (/Opera[\/\s](\d+\.\d+)/.test(navigator.userAgent)) {
                var v = new Number(RegExp.$1);
                return v >= 10.5;
            }
            return false;
        })();
        this._browser.webkit522 = (function() {
            if (/AppleWebKit[\/\s](\d+\.\d+)/.test(navigator.userAgent)) {
                var v = new Number(RegExp.$1);
                return v >= 522;
            }
            return false;
        })();

        this.clearSelection = function() {
            if (!this.selectedCells) {
                this.selectedCells = [];
                return;
            }
            this._hideSelection();
            this.selectedCells = [];
        };
        
        this._hideSelection = function() {
            if (!this.selectedCells) {
                return;
            }
            for (var j = 0; j < this.selectedCells.length; j++) {
                var cell = this.selectedCells[j];
                if (cell) {
                    //cell.style.backgroundColor = cell.originalColor;
                    //cell.selected = false;
                    //cell.oncontextmenu = null;
                    if (cell.selected) {
                        cell.removeChild(cell.selected);
                        cell.firstChild.style.display = '';
                        cell.selected = null;
                    }
                }
            }
        };
        
        this.cleanSelection = this.clearSelection;

        this._postBack2 = function(action, data, parameters) {
            var envelope = {};
            envelope.action = action;
            envelope.parameters = parameters;
            envelope.data = data;
            envelope.header = this._getCallBackHeader();

            var commandstring = "JSON" + DayPilot.JSON.stringify(envelope);
            __doPostBack(calendar.uniqueID, commandstring);
        };

        this._callBack2 = function(action, parameters, data, type) {

            if (this.callbackTimeout) {
                window.clearTimeout(this.callbackTimeout);
            }

            if (typeof type === 'undefined') {
                type = "CallBack";
            }

            this.callbackTimeout = window.setTimeout(function() {
                calendar._loadingStart();
            }, 100);

            var envelope = {};

            envelope.action = action;
            envelope.type = type;
            envelope.parameters = parameters;
            envelope.data = data;
            envelope.header = this._getCallBackHeader();

            var commandstring = "JSON" + DayPilot.JSON.stringify(envelope);
            if (this.backendUrl) {
                DayPilot.request(this.backendUrl, this._callBackResponse, commandstring, this._ajaxError);
            }
            else if (typeof WebForm_DoCallback === 'function') {
                WebForm_DoCallback(this.uniqueID, commandstring, this._updateView, this.clientName, this.onCallbackError, true);
            }
        };
        
        this._ajaxError = function(req) {
            if (typeof calendar.onAjaxError === 'function') {
                var args = {};
                args.request = req;
                calendar.onAjaxError(args);
            }
            else if (typeof calendar.ajaxError === 'function') { // backwards compatibility
                calendar.ajaxError(req);
            }
        };      

        this.dispose = function() {
            //var start = new Date();

            var c = calendar;
            
            if (!c.nav.top) {
                return;
            }
            
            c._stopAutoRefresh();
            c._deleteEvents();

            if (c.nav.messageClose) { c.nav.messageClose.onclick = null; }
            if (c.nav.hourTable) c.nav.hourTable.oncontextmenu = null;
            if (c.nav.hourTable) c.nav.hourTable.onmousemove = null;
            if (c.nav.header) c.nav.header.oncontextmenu = null;
            if (c.nav.corner) c.nav.corner.oncontextmenu = null;
            c.nav.zoom.onmousemove = null;
            c.nav.scroll.onscroll = null;

            c.nav.scroll.root = null;

            DayPilot.pu(c.nav.loading);

            c._disposeMain();
            c._disposeHeader();

            c.nav.select = null;
            c.nav.cornerRight = null;
            c.nav.scrollable = null;
            c.nav.bottomLeft = null;
            c.nav.bottomRight = null;
            c.nav.allday = null;
            c.nav.zoom = null;
            c.nav.loading = null;
            c.nav.events = null;
            c.nav.header = null;
            c.nav.hourTable = null;
            c.nav.scrolltop = null;
            c.nav.scroll = null;
            c.nav.vsph = null;
            c.nav.main = null;
            c.nav.message = null;
            c.nav.messageClose = null;
            
            c.nav.top.removeAttribute("style");
            c.nav.top.removeAttribute("class");
            c.nav.top.innerHTML = '';
            c.nav.top.dp = null;
            c.nav.top = null;

            DayPilot.ue(window, 'resize', c._onResize);

            DayPilotCalendar.unregister(c);

            //var end = new Date();
            //alert('disposing took ' + (end - start));
        };

        // not sure about this one
        this._registerDispose = function() {
            var root = document.getElementById(id);
            root.dispose = this.dispose;
        };

        this._callBackResponse = function(response) {
            calendar._updateView(response.responseText);
        };

        this._getCallBackHeader = function() {
            var h = {};

            h.v = this.v;
            h.control = "dpc";
            h.id = this.id;
            h.clientState = calendar.clientState;
            h.columns = this._getTreeState();

            h.days = calendar.days;
            h.startDate = calendar.startDate;
            h.cellDuration = calendar.cellDuration;
            h.cssOnly = calendar.cssOnly;
            h.cssClassPrefix = calendar.cssClassPrefix;
            h.heightSpec = calendar.heightSpec;
            h.businessBeginsHour = calendar.businessBeginsHour;
            h.businessEndsHour = calendar.businessEndsHour;
            h.viewType = calendar.viewType;

            h.dayBeginsHour = calendar.dayBeginsHour;
            h.dayEndsHour = calendar.dayEndsHour;
            h.headerLevels = calendar.headerLevels;
            h.backColor = calendar.cellBackColor;
            h.nonBusinessBackColor = calendar.cellBackColorNonBusiness;
            h.eventHeaderVisible = calendar.eventHeaderVisible;
            h.timeFormat = calendar.timeFormat;
            h.timeHeaderCellDuration = calendar.timeHeaderCellDuration;
            h.locale = calendar.locale;
            h.showAllDayEvents = calendar.showAllDayEvents;
            h.tagFields = calendar.tagFields;

            // required for custom hour header rendering
            h.hourNameBackColor = calendar.hourNameBackColor;
            h.hourFontFamily = calendar.hourFontFamily;
            h.hourFontSize = calendar.hourFontSize;
            h.hourFontColor = calendar.hourFontColor;

            h.selected = calendar.multiselect.events();

            // special
            h.hashes = calendar.hashes;

            return h;
        };

        this._out = function() {
            this._crosshairHide();

            // clear active areas
            DayPilot.Areas.hideAll();

            //this.stopScroll();
        };

        this._getTreeState = function() {
            var tree = [];
            tree.ignoreToJSON = true; // preventing Gaia and prototype to mess up with Array serialization

            if (!this.columns) {
                return tree;
            }

            for (var i = 0; i < this.columns.length; i++) {
                var column = this.columns[i];
                var node = this._getNodeState(column);
                tree.push(node);
            }
            return tree;
        };

        this._getNodeState = function(column) {
            //var row = this.rows[i];

            var node = {};
            node.Value = column.id;
            node.Name = column.name;
            node.ToolTip = column.toolTip;
            node.Date = column.start;
            node.Children = this._getNodeChildren(column.children);

            return node;
        };


        this._getNodeChildren = function(array) {
            var children = [];
            children.ignoreToJSON = true; // preventing Gaia to mess up with Array serialization

            if (!array) {
                return children;
            }
            for (var i = 0; i < array.length; i++) {
                children.push(this._getNodeState(array[i]));
            }
            return children;
        };

/*
        // adds a listener
        this.listener = function(object) {
            if (!this.listeners) {
                this.listeners = [];
            }
            this.listeners.push(object);
        };

        this.callListeners = function(action, data) {
            if (!this.listeners) {
                return;
            }

            for (var i = 0; i < this.listeners.length; i++) {
                this.listeners[i]();
            }
        };
*/

        this._updateView = function(result, context) {

            //var start = new Date();
            var result = eval("(" + result + ")");

            if (result.BubbleGuid) {
                var guid = result.BubbleGuid;
                var bubble = this.bubbles[guid];
                delete this.bubbles[guid];

                calendar._loadingStop();
                if (typeof result.Result.BubbleHTML !== 'undefined') {
                    bubble.updateView(result.Result.BubbleHTML, bubble);
                }
                return;
            }
            
            if (result.CallBackRedirect) {
                document.location.href = result.CallBackRedirect;
                return;
            }
            
            if (typeof result.ClientState !== 'undefined') {
                calendar.clientState = result.ClientState;
            }

            if (result.UpdateType === "None") {
                calendar._loadingStop();

                calendar._fireAfterRenderDetached(result.CallBackData, true);
                //calendar.afterRender(result.CallBackData, true);

                if (result.Message) {
                    calendar.message(result.Message);
                }

                return;
            }

            // viewstate update
            if (result.VsUpdate) {
                var vsph = document.createElement("input");
                vsph.type = 'hidden';
                vsph.name = calendar.id + "_vsupdate";
                vsph.id = vsph.name;
                vsph.value = result.VsUpdate;
                calendar.nav.vsph.innerHTML = '';
                calendar.nav.vsph.appendChild(vsph);
            }

            calendar._deleteEvents();

            calendar.multiselect.clear(true);
            calendar.multiselect._initList = result.SelectedEvents;

            if (typeof result.TagFields !== 'undefined') {
                calendar.tagFields = result.TagFields;
            }

            if (typeof result.SortDirections !== 'undefined') {
                calendar.sortDirections = result.SortDirections;
            }

            if (result.UpdateType === "Full") {
                // generated
                calendar.colors = result.Colors;
                calendar.palette = result.Palette;
                calendar.dirtyColors = result.DirtyColors;
                calendar.cellProperties = result.CellProperties;
                calendar.cellConfig = result.CellConfig;

                calendar.columns = result.Columns;

                // state
                // selectedeventvalue

                // properties
                calendar.days = result.Days; //
                calendar.startDate = new DayPilot.Date(result.StartDate).getDatePart(); //
                calendar.cellDuration = result.CellDuration; //
                calendar.heightSpec = result.HeightSpec ? result.HeightSpec : calendar.heightSpec;
                calendar.businessBeginsHour = result.BusinessBeginsHour ? result.BusinessBeginsHour : calendar.businessBeginsHour;
                calendar.businessEndsHour = result.BusinessEndsHour ? result.BusinessEndsHour : calendar.businessEndsHour;
                calendar.viewType = result.ViewType; //
                calendar.headerLevels = result.HeaderLevels; //
                calendar.backColor = result.BackColor ? result.BackColor : calendar.backColor;
                calendar.nonBusinessBackColor = result.NonBusinessBackColor ? result.NonBusinessBackColor : calendar.nonBusinessBackColor;
                calendar.eventHeaderVisible = result.EventHeaderVisible ? result.EventHeaderVisible : calendar.eventHeaderVisible;
                calendar.timeFormat = result.TimeFormat ? result.TimeFormat : calendar.timeFormat;
                calendar.timeHeaderCellDuration = typeof result.TimeHeaderCellDuration !== 'undefined' ? result.TimeHeaderCellDuration : calendar.timeHeaderCellDuration;
                calendar.locale = result.Locale ? result.Locale : calendar.locale;

                calendar.dayBeginsHour = typeof result.DayBeginsHour !== 'undefined' ? result.DayBeginsHour : calendar.dayBeginsHour;
                calendar.dayEndsHour = typeof result.DayEndsHour !== 'undefined' ? result.DayEndsHour : calendar.dayEndsHour;

                // corner
                calendar.cornerBackColor = result.CornerBackColor;
                calendar.cornerHtml = result.CornerHTML;

                // hours
                calendar.hours = result.Hours;

                calendar._prepareColumns();
                calendar._expandCellProperties();
            }

            // hashes
            if (result.Hashes) {
                for (key in result.Hashes) {
                    calendar.hashes[key] = result.Hashes[key];
                }
                //calendar.hashes = result.Hashes;
            }

            calendar._loadEvents(result.Events);
            calendar._updateHeaderHeight();


            if (result.UpdateType === "Full" || calendar.hideFreeCells) {
                calendar._drawHeader();
                calendar._autoHeaderHeight();
                calendar._deleteScrollLabels();
                calendar._updateMessagePosition();
                calendar._drawMain();
                calendar._drawHourTable();
                calendar._updateHeight();
                calendar._fixScrollHeader();
                calendar.clearSelection();
            }

            calendar._show();  // if not visible

            calendar._drawEvents();
            calendar._drawEventsAllDay();
            
            if (calendar.heightSpec === "Parent100Pct") {
                calendar._onResize();
            }

            if (calendar.timeRangeSelectedHandling !== "HoldForever") {
                calendar.clearSelection();
            }

            calendar._updateScrollLabels();

            if (calendar.todo) {
                if (calendar.todo.del) {
                    var del = calendar.todo.del;
                    del.parentNode.removeChild(del);
                    calendar.todo.del = null;
                }
            }

            calendar._fireAfterRenderDetached(result.CallBackData, true);

            calendar._loadingStop();

            calendar._startAutoRefresh();

            if (result.Message) {
                calendar.message(result.Message);
            }

        };

        this._fireAfterRenderDetached = function(data, isCallBack) {
            var afterRenderDelayed = function(data, isc) {
                return function() {
                    if (calendar._api2()) {
                        if (typeof calendar.onAfterRender === 'function') {
                            var args = {};
                            args.isCallBack = isc;
                            args.data = data;
                            
                            calendar.onAfterRender(args);
                        }
                    }
                    else {
                        if (calendar.afterRender) {
                            calendar.afterRender(data, isc);
                        }
                    }
                };
            };

            window.setTimeout(afterRenderDelayed(data, isCallBack), 0);
        };

        this._createShadow = function(object, copyText, type) {
            var main = calendar.nav.events;

            var colWidth = main.clientWidth / main.rows[0].cells.length;
            //var i = Math.floor((calendar.coords.x - 45) / colWidth);
            var i = Math.floor(calendar.coords.x / colWidth);

            if (i < 0) {
                i = 0;
            }

            if (calendar.rtl) {
                i = calendar.columnsBottom.length - i - 1;
            }

            var column = main.rows[0].cells[i];
            
            var _startOffset = 0;

            if (typeof object.duration !== 'undefined') { // external drag&drop
                var duration = object.duration;
                var top = Math.floor(((calendar.coords.y - _startOffset) + calendar.cellHeight / 2) / calendar.cellHeight) * calendar.cellHeight + _startOffset;
                var height = duration * calendar.cellHeight / (60 * calendar.cellDuration);
            }
            else {
                var e = object.event;
                //var data = object.data;
                var height = e.part.height;
                var top = e.part.top;
            }

            var shadow = document.createElement('div');
            shadow.setAttribute('unselectable', 'on');
            shadow.style.position = 'absolute';
            shadow.style.width = '100%';
            shadow.style.height = height + 'px';
            shadow.style.left = '0px';
            shadow.style.top = top + 'px';
            shadow.style.zIndex = 101;
            shadow.exclude = true; // trying to fix the IE flickering issue

            var inner = document.createElement("div");
            shadow.appendChild(inner);

            if (this.cssOnly) {
                shadow.className = calendar._prefixCssClass("_shadow");
                inner.className = this._prefixCssClass("_shadow_inner");
            }

            if (!this.cssOnly) {
                inner.style.position = "absolute";
                inner.style.top = "0px";
                inner.style.bottom = "0px";
                inner.style.left = "0px";
                inner.style.right = "0px";

                if (type === 'Fill') {    // transparent
                    inner.style.backgroundColor = "#aaaaaa";
                    inner.style.opacity = 0.5;
                    inner.style.filter = "alpha(opacity=50)";
                    inner.style.border = '2px solid #aaaaaa';
                }
                else {
                    inner.style.border = '2px dotted #666666';
                }

                if (this.roundedCorners) {
                    inner.style.MozBorderRadius = "5px";
                    inner.style.webkitBorderRadius = "5px";
                    inner.style.borderRadius = "5px";
                }
            }

            column.firstChild.appendChild(shadow);

            return shadow;
        };

        this._durationHours = function() {
            return this._duration() / (3600 * 1000);
        };

        this._businessHoursSpan = function() {
            if (this.businessBeginsHour > this.businessEndsHour) {
                return 24 - this.businessBeginsHour + this.businessEndsHour;
            }
            else {
                return this.businessEndsHour - this.businessBeginsHour;
            }
        };

        this._dayHoursSpan = function() {
            if (this.dayBeginsHour >= this.dayEndsHour) {
                return 24 - this.dayBeginsHour + this.dayEndsHour;
            }
            else {
                return this.dayEndsHour - this.dayBeginsHour;
            }

        };

        // in ticks
        this._duration = function(max) {
            var dHours = 0;

            if (this.heightSpec === 'BusinessHoursNoScroll') {
                dHours = this._businessHoursSpan();
            }
            else if (this.hideFreeCells && !max) {
                var addMinutes = (this.maxEnd - 1) * this.cellDuration / this.cellHeight;
                var addHours = Math.ceil(addMinutes / 60);
                dHours = Math.max(this.dayBeginsHour + addHours, this.businessEndsHour) - this._visibleStart();
            }
            else {
                dHours = this._dayHoursSpan();
            }
            return dHours * 60 * 60 * 1000; // return ticks
        };

        this.message = function(html, delay, foreColor, backColor) {
            if (!html) {
                return;
            }

            var delay = delay || this.messageHideAfter || 2000;
            var foreColor = foreColor || "#ffffff";
            var backColor = backColor || "#000000";
            var opacity = 0.8;

            var div;

            var top = this._totalHeaderHeight();
            var left = this.showHours ? this.hourWidth : 0;
            var right = DayPilot.sw(calendar.nav.scroll);
            
            if (!this.cssOnly) {
                top += 1;
                left += 2;
                right -= 2;
            }

            if (calendar.rtl) {
                var temp = left;
                left = right;
                right = temp;
            }

            if (!this.nav.message) {
                div = document.createElement("div");
                div.style.position = "absolute";
                //div.style.width = "100%";
                div.style.left = (left) + "px";
                div.style.top = (top) + "px";
                div.style.right = "0px";
                div.style.display = 'none';
                //div.style.paddingLeft = (left) + "px";
                
                div.onmousemove = function() {
                    if (calendar.messageTimeout) {
                        clearTimeout(calendar.messageTimeout);
                    }
                };
                
                div.onmouseout = function() {
                    if (calendar.nav.message.style.display !== 'none') {
                        calendar.messageTimeout = setTimeout(calendar._hideMessage, 500);
                    }
                };

                var inner = document.createElement("div");
                inner.onclick = function() { calendar.nav.message.style.display = 'none'; };
                if (!this.cssOnly) {
                    inner.style.padding = "5px";
                    inner.style.opacity = opacity;
                    inner.style.filter = "alpha(opacity=" + (opacity * 100) + ")";
                }
                else {
                    inner.className = this._prefixCssClass("_message");
                }
                div.appendChild(inner);

                var close = document.createElement("div");
                close.style.position = "absolute";
                if (!this.cssOnly) {
                    close.style.top = "5px";
                    close.style.right = (DayPilot.sw(calendar.nav.scroll) + 5) + "px";
                    close.style.color = foreColor;
                    close.style.lineHeight = "100%";
                    close.style.cursor = "pointer";
                    close.style.fontWeight = "bold";
                    close.innerHTML = "X";
                }
                else {
                    close.className = this._prefixCssClass("_message_close");
                }
                close.onclick = function() { calendar.nav.message.style.display = 'none'; };
                div.appendChild(close);

                //this.nav.top.appendChild(div);
                this.nav.top.insertBefore(div, this.nav.loading);
                this.nav.message = div;
                this.nav.messageClose = close;

            }
            else {
                this.nav.message.style.top = top + "px";
            }

            if (this.nav.cornerRight) {
                this.nav.message.style.right = right + "px";
            }
            else {
                this.nav.message.style.right = "0px";
            }

            var showNow = function() {
                //calendar.nav.message.style.opacity = opacity;

                var inner = calendar.nav.message.firstChild;

                if (!calendar.cssOnly) {
                    inner.style.padding = "5px";
                    inner.style.opacity = opacity;
                    inner.style.backgroundColor = backColor;
                    inner.style.color = foreColor;
                }
                inner.innerHTML = html;

                var end = function() { calendar.messageTimeout = setTimeout(calendar._hideMessage, delay); };
                DayPilot.fade(calendar.nav.message, 0.2, end);
            };

            clearTimeout(calendar.messageTimeout);

            // another message was visible
            if (this.nav.message.style.display !== 'none') {
                DayPilot.fade(calendar.nav.message, -0.2, showNow);
            }
            else {
                showNow();
            }

        };
        
        this.message.show = function(html) {
            calendar.message(html);
        };
        
        this.message.hide = function() {
            calendar._hideMessage();
        };

        this._hideMessage = function() {
            var end = function() { calendar.nav.message.style.display = 'none'; };
            DayPilot.fade(calendar.nav.message, -0.2, end);
        };

        /*
        this._hideMessageNow = function() {
            if (this.nav.message) {
                this.nav.message.style.display = 'none';
            }
        };
        */

        this._updateMessagePosition = function() {
            if (this.nav.message) {
                this.nav.message.style.top = (this._totalHeaderHeight()) + "px";
            }
        };

        this._rowCount = function() {
            return this._duration() / (60 * 1000 * this.cellDuration);
        };

        this.eventClickPostBack = function(e, data) {
            this._postBack2('EventClick', data, e);
        };
        this.eventClickCallBack = function(e, data) {
            this._callBack2('EventClick', e, data);
        };

        this._eventClickDispatch = function(e) {
            var div = this;

            var e = e || window.event;
            var ctrlKey = e.ctrlKey;

            if (typeof (DayPilot.Bubble) !== 'undefined') {
                DayPilot.Bubble.hideActive();
            }

            if (calendar.eventDoubleClickHandling === 'Disabled') {
                calendar._eventClickSingle(div, ctrlKey);
                return;
            }

            if (!calendar.timeouts) {
                calendar.timeouts = [];
            }
            else {
                for (var toid in calendar.timeouts) {
                    window.clearTimeout(calendar.timeouts[toid]);
                }
                calendar.timeouts = [];
            }

            var eventClickDelayed = function(div, ctrlKey) {
                return function() {
                    calendar._eventClickSingle(div, ctrlKey);
                };
            };

            calendar.timeouts.push(window.setTimeout(eventClickDelayed(this, ctrlKey), calendar.doubleClickTimeout));

        };

        this._eventClickSingle = function(thisDiv, ctrlKey) {

            //var ev = ev || window.event;

            var e = thisDiv.event;
            if (!e.client.clickEnabled()) {
                return;
            }
            
            if (calendar._api2()) {
                
                var args = {};
                args.e = e;
                args.preventDefault = function() {
                    this.preventDefault.value = true;
                };

                if (typeof calendar.onEventClick === 'function') {
                    calendar.onEventClick(args);
                    if (args.preventDefault.value) {
                        return;
                    }
                }

                switch (calendar.eventClickHandling) {
                    case 'PostBack':
                        calendar.eventClickPostBack(e);
                        break;
                    case 'CallBack':
                        calendar.eventClickCallBack(e);
                        break;
                    case 'JavaScript':
                        calendar.onEventClick(e);
                        break;
                    case 'Edit':
                        if (!e.allday()) {
                            calendar._divEdit(thisDiv);
                        }
                        break;
                    case 'Select':
                        if (!e.allday()) {
                            calendar._eventSelect(thisDiv, e, ctrlKey);
                        }
                        break;
                    case 'Bubble':
                        if (calendar.bubble) {
                            calendar.bubble.showEvent(e);
                        }
                        break;
                    case 'ContextMenu':
                        var menu = e.client.contextMenu();
                        if (menu) {
                            menu.show(e);
                        }
                        else {
                            if (calendar.contextMenu) {
                                calendar.contextMenu.show(e);
                            }
                        }
                        break;
                }

                if (typeof calendar.onEventClicked === 'function') {
                    calendar.onEventClicked(args);
                }                


            }
            else {
                switch (calendar.eventClickHandling) {
                    case 'PostBack':
                        calendar.eventClickPostBack(e);
                        break;
                    case 'CallBack':
                        calendar.eventClickCallBack(e);
                        break;
                    case 'JavaScript':
                        calendar.onEventClick(e);
                        break;
                    case 'Edit':
                        if (!e.allday()) {
                            calendar._divEdit(thisDiv);
                        }
                        break;
                    case 'Select':
                        if (!e.allday()) {
                            calendar._eventSelect(thisDiv, e, ctrlKey);
                        }
                        break;
                    case 'Bubble':
                        if (calendar.bubble) {
                            calendar.bubble.showEvent(e);
                        }
                        break;
                    case 'ContextMenu':
                        var menu = e.client.contextMenu();
                        if (menu) {
                            menu.show(e);
                        }
                        else {
                            if (calendar.contextMenu) {
                                calendar.contextMenu.show(e);
                            }
                        }
                        break;
                }
                
            }

        };


        this.eventDoubleClickPostBack = function(e, data) {
            this._postBack2('EventDoubleClick', data, e);
        };
        this.eventDoubleClickCallBack = function(e, data) {
            this._callBack2('EventDoubleClick', e, data);
        };

        this._eventDoubleClickDispatch = function(ev) {

            if (typeof (DayPilotBubble) !== 'undefined') {
                DayPilotBubble.hideActive();
            }


            if (calendar.timeouts) {
                for (var toid in calendar.timeouts) {
                    window.clearTimeout(calendar.timeouts[toid]);
                }
                calendar.timeouts = null;
            }

            // choose the action 

            var e = this.event;
            var ev = ev || window.event;            
            
            /*
            if (!e.clickingAllowed()) {
            return;
            }*/


            if (calendar._api2()) {
                
                var args = {};
                args.e = e;
                args.preventDefault = function() {
                    this.preventDefault.value = true;
                };

                if (typeof calendar.onEventDoubleClick === 'function') {
                    calendar.onEventDoubleClick(args);
                    if (args.preventDefault.value) {
                        return;
                    }
                }

                switch (calendar.eventDoubleClickHandling) {
                    case 'PostBack':
                        calendar.eventDoubleClickPostBack(e);
                        break;
                    case 'CallBack':
                        calendar.eventDoubleClickCallBack(e);
                        break;
                    case 'Edit':
                        if (!e.allday()) {
                            calendar._divEdit(this);
                        }
                        break;
                    case 'Select':
                        if (!e.allday()) {
                            calendar._eventSelect(this, e, ev.ctrlKey);
                        }
                        break;
                    case 'Bubble':
                        if (calendar.bubble) {
                            calendar.bubble.showEvent(e);
                        }
                        break;
                }
                
                if (typeof calendar.onEventDoubleClicked === 'function') {
                    calendar.onEventDoubleClicked(args);
                }

            }
            else {
                switch (calendar.eventDoubleClickHandling) {
                    case 'PostBack':
                        calendar.eventDoubleClickPostBack(e);
                        break;
                    case 'CallBack':
                        calendar.eventDoubleClickCallBack(e);
                        break;
                    case 'JavaScript':
                        calendar.onEventDoubleClick(e);
                        break;
                    case 'Edit':
                        if (!e.allday()) {
                            calendar._divEdit(this);
                        }
                        break;
                    case 'Select':
                        if (!e.allday()) {
                            calendar._eventSelect(this, e, ev.ctrlKey);
                        }
                        break;
                    case 'Bubble':
                        if (calendar.bubble) {
                            calendar.bubble.showEvent(e);
                        }
                        break;
                }
                
            }

/*
            switch (calendar.eventDoubleClickHandling) {
                case 'PostBack':
                    calendar.eventDoubleClickPostBack(e);
                    break;
                case 'CallBack':
                    calendar.eventDoubleClickCallBack(e);
                    break;
                case 'JavaScript':
                    calendar.onEventDoubleClick(e);
                    break;
                case 'Edit':
                    if (!e.allday()) {
                        calendar._divEdit(this);
                    }
                    break;
                case 'Select':
                    if (!e.allday()) {
                        calendar._eventSelect(this);
                    }
                    break;
                case 'Bubble':
                    if (calendar.bubble) {
                        calendar.bubble.showEvent(e);
                    }
                    break;
            }
*/
        };

        this.eventRightClickPostBack = function(e, data) {
            this._postBack2('EventRightClick', data, e);
        };
        this.eventRightClickCallBack = function(e, data) {
            this._callBack2('EventRightClick', e, data);
        };

        this._eventRightClickDispatch = function() {
            var e = this.event;

            if (!e.client.rightClickEnabled()) {
                return false;
            }

            if (calendar._api2()) {

                var args = {};
                args.e = e;
                args.preventDefault = function() {
                    this.preventDefault.value = true;
                };

                if (typeof calendar.onEventRightClick === 'function') {
                    calendar.onEventRightClick(args);
                    if (args.preventDefault.value) {
                        return;
                    }
                }
                
                switch (calendar.eventRightClickHandling) {
                    case 'PostBack':
                        calendar.eventRightClickPostBack(e);
                        break;
                    case 'CallBack':
                        calendar.eventRightClickCallBack(e);
                        break;
                    case 'ContextMenu':
                        var menu = e.client.contextMenu();
                        if (menu) {
                            menu.show(e);
                        }
                        else {
                            if (calendar.contextMenu) {
                                calendar.contextMenu.show(this.event);
                            }
                        }
                        break;
                    case 'Bubble':
                        if (calendar.bubble) {
                            calendar.bubble.showEvent(e);
                        }
                        break;                        
                }
                
                if (typeof calendar.onEventRightClicked === 'function') {
                    calendar.onEventRightClicked(args);
                }                
                
            }
            else {
                switch (calendar.eventRightClickHandling) {
                    case 'PostBack':
                        calendar.eventRightClickPostBack(e);
                        break;
                    case 'CallBack':
                        calendar.eventRightClickCallBack(e);
                        break;
                    case 'JavaScript':
                        calendar.onEventRightClick(e);
                        break;
                    case 'ContextMenu':
                        var menu = e.client.contextMenu();
                        if (menu) {
                            menu.show(e);
                        }
                        else {
                            if (calendar.contextMenu) {
                                calendar.contextMenu.show(this.event);
                            }
                        }
                        break;
                    case 'Bubble':
                        if (calendar.bubble) {
                            calendar.bubble.showEvent(e);
                        }
                        break;
                }
            }

/*
            switch (calendar.eventRightClickHandling) {
                case 'PostBack':
                    calendar.eventRightClickPostBack(e);
                    break;
                case 'CallBack':
                    calendar.eventRightClickCallBack(e);
                    break;
                case 'JavaScript':
                    calendar.onEventRightClick(e);
                    break;
                case 'ContextMenu':
                    var menu = e.client.contextMenu();
                    if (menu) {
                        menu.show(e);
                    }
                    else {
                        if (calendar.contextMenu) {
                            calendar.contextMenu.show(e);
                        }
                    }
                    break;
                case 'Bubble':
                    if (calendar.bubble) {
                        calendar.bubble.showEvent(e);
                    }
                    break;

            }
            */

            return false;
        };

        this.headerClickPostBack = function(c, data) {
            this._postBack2('HeaderClick', data, c);
        };
        this.headerClickCallBack = function(c, data) {
            this._callBack2('HeaderClick', c, data);
        };

        this._headerClickDispatch = function(object) {

            var data = this.data;
            var c = new DayPilotCalendar.Column(data.id, data.name, data.start);
            // check if allowed

            if (calendar._api2()) {

                var args = {};
                args.header = {};
                args.header.id = data.id;
                args.header.name = data.name;
                args.header.start = data.start;
                
                args.preventDefault = function() {
                    this.preventDefault.value = true;
                };

                if (typeof calendar.onHeaderClick === 'function') {
                    calendar.onHeaderClick(args);
                    if (args.preventDefault.value) {
                        return;
                    }
                }

                switch (calendar.headerClickHandling) {
                    case 'PostBack':
                        calendar.headerClickPostBack(c);
                        break;
                    case 'CallBack':
                        calendar.headerClickCallBack(c);
                        break;
                }
                
                if (typeof calendar.onHeaderClicked === 'function') {
                    calendar.onHeaderClicked(args);
                }
            }
            else {
                switch (calendar.headerClickHandling) {
                    case 'PostBack':
                        calendar.headerClickPostBack(c);
                        break;
                    case 'CallBack':
                        calendar.headerClickCallBack(c);
                        break;
                    case 'JavaScript':
                        calendar.onHeaderClick(c);
                        break;
                }
            }

/*
            switch (calendar.headerClickHandling) {
                case 'PostBack':
                    calendar.headerClickPostBack(c);
                    break;
                case 'CallBack':
                    calendar.headerClickCallBack(c);
                    break;
                case 'JavaScript':
                    calendar.onHeaderClick(c);
                    break;
            }
            */
        };

        this._headerMouseMove = function() {
            if (typeof (DayPilotBubble) !== 'undefined' && calendar.columnBubble) {
                if (calendar.viewType === "Resources") {
                    var res = {};
                    res.calendar = calendar;
                    res.id = this.data.id;
                    res.toJSON = function() {
                        var json = {};
                        json.id = this.id;
                        return json;
                    };
                    calendar.columnBubble.showResource(res);
                }
                else {
                    var start = new DayPilot.Date(this.data.start);
                    var end = start.addDays(1);

                    var time = {};
                    time.calendar = calendar;
                    time.start = start;
                    time.end = end;
                    time.toJSON = function() {
                        var json = {};
                        json.start = this.start;
                        json.end = this.end;
                        return json;
                    };

                    calendar.columnBubble.showTime(time);
                }
            }

            var cell = this;
            var div = cell.firstChild; // rowheader
            if (!div.active) {
                //div.data = calendar.rows[td.index];  // TODO replace with custom object
                var data = cell.data;
                var c = new DayPilotCalendar.Column(data.id, data.name, data.start);
                c.areas = cell.data.areas;

                DayPilot.Areas.showAreas(div, c);
            }

        };

        this._headerMouseOut = function(ev) {
            if (typeof (DayPilotBubble) !== 'undefined' && calendar.columnBubble) {
                calendar.columnBubble.hideOnMouseOut();
            }
            DayPilot.Areas.hideAreas(this.firstChild, ev);
        };

        this.eventDeletePostBack = function(e, data) {
            this._postBack2('EventDelete', data, e);
        };
        this.eventDeleteCallBack = function(e, data) {
            this._callBack2('EventDelete', e, data);
        };

        this._eventDeleteDispatch = function(object) {
            var e = object.parentNode.parentNode.event;

            if (calendar._api2()) {

                var args = {};
                args.e = e;
                
                args.preventDefault = function() {
                    this.preventDefault.value = true;
                };

                if (typeof calendar.onEventDelete === 'function') {
                    calendar.onEventDelete(args);
                    if (args.preventDefault.value) {
                        return;
                    }
                }

                switch (calendar.eventDeleteHandling) {
                    case 'PostBack':
                        calendar.eventDeletePostBack(e);
                        break;
                    case 'CallBack':
                        calendar.eventDeleteCallBack(e);
                        break;
                }
                
                if (typeof calendar.onEventDeleted === 'function') {
                    calendar.onEventDeleted(args);
                }
            }
            else {
                switch (calendar.eventDeleteHandling) {
                    case 'PostBack':
                        calendar.eventDeletePostBack(e);
                        break;
                    case 'CallBack':
                        calendar.eventDeleteCallBack(e);
                        break;
                    case 'JavaScript':
                        calendar.onEventDelete(e);
                        break;
                }
            }

/*
            switch (calendar.eventDeleteHandling) {
                case 'PostBack':
                    calendar.eventDeletePostBack(e);
                    break;
                case 'CallBack':
                    calendar.eventDeleteCallBack(e);
                    break;
                case 'JavaScript':
                    calendar.onEventDelete(e);
                    break;
            }
            */
        };

        this.eventResizePostBack = function(e, newStart, newEnd, data) {
            if (!newStart) {
                throw 'newStart is null';
            }
            if (!newEnd) {
                throw 'newEnd is null';
            }

            var params = {};
            params.e = e;
            params.newStart = newStart;
            params.newEnd = newEnd;

            this._postBack2('EventResize', data, params);
        };

        this.eventResizeCallBack = function(e, newStart, newEnd, data) {
            if (!newStart)
                throw 'newStart is null';
            if (!newEnd)
                throw 'newEnd is null';

            var params = {};
            params.e = e;
            params.newStart = newStart;
            params.newEnd = newEnd;

            this._callBack2('EventResize', params, data);
        };
        
        this._invokeEvent = function(type, action, params, data) {

            if (type === 'PostBack') {
                calendar.postBack2(action, params, data);
            }
            else if (type === 'CallBack') {
                calendar._callBack2(action, params, data, "CallBack");
            }
            else if (type === 'Immediate') {
                calendar._callBack2(action, params, data, "Notify");
            }
            else if (type === 'Queue') {
                calendar.queue.add(new DayPilot.Action(this, action, params, data));
            }
            else if (type === 'Notify') {
                if (resolved.notifyType() === 'Notify') {
                    calendar._callBack2(action, params, data, "Notify");
                }
                else {
                    calendar.queue.add(new DayPilot.Action(calendar, action, params, data));
                }
            }
            else {
                throw "Invalid event invocation type";
            }
        };    
        
        // just hours (int)
        this._visibleStart = function(max) {

            if (this.heightSpec === 'BusinessHoursNoScroll') {
                return this.businessBeginsHour;
            }
            else if (this.hideFreeCells && !max) {
                var addMinutes = (this.minStart - 1) * this.cellDuration / this.cellHeight;
                var addHours = Math.floor(addMinutes / 60);
                return Math.min(this.dayBeginsHour + addHours, this.businessBeginsHour);
            }
            else {
                return this.dayBeginsHour;
            }
        };
        
        this._api2 = function() {
            return calendar.api === 2;
        };        

        this._eventResizeDispatch = function(e, shadowHeight, shadowTop, border) {
            
            if (this.eventResizeHandling === 'Disabled') {
                return;
            }
            
            var _startOffset = 0;

            var newStart = new Date();
            var newEnd = new Date();

            var start = e.start();
            var end = e.end();
            var cellSize = calendar.cellDuration; // should be integer
            //var day = new Date();

            if (border === 'top') {
                var day = start.getDatePart();
                var step = Math.floor((shadowTop - _startOffset) / calendar.cellHeight);
                var minutes = step * cellSize;
                var ts = minutes * 60 * 1000;
                var visibleStartOffset = calendar._visibleStart() * 60 * 60 * 1000;

                newStart = day.addTime(ts + visibleStartOffset);
                newEnd = e.end();
                
            }
            else if (border === 'bottom') {
                var day = end.getDatePart();
                var step = Math.floor((shadowTop + shadowHeight - _startOffset) / calendar.cellHeight);
                var minutes = step * cellSize;
                var ts = minutes * 60 * 1000;
                var visibleStartOffset = calendar._visibleStart() * 60 * 60 * 1000;

                newStart = start;
                newEnd = day.addTime(ts + visibleStartOffset);
            }

            if (calendar._api2()) {
                // API v2
                var args = {};

                args.e = e;
                args.newStart = newStart;
                args.newEnd = newEnd;
                args.preventDefault = function() {
                    this.preventDefault.value = true;
                };

                if (typeof calendar.onEventResize === 'function') {
                    calendar.onEventResize(args);
                    if (args.preventDefault.value) {
                        return;
                    }
                }

                switch (calendar.eventResizeHandling) {
                    case 'PostBack':
                        calendar.eventResizePostBack(e, newStart, newEnd);
                        break;
                    case 'CallBack':
                        calendar.eventResizeCallBack(e, newStart, newEnd);
                        break;
                    case 'Notify':
                        calendar.eventResizeNotify(e, newStart, newEnd);
                        break;
                    case 'Update':
                        e.start(newStart);
                        e.end(newEnd);
                        calendar.events.update(e);
                        break;
                }

                if (typeof calendar.onEventResized === 'function') {
                    calendar.onEventResized(args);
                }
            }
            else {
               switch (calendar.eventResizeHandling) {
                    case 'PostBack':
                        calendar.eventResizePostBack(e, newStart, newEnd);
                        break;
                    case 'CallBack':
                        calendar.eventResizeCallBack(e, newStart, newEnd);
                        break;
                    case 'JavaScript':
                        calendar.onEventResize(e, newStart, newEnd);
                        break;
                    case 'Notify':
                        calendar.eventResizeNotify(e, newStart, newEnd);
                        break;

                }
            }
        };
        
        this.eventResizeNotify = function(e, newStart, newEnd, data) {

            var old = new DayPilot.Event(e.copy(), this);

            e.start(newStart);
            e.end(newEnd);
            e.commit();

            calendar.update();

            this._invokeEventResize("Notify", old, newStart, newEnd, data);

        };    
        
        this._invokeEventResize = function(type, e, newStart, newEnd, data) {
            var params = {};
            params.e = e;
            params.newStart = newStart;
            params.newEnd = newEnd;

            this._invokeEvent(type, "EventResize", params, data);
        };
        

        this.eventMovePostBack = function(e, newStart, newEnd, newResource, data) {
            if (!newStart)
                throw 'newStart is null';
            if (!newEnd)
                throw 'newEnd is null';

            var params = {};
            params.e = e;
            params.newStart = newStart;
            params.newEnd = newEnd;
            params.newResource = newResource;

            this._postBack2('EventMove', data, params);
        };

        this.eventMoveCallBack = function(e, newStart, newEnd, newResource, data) {
            if (!newStart)
                throw 'newStart is null';
            if (!newEnd)
                throw 'newEnd is null';

            var params = {};
            params.e = e;
            params.newStart = newStart;
            params.newEnd = newEnd;
            params.newResource = newResource;

            this._callBack2('EventMove', params, data);
        };

        this._eventMoveDispatch = function(e, newColumnIndex, shadowTop, ev, drag) {
            
            if (calendar.eventMoveHandling === 'Disabled') {
                return;
            }
            
            var _startOffset = 0;
            var step = Math.floor((shadowTop - _startOffset) / calendar.cellHeight);

            var cellSize = calendar.cellDuration; // should be integer
            var boxStart = step * cellSize * 60 * 1000;
            var start = e.start();
            var end = e.end();
            var day = new Date();

            if (start.isDayPilotDate) {
                start = start.d;
            }
            day.setTime(Date.UTC(start.getUTCFullYear(), start.getUTCMonth(), start.getUTCDate()));

            var startOffset = (calendar.useEventBoxes !== 'Never') ? start.getTime() - (day.getTime() + start.getUTCHours() * 3600 * 1000 + Math.floor(start.getUTCMinutes() / cellSize) * cellSize * 60 * 1000) : 0;
            var length = end.getTime() - start.getTime();
            var visibleStartOffset = calendar._visibleStart() * 3600 * 1000;

            var newColumn = this.columnsBottom[newColumnIndex];

            var date = newColumn.start.getTime();
            var newStartUTC = new Date();
            newStartUTC.setTime(date + boxStart + startOffset + visibleStartOffset);

            var newStart = new DayPilot.Date(newStartUTC);

            var newEnd = newStart.addTime(length);
            
            var external = !!drag;
            var newResource = newColumn.id;
            
            if (calendar._api2()) {
                // API v2
                var args = {};

                args.e = e;
                args.newStart = newStart;
                args.newEnd = newEnd;
                args.newResource = newResource;
                args.external = external;
                args.ctrl = false;
                if (ev) {
                    args.ctrl = ev.ctrlKey;
                }
                args.shift = false;
                if (ev) {
                    args.shift = ev.shiftKey;
                }
                args.preventDefault = function() {
                    this.preventDefault.value = true;
                };

                if (typeof calendar.onEventMove === 'function') {
                    calendar.onEventMove(args);
                    if (args.preventDefault.value) {
                        return;
                    }
                }
                
                switch (calendar.eventMoveHandling) {
                    case 'PostBack':
                        calendar.eventMovePostBack(e, newStart, newEnd, newResource);
                        break;
                    case 'CallBack':
                        calendar.eventMoveCallBack(e, newStart, newEnd, newResource);
                        break;
                    case 'Notify':
                        calendar.eventMoveNotify(e, newStart, newEnd, newResource);
                        break;
                    case 'Update':
                        e.start(newStart);
                        e.end(newEnd);
                        e.resource(newResource);
                        calendar.events.update(e);
                        break;
                }
                
                if (typeof calendar.onEventMoved === 'function') {
                    calendar.onEventMoved(args);
                }                
            }
            else {
                switch (calendar.eventMoveHandling) {
                    case 'PostBack':
                        calendar.eventMovePostBack(e, newStart, newEnd, newResource);
                        break;
                    case 'CallBack':
                        calendar.eventMoveCallBack(e, newStart, newEnd, newResource);
                        break;
                    case 'JavaScript':
                        calendar.onEventMove(e, newStart, newEnd, newResource, external, ev ? ev.ctrlKey : false, ev ? ev.shiftKey : false);
                        break;
                    case 'Notify':
                        calendar.eventMoveNotify(e, newStart, newEnd, newResource, null);
                        break;

                }
            }

        };

        this.eventMoveNotify = function(e, newStart, newEnd, newResource, data) {

            var old = new DayPilot.Event(e.copy(), this);

            e.start(newStart);
            e.end(newEnd);
            e.resource(newResource);
            e.commit();

            calendar.update();

            this._invokeEventMove("Notify", old, newStart, newEnd, newResource, data);

        };

        this._invokeEventMove = function(type, e, newStart, newEnd, newResource, data) {
            var params = {};
            params.e = e;
            params.newStart = newStart;
            params.newEnd = newEnd;
            params.newResource = newResource;

            this._invokeEvent(type, "EventMove", params, data);
        };

/*
        this._eventBubbleCallBack = function(e, bubble) {
            var guid = DayPilot.guid();
            if (!this.bubbles) {
                this.bubbles = [];
            }

            this.bubbles[guid] = bubble;

            var params = {};
            params.e = e;
            params.guid = guid;

            this._callBack2("EventBubble", null, params);
        };
*/

        // called by DayPilot.Bubble
        this._bubbleCallBack = function(args, bubble) {
            var guid = calendar._recordBubbleCall(bubble);

            var params = {};
            params.args = args;
            params.guid = guid;

            calendar._callBack2("Bubble", params);
        };

        this._recordBubbleCall = function(bubble) {
            var guid = DayPilot.guid();
            if (!this.bubbles) {
                this.bubbles = [];
            }

            this.bubbles[guid] = bubble;
            return guid;
        };

        this.eventMenuClickPostBack = function(e, command, data) {
            var params = {};
            params.e = e;
            params.command = command;

            this._postBack2('EventMenuClick', data, params);

        };
        this.eventMenuClickCallBack = function(e, command, data) {
            var params = {};
            params.e = e;
            params.command = command;

            this._callBack2('EventMenuClick', params, data);

        };

        // called by DayPilot.Menu
        this._eventMenuClick = function(command, e, handling) {
            switch (handling) {
                case 'PostBack':
                    calendar.eventMenuClickPostBack(e, command);
                    break;
                case 'CallBack':
                    calendar.eventMenuClickCallBack(e, command);
                    break;
            }
        };

        this.timeRangeMenuClickPostBack = function(e, command, data) {
            //        this.postBack('TRM:', e.start, e.end, e.resource, command);
            var params = {};
            params.selection = e;
            params.command = command;

            this._postBack2("TimeRangeMenuClick", data, params);

        };
        this.timeRangeMenuClickCallBack = function(e, command, data) {
            var params = {};
            params.selection = e;
            params.command = command;

            this._callBack2("TimeRangeMenuClick", params, data);
        };

        // called by DayPilot.Menu
        this._timeRangeMenuClick = function(command, e, handling) {
            switch (handling) {
                case 'PostBack':
                    calendar.timeRangeMenuClickPostBack(e, command);
                    break;
                case 'CallBack':
                    calendar.timeRangeMenuClickCallBack(e, command);
                    break;
            }
        };

        this.timeRangeSelectedPostBack = function(start, end, resource, data) {
            //this.postBack('FRE:', start, end, column);
            var range = {};
            range.start = start;
            range.end = end;
            range.resource = resource;

            this._postBack2('TimeRangeSelected', data, range);
        };
        this.timeRangeSelectedCallBack = function(start, end, resource, data) {

            var range = {};
            range.start = start;
            range.end = end;
            range.resource = resource;

            this._callBack2('TimeRangeSelected', range, data);
        };

        this._timeRangeSelectedDispatch = function(start, end, column) {
            // make sure it's DayPilot.Date
            if (!start.isDayPilotDate) {
                start = new DayPilot.Date(start);
            }
            if (!end.isDayPilotDate) {
                end = new DayPilot.Date(end);
            }
            
            var resource = column;
            
            if (calendar._api2()) {
                
                var args = {};
                args.start = start;
                args.end = end;
                args.resource = resource;
                args.preventDefault = function() {
                    this.preventDefault.value = true;
                };
                
                if (typeof calendar.onTimeRangeSelect === 'function') {
                    calendar.onTimeRangeSelect(args);
                    if (args.preventDefault.value) {
                        return;
                    }
                }

                // now perform the default builtin action
                switch (calendar.timeRangeSelectedHandling) {
                    case 'PostBack':
                        calendar.timeRangeSelectedPostBack(start, end, resource);
                        break;
                    case 'CallBack':
                        calendar.timeRangeSelectedCallBack(start, end, resource);
                        break;
                }
                
                if (typeof calendar.onTimeRangeSelected === 'function') {
                    calendar.onTimeRangeSelected(args);
                }
                
            }
            else {
                switch (calendar.timeRangeSelectedHandling) {
                    case 'PostBack':
                        calendar.timeRangeSelectedPostBack(start, end, column);
                        break;
                    case 'CallBack':
                        calendar.timeRangeSelectedCallBack(start, end, column);
                        break;
                    case 'JavaScript':
                        calendar.onTimeRangeSelected(start, end, column);
                        break;
                }
            }
        };

        this.timeRangeDoubleClickPostBack = function(start, end, column, data) {
            var range = {};
            range.start = start;
            range.end = end;
            range.resource = column;

            this._postBack2('TimeRangeDoubleClick', data, range);
            //        this.postBack('TRD:', start, end, column);
        };
        this.timeRangeDoubleClickCallBack = function(start, end, column, data) {

            var range = {};
            range.start = start;
            range.end = end;
            range.resource = column;

            this._callBack2('TimeRangeDoubleClick', range, data);
        };

        this._timeRangeDoubleClickDispatch = function(start, end, column) {
            if (calendar._api2()) {

                var resource = column;
                
                var args = {};
                args.start = start;
                args.end = end;
                args.resource = resource;
                
                args.preventDefault = function() {
                    this.preventDefault.value = true;
                };

                if (typeof calendar.onTimeRangeDoubleClick === 'function') {
                    calendar.onTimeRangeDoubleClick(args);
                    if (args.preventDefault.value) {
                        return;
                    }
                }

                switch (calendar.timeRangeDoubleClickHandling) {
                    case 'PostBack':
                        calendar.timeRangeDoubleClickPostBack(start, end, resource);
                        break;
                    case 'CallBack':
                        calendar.timeRangeDoubleClickCallBack(start, end, resource);
                        break;
                }
                
                if (typeof calendar.onTimeRangeDoubleClicked === 'function') {
                    calendar.onTimeRangeDoubleClicked(args);
                }
            }
            else {
                switch (calendar.timeRangeDoubleClickHandling) {
                    case 'PostBack':
                        calendar.timeRangeDoubleClickPostBack(start, end, column);
                        break;
                    case 'CallBack':
                        calendar.timeRangeDoubleClickCallBack(start, end, column);
                        break;
                    case 'JavaScript':
                        calendar.onTimeRangeDoubleClick(start, end, column);
                        break;
                }
            }
        };

        this.eventEditPostBack = function(e, newText, data) {
            var params = {};
            params.e = e;
            params.newText = newText;

            this._postBack2("EventEdit", data, params);
        };
        
        this.eventEditCallBack = function(e, newText, data) {

            var params = {};
            params.e = e;
            params.newText = newText;

            this._callBack2("EventEdit", params, data);
        };

        this._eventEditDispatch = function(e, newText) {
            if (calendar._api2()) {
                
                var args = {};
                args.e = e;
                args.newText = newText;
                args.preventDefault = function() {
                    this.preventDefault.value = true;
                };

                if (typeof calendar.onEventEdit === 'function') {
                    calendar.onEventEdit(args);
                    if (args.preventDefault.value) {
                        return;
                    }
                }

                switch (calendar.eventEditHandling) {
                    case 'PostBack':
                        calendar.eventEditPostBack(e, newText);
                        break;
                    case 'CallBack':
                        calendar.eventEditCallBack(e, newText);
                        break;
                    case 'Update':
                        e.text(newText);
                        calendar.events.update(e);
                        break;
                }   
                
                if (typeof calendar.onEventEdited === 'function') {
                    calendar.onEventEdited(args);
                    if (args.preventDefault.value) {
                        return;
                    }
                }
            }
            else {
                switch (calendar.eventEditHandling) {
                    case 'PostBack':
                        calendar.eventEditPostBack(e, newText);
                        break;
                    case 'CallBack':
                        calendar.eventEditCallBack(e, newText);
                        break;
                    case 'JavaScript':
                        calendar.onEventEdit(e, newText);
                        break;
                }
            }
            
            /*
            switch (calendar.eventEditHandling) {
                case 'PostBack':
                    calendar.eventEditPostBack(e, newText);
                    break;
                case 'CallBack':
                    calendar.eventEditCallBack(e, newText);
                    break;
                case 'JavaScript':
                    calendar.onEventEdit(e, newText);
                    break;
            }*/
        };

        this.eventSelectPostBack = function(e, change, data) {
            var params = {};
            params.e = e;
            params.change = change;
            this._postBack2('EventSelect', data, params);
        };
        
        this.eventSelectCallBack = function(e, change, data) {
            var params = {};
            params.e = e;
            params.change = change;
            this._callBack2('EventSelect', params, data);
        };

        this._eventSelectDispatch = function(div, e, ctrlKey) {
            
            if (calendar._api2()) {
                
                var m = calendar.multiselect;
                m.previous = m.events();

                var args = {};
                args.e = e;
                args.selected = m.isSelected(e);
                args.ctrl = ctrlKey;
                args.preventDefault = function() {
                    this.preventDefault.value = true;
                };

                if (typeof calendar.onEventSelect === 'function') {
                    calendar.onEventSelect(args);
                    if (args.preventDefault.value) {
                        return;
                    }
                }

                switch (calendar.eventSelectHandling) {
                    case 'PostBack':
                        calendar.eventSelectPostBack(e, change);
                        break;
                    case 'CallBack':
                        if (typeof WebForm_InitCallback !== 'undefined') {
                            __theFormPostData = "";
                            __theFormPostCollection = [];
                            WebForm_InitCallback();
                        }
                        calendar.eventSelectCallBack(e, change);
                        break;
                    case 'Update':
                        m._toggleDiv(div, ctrlKey);
                        break;
                }
                
                if (typeof calendar.onEventSelected === 'function') {
                    args.change = m.isSelected(e) ? "selected" : "deselected";
                    args.selected = m.isSelected(e);
                    calendar.onEventSelected(args);
                }                
                
            }
            else {
                var m = calendar.multiselect;
                m.previous = m.events();
                m._toggleDiv(div, ctrlKey);
                var change = m.isSelected(e) ? "selected" : "deselected";

                switch (calendar.eventSelectHandling) {
                    case 'PostBack':
                        calendar.eventSelectPostBack(e, change);
                        break;
                    case 'CallBack':
                        if (typeof WebForm_InitCallback !== 'undefined') {
                            __theFormPostData = "";
                            __theFormPostCollection = [];
                            WebForm_InitCallback();
                        }
                        calendar.eventSelectCallBack(e, change);
                        break;
                    case 'JavaScript':
                        calendar.onEventSelect(e, change);
                        break;
                }
            }
            
            /*
            switch (calendar.eventSelectHandling) {
                case 'PostBack':
                    calendar.eventSelectPostBack(e, change);
                    break;
                case 'CallBack':
                    __theFormPostData = "";
                    __theFormPostCollection = [];
                    if (WebForm_InitCallback) {
                        WebForm_InitCallback();
                    }
                    calendar.eventSelectCallBack(e, change);
                    break;
                case 'JavaScript':
                    calendar.onEventSelect(e, change);
                    break;
            }
            */
        };

        this.commandCallBack = function(command, data) {
            this._stopAutoRefresh();

            var params = {};
            params.command = command;

            this._callBack2('Command', params, data);
        };

        this._onCellMouseDown = function(ev) {
            
            clearTimeout(DayPilotCalendar.selectedTimeout);

            //alert('mousedown');
            
            if (DayPilotCalendar.selecting) {
                return;
            }

            if (DayPilotCalendar.editing) {
                DayPilotCalendar.editing.blur();
                return;
            }

            // if double click is active, check if the click was inside
            //if (DayPilotCalendar.selectedCells && calendar.freeTimeDoubleClickHandling != 'Disabled') {
            if (calendar.selectedCells && calendar.timeRangeDoubleClickHandling !== 'Disabled') {
                // only if the click is on an active cell
                for (var i = 0; i < calendar.selectedCells.length; i++) {
                    if (this === calendar.selectedCells[i]) {
                        return;
                    }
                }
            }

            if (calendar.timeRangeSelectedHandling === "Disabled") {
                return;
            }

            var button = (window.event) ? window.event.button : ev.which;
            if (button !== 1 && button !== 0) {  // Khtml says first button is 0
                return;
            }

            DayPilotCalendar.firstMousePos = DayPilot.mc(ev || window.event);
            calendar.clearSelection();  // initializes selectedCells if necessary
            DayPilotCalendar.topSelectedCell = this;
            DayPilotCalendar.bottomSelectedCell = this;
            DayPilotCalendar.column = DayPilotCalendar.getColumn(this);
            calendar.selectedCells.push(this);
            DayPilotCalendar.firstSelected = this;

        };

        this._activateSelection = function() {
            if (!this.selectedCells) {
                return;
            }
            
            var selection = this.getSelection();
            
            if (!selection) {
                return;
            }

            // color them
            for (var j = 0; j < calendar.selectedCells.length; j++) {
                var cell = calendar.selectedCells[j];
                if (cell && !cell.selected) {
                    //cell.style.backgroundColor = calendar.selectedColor;
                    //cell.selected = true;

                    var div = document.createElement("div");
                    //div.style.position = "absolute";
                    //div.style.top = "0px";
                    //div.style.left = "0px";
                    div.style.height = (calendar.cellHeight - 1) + "px";
                    //div.style.right = "0px";
                    div.style.backgroundColor = calendar.selectedColor;
                    cell.firstChild.style.display = "none";
                    cell.insertBefore(div, cell.firstChild);

                    cell.selected = div;
                }
            }
        };

        this._onCellMouseOut = function(ev) {
            if (typeof (DayPilotBubble) !== 'undefined' && calendar.cellBubble) {
                calendar.cellBubble.hideOnMouseOut();
            }

        };

        this._onCellMouseMove = function(ev) {

            if (typeof (DayPilotCalendar) === 'undefined') {
                return;
            }

            if (typeof (DayPilotBubble) !== 'undefined' && calendar.cellBubble) {
                var column = DayPilotCalendar.getColumn(this);
                var res = calendar.columnsBottom[column].id;

                var cell = {};
                cell.calendar = calendar;
                cell.start = this.start;
                cell.end = this.end;
                cell.resource = res;
                cell.toJSON = function() {
                    var json = {};
                    json.start = this.start;
                    json.end = this.end;
                    json.resource = this.resource;
                    return json;
                };

                calendar.cellBubble.showCell(cell);
            }

            // activate selecting on first move
            if (DayPilotCalendar.firstMousePos) {
                var first = DayPilotCalendar.firstMousePos;
                var now = DayPilot.mc(ev || window.event);
                if (first.x !== now.x || first.y !== now.y) {
                    DayPilotCalendar.selecting = true;
                    calendar.clearSelection();
                    calendar._activateSelection();
                }
            }

            if (!DayPilotCalendar.selecting) {
                return;
            }

            var mousePos = DayPilot.mc(ev || window.event);

            var thisColumn = DayPilotCalendar.getColumn(this);
            if (thisColumn !== DayPilotCalendar.column) {
                return;
            }

            // clean
            calendar.clearSelection();

            // new selected cells
            if (mousePos.y < DayPilotCalendar.firstMousePos.y) {
                calendar.selectedCells = DayPilotCalendar.getCellsBelow(this);
                DayPilotCalendar.topSelectedCell = calendar.selectedCells[0];
                DayPilotCalendar.bottomSelectedCell = DayPilotCalendar.firstSelected;
            }
            else {
                calendar.selectedCells = DayPilotCalendar.getCellsAbove(this);
                DayPilotCalendar.topSelectedCell = DayPilotCalendar.firstSelected;
                DayPilotCalendar.bottomSelectedCell = calendar.selectedCells[0];
            }

            calendar._activateSelection();
        };

        this.getSelection = function() {
            if (!DayPilotCalendar.topSelectedCell) {
                return null;
            }
            if (!DayPilotCalendar.bottomSelectedCell) {
                return null;
            }
            
            var start = DayPilotCalendar.topSelectedCell.start;
            var end = DayPilotCalendar.bottomSelectedCell.end;
            var columnId = DayPilotCalendar.topSelectedCell.resource;

            return new DayPilot.Selection(start, end, columnId, calendar);
        };

        this._onMainMouseUp = function(ev) {
            
            if (DayPilotCalendar.firstMousePos) {
                var fire = function() {
                    
                    // make sure it's visible
                    DayPilotCalendar.selecting = true;
                    //calendar.clearSelection();
                    calendar._activateSelection();
                    
                    DayPilotCalendar.firstMousePos = null;
                    DayPilotCalendar.selecting = false;
                    var sel = calendar.getSelection();
                    calendar._timeRangeSelectedDispatch(sel.start, sel.end, sel.resource);
                };

                if (DayPilotCalendar.selecting && DayPilotCalendar.topSelectedCell !== null) {
                    fire();
                }
                else {  // delayed
                    //DayPilotCalendar.selectedTimeout = setTimeout(fire, calendar.doubleClickTimeout);
                    DayPilotCalendar.selectedTimeout = setTimeout(fire, 100);
                }
            }
            else {
                DayPilotCalendar.selecting = false;
            }
        };

        this._scroll = function(ev) {
            // don't run it if scrolling is disabled
            if (!calendar.initScrollPos)  { 
                return;
            }
            
            var scrolling = calendar.columnWidthSpec === "Fixed";
            
            if (scrolling) {
                calendar.nav.bottomLeft.scrollTop = calendar.nav.bottomRight.scrollTop;
                calendar.nav.upperRight.scrollLeft = calendar.nav.bottomRight.scrollLeft;
            }
            
            var scroll = scrolling ? calendar.nav.bottomRight : calendar.nav.scroll;

            calendar.scrollPos = scroll.scrollTop;
            calendar.scrollHeight = scroll.clientHeight;
            calendar.nav.scrollpos.value = calendar.scrollPos;
            calendar._updateScrollLabels();
        };


        this._updateScrollLabels = function() {
            if (!this.scrollLabelsVisible) {
                return;
            }

            if (!this.scrollLabels) {
                return;
            }

            // update horizontal position
            var columns = this.columnsBottom;
            var hoursWidth = (this.showHours ? this.hourWidth : 0);
            //var colWidth = (this.nav.scroll.clientWidth - hoursWidth) / columns.length;
            var colWidth = this.nav.main.rows[0].cells[0].clientWidth;
            var iw = 10;
            var offset = 1;

            for (var i = 0; i < columns.length; i++) {
                var scrollUp = this.nav.scrollUp[i];
                var scrollDown = this.nav.scrollDown[i];
                //scrollUp.setAttribute("coords", "hoursWidth:" + hoursWidth + " colWidth:" + colWidth);
                scrollUp.style.left = (hoursWidth + i * colWidth + colWidth / 2 - (iw / 2) + offset) + "px";
                scrollDown.style.left = (hoursWidth + i * colWidth + colWidth / 2 - (iw / 2) + offset) + "px";
            }

            var hiddenPixels = this._autoHiddenPixels();

            // update vertical position
            for (var i = 0; i < this.nav.scrollUp.length; i++) {
                var up = this.nav.scrollUp[i];
                var down = this.nav.scrollDown[i];
                var minEnd = this.scrollLabels[i].minEnd - hiddenPixels;
                var maxStart = this.scrollLabels[i].maxStart - hiddenPixels;

                if (up && down) {
                    if (minEnd <= calendar.scrollPos) {
                        up.style.top = (this._totalHeaderHeight() + 2) + "px";
                        up.style.display = '';
                    }
                    else {
                        up.style.display = 'none';
                    }

                    if (maxStart >= calendar.scrollPos + calendar.scrollHeight) {
                        // scrollHeight is updated on scrolling
                        down.style.top = (this._totalHeaderHeight() + this.scrollHeight - 8) + "px";
                        down.style.display = '';
                    }
                    else {
                        down.style.display = 'none';
                    }
                }
            }
        };

        this._createEdit = function(object) {
            var parentTd = object.parentNode;
            while (parentTd && parentTd.tagName !== "TD") {
                parentTd = parentTd.parentNode;
            }

            var edit = document.createElement('textarea');
            edit.style.position = 'absolute';
            edit.style.width = (object.parentNode.offsetWidth - 2) + 'px';
            edit.style.height = (object.offsetHeight - 2) + 'px'; //offsetHeight

            var fontFamily = DayPilot.gs(object, 'fontFamily');
            if (!fontFamily) fontFamily = DayPilot.gs(object, 'font-family');
            edit.style.fontFamily = fontFamily;

            var fontSize = DayPilot.gs(object, 'fontSize');
            if (!fontSize) fontSize = DayPilot.gs(object, 'font-size');
            edit.style.fontSize = fontSize;

            edit.style.left = '0px';
            edit.style.top = object.offsetTop + 'px';
            edit.style.border = '1px solid black';
            edit.style.padding = '0px';
            edit.style.marginTop = '0px';
            edit.style.backgroundColor = 'white';
            edit.value = DayPilot.tr(object.event.text());

            edit.event = object.event;
            parentTd.firstChild.appendChild(edit);
            return edit;
        };

/*
        this._clearEventSelection = function() {
            calendar.multiselect.divDeselectAll();
            calendar.multiselect.clear(true);
        };

        this.cleanEventSelection = this.clearEventSelection;
*/
        this._eventSelect = function(div, e, ctrlKey) {
            /*
            var m = calendar.multiselect;
            var e = div.event;
            m._previous = m.events();
            m._toggleDiv(div, ctrlKey);
            var change = m.isSelected(e) ? "selected" : "deselected";*/
            calendar._eventSelectDispatch(div, e, ctrlKey);
        };

        // internal methods for handling event selection
        this.multiselect = {};

        this.multiselect._initList = [];
        this.multiselect._list = [];
        this.multiselect._divs = [];
        this.multiselect._previous = []; // not used at the moment

        this.multiselect._serialize = function() {
            var m = calendar.multiselect;
            return DayPilot.JSON.stringify(m.events());
        };

        this.multiselect.events = function() {
            var m = calendar.multiselect;
            var events = [];
            events.ignoreToJSON = true;
            for (var i = 0; i < m._list.length; i++) {
                events.push(m._list[i]);
            }
            return events;
        };

        this.multiselect._updateHidden = function() {
            var h = calendar.nav.select;
            h.value = calendar.multiselect._serialize();
        };

        this.multiselect._toggleDiv = function(div, ctrl) {
            var m = calendar.multiselect;
            if (m.isSelected(div.event)) {
                if (calendar.allowMultiSelect) {
                    if (ctrl) {
                        m.remove(div.event, true);
                    }
                    else {
                        var count = m._list.length;
                        m.clear(true);
                        if (count > 1) {
                            m.add(div.event, true);
                        }

                    }
                }
                else { // clear all
                    m.clear(true);
                }
            }
            else {
                if (calendar.allowMultiSelect) {
                    if (ctrl) {
                        m.add(div.event, true);
                    }
                    else {
                        m.clear(true);
                        m.add(div.event, true);
                    }
                }
                else {
                    m.clear(true);
                    m.add(div.event, true);
                }
            }
            m.redraw();
            m._updateHidden();
        };

        // compare event with the init select list
        this.multiselect._shouldBeSelected = function(ev) {
            var m = calendar.multiselect;
            return m._isInList(ev, m._initList);
        };

        this.multiselect._alert = function() {
            var m = calendar.multiselect;
            var list = [];
            for (var i = 0; i < m._list.length; i++) {
                var event = m._list[i];
                list.push(event.value());
            }
            alert(list.join("\n"));
        };

        this.multiselect.add = function(ev, dontRedraw) {
            var m = calendar.multiselect;
            if (m._indexOf(ev) === -1) {
                m._list.push(ev);
            }
            m._updateHidden();
            if (dontRedraw) {
                return;
            }
            m.redraw();
        };

        this.multiselect.remove = function(ev, dontRedraw) {
            var m = calendar.multiselect;
            var i = m._indexOf(ev);
            if (i !== -1) {
                m._list.splice(i, 1);
            }
            m._updateHidden();

            if (dontRedraw) {
                return;
            }
            m.redraw();
        };

        this.multiselect.clear = function(dontRedraw) {
            var m = calendar.multiselect;
            m._list = [];

            m._updateHidden();

            if (dontRedraw) {
                return;
            }
            m.redraw();
        };

        this.multiselect.redraw = function() {
            var m = calendar.multiselect;
            for (var i = 0; i < calendar.elements.events.length; i++) {
                var div = calendar.elements.events[i];
                if (m.isSelected(div.event)) {
                    m._divSelect(div);
                }
                else {
                    m._divDeselect(div);
                }
            }
        };

        this.multiselect._divSelect = function(div) {
            var m = calendar.multiselect;
            var cn = calendar.cssOnly ? calendar._prefixCssClass("_selected") : calendar._prefixCssClass("selected");
            var c = m._findContentDiv(div);
            DayPilot.Util.addClass(c, cn);
            if (calendar.useEventSelectionBars) {
                m._divSelectTraditional(div);
            }
            m._divs.push(div);
        };

        this.multiselect._findContentDiv = function(div) {
            if (calendar.cssOnly) {
                return div;
            }
            for (var i = 0; i < div.childNodes.length; i++) {
                var c = div.childNodes[i];
                if (c.getAttribute("c") === "1") {  // hack
                    return c;
                }
            }
            return null;
        };

        this.multiselect._divDeselectAll = function() {
            var m = calendar.multiselect;
            for (var i = 0; i < m._divs.length; i++) {
                var div = m._divs[i];
                m._divDeselect(div, true);
            }
            m._divs = [];
        };

        this.multiselect._divDeselect = function(div, dontRemoveFromCache) {
            var m = calendar.multiselect;
            var cn = calendar.cssOnly ? calendar._prefixCssClass("_selected") : calendar._prefixCssClass("selected");
            var c = m._findContentDiv(div);
            DayPilot.Util.removeClass(c, cn);

            if (calendar.useEventSelectionBars) {
                m._divDeselectTraditional(div);
            }

            if (dontRemoveFromCache) {
                return;
            }
            var i = DayPilot.indexOf(m._divs, div);
            if (i !== -1) {
                m._divs.splice(i, 1);
            }

        };

        this.multiselect.isSelected = function(ev) {
            return calendar.multiselect._isInList(ev, calendar.multiselect._list);
        };

        this.multiselect._indexOf = function(ev) {
            return DayPilot.indexOf(calendar.multiselect._list, ev);
        };

        this.multiselect._isInList = function(e, list) {
            if (!list) {
                return false;
            }
            for (var i = 0; i < list.length; i++) {
                var ei = list[i];
                if (e === ei) {
                    return true;
                }
                if (typeof ei.value === 'function') {
                    if (ei.value() !== null && e.value() !== null && ei.value() === e.value()) {
                        return true;
                    }
                    if (ei.value() === null && e.value() === null && ei.recurrentMasterId() === e.recurrentMasterId() && e.start().toStringSortable() === ei.start()) {
                        return true;
                    }
                }
                else {
                    if (ei.value !== null && e.value() !== null && ei.value === e.value()) {
                        return true;
                    }
                    if (ei.value === null && e.value() === null && ei.recurrentMasterId === e.recurrentMasterId() && e.start().toStringSortable() === ei.start) {
                        return true;
                    }
                }

            }

            return false;
        };

        this.multiselect._divSelectTraditional = function(obj) {
            var w = 5;

            if (!obj.top) {
                var top = document.createElement("div");
                top.setAttribute("unselectable", "on");
                top.style.position = 'absolute';
                top.style.left = obj.offsetLeft + 'px';
                top.style.width = obj.offsetWidth + 'px';
                top.style.top = (obj.offsetTop - w) + 'px';
                top.style.height = w + 'px';
                top.style.backgroundColor = calendar.eventSelectColor;
                top.style.zIndex = 100;
                obj.parentNode.appendChild(top);
                obj.top = top;
            }

            if (!obj.bottom) {
                var bottom = document.createElement("div");
                bottom.setAttribute("unselectable", "on");
                bottom.style.position = 'absolute';
                bottom.style.left = obj.offsetLeft + 'px';
                bottom.style.width = obj.offsetWidth + 'px';
                bottom.style.top = (obj.offsetTop + obj.offsetHeight) + 'px';
                bottom.style.height = w + 'px';
                bottom.style.backgroundColor = calendar.eventSelectColor;
                bottom.style.zIndex = 100;
                obj.parentNode.appendChild(bottom);
                obj.bottom = bottom;
            }

        };

        this.multiselect._divDeselectTraditional = function(obj) {
            if (obj.top) {
                obj.parentNode.removeChild(obj.top);
                obj.top = null;
            }
            if (obj.bottom) {
                obj.parentNode.removeChild(obj.bottom);
                obj.bottom = null;
            }
        };

/*
        this.selectedEvent = function() {
            var a = calendar.multiselect._list;
            if (a.length <= 0) {
                return null;
            }
            if (a.length === 1) {
                return a[0];
            }
            //multiselect not supported yet
            return null;
        };
        */

        this._divEdit = function(object) {
            if (DayPilotCalendar.editing) {
                DayPilotCalendar.editing.blur();
                return;
            }

            var edit = this._createEdit(object);
            DayPilotCalendar.editing = edit;

            edit.onblur = function() {
                var id = object.event.value();
                var tag = object.event.tag();
                var oldText = object.event.text();
                var newText = edit.value;

                DayPilotCalendar.editing = null;
                edit.parentNode.removeChild(edit);

                if (oldText === newText) {
                    return;
                }

                object.style.display = 'none';
                calendar._eventEditDispatch(object.event, newText);
            };

            edit.onkeypress = function(e) {
                var keynum = (window.event) ? event.keyCode : e.keyCode;

                if (keynum === 13) {
                    this.onblur();
                    return false;
                }
                else if (keynum === 27) {
                    edit.parentNode.removeChild(edit);
                    DayPilotCalendar.editing = false;
                }

                return true;
            };

            edit.select();
            edit.focus();
        };

        this._prepareColumns = function() {
            if (!this.columns) {
                this.columnsBottom = this._createDaysViewColumns();
                this._activateColumnCollection(this.columnsBottom);
            }
            else {
                this._activateColumnCollection(this.columns);
                this.columnsBottom = this._getColumns(this.headerLevels, true);
            }
        };
        
        this._getVisibleRange = function() {
            var start = this.startDate.getDatePart();
            var days = this.days;

            switch (this.viewType) {
                case "Day":
                    days = 1;
                    break;
                case "Week":
                    days = 7;
                    // TODO let weekStarts property override it?
                    start = start.firstDayOfWeek(resolved.weekStarts());
                    break;
                case "WorkWeek":
                    days = 5;
                    start = start.firstDayOfWeek(1); // Monday
                    break;
            }
            
            var end = start.addDays(days);
            
            var result = {};
            result.start = start;
            result.end = end;
            result.days = days;
            
            return result;
        };


        this._createDaysViewColumns = function() {
            var columns = [];

            var visible = this._getVisibleRange();
            var start = visible.start;
            var days = visible.days;

            if (this.heightSpec === 'BusinessHoursNoScroll') {
                start = start.addHours(this.businessBeginsHour);
            }

            for (var i = 0; i < days; i++) {

                var column = {};
                column.start = start.addDays(i);
                column.name = column.start.toString(resolved.locale().datePattern);
                column.html = column.name;

                columns.push(column);
            }
            
            return columns;
        };

        this._activateColumn = function(column) {
            if (column.Start) {  // detect the old mode
                /*
                 * Value
                 * Name
                 * Start
                 * 
                 * InnerHTML
                 * ToolTip
                 * BackColor
                 * Areas
                 * Children
                 */
                column.id = column.Value;
                column.start = column.Start;
                column.name = column.Name;
                
                column.html = column.InnerHTML;
                column.toolTip = column.ToolTip;
                column.backColor = column.BackColor;
                
                column.areas = column.Areas;
                column.children = column.Children;
                
                delete column.Value;
                delete column.Start;
                delete column.Name;
                delete column.InnerHTML;
                delete column.ToolTip;
                delete column.BackColor;
                delete column.Areas;
                delete column.Children;
            }
            
            column.start = column.start || calendar.startDate; // use default value
            column.start = new DayPilot.Date(column.start);
            column.html = column.html || column.name;

            column.getChildren = function(level, inherit) {
                var list = [];
                if (level <= 1) {
                    list.push(this);
                    return list;
                }

                if (!this.children || this.children.length === 0) {
                    if (inherit) {
                        list.push(this);
                    }
                    else {
                        list.push("empty");
                    }
                    return list;
                }

                for (var i = 0; i < this.children.length; i++) {
                    var child = this.children[i];
                    var subChildren = child.getChildren(level - 1, inherit);

                    for (var j = 0; j < subChildren.length; j++) {
                        list.push(subChildren[j]);
                    }
                }

                return list;

            };

            column.getChildrenCount = function(level) {
                var count = 0;

                if (!this.children || this.children.length <= 0 || level <= 1) {
                    return 1;
                }

                for (var i = 0; i < this.children.length; i++) {
                    count += this.children[i].getChildrenCount(level - 1);
                }

                return count;

            };

            column.putIntoBlock = function(ep) {

                for (var i = 0; i < this.blocks.length; i++) {
                    var block = this.blocks[i];
                    if (block.overlapsWith(ep.part.top, ep.part.height)) {
                        //block.putIntoLine(ep);
                        block.events.push(ep);
                        block.min = Math.min(block.min, ep.part.top);
                        block.max = Math.max(block.max, ep.part.top + ep.part.height);
                        return i;
                    }
                }

                // no suitable block found, create a new one
                var block = [];
                block.lines = [];
                block.events = [];

                block.overlapsWith = function(start, width) {
                    var end = start + width - 1;

                    if (!(end < this.min || start > this.max - 1)) {
                        return true;
                    }

                    return false;
                };
                block.putIntoLine = function(ep) {
                    var thisCol = this;

                    for (var i = 0; i < this.lines.length; i++) {
                        var line = this.lines[i];
                        if (line.isFree(ep.part.top, ep.part.height)) {
                            line.push(ep);
                            return i;
                        }
                    }

                    var line = [];
                    line.isFree = function(start, width) {
                        //var free = true;
                        var end = start + width - 1;
                        var max = this.length;

                        for (var i = 0; i < max; i++) {
                            var e = this[i];
                            if (!(end < e.part.top || start > e.part.top + e.part.height - 1)) {
                                return false;
                            }
                        }

                        return true;
                    };

                    line.push(ep);

                    this.lines.push(line);

                    return this.lines.length - 1;

                };

                //block.putIntoLine(ep);
                block.events.push(ep);
                block.min = ep.part.top;
                block.max = ep.part.top + ep.part.height;

                this.blocks.push(block);

                return this.blocks.length - 1;

            };

            column.putIntoLine = function(ep) {
                var thisCol = this;

                for (var i = 0; i < this.lines.length; i++) {
                    var line = this.lines[i];
                    if (line.isFree(ep.part.top, ep.part.height)) {
                        line.push(ep);
                        return i;
                    }
                }

                var line = [];
                line.isFree = function(start, width) {
                    //var free = true;
                    var end = start + width - 1;
                    var max = this.length;

                    for (var i = 0; i < max; i++) {
                        var e = this[i];
                        if (!(end < e.part.top || start > e.part.top + e.part.height - 1)) {
                            return false;
                        }
                    }

                    return true;
                };

                line.push(ep);

                this.lines.push(line);

                return this.lines.length - 1;
            };

            if (column.children) {
                this._activateColumnCollection(column.children);
            }
        };

        this._activateColumnCollection = function(cc) {

            for (var i = 0; i < cc.length; i++) {
                this._activateColumn(cc[i]);
            }


            /*
            cc.getColumnCount = function(level) {
                var count = 0;

                for (var i = 0; i < this.length; i++) {
                    count += this[i].getChildrenCount(level);
                }

                return count;
            };
            */
        };
        
        this._getColumns = function(level, inherit) {
            var source = this.columns || this.columnsBottom;
            var list = [];

            for (var i = 0; i < source.length; i++) {
                var children = source[i].getChildren(level, inherit);
                for (var j = 0; j < children.length; j++) {
                    list.push(children[j]);
                }
                //list.concat(children);
            }
            return list;
            
        };

        this._drawEventsAllDay = function() {
            if (!this.showAllDayEvents) {
                return;
            }

            var header = this.nav.header;
            
            if (!header) {
                return;
            }
            
            header.style.display = 'none';

            var columns = this.columnsBottom.length;

            for (var j = 0; j < this.allDay.lines.length; j++) {
                var line = this.allDay.lines[j];

                for (var i = 0; i < line.length; i++) {
                    //var data = this.eventsAllDay[i];
                    var data = line[i];

                    var div = document.createElement("div");
                    div.event = data;

                    div.setAttribute("unselectable", "on");
                    div.style.position = 'absolute';

                    if (calendar.rtl) {
                        div.style.right = (100.0 * data.part.colStart / columns) + "%";
                    }
                    else {
                        div.style.left = (100.0 * data.part.colStart / columns) + "%";
                    }
                    div.style.width = (100.0 * data.part.colWidth / columns) + "%";
                    div.style.height = resolved.allDayEventHeight() + 'px';
                    if (!this.cssOnly) {
                        div.style.top = (3 + this.headerLevels * resolved.headerHeight() + j * (resolved.allDayEventHeight() + 2)) + "px";
                    }
                    else {
                        div.className = this._prefixCssClass("_alldayevent");
                        div.style.top = (this.headerLevels * resolved.headerHeight() + j * (resolved.allDayEventHeight())) + "px";
                    }

                    // prevention of global alignment changes                
                    div.style.textAlign = 'left';
                    div.style.lineHeight = "1.2";

                    if (data.client.clickEnabled()) {
                        div.onclick = this._eventClickDispatch;
                    }
                    if (data.client.doubleClickEnabled()) {
                        div.ondblclick = this._eventDoubleClickDispatch;
                    }

                    div.oncontextmenu = this._eventRightClickDispatch;
                    div.onmousemove = function(ev) {
                        var div = this;
                        if (!div.active) {
                            if (calendar.cssOnly) {
                                DayPilot.Util.addClass(div, calendar._prefixCssClass("_alldayevent_hover"));
                            }
                            DayPilot.Areas.showAreas(div, this.event);
                        }
                        
                        if (typeof (DayPilotBubble) !== 'undefined' && calendar.bubble && calendar.eventHoverHandling !== 'Disabled') {
                            calendar.bubble.showEvent(this.event);
                        }
                    };
                    div.onmouseout = function(ev) {
                        var div = this;
                        if (calendar.cssOnly) {
                            DayPilot.Util.removeClass(div, calendar._prefixCssClass("_alldayevent_hover"));
                        }
                        DayPilot.Areas.hideAreas(this, ev);
                        if (calendar.bubble) {
                            calendar.bubble.hideOnMouseOut();
                        }
                    };


                    if (this.showToolTip && !this.bubble) {
                        div.title = data.client.toolTip();
                    }

                    var startsHere = data.start().getTime() === data.part.start.getTime();
                    var endsHere = data.end().getTime() === data.part.end.getTime();

                    var back = data.data.backColor;

                    if (!this.cssOnly) {

                        var inner = document.createElement("div");
                        inner.setAttribute("unselectable", "on");
                        inner.style.marginLeft = '2px';
                        inner.style.marginRight = '3px';
                        inner.style.paddingLeft = '2px';
                        //inner.style.paddingRight = '1px';
                        inner.style.height = (resolved.allDayEventHeight() - 2) + 'px';
                        inner.style.border = '1px solid ' + this.allDayEventBorderColor;
                        inner.style.overflow = 'hidden';
                        inner.style.position = 'relative';
                        inner.style.backgroundColor = back;
                        inner.className = this._prefixCssClass("alldayevent");
                        //inner.style.backgroundColor = "red";

                        if (this.roundedCorners) {
                            inner.style.MozBorderRadius = "5px";
                            inner.style.webkitBorderRadius = "5px";
                            inner.style.borderRadius = "5px";
                        }

                        var inside = [];

                        // display properties
                        var textOnTop = true;
                        var textLeft = this.showAllDayEventStartEnd;
                        var textRight = this.showAllDayEventStartEnd;
                        var textAlign = "Center";
                        var textIndent = 0;

                        // left
                        if (textLeft) {
                            if (textAlign === 'Left') {
                                inside.push("<div unselectable='on' style='position:absolute;text-align:left;height:1px;font-size:1px;width:100%'><div unselectable='on' style='font-size:8pt;color:gray;text-align:right;");
                                inside.push("width:");
                                inside.push(textIndent - 4);
                                inside.push("px;");
                                inside.push("><span style='background-color:");
                            }
                            else {
                                inside.push("<div unselectable='on' style='position:absolute;text-align:left;height:1px;font-size:1px;width:100%'><div unselectable='on' style='font-size:8pt;color:gray'><span style='background-color:");
                            }

                            //inside.push("<div unselectable='on' style='position:absolute;text-align:left;height:1px;font-size:1px;width:100%'><div unselectable='on' style='font-size:8pt;color:gray'><span style='background-color:");
                            //inside.push(back);
                            inside.push('transparent');
                            inside.push("' unselectable='on'>");
                            if (startsHere) {
                                if (data.start().getDatePart().getTime() !== data.start().getTime()) {
                                    inside.push(DayPilot.Date.hours(data.start().d, this._resolved.timeFormat() === 'Clock12Hours'));
                                }
                            }
                            else {
                                inside.push("~");
                            }
                            inside.push("</span></div></div>");
                        }

                        // right
                        //if (textRight && (DayPilot.Date.getTime(ev.End) != 0 || !eventPart.endsHere)) {
                        if (textRight) {
                            inside.push("<div unselectable='on' style='position:absolute;text-align:right;height:1px;font-size:1px;width:100%'><div unselectable='on' style='margin-right:4px;font-size:8pt;color:gray'><span style='background-color:");
                            //inside.push(back);
                            inside.push('transparent');
                            inside.push("' unselectable='on'>");
                            if (endsHere) {
                                if (data.end().getDatePart().getTime() !== data.end().getTime()) {
                                    inside.push(DayPilot.Date.hours(data.end().d, this._resolved.timeFormat() === 'Clock12Hours'));
                                }
                            }
                            else {
                                inside.push("~");
                            }
                            inside.push("</span></div></div>");
                        }


                        // fix box
                        if (textAlign === 'Left') {
                            var left = textLeft ? textIndent : 0;
                            inside.push("<div style='margin-top:0px;height:");
                            inside.push(resolved.allDayEventHeight() - 2);
                            inside.push("px;");
                            inside.push(";overflow:hidden;text-align:left;padding-left:");
                            inside.push(left);
                            inside.push("px;font-size:");
                            inside.push(this.allDayEventFontSize);
                            inside.push(";color:");
                            inside.push(this.allDayEventFontColor);
                            inside.push(";font-family:");
                            inside.push(this.eventFontFamily);
                            inside.push("' unselectable='on'>");
                            if (data.client.innerHTML()) {
                                inside.push(data.client.innerHTML());
                            }
                            else {
                                inside.push(data.text());
                            }
                            inside.push("</div>");
                        }
                        else if (textAlign === 'Center') {
                            if (textOnTop) {

                                // alternate elements order: text on top
                                inside.push("<div style='position:absolute; text-align:center; width: 98%; height:1px;'>");
                                inside.push("<span style='background-color:");
                                //inside.push(back);
                                inside.push('transparent');
                                inside.push(";font-size:");
                                inside.push(this.allDayEventFontSize);
                                inside.push(";color:");
                                inside.push(this.allDayEventFontColor);
                                inside.push(";font-family:");
                                inside.push(this.allDayEventFontFamily);
                                inside.push("' unselectable='on'>");

                                if (data.client.innerHTML()) {
                                    inside.push(data.client.innerHTML());
                                }
                                else {
                                    inside.push(data.text());
                                }

                                inside.push("</span>");
                                inside.push("</div>");
                            }
                            else {
                                inside.push("<div style='margin-top:0px;height:");
                                inside.push(resolved.allDayEventHeight() - 2);
                                inside.push("px;");
                                inside.push(";overflow:hidden;text-align:center;font-size:");
                                inside.push(this.allDayEventFontSize);
                                inside.push(";color:");
                                inside.push(this.allDayEventFontColor);
                                inside.push(";font-family:");
                                inside.push(this.allDayEventFontFamily);
                                inside.push("' unselectable='on'>");
                                if (data.client.innerHTML()) {
                                    inside.push(data.client.innerHTML());
                                }
                                else {
                                    inside.push(data.text());
                                }
                                inside.push("</div>");

                            }
                        }

                        inner.innerHTML = inside.join('');
                        div.appendChild(inner);
                    }
                    else {

                        var inner = document.createElement("div");
                        inner.setAttribute("unselectable", "on");
                        inner.className = this._prefixCssClass("_alldayevent_inner");
                        
                        if (back) {
                            inner.style.background = back;
                        }

                        if (calendar.rtl) {
                            if (!startsHere) {
                                DayPilot.Util.addClass(div, this._prefixCssClass("_alldayevent_continueright"));
                            }
                            if (!endsHere) {
                                DayPilot.Util.addClass(div, this._prefixCssClass("_alldayevent_continueleft"));
                            }
                        }
                        else {
                            if (!startsHere) {
                                DayPilot.Util.addClass(div, this._prefixCssClass("_alldayevent_continueleft"));
                            }
                            if (!endsHere) {
                                DayPilot.Util.addClass(div, this._prefixCssClass("_alldayevent_continueright"));
                            }
                        }

                        if (data.client.innerHTML()) {
                            inner.innerHTML = data.client.innerHTML();
                        }
                        else {
                            inner.innerHTML = data.text();
                        }
                        div.appendChild(inner);
                    }

                    if (calendar._api2()) {
                        if (typeof calendar.onAfterEventRender === 'function') {
                            var args = {};
                            args.e = div.event;
                            args.div = div;

                            calendar.onAfterEventRender(args);
                        }
                    }
                    else {
                        if (calendar.afterEventRender) {
                            calendar.afterEventRender(div.event, div);
                        }
                    }

                    this.nav.allday.appendChild(div);

                    //new DayPilot.Event(div, calendar);
                    this.elements.events.push(div);
                }
            }

            header.style.display = '';
        };

        this._deleteEvents = function(allDayOnly) {
            calendar.multiselect._divDeselectAll();

            //var start = new Date();
            if (this.elements.events) {
                //DayPilot.pu(maind);

                for (var i = 0; i < this.elements.events.length; i++) {
                    var div = this.elements.events[i];

                    var object = div.event;

                    if (object && allDayOnly && !object.allday()) {
                        continue;
                    }

                    if (object) {
                        object.div = null;
                        object.root = null;
                    }

                    div.onclick = null;
                    div.onclickSave = null;
                    div.ondblclick = null;
                    div.oncontextmenu = null;
                    div.onmouseover = null;
                    div.onmouseout = null;
                    div.onmousemove = null;
                    div.onmousedown = null;

                    if (div.firstChild && div.firstChild.firstChild && div.firstChild.firstChild.tagName && div.firstChild.firstChild.tagName.toUpperCase() === 'IMG') {
                        var img = div.firstChild.firstChild;
                        img.onmousedown = null;
                        img.onmousemove = null;
                        img.onclick = null;

                    }

                    div.helper = null;
                    div.event = null;

                    DayPilot.de(div);
                }
            }
            //this.events.innerHTML = '';

            this.elements.events = [];

        };

        this._drawEvent = function(data) {
            var main = this.nav.events;

            var rounded = this.roundedCorners;
            var radius = this.roundedCorners && (this._browser.ff || this._browser.opera105 || this._browser.webkit522 || !this._browser.ielt9);
            var pixels = this.roundedCorners && !radius;
            
            var cache = data.cache || data.data;

            var borderColor = cache.borderColor || this.eventBorderColor;

            var div = document.createElement("div");
            //div.data = data;
            div.setAttribute("unselectable", "on");
            div.style.MozUserSelect = 'none';
            div.style.KhtmlUserSelect = 'none';
            div.style.WebkitUserSelect = 'none';
            div.style.position = 'absolute';
            if (!this.cssOnly) {
                div.style.fontFamily = this.eventFontFamily;
                div.style.fontSize = this.eventFontSize;
                div.style.color = cache.fontColor || this.eventFontColor;
                if (!rounded) {
                    div.style.backgroundColor = borderColor;
                }
                if (this.transparent) {
                    div.style.opacity = 0.6;
                    div.style.filter = "alpha(opacity=60)";
                }
            }
            else {
                div.className = this._prefixCssClass("_event");
            }
            div.style.left = data.part.left + '%';
            div.style.top = (data.part.top - this._autoHiddenPixels()) + 'px';
            div.style.width = data.part.width + '%';
            div.style.height = Math.max(data.part.height, 2) + 'px';

            div.style.overflow = 'hidden';

            div.isFirst = data.part.start.getTime() === data.start().getTime();
            div.isLast = data.part.end.getTime() === data.end().getTime();

            if (data.client.clickEnabled()) {
                div.onclick = this._eventClickDispatch;
            }
            if (data.client.doubleClickEnabled()) {
                div.ondblclick = this._eventDoubleClickDispatch;
            }
            div.oncontextmenu = this._eventRightClickDispatch;
            div.onmousemove = this._onEventMouseMove;
            div.onmouseout = this._onEventMouseOut;
            div.onmousedown = this._onEventMouseDown;
            
            div.ontouchstart = this._touch.onEventTouchStart;
            div.ontouchmove = this._touch.onEventTouchMove;
            div.ontouchend = this._touch.onEventTouchEnd;

            // inner divs
            var inside = [];

            if (!this.cssOnly) {
                if (this.eventDeleteHandling !== 'Disabled' && data.client.deleteEnabled()) {
                    inside.push("<div unselectable='on' style='position:absolute; width:100%;text-align:right;'><div style='position:absolute; width:10px; height:10px; right:2px; top: 2px; cursor:pointer;");
                    if (this.deleteImageUrl) {
                        inside.push("background-image:url(\"" + this.deleteImageUrl + "\");");
                    }
                    inside.push("' class='");
                    inside.push(this._prefixCssClass("event_delete"));
                    inside.push("' onmousemove=\"if (typeof(DayPilotBubble) !== 'undefined' && ");
                    inside.push(this.clientName);
                    inside.push(".bubble && ");
                    inside.push(this.clientName);
                    inside.push(".bubble.hideAfter > 0");
                    inside.push(") { DayPilotBubble.hideActive(); event.cancelBubble = true; }\" onmousedown=\"this.parentNode.parentNode.style.cursor='default';\" onclick='");
                    inside.push(this.clientName);
                    inside.push(".internal.eventDeleteDispatch(this); event.cancelBubble = true; if (event.stopPropagation) event.stopPropagation();' ></div></div>");
                }

                if (pixels) {
                    // top line
                    inside.push("<div style='margin-right:2px;'>");
                    inside.push("<div style='height:1px;line-height:1px;font-size:0px; margin-left:2px; background-color:");
                    inside.push(borderColor);
                    inside.push(";'>&nbsp;</div>");
                    inside.push("</div>");

                    // wrapper
                    inside.push("<div unselectable='on' style='position:absolute;width:100%;margin-top:-1px;'>");

                    inside.push("<div style='height:1px;line-height:1px;font-size:0px;margin-left:1px;margin-top:1px; margin-right:1px;border-right:1px solid ");
                    inside.push(borderColor);
                    inside.push(";border-left:1px solid ");
                    inside.push(borderColor);
                    inside.push(";background-color:");
                    inside.push(data.client.header() ? borderColor : data.BackgroundColor);
                    inside.push("'>");
                    inside.push("&nbsp;</div>");

                    inside.push("</div>");

                }
                else if (!radius) {
                    inside.push("<div style='height:1px;line-height:1px;font-size:0px; width:1px;'>&nbsp;</div>");
                }

                // fix box
                inside.push("<div");

                if (this.showToolTip && !this.bubble) {
                    inside.push(" title='");
                    inside.push(data.client.toolTip().replace(/'/g, "&apos;"));
                    inside.push("'");
                }

                var height = Math.max(data.part.height - 2, 0);

                inside.push(" c='1'"); // hack for multiselect._findContentDiv()

                inside.push(" class='");
                inside.push(cache.cssClass || this._prefixCssClass('event'));
                inside.push("'");
                if (pixels) {
                    inside.push(" style='margin-top:1px;height:");
                    inside.push(height - 2);
                }
                else {
                    inside.push(" style='margin-top:0px;height:");
                    inside.push(height);
                }
                inside.push("px;background-color:");
                inside.push(data.client.backColor());
                if (radius) {
                    inside.push(";border:1px solid ");
                    inside.push(borderColor);
                    inside.push(";-moz-border-radius:5px;");
                    inside.push(";-webkit-border-radius:5px;");
                    inside.push(";border-radius:5px;");
                }
                else {
                    inside.push(";border-left:1px solid ");
                    inside.push(borderColor);
                    inside.push(";border-right:1px solid ");
                    inside.push(borderColor);
                }
                //inside.push(";overflow:hidden;");
                inside.push(";");
                if (data.data.backgroundImage) {
                    inside.push("background-image:url(");
                    inside.push(data.data.backgroundImage);
                    inside.push(");");
                    if (data.data.backgroundRepeat) {
                        inside.push("background-repeat:");
                        inside.push(data.data.backgroundRepeat);
                        inside.push(";");
                    }
                }
                if (calendar.rtl) {
                    inside.push("direction:rtl;");
                }
                inside.push("' unselectable='on'>");

                if (this.durationBarVisible) {
                    var barColor = data.client.barColor() || calendar.durationBarColor;
                    // white space top
                    inside.push("<div style='position:absolute;left:1px;top:1px;width:");
                    inside.push(calendar.durationBarWidth - 1);
                    inside.push("px;height:");
                    inside.push(data.part.barTop);
                    inside.push("px;background-color:white;font-size:1px' unselectable='on'></div>");

                    // white space bottom
                    inside.push("<div style='position:absolute;left:1px;top:");
                    inside.push(data.part.barTop + data.part.barHeight);
                    inside.push("px;width:");
                    inside.push(calendar.durationBarWidth - 1);
                    inside.push("px;height:");
                    inside.push(height - (data.part.barTop + data.part.barHeight));
                    inside.push("px;background-color:white;font-size:1px' unselectable='on'></div>");

                    // duration bar
                    inside.push("<div style='position:absolute;left:1px;width:");
                    inside.push(calendar.durationBarWidth);
                    inside.push("px;height:");
                    inside.push(data.part.barHeight);
                    inside.push("px;");

                    if (data.data.durationBarImageUrl) {
                        inside.push("background-image:url(");
                        inside.push(data.data.durationBarImageUrl);
                        inside.push(");");
                    }
                    else if (calendar.durationBarImageUrl) {
                        inside.push("background-image:url(");
                        inside.push(calendar.durationBarImageUrl);
                        inside.push(");");
                    }
                    inside.push("top:");
                    inside.push(data.part.barTop + 1);
                    inside.push("px;background-color:");
                    inside.push(barColor);
                    inside.push(";font-size:1px' unselectable='on'></div><div style='position:absolute;left:");
                    inside.push(calendar.durationBarWidth);
                    inside.push("px;top:1px;width:1px;background-color:");
                    inside.push(borderColor);
                    inside.push(";height:100%' unselectable='on'></div>");

                }

                var headerHeight = data.client.header() ? this.eventHeaderHeight : 0;

                if (data.client.header()) {
                    inside.push("<div unselectable='on' style='overflow:hidden;height:");
                    inside.push(this.eventHeaderHeight);
                    inside.push("px; background-color:");
                    inside.push(borderColor);
                    inside.push(";font-size:");
                    inside.push(this.eventHeaderFontSize);
                    inside.push(";color:");
                    inside.push(this.eventHeaderFontColor);
                    inside.push("'>");
                    inside.push(data.client.header());
                    inside.push("</div>");
                }

                // space - TODO replace?
                //inside.push("<div style='float:left;width:2px;height:100%' unselectable='on'></div>");
                if (this.durationBarVisible) {
                    //inside.push("<div unselectable='on' style='overflow:hidden;padding-left:");
                    inside.push("<div unselectable='on' style='padding-left:");
                    inside.push(calendar.durationBarWidth + 3);
                    inside.push("px;");
                }
                else {
                    inside.push("<div unselectable='on' style='overflow:hidden;padding-left:2px;height:");
                    inside.push(height - headerHeight - 1);
                    inside.push("px;");
                }
                inside.push("'>");
                inside.push(data.client.innerHTML());
                inside.push("</div></div>");

                if (pixels) {
                    // bottom line
                    inside.push("<div unselectable='on' style='margin-right:2px;'>");
                    inside.push("<div unselectable='on' style='height:1px;line-height:1px;font-size:0px;margin-left:2px;margin-top:1px;background-color:");
                    inside.push(borderColor);
                    inside.push(";'><!-- --></div>");
                    inside.push("</div>");

                    // wrapper
                    inside.push("<div unselectable='on' style='margin-right:0px;margin-top:-3px;position:relative;'>");

                    // lower-right corner
                    // wrapper
                    inside.push("<div unselectable='on' style='margin-right:0px;position:relative;'>");

                    // line 1
                    inside.push("<div unselectable='on' style='height:1px;line-height:1px;font-size:0px;margin-top:1px;margin-left:1px;margin-right:1px;border-right:1px solid ");
                    inside.push(borderColor);
                    inside.push(";border-left:1px solid ");
                    inside.push(borderColor);
                    inside.push(";background-color:");
                    inside.push(data.client.backColor());
                    inside.push("'>");
                    inside.push("<!-- --></div>");

                    // lower-right wrapper
                    // wrapper
                    inside.push("</div>");

                    // wrapper
                    inside.push("</div>");
                }

                div.innerHTML = inside.join('');
            }
            else {
                if (cache.cssClass) {
                    DayPilot.Util.addClass(div, cache.cssClass);
                }

                var inner = document.createElement("div");
                inner.setAttribute("unselectable", "on");
                inner.className = calendar._prefixCssClass("_event_inner");
                inner.innerHTML = data.client.innerHTML();

                if (cache.backColor) {
                    inner.style.background = cache.backColor;
                }
                if (cache.borderColor) {
                    inner.style.borderColor = cache.borderColor;
                }
                if (cache.backgroundImage) {
                    inner.style.backgroundImage = "url(" + cache.backgroundImage + ")";
                    if (cache.backgroundRepeat) {
                        inner.style.backgroundRepeat = cache.backgroundRepeat;
                    }
                }

                div.appendChild(inner);
                
                // TODO
                if (data.client.barVisible()) {
                    var height = data.part.height - 2;
                    //var barLeft = 100 * data.part.barLeft / (width); // %
                    //var barWidth = Math.ceil(100 * data.part.barWidth / (width)); // %
                    var barTop =  100 * data.part.barTop / height; // %
                    var barHeight = Math.ceil(100 * data.part.barHeight / height); // %

                    if (this.durationBarMode === "PercentComplete") {
                        barLeft = 0;
                        barWidth = cache.complete;
                    }

                    var bar = document.createElement("div");
                    bar.setAttribute("unselectable", "on");
                    bar.className = this._prefixCssClass("_event_bar");
                    bar.style.position = "absolute";

                    var barInner = document.createElement("div");
                    barInner.setAttribute("unselectable", "on");
                    barInner.className = this._prefixCssClass("_event_bar_inner");
                    barInner.style.top = barTop + "%";
                    //barInner.setAttribute("barWidth", data.part.barWidth);  // debug
                    if (0 < barHeight && barHeight <= 1) {
                        barInner.style.height = "1px";
                    }
                    else {
                        barInner.style.height = barHeight + "%";
                    }
                    
                    bar.appendChild(barInner);
                    div.appendChild(bar);
                }                
            }
            
            
            if (cache.areas) {
                for (var i = 0; i < cache.areas.length; i++) {
                    var area = cache.areas[i];
                    if (area.v !== 'Visible') {
                        continue;
                    }
                    var a = DayPilot.Areas.createArea(div, data, area);
                    div.appendChild(a);
                }
            }


            if (main.rows[0].cells[data.part.dayIndex]) { // temporary fix for multirow header, but won't hurt later
                var wrapper = main.rows[0].cells[data.part.dayIndex].firstChild;
                wrapper.appendChild(div);

                calendar._makeChildrenUnselectable(div);
                //var e = new DayPilot.Event(data, calendar);
                div.event = data;

                if (calendar.multiselect._shouldBeSelected(data)) {
                    calendar.multiselect.add(div.event, true);
                }

                if (calendar._api2()) {
                    if (typeof calendar.onAfterEventRender === 'function') {
                        var args = {};
                        args.e = div.event;
                        args.div = div;

                        calendar.onAfterEventRender(args);
                    }
                }
                else {
                    if (calendar.afterEventRender) {
                        calendar.afterEventRender(div.event, div);
                    }
                }
/*
                if (calendar.afterEventRender) {
                    calendar.afterEventRender(data, div);
                }*/
            }

            calendar.elements.events.push(div);
        };

        /*
        this.relativizeChildren = function(el) {
            var c = (el && el.childNodes) ? el.childNodes.length : 0;
            for (var i = 0; i < c; i++) {
                var child = el.childNodes[i];
                if (child.style.position === '') {
                    child.style.position = 'relative';
                }
                this.relativizeChildren(child);
            }
        };
        */

        this._makeChildrenUnselectable = function(el) {
            var c = (el && el.childNodes) ? el.childNodes.length : 0;
            for (var i = 0; i < c; i++) {
                try {
                    var child = el.childNodes[i];
                    if (child.nodeType === 1) {
                        child.setAttribute("unselectable", "on");
                        this._makeChildrenUnselectable(child);
                    }
                }
                catch (e) {
                    //alert(e + " " + child.type);
                }
            }
        };

        this._drawEvents = function() {
            this.multiselect._list = [];

            var start = new Date();

            for (var i = 0; i < this.columnsBottom.length; i++) {
                var col = this.columnsBottom[i];

                for (var m = 0; m < col.blocks.length; m++) {
                    var block = col.blocks[m];
                    for (var j = 0; j < block.lines.length; j++) {
                        var line = block.lines[j];

                        for (var k = 0; k < line.length; k++) {
                            var e = line[k];

                            e.part.width = 100 / block.lines.length;
                            e.part.left = e.part.width * j;

                            if (this.eventArrangement === 'Cascade') {
                                var isLastBlock = (j === block.lines.length - 1);
                                if (!isLastBlock) {
                                    e.part.width = e.part.width * 1.5;
                                }
                            }
                            if (this.eventArrangement === 'Full') {
                                e.part.left = e.part.left / 2;
                                e.part.width = 100 - e.part.left;
                            }

                            if (!e.allday()) {
                                this._drawEvent(e);
                            }
                        }
                    }
                }
            }

            this.multiselect.redraw();

            var end = new Date();
            var diff = end.getTime() - start.getTime();
        };

        this._drawEventsFromLines = function() {
            //return; 
            this.multiselect._list = [];

            for (var i = 0; i < this.columnsBottom.length; i++) {
                var col = this.columnsBottom[i];

                for (var j = 0; j < col.lines.length; j++) {
                    var line = col.lines[j];

                    for (var k = 0; k < line.length; k++) {
                        var e = line[k];

                        e.part.width = 100 / col.lines.length;
                        e.part.left = e.Width * j;

                        if (!e.allday()) {
                            this._drawEvent(e);
                        }
                    }
                }
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

        this._show = function() {
            if (this.nav.top.style.visibility === 'hidden') {
                this.nav.top.style.visibility = 'visible';
            }
        };
        
        this._totalHeight = function() {
            var height = this._totalHeaderHeight() + this._getScrollableHeight();
            
            calendar.debug.message("Getting totalHeight, headerHeight: " + this._totalHeaderHeight() + " scrollable: " + this._getScrollableHeight());
            
            if (height < 0) {
                return 0;
            }
            return height;
        };

        this._drawTop = function() {

            this.nav.top = document.getElementById(this.id);
            this.nav.top.dp = this;
            this.nav.top.innerHTML = '';

            this.nav.top.style.MozUserSelect = 'none';
            this.nav.top.style.KhtmlUserSelect = 'none';
            this.nav.top.style.WebkitUserSelect = 'none';
            this.nav.top.style.position = 'relative';
            if (this.width) {
                this.nav.top.style.width = this.width;
            }
            if (this.rtl) {
                this.nav.top.style.direction = "rtl";
            }
            //this.nav.top.style.width = this.width ? this.width : '100%';
            if (!this.cssOnly) {
                this.nav.top.style.lineHeight = "1.2";
                this.nav.top.style.textAlign = "left";
            }
            if (this.heightSpec === "Parent100Pct") {
                this.nav.top.style.height = "100%";
            }
            else {
                this.nav.top.style.height = this._totalHeight() + "px";
            }

            if (this.hideUntilInit) {
                this.nav.top.style.visibility = 'hidden';
            }
            

            this.nav.scroll = document.createElement("div");
            this.nav.scroll.style.height = this._getScrollableHeight() + "px";

            if (this.cssOnly) {
                DayPilot.Util.addClass(this.nav.top, this._prefixCssClass("_main"));
            }

            var scrolling = this.columnWidthSpec === 'Fixed';
            if (!scrolling) {
                if (this.heightSpec === "Fixed") {
                    this.nav.scroll.style.overflowY = "scroll";
                }
                else if (this.heightSpec === 'BusinessHours' && this._durationHours() <= this.businessEndsHour - this.businessBeginsHour) {
                    this.nav.scroll.style.overflow = "hidden";
                }
                else if (this.heightSpec !== "Full" && this.heightSpec !== "BusinessHoursNoScroll") {
                    this.nav.scroll.style.overflow = "auto";
                }
                else {
                    this.nav.scroll.style.overflow = "hidden";
                }
            }

            this.nav.scroll.style.position = "relative";
            if (!this.cssOnly) {
                this.nav.scroll.style.border = "1px solid " + this.borderColor;
                this.nav.scroll.style.backgroundColor = this.hourNameBackColor;
            }

            // this muse be called after setting overflow on this.nav.scroll because it's used to detect the scrollbar
            if (this.showHeader) {
                var header = this._drawTopHeaderDiv();
                this.nav.top.appendChild(header);
            }

            // fixing the column alignment bug
            // solved thanks to http://stackoverflow.com/questions/139000/div-with-overflowauto-and-a-100-wide-table-problem
            this.nav.scroll.style.zoom = 1;
            this.nav.scroll.setAttribute("data-id", "nav.scroll");
            this.nav.scroll.style.position = "absolute";
            this.nav.scroll.style.left = "0px";
            this.nav.scroll.style.right = "0px";
            this.nav.scroll.style.top = this._totalHeaderHeight() + "px";

            var wrap = this._drawScrollable();
            this.nav.scrollable = wrap.firstChild;
            this.nav.scroll.appendChild(wrap);
            this.nav.top.appendChild(this.nav.scroll);

            this.nav.vsph = document.createElement("div");
            this.nav.vsph.style.display = "none";

            this.nav.top.appendChild(this.nav.vsph);

            this.nav.scrollpos = document.createElement("input");
            this.nav.scrollpos.type = "hidden";
            this.nav.scrollpos.id = calendar.id + "_scrollpos";
            this.nav.scrollpos.name = this.nav.scrollpos.id;
            this.nav.top.appendChild(this.nav.scrollpos);

            this.nav.select = document.createElement("input");
            this.nav.select.type = "hidden";
            this.nav.select.id = calendar.id + "_select";
            this.nav.select.name = this.nav.select.id;
            this.nav.select.value = null;  // used to be selectedEventValue on the server side
            this.nav.top.appendChild(this.nav.select);

            this.nav.scrollLayer = document.createElement("div");
            this.nav.scrollLayer.style.position = 'absolute';
            this.nav.scrollLayer.style.top = '0px';
            this.nav.scrollLayer.style.left = '0px';
            this.nav.top.appendChild(this.nav.scrollLayer);

            this.nav.scrollUp = [];
            this.nav.scrollDown = [];

            this.nav.loading = document.createElement("div");
            this.nav.loading.style.position = 'absolute';
            //this.nav.loading.style.left = '0px';
            this.nav.loading.style.top = '0px';
            this.nav.loading.style.left = (this.hourWidth + 5) + "px";
            // top assigned in loadingStart()
            this.nav.loading.style.backgroundColor = this.loadingLabelBackColor;
            this.nav.loading.style.fontSize = this.loadingLabelFontSize;
            this.nav.loading.style.fontFamily = this.loadingLabelFontFamily;
            this.nav.loading.style.color = this.loadingLabelFontColor;
            this.nav.loading.style.padding = '2px';
            this.nav.loading.innerHTML = this.loadingLabelText;
            this.nav.loading.style.display = 'none';

            this.nav.top.appendChild(this.nav.loading);

        };

        // used during full update
        this._drawHourTable = function() {
            // clear old hour table
            if (!this.fasterDispose) {
                DayPilot.pu(this.nav.hourTable);
            }
            else {
                this._disposeHourTable();
            }
            
            if (this.nav.hoursPlaceholder) {
                this.nav.hoursPlaceholder.innerHTML = '';
                this.nav.hourTable = this._createHourTable();
                this.nav.hoursPlaceholder.appendChild(this.nav.hourTable);
            }
        };

        this._disposeHourTable = function() {
            if (!this.nav.hourTable) {
                return;
            }
            for (var i = 0; i < this.nav.hourTable.rows.length; i++) {
                var row = this.nav.hourTable.rows[i];
                var div = row.cells[0].firstChild;
                div.data = null;
                div.onmousemove = null;
                div.onmouseout = null;
            }
        };

        // used during initial load only
        this._drawScrollable = function() {
            var zoom = document.createElement("div");
            zoom.style.zoom = 1;
            zoom.style.position = 'relative';
            zoom.onmousemove = this._onMainMouseMove;
            zoom.ontouchmove = this._touch.onMainTouchMove;
            zoom.ontouchend = this._touch.onMainTouchEnd;

            var bottomLeft = null;
            var bottomRight = null;
            var hoursPlaceholder = null;
            
            var scrolling = this.columnWidthSpec === 'Fixed';
            if (scrolling) {
                if (this.showHours) {
                    var left = document.createElement("div");
                    left.style.cssFloat = "left";
                    left.style.styleFloat = "left";  // IE7
                    left.style.width = (this.hourWidth) + "px";
                    left.style.height = this._getScrollableHeight() + "px";
                    left.style.overflow = "hidden";
                    zoom.appendChild(left);
                    bottomLeft = left;

                    var scrollbarSpace = 30;
                    var height = (this._duration() * this.cellHeight) / (60000 * this.cellDuration) + scrollbarSpace;

                    hoursPlaceholder = document.createElement("div");
                    hoursPlaceholder.style.height = (height) + "px";

                    left.appendChild(hoursPlaceholder);
                }
                
                var right = document.createElement("div");
                //right.style.width = "500px";
                right.style.height = this._getScrollableHeight() + "px";
                if (this.showHours) {
                    right.style.marginLeft = (this.hourWidth) + "px";
                }
                //right.style.backgroundColor = "red";
                right.style.position = "relative";
                right.style.overflow = "auto";
                /*
                right.onscroll = function() {
                    calendar.nav.bottomLeft.scrollTop = calendar.nav.bottomRight.scrollTop;
                    calendar.nav.upperRight.scrollLeft = calendar.nav.bottomRight.scrollLeft;
                };
                */
                zoom.appendChild(right);
                bottomRight = right;
            }
            else {
                var table = document.createElement("table");

                table.cellSpacing = "0";
                table.cellPadding = "0";
                table.border = "0";
                table.style.border = "0px none";
                table.style.width = "100%";
                // absolute position causing problems in Chrome (not full width sometimes)
                table.style.position = 'relative';

                var r = table.insertRow(-1);

                var c;
                if (this.showHours) {
                    c = r.insertCell(-1);
                    //c.valign = "top";
                    c.style.verticalAlign = "top";
                    c.style.padding = '0px';
                    c.style.border = '0px none';

                    hoursPlaceholder = c;
                }

                c = r.insertCell(-1);
                //c.setAttribute("valign", "top");
                //c.valign = "top";
                c.width = "100%";
                c.style.padding = '0px';
                c.style.border = '0px none';
                c.style.verticalAlign = "top";

                if (!this.cssOnly) {
                    c.style.borderLeft = "1px solid " + this.borderColor;
                }
                
                bottomRight = c;

                zoom.appendChild(table);
            }
            
            
            if (hoursPlaceholder) {
                this.nav.hourTable = this._createHourTable();
                hoursPlaceholder.appendChild(this.nav.hourTable);
            }
            
            if (!this.cssOnly && !this.separateEventsTable) {
                bottomRight.appendChild(this._createEventsAndCells());
            }
            else {
                var parent = document.createElement("div");
                parent.style.height = "0px";
                parent.style.position = "relative";

                parent.appendChild(this._createEventsAndCells());

                var crosshair = document.createElement("div");
                crosshair.style.position = "absolute";
                crosshair.style.top = "0px";
                crosshair.style.left = "0px";
                crosshair.style.right = "0px";
                crosshair.style.height = "0px";
                parent.appendChild(crosshair);
                this.nav.crosshair = crosshair;

                parent.appendChild(this._createEventsTable());

                bottomRight.appendChild(parent);
            }

            this.nav.zoom = zoom;
            this.nav.bottomLeft = bottomLeft;
            this.nav.bottomRight = bottomRight;
            this.nav.hoursPlaceholder = hoursPlaceholder;
            return zoom;
        };
        
        this._createEventsAndCells = function() {
            var table = document.createElement("table");

            //table.style.position = "absolute";
            //table.style.top = "0px";
            table.cellPadding = "0";
            table.cellSpacing = "0";
            table.border = "0";
            var scrolling = this.columnWidthSpec === 'Fixed';

            if (!scrolling) {
                table.style.width = "100%";
            }
            table.style.border = "0px none";
            if (!this.cssOnly) {
             //   table.style.borderLeft = "1px solid " + this.borderColor;
            }
            table.style.tableLayout = 'fixed';

            this.nav.main = table;
            this.nav.events = table;

            return table;

        };

        this._createEventsTable = function() {
            var table = document.createElement("table");

            var scrolling = this.columnWidthSpec === 'Fixed';

            table.style.position = "absolute";
            table.style.top = "0px";
            table.cellPadding = "0";
            table.cellSpacing = "0";
            table.border = "0";
            if (!scrolling) {
                table.style.width = "100%";
            }
            else {
                table.style.width = (this.columnsBottom.length * this.columnWidth) + "px";
            }
            table.style.border = "0px none";
            table.style.tableLayout = 'fixed';
            //table.setAttribute("events", "true");

            this.nav.events = table;
            var create = true;
            var columns = this.columnsBottom;
            var cl = columns.length;

            var r = (create) ? table.insertRow(-1) : table.rows[0];

            for (var j = 0; j < cl; j++) {
                var c = (create) ? r.insertCell(-1) : r.cells[j];

                if (create) {

                    c.style.padding = '0px';
                    c.style.border = '0px none';
                    c.style.height = '0px';
                    c.style.overflow = 'visible';
                    if (!calendar.rtl) {
                        c.style.textAlign = 'left';
                    }

                    // withpct
                    //c.style.width = (100.0 / columns.length) + "%";

                    var div = document.createElement("div");
                    div.style.marginRight = calendar.columnMarginRight + "px";
                    div.style.position = 'relative';
                    div.style.height = '1px';
                    if (!this.cssOnly) {
                        div.style.fontSize = '1px';
                        div.style.lineHeight = '1.2';
                    }
                    div.style.marginTop = '-1px';

                    c.appendChild(div);

                }
            }

            return table;
        };

        this._createHourTable = function() {
            var table = document.createElement("table");
            table.cellSpacing = "0";
            table.cellPadding = "0";
            table.border = "0";
            table.style.border = '0px none';
            table.style.width = this.hourWidth + "px";
            table.oncontextmenu = function() { return false; };
            table.onmousemove = function() { calendar._crosshairHide(); };

            var hours = this._duration() / (this.timeHeaderCellDuration * 60 * 1000);  // duration in ticks
            for (var i = 0; i < hours; i++) {
                this._createHourRow(table, i);
            }

            return table;

        };

        this._autoHiddenRows = function() {
            return (this._visibleStart() - this._visibleStart(true)) * (60 / this.cellDuration);
        };

        this._autoHiddenHours = function() {
            return (this._visibleStart() - this._visibleStart(true));
        };

        this._autoHiddenPixels = function() {
            return this._autoHiddenRows() * this.cellHeight;
        };

        this._createHourRow = function(table, i) {
            var height = (this.cellHeight * 60 / this.cellDuration) / (60 / this.timeHeaderCellDuration);

            var r = table.insertRow(-1);
            r.style.height = height + "px";

            var c = r.insertCell(-1);
            c.valign = "bottom";
            c.setAttribute("unselectable", "on");
            if (!this.cssOnly) {
                c.className = this._prefixCssClass("rowheader");
                c.style.backgroundColor = this.hourNameBackColor;
                c.style.cursor = "default";
            }
            c.style.padding = '0px';
            c.style.border = '0px none';

            var frame = document.createElement("div");
            if (this.cssOnly) {
                frame.className = this._prefixCssClass("_rowheader");
            }
            frame.style.position = "relative";
            frame.style.width = this.hourWidth + "px";
            frame.style.height = (height) + "px";
            frame.style.overflow = 'hidden';
            frame.setAttribute("unselectable", "on");

            var block = document.createElement("div");
            if (this.cssOnly) {
                block.className = this._prefixCssClass("_rowheader_inner");
            }
            block.setAttribute("unselectable", "on");
            //block.style.display = "block";
            if (!this.cssOnly) {
                block.style.borderBottom = "1px solid " + this.hourNameBorderColor;
                block.style.textAlign = "right";
                block.style.height = (height - 1) + "px";
            }
            
            var html = null;
            var data = null;
            
            if (this.hours) {
                data = this.hours[i + this._autoHiddenHours()];
                html = data.html;
            }

            var start = this.startDate.addHours(i + this._visibleStart());
            var hour = start.getHours();
            
            if (typeof calendar.onBeforeTimeHeaderRender === 'function') {

                var args = {};
                args.header = {};
                args.header.hours = hour;
                args.header.minutes = start.getMinutes();
                args.header.start = start.toString("HH:mm");
                args.header.html = html;
                args.header.areas = data ? data.areas : null;

                calendar.onBeforeTimeHeaderRender(args);
                
                if (args.header.html !== null) {
                    html = args.header.html;
                }
                data = args.header;
            }

            if (data) {
                frame.data = data;
                frame.onmousemove = calendar._onTimeHeaderMouseMove;
                frame.onmouseout = calendar._onTimeHeaderMouseOut;
            }


            if (html) {
                block.innerHTML = html;
            }
            else {

                
                var text = document.createElement("div");
                text.setAttribute("unselectable", "on");
                if (!this.cssOnly) {
                    text.style.padding = "2px";
                    text.style.fontFamily = this.hourFontFamily;
                    text.style.fontSize = this.hourFontSize;
                    text.style.color = this.hourFontColor;
                }


                var am = hour < 12;
                if (this._resolved.timeFormat() === "Clock12Hours") {
                    hour = hour % 12;
                    if (hour === 0) {
                        hour = 12;
                    }
                }

                text.innerHTML = hour;

                var span = document.createElement("span");
                span.setAttribute("unselectable", "on");
                if (!this.cssOnly) {
                    span.style.fontSize = "10px";
                    span.style.verticalAlign = "super";
                }
                else {
                    span.className = this._prefixCssClass("_rowheader_minutes");
                }

                var sup;
                if (this._resolved.timeFormat() === "Clock12Hours") {
                    if (am) {
                        sup = "AM";
                    }
                    else {
                        sup = "PM";
                    }
                }
                else {
                    sup = "00";
                }

                if (!this.cssOnly) {
                    span.innerHTML = "&nbsp;" + sup;
                }
                else {
                    span.innerHTML = sup;
                }


                text.appendChild(span);

                block.appendChild(text);
            }

            frame.appendChild(block);

            c.appendChild(frame);
        };

        this._onTimeHeaderMouseMove = function(ev) {
            calendar._crosshairHide();

            var div = this;
            if (!div.active) {
                DayPilot.Areas.showAreas(div, div.data);
            }

        };

        this._onTimeHeaderMouseOut = function(ev) {
            DayPilot.Areas.hideAreas(this, ev);
        };

        this._getScrollableHeight = function() {
            switch (this.heightSpec) {
                case "Fixed":
                    return this.height;
                case "Parent100Pct":
                    return this.height;
                case "Full":
                    return (this._duration() * this.cellHeight) / (60000 * this.cellDuration);
                case "BusinessHours":
                case "BusinessHoursNoScroll":
                    var dHours = this._businessHoursSpan();
                    return dHours * this.cellHeight * 60 / this.cellDuration;
                default:
                    throw "DayPilot.Calendar: Unexpected 'heightSpec' value.";

            }
        };

        this._totalHeaderHeight = function() {
            if (!this.showHeader) {
                return 0;
            }
            var headerRowsHeight = this.headerLevels * resolved.headerHeight() + this.headerLevels - 1;
            if (this.showAllDayEvents && resolved.allDayHeaderHeight()) {
                if (!this.cssOnly) {
                    return headerRowsHeight + resolved.allDayHeaderHeight();
                }
                else {
                    return headerRowsHeight + resolved.allDayHeaderHeight();
                }
            }
            else {
                return headerRowsHeight;
            }
        };

        this._autoHeaderHeight = function() {
            if (!this.headerHeightAutoFit) {
                return;
            }

            if (this.headerLevels > 1) {
                throw "Header height can't be adjusted for HeaderLevels > 1 (not implemented yet).";
                return;
            }
            var max = 0;
            for (var i = 0; i < this.columnsBottom.length; i++) {
                var cell = this.nav.header.rows[this.headerLevels - 1].cells[i];
                var div = cell.firstChild;
                var inner = div.firstChild;

                var oldHeight = div.style.height;
                div.style.height = "auto";

                inner.style.position = "static";

                var h = div.offsetHeight;

                div.style.height = oldHeight;
                inner.style.position = '';

                max = Math.max(max, h);
            }

            if (max > this.headerHeight) {
                this._cache.headerHeight = max;
                this._updateHeaderHeight();
                this._drawHeader();
            }

        };

        this._scrollbarVisible = function() {
            if (this.cssOnly) {
                return DayPilot.sw(calendar.nav.scroll) > 0;
            }
            else {
                return DayPilot.sw(calendar.nav.scroll) > 2;  //borders
            }
            
            /*
            if (this.heightSpec === 'Parent100Pct') {
                var inner = (this._duration() * this.cellHeight) / (60000 * this.cellDuration);
                return inner > this.height;
            }
            return this.nav.scroll.style.overflow !== 'hidden';
            */
        };

        this._drawTopHeaderDiv = function() {
            var header = document.createElement("div");
            //header.setAttribute("data-id", "header-div");
            if (!this.cssOnly) {
                header.style.borderLeft = "1px solid " + this.borderColor;
                header.style.borderRight = "1px solid " + this.borderColor;
            }
            var scrolling = this.columnWidthSpec === 'Fixed';
            if (!scrolling) {
                header.style.overflow = "auto";
            }
            
            header.style.position = "absolute";
            header.style.left = "0px";
            header.style.right = "0px";
            
            // to match main grid structure
            var zoom = document.createElement("div");
            zoom.style.position = "relative";
            zoom.style.zoom = "1";

            var relative = null;
            var scrolling = this.columnWidthSpec === 'Fixed';
            if (scrolling) {
                var left = document.createElement("div");
                left.style.cssFloat = "left";
                left.style.styleFloat = "left";  // IE7
                left.style.width = (this.hourWidth) + "px";
                
                if (this.showHours) {
                    var corner = this._drawCorner();
                    this.nav.corner = corner;
                    left.appendChild(corner);
                    zoom.appendChild(left);
                    
                    this.nav.upperLeft = left;
                }
                
                var scrollbarSpace = 30;
                
                var right = document.createElement("div");
                if (this.showHours) {
                    right.style.marginLeft = (this.hourWidth) + "px";
                }
                right.style.position = "relative";
                right.style.overflow = "hidden";
                right.style.height = this._totalHeaderHeight() + "px";
                
                
                zoom.appendChild(right);
                this.nav.upperRight = right;
                
                relative = document.createElement("div");
                relative.style.width = (this.columnsBottom.length * this.columnWidth + scrollbarSpace) + "px";
                
                right.appendChild(relative);

            }
            else {

                var table = document.createElement("table");
                table.cellPadding = "0";
                table.cellSpacing = "0";
                table.border = "0";
                table.style.width = "100%";
                table.style.borderCollapse = 'separate';
                //table.style.position = "absolute";
                table.style.border = "0px none";

                var r = table.insertRow(-1);
                this.nav.fullHeader = table;


                if (this.showHours) {
                    // corner
                    var c = r.insertCell(-1);
                    c.style.padding = '0px';
                    c.style.border = '0px none';

                    var corner = this._drawCorner();
                    c.appendChild(corner);
                    this.nav.corner = corner;
                }/*
                else {
                    var placeholder = document.createElement("div");
                    placeholder.style.backgroundColor = this.hourNameBackColor;
                    placeholder.style.width = "0px";
                    placeholder.style.height = this._totalHeaderHeight() + "px";
                    placeholder.style.borderTop = "1px solid " + this.borderColor;
                    placeholder.setAttribute("unselectable", "on");

                    c.appendChild(placeholder);
                }*/

                // top header
                c = r.insertCell(-1);
                //var mid = c;

                c.style.width = "100%";
                if (!this.cssOnly) {
                    c.style.backgroundColor = this.hourNameBackColor;
                }
                c.valign = "top";
                c.style.position = 'relative';  // ref point
                c.style.padding = '0px';
                c.style.border = '0px none';

                relative = document.createElement("div");
                //relative.setAttribute("data-id", "nav.mid");
                relative.style.position = "relative";
                relative.style.height = this._totalHeaderHeight() + "px";
                relative.style.overflow = "hidden";
                c.appendChild(relative);
                this.nav.mid = relative;

                zoom.appendChild(table);

            }
            
            this.nav.headerParent = relative;
            this._createNavHeader();

            var scrollbar = this._scrollbarVisible();

            // above the vertical scrollbar
            //if (this.heightSpec !== "Full" && this.heightSpec !== "BusinessHoursNoScroll") {
            var scrolling = this.columnWidthSpec === 'Fixed';
            if (scrollbar && !scrolling) {
                this._createCornerRightTd();
            }

            header.appendChild(zoom);

            return header;

        };
        
        this._createNavHeader = function() {
            this.nav.header = document.createElement("table");
            this.nav.header.cellPadding = "0";
            this.nav.header.cellSpacing = "0";
            //this.nav.header.border = "0";
            var scrolling = this.columnWidthSpec === 'Fixed';
            if (!scrolling) {
                this.nav.header.width = "100%";
            }
            this.nav.header.style.tableLayout = "fixed";
            if (!this.cssOnly) {
                this.nav.header.style.borderBottom = "0px none #000000";
                this.nav.header.style.borderLeft = "1px solid " + this.borderColor;
                //this.nav.header.style.borderRight = "1px solid " + this.borderColor;
                this.nav.header.style.borderTop = "1px solid " + this.borderColor;
            }
            //this.nav.header.style.borderCollapse = 'separate';
            this.nav.header.oncontextmenu = function() { return false; };

            var scrollbar = this._scrollbarVisible();
            //var scrollbar = DayPilot.sw(this.nav.scroll) > 0;

            if (!this.cssOnly) {
                if (scrollbar) {
                    this.nav.header.style.borderRight = "1px solid " + this.borderColor;
                }
            }

            this.nav.headerParent.appendChild(this.nav.header);
            
            if (this.nav.allday) {
                DayPilot.de(this.nav.allday);
            }
            
            if (this.showAllDayEvents) {        // caused problems in IE7 compatibility mode

                var allday = document.createElement("div");
                allday.style.position = 'absolute';
                allday.style.top = "0px";
                allday.style.height = "0px";

                var scrolling = this.columnWidthSpec === 'Fixed';
                if (!scrolling) {
                    allday.style.width = "100%";
                }
                else {
                    allday.style.width = (this.columnsBottom.length * this.columnWidth) + "px";
                }
                

                this.nav.allday = allday;
                this.nav.headerParent.appendChild(allday);
            }            
        };

        this._createCornerRightTd = function() {
            if (!this.nav.fullHeader) {
                return;
            }
            
            var r = this.nav.fullHeader.rows[0];
            var c = r.insertCell(-1);

            if (!this.cssOnly) {
                c.className = this._prefixCssClass('cornerright');
                c.style.backgroundColor = this.hourNameBackColor;
                c.style.borderBottom = "0px none";
                c.style.borderLeft = "1px solid " + this.borderColor;
                c.style.borderRight = "0px none";
            }
            c.style.padding = '0px';
            c.style.verticalAlign = 'top';
            c.setAttribute("unselectable", "on");
            //c.innerHTML = "&nbsp;";

            var inside = document.createElement("div");
            inside.setAttribute("unselectable", "on");
            if (this.cssOnly) {
                inside.className = this._prefixCssClass('_cornerright');
            }
            inside.style.overflow = "hidden";
            inside.style.position = "relative";
            inside.style.width = "16px";
            inside.style.height = this._totalHeaderHeight() + "px";

            var inner = document.createElement("div");
            if (this.cssOnly) {
                inner.className = this._prefixCssClass('_cornerright_inner');
            }
            else {
                inner.style.borderTop = "1px solid " + this.borderColor;
            }
            inside.appendChild(inner);

            c.appendChild(inside);

            this.nav.cornerRight = inside;
        };

        this._drawCorner = function() {
            var wrap = document.createElement("div");
            wrap.style.position = 'relative';
            if (!this.cssOnly) {
                wrap.style.backgroundColor = this.cornerBackColor;
                wrap.style.fontFamily = this.headerFontFamily;
                wrap.style.fontSize = this.headerFontSize;
                wrap.style.color = this.headerFontColor;
                wrap.className = this._prefixCssClass("corner");
            }
            else {
                wrap.className = this._prefixCssClass("_corner");
            }
            wrap.style.width = this.hourWidth + "px";
            wrap.style.height = this._totalHeaderHeight() + "px";
            wrap.style.overflow = "hidden";
            wrap.oncontextmenu = function() { return false; };

            var corner = document.createElement("div");
            if (this.cssOnly) {
                corner.className = this._prefixCssClass("_corner_inner");
            }
            else {
                corner.style.borderTop = "1px solid " + this.borderColor;
            }
            corner.setAttribute("unselectable", "on");
            
            var html = this.cornerHTML || this.cornerHtml;
            
            corner.innerHTML = html ? html : '';

            wrap.appendChild(corner);

            if (!this.numberFormat) return wrap;

            var inner2 = document.createElement("div");
            inner2.style.position = 'absolute';
            inner2.style.padding = '2px';
            inner2.style.top = '0px';
            inner2.style.left = '1px';
            inner2.style.backgroundColor = "#FF6600";
            inner2.style.color = "white";
            inner2.innerHTML = "\u0044\u0045\u004D\u004F";
            inner2.setAttribute("unselectable", "on");
            wrap.appendChild(inner2);

            return wrap;
        };

        this._disposeMain = function() {
            var table = this.nav.main;
            table.root = null;
            table.onmouseup = null;

            for (var y = 0; y < table.rows.length; y++) {
                var r = table.rows[y];
                for (var x = 0; x < r.cells.length; x++) {
                    var c = r.cells[x];
                    c.root = null;

                    c.onmousedown = null;
                    c.onmousemove = null;
                    c.onmouseout = null;
                    c.onmouseup = null;
                    c.onclick = null;
                    c.ondblclick = null;
                    c.oncontextmenu = null;
                }
            }

            if (!this.fasterDispose) DayPilot.pu(table);
        };

        this._deleteScrollLabels = function() {
            for (var i = 0; this.nav.scrollUp && i < this.nav.scrollUp.length; i++) {
                this.nav.scrollLayer.removeChild(this.nav.scrollUp[i]);
            }

            for (var i = 0; this.nav.scrollDown && i < this.nav.scrollDown.length; i++) {
                this.nav.scrollLayer.removeChild(this.nav.scrollDown[i]);
            }

            this.nav.scrollUp = [];
            this.nav.scrollDown = [];

        };

        // draw time cells
        this._drawMain = function() {

/*
            var cols = [];
            var dates = [];*/

            var table = this.nav.main;
            var step = this.cellDuration * 60 * 1000;
            var rowCount = this._rowCount();

            var columns = calendar.columnsBottom;
            var create = !this.tableCreated || table.rows.length === 0 || columns.length !== table.rows[0].cells.length || rowCount !== table.rows.length; // redraw only if number of columns changes

            if (table) {
                this._disposeMain();
                if (calendar._browser.ielt9 && create) {
                    DayPilot.de(this.nav.scrollable.parentNode);
                    var wrap = this._drawScrollable();
                    this.nav.scrollable = wrap.firstChild;
                    this.nav.scroll.appendChild(wrap);
                    //this._drawScrollable();
                    table = this.nav.main;
                    /*
                    var parent = table.parentNode;
                    DayPilot.de(table);
                    parent.appendChild(this._createEventsAndCells());
                    table = this.nav.main;*/
                }
            }

            //var i = 0;
            while (table && table.rows && table.rows.length > 0 && create) {
                if (!this.fasterDispose) DayPilot.pu(table.rows[0]);
                /*
                var row = table.rows[0];
                while (calendar._browser.ie && row.cells && row.cells.length > 0) {
                    row.deleteCell(0);
                }*/
                table.deleteRow(0);
            }
            
            /*
            if (i > 0) {
                alert("rows deleted: " + i);
            }*/

            this.tableCreated = true;

            // scroll labels
            if (this.scrollLabelsVisible) {
                var columns = this.columnsBottom;
                var hoursWidth = (this.showHours ? this.hourWidth : 0);
                var colWidth = (this.nav.scroll.clientWidth - hoursWidth) / columns.length;
                for (var i = 0; i < columns.length; i++) {
                    var scrollUp = document.createElement("div");
                    scrollUp.style.position = 'absolute';
                    scrollUp.style.top = '0px';
                    scrollUp.style.left = (hoursWidth + 2 + i * colWidth + colWidth / 2) + "px";
                    scrollUp.style.display = 'none';


                    var img = document.createElement("div");
                    img.style.height = '10px';
                    img.style.width = '10px';
                    if (this.cssOnly) {
                        img.className = this._prefixCssClass("_scroll_up");
                    }
                    else {
                        img.style.backgroundRepeat = "no-repeat";
                        if (this.scrollUpUrl) {
                            img.style.backgroundImage = "url('" + this.scrollUpUrl + "')";
                        }
                        img.className = this._prefixCssClass("scroll_up");
                    }
                    scrollUp.appendChild(img);

                    this.nav.scrollLayer.appendChild(scrollUp);
                    this.nav.scrollUp.push(scrollUp);

                    var scrollDown = document.createElement("div");
                    scrollDown.style.position = 'absolute';
                    scrollDown.style.top = '0px';
                    scrollDown.style.left = (hoursWidth + 2 + i * colWidth + colWidth / 2) + "px";
                    scrollDown.style.display = 'none';

                    var img = document.createElement("div");
                    img.style.height = '10px';
                    img.style.width = '10px';
                    if (this.cssOnly) {
                        img.className = this._prefixCssClass("_scroll_down");
                    }
                    else {
                        img.style.backgroundRepeat = "no-repeat";
                        if (this.scrollDownUrl) {
                            img.style.backgroundImage = "url('" + this.scrollDownUrl + "')";
                        }
                        img.className = this._prefixCssClass("scroll_down");
                    }
                    scrollDown.appendChild(img);

                    this.nav.scrollLayer.appendChild(scrollDown);
                    this.nav.scrollDown.push(scrollDown);
                }
            }

            var cl = columns.length;

            // CssOnly mode, event table is separate
            if (this.cssOnly || this.separateEventsTable) {
                var events = this.nav.events;

                while (events && events.rows && events.rows.length > 0 && create) {
                    if (!this.fasterDispose) DayPilot.pu(events.rows[0]);
                    events.deleteRow(0);
                }

                // TODO identical code is in createEventsTable, merge
                var r = (create) ? events.insertRow(-1) : events.rows[0];

                for (var j = 0; j < cl; j++) {  
                    var c = (create) ? r.insertCell(-1) : r.cells[j];

                    if (create) {

                        c.style.padding = '0px';
                        c.style.border = '0px none';
                        c.style.height = '1px';
                        c.style.overflow = 'visible';
                        var scrolling = this.columnWidthSpec === 'Fixed';
                        if (scrolling) {
                            c.style.width = this.columnWidth + "px";
                        }
                        if (!calendar.rtl) {
                            c.style.textAlign = 'left';
                        }

                        var div = document.createElement("div");
                        div.style.marginRight = calendar.columnMarginRight + "px";
                        div.style.position = 'relative';
                        div.style.height = '1px';
                        if (!this.cssOnly) {
                            div.style.fontSize = '1px';
                            div.style.lineHeight = '1.2';
                        }
                        div.style.marginTop = '-1px';

                        c.appendChild(div);

                    }
                }
            }

            for (var i = 0; i < rowCount; i++) {
                var r = (create) ? table.insertRow(-1) : table.rows[i];

                if (create) {
                    r.style.MozUserSelect = 'none';
                    r.style.KhtmlUserSelect = 'none';
                    r.style.WebkitUserSelect = 'none';
                }

                for (var j = 0; j < cl; j++) {
                    var col = this.columnsBottom[j];

                    var c = (create) ? r.insertCell(-1) : r.cells[j];

                    // always update
                    c.start = col.start.addTime(i * step).addHours(this._visibleStart());
                    c.end = c.start.addTime(step);
                    c.resource = col.id;
                    
                    if (typeof this.onBeforeCellRender === 'function') {

                        if (!this.cellProperties) {
                            this.cellProperties = [];
                        }

                        var cell = {};
                        cell.resource = c.resource;
                        cell.start = c.start;
                        cell.end = c.end;

                        var index = j + "_" + i;
                        
                        cell.cssClass = null;
                        cell.html = null;
                        cell.backImage = null;
                        cell.backRepeat = null;
                        cell.backColor = null;
                        cell.business = this._isBusinessCell(c.start, c.end);

                        if (this.cellProperties[index]) {
                            DayPilot.Util.copyProps(this.cellProperties[index], cell, ['cssClass', 'html', 'backImage', 'backRepeat', 'backColor', 'business']);
                        }

                        var args = {};
                        args.cell = cell;

                        this.onBeforeCellRender(args);
                        
                        this.cellProperties[index] = cell;
                    }                    

                    var props = calendar._getProperties(j, i);

                    if (create) {
                        c.root = this;

                        c.style.padding = '0px';
                        c.style.border = '0px none';
                        c.style.verticalAlign = 'top';
                        c.style.height = calendar.cellHeight + 'px';
                        c.style.overflow = 'hidden';
                        c.setAttribute("unselectable", "on");

                        if (!this.cssOnly) {
                            var content = document.createElement("div");
                            content.style.height = (calendar.cellHeight - 1) + "px";
                            content.style.width = '100%';
                            content.style.overflow = 'hidden';
                            content.setAttribute("unselectable", "on");
                            c.appendChild(content);

                            var div = document.createElement("div");
                            div.setAttribute("unselectable", "on");
                            div.style.fontSize = '1px';
                            div.style.height = '0px';
                            c.appendChild(div);

                            if ((!calendar.rtl && j !== cl - 1) || calendar.rtl) {
                                c.style.borderRight = '1px solid ' + calendar.cellBorderColor;
                            }

                            // hack, no multiplying
                            var endHour = (c.end.getMinutes() + c.end.getSeconds() + c.end.getMilliseconds()) > 0;

                            if (endHour) {
                                if (calendar.hourHalfBorderColor !== '') {
                                    div.style.borderBottom = '1px solid ' + calendar.hourHalfBorderColor; // HourHalfBorderColor
                                }
                                div.className = calendar._prefixCssClass("hourhalfcellborder");
                            }
                            else {
                                if (calendar.hourBorderColor !== '') {
                                    div.style.borderBottom = '1px solid ' + calendar.hourBorderColor; // HourBorderColor
                                }
                                div.className = calendar._prefixCssClass("hourcellborder");
                            }

                        }
                        else {  // cssonly
                            var content = document.createElement("div");
                            content.className = calendar._prefixCssClass("_cell");
                            content.style.position = "relative";
                            content.style.height = (calendar.cellHeight) + "px";
                            //content.style.width = '100%';
                            content.style.overflow = 'hidden';
                            content.setAttribute("unselectable", "on");

                            var inner = document.createElement("div");
                            inner.className = calendar._prefixCssClass("_cell_inner");
                            content.appendChild(inner);

                            c.appendChild(content);
                        }


                    }
                    else {
                        content = c.firstChild;
                        content.className = calendar._prefixCssClass("_cell"); // must be reset
                    }
/*
                    if (this.scrolling) {
                        c.style.width = this.scrollingColumnWidth + "px";
                    }*/

                    c.onmousedown = this._onCellMouseDown;
                    c.onmousemove = this._onCellMouseMove;
                    c.onmouseout = this._onCellMouseOut;
                    
                    c.ontouchstart = this._touch.onCellTouchStart;
                    c.ontouchmove = this._touch.onCellTouchMove;
                    c.ontouchend = this._touch.onCellTouchEnd;
                    
                    c.onmouseup = function() { return false; };

                    c.onclick = function() { return false; };

                    c.ondblclick = function() {
                        DayPilotCalendar.firstMousePos = null;
                        
                        calendar._activateSelection();
                        clearTimeout(DayPilotCalendar.selectedTimeout);

                        if (calendar.timeRangeDoubleClickHandling === 'Disabled') {
                            return;
                        }
                        var sel = calendar.getSelection();
                        calendar._timeRangeDoubleClickDispatch(sel.start, sel.end, sel.resource);
                    };

                    c.oncontextmenu = function() {
                        if (!this.selected) {
                            //if (DayPilotCalendar.selectedCells) {
                            calendar.clearSelection();
                            //    DayPilotCalendar.selectedCells = [];
                            //}
                            DayPilotCalendar.column = DayPilotCalendar.getColumn(this);
                            calendar.selectedCells.push(this);
                            DayPilotCalendar.firstSelected = this;
                            DayPilotCalendar.topSelectedCell = this;
                            DayPilotCalendar.bottomSelectedCell = this;

                            calendar._activateSelection();
                        }

                        if (calendar.contextMenuSelection) {
                            calendar.contextMenuSelection.show(calendar.getSelection());
                        }
                        return false;
                    };


                    var backColor = calendar._getColor(j, i); // = c.style.backgroundColor
                    content = c.firstChild;

                    var scrolling = this.columnWidthSpec === 'Fixed';
                    if (scrolling) {
                        content.style.width = this.columnWidth + "px";
                    }

                    if (backColor) {
                        if (this.cssOnly) {
                            content.firstChild.style.background = backColor;
                        }
                        else {
                            content.style.background = backColor;
                        }
                    }

                    var business = props ? props.business : this._isBusinessCell(c.start, c.end);
                    if (business && this.cssOnly) {
                        DayPilot.Util.addClass(content, calendar._prefixCssClass("_cell_business"));
                    }
                    
                    var html = this.cssOnly ? content.firstChild : content;
                    
                    // reset custom properties
                    if (html) {
                        html.innerHTML = ''; // reset
                    }
                    content.style.backgroundImage = "";
                    content.style.backgroundRepeat = "";
                    
                    if (props) {
                        if (props.html) {
                            html.innerHTML = props.html;
                        }
                        if (props.cssClass) {
                            if (this.cssOnly) {
                                DayPilot.Util.addClass(content, props.cssClass);
                            }
                            else {
                                DayPilot.Util.addClass(c, calendar._prefixCssClass(props.cssClass));
                            }
                        }
                        if (props.backImage) {
                            content.style.backgroundImage = "url('" + props.backImage + "')";
                        }
                        if (props.backRepeat) {
                            content.style.backgroundRepeat = props.backRepeat;
                        }
                    }
                    
                    if (!this.cssOnly) {
                        DayPilot.Util.addClass(c, calendar._prefixCssClass("cellbackground"));
                        //c.className = calendar._prefixCssClass("cellbackground");
                    }

                }
            }

            table.onmouseup = this._onMainMouseUp;
            table.root = this;

            calendar.nav.scrollable.style.display = '';
        };
        
        this._isBusinessCell = function(start, end) {
            if (this.businessBeginsHour < this.businessEndsHour)
            {
                return !(start.getHours() < this.businessBeginsHour || start.getHours() >= this.businessEndsHour || start.getDayOfWeek() === 6 || start.getDayOfWeek() === 0);
            }

            if (start.getHours() >= this.businessBeginsHour)
            {
                return true;
            }

            if (start.getHours() < this.businessEndsHour)
            {
                return true;
            }

            return false;            
        };

        this._onMainMouseMove = function(ev) {

            ev = ev || window.event;
            ev.insideMainD = true;
            if (window.event) {
                window.event.srcElement.inside = true;
            }

            DayPilotCalendar.activeCalendar = this; // required for moving

            var ref = calendar.nav.main; // changed from this.nav.scrollable
            calendar.coords = DayPilot.mo3(ref, ev);

            var mousePos = DayPilot.mc(ev);

            var crosshair = calendar.crosshairType && calendar.crosshairType !== "Disabled";
            var inTimeHeader = calendar.coords.x < calendar.hourWidth;
            if (DayPilot.Global.moving || DayPilot.Global.resizing || DayPilotCalendar.selecting || inTimeHeader) {
                calendar._crosshairHide();
            }
            else if (crosshair) {
                calendar._crosshair();
            }
            
            if (DayPilot.Global.resizing) {
                if (!DayPilotCalendar.resizingShadow) {
                    //DayPilotCalendar.deleteShadow(DayPilotCalendar.resizingShadow);                    
                    DayPilotCalendar.resizingShadow = calendar._createShadow(DayPilot.Global.resizing, false, calendar.shadow);
                }
                // make sure the cursor is correct
                //DayPilotCalendar.resizingShadow.style.cursor = 'n-resize';

                //DayPilotCalendar.resizing.dirty = true;
                var _step = DayPilot.Global.resizing.event.calendar.cellHeight;
                var _startOffset = 0;
                var delta = (mousePos.y - DayPilotCalendar.originalMouse.y);

                if (DayPilot.Global.resizing.dpBorder === 'bottom') {
                    //var newHeight = Math.floor( ((DayPilotCalendar.originalHeight + delta) + _step/2) / _step ) * _step;
                    var newHeight = Math.floor(((DayPilotCalendar.originalHeight + DayPilotCalendar.originalTop + delta) + _step / 2) / _step) * _step - DayPilotCalendar.originalTop + _startOffset;

                    if (newHeight < _step)
                        newHeight = _step;

                    var max = DayPilot.Global.resizing.event.calendar.nav.main.clientHeight;
                    if (DayPilotCalendar.originalTop + newHeight > max) {
                        newHeight = max - DayPilotCalendar.originalTop;
                    }
                    
                    DayPilotCalendar.resizingShadow.style.height = (newHeight) + 'px';
                }
                else if (DayPilot.Global.resizing.dpBorder === 'top') {
                    var newTop = Math.floor(((DayPilotCalendar.originalTop + delta - _startOffset) + _step / 2) / _step) * _step + _startOffset;

                    if (newTop < _startOffset) {
                        newTop = _startOffset;
                    }

                    if (newTop > DayPilotCalendar.originalTop + DayPilotCalendar.originalHeight - _step) {
                        newTop = DayPilotCalendar.originalTop + DayPilotCalendar.originalHeight - _step;
                    }

                    var newHeight = DayPilotCalendar.originalHeight - (newTop - DayPilotCalendar.originalTop);

                    if (newHeight < _step) {
                        newHeight = _step;
                    }
                    else {
                        DayPilotCalendar.resizingShadow.style.top = newTop + 'px';
                    }

                    DayPilotCalendar.resizingShadow.style.height = (newHeight) + 'px';
                }
            }

            else if (DayPilot.Global.moving) {

                if (!DayPilotCalendar.movingShadow) {
                    // don't start dragging unless a minimal move has been performed
                    var distance = 3;
                    if (DayPilot.distance(mousePos, DayPilotCalendar.originalMouse) > distance) {

                        // fixes the ie8 bug (incorrect offsetX and offsetY cause flickering during move if there are inline elements in the event
                        DayPilotCalendar.movingShadow = calendar._createShadow(DayPilot.Global.moving, !calendar._browser.ie, calendar.shadow);
                        DayPilotCalendar.movingShadow.style.width = (DayPilotCalendar.movingShadow.parentNode.offsetWidth + 1) + 'px';
                    }
                    else {
                        return;
                    }
                }

                //                    DayPilotCalendar.moving.dirty = true;

                if (!calendar.coords) {
                    return;
                }

                var _step = calendar.cellHeight;
                var _startOffset = 0;

                var offset = DayPilotCalendar.moveOffsetY;
                if (!offset) {
                    offset = _step / 2; // for external drag
                }
                if (this.moveBy === "Top") {
                    offset = 0;
                }

                var newTop = Math.floor(((calendar.coords.y - offset - _startOffset) + _step / 2) / _step) * _step + _startOffset;

                if (newTop < _startOffset) {
                    newTop = _startOffset;
                }

                var main = calendar.nav.main;
                var max = main.clientHeight;

                var height = parseInt(DayPilotCalendar.movingShadow.style.height);  // DayPilotCalendar.moving.data.height
                if (newTop + height > max) {
                    newTop = max - height;
                }

                //DayPilotCalendar.movingShadow.parentNode.style.display = 'none';
                DayPilotCalendar.movingShadow.style.top = newTop + 'px';
                //DayPilotCalendar.movingShadow.parentNode.style.display = '';

                var colWidth = main.clientWidth / main.rows[0].cells.length;
                //var column = Math.floor((calendar.coords.x - calendar.hourWidth) / colWidth);
                var column = Math.floor((calendar.coords.x) / colWidth);

                if (column < 0) {
                    column = 0;
                }
                
                if (calendar.rtl) {
                    column = calendar.columnsBottom.length - column - 1;
                }

                var events = calendar.nav.events;
                if (column < events.rows[0].cells.length && column >= 0 && DayPilotCalendar.movingShadow.column !== column) {
                    DayPilotCalendar.movingShadow.column = column;
                    DayPilotCalendar.moveShadow(events.rows[0].cells[column]);
                }

            }

            if (DayPilotCalendar.drag) {

                // drag detected
                if (DayPilotCalendar.gShadow) {
                    document.body.removeChild(DayPilotCalendar.gShadow);
                }
                DayPilotCalendar.gShadow = null;

                if (!DayPilotCalendar.movingShadow && calendar.coords) {
                    var shadow = calendar._createShadow(DayPilotCalendar.drag, false, DayPilotCalendar.drag.shadowType);

                    if (shadow) {
                        DayPilotCalendar.movingShadow = shadow;

                        var now = new DayPilot.Date().getDatePart();

                        var ev = { 'value': DayPilotCalendar.drag.id, 'start': now, 'end': now.addSeconds(DayPilotCalendar.drag.duration), 'text': DayPilotCalendar.drag.text };
                        var event = new DayPilot.Event(ev, calendar);
                        //event.calendar = calendar;
                        //event.root = calendar;
                        event.external = true;

                        DayPilot.Global.moving = {};
                        DayPilot.Global.moving.event = event;
                        DayPilot.Global.moving.helper = {};
                    }
                }

                ev.cancelBubble = true;
            }


        };
        
        this.temp = {};
        this.temp.getPosition = function() {
           var coords = calendar._table.getCellCoords();
           
           if (!coords) {
               return null;
           }
           
           //var td = calendar.nav.main.rows[cellCoords.y + extraCells].cells[cellCoords.x];
           
           var column = calendar.columnsBottom[coords.x];
           
           var cell = {};
           cell.resource = column.id;
           cell.start = new DayPilot.Date(column.start).addHours(calendar._visibleStart(true)).addMinutes(coords.y*calendar.cellDuration);
           cell.end = cell.start.addMinutes(calendar.cellDuration);
                      
           return cell;
        };

        this._table = {};
        this._table.getCellCoords = function() {
            var result = {};
            result.x = 0;
            result.y = 0;
            
            if (!calendar.coords) {
                return null;
            }

            var main = calendar.nav.main;

            var relativeX = calendar.coords.x;

            /*
            var scrolling = this.columnWidthSpec === 'Fixed';
            if (!scrolling) {
                relativeX -= calendar.hourWidth;
            }*/
            var i = 0;
            var col = this.col(main, i);
            while (col && relativeX > col.left) {
                i += 1;
                col = this.col(main, i);
            }
            result.x = i - 1;

            var _startOffset = 0;
            var row = Math.floor((calendar.coords.y - _startOffset) / calendar.cellHeight);
            result.y = row;
            
            if (result.x < 0) {
                return null;
            }

            return result;
        };

        this._table.col = function(table, x) {
            var result = {};
            result.left = 0;
            result.width = 0;

            var cell = table.rows[0].cells[x];

            if (!cell) {
                return null;
            }
            var t = DayPilot.abs(table);
            var c = DayPilot.abs(cell);

            result.left = c.x - t.x;
            result.width = cell.offsetWidth;

            return result;
        };

        this._crosshair = function() {
            this._crosshairHide();

            if (!this.elements.crosshair) {
                this.elements.crosshair = [];
            }

            var cellCoords = this._table.getCellCoords();
            if (!cellCoords) {
                return;
            }

            var column = cellCoords.x;
            var y = Math.floor(cellCoords.y / (60 / calendar.cellDuration) * (60 / calendar.timeHeaderCellDuration));
            
            if (y < 0) {
                return;
            }
            

            if (this.nav.hourTable) { // not accessible when ShowHours = true
                if (y >= this.nav.hourTable.rows.length) {
                    return;
                }

                var vertical = document.createElement("div");
                vertical.style.position = "absolute";
                vertical.style.left = "0px";
                vertical.style.right = "0px";
                vertical.style.top = "0px";
                vertical.style.bottom = "0px";
                vertical.style.opacity = .5;
                vertical.style.backgroundColor = this.crosshairColor;
                vertical.style.opacity = this.crosshairOpacity / 100;
                vertical.style.filter = "alpha(opacity=" + this.crosshairOpacity + ")";

                this.nav.hourTable.rows[y].cells[0].firstChild.appendChild(vertical);
                this.elements.crosshair.push(vertical);
            }

            if (this.nav.header) {
                var horizontal = document.createElement("div");
                horizontal.style.position = "absolute";
                horizontal.style.left = "0px";
                horizontal.style.right = "0px";
                horizontal.style.top = "0px";
                horizontal.style.bottom = "0px";
                horizontal.style.opacity = .5;
                horizontal.style.backgroundColor = this.crosshairColor;
                horizontal.style.opacity = this.crosshairOpacity / 100;
                horizontal.style.filter = "alpha(opacity=" + this.crosshairOpacity + ")";

                var row = this.nav.header.rows[this.headerLevels - 1];
                if (row.cells[column]) {
                    row.cells[column].firstChild.appendChild(horizontal);
                    this.elements.crosshair.push(horizontal);
                }
            }

            if (this.crosshairType === "Header") {
                return;
            }

            var layer = this.nav.crosshair;

            var _startOffset = 0;
            var top = Math.floor(((calendar.coords.y - _startOffset)) / calendar.cellHeight) * calendar.cellHeight + _startOffset;
            //var height = duration * calendar.cellHeight / (60 * calendar.cellDuration);
            var height = calendar.cellHeight;

            var fullh = document.createElement("div");
            fullh.style.position = "absolute";
            fullh.style.left = "0px";
            fullh.style.right = "0px";
            fullh.style.top = top + "px";
            fullh.style.height = height + "px";
            fullh.style.backgroundColor = this.crosshairColor;
            fullh.style.opacity = this.crosshairOpacity / 100;
            fullh.style.filter = "alpha(opacity=" + this.crosshairOpacity + ")";
            fullh.onmousedown = this._onCrosshairMouseDown;

            layer.appendChild(fullh);
            this.elements.crosshair.push(fullh);

            var col = this._table.col(this.nav.main, column);
            height = this.nav.main.clientHeight;

            if (col) {
                var fullv = document.createElement("div");
                fullv.style.position = "absolute";
                fullv.style.left = col.left + "px";
                fullv.style.width = col.width + "px";
                fullv.style.top = "0px";
                fullv.style.height = height + "px";
                fullv.style.backgroundColor = this.crosshairColor;
                fullv.style.opacity = this.crosshairOpacity / 100;
                fullv.style.filter = "alpha(opacity=" + this.crosshairOpacity + ")";
                fullv.onmousedown = this._onCrosshairMouseDown;

                layer.appendChild(fullv);
                this.elements.crosshair.push(fullv);
            }

        };

        this._onCrosshairMouseDown = function(ev) {
            calendar._crosshairHide();

            var cellCoords = calendar._table.getCellCoords();
            var extraCells = 0; // events
            var cell = calendar.nav.main.rows[cellCoords.y + extraCells].cells[cellCoords.x];


            calendar._onCellMouseDown.apply(cell, [ev]);
        };

        this._crosshairHide = function() {
            if (!this.elements.crosshair || this.elements.crosshair.length === 0) {
                return;
            }

            for (var i = 0; i < this.elements.crosshair.length; i++) {
                var e = this.elements.crosshair[i];
                if (e && e.parentNode) {
                    e.parentNode.removeChild(e);
                }
            }
            this.elements.crosshair = [];
        };
        
        this._expandCellProperties = function() {
            if (!this.cellConfig) {
                return;
            }
            
            var config = this.cellConfig;
            
            if (config.vertical) {
                for (var x = 0; x < config.x; x++) {
                    var def = this.cellProperties[x + "_0"];
                    for (var y = 1; y < config.y; y++) {
                        this.cellProperties[x + "_" + y] = def;
                    }
                }
            }
            
            if (config.horizontal) {
                for (var y = 0; y < config.y; y++) {
                    var def = this.cellProperties["0_" + y];
                    for (var x = 1; x < config.x; x++) {
                        this.cellProperties[x + "_" + y] = def;
                    }
                }
            }
            
            if (config["default"]) {
                var def = config["default"];
                for (var y = 0; y < config.y; y++) {
                    for (var x = 0; x < config.x; x++) {
                        if (!this.cellProperties[x + "_" + y]) {
                            this.cellProperties[x + "_" + y] = def;
                        }
                    }
                }                
            }
        };

        this._getProperties = function(x, y) {
            if (!this.cellProperties) {
                return null;
            }
            return this.cellProperties[x + "_" + y];

        };
        
        this._isBusiness = function(x, y) {
            var index = x + '_' + y;
            if (this.cellProperties && this.cellProperties[index]) {
                return this.cellProperties[index].business;
            }
            return false;
        };

        this._getColor = function(x, y) {
            var index = x + '_' + y;
            if (this.cellProperties && this.cellProperties[index]) {
                return this.cellProperties[index].backColor;
            }
            return null;

        };

/*
        this.isBusiness = function(cell) {

            if (cell.start.dayOfWeek() === 0 || cell.start.dayOfWeek() === 6) {
                return false;
            }

            if (this.cellDuration < 720) {
                if (cell.start.getHours() < this.businessBeginsHour || cell.start.getHours() >= this.businessEndsHour) {
                    return false;
                }
                else {
                    return true;
                }
            }

            return true;
        };
*/
/*
        this.onBeforeCellRender = function(args) {
        };
*/
        this._disposeHeader = function() {
            var table = this.nav.header;
            if (table && table.rows) {
                for (var y = 0; y < table.rows.length; y++) {
                    var r = table.rows[y];
                    for (var x = 0; x < r.cells.length; x++) {
                        var c = r.cells[x];
                        c.onclick = null;
                        c.onmousemove = null;
                        c.onmouseout = null;
                    }
                }
            }
            if (!this.fasterDispose) DayPilot.pu(table);
        };

        this._drawHeaderRow = function(level, create) {

            // column headers
            var r = (create) ? this.nav.header.insertRow(-1) : this.nav.header.rows[level - 1];

            var columns = this._getColumns(level);
            var len = columns.length;
            var lastRow = (level === calendar.headerLevels);
            
            for (var i = 0; i < len; i++) {
                var data = columns[i];
                
                if (calendar._api2()) {
                    if (typeof calendar.onBeforeHeaderRender === 'function') {
                        var args = {};
                        args.header = {};
                        // TODO deap copy of children, areas?
                        DayPilot.Util.copyProps(data, args.header, ['id', 'start', 'name', 'html', 'backColor', 'toolTip', 'areas', 'children']);
                        this.onBeforeHeaderRender(args);
                        DayPilot.Util.copyProps(args.header, data, ['html', 'backColor', 'toolTip', 'areas']);
                    }
                }
                
                var nonEmpty = data.getChildren ? true : false;

                var cell = (create) ? r.insertCell(-1) : r.cells[i];
                cell.data = data;

                if (lastRow) { // use the width spec only for the last row, otherwise use colspan
                    // widthpct
                    //cell.style.width = (100.0 / columns.length) + "%";
                }
                else {
                    var colspan = 1;
                    if (nonEmpty) {
                        colspan = data.getChildrenCount(calendar.headerLevels - level + 1);
                    }
                    cell.colSpan = colspan;
                }

                if (nonEmpty) {
                    cell.onclick = this._headerClickDispatch;
                    cell.onmousemove = this._headerMouseMove;
                    cell.onmouseout = this._headerMouseOut;
                    if (data.toolTip) {
                        cell.title = data.toolTip;
                    }
                }
                cell.style.overflow = 'hidden';
                cell.style.padding = '0px';
                cell.style.border = '0px none';
                cell.style.height = (resolved.headerHeight()) + "px";
                
                if (!this.cssOnly) {
                    cell.style.borderLeft = "0px none";
                    if (i !== len - 1) { // last one
                        cell.style.borderRight = "1px solid " + this.borderColor;
                    }
                }

                /*
                if (!this.cssOnly && i === 0) {
                    cell.style.borderLeft = "1px solid " + this.borderColor;
                }
                */

                var div = (create) ? document.createElement("div") : cell.firstChild;

                var scrolling = this.columnWidthSpec === 'Fixed';
                if (scrolling && lastRow) {
                    div.style.width = this.columnWidth + "px";
                }

                if (create) {
                    div.setAttribute("unselectable", "on");
                    div.style.MozUserSelect = 'none';
                    div.style.KhtmlUserSelect = 'none';
                    div.style.WebkitUserSelect = 'none';
                    div.style.position = 'relative';
                    div.style.height = resolved.headerHeight() + "px";

                    if (!this.cssOnly) {
                        div.className = calendar._prefixCssClass('colheader');
                        div.style.cursor = 'default';
                        div.style.fontFamily = this.headerFontFamily;
                        div.style.fontSize = this.headerFontSize;
                        div.style.color = this.headerFontColor;
                        div.style.backgroundColor = data.backColor;
                        div.style.textAlign = 'center';

                        var text = document.createElement("div");
                        text.style.position = 'absolute';
                        text.style.left = '0px';
                        text.style.right = '0px';
                        text.style.top = "0px";
                        text.style.bottom = "0px";
                        text.style.padding = "2px";
                        text.setAttribute("unselectable", "on");
                        

                        if (level !== 1) {
                            text.style.borderTop = '1px solid ' + this.borderColor;
                        }
                        

/*
                        if (calendar.rtl) {
                            if (i === len - 1) { // last one
                                text.style.borderLeft = "1px solid " + data.backColor;
                            }
                            else {
                                text.style.borderLeft = "1px solid " + this.borderColor;
                            }
                        }
                        else {
                            if (i !== len - 1) { // last one
                                text.style.borderRight = "1px solid " + this.borderColor;
                            }
                        }
*/

                        if (this.headerClickHandling !== "Disabled") {
                            text.style.cursor = 'pointer';
                        }
                        div.appendChild(text);
                    }
                    else {
                        div.className = calendar._prefixCssClass('_colheader');

                        var inner = document.createElement("div");
                        inner.className = calendar._prefixCssClass('_colheader_inner');
                        if (data.backColor) {
                            inner.style.background = data.backColor;
                        }
                        div.appendChild(inner);
                    }
                    cell.appendChild(div);
                }
                else {
                    div.style.height = resolved.headerHeight() + "px";
                }
                
                this._updateHeaderActiveAreas(div, data);
                

                if (nonEmpty) {
                    div.firstChild.innerHTML = data.html;
                }
            }

        };
        
        
        this._updateHeaderActiveAreas = function(div, data) {
            
            // delete active areas
            var tobedeleted = [];
            for (var j = 0; j < div.childNodes.length; j++) {
                var node = div.childNodes[j];
                if (node.isActiveArea) {
                    tobedeleted.push(node);
                }
            }

            for (var j = 0; j < tobedeleted.length; j++) {
                var node = tobedeleted[j];
                DayPilot.de(node);
            }

            // areas (permanently visible)
            if (data.areas) {
                var areas = data.areas;
                for (var j = 0; j < areas.length; j++) {
                    var area = areas[j];
                    if (area.v !== 'Visible') {
                        continue;
                    }
                    var o = new DayPilotCalendar.Column(data.id, data.name, data.start);
                    var a = DayPilot.Areas.createArea(div, o, area);
                    div.appendChild(a);
                }
            }
            
        };

/*
        this._widthUnit = function() {
            if (this.width && this.width.indexOf("px") !== -1) {
                return "Pixel";
            }
            return "Percentage";
        };
*/
        this._drawHeader = function() {
            if (!this.showHeader) {
                return;
            }

            var header = this.nav.header;
            var create = true;

            var columns = this._getColumns(calendar.headerLevels, true);
            var len = columns.length;

            if (this.headerCreated && header && header.rows && header.rows.length > 0) {
                create = header.rows[header.rows.length - 1].cells.length !== len;
            }
            
            //if (calendar._browser.ielt9 && create) {
            if (this.headerCreated && calendar._browser.ielt9 && create) {
                DayPilot.de(this.nav.header);
                this._createNavHeader();
                //header = this._drawTopHeaderDiv();
                //this.nav.header = header;
                //this.nav.top.appendChild(header);
            }
            
            while (this.headerCreated && header && header.rows && header.rows.length > 0 && create) {
                if (!this.fasterDispose) DayPilot.pu(header.rows[0]);
                header.deleteRow(0);
            }

            this.headerCreated = true;

            var html = calendar.cornerHTML || calendar.cornerHtml;

            if (!create) {
                // corner        
                var corner = calendar.nav.corner;
                if (corner) {
                    if (!this.cssOnly) {
                        if (calendar.cornerBackColor) {
                            corner.style.backgroundColor = calendar.cornerBackColor;
                        }
                        else {
                            corner.style.backgroundColor = calendar.hourBackColor;
                        }
                    }
                    if (!this.fasterDispose) DayPilot.pu(corner.firstChild);
                    corner.firstChild.innerHTML = html ? html : '';
                }
            }

            for (var i = 0; i < calendar.headerLevels; i++) {
                this._drawHeaderRow(i + 1, create);
            }

            if (!this.showAllDayEvents) {
                return;
            }

            // all day events
            var r = (create) ? this.nav.header.insertRow(-1) : this.nav.header.rows[calendar.headerLevels];

            for (var i = 0; i < len; i++) {
                var data = columns[i];

                var cell = (create) ? r.insertCell(-1) : r.cells[i];
                cell.data = data;
                cell.style.padding = '0px';
                cell.style.border = '0px none';
                cell.style.overflow = 'hidden';
                if (!this.cssOnly) {
                    cell.style.lineHeight = '1.2';
                }

                var div = (create) ? document.createElement("div") : cell.firstChild;

                if (create) {
                    div.setAttribute("unselectable", "on");
                    div.style.MozUserSelect = 'none';
                    div.style.KhtmlUserSelect = 'none';
                    div.style.WebkitUserSelect = 'none';
                    div.style.overflow = 'hidden';
                    div.style.position = "relative";
                    div.style.height = resolved.allDayHeaderHeight() + "px";
                    if (!this.cssOnly) {
                        div.className = this._prefixCssClass("alldayheader");
                        div.style.textAlign = 'center';
                        div.style.backgroundColor = data.backColor;
                        div.style.cursor = 'default';

                        var text = document.createElement("div");
                        text.style.position = "absolute";
                        text.style.left = '0px';
                        text.style.right = '0px';
                        text.style.top = "0px";
                        text.style.bottom = "0px";
                        text.setAttribute("unselectable", "on");
                        text.style.borderTop = '1px solid ' + this.borderColor;
                        div.appendChild(text);

                        if (calendar.rtl) {
                            if (i === len - 1) { // last one
                                text.style.borderLeft = "1px solid " + data.backColor;
                            }
                            else {
                                text.style.borderLeft = "1px solid " + this.borderColor;
                            }
                        }
                        else {
                            if (i !== len - 1) { // last one
                                text.style.borderRight = "1px solid " + this.borderColor;
                            }
                        }
                    }
                    else {
                        div.className = this._prefixCssClass("_alldayheader");

                        var inner = document.createElement("div");
                        inner.className = this._prefixCssClass("_alldayheader_inner");
                        div.appendChild(inner);
                    }


                    cell.appendChild(div);
                }
                div.style.height = resolved.allDayHeaderHeight() + "px";

            }
        };

        this._loadingStart = function() {
            if (this.loadingLabelVisible) {
                this.nav.loading.innerHTML = this.loadingLabelText;
                this.nav.loading.style.top = (this._totalHeaderHeight() + 5) + "px";
                this.nav.loading.style.display = '';
            }
        };

        this._loadingStop = function() {
            if (this.callbackTimeout) {
                window.clearTimeout(this.callbackTimeout);
            }

            this.nav.loading.style.display = 'none';
        };

        this._enableScrolling = function() {
            
            var scrolling = this.columnWidthSpec === 'Fixed';
            
            //this.debug.message("scrolling: " + scrolling);

            var scrollDiv = scrolling ? this.nav.bottomRight : this.nav.scroll;
            if (!this.initScrollPos)
                return;

            scrollDiv.root = this;
            scrollDiv.onscroll = this._scroll;

            // initial position
            if (scrollDiv.scrollTop === 0) {
                scrollDiv.scrollTop = this.initScrollPos - this._autoHiddenPixels();
            }
            else {
                this._scroll();
            }

        };

        this.onCallbackError = function(result, context) {
            alert("Error!\r\nResult: " + result + "\r\nContext:" + context);
        };
        
        this.scrollbarVisible = this._scrollbarVisible;

        this._fixScrollHeader = function() {

            var show = this._scrollbarVisible();
            var visible = !!this.nav.cornerRight;
            
            if (show !== visible) {  // change required
                if (show) {  // show it
                    this._createCornerRightTd();
                }
                else {  // hide it
                    if (this.nav.fullHeader && this.nav.fullHeader.rows[0].cells.length === 3) {
                        var c = this.nav.fullHeader.rows[0].cells[2];
                        if (c.parentNode) {
                            c.parentNode.removeChild(c);
                        }
                    }
                    this.nav.cornerRight = null;
                }
            }

            // now fix the width
            var d = this.nav.cornerRight;

            if (!d) {
                return;
            }
            var w = DayPilot.sw(this.nav.scroll);

            if (!this.cssOnly) {
                if (w >= 3) {
                    d.style.width = (w - 3) + 'px';  // -2 borders, -1 correction
                }
            }
            else {
                if (d) {
                    d.style.width = (w) + 'px';
                }
            }
            return w;
        };

        // interval defined in seconds, minimum 30 seconds
        this._startAutoRefresh = function(forceEnabled) {
            if (forceEnabled) {
                this.autoRefreshEnabled = true;
            }

            if (!this.autoRefreshEnabled) {
                return;
            }

            if (this.autoRefreshCount >= this.autoRefreshMaxCount) {
                return;
            }

            //this.autoRefreshCount = 0; // reset
            this._stopAutoRefresh();

            var interval = this.autoRefreshInterval;
            if (!interval || interval < 10) {
                throw "The minimum autoRefreshInterval is 10 seconds";
            }
            //this.autoRefresh = interval * 1000;
            this.autoRefreshTimeout = window.setTimeout(function() { calendar._doRefresh(); }, this.autoRefreshInterval * 1000);
        };

        this._stopAutoRefresh = function() {
            if (this.autoRefreshTimeout) {
                window.clearTimeout(this.autoRefreshTimeout);
            }
        };

        this._doRefresh = function() {
            if (!DayPilot.Global.resizing && !DayPilot.Global.moving && !DayPilotCalendar.drag && !DayPilotCalendar.selecting) {
                this.autoRefreshCount++;
                this.commandCallBack(this.autoRefreshCommand);
            }
            if (this.autoRefreshCount < this.autoRefreshMaxCount) {
                this.autoRefreshTimeout = window.setTimeout(function() { calendar._doRefresh(); }, this.autoRefreshInterval * 1000);
            }
        };

        this._onResize = function() {
            if (calendar.heightSpec === "Parent100Pct") {
                calendar.setHeight(parseInt(calendar.nav.top.clientHeight, 10));
            }

            calendar._updateHeight();
            calendar._updateScrollLabels();
        };

        this._registerGlobalHandlers = function() {
            if (!DayPilotCalendar.globalHandlers) {
                DayPilotCalendar.globalHandlers = true;
                DayPilot.re(document, 'mousemove', DayPilotCalendar.gMouseMove);
                DayPilot.re(document, 'mouseup', DayPilotCalendar.gMouseUp);
                //DayPilot.re(window, 'unload', DayPilotCalendar.gUnload);
            }
            DayPilot.re(window, 'resize', this._onResize);
        };

        this._onEventMouseDown = function(ev) {
            ev = ev || window.event;
            var button = ev.which || ev.button;

            if (typeof (DayPilotBubble) !== 'undefined') {
                DayPilotBubble.hideActive();
            }

            if ((this.style.cursor === 'n-resize' || this.style.cursor === 's-resize') && button === 1) {
                // set
                DayPilot.Global.resizing = this;
                DayPilotCalendar.originalMouse = DayPilot.mc(ev);
                DayPilotCalendar.originalHeight = this.offsetHeight;
                DayPilotCalendar.originalTop = this.offsetTop;

                // shadow
                // 1 line, moved to scroll.mousemove
                //DayPilotCalendar.resizingShadow = DayPilotCalendar.createShadow(this, false, calendar.shadow);

                // cursor
                //document.body.style.cursor = this.style.cursor;
                calendar.nav.top.style.cursor = this.style.cursor;

                // disabled, causes problems, maybe even leaks
                //this.onclickSave = this.onclick;
                //this.onclick = null;
            }
            else if ((this.style.cursor === 'move' || (calendar.moveBy === 'Full' && calendar.eventMoveHandling !== 'Disabled')) && button === 1) {
                DayPilot.Global.moving = this;
                var helper = DayPilot.Global.moving.helper = {};
                helper.oldColumn = calendar.columnsBottom[this.event.part.dayIndex].id;
                DayPilotCalendar.originalMouse = DayPilot.mc(ev);
                DayPilotCalendar.originalTop = this.offsetTop;

                var offset = DayPilot.mo3(this, ev);
                if (offset) {
                    DayPilotCalendar.moveOffsetY = offset.y;
                }
                else {
                    DayPilotCalendar.moveOffsetY = 0;
                }

                calendar.nav.top.style.cursor = this.style.cursor;
            }

            return false;
        };
        
        this._doBeforeEventRender = function(i) {
            var cache = this._cache.events;
            var data = this.events.list[i];
            var evc = {};
            
            // make a copy
            for (var name in data) {
                evc[name] = data[name];
            }
            
            if (typeof this.onBeforeEventRender === 'function') {
                var args = {};
                args.e = evc;
                this.onBeforeEventRender(args);
            }
            
            cache[i] = evc;
            
        };
        
        this._touch = {};
        var touch = calendar._touch;

        touch.active = false;
        touch.start = null;
        touch.timeout = null;
        
        touch.startcell = null;

        this._touch.getCellCoords = function(x, y) {
            var abs = DayPilot.abs(calendar.nav.main);
            var pos = {x: x - abs.x, y: y - abs.y};

            var w = (calendar.nav.main.clientWidth / calendar.columnsBottom.length);

            var coords = {
                x: Math.floor(pos.x / w), 
                y: Math.floor(pos.y / calendar.cellHeight), 
                toString : function() { return "x: " + this.x + " y:" + this.y; } 
            };

            return coords;
        };
        
        this._touch.startSelecting = function(coords) {

            var cell = calendar.nav.main.rows[coords.y].cells[coords.x];
            touch.startcell = coords;

            calendar.clearSelection();

            // don't use this flag, it's for mouse cursor
            // DayPilotCalendar.selecting = true;
            DayPilotCalendar.column = DayPilotCalendar.getColumn(cell);
            calendar.selectedCells.push(cell);
            DayPilotCalendar.firstSelected = cell;

            DayPilotCalendar.topSelectedCell = cell;
            DayPilotCalendar.bottomSelectedCell = cell;

            calendar._activateSelection();
            
        };
        
        
        this._touch.extendSelection = function(coords) {
            var cell = calendar.nav.main.rows[coords.y].cells[coords.x];

            calendar.clearSelection();

            // new selected cells
            if (coords.y < touch.startcell.y) {
              calendar.selectedCells = DayPilotCalendar.getCellsBelow(cell);
              DayPilotCalendar.topSelectedCell = calendar.selectedCells[0];
              DayPilotCalendar.bottomSelectedCell = DayPilotCalendar.firstSelected;
            }
            else {
              calendar.selectedCells = DayPilotCalendar.getCellsAbove(cell);
              DayPilotCalendar.topSelectedCell = DayPilotCalendar.firstSelected;
              DayPilotCalendar.bottomSelectedCell = calendar.selectedCells[0];
            }

            calendar._activateSelection();
        }; 

        this._touch.onCellTouchStart = function(ev) {
            if (touch.active) {
                return;
            }

            var x = ev.touches[0].pageX;
            var y = ev.touches[0].pageY;
            
            var coords = touch.getCellCoords(x, y);
            
            var holdfor = 500;
            touch.timeout = window.setTimeout(function() {
                touch.active = true;
                touch.startSelecting(coords);
            }, holdfor);
            
        };
        
        this._touch.onCellTouchMove = function(ev) {
            // regular move
            if (!touch.active) {
                window.clearTimeout(touch.timeout);
                return;
            }
            
            
            ev.preventDefault();
            
            if (!ev.touches[0]) {
                return;
            }
            var x = ev.touches[0].pageX;
            var y = ev.touches[0].pageY;

            var coords = touch.getCellCoords(x, y);
            touch.extendSelection(coords);
            
        };
        
        this._touch.onCellTouchEnd = function(ev) {
            if (!touch.active) {
                window.clearTimeout(touch.timeout);  // not sure
                return;
            }
            
            ev.preventDefault();
            
            touch.startcell = null;
            
            var sel = calendar.getSelection();
            sel.toString = function() {
                return "start: " + this.start + "\nend: " + this.end;
            };
            calendar._timeRangeSelectedDispatch(sel.start, sel.end, sel.resource);
            
            // prevent alert-initiated touchstart on iOS
            window.setTimeout(function() {
                touch.active = false;
            }, 500);
            
        };
        
        this._touch.startMoving = function(div, coords) {
            DayPilot.Global.moving = div;
            var helper = DayPilot.Global.moving.helper = {};
            helper.oldColumn = calendar.columnsBottom[div.event.part.dayIndex].id;
            DayPilotCalendar.originalMouse = coords;
            DayPilotCalendar.originalTop = this.offsetTop;

            var abs = DayPilot.abs(div);
            DayPilotCalendar.moveOffsetY = coords.y - abs.y;

            if (!DayPilotCalendar.movingShadow) {
                DayPilotCalendar.movingShadow = calendar._createShadow(DayPilot.Global.moving, !calendar._browser.ie, calendar.shadow);
                DayPilotCalendar.movingShadow.style.width = (DayPilotCalendar.movingShadow.parentNode.offsetWidth + 1) + 'px';
            }
            
        };
        
        // coords relative to main
        this._touch.updateMoving = function(coords) {
            
            var _step = calendar.cellHeight;
            var _startOffset = 0;

            var offset = DayPilotCalendar.moveOffsetY;
            if (!offset) {
              offset = _step / 2; // for external drag
            }

            var newTop = Math.floor(((coords.y - offset - _startOffset) + _step / 2) / _step) * _step + _startOffset;

            if (newTop < _startOffset) {
              newTop = _startOffset;
            }

            var main = calendar.nav.main;
            var max = main.clientHeight;

            var height = parseInt(DayPilotCalendar.movingShadow.style.height);  // DayPilotCalendar.moving.data.height
            if (newTop + height > max) {
              newTop = max - height;
            }

            DayPilotCalendar.movingShadow.style.top = newTop + 'px';

            var colWidth = main.clientWidth / main.rows[0].cells.length;
            var column = Math.floor(coords.x / colWidth);

            if (column < 0) {
              column = 0;
            }

            var events = calendar.nav.events;
            if (column < events.rows[0].cells.length && column >= 0 && DayPilotCalendar.movingShadow.column !== column) {
              DayPilotCalendar.movingShadow.column = column;
              DayPilotCalendar.moveShadow(events.rows[0].cells[column]);
            }
  
        };
        
        this._touch.onEventTouchStart = function(ev) {
            if (touch.active) {
                return;
            }
            
            touch.preventEventTap = false;

            var div = this;
            var x = ev.touches[0].pageX;
            var y = ev.touches[0].pageY;
            var coords  = { x: x, y: y, div: this};
            
            var abs = DayPilot.abs(calendar.nav.scrollable);
            calendar.coords = {x: x - abs.x, y: y - abs.y};
            
            var holdfor = 500;
            touch.timeout = window.setTimeout(function() {
                touch.active = true;
                touch.startMoving(div, coords);
            }, holdfor);
            
        };
        
        this._touch.onMainTouchMove = function(ev) {
            if (touch.timeout) {
                window.clearTimeout(calendar._touch.timeout);
            }
            
            if (DayPilot.Global.moving) {
                ev.preventDefault();
                //alert('moving');
    
                var x = ev.touches[0].pageX;
                var y = ev.touches[0].pageY;

                var abs = DayPilot.abs(calendar.nav.main);
                var coords = {x: x - abs.x, y: y - abs.y };
                touch.updateMoving(coords);
                return;
            }
            
            touch.preventEventTap = true;
        };
        
        this._touch.onMainTouchEnd = function(ev) {
            if (DayPilot.Global.moving) {
                touch.active = false;
                ev.preventDefault();
                
                var top = DayPilotCalendar.movingShadow.offsetTop;

                DayPilotCalendar.deleteShadow(DayPilotCalendar.movingShadow);
                var dpEvent = DayPilot.Global.moving.event;
                var newColumnIndex = DayPilotCalendar.movingShadow.column;

                // stop moving on the client     
                DayPilot.Global.moving = null;
                DayPilotCalendar.movingShadow = null;

                dpEvent.calendar._eventMoveDispatch(dpEvent, newColumnIndex, top);
            }
        };        
        
        this._touch.onEventTouchMove = function(ev) {
            touch.preventEventTap = true;
        };
        
        this._touch.onEventTouchEnd = function(ev) {
            // quick tap
            if (!touch.active) { 
                if (touch.preventEventTap) {
                    return;
                }
                ev.preventDefault();
                calendar._eventClickSingle(this, false);
                return;
            }
            
            touch.active = false;
            if (touch.timeout) {
                window.clearTimeout(calendar._touch.timeout);
                return;
            }
            //ev.preventDefault();
        };

        this._onEventMouseMove = function(ev) {
            // const
            var resizeMargin = 5;
            var moveMargin = Math.max(calendar.durationBarWidth, 10);
            var w = 5;

            var header = (calendar.moveBy === 'Top');

            if (typeof (DayPilotCalendar) === 'undefined') {
                return;
            }

            // position
            var offset = DayPilot.mo3(this, ev);

            if (!offset) {
                return;
            }

            //document.title = "offset.y:" + offset.y;

            var div = this;
            if (!div.active) {
                if (calendar.cssOnly) {
                    DayPilot.Util.addClass(div, calendar._prefixCssClass("_event_hover"));
                }
                DayPilot.Areas.showAreas(div, this.event);
            }

            if (DayPilot.Global.resizing || DayPilot.Global.moving) {
                return;
            }

            var isFirstPart = this.isFirst;
            var isLastPart = this.isLast;

            if (calendar.moveBy === "Disabled" || calendar.moveBy === "None") {
                return;
            }

            if (!header && offset.x <= moveMargin && this.event.client.moveEnabled()) {
                if (isFirstPart) {
                    this.style.cursor = 'move';
                }
                else {
                    this.style.cursor = 'not-allowed';
                }
            }
            else if (!header && offset.y <= resizeMargin && this.event.client.resizeEnabled()) {
                if (isFirstPart) {
                    this.style.cursor = "n-resize";
                    this.dpBorder = 'top';
                }
                else {
                    this.style.cursor = 'not-allowed';
                }
            }
            else if (header && offset.y <= moveMargin && this.event.client.moveEnabled()) {
                this.style.cursor = "move";
            }
            else if (this.offsetHeight - offset.y <= resizeMargin && this.event.client.resizeEnabled()) {
                if (isLastPart) {
                    this.style.cursor = "s-resize";
                    this.dpBorder = 'bottom';
                }
                else {
                    this.style.cursor = 'not-allowed';
                }
            }
            else if (!DayPilot.Global.resizing && !DayPilot.Global.moving) {
                if (this.event.client.clickEnabled())
                    this.style.cursor = 'pointer';
                else
                    this.style.cursor = 'default';
            }

            if (typeof (DayPilotBubble) !== 'undefined' && calendar.bubble && calendar.eventHoverHandling !== 'Disabled') {
                if (this.style.cursor === 'default' || this.style.cursor === 'pointer') {
                    var notMoved = this._lastOffset && offset.x === this._lastOffset.x && offset.y === this._lastOffset.y;
                    if (!notMoved) {
                        this._lastOffset = offset;
                        calendar.bubble.showEvent(this.event);
                    }
                    //calendar.bubble.showEvent(this.event);
                }
                else {
                    // disabled, hiding on click
                    //DayPilotBubble.hideActive();
                }
            }


        };
        
        this._onEventMouseOut = function(ev) {
            if (calendar.cssOnly) {
                DayPilot.Util.removeClass(this, calendar._prefixCssClass("_event_hover"));
            }
            if (calendar.bubble) {
                calendar.bubble.hideOnMouseOut();
            }

            DayPilot.Areas.hideAreas(this, ev);

        };

        this._loadEvents = function(events) {

            if (!events) {
                events = this.events.list;
            }
            else {
                this.events.list = events;
            }

            if (!events) {
                return;
            }
            

            this.allDay = {};
            this.allDay.events = [];
            this.allDay.lines = [];

            var length = events.length;
            var duration = this._duration(true);

            this._cache.pixels = {};

            var loadCache = [];

            this.scrollLabels = [];

            this.minStart = 10000;
            this.maxEnd = 0;

            // make sure it's DayPilot.Date
            this.startDate = new DayPilot.Date(this.startDate);

            for (var i = 0; i < length; i++) {
                var e = events[i];
                e.start = new DayPilot.Date(e.start);
                e.end = new DayPilot.Date(e.end);
            }

            if (typeof this.onBeforeEventRender === 'function') {
                for (var i = 0; i < length; i++) {
                    //var e = events[i];
                    this._doBeforeEventRender(i);
                }
            }

            var isResourcesView = this.viewType === 'Resources';
            
            var visible = this._getVisibleRange();
            
            var allStart = visible.start;
            var allEnd = visible.end;
            
            //calendar.debug.message("allStart: " + allStart);
            
            for (var i = 0; i < this.columnsBottom.length; i++) {
                var scroll = {};
                scroll.minEnd = 1000000;
                scroll.maxStart = -1;
                this.scrollLabels.push(scroll);

                var col = this.columnsBottom[i];
                col.events = [];
                col.lines = [];
                col.blocks = [];

                var colStart = new DayPilot.Date(col.start).addHours(this._visibleStart(true));
                var colStartTicks = colStart.getTime();
                var colEnd = colStart.addTime(duration);
                var colEndTicks = colEnd.getTime();

                if (isResourcesView) {
                    allStart = colStart.getDatePart();
                    allEnd = colEnd.getDatePart();
                }

                for (var j = 0; j < length; j++) {
                    if (loadCache[j]) {
                        continue;
                    }

                    var e = events[j];

                    var start = e.start;
                    var end = e.end;

                    var startTicks = start.getTime();
                    var endTicks = end.getTime();

                    if (endTicks < startTicks) {  // skip invalid events
                        continue;
                    }

                    if (e.allday) {
                        var belongsHere = false;
                        if (calendar.allDayEnd === 'Date') {
                            belongsHere = !(endTicks < allStart.getTime() || startTicks >= allEnd.getTime());
                        }
                        else {
                            belongsHere = !(endTicks <= allStart.getTime() || startTicks >= allEnd.getTime());
                        }
                        if (isResourcesView) {
                            belongsHere = belongsHere && (e.resource === col.id || col.id === "*");
                        }

                        if (belongsHere) {
                            var ep = new DayPilot.Event(e, this);
                            ep.part.start = allStart.getTime() < startTicks ? start : allStart;
                            ep.part.end = allEnd.getTime() > endTicks ? end : allEnd;
                            ep.part.colStart = DayPilot.Date.daysDiff(allStart.d, ep.part.start.d);
                            ep.part.colWidth = DayPilot.Date.daysSpan(ep.part.start.d, ep.part.end.d) + 1;
                           
                            if (isResourcesView) {
                                ep.part.colStart = i;
                                ep.part.colWidth = 1;
                            }

                            this.allDay.events.push(ep);
                            
                            if (typeof this.onBeforeEventRender === 'function') {
                                ep.cache = this._cache.events[j];
                            }
                            
                            // always put into cache, it can have just one box
                            loadCache[j] = true;

                            if (isResourcesView && (ep.part.start.getTime() !== startTicks || ep.part.end.getTime() !== endTicks)) {
                                loadCache[j] = false;
                            }

                        }

                        continue;
                    }

                    // belongs here
                    var belongsHere = false;
                    if (isResourcesView) {
                        belongsHere = (col.id === e.resource) && !(endTicks <= colStartTicks || startTicks >= colEndTicks);
                    }
                    else {
                        belongsHere = !(endTicks <= colStartTicks || startTicks >= colEndTicks) || (endTicks === startTicks && startTicks === colStartTicks);
                    }

                    if (belongsHere) {
                        
                        var ep = new DayPilot.Event(e, calendar);
                        ep.part.dayIndex = i;
                        ep.part.start = colStartTicks < startTicks ? start : colStart;
                        ep.part.end = colEndTicks > endTicks ? end : colEnd;

                        var partStartPixels = this._getPixels(ep.part.start, col.start);
                        var partEndPixels = this._getPixels(ep.part.end, col.start);

                        var top = partStartPixels.top;
                        var bottom = partEndPixels.top;

                        // events in the hidden areas
                        if (top === bottom && (partStartPixels.cut || partEndPixels.cut)) {
                            continue;
                        }

                        ep.part.box = resolved.useBox(endTicks - startTicks);

                        var _startOffset = 0;
                        // continue here **************************
                        if (ep.part.box) {
                            var boxBottom = partEndPixels.boxBottom;

                            ep.part.top = Math.floor(top / this.cellHeight) * this.cellHeight + _startOffset;
                            //ep.Height = Math.max(Math.ceil(boxBottom / this.cellHeight) * this.cellHeight - ep.Top, this.cellHeight - 1) + 1;
                            ep.part.height = Math.max(Math.ceil(boxBottom / this.cellHeight) * this.cellHeight - ep.part.top, this.cellHeight - 1);
                            ep.part.barTop = Math.max(top - ep.part.top - 1, 0);  // minimum 0
                            ep.part.barHeight = Math.max(bottom - top - 2, 1);  // minimum 1
                        }
                        else {
                            ep.part.top = top + _startOffset;
                            ep.part.height = Math.max(bottom - top, 0);
                            ep.part.barTop = 0;
                            ep.part.barHeight = Math.max(bottom - top - 2, 1);
                        }

                        var start = ep.part.top;
                        var end = ep.part.top + ep.part.height;

                        if (start > scroll.maxStart) {
                            scroll.maxStart = start;
                        }
                        if (end < scroll.minEnd) {
                            scroll.minEnd = end;
                        }

                        if (start < this.minStart) {
                            this.minStart = start;
                        }
                        if (end > this.maxEnd) {
                            this.maxEnd = end;
                        }
                        col.events.push(ep);

                        if (typeof this.onBeforeEventRender === 'function') {
                            ep.cache = this._cache.events[j];
                        }

                        if (ep.part.start.getTime() === startTicks && ep.part.end.getTime() === endTicks) {
                            loadCache[j] = true;
                        }
                    }
                }
            }

            // sort events inside rows
            for (var i = 0; i < this.columnsBottom.length; i++) {
                var col = this.columnsBottom[i];
                col.events.sort(this._eventComparer);

                // put into lines
                for (var j = 0; j < col.events.length; j++) {
                    var e = col.events[j];
                    col.putIntoBlock(e);
                }

                for (var j = 0; j < col.blocks.length; j++) {
                    var block = col.blocks[j];
                    block.events.sort(this._eventComparerCustom);
                    for (var k = 0; k < block.events.length; k++) {
                        var e = block.events[k];
                        block.putIntoLine(e);
                    }
                }
            }

            // sort allday events
            this.allDay.events.sort(this._eventComparerCustom);

            this.allDay.putIntoLine = function(ep) {
                var thisCol = this;

                for (var i = 0; i < this.lines.length; i++) {
                    var line = this.lines[i];
                    if (line.isFree(ep.part.colStart, ep.part.colWidth)) {
                        line.push(ep);
                        return i;
                    }
                }

                var line = [];
                line.isFree = function(start, width) {
                    //var free = true;
                    var end = start + width - 1;
                    var max = this.length;

                    for (var i = 0; i < max; i++) {
                        var e = this[i];
                        if (!(end < e.part.colStart || start > e.part.colStart + e.part.colWidth - 1)) {
                            return false;
                        }
                    }

                    return true;
                };

                line.push(ep);

                this.lines.push(line);

                return this.lines.length - 1;
            };

            for (var i = 0; i < this.allDay.events.length; i++) {
                var e = this.allDay.events[i];
                this.allDay.putIntoLine(e);
            }

            var lines = Math.max(this.allDay.lines.length, 1);
            this._cache.allDayHeaderHeight = lines * (resolved.allDayEventHeight() + 2) + 2; // overriding
        };

        this._eventComparer = function(a, b) {
            if (!a || !b || !a.start || !b.start) {
                return 0; // no sorting, invalid arguments
            }

            var byStart = a.start().ticks - b.start().ticks;
            if (byStart !== 0) {
                return byStart;
            }

            var byEnd = b.end().ticks - a.end().ticks; // desc
            return byEnd;
        };

        this._eventComparerCustom = function(a, b) {
            if (!a || !b) {
                //calendar.debug("no sorting, invalid arguments");
                return 0; // no sorting, invalid arguments
            }

            if (!a.data || !b.data || !a.data.sort || !b.data.sort || a.data.sort.length === 0 || b.data.sort.length === 0) { // no custom sorting, using default sorting (start asc, end asc);
                //calendar.debug("using default comparer: " + a.Sort + ' ' + b.Sort);
                return calendar._eventComparer(a, b);
            }
            //calendar.debug("using custom comparer");

            var result = 0;
            var i = 0;
            while (result === 0 && a.data.sort[i] && b.data.sort[i]) {
                if (a.data.sort[i] === b.data.sort[i]) {
                    result = 0;
                }
                else {
                    result = calendar._stringComparer(a.data.sort[i], b.data.sort[i], calendar.sortDirections[i]);
                }
                i++;
            }

            return result;
        };

        this._stringComparer = function(a, b, direction) {
            var asc = (direction !== "desc");
            var aFirst = asc ? -1 : 1;
            var bFirst = -aFirst;

            if (a === null && b === null) {
                return 0;
            }
            // nulls first
            if (b === null) { // b is smaller
                return bFirst;
            }
            if (a === null) {
                return aFirst;
            }

            //return asc ? a.localeCompare(a, b) : -a.localeCompare(a, b);

            var ar = [];
            ar[0] = a;
            ar[1] = b;

            ar.sort();

            return a === ar[0] ? aFirst : bFirst;
        };
        
        this._findEventDiv = function(e) {
            for (var i = 0; i < calendar.elements.events.length; i++) {
                var div = calendar.elements.events[i];
                if (div.event === e || div.event.data === e.data) {
                    return div;
                }
            }
            return null;
        };        

        this.events.find = function(id) {
            if (!calendar.events.list || typeof calendar.events.list.length === 'undefined') {
                return null;
            }
            
            var len = calendar.events.list.length;
            for (var i = 0; i < len; i++) {
                if (calendar.events.list[i].id === id) {
                    return new DayPilot.Event(calendar.events.list[i], calendar);
                }
            }
            return null;
        };
        
        this.events.findRecurrent = function(masterId, time) {
            if (!calendar.events.list || typeof calendar.events.list.length === 'undefined') {
                return null;
            }
            var len = calendar.events.list.length;
            for (var i = 0; i < len; i++) {
                if (calendar.events.list[i].recurrentMasterId === masterId && calendar.events.list[i].start.getTime() === time.getTime()) {
                    return new DayPilot.Event(calendar.events.list[i], calendar);
                }
            }
            return null;
        };

        this.events.update = function(e, data) {
            var params = {};
            params.oldEvent = new DayPilot.Event(e.copy(), calendar);
            params.newEvent = new DayPilot.Event(e.temp(), calendar);

            var action = new DayPilot.Action(calendar, "EventUpdate", params, data);

            e.commit();

            calendar.update();

            return action;
        };


        this.events.remove = function(e, data) {

            var params = {};
            params.e = new DayPilot.Event(e.data, calendar);

            var action = new DayPilot.Action(calendar, "EventRemove", params, data);

            var index = DayPilot.indexOf(calendar.events.list, e.data);
            calendar.events.list.splice(index, 1);

            calendar.update();

            return action;
        };

        this.events.add = function(e, data) {

            e.calendar = calendar;

            if (!calendar.events.list) {
                calendar.events.list = [];
            }

            calendar.events.list.push(e.data);

            var params = {};
            params.e = e;

            var action = new DayPilot.Action(calendar, "EventAdd", params, data);
			
            calendar.update();

            return action;

        };

        this.queue = {};
        this.queue.list = [];
        this.queue.list.ignoreToJSON = true;

        this.queue.add = function(action) {
            if (!action) {
                return;
            }
            if (action.isAction) {
                calendar.queue.list.push(action);
            }
            else {
                throw "DayPilot.Action object required for queue.add()";
            }
        };

        this.queue.notify = function(data) {
            var params = {};
            params.actions = calendar.queue.list;
            calendar._callBack2('Notify', params, data, "Notify");

            calendar.queue.list = [];
        };


        this.queue.clear = function() {
            calendar.queue.list = [];
        };

        this.queue.pop = function() {
            return calendar.queue.list.pop();
        };
		
        this.update = function() {
            if (!this.columnsBottom) {  // not initialized yet, don't update TODO better detection
                return;
            }

            var full = true;
            if (full) {
                //this.columns = null;  // make sure it's created from scratch

                calendar._deleteEvents();

                this._prepareVariables();
                this._prepareColumns();
                this._loadEvents();

                calendar._drawHeader();
                calendar._autoHeaderHeight();
                calendar._deleteScrollLabels();
                calendar._updateMessagePosition();
                calendar._hideSelection();
                calendar._drawMain();
                calendar._activateSelection();
                calendar._drawHourTable();
                calendar._updateHeight();
                calendar._fixScrollHeader();

                this._drawEvents();
                this._drawEventsAllDay();

                calendar._updateScrollLabels();
            }
            else {  // events only
                calendar._deleteEvents();
                calendar._loadEvents();
                calendar._updateHeaderHeight();
                calendar._drawEvents();
                calendar._drawEventsAllDay();
                calendar._updateScrollLabels();
            }

        };
        
        this.show = function() {
            calendar.nav.top.style.display = '';
            calendar._onResize();
            calendar._fixScrollHeader();
        };
        
        this.hide = function() {
            calendar.nav.top.style.display = 'none';
        };     
        
        this.debug = new DayPilot.Debug(this);
		
        this._debug = function(msg, append) {
            if (!this.debuggingEnabled) {
                return;
            }

            if (!calendar.debugMessages) {
                calendar.debugMessages = [];
            }
            calendar.debugMessages.push(msg);

            if (typeof console !== 'undefined') {
                console.log(msg);
            }
        };

        this._getPixels = function(date, start) {
            if (!start) start = this.startDate;

            var startTicks = start.getTime();
            var ticks = date.getTime();

            var cache = this._cache.pixels[ticks + "_" + startTicks];
            if (cache) {
                return cache;
            }

            startTicks = start.addHours(this._visibleStart(true)).getTime();

            var boxTicks = this.cellDuration * 60 * 1000;
            var topTicks = ticks - startTicks;
            var boxOffsetTicks = topTicks % boxTicks;

            var boxStartTicks = topTicks - boxOffsetTicks;
            var boxEndTicks = boxStartTicks + boxTicks;
            if (boxOffsetTicks === 0) {
                boxEndTicks = boxStartTicks;
            }

            // it's linear scale so far
            var result = {};
            result.cut = false;
            result.top = this._ticksToPixels(topTicks);
            result.boxTop = this._ticksToPixels(boxStartTicks);
            result.boxBottom = this._ticksToPixels(boxEndTicks);

            this._cache.pixels[ticks + "_" + startTicks] = result;

            return result;
        };

        this._ticksToPixels = function(ticks) {
            return Math.floor((this.cellHeight * ticks) / (1000 * 60 * this.cellDuration));
        };

        this._prepareVariables = function() {
            this.startDate = new DayPilot.Date(this.startDate);
            this.allDayHeaderHeight = resolved.allDayEventHeight() + 4;
        };

        this._updateHeaderHeight = function() {
            var header = this._totalHeaderHeight();
            var total = this._totalHeight();
            //var scroll = total - header;
            
            if (this.nav.corner) {
                this.nav.corner.style.height = header + "px";
            }
            if (this.nav.cornerRight) {
                this.nav.cornerRight.style.height = header + "px";
            }
            if (this.nav.mid) {
                this.nav.mid.style.height = header + "px";
            }

            if (this.showAllDayEvents && this.nav.header) {
                var row = this.nav.header.rows[this.nav.header.rows.length - 1];
                for (var i = 0; i < row.cells.length; i++) {
                    var column = row.cells[i];
                    column.firstChild.style.height = resolved.allDayHeaderHeight() + "px";
                }
            }
            
            if (this.nav.upperRight) {
                this.nav.upperRight.style.height = header + "px";
            }

            this.nav.scroll.style.top = header + "px";
            this.nav.top.style.height = total + "px";
            /*
            if (this.heightSpec === "Parent100Pct") {
                this.nav.scroll.style.height = scroll + "px";
            }*/
        };

        this._updateHeight = function() {
            var sh = this._getScrollableHeight();
            
            if (this.nav.scroll && sh > 0) {
                this.nav.scroll.style.height = sh + "px";
                this.scrollHeight = calendar.nav.scroll.clientHeight;
                
                // scrolling
                if (this.nav.bottomLeft) {
                    this.nav.bottomLeft.style.height = sh + "px";
                }
                if (this.nav.bottomRight) {
                    this.nav.bottomRight.style.height = sh + "px";
                }
            }
            
            if (this.heightSpec === "Parent100Pct") {
                this.nav.top.style.height = "100%";
            }
            else {
                this.nav.top.style.height = this._totalHeight() + "px";
            }
            
        };

        this.setHeight = function(pixels) {
            if (this.heightSpec !== "Parent100Pct") {
                this.heightSpec = "Fixed";
            }
            if (this.cssOnly) {
                this.height = pixels - (this._totalHeaderHeight());
            }
            else {
                this.height = pixels - (this._totalHeaderHeight() + 3);  // guess value
            }
            this._updateHeight();
        };
        
        this._getDimensionsFromCss = function(className) {
            var div = document.createElement("div");
            div.style.position = "absolute";
            div.style.top = "-2000px";
            div.style.left = "-2000px";
            div.className = this._prefixCssClass(className);
            
            document.body.appendChild(div);
            var height = div.offsetHeight;
            var width = div.offsetWidth;
            document.body.removeChild(div);
            
            var result = {};
            result.height = height;
            result.width = width;
            return result;
        };
		
        this._resolved = {};
        
        var resolved = this._resolved;

        resolved.locale = function() {
            return DayPilot.Locale.find(calendar.locale);
        };
        
        resolved.weekStarts = function() {
            if (typeof calendar.weekStarts !== 'undefined') {
                return calendar.weekStarts;
            }
            else {
                return resolved.locale().weekStarts; // Monday
            }
        };        

        resolved.timeFormat = function() {
            if (calendar.timeFormat !== 'Auto') {
                return calendar.timeFormat;
            }
            return this.locale().timeFormat;
        };
        
        resolved.useBox = function(durationTicks) {
            if (calendar.useEventBoxes === 'Always') {
                return true;
            }
            if (calendar.useEventBoxes === 'Never') {
                return false;
            }
            return durationTicks < calendar.cellDuration * 60 * 1000;
        };
        
        resolved.notifyType = function() {
            var type;
            if (calendar.notifyCommit === 'Immediate') {
                type = "Notify";
            }
            else if (calendar.notifyCommit === 'Queue') {
                type = "Queue";
            }
            else {
                throw "Invalid notifyCommit value: " + calendar.notifyCommit;
            }

            return type;
        };

        resolved.allDayEventHeight = function() {
            if (calendar._cache.allDayEventHeight) {
                return calendar._cache.allDayEventHeight;
            }
            var height = calendar._getDimensionsFromCss("_alldayevent_height").height;
            if (!height) {
                height = calendar.allDayEventHeight;
            }
            calendar._cache.allDayEventHeight = height;
            return height;
        };        
        
        resolved.allDayHeaderHeight = function() {
            if (calendar._cache.allDayHeaderHeight) {
                return calendar._cache.allDayHeaderHeight;
            }
            height = calendar.allDayHeaderHeight;
            calendar._cache.allDayHeaderHeight = height;
            return height;
        }; 

        resolved.headerHeight = function() {
            if (calendar._cache.headerHeight) {
                return calendar._cache.headerHeight;
            }
            var height = calendar._getDimensionsFromCss("_header_height").height;
            if (!height) {
                height = calendar.headerHeight;
            }
            calendar._cache.headerHeight = height;
            return height;
        };        
        
        this._loadFromServer = function() {
            // make sure it has a place to ask
            if (this.backendUrl || typeof WebForm_DoCallback === 'function') {
                return (typeof calendar.events.list === 'undefined') || (!calendar.events.list);
            }
            else {
                return false;
            }
        };

        this._initShort = function() {
            this._prepareVariables();
            this._prepareColumns();
            this._drawTop();
            this._drawHeader();
            this._autoHeaderHeight();
            this._drawMain();
            this._fixScrollHeader();
            this._enableScrolling();
            this._registerGlobalHandlers();
            this._registerDispose();
            DayPilotCalendar.register(this);

            this._onResize(); // adjust the height if 100%

            this._startAutoRefresh();
            this._callBack2('Init');
        };

        this.init = function() {
            
            this.nav.top = document.getElementById(id);
            
            if (!this.nav.top) {
                throw "DayPilot.Calendar.init(): The placeholder element not found: '" + id + "'.";
            }

            if (this.nav.top.dp) {
                return;
            }
            
            var loadFromServer = this._loadFromServer();

            if (loadFromServer) {
                this._initShort();
                this.initialized = true;
                return;
            }
            
            this._prepareVariables();
            this._prepareColumns();
            this._expandCellProperties();

            if (this.events.list) { // are events available?
                this._loadEvents();
            }

            this._drawTop();
            this._drawHeader();
            this._autoHeaderHeight();
            this._drawMain();

            this._show();

            this._fixScrollHeader();
            this._enableScrolling();
            this._registerGlobalHandlers();
            this._registerDispose();
            DayPilotCalendar.register(this);

            if (this.events.list) { // are events available?
                this._updateHeaderHeight();
                this._drawEvents();
                this._drawEventsAllDay();
            }

            this._onResize(); // adjust the height if 100%

            if (this.messageHTML) {
                this.message(this.messageHTML);
            }

            this._fireAfterRenderDetached(null, false);

            this._startAutoRefresh();
            this.initialized = true;
        };

        // communication between components
        this.internal = {}; 
        // DayPilot.Action
        this.internal.invokeEvent = this._invokeEvent;
        // DayPilot.Menu
        this.internal.eventMenuClick = this._eventMenuClick;
        this.internal.timeRangeMenuClick = this._timeRangeMenuClick;
        // DayPilot.Bubble
        this.internal.bubbleCallBack = this._bubbleCallBack;
        this.internal.findEventDiv = this._findEventDiv;
        
        this.internal.eventDeleteDispatch = this._eventDeleteDispatch;

        this.Init = this.init;

    };

    
    var DayPilotCalendar = {};

    // internal selecting
    //DayPilotCalendar.selectedCells = null;
    DayPilotCalendar.topSelectedCell = null;
    DayPilotCalendar.bottomSelectedCell = null;
    DayPilotCalendar.selecting = false;
    DayPilotCalendar.column = null;
    DayPilotCalendar.firstSelected = null;
    DayPilotCalendar.firstMousePos = null;

    // internal resizing
    DayPilotCalendar.originalMouse = null;
    DayPilotCalendar.originalHeight = null;
    DayPilotCalendar.originalTop = null;
    //DayPilotCalendar.resizing = null;
    DayPilotCalendar.globalHandlers = false;

    // internal moving
    //DayPilotCalendar.moving = null;
    //DayPilotCalendar.originalLeft = null;

    // internal editing
    DayPilotCalendar.editing = false;
    DayPilotCalendar.originalText = null;

    // scrollbar width
    //DayPilotCalendar.scrollWidth = null;

    // helpers
    DayPilotCalendar.register = function(calendar) {
        if (!DayPilotCalendar.registered) {
            DayPilotCalendar.registered = [];
        }
        var r = DayPilotCalendar.registered;

        for (var i = 0; i < r.length; i++) {
            if (r[i] === calendar) {
                return;
            }
        }
        r.push(calendar);
    };

/*
    DayPilotCalendar.unregister = function(calendar) {
        var a = DayPilotCalendar.registered;
        if (!a) {
            return;
        }

        var i = DayPilot.indexOf(a, calendar);
        if (i === -1) {
            return;
        }
        a.splice(i, 1);
    };
   */
  
    DayPilotCalendar.unregister = function(calendar) {
        var a = DayPilotCalendar.registered;
        if (a) {
            var i = DayPilot.indexOf(a, calendar);
            if (i !== -1) {
                a.splice(i, 1);
            }
            if (a.length === 0) {
                a = null;
            }
        }

        if (!a) {
            DayPilot.ue(document, 'mousemove', DayPilotCalendar.gMouseMove);
            DayPilot.ue(document, 'mouseup', DayPilotCalendar.gMouseUp);
            //DayPilot.ue(window, 'unload', DayPilotCalendar.gUnload);
            DayPilotCalendar.globalHandlers = false;
        }
    };
    

    DayPilotCalendar.getCellsAbove = function(cell) {
        var array = [];
        var c = DayPilotCalendar.getColumn(cell);

        var tr = cell.parentNode;

        var select = null;
        while (tr && select !== DayPilotCalendar.firstSelected) {
            select = tr.getElementsByTagName("td")[c];
            array.push(select);
            tr = tr.previousSibling;
            while (tr && tr.tagName !== "TR") {
                tr = tr.previousSibling;
            }
        }
        return array;
    };

    DayPilotCalendar.getCellsBelow = function(cell) {
        var array = [];
        var c = DayPilotCalendar.getColumn(cell);
        var tr = cell.parentNode;

        var select = null;
        while (tr && select !== DayPilotCalendar.firstSelected) {
            select = tr.getElementsByTagName("td")[c];
            array.push(select);
            tr = tr.nextSibling;
            while (tr && tr.tagName !== "TR") {
                tr = tr.nextSibling;
            }
        }
        return array;
    };

    DayPilotCalendar.getColumn = function(cell) {
        var i = 0;
        while (cell.previousSibling) {
            cell = cell.previousSibling;
            if (cell.tagName === "TD") {
                i++;
            }
        }
        return i;
    };



    DayPilotCalendar.getShadowColumn = function(object) {
        if (!object) {
            return null;
        }

        var parentTd = object.parentNode;
        while (parentTd && parentTd.tagName !== "TD") {
            parentTd = parentTd.parentNode;
        }

        return parentTd;
    };

    DayPilotCalendar.gMouseMove = function(ev) {

        if (typeof (DayPilotCalendar) === 'undefined') {
            return;
        }

        // quick and dirty inside detection
        // hack, but faster then recursing through the parents
        if (ev.insideMainD) {  // FF
            return;
        }
        else if (ev.srcElement) {  // IE
            if (ev.srcElement.inside) {
                return;
            }
        }

        var mousePos = DayPilot.mc(ev);

        if (DayPilotCalendar.drag) {

            document.body.style.cursor = 'move';
            if (!DayPilotCalendar.gShadow) {
                DayPilotCalendar.gShadow = DayPilotCalendar.createGShadow(DayPilotCalendar.drag.shadowType);
            }

            //if (!DayPilotCalendar.dragInside) {
            var shadow = DayPilotCalendar.gShadow;
            shadow.style.left = mousePos.x + 'px';
            shadow.style.top = mousePos.y + 'px';
            //}

            DayPilot.Global.moving = null;
            DayPilotCalendar.deleteShadow(DayPilotCalendar.movingShadow);
            DayPilotCalendar.movingShadow = null;

        }

        for (var i = 0; i < DayPilotCalendar.registered.length; i++) {
            if (DayPilotCalendar.registered[i]._out) {
                DayPilotCalendar.registered[i]._out();
            }
        }

    };

    DayPilotCalendar.gUnload = function(ev) {

        if (!DayPilotCalendar.registered) {
            return;
        }
        var r = DayPilotCalendar.registered;

        for (var i = 0; i < r.length; i++) {
            var c = r[i];
            //c.dispose();

            DayPilotCalendar.unregister(c);
        }

    };

    DayPilotCalendar.gMouseUp = function(e) {
        var e = e || window.event;

        if (e.preventDefault) { e.preventDefault(); } else { e.returnValue = false; }
        e.cancelBubble = true;
        if (e.stopPropagation) {
            e.stopPropagation();
        }

        if (DayPilot.Global.resizing) {
            if (!DayPilotCalendar.resizingShadow) {
                DayPilot.Global.resizing.style.cursor = 'default';
                //document.body.style.cursor = 'default';
                DayPilot.Global.resizing.event.calendar.nav.top.style.cursor = 'auto';
                DayPilot.Global.resizing = null;
                return;
            }
            
            var dpEvent = DayPilot.Global.resizing.event;
            var border = DayPilot.Global.resizing.dpBorder;
            var height = DayPilotCalendar.resizingShadow.clientHeight;
            var top = DayPilotCalendar.resizingShadow.offsetTop;

            // stop resizing on the client
            DayPilotCalendar.deleteShadow(DayPilotCalendar.resizingShadow);
            DayPilotCalendar.resizingShadow = null;
            DayPilot.Global.resizing.style.cursor = 'default';
            dpEvent.calendar.nav.top.style.cursor = 'auto';
            //document.body.style.cursor = 'default';

            DayPilot.Global.resizing.onclick = null;  // trying to prevent onclick, the event should be always recreated

            DayPilot.Global.resizing = null;

            if (dpEvent.calendar.overlap) { // event overlap
                return;
            }

            // action here
            //if (dirty) {
            dpEvent.calendar._eventResizeDispatch(dpEvent, height, top, border);

            //}
        }
        else if (DayPilot.Global.moving) {
            if (!DayPilotCalendar.movingShadow) {
                DayPilot.Global.moving.event.calendar.nav.top.style.cursor = 'auto';
                DayPilot.Global.moving = null;
                return;
            }

            //var dirty = DayPilotCalendar.moving.dirty;

            var oldColumn = DayPilot.Global.moving.helper.oldColumn;
            //var newColumn = DayPilotCalendar.getShadowColumn(DayPilotCalendar.movingShadow).getAttribute("dpColumn");
            //var newColumnDate = DayPilotCalendar.getShadowColumn(DayPilotCalendar.movingShadow).getAttribute("dpColumnDate");
            var top = DayPilotCalendar.movingShadow.offsetTop;

            DayPilotCalendar.deleteShadow(DayPilotCalendar.movingShadow);

            var dpEvent = DayPilot.Global.moving.event;

            var newColumnIndex = DayPilotCalendar.movingShadow.column;

            var drag = DayPilotCalendar.drag;

            DayPilot.Global.moving.event.calendar.nav.top.style.cursor = 'auto';

            // stop moving on the client     
            //DayPilotCalendar.drag = null;  // will be reset below
            DayPilot.Global.moving = null;
            DayPilotCalendar.movingShadow = null;

            if (drag) {
                if (!dpEvent.calendar.todo) {
                    dpEvent.calendar.todo = {};
                }
                dpEvent.calendar.todo.del = drag.element;
            }

            if (dpEvent.calendar.overlap) { // event overlap
                return;
            }

            //if (dirty) {
            var ev = e || window.event;
            dpEvent.calendar._eventMoveDispatch(dpEvent, newColumnIndex, top, ev, drag);
            //}
        }

        // clean up external drag helpers
        if (DayPilotCalendar.drag) {
            DayPilotCalendar.drag = null;

            document.body.style.cursor = '';
        }

        if (DayPilotCalendar.gShadow) {
            document.body.removeChild(DayPilotCalendar.gShadow);
            DayPilotCalendar.gShadow = null;
        }

        DayPilotCalendar.moveOffsetY = null; // clean for next external drag

    };

    DayPilotCalendar.dragStart = function(element, duration, id, text, type) {
        DayPilot.us(element);

        var drag = DayPilotCalendar.drag = {};
        drag.element = element;
        drag.duration = duration;
        drag.text = text;
        drag.id = id;
        drag.shadowType = type ? type : 'Fill';  // default value

        return false;
    };

    DayPilotCalendar.deleteShadow = function(shadow) {
        if (!shadow) {
            return;
        }
        if (!shadow.parentNode) {
            return;
        }

        //DayPilot.pu(shadow);
        shadow.parentNode.removeChild(shadow);
    };

    DayPilotCalendar.createGShadow = function(type) {

        var shadow = document.createElement('div');
        shadow.setAttribute('unselectable', 'on');
        shadow.style.position = 'absolute';
        shadow.style.width = '100px';
        shadow.style.height = '20px';
        shadow.style.border = '2px dotted #666666';
        shadow.style.zIndex = 101;

        if (type === 'Fill') {    // transparent
            shadow.style.backgroundColor = "#aaaaaa";
            shadow.style.opacity = 0.5;
            shadow.style.filter = "alpha(opacity=50)";
            shadow.style.border = '2px solid #aaaaaa';
        }

        document.body.appendChild(shadow);

        return shadow;
    };

    DayPilotCalendar.moveShadow = function(column) {
        var shadow = DayPilotCalendar.movingShadow;
        //var parent = shadow.parentNode;

        //parent.style.display = 'none';

        shadow.parentNode.removeChild(shadow);
        column.firstChild.appendChild(shadow);
        shadow.style.left = '0px';

        shadow.style.width = (DayPilotCalendar.movingShadow.parentNode.offsetWidth) + 'px';
    };
    
    DayPilotCalendar.Column = function(value, name, date) {
        this.value = value;
        this.id = value;
        this.name = name;
        this.date = new DayPilot.Date(date);
    };

    // publish the API 

    // (backwards compatibility)    
    DayPilot.CalendarVisible.dragStart = DayPilotCalendar.dragStart;
    DayPilot.CalendarVisible.Calendar = DayPilotCalendar.Calendar;

    // experimental jQuery bindings
    if (typeof jQuery !== 'undefined') {
        (function($) {
            $.fn.daypilotCalendar = function(options) {
                var first = null;
                var j = this.each(function() {
                    if (this.daypilot) { // already initialized
                        return;
                    };

                    var daypilot = new DayPilot.Calendar(this.id);
                    this.daypilot = daypilot;
                    for (var name in options) {
                        daypilot[name] = options[name];
                    }
                    daypilot.Init();
                    if (!first) {
                        first = daypilot;
                    }
                });
                if (this.length === 1) {
                    return first;
                }
                else {
                    return j;
                }
            };
        })(jQuery);
    }

    if (typeof Sys !== 'undefined' && Sys.Application && Sys.Application.notifyScriptLoaded) {
        Sys.Application.notifyScriptLoaded();
    }

})();
