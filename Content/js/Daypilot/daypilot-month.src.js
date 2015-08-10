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
if (typeof DayPilotMonth === 'undefined') {
    var DayPilotMonth = DayPilot.MonthVisible = {};
}

(function() {

    var doNothing = function() { };

    if (typeof DayPilot.Month !== 'undefined') {
        return;
    }
    
    // register the default theme
    (function() {
        if (DayPilot.Global.defaultMonthCss) {
            return;
        }
        
        var sheet = DayPilot.sheet();
        
        sheet.add(".month_default_main", "border: 1px solid #aaa;font-family: Tahoma, Arial, sans-serif; font-size: 12px;color: #666;");
        sheet.add(".month_default_cell_inner", "border-right: 1px solid #ddd;border-bottom: 1px solid #ddd;position: absolute;top: 0px;left: 0px;bottom: 0px;right: 0px;background-color: #f9f9f9;");
        sheet.add(".month_default_cell_business .month_default_cell_inner", "background-color: #fff;");
        sheet.add(".month_default_cell_header", "text-align: right;padding-right: 2px;");
        sheet.add(".month_default_header_inner", 'text-align: center; vertical-align: middle;position: absolute;top: 0px;left: 0px;bottom: 0px;right: 0px;border-right: 1px solid #999;border-bottom: 1px solid #999;cursor: default;color: #666;background: #eee;');
        sheet.add(".month_default_message", 'padding: 10px;opacity: 0.9;filter: alpha(opacity=90);color: #ffffff;background: #ffa216;background: -webkit-gradient(linear, left top, left bottom, from(#ffa216), to(#ff8400));background: -webkit-linear-gradient(top, #ffa216 0%, #ff8400);background: -moz-linear-gradient(top, #ffa216 0%, #ff8400);background: -ms-linear-gradient(top, #ffa216 0%, #ff8400);background: -o-linear-gradient(top, #ffa216 0%, #ff8400);background: linear-gradient(top, #ffa216 0%, #ff8400);filter: progid:DXImageTransform.Microsoft.Gradient(startColorStr="#ffa216", endColorStr="#ff8400");');
        sheet.add(".month_default_event_inner", 'position: absolute;top: 0px;bottom: 0px;left: 1px;right: 1px;overflow:hidden;padding: 2px;padding-left: 5px;font-size: 12px;color: #666;background: #fff;background: -webkit-gradient(linear, left top, left bottom, from(#ffffff), to(#eeeeee));background: -webkit-linear-gradient(top, #ffffff 0%, #eeeeee);background: -moz-linear-gradient(top, #ffffff 0%, #eeeeee);background: -ms-linear-gradient(top, #ffffff 0%, #eeeeee);background: -o-linear-gradient(top, #ffffff 0%, #eeeeee);background: linear-gradient(top, #ffffff 0%, #eeeeee);filter: progid:DXImageTransform.Microsoft.Gradient(startColorStr="#ffffff", endColorStr="#eeeeee");border: 1px solid #999;border-radius: 0px;');
        sheet.add(".month_default_event_continueright .month_default_event_inner", "border-top-right-radius: 0px;border-bottom-right-radius: 0px;border-right-style: dotted;");
        sheet.add(".month_default_event_continueleft .month_default_event_inner", "border-top-left-radius: 0px;border-bottom-left-radius: 0px;border-left-style: dotted;");
        sheet.add(".month_default_event_hover .month_default_event_inner", 'background: #fff;background: -webkit-gradient(linear, left top, left bottom, from(#ffffff), to(#e8e8e8));background: -webkit-linear-gradient(top, #ffffff 0%, #e8e8e8);background: -moz-linear-gradient(top, #ffffff 0%, #e8e8e8);background: -ms-linear-gradient(top, #ffffff 0%, #e8e8e8);background: -o-linear-gradient(top, #ffffff 0%, #e8e8e8);background: linear-gradient(top, #ffffff 0%, #e8e8e8);filter: progid:DXImageTransform.Microsoft.Gradient(startColorStr="#ffffff", endColorStr="#e8e8e8");');
        sheet.add(".month_default_selected .month_default_event_inner, .month_default_event_hover.month_default_selected .month_default_event_inner", "background: #ddd;");
        sheet.add(".month_default_shadow_inner", "background-color: #666666;opacity: 0.5;filter: alpha(opacity=50);height: 100%;-moz-border-radius: 5px;-webkit-border-radius: 5px;border-radius: 5px;");
        sheet.commit(); 
        
        // trying to define event height using css
        //sheet.add(".month_default_event_height", "height:50px");
        //sheet.add(".month_default_header_height", "height:50px");
        
        DayPilot.Global.defaultMonthCss = true;
    })();

    var DayPilotMonth = {};

    DayPilot.Month = function(placeholder) {
        this.v = '800';

        this.nav = {};
        this.nav.top = document.getElementById(placeholder);

        var calendar = this;

        this.id = placeholder;
        this.isMonth = true;
        this.api = 2;

        this.hideUntilInit = true;

        this.startDate = new DayPilot.Date(); // today
        this.width = '100%'; // default width is 100%
        this.cssClassPrefix = "month_default";
        this.cellHeight = 100; // default cell height is 100 pixels (it's a minCellHeight, it will be extended if needed)
        this.cellMarginBottom = 0;
        this.allowMultiSelect = true;
        this.autoRefreshCommand = 'refresh';
        this.autoRefreshEnabled = false;
        this.autoRefreshInterval = 60;
        this.autoRefreshMaxCount = 20;
        this.doubleClickTimeout = 300;
        this.eventFontColor = "#000000";
        this.eventFontFamily = "Tahoma";
        this.eventFontSize = "11px";
        this.headerBackColor = '#ECE9D8';
        this.headerFontColor = '#000000';
        this.headerFontFamily = "Tahoma";
        this.headerFontSize = "10pt";
        this.headerHeight = 20;
        this.heightSpec = "Auto";
        this.weekStarts = 1; // Monday
        this.innerBorderColor = '#cccccc';
        this.borderColor = 'black';
        this.eventHeight = 25;
        this.cellHeaderHeight = 16;
        this.numberFormat = null;
        this.clientState = {};

        this.afterRender = function() { };
        this.backColor = '#FFFFD5';
        this.nonBusinessBackColor = '#FFF4BC';
        this.cellHeaderBackColor = '';
        this.cellHeaderFontColor = '#000000';
        this.cellHeaderFontFamily = 'Tahoma';
        this.cellHeaderFontSize = '10pt';
        this.cssOnly = true;
        this.eventBackColor = 'White';
        this.eventBorderColor = 'Black';
        this.eventCorners = "Regular";
        this.eventFontColor = '#000000';
        this.eventFontFamily = 'Tahoma';
        this.eventFontSize = '11px';
        this.cellWidth = 14.285; // internal, 7 cells per row
        this.lineSpace = 1;
        this.locale = "en-us";
        this.messageHideAfter = 5000;
        this.notifyCommit = 'Immediate'; // or 'Queue'

        this.eventMoveToPosition = false;

        this.eventTextLayer = 'Top';
        this.eventStartTime = true;
        this.eventEndTime = true;
        this.eventTextAlignment = 'Center';
        this.eventTextLeftIndent = 20;

        this.showWeekend = true;
        this.cellMode = false;
        this.shadowType = "Fill";

        this.eventTimeFontColor = 'gray';
        this.eventTimeFontFamily = 'Tahoma';
        this.eventTimeFontSize = '8pt';

        this.viewType = 'Month';
        this.weeks = 1;

        this.eventClickHandling = 'Enabled';
        this.eventDoubleClickHandling = 'Enabled';
        this.eventMoveHandling = 'Update';
        this.eventResizeHandling = 'Update';
        this.eventRightClickHandling = 'ContextMenu';
        this.eventSelectHandling = 'Update';
        this.headerClickHandling = "Enabled";
        this.timeRangeSelectedHandling = 'Enabled';
        this.timeRangeDoubleClickHandling = 'Enabled';

        this.backendUrl = null;
        this.cellEvents = [];
        //this.eventDivs = [];

        this.elements = {};
        this.elements.events = [];

        this._cache = {};
        this._cache.events = {}; // register DayPilotMonth.Event objects here, key is the data event, reset during drawevents

        this.events = {};

        this.autoRefreshCount = 0;

        this._updateView = function(result, context) {

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

                calendar._fireAfterRenderDetached(result.CallBackData, true);

                if (result.Message) {
                    calendar.message(result.Message);
                }
                return;
            }

            // config
            if (result.VsUpdate) {
                var vsph = document.createElement("input");
                vsph.type = 'hidden';
                vsph.name = calendar.id + "_vsupdate";
                vsph.id = vsph.name;
                vsph.value = result.VsUpdate;
                calendar.vsph.innerHTML = '';
                calendar.vsph.appendChild(vsph);
            }

            calendar.events.list = result.Events;

            if (typeof result.TagFields !== 'undefined') {
                calendar.tagFields = result.TagFields;
            }

            if (typeof result.SortDirections !== 'undefined') {
                calendar.sortDirections = result.SortDirections;
            }

            if (result.UpdateType === "Full") {
                // generated
                calendar.cellProperties = result.CellProperties;
                calendar.headerProperties = result.HeaderProperties;

                // properties
                calendar.startDate = result.StartDate;
                if (typeof result.ShowWeekend !== 'undefined') { calendar.showWeekend = result.ShowWeekend; } // number, can be 0
                //calendar.showWeekend = result.ShowWeekend ? result.ShowWeekend : calendar.showWeekend;
                calendar.headerBackColor = result.HeaderBackColor ? result.HeaderBackColor : calendar.headerBackColor;
                calendar.backColor = result.BackColor ? result.BackColor : calendar.backColor;
                calendar.nonBusinessBackColor = result.NonBusinessBackColor ? result.NonBusinessBackColor : calendar.nonBusinessBackColor;
                calendar.locale = result.Locale ? result.Locale : calendar.locale;
                calendar.timeFormat = result.TimeFormat ? result.TimeFormat : calendar.timeFormat;
                if (typeof result.WeekStarts !== 'undefined') { calendar.weekStarts = result.WeekStarts; } // number, can be 0

                calendar.hashes = result.Hashes;
            }

            calendar.multiselect.clear(true);
            calendar.multiselect.initList = result.SelectedEvents;

            calendar._deleteEvents();
            calendar._prepareRows();
            calendar._loadEvents();

            if (result.UpdateType === "Full") {
                calendar._clearTable();
                calendar._drawTable();
            }
            calendar._updateHeight();

            calendar._show();

            calendar._drawEvents();

            calendar._fireAfterRenderDetached(result.CallBackData, true);

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
/*
                return function() {
                    if (calendar.afterRender) {
                        calendar.afterRender(data, isc);
                    }
                };*/
            };

            window.setTimeout(afterRenderDelayed(data, isCallBack), 0);
        };

        this._api2 = function() {
            return calendar.api === 2;
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

        this._loadEvents = function() {
            
            if (!this.events.list) {
                return;
            }
            
            if (typeof this.onBeforeEventRender === 'function') {
                var length = this.events.list.length;
                for (var i = 0; i < length; i++) {
                    this._doBeforeEventRender(i);
                }
            }
            
            if (this.cellMode) {
                this._loadEventsCells();
            }
            else {
                this._loadEventsRows();
            }

        };

        this._loadingStop = function() {
            // not implemented yet
        };

        this._loadEventsRows = function() {
            
            if (!this.events.list) {
                return;
            }
            
            // prepare rows and columns
            for (var x = 0; x < this.events.list.length; x++) {
                var e = this.events.list[x];
                e.start = new DayPilot.Date(e.start).d;
                e.end = new DayPilot.Date(e.end).d;
                if (e.start.getTime() > e.end.getTime()) { // skip invalid events, zero duration allowed
                    continue;
                }
                for (var i = 0; i < this.rows.length; i++) {
                    var row = this.rows[i];
                    if (row.belongsHere(e)) {
                        var ep = new DayPilot.Event(e, calendar);
                        row.events.push(ep);
                        
                        if (typeof this.onBeforeEventRender === 'function') {
                            ep.cache = this._cache.events[x];
                        }                        
                    }
                }
            }

            // arrange events into lines
            for (var ri = 0; ri < this.rows.length; ri++) {
                var row = this.rows[ri];
                row.events.sort(this._eventComparerCustom);

                for (var ei = 0; ei < this.rows[ri].events.length; ei++) {
                    var ev = row.events[ei];
                    var colStart = row.getStartColumn(ev);
                    var colWidth = row.getWidth(ev);
                    //var line = row.putIntoLine(ev, colStart, colWidth, ri);
                    row.putIntoLine(ev, colStart, colWidth, ri);
                }
            }

        };

        this._loadEventsCells = function() {
            this.cellEvents = [];
            for (var x = 0; x < this._getColCount(); x++) {
                this.cellEvents[x] = [];

                for (var y = 0; y < this.rows.length; y++) {
                    var cell = {};
                    var d = DayPilot.Date.addDays(this.firstDate, y * 7 + x);

                    cell.start = d;
                    cell.end = DayPilot.Date.addDays(d, 1);
                    cell.events = [];

                    this.cellEvents[x][y] = cell;
                }
            }

            // prepare rows and columns
            for (var i = 0; i < this.events.list.length; i++) {
                var e = this.events.list[i];
                e.start = new DayPilot.Date(e.start);
                e.end = new DayPilot.Date(e.start); // we ignore the end date in cellMode
                if (e.start.getTime() > e.end.getTime()) { // skip invalid events, zero duration allowed
                    continue;
                }

                for (var x = 0; x < this._getColCount(); x++) {
                    for (var y = 0; y < this.rows.length; y++) {
                        var cell = this.cellEvents[x][y];
                        if (e.start.getTime() >= cell.start.getTime() && e.start.getTime() < cell.end.getTime()) {
                            var ep = new DayPilot.Event(e, calendar);
                            cell.events.push(ep);
                            
                            if (typeof this.onBeforeEventRender === 'function') {
                                ep.cache = this._cache.events[i];
                            }                            
                        }
                    }
                }
            }

            for (var x = 0; x < this._getColCount(); x++) {
                for (var y = 0; y < this.rows.length; y++) {
                    var cell = this.cellEvents[x][y];
                    cell.events.sort(this._eventComparerCustom);
                }
            }
        };

        this._deleteEvents = function() {
            for (var i = 0; i < this.elements.events.length; i++) {
                var e = this.elements.events[i];
                e.event = null;
                e.click = null;
                e.parentNode.removeChild(e);
            }

            this.elements.events = [];

        };

        this._drawEvents = function() {
            this._cache.events = {};  // reset DayPilotMonth.Event object cache

            if (this.cellMode) {
                this._drawEventsCells();
            }
            else {
                this._drawEventsRows();
            }

            this.multiselect.redraw();

        };

        this._drawEventsCells = function() {
            this.elements.events = [];

            for (var x = 0; x < this._getColCount(); x++) {
                for (var y = 0; y < this.rows.length; y++) {
                    var cell = this.cellEvents[x][y];
                    var div = this.cells[x][y];

                    for (var i = 0; i < cell.events.length; i++) {

                        var ep = cell.events[i];
                        
                        //var eventPart = {};
                        //eventPart.event = cell.events[i];
                        ep.part.colStart = x;
                        ep.part.colWidth = 1;
                        ep.part.row = y;
                        ep.part.line = i;
                        ep.part.startsHere = true;
                        ep.part.endsHere = true;

                        this._drawEvent(ep);

                    }
                }
            }

        };

        this._drawEventsRows = function() {
            this.elements.events = [];

            // draw events
            for (var ri = 0; ri < this.rows.length; ri++) {
                var row = this.rows[ri];

                for (var li = 0; li < row.lines.length; li++) {
                    var line = row.lines[li];

                    for (var pi = 0; pi < line.length; pi++) {
                        this._drawEvent(line[pi]);
                    }
                }
            }

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
                return calendar._eventComparer(a, b);
            }

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

        this._drawShadow = function(x, y, line, width, offset, e) {

            if (!offset) {
                offset = 0;
            }

            var remains = width;

            this.shadow = {};
            this.shadow.list = [];
            this.shadow.start = { x: x, y: y };
            this.shadow.width = width;

            if (this.eventMoveToPosition) {
                remains = 1;
                this.shadow.position = line;
            }

            // something before the first day
            var hidden = y * 7 + x - offset;
            if (hidden < 0) {
                remains += hidden;
                x = 0;
                y = 0;
            }

            var remainingOffset = offset;
            while (remainingOffset >= 7) {
                y--;
                remainingOffset -= 7;
            }
            if (remainingOffset > x) {
                var plus = 7 - this._getColCount();
                if (remainingOffset > (x + plus)) {
                    y--;
                    x = x + 7 - remainingOffset;
                }
                else {
                    remains = remains - remainingOffset + x;
                    x = 0;
                }
            }
            else {
                x -= remainingOffset;
            }

            if (y < 0) {
                y = 0;
                x = 0;
            }

            var cursor = null;
            if (DayPilotMonth.resizingEvent) {
                cursor = 'w-resize';
            }
            else if (DayPilotMonth.movingEvent) {
                cursor = "move";
            }

            this.nav.top.style.cursor = cursor;

            while (remains > 0 && y < this.rows.length) {
                var drawNow = Math.min(this._getColCount() - x, remains);
                var row = this.rows[y];

                /*            
                if (!row) {
                return;
                }
                */

                var top = this._getRowTop(y);
                var height = row.getHeight();
                /*
                if (!this.cssOnly) {
                top += 1;
                }
                */

                if (this.eventMoveToPosition) {
                    top = this._getEventTop(y, line);
                    height = 2;
                }

                var shadow = document.createElement("div");
                shadow.setAttribute("unselectable", "on");
                shadow.style.position = 'absolute';
                shadow.style.left = (this._getCellWidth() * x) + '%';
                shadow.style.width = (this._getCellWidth() * drawNow) + '%';
                shadow.style.top = (top) + 'px';
                shadow.style.height = (height) + 'px';
                shadow.style.cursor = cursor;

                var inside = document.createElement("div");
                inside.setAttribute("unselectable", "on");
                shadow.appendChild(inside);

                if (this.cssOnly) {
                    shadow.className = this._prefixCssClass("_shadow");
                    inside.className = this._prefixCssClass("_shadow_inner");
                }

                if (!this.cssOnly) {
                    inside.style.position = "absolute";
                    inside.style.top = "0px";
                    inside.style.right = "0px";
                    inside.style.left = "0px";
                    inside.style.bottom = "0px";
                    /*
                    inside.style.marginLeft = '1px';
                    inside.style.height = ( - 5) + 'px'; // 4 for borders
                    */

                    if (this.shadowType === 'Fill') {       // transparent shadow    
                        inside.style.backgroundColor = "#aaaaaa";
                        inside.style.opacity = 0.5;
                        inside.style.filter = "alpha(opacity=50)";
                        //inside.style.border = '2px solid #aaaaaa';
                        if (e && e.event) {
                            inside.style.overflow = 'hidden';
                            inside.style.fontSize = this.eventFontSize;
                            inside.style.fontFamily = this.eventFontFamily;
                            inside.style.color = this.eventFontColor;
                            inside.innerHTML = e.event.client.innerHTML() ? e.event.client.innerHTML() : e.event.text();
                        }
                    }
                    else {
                        inside.style.border = '2px dotted #666666';
                    }

                }
                
                var ref = this.nav.events;

                ref.appendChild(shadow);
                this.shadow.list.push(shadow);

                remains -= (drawNow + 7 - this._getColCount());
                x = 0;
                y++;
            }

        };

        this._clearShadow = function() {
            if (this.shadow) {
                var ref = this.nav.events;
                for (var i = 0; i < this.shadow.list.length; i++) {
                    ref.removeChild(this.shadow.list[i]);
                }
                this.shadow = null;
                this.nav.top.style.cursor = '';
            }

        };

        this._getEventTop = function(row, line) {
            //var top = this.headerHeight;
            var top = 0;
            for (var i = 0; i < row; i++) {
                top += this.rows[i].getHeight();
            }
            top += this.cellHeaderHeight; // space on top
            top += line * resolved.lineHeight();
            return top;
        };

        this._getDateFromCell = function(x, y) {
            return DayPilot.Date.addDays(this.firstDate, y * 7 + x);
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

        this._drawEvent = function(ep, cellMode) { // row, startCol, widthCols, text
            var cellMode = this.cellMode;

            //var ev = ep.data;
            var row = ep.part.row;
            var line = ep.part.line;
            var colStart = ep.part.colStart;
            var colWidth = ep.part.colWidth;
            
            var cache = ep.cache || ep.data;

            var left = cellMode ? 0 : this._getCellWidth() * (colStart);
            var width = cellMode ? 100 : this._getCellWidth() * (colWidth);
            var top = cellMode ? line * resolved.lineHeight() : this._getEventTop(row, line);

            var e = document.createElement("div");
            e.setAttribute("unselectable", "on");
            e.style.height = resolved.eventHeight() + 'px';

            if (!this.cssOnly) {
                e.style.fontFamily = this.eventFontFamily;
            }
            else {  // can be used always?
                e.style.position = "relative";
                e.style.overflow = "hidden";
                e.className = this._prefixCssClass("_event");
            }

            if (cache.cssClass) {
                DayPilot.Util.addClass(e, cache.cssClass);
            }

            e.event = ep;

            if (cellMode) {
                e.style.marginRight = "2px";
                e.style.marginBottom = "2px";
                //e.style.position = 'relative';
            }
            else {
                e.style.width = width + '%';
                e.style.position = 'absolute';
                e.style.left = left + '%';
                e.style.top = top + 'px'; // plus space on top
            }

            if (this.showToolTip && cache.toolTip && !this.bubble) {
                e.title = cache.toolTip;
            }

            e.onclick = this._onEventClick;;
            e.ondblclick = this._onEventDoubleClick;
            e.oncontextmenu = this._onEventContextMenu;
            e.onmousedown = this._onEventMouseDown;
            e.onmousemove = this._onEventMouseMove;
            e.onmouseout = this._onEventMouseOut;
            
            e.ontouchstart = touch.onEventTouchStart;
            e.ontouchmove = touch.onEventTouchMove;
            e.ontouchend = touch.onEventTouchEnd;

            if (!this.cssOnly) {
                var back = (ep.client.backColor()) ? ep.client.backColor() : this.eventBackColor;

                var inner = document.createElement("div");
                inner.setAttribute("unselectable", "on");
                inner.style.height = (resolved.eventHeight() - 2) + 'px';
                inner.style.overflow = 'hidden';
                /*
                inner.style.position = 'relative';
                inner.style.marginLeft = '2px';
                inner.style.marginRight = '1px';
                */
                inner.style.position = "absolute";
                inner.style.left = "2px";
                inner.style.right = "2px";

                inner.style.paddingLeft = '2px';
                inner.style.border = '1px solid ' + calendar.eventBorderColor;
                inner.style.backgroundColor = back;
                inner.style.fontFamily = "";
                inner.className = this._prefixCssClass("event");

                if (resolved.rounded()) {
                    inner.style.MozBorderRadius = "5px";
                    inner.style.webkitBorderRadius = "5px";
                    inner.style.borderRadius = "5px";
                }

                var inside = [];

                // display properties
                var textOnTop = this.eventTextLayer === 'Top';
                var textLeft = this.eventStartTime;
                var textRight = this.eventEndTime;
                var textAlign = this.eventTextAlignment;
                var textIndent = this.eventTextLeftIndent;

                var useFloats = this.eventTextLayer === 'Floats';
                // left
                //if (textLeft && (DayPilot.Date.getTime(ev.Start) != 0 || !eventPart.startsHere)) {
                if (useFloats) {
                    if (textLeft) {
                        inside.push("<div unselectable='on' style='float:left; font-size:");
                        inside.push(this.eventTimeFontSize);
                        inside.push(";color:");
                        inside.push(this.eventTimeFontColor);
                        inside.push(";font-family:");
                        inside.push(this.eventTimeFontFamily);
                        inside.push("'>");
                        inside.push(DayPilot.Date.hours(ep.start().d, calendar.timeFormat === 'Clock12Hours'));
                        inside.push("</div>");
                    }
                    if (textRight) {
                        inside.push("<div unselectable='on' style='float:right;font-size:");
                        inside.push(this.eventTimeFontSize);
                        inside.push(";color:");
                        inside.push(this.eventTimeFontColor);
                        inside.push(";font-family:");
                        inside.push(this.eventTimeFontFamily);
                        inside.push("'>");
                        inside.push(DayPilot.Date.hours(ep.end().d, calendar.timeFormat === 'Clock12Hours'));
                        inside.push("</div>");
                    }

                    inside.push("<div unselectable='on' style='");
                    inside.push("font-size:");
                    inside.push(this.eventFontSize);
                    inside.push(";color:");
                    inside.push(this.eventFontColor);
                    inside.push(";font-family:");
                    inside.push(this.eventFontFamily);
                    if (textAlign === 'Center') {
                        inside.push(";text-align:center;");
                    }
                    inside.push("'>");
                    if (ep.client.innerHTML()) {
                        inside.push(ep.client.innerHTML());
                    }
                    else {
                        inside.push(ep.text());
                    }
                    inside.push("</div>");

                }
                else {
                    if (textLeft) {
                        if (textAlign === 'Left') {
                            inside.push("<div unselectable='on' style='position:absolute;text-align:left;height:1px;font-size:1px;width:100%'><div unselectable='on' style='font-size:");
                            inside.push(this.eventTimeFontSize);
                            inside.push(";color:");
                            inside.push(this.eventTimeFontColor);
                            inside.push(";font-family:");
                            inside.push(this.eventTimeFontFamily);
                            inside.push(";text-align:right;");
                            inside.push("width:");
                            inside.push(textIndent - 4);
                            inside.push("px;");
                            inside.push("><span style='background-color:");
                        }
                        else {
                            inside.push("<div unselectable='on' style='position:absolute;text-align:left;height:1px;font-size:1px;width:100%'><div unselectable='on' style='font-size:");
                            //inside.push("<div unselectable='on' style='float:left;text-align:left;height:1px;font-size:1px;width:100%'><div unselectable='on' style='font-size:");
                            inside.push(this.eventTimeFontSize);
                            inside.push(";color:");
                            inside.push(this.eventTimeFontColor);
                            inside.push(";font-family:");
                            inside.push(this.eventTimeFontFamily);
                            inside.push(";'><span style='background-color:");
                        }

                        //inside.push("<div unselectable='on' style='position:absolute;text-align:left;height:1px;font-size:1px;width:100%'><div unselectable='on' style='font-size:8pt;color:gray'><span style='background-color:");
                        inside.push(back);
                        inside.push("' unselectable='on'>");
                        if (ep.part.startsHere) {
                            inside.push(DayPilot.Date.hours(ep.start().d, calendar.timeFormat === 'Clock12Hours'));
                        }
                        else {
                            inside.push("~");
                        }
                        inside.push("</span></div></div>");
                    }

                    // right
                    //if (textRight && (DayPilot.Date.getTime(ev.End) != 0 || !eventPart.endsHere)) {
                    if (textRight) {
                        inside.push("<div unselectable='on' style='position:absolute;text-align:right;height:1px;font-size:1px;width:100%'><div unselectable='on' style='margin-right:4px;font-size:");
                        inside.push(this.eventTimeFontSize);
                        inside.push(";color:");
                        inside.push(this.eventTimeFontColor);
                        inside.push(";font-family:");
                        inside.push(this.eventTimeFontFamily);
                        inside.push(";'><span style='background-color:");
                        inside.push(back);
                        inside.push("' unselectable='on'>");
                        if (ep.part.endsHere) {
                            inside.push(DayPilot.Date.hours(ep.end().d, calendar.timeFormat === 'Clock12Hours'));
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
                        inside.push(resolved.eventHeight() - 2);
                        inside.push("px;");
                        inside.push(";overflow:hidden;text-align:left;padding-left:");
                        inside.push(left);
                        inside.push("px;font-size:");
                        inside.push(this.eventFontSize);
                        inside.push(";color:");
                        inside.push(this.eventFontColor);
                        inside.push(";font-family:");
                        inside.push(this.eventFontFamily);
                        inside.push("' unselectable='on'>");
                        if (ep.client.innerHTML()) {
                            inside.push(ep.client.innerHTML());
                        }
                        else {
                            inside.push(ep.text());
                        }
                        inside.push("</div>");
                    }
                    else if (textAlign === 'Center') {
                        if (textOnTop) {

                            // alternate elements order: text on top
                            inside.push("<div style='position:absolute; text-align:center; width: 98%; height:1px; font-size: 1px;'>");
                            inside.push("<span style='background-color:");
                            inside.push(back);
                            inside.push(";font-size:");
                            inside.push(this.eventFontSize);
                            inside.push(";color:");
                            inside.push(this.eventFontColor);
                            inside.push(";font-family:");
                            inside.push(this.eventFontFamily);
                            inside.push("' unselectable='on'>");

                            if (ep.client.innerHTML()) {
                                inside.push(ep.client.innerHTML());
                            }
                            else {
                                inside.push(ep.text());
                            }

                            inside.push("</span>");
                            inside.push("</div>");
                        }
                        else {
                            inside.push("<div style='margin-top:12px;height:");
                            inside.push(resolved.eventHeight() - 2);
                            inside.push("px;");
                            inside.push(";overflow:hidden;text-align:center;font-size:");
                            inside.push(this.eventFontSize);
                            inside.push(";color:");
                            inside.push(this.eventFontColor);
                            inside.push(";font-family:");
                            inside.push(this.eventFontFamily);
                            inside.push("' unselectable='on'>");
                            if (ep.client.innerHTML()) {
                                inside.push(ep.client.innerHTML());
                            }
                            else {
                                inside.push(ep.text());
                            }
                            inside.push("</div>");

                        }
                    }
                }
                inner.innerHTML = inside.join('');

                e.appendChild(inner);
            }
            else {
                if (!ep.part.startsHere) {
                    DayPilot.Util.addClass(e, this._prefixCssClass("_event_continueleft"));
                }
                if (!ep.part.endsHere) {
                    DayPilot.Util.addClass(e, this._prefixCssClass("_event_continueright"));
                }
                
                var inner = document.createElement("div");
                inner.setAttribute("unselectable", "on");
                inner.className = this._prefixCssClass("_event_inner");
                if (ep.client.innerHTML()) {
                    inner.innerHTML = ep.client.innerHTML();
                }
                else {
                    inner.innerHTML = ep.text();
                }
                if (cache.backColor) {
                    inner.style.background = cache.backColor;
                }
                e.appendChild(inner);
            }
            
            if (cache.areas) {
                var areas = cache.areas;
                for (var i = 0; i < areas.length; i++) {
                    var area = areas[i];
                    if (area.v !== 'Visible') {
                        continue;
                    }
                    var a = DayPilot.Areas.createArea(e, ep, area);
                    e.appendChild(a);
                }
            }


            this.elements.events.push(e);

            if (cellMode) {
                this.cells[colStart][row].body.appendChild(e);
            }
            else {
                this.nav.events.appendChild(e);
            }

            if (calendar.multiselect._shouldBeSelected(e.event)) {
                calendar.multiselect.add(e.event, true);
            }

            var div = e;

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
                calendar.afterEventRender(e.event, e);
            }*/

        };
        
        this._onEventClick = function(ev) {
            if (touch.start) {
                return;
            }
            
            calendar._eventClickDispatch(this, ev);  
        };
        
        this._onEventDoubleClick = function(ev) {
            calendar._eventDoubleClickDispatch(this, ev);
        };
        
        this._onEventMouseMove = function(ev) {
            var e = this;
            var ep = e.event;
            
            if (typeof (DayPilotMonth) === 'undefined') {
                return;
            }

            if (DayPilotMonth.movingEvent || DayPilotMonth.resizingEvent) {
                return;
            }

            // position
            var offset = DayPilot.mo3(e, ev);
            if (!offset) {
                return;
            }

            DayPilot.Areas.showAreas(e, e.event);

            if (calendar.cssOnly) {
                calendar._findEventDivs(e.event).each(function(div) {
                    DayPilot.Util.addClass(div, calendar._prefixCssClass("_event_hover"));
                });
            }

            var resizeMargin = 6;

            if (!calendar.cellMode && offset.x <= resizeMargin && ep.client.resizeEnabled()) {
                if (ep.part.startsHere) {
                    e.style.cursor = "w-resize";
                    e.dpBorder = 'left';
                }
                else {
                    e.style.cursor = 'not-allowed';
                }
            }
            else if (!calendar.cellMode && e.clientWidth - offset.x <= resizeMargin && ep.client.resizeEnabled()) {
                if (ep.part.endsHere) {
                    e.style.cursor = "e-resize";
                    e.dpBorder = 'right';
                }
                else {
                    e.style.cursor = 'not-allowed';
                }
            }
            else {
                e.style.cursor = 'default';
            }

            if (typeof (DayPilotBubble) !== 'undefined' && calendar.bubble && calendar.eventHoverHandling !== 'Disabled') {
                //if (this.style.cursor == 'default' || this.style.cursor == 'pointer') {
                if (!DayPilotMonth.movingEvent && !DayPilotMonth.resizingEvent) {
                    var notMoved = this._lastOffset && offset.x === this._lastOffset.x && offset.y === this._lastOffset.y;
                    if (!notMoved) {
                        this._lastOffset = offset;
                        calendar.bubble.showEvent(e.event);
                    }
                    //calendar.bubble.showEvent(e.event);
                }
                else {
                    DayPilotBubble.hideActive();
                }
            }

        };
        
        this._onEventMouseOut  = function(ev) {
            var e = this;
            
            if (typeof (DayPilotBubble) !== 'undefined' && calendar.bubble) {
                calendar.bubble.hideOnMouseOut();
            }
            e.style.cursor = '';

            if (calendar.cssOnly) {
                calendar._findEventDivs(e.event).each(function(div) {
                    DayPilot.Util.removeClass(div, calendar._prefixCssClass("_event_hover"));
                });
            }

            DayPilot.Areas.hideAreas(e, ev);

        };
        
        this._onEventContextMenu  = function() {
            var e = this;
            calendar._eventRightClickDispatch(e.event);
            return false;
        };
            
        this._onEventMouseDown = function(ev) {
            if (touch.start) {
                return;
            }
            var e = this;
            var ep = e.event;
            var row = ep.part.row;
            var colStart = ep.part.colStart;
            var line = ep.part.line;
            var colWidth = ep.part.colWidth;

            ev = ev || window.event;
            var button = ev.which || ev.button;

            ev.cancelBubble = true;
            if (ev.stopPropagation) {
                ev.stopPropagation();
            }

            if (button === 1) {
                if (typeof (DayPilotBubble) !== 'undefined' && calendar.bubble) {
                    DayPilotBubble.hideActive();
                }

                DayPilotMonth.movingEvent = null;
                if (this.style.cursor === 'w-resize' || this.style.cursor === 'e-resize') {
                    var resizing = {};
                    resizing.start = {};
                    resizing.start.x = colStart;
                    resizing.start.y = row;
                    resizing.event = e.event;
                    resizing.width = DayPilot.Date.daysSpan(resizing.event.start().d, resizing.event.end().d) + 1;
                    resizing.direction = this.style.cursor;
                    DayPilotMonth.resizingEvent = resizing;
                }
                else if (this.style.cursor === 'move' || ep.client.moveEnabled()) {
                    calendar._clearShadow();

                    var coords = DayPilot.mo3(calendar.nav.events, ev);
                    if (!coords) {
                        return;
                    }

                    //coords.y -= this.parentNode.parentNode.scrollTop;

                    var cell = calendar._getCellBelowPoint(coords.x, coords.y);
                    if (!cell) {
                        return;
                    }

                    var hidden = DayPilot.Date.daysDiff(ep.start(), calendar.rows[row].start);
                    var offset = (cell.y * 7 + cell.x) - (row * 7 + colStart);
                    if (hidden) {
                        offset += hidden;
                    }

                    var moving = {};
                    moving.start = {};
                    moving.start.x = colStart;
                    moving.start.y = row;
                    moving.start.line = line;
                    moving.offset = calendar.eventMoveToPosition ? 0 : offset;
                    moving.colWidth = colWidth;
                    moving.event = e.event;
                    moving.coords = coords;
                    DayPilotMonth.movingEvent = moving;
                }
            }
        };
        
        this.temp = {};
        this.temp.getPosition = function() {
            if (!calendar.coords) {
                return null;
            }
            var cell = calendar._getCellBelowPoint(calendar.coords.x, calendar.coords.y);
            if (!cell) {
                return null;
            }

            var d = new DayPilot.Date(calendar._getDateFromCell(cell.x, cell.y));
            var cell = {};
            cell.start = d;
            cell.end = d.addDays(1);
            return cell;
        };

        this._touch = {};
        var touch = calendar._touch;

        touch.active = false;
        touch.start = false;
        
        touch.timeouts = [];

        touch.onEventTouchStart = function(ev) {
            // iOS
            if (touch.active || touch.start) {
                return;
            }

            touch.clearTimeouts();
            
            touch.start = true;
            touch.active = false;

            var div = this;
            
            var holdfor = 500;
            touch.timeouts.push(window.setTimeout(function() {
                touch.active = true;
                touch.start = false;
                
                var coords = touch.relativeCoords(ev);
                touch.startMoving(div, coords);
                
                ev.preventDefault();

            }, holdfor));
            
            // prevent onMainTouchStart
            ev.stopPropagation();
            
        };
        
        touch.onEventTouchMove = function(ev) {
            touch.clearTimeouts();
            touch.start = false;
        };
        
        touch.onEventTouchEnd = function(ev) {
            touch.clearTimeouts();
            
            // quick tap
            if (touch.start) { 
                calendar._eventClickSingle(this, false);
            }
            
            window.setTimeout(function() {
                touch.start = false;
                touch.active = false;
            }, 500);
        };        
                
        
        touch.onMainTouchStart = function(ev) {
            // prevent after-alert firing on iOS
            if (touch.active || touch.start) {
                return;
            }

            touch.clearTimeouts();

            touch.start = true;
            touch.active = false;

            var holdfor = 500;
            touch.timeouts.push( window.setTimeout(function() {
                touch.active = true;
                touch.start = false;

                ev.preventDefault();

                var coords = touch.relativeCoords(ev);
                touch.startRange(coords);
            }, holdfor));
            
        };
        
        touch.onMainTouchMove = function(ev) {
            touch.clearTimeouts();

            touch.start = false;

            if (touch.active) {
                ev.preventDefault();

                var coords = touch.relativeCoords(ev);

                if (touch.moving) {
                    //ev.preventDefault();
                    touch.updateMoving(coords);
                    return;
                }

                if (touch.range) {
                    touch.updateRange(coords);
                }
            }
        };
        
        touch.onMainTouchEnd = function(ev) {
            touch.clearTimeouts();
            
            if (touch.active) {
                if (touch.moving) {
                    //alert("touchend, moving");
                    var src = touch.moving;

                    // load ref
                    //var calendar = DayPilotMonth.movingEvent.event.calendar;
                    var e = touch.moving.event;
                    var start = calendar.shadow.start;
                    var position = calendar.shadow.position;
                    var offset = touch.moving.offset;

                    // cleanup
                    calendar._clearShadow();
                    touch.moving = null;

                    // fire the event
                    //console.log("eventMoveDispatch");
                    calendar._eventMoveDispatch(e, start.x, start.y, offset, ev, position);

                }

                if (touch.range) {
                    var sel = touch.range;
                    //var calendar = sel.root;

                    var start = new DayPilot.Date(calendar._getDateFromCell(sel.from.x, sel.from.y));
                    var end = start.addDays(sel.width);
                    touch.range = null;
                    calendar._timeRangeSelectedDispatch(start, end);
                }
            }
            
            window.setTimeout(function() {
                touch.start = false;
                touch.active = false;
            }, 500);
            
        };
        
        touch.clearTimeouts = function() {
            for (var i = 0; i < touch.timeouts.length; i++) {
                clearTimeout(touch.timeouts[i]);
            }
            touch.timeouts = [];
        };
        
        touch.relativeCoords = function(ev) {
            var ref = calendar.nav.events;
            
            var x = ev.touches[0].pageX;
            var y = ev.touches[0].pageY;
            var coords  = { x: x, y: y};
            
            var abs = DayPilot.abs(ref);
            var coords = {x: x - abs.x, y: y - abs.y, toString: function() { return "x: " + this.x + ", y:" + this.y; } };
            return coords;
        };

        
        touch.startMoving = function(div, coords) {
            calendar._clearShadow();
            
            ep = div.event;

            var cell = calendar._getCellBelowPoint(coords.x, coords.y);
            if (!cell) {
                return;
            }

            var hidden = DayPilot.Date.daysDiff(ep.start(), calendar.rows[ep.part.row].start);
            var offset = (cell.y * 7 + cell.x) - (ep.part.row * 7 + ep.part.colStart);
            if (hidden) {
                offset += hidden;
            }

            var moving = {};
            moving.start = {};
            moving.start.x = ep.part.colStart;
            moving.start.y = ep.part.row;
            moving.start.line = ep.part.line;
            moving.offset = calendar.eventMoveToPosition ? 0 : offset;
            moving.colWidth = ep.part.colWidth;
            moving.event = ep;
            moving.coords = coords;
            touch.moving = moving;
            
            touch.updateMoving(coords);
        };

        touch.updateMoving = function(coords) {
            var cell = calendar._getCellBelowPoint(coords.x, coords.y);
            if (!cell) {
                return;
            }
            
            var linepos = calendar._linePos(cell);

            calendar._clearShadow();

            var event = touch.moving.event;
            var offset = touch.moving.offset;
            var width = calendar.cellMode ? 1 : DayPilot.Date.daysSpan(event.start().d, event.end().d) + 1;

            if (width < 1) {
                width = 1;
            }
            calendar._drawShadow(cell.x, cell.y, linepos, width, offset, event);
            
        };

        touch.startRange = function(coords) {
            //touch.range = { "root": calendar, "x": x, "y": y, "from": { x: x, y: y }, "width": 1 };

            var cell = calendar._getCellBelowPoint(coords.x, coords.y);
            if (!cell) {
                return;
            }
            
            calendar._clearShadow();

            var range = {};
            range.start = {};
            range.start.x = cell.x;
            range.start.y = cell.y;
            range.x = cell.x; // not necessary
            range.y = cell.y; // not necessary
            range.width = 1;
            
            touch.range = range;
            touch.updateRange(coords);
            
        };
        
        touch.updateRange = function(coords) {
            var cell = calendar._getCellBelowPoint(coords.x, coords.y);
            if (!cell) {
                return;
            }

            calendar._clearShadow();

            var start = touch.range.start;
            var startIndex = start.y * 7 + start.x;
            var cellIndex = cell.y * 7 + cell.x;

            var width = Math.abs(cellIndex - startIndex) + 1;

            if (width < 1) {
                width = 1;
            }

            var shadowStart = startIndex < cellIndex ? start : cell;
            
            touch.range.width = width;
            touch.range.from = { x: shadowStart.x, y: shadowStart.y };

            calendar._drawShadow(shadowStart.x, shadowStart.y, 0, width, 0, null);
            
        };
        
        // overridable
        this.isWeekend = function(date) {
            var sunday = 0;
            var saturday = 6;

            if (date.dayOfWeek() === sunday) {
                return true;
            }
            if (date.dayOfWeek() === saturday) {
                return true;
            }
            return false;
        };

        // returns DayPilot.Date object
        this._lastVisibleDayOfMonth = function() {
            var last = this.startDate.lastDayOfMonth();

            if (this.showWeekend) {
                return last;
            }

            while (this.isWeekend(last)) {
                last = last.addDays(-1);
            }
            return last;
        };

        this._prepareRows = function() {

            if (typeof this.startDate === 'string') {
                this.startDate = DayPilot.Date.fromStringSortable(this.startDate);
            }
            if (this.viewType === 'Month') {
                this.startDate = this.startDate.firstDayOfMonth();
            }
            else {
                this.startDate = this.startDate.getDatePart();
            }
            

            this.firstDate = this.startDate.firstDayOfWeek(resolved.getWeekStart());
            if (!this.showWeekend) {
                var previousMonth = this.startDate.addMonths(-1).getMonth();

                var lastBeforeWeekend = new DayPilot.Date(this.firstDate).addDays(6);
                while (this.isWeekend(lastBeforeWeekend)) {
                    lastBeforeWeekend = lastBeforeWeekend.addDays(-1);
                }

                if (lastBeforeWeekend.getMonth() === previousMonth) {
                    this.firstDate = DayPilot.Date.addDays(this.firstDate, 7);
                }
            }

            //var firstDayOfMonth = DayPilot.Date.firstDayOfMonth(this.year, this.month);
            var firstDayOfMonth = this.startDate;

            var rowCount;

            if (this.viewType === 'Month') {
                var lastVisibleDayOfMonth = this._lastVisibleDayOfMonth().d;
                var count = DayPilot.Date.daysDiff(this.firstDate, lastVisibleDayOfMonth) + 1;
                rowCount = Math.ceil(count / 7);
            }
            else {
                rowCount = this.weeks;
            }

            this.days = rowCount * 7;

            this.rows = [];
            for (var x = 0; x < rowCount; x++) {
                var r = {};
                r.start = DayPilot.Date.addDays(this.firstDate, x * 7);  // start point
                r.end = DayPilot.Date.addDays(r.start, this._getColCount()); // end point
                r.events = []; // collection of events
                r.lines = []; // collection of lines
                r.index = x; // row index
                r.minHeight = this.cellHeight; // default, can be extended during events loading
                r.calendar = this;

                r.belongsHere = function(ev) {
                    if (ev.end.getTime() === ev.start.getTime() && ev.start.getTime() === this.start.getTime()) {
                        return true;
                    }
                    return !(ev.end.getTime() <= this.start.getTime() || ev.start.getTime() >= this.end.getTime());
                };

                r.getPartStart = function(ep) {
                    return DayPilot.Date.max(this.start, ep.start());
                };

                r.getPartEnd = function(ep) {
                    return DayPilot.Date.min(this.end, ep.end());
                };

                r.getStartColumn = function(ep) {
                    var partStart = this.getPartStart(ep);
                    return DayPilot.Date.daysDiff(this.start, partStart);
                };

                r.getWidth = function(ep) {
                    return DayPilot.Date.daysSpan(this.getPartStart(ep), this.getPartEnd(ep)) + 1;
                };

                r.putIntoLine = function(ep, colStart, colWidth, row) {
                    var thisRow = this;

                    for (var i = 0; i < this.lines.length; i++) {
                        var line = this.lines[i];
                        if (line.isFree(colStart, colWidth)) {
                            line.addEvent(ep, colStart, colWidth, row, i);
                            return i;
                        }
                    }

                    var line = [];
                    line.isFree = function(colStart, colWidth) {
                        var free = true;

                        for (var i = 0; i < this.length; i++) {
                            var ep = this[i];
                            if (!(colStart + colWidth - 1 < ep.part.colStart || colStart > ep.part.colStart + ep.part.colWidth - 1)) {
                                free = false;
                            }
                        }

                        return free;
                    };

                    line.addEvent = function(ep, colStart, colWidth, row, index) {
                        //var eventPart = {};
                        //eventPart.event = ev;
                        ep.part.colStart = colStart;
                        ep.part.colWidth = colWidth;
                        ep.part.row = row;
                        ep.part.line = index;
                        ep.part.startsHere = thisRow.start.getTime() <= ep.start().getTime();
                        //if (confirm('r.start: ' + thisRow.start + ' ev.Start: ' + ev.Start)) thisRow = null;
                        ep.part.endsHere = thisRow.end.getTime() >= ep.end().getTime();

                        this.push(ep);
                    };

                    line.addEvent(ep, colStart, colWidth, row, this.lines.length);

                    this.lines.push(line);

                    return this.lines.length - 1;
                };

                r.getStart = function() {
                    var start = 0;
                    for (var i = 0; i < calendar.rows.length && i < this.index; i++) {
                        start += calendar.rows[i].getHeight();
                    }
                };

                r.getHeight = function() {
                    return Math.max(this.lines.length * resolved.lineHeight() + calendar.cellHeaderHeight + calendar.cellMarginBottom, this.calendar.cellHeight);
                };

                this.rows.push(r);
            }

            this.endDate = DayPilot.Date.addDays(this.firstDate, rowCount * 7);
        };

        this._getHeight = function() {
            switch (this.heightSpec) {
                case "Auto":
                    var height = resolved.headerHeight();
                    for (var i = 0; i < this.rows.length; i++) {
                        height += this.rows[i].getHeight();
                    }
                    return height;
                case "Fixed":
                    return this.height;
            }
        };

        this._getWidth = function(start, end) {
            var diff = (end.y * 7 + end.x) - (start.y * 7 + start.x);
            return diff + 1;
        };

/*
        this._getMinCoords = function(first, second) {
            if ((first.y * 7 + first.x) < (second.y * 7 + second.x)) {
                return first;
            }
            else {
                return second;
            }
        };
*/

        this.debug = new DayPilot.Debug(this);

        this._drawTop = function() {
            var relative = this.nav.top;
            this.nav.top.dp = this;
            //this.nav.top = relative;
            relative.setAttribute("unselectable", "on");
            relative.style.MozUserSelect = 'none';
            relative.style.KhtmlUserSelect = 'none';
            relative.style.WebkitUserSelect = 'none';
            relative.style.position = 'relative';
            if (this.width) {
                relative.style.width = this.width;
            }
            // not setting height now, will be set using _updateHeight() later
            //relative.style.height = this._getHeight() + 'px';
            relative.onselectstart = function(e) { return false; }; // prevent text cursor in Chrome during drag&drop

            if (this.cssOnly) {
                relative.className = this._prefixCssClass("_main");
            }
            else {
                relative.style.border = "1px solid " + this.borderColor;
            }

            if (this.hideUntilInit) {
                relative.style.visibility = 'hidden';
            }

            /*
            var cells = document.createElement("div");
            this.nav.cells = cells;
            cells.style.position = "absolute";
            cells.style.left = "0px";
            cells.style.right = "0px";
            cells.setAttribute("unselectable", "on");
            relative.appendChild(cells);

            var events = document.createElement("div");
            this.nav.events = events;
            events.style.position = "absolute";
            events.style.left = "0px";
            events.style.right = "0px";
            events.setAttribute("unselectable", "on");
            relative.appendChild(events);
            */

            relative.onmousemove = this._onMainMouseMove;
            
            relative.ontouchstart = touch.onMainTouchStart;
            relative.ontouchmove = touch.onMainTouchMove;
            relative.ontouchend = touch.onMainTouchEnd;

            this.vsph = document.createElement("div");
            this.vsph.style.display = 'none';

            this.nav.top.appendChild(this.vsph);
            
            var table = document.createElement("div");
            //table.setAttribute("data-id", "header");
            table.style.position = "relative";
            table.style.height = resolved.headerHeight() + "px";
            //table.style.marginRight = "20px";
            table.oncontextmenu = function() { return false; };
            this.nav.top.appendChild(table);
            this.nav.header = table;
            
            var scrollable = document.createElement("div");
            //cells.setAttribute("data-id", "events");
            scrollable.style.position = "relative";
            scrollable.style.zoom = "1";  // ie7, makes DayPilot.sw working
            if (this.heightSpec === "Parent100Pct" || this.heightSpec === 'Fixed') {
                //cells.style.height = (this._getHeight() - this.headerHeight) + "px";
                scrollable.style.overflow = "auto";
            }
            
            var cells = document.createElement("div");
            cells.style.position = "relative";
            scrollable.appendChild(cells);
            
            this.nav.top.appendChild(scrollable);
            this.nav.scrollable = scrollable;
            this.nav.events = cells;
            
        };

        this._onMainMouseMove = function(ev) {
            
            calendar.coords = DayPilot.mo3(calendar.nav.events, ev);
            var coords = calendar.coords;
            if (!coords) {
                return;
            }
            
            var cell = calendar._getCellBelowPoint(coords.x, coords.y);
            if (!cell) {
                return;
            }
            
            //console.log(cell);

            if (DayPilotMonth.resizingEvent) {
                calendar._clearShadow();
                var resizing = DayPilotMonth.resizingEvent;

                var original = resizing.start;
                var width, start;

                if (resizing.direction === 'w-resize') {
                    start = cell;

                    var endDate = resizing.event.end().d;
                    if (DayPilot.Date.getDate(endDate).getTime() === endDate.getTime()) {
                        endDate = DayPilot.Date.addDays(endDate, -1);
                    }

                    var end = calendar._getCellFromDate(endDate);
                    width = calendar._getWidth(cell, end);
                }
                else {
                    start = calendar._getCellFromDate(resizing.event.start().d);
                    width = calendar._getWidth(start, cell);
                }

                if (width < 1) {
                    width = 1;
                }

                calendar._drawShadow(start.x, start.y, 0, width);

            }
            else if (DayPilotMonth.movingEvent) {
                
                calendar.debug.message("mousemove/moving start coords: " + DayPilotMonth.movingEvent.coords.x + " " + DayPilotMonth.movingEvent.coords.y);
                calendar.debug.message("mousemove/current coords: " + coords.x + " " + coords.y);
                
                // not actually moved, Chrome bug
                if (coords.x === DayPilotMonth.movingEvent.coords.x && coords.y === DayPilotMonth.movingEvent.coords.y) {
                    return;
                }

                var linepos = calendar._linePos(cell);

                calendar._clearShadow();

                var event = DayPilotMonth.movingEvent.event;
                var offset = DayPilotMonth.movingEvent.offset;
                var width = calendar.cellMode ? 1 : DayPilot.Date.daysSpan(event.start().d, event.end().d) + 1;

                if (width < 1) {
                    width = 1;
                }
                calendar._drawShadow(cell.x, cell.y, linepos, width, offset, event);
            }
            else if (DayPilotMonth.timeRangeSelecting) {
                DayPilotMonth.cancelCellClick = true;

                calendar._clearShadow();

                var start = DayPilotMonth.timeRangeSelecting;

                var startIndex = start.y * 7 + start.x;
                var cellIndex = cell.y * 7 + cell.x;

                var width = Math.abs(cellIndex - startIndex) + 1;

                if (width < 1) {
                    width = 1;
                }

                var shadowStart = startIndex < cellIndex ? start : cell;

                DayPilotMonth.timeRangeSelecting.from = { x: shadowStart.x, y: shadowStart.y };
                DayPilotMonth.timeRangeSelecting.width = width;
                DayPilotMonth.timeRangeSelecting.moved = true;

                calendar._drawShadow(shadowStart.x, shadowStart.y, 0, width, 0, null);

            }

        };
        
        // cell is result of _getCellBelowPoint
        this._linePos = function(cell) {
            var y = cell.relativeY;
            var row = calendar.rows[cell.y];
            //var linesCount = row.lines.length;
            var top = calendar.cellHeaderHeight;
            var lh = resolved.lineHeight();
            var max = row.lines.length;

            for (var i = 0; i < row.lines.length; i++) {
                var line = row.lines[i];
                if (line.isFree(cell.x, 1)) {
                    max = i;
                    break;
                }
            }

            var pos = Math.floor((y - top + lh / 2) / lh);  // rounded position
            var pos = Math.min(max, pos);  // no more than max
            var pos = Math.max(0, pos);  // no less then 0

            return pos;
        };

        this.message = function(html, delay, foreColor, backColor) {
            if (html === null) {
                return;
            }

            var delay = delay || this.messageHideAfter || 2000;
            var foreColor = foreColor || "#ffffff";
            var backColor = backColor || "#000000";
            var opacity = 0.8;

            var top = resolved.headerHeight();
            var left = 1;
            var right = 0;

            var div;

            if (!this.nav.message) {
                div = document.createElement("div");
                div.setAttribute("unselectable", "on");
                div.style.position = "absolute";
                div.style.right = "0px";
                div.style.left = "0px";
                div.style.top = top + "px";
                div.style.opacity = opacity;
                div.style.filter = "alpha(opacity=" + (opacity * 100) + ")";
                div.style.display = 'none';
                //div.style.paddingLeft = left + "px";
                //div.style.paddingRight = right + "px";
                
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

                
                if (!this.cssOnly) {
                    div.style.textAlign = "left";
                    //div.style.paddingRight = "-2px";
                }

                var inner = document.createElement("div");
                inner.setAttribute("unselectable", "on");
                inner.onclick = function() { 
                    calendar.nav.message.style.display = 'none'; 
                };
                if (!this.cssOnly) {
                    inner.style.padding = "5px";
                }
                else {
                    inner.className = this._prefixCssClass("_message");
                }
                div.appendChild(inner);

                var close = document.createElement("div");
                close.setAttribute("unselectable", "on");
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

                this.nav.top.appendChild(div);
                this.nav.message = div;

            }

            var showNow = function() {
                calendar.nav.message.style.opacity = opacity;

                var inner = calendar.nav.message.firstChild;

                if (!calendar.cssOnly) {
                    inner.style.backgroundColor = backColor;
                    inner.style.color = foreColor;
                }
                inner.innerHTML = html;

                var end = function() { calendar.messageTimeout = setTimeout(calendar._hideMessage, delay); };
                DayPilot.fade(calendar.nav.message, 0.2, end);
            };

            // make sure not timeout is active
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
        
        this._onResize = function(ev) {
            if (calendar.heightSpec === "Parent100Pct") {
                calendar._updateHeight();
            }
        };

        this._updateHeight = function() {
            if (this.heightSpec === 'Parent100Pct') {
                this.nav.top.style.height = "100%";
                var height = this.nav.top.clientHeight;
                this.nav.scrollable.style.height = (height - resolved.headerHeight()) + "px";
            }
            else {
                this.nav.top.style.height = this._getHeight() + 'px';
            }

            for (var x = 0; x < this.cells.length; x++) {
                for (var y = 0; y < this.cells[x].length; y++) {
                    this.cells[x][y].style.top = this._getRowTop(y) + 'px';
                    this.cells[x][y].style.height = this.rows[y].getHeight() + 'px';
                }
            }
            
            this._updateScrollbarWidth();
        };

        // x, y are relative
        this._getCellBelowPoint = function(x, y) {
            var columnWidth = Math.floor(this.nav.top.clientWidth / this._getColCount());
            var column = Math.min(Math.floor(x / columnWidth), this._getColCount() - 1);

            var row = null;

            //var height = resolved.headerHeight();
            var relativeY = 0;
            
            /*
            if (y < height) {
                return null;
            }

            var baseHeight = height; // coords are alway relative to nav.events
            */
           var height = 0;
           var baseHeight = 0;

            for (var i = 0; i < this.rows.length; i++) {
                height += this.rows[i].getHeight();
                if (y < height) {
                    relativeY = y - baseHeight;
                    row = i;
                    break;
                }
                baseHeight = height;
            }
            
            if (row === null) {
                row = this.rows.length - 1; // might be a pixel below the last line
            }

            var cell = {};
            cell.x = column;
            cell.y = row;
            cell.relativeY = relativeY;
            
            return cell;
        };

        this._getCellFromDate = function(date) {
            var width = DayPilot.Date.daysDiff(this.firstDate, date);
            var cell = { x: 0, y: 0 };
            while (width >= 7) {
                cell.y++;
                width -= 7;
            }
            cell.x = width;
            return cell;
        };
        
        this._updateScrollbarWidth = function() {
            var width = DayPilot.sw(this.nav.scrollable);
            this.nav.header.style.marginRight = width + "px";
        };

        this._drawTable = function() {

/*
            if (this.heightSpec === 'Parent100Pct') {
                this.nav.top.style.height = "100%";
            }
*/
            var table = this.nav.header;
            var cells = this.nav.events;

            this.cells = [];

            for (var x = 0; x < this._getColCount(); x++) {

                this.cells[x] = [];
                var headerProperties = this.headerProperties ? this.headerProperties[x] : null;
                
                var dayIndex = x + resolved.getWeekStart();
                if (dayIndex > 6) {
                    dayIndex -= 7;
                }
                
                if (!headerProperties) {
                    var headerProperties = {};
                    headerProperties.html = resolved.locale().dayNames[dayIndex];
                    if (!this.cssOnly) {
                        headerProperties.backColor = this.headerBackColor;
                    }
                }
                
                if (typeof calendar.onBeforeHeaderRender === 'function') {
                    var args = {};
                    args.header = {};
                    args.header.dayOfWeek = dayIndex;
                    //args.header.html = html;
                    
                    DayPilot.Util.copyProps(headerProperties, args.header, ['html', 'backColor']);
                    calendar.onBeforeHeaderRender(args);
                    DayPilot.Util.copyProps(args.header, headerProperties, ['html', 'backColor']);
                }

                var header = document.createElement("div");
                header.setAttribute("unselectable", "on");
                header.style.position = 'absolute';

                header.style.left = (this._getCellWidth() * x) + '%';
                header.style.width = (this._getCellWidth()) + '%';
                header.style.top = '0px';
                header.style.height = (resolved.headerHeight()) + 'px';

                (function(x) {
                    header.onclick = function() { calendar._headerClickDispatch(x); };
                })(dayIndex);

                var inner = document.createElement("div");
                inner.setAttribute("unselectable", "on");
                inner.className = this._prefixCssClass("_header_inner");
                inner.innerHTML = headerProperties.html;

                header.appendChild(inner);

                if (!this.cssOnly) {
                    //inner.style.height = (this.headerHeight) + 'px';
                    inner.style.position = "absolute";
                    inner.style.top = "0px";
                    inner.style.bottom = "0px";
                    inner.style.left = "0px";
                    inner.style.right = "0px";
                    inner.style.backgroundColor = headerProperties.backColor;
                    inner.style.fontFamily = this.headerFontFamily;
                    inner.style.fontSize = this.headerFontSize;
                    inner.style.color = this.headerFontColor;

                    //inner.style.borderLeft = '1px solid ' + this.borderColor;
                    //inner.style.borderTop = '1px solid ' + this.borderColor;
                    //inner.style.borderBottom = '1px solid ' + this.borderColor;
                    inner.style.textAlign = 'center';
                    inner.style.cursor = 'default';
                    inner.className = this._prefixCssClass("header");

                    if (x !== this._getColCount() - 1) {
                        inner.style.borderRight = '1px solid ' + this.borderColor;
                    }
                    //inner.innerHTML = headerProperties ? headerProperties.InnerHTML : resolved.locale().dayNames[dayIndex];
                }
                else {
                    header.className = this._prefixCssClass("_header");
                    if (headerProperties && headerProperties.backColor) {
                        inner.style.background = headerProperties.backColor;
                    }

                }

                table.appendChild(header);

                for (var y = 0; y < this.rows.length; y++) {
                    this._drawCell(x, y, cells);
                }

            }

            var div = document.createElement("div");
            div.style.position = 'absolute';
            div.style.padding = '2px';
            div.style.top = '0px';
            div.style.left = '0px';
            div.style.backgroundColor = "#FF6600";
            div.style.color = "white";
            div.innerHTML = "\u0044\u0045\u004D\u004F";

            if (this.numberFormat) this.nav.top.appendChild(div);
            
            //this._updateScrollbarWidth();

        };
        
        this._clearTable = function() {

            // clear event handlers
            for (var x = 0; x < this.cells.length; x++) {
                for (var y = 0; y < this.cells[x].length; y++) {
                    this.cells[x][y].onclick = null;
                }
            }

            this.nav.header.innerHTML = '';
            //this.nav.scrollable.innerHTML = '';
            this.nav.events.innerHTML = '';

        };

        this._drawCell = function(x, y, table) {
            
            var row = this.rows[y];
            var d = new DayPilot.Date(DayPilot.Date.addDays(this.firstDate, y * 7 + x));
            var cellProperties = this.cellProperties ? this.cellProperties[y * this._getColCount() + x] : null;
            
            var headerHtml = null;
            if (cellProperties) {
                headerHtml = cellProperties["headerHtml"];
            }
            else {
                var date = d.getDay();
                if (date === 1) {
                    headerHtml = resolved.locale().monthNames[d.getMonth()] + ' ' + date;
                }
                else {
                    headerHtml = date + "";
                }
            }

            if (!cellProperties) {
                var cellProperties = {};
                cellProperties.business = !calendar.isWeekend(d);
            }

            if (typeof calendar.onBeforeCellRender === 'function') {
                
                var args = {};
                args.cell = {};
                args.cell.areas = null;
                args.cell.backColor = null;
                args.cell.backImage = null;
                args.cell.backRepeat = null;
                args.cell.business = calendar.isWeekend(d);
                args.cell.headerHtml = headerHtml;
                args.cell.headerBackColor = null;
                args.cell.cssClass = null;
                args.cell.html = null;
                args.cell.start = d;
                args.cell.end = args.cell.start.addDays(1);
                
                DayPilot.Util.copyProps(cellProperties, args.cell);
                calendar.onBeforeCellRender(args);
                DayPilot.Util.copyProps(args.cell, cellProperties, ['areas', 'backColor', 'backImage', 'backRepeat', 'business', 'headerHtml', 'headerBackColor', 'cssClass', 'html']);
            }

            var cell = document.createElement("div");
            cell.setAttribute("unselectable", "on");
            cell.style.position = 'absolute';
            cell.style.cursor = 'default';
            cell.style.left = (this._getCellWidth() * x) + '%';
            cell.style.width = (this._getCellWidth()) + '%';
            cell.style.top = (this._getRowTop(y)) + 'px';
            cell.style.height = (row.getHeight()) + 'px';
            
            // TODO wrap in an object
            cell.d = d;
            cell.x = x;
            cell.y = y;
            cell.properties = cellProperties;

            var previousMonth = this.startDate.addMonths(-1).getMonth();
            var nextMonth = this.startDate.addMonths(1).getMonth();

            var thisMonth = this.startDate.getMonth();
            
            var inner = document.createElement("div");
            inner.setAttribute("unselectable", "on");
            cell.appendChild(inner);
            if (this.cssOnly) {
                inner.className = this._prefixCssClass("_cell_inner");
            }

            if (!this.cssOnly) {
                inner.style.position = "absolute";
                inner.style.left = "0px";
                inner.style.right = "0px";
                inner.style.top = "0px";
                inner.style.bottom = "0px";

                if (d.getMonth() === thisMonth) {
                    cell.className = this._prefixCssClass("cell");
                }
                else if (d.getMonth() === previousMonth) {
                    cell.className = this._prefixCssClass("cell") + " " + this._prefixCssClass("previous");
                }
                else if (d.getMonth() === nextMonth) {
                    cell.className = this._prefixCssClass("cell") + " " + this._prefixCssClass("next");
                }

                if (cellProperties) {
                    if (cellProperties["backColor"]) {
                        inner.style.background = cellProperties["backColor"];
                    }
                    if (cellProperties["cssClass"]) {
                        inner.className += " " + this._prefixCssClass(cellProperties["cssClass"]);
                    }
                    if (cellProperties["backImage"]) {
                        inner.style.backgroundImage = "url('" + cellProperties["backImage"] + "')";
                    }
                    if (cellProperties["backRepeat"]) {
                        inner.style.backgroundRepeat = cellProperties["backRepeat"];
                    }
                }
                else {
                    inner.style.background = this._getCellBackColor(d);
                }

                if (x !== this._getColCount() - 1) {
                    inner.style.borderRight = '1px solid ' + this.innerBorderColor;
                }

                if (y === 0) {
                    inner.style.borderTop = '1px solid ' + this.borderColor;
                } 

                inner.style.borderBottom = '1px solid ' + this.innerBorderColor;

            }
            else {
                inner.className = this._prefixCssClass("_cell_inner");
                if (d.getMonth() === thisMonth) {
                    cell.className = this._prefixCssClass("_cell");
                }
                else if (d.getMonth() === previousMonth) {
                    cell.className = this._prefixCssClass("_cell") + " " + this._prefixCssClass("_previous");
                }
                else if (d.getMonth() === nextMonth) {
                    cell.className = this._prefixCssClass("_cell") + " " + this._prefixCssClass("_next");
                }
                else {
                    doNothing();
                }

                if (cellProperties) {
                    if (cellProperties["cssClass"]) {
                        DayPilot.Util.addClass(cell, cellProperties.cssClass);
                    }
                    
                    if (cellProperties["business"]) {
                        DayPilot.Util.addClass(cell, this._prefixCssClass("_cell_business"));
                    }

                    if (cellProperties["backColor"]) {
                        inner.style.backgroundColor = cellProperties["backColor"];
                    }
                    
                    if (cellProperties["backImage"]) {
                        inner.style.backgroundImage = "url('" + cellProperties["backImage"] + "')";
                    }
                    if (cellProperties["backRepeat"]) {
                        inner.style.backgroundRepeat = cellProperties["backRepeat"];
                    }
                    
                }
            }

            cell.onmousedown = this._onCellMouseDown; 
            cell.onmousemove = this._onCellMouseMove;
            cell.onmouseout = this._onCellMouseOut;
            cell.oncontextmenu = this._onCellContextMenu;
            cell.onclick = this._onCellClick; 
            cell.ondblclick = this._onCellDoubleClick;

            var day = document.createElement("div");
            day.setAttribute("unselectable", "on");
            day.style.height = this.cellHeaderHeight + "px";

            if (!this.cssOnly) {
                if (cellProperties && cellProperties["headerBackColor"]) {
                    day.style.backgroundColor = cellProperties["headerBackColor"];
                }
                else if (this.cellHeaderBackColor) {
                    day.style.backgroundColor = this.cellHeaderBackColor;
                }
                day.style.paddingRight = '2px';
                day.style.textAlign = "right";
                day.style.fontFamily = this.cellHeaderFontFamily;
                day.style.fontSize = this.cellHeaderFontSize;
                day.style.color = this.cellHeaderFontColor;
                day.className = this._prefixCssClass("cellheader");
            }
            else {
                if (cellProperties && cellProperties["headerBackColor"]) {
                    day.style.background = cellProperties["headerBackColor"];
                }
                day.className = this._prefixCssClass("_cell_header");
            }

            day.innerHTML = headerHtml;

            inner.appendChild(day);

            if (cellProperties && cellProperties["html"]) {
                var html = document.createElement("div");
                html.setAttribute("unselectable", "on");
                html.style.height = (row.getHeight() - this.cellHeaderHeight) + 'px';
                html.style.overflow = 'hidden';
                html.innerHTML = cellProperties["html"];
                inner.appendChild(html);
            }

            if (this.cellMode) {
                var scrolling = document.createElement("div");
                scrolling.setAttribute("unselectable", "on");
                scrolling.style.height = (this.cellHeight - this.cellHeaderHeight) + "px";
                scrolling.style.overflow = 'auto';
                scrolling.style.position = 'relative';

                var inside = document.createElement('div');
                inside.setAttribute("unselectable", "on");
                inside.style.paddingTop = "1px";
                inside.style.paddingBottom = "1px";

                scrolling.appendChild(inside);
                inner.appendChild(scrolling);

                cell.body = inside;
                cell.scrolling = scrolling;
            }

/*
            // TODO remove
            var testing = false;
            if (testing) {
                if (!cell.properties) {
                    cell.properties = {};
                }
                cell.properties.areas = [
                    {"w":17, "h":17,"v":"Visible","js":"alert('static area');","action":"JavaScript","right":16,"css":"event_action_menu","top":2},
                    {"w":17, "h":17,"v":"Hover","js":"alert('hover area');","action":"JavaScript","right":36,"css":"event_action_menu","top":2}
                ];
            }    */        

            if (cell.properties) {
                var areas = cell.properties.areas || [];
                for (var i = 0; i < areas.length; i++) {
                    var area = areas[i];
                    if (area.v !== 'Visible') {
                        continue;
                    }
                    var a = DayPilot.Areas.createArea(cell, cell.properties, area);
                    cell.appendChild(a);
                }
            }

            this.cells[x][y] = cell;

            table.appendChild(cell);
        };
        
        this._onCellMouseMove = function() {
            var c = this;
            if (c.properties) {
                DayPilot.Areas.showAreas(c, c.properties);
            }
        };
        
        this._onCellMouseOut = function(ev) {
            var c = this;
            if (c.properties) {
                DayPilot.Areas.hideAreas(c, ev);
            }
        };
        
        this._onCellContextMenu = function() {
            var d = this.d;
            
            var go = function(d) {
                var start = new DayPilot.Date(d);
                var end = start.addDays(1);

                var selection = new DayPilot.Selection(start, end, null, calendar);
                if (calendar.contextMenuSelection) {
                    calendar.contextMenuSelection.show(selection);
                }
            };

            go(d);

        };
        
        this._onCellDoubleClick = function() {
            var d = this.d;
            
            if (calendar.timeouts) {
                for (var toid in calendar.timeouts) {
                    window.clearTimeout(calendar.timeouts[toid]);
                }
                calendar.timeouts = null;
            }

            if (calendar.timeRangeDoubleClickHandling !== 'Disabled') {
                var start = new DayPilot.Date(d);
                var end = start.addDays(1);
                calendar._timeRangeDoubleClickDispatch(start, end);
            }
        };
        
        this._onCellClick = function() {
            
            if (DayPilotMonth.cancelCellClick) {
                return;
            }
            
            var d = this.d;

            var single = function(d) {
                var start = new DayPilot.Date(d);
                var end = start.addDays(1);
                calendar._timeRangeSelectedDispatch(start, end);
            };

            if (calendar.timeRangeSelectedHandling !== 'Disabled' && calendar.timeRangeDoubleClickHandling === 'Disabled') {
                single(d);
                return;
            }

            if (!calendar.timeouts) {
                calendar.timeouts = [];
            }

            var clickDelayed = function(d) {
                return function() {
                    single(d);
                };
            };

            calendar.timeouts.push(window.setTimeout(clickDelayed(d), calendar.doubleClickTimeout));

        };
        
        this._onCellMouseDown = function(e) {
            var cell = this;
            var x = cell.x;
            var y = cell.y;
            
            DayPilotMonth.cancelCellClick = false;
            
            if (cell.scrolling) {
                var offset = DayPilot.mo3(cell.scrolling, e);
                var sw = DayPilot.sw(cell.scrolling);
                var width = cell.scrolling.offsetWidth;
                if (offset.x > width - sw) {  // clicking on the vertical scrollbar
                    return;
                }
            }

            if (calendar.timeRangeSelectedHandling !== 'Disabled') {
                calendar._clearShadow();
                DayPilotMonth.timeRangeSelecting = { "root": calendar, "x": x, "y": y, "from": { x: x, y: y }, "width": 1 };
            }
        };

        this._getColCount = function() {
            if (this.showWeekend) {
                return 7;
            }
            else {
                return 5;
            }
        };

        this._getCellWidth = function() {
            if (this.showWeekend) {
                return 14.285;
            }
            else {
                return 20;
            }
        };

        this._getCellBackColor = function(d) {
            if (d.getUTCDay() === 6 || d.getUTCDay() === 0) {
                return this.nonBusinessBackColor;
            }
            return this.backColor;
        };

        this._getRowTop = function(index) {
            //var top = this.headerHeight;
            var top = 0;
            for (var i = 0; i < index; i++) {
                top += this.rows[i].getHeight();
            }
            return top;
        };
        
        this.clearSelection = function() {
            this._clearShadow();
        };

/*
        this._postBack = function(prefix) {
            var args = [];
            for (var i = 1; i < arguments.length; i++) {
                args.push(arguments[i]);
            }

            __doPostBack(calendar.uniqueID, prefix + DayPilot.ea(args));
        };
*/
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

            if (typeof type === 'undefined') {
                type = "CallBack";
            }
            
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
                WebForm_DoCallback(this.uniqueID, commandstring, this._updateView, null, this.callbackError, true);
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

        this._callBackResponse = function(response) {
            calendar._updateView(response.responseText);
        };

        this._getCallBackHeader = function() {
            var h = {};
            h.v = this.v;
            h.control = "dpm";
            h.id = this.id;
            h.visibleStart = new DayPilot.Date(this.firstDate);
            h.visibleEnd = h.visibleStart.addDays(this.days);

            h.clientState = this.clientState;
            h.cssOnly = calendar.cssOnly;
            h.cssClassPrefix = calendar.cssClassPrefix;

            h.startDate = calendar.startDate;
            h.showWeekend = this.showWeekend;
            h.headerBackColor = this.headerBackColor;
            h.backColor = this.backColor;
            h.nonBusinessBackColor = this.nonBusinessBackColor;
            h.locale = this.locale;
            h.timeFormat = this.timeFormat;
            h.weekStarts = this.weekStarts;
            h.viewType = this.viewType;
            h.weeks = this.weeks;

            h.selected = calendar.multiselect.events();

            h.hashes = calendar.hashes;

            return h;
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
        /*
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
*/
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


        this.eventClickPostBack = function(e, data) {
            this._postBack2("EventClick", data, e);
        };
        this.eventClickCallBack = function(e, data) {
            this._callBack2('EventClick', e, data);
        };

        this._eventClickDispatch = function(div, e) {

            DayPilotMonth.movingEvent = null;
            DayPilotMonth.resizingEvent = null;

            //var div = this;

            var e = e || window.event;
            var ctrlKey = e.ctrlKey;

            e.cancelBubble = true;
            if (e.stopPropagation) {
                e.stopPropagation();
            }

            if (typeof (DayPilotBubble) !== 'undefined') {
                DayPilotBubble.hideActive();
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

            calendar.timeouts.push(window.setTimeout(eventClickDelayed(div, ctrlKey), calendar.doubleClickTimeout));

        };


        this._eventClickSingle = function(div, ctrlKey) {
            var e = div.event;
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
                    case 'Select':
                        calendar._eventSelect(div, e, ctrlKey);
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
                    case 'Select':
                        calendar._eventSelect(div, e, ctrlKey);
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
                
            }

/*
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
                case 'Select':
                    calendar._eventSelect(div, e, ctrlKey);
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
        };

        this.eventDoubleClickPostBack = function(e, data) {
            this._postBack2('EventDoubleClick', data, e);
        };
        this.eventDoubleClickCallBack = function(e, data) {
            this._callBack2('EventDoubleClick', e, data);
        };

        this._eventDoubleClickDispatch = function(div, ev) {

            if (typeof (DayPilotBubble) !== 'undefined') {
                DayPilotBubble.hideActive();
            }

            if (calendar.timeouts) {
                for (var toid in calendar.timeouts) {
                    window.clearTimeout(calendar.timeouts[toid]);
                }
                calendar.timeouts = null;
            }

            var ev = ev || window.event;
            var e = div.event;

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
                    case 'Select':
                        calendar._eventSelect(div, e, ev.ctrlKey);
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
                    case 'Select':
                        calendar._eventSelect(div, e, ev.ctrlKey);
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
                case 'Select':
                    calendar._eventSelect(div, e);
                    break;
                case 'Bubble':
                    if (calendar.bubble) {
                        calendar.bubble.showEvent(e);
                    }
                    break;
            }
*/
        };

        this._eventSelect = function(div, e, ctrlKey) {
            calendar._eventSelectDispatch(div, e, ctrlKey);
        };
/*
        this._eventSelect = function(div, e, ctrlKey) {
            var m = calendar.multiselect;
            m.previous = m.events();
            m._toggleDiv(div, ctrlKey);
            var change = m.isSelected(e) ? "selected" : "deselected";
            calendar._eventSelectDispatch(e, change);
        };*/

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


        this.eventRightClickPostBack = function(e, data) {
            this._postBack2("EventRightClick", data, e);
        };
        this.eventRightClickCallBack = function(e, data) {
            this._callBack2('EventRightClick', e, data);
        };

        this._eventRightClickDispatch = function(e) {

            this.event = e; 

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
            }
*/
            return false;
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

        this.eventMovePostBack = function(e, newStart, newEnd, data, position) {
            if (!newStart)
                throw 'newStart is null';
            if (!newEnd)
                throw 'newEnd is null';

            var params = {};
            params.e = e;
            params.newStart = newStart;
            params.newEnd = newEnd;
            params.position = position;

            this._postBack2('EventMove', data, params);

        };
        this.eventMoveCallBack = function(e, newStart, newEnd, data, position) {
            if (!newStart)
                throw 'newStart is null';
            if (!newEnd)
                throw 'newEnd is null';

            var params = {};
            params.e = e;
            params.newStart = newStart;
            params.newEnd = newEnd;
            params.position = position;
            //params.newColumn = newColumn;

            this._callBack2('EventMove', params, data);
        };

        this._eventMoveDispatch = function(e, x, y, offset, ev, position) {

            var startOffset = DayPilot.Date.getTime(e.start().d);

            var endDate = DayPilot.Date.getDate(e.end().d);
            if (endDate.getTime() !== e.end().d.getTime()) {
                endDate = DayPilot.Date.addDays(endDate, 1);
            }
            var endOffset = DayPilot.Date.diff(e.end().d, endDate);

            var boxStart = this._getDateFromCell(x, y);
            boxStart = DayPilot.Date.addDays(boxStart, -offset);
            var width = DayPilot.Date.daysSpan(e.start().d, e.end().d) + 1;

            var boxEnd = DayPilot.Date.addDays(boxStart, width);

            var newStart = new DayPilot.Date(DayPilot.Date.addTime(boxStart, startOffset));
            var newEnd = new DayPilot.Date(DayPilot.Date.addTime(boxEnd, endOffset));


            if (calendar._api2()) {
                // API v2
                var args = {};

                args.e = e;
                args.newStart = newStart;
                args.newEnd = newEnd;
                args.position = position;
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
                        calendar.eventMovePostBack(e, newStart, newEnd, null, position);
                        break;
                    case 'CallBack':
                        calendar.eventMoveCallBack(e, newStart, newEnd, null, position);
                        break;
                    case 'Notify':
                        calendar.eventMoveNotify(e, newStart, newEnd, null, position);
                        break;
                    case 'Update':
                        e.start(newStart);
                        e.end(newEnd);
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
                        calendar.eventMovePostBack(e, newStart, newEnd, null, position);
                        break;
                    case 'CallBack':
                        calendar.eventMoveCallBack(e, newStart, newEnd, null, position);
                        break;
                    case 'JavaScript':
                        calendar.onEventMove(e, newStart, newEnd, ev.ctrlKey, ev.shiftKey, position);
                        break;
                    case 'Notify':
                        calendar.eventMoveNotify(e, newStart, newEnd, null, position);
                        break;
                }
            }


/*
            switch (calendar.eventMoveHandling) {
                case 'PostBack':
                    calendar.eventMovePostBack(e, newStart, newEnd, null, position);
                    break;
                case 'CallBack':
                    calendar.eventMoveCallBack(e, newStart, newEnd, null, position);
                    break;
                case 'JavaScript':
                    calendar.onEventMove(e, newStart, newEnd, ev.ctrlKey, ev.shiftKey, position);
                    break;
                case 'Notify':
                    calendar.eventMoveNotify(e, newStart, newEnd, null, position);
                    break;
                    
            }
*/
        };
        
        this.eventMoveNotify = function(e, newStart, newEnd, data, line) {

            var old = new DayPilot.Event(e.copy(), this);

            e.start(newStart);
            e.end(newEnd);
            //e.resource(newResource);
            e.commit();

            calendar.update();

            this._invokeEventMove("Notify", old, newStart, newEnd, data, line);

        };
        
        this._invokeEventMove = function(type, e, newStart, newEnd, data, line) {
            var params = {};
            params.e = e;
            params.newStart = newStart;
            params.newEnd = newEnd;
            //params.newResource = newResource;
            params.position = line;

            this._invokeEvent(type, "EventMove", params, data);
        };        

        this.eventResizePostBack = function(e, newStart, newEnd, data) {
            if (!newStart)
                throw 'newStart is null';
            if (!newEnd)
                throw 'newEnd is null';

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

        this._eventResizeDispatch = function(e, start, width) {
            var startOffset = DayPilot.Date.getTime(e.start().d);

            var endDate = DayPilot.Date.getDate(e.end().d);
            if (!DayPilot.Date.equals(endDate, e.end().d)) {
                endDate = DayPilot.Date.addDays(endDate, 1);
            }
            var endOffset = DayPilot.Date.diff(e.end().d, endDate);

            var boxStart = this._getDateFromCell(start.x, start.y);
            //var width = DayPilot.Date.daysSpan(e.start(), e.end()) + 1;
            var boxEnd = DayPilot.Date.addDays(boxStart, width);

            var newStart = new DayPilot.Date(DayPilot.Date.addTime(boxStart, startOffset));
            var newEnd = new DayPilot.Date(DayPilot.Date.addTime(boxEnd, endOffset));

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

/*
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
            */
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


        this.timeRangeSelectedPostBack = function(start, end, data) {
            var range = {};
            range.start = start;
            range.end = end;

            this._postBack2('TimeRangeSelected', data, range);
        };
        this.timeRangeSelectedCallBack = function(start, end, data) {

            var range = {};
            range.start = start;
            range.end = end;

            this._callBack2('TimeRangeSelected', range, data);
        };

        this._timeRangeSelectedDispatch = function(start, end) {
            
            if (calendar._api2()) {
                
                var args = {};
                args.start = start;
                args.end = end;
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
                        calendar.timeRangeSelectedPostBack(start, end);
                        calendar.clearSelection();
                        break;
                    case 'CallBack':
                        calendar.timeRangeSelectedCallBack(start, end);
                        calendar.clearSelection();
                        break;
                }
                
                if (typeof calendar.onTimeRangeSelected === 'function') {
                    calendar.onTimeRangeSelected(args);
                }
                
            }
            else {
                switch (calendar.timeRangeSelectedHandling) {
                    case 'PostBack':
                        calendar.timeRangeSelectedPostBack(start, end);
                        calendar.clearSelection();
                        break;
                    case 'CallBack':
                        calendar.timeRangeSelectedCallBack(start, end);
                        calendar.clearSelection();
                        break;
                    case 'JavaScript':
                        calendar.onTimeRangeSelected(start, end);
                        break;
                }
            }
            
            /*
            switch (calendar.timeRangeSelectedHandling) {
                case 'PostBack':
                    calendar.timeRangeSelectedPostBack(start, end);
                    calendar.clearSelection();
                    break;
                case 'CallBack':
                    calendar.timeRangeSelectedCallBack(start, end);
                    calendar.clearSelection();
                    break;
                case 'JavaScript':
                    calendar.onTimeRangeSelected(start, end);
                    break;
            }
            */
        };

        this.timeRangeMenuClickPostBack = function(e, command, data) {
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

        this.headerClickPostBack = function(c, data) {
            this._postBack2('HeaderClick', data, c);
        };
        this.headerClickCallBack = function(c, data) {
            this._callBack2('HeaderClick', c, data);
        };

        this._headerClickDispatch = function(x) {

            var data = this.data;
            var c = { day: x };
            // check if allowed

            if (calendar._api2()) {

                var args = {};
                args.header = {};
                args.header.dayOfWeek = x;
                
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

        };

        this.timeRangeDoubleClickPostBack = function(start, end, data) {
            var range = {};
            range.start = start;
            range.end = end;
            //range.resource = column;

            this._postBack2('TimeRangeDoubleClick', data, range);
        };
        this.timeRangeDoubleClickCallBack = function(start, end, data) {

            var range = {};
            range.start = start;
            range.end = end;
            //range.resource = column;

            this._callBack2('TimeRangeDoubleClick', range, data);
        };

        this._timeRangeDoubleClickDispatch = function(start, end) {
            if (calendar._api2()) {

                var args = {};
                args.start = start;
                args.end = end;
                
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
                        calendar.timeRangeDoubleClickPostBack(start, end);
                        break;
                    case 'CallBack':
                        calendar.timeRangeDoubleClickCallBack(start, end);
                        break;
                }
                
                if (typeof calendar.onTimeRangeDoubleClicked === 'function') {
                    calendar.onTimeRangeDoubleClicked(args);
                }
            }
            else {
                switch (calendar.timeRangeDoubleClickHandling) {
                    case 'PostBack':
                        calendar.timeRangeDoubleClickPostBack(start, end);
                        break;
                    case 'CallBack':
                        calendar.timeRangeDoubleClickCallBack(start, end);
                        break;
                    case 'JavaScript':
                        calendar.onTimeRangeDoubleClick(start, end);
                        break;
                }
            }
            
            /*
            switch (calendar.timeRangeDoubleClickHandling) {
                case 'PostBack':
                    calendar.timeRangeDoubleClickPostBack(start, end);
                    break;
                case 'CallBack':
                    calendar.timeRangeDoubleClickCallBack(start, end);
                    break;
                case 'JavaScript':
                    calendar.onTimeRangeDoubleClick(start, end);
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

        this._findEventDiv = function(e) {
            for (var i = 0; i < calendar.elements.events.length; i++) {
                var div = calendar.elements.events[i];
                if (div.event === e || div.event.data === e.data) {
                    return div;
                }
            }
            return null;
        };

        this._findEventDivs = function(e) {
            var result = {};
            result.list = [];
            result.each = function(m) {
                if (!m) { return; }
                for (var i = 0; i < this.list.length; i++) {
                    m(this.list[i]);
                }
            };

            for (var i = 0; i < this.elements.events.length; i++) {
                var div = this.elements.events[i];
                if (div.event.data === e.data) {
                    result.list.push(div);
                }
            }
            return result;

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
        
        resolved.lineHeight = function() {
            return resolved.eventHeight() + calendar.lineSpace;
        };
        
        resolved.rounded = function() {
            return calendar.eventCorners === "Rounded";
        };
        
        resolved.loadFromServer = function() {
            // make sure it has a place to ask
            if (calendar.backendUrl || typeof WebForm_DoCallback === 'function') {
                return (typeof calendar.events.list === 'undefined') || (!calendar.events.list);
            }
            else {
                return false;
            }
        };
        
        resolved.locale = function() {
            return DayPilot.Locale.find(calendar.locale);
        };        
        
        resolved.getWeekStart = function() {
            if (calendar.showWeekend) {
                return calendar.weekStarts;
            }
            else {
                return 1; // Monday
            }
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
        
        resolved.eventHeight = function() {
            if (calendar._cache.eventHeight) {
                return calendar._cache.eventHeight;
            }
            var height = calendar._getDimensionsFromCss("_event_height").height;
            if (!height) {
                height = calendar.eventHeight;
            }
            calendar._cache.eventHeight = height;
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

        // internal methods for handling event selection
        this.multiselect = {};

        this.multiselect.initList = [];
        this.multiselect.list = [];
        this.multiselect.divs = [];
        this.multiselect.previous = [];

        this.multiselect._serialize = function() {
            var m = calendar.multiselect;
            return DayPilot.JSON.stringify(m.events());
        };

        this.multiselect.events = function() {
            var m = calendar.multiselect;
            var events = [];
            events.ignoreToJSON = true;
            for (var i = 0; i < m.list.length; i++) {
                events.push(m.list[i]);
            }
            return events;
        };

        this.multiselect._updateHidden = function() {
            // update the hidden field, not implemented
        };

        this.multiselect._toggleDiv = function(div, ctrl) {
            var m = calendar.multiselect;
            if (m.isSelected(div.event)) {
                if (calendar.allowMultiSelect) {
                    if (ctrl) {
                        m.remove(div.event, true);
                    }
                    else {
                        var count = m.list.length;
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
            return m._isInList(ev, m.initList);
        };

        this.multiselect._alert = function() {
            var m = calendar.multiselect;
            var list = [];
            for (var i = 0; i < m.list.length; i++) {
                var event = m.list[i];
                list.push(event.value());
            }
            alert(list.join("\n"));
        };

        this.multiselect.add = function(ev, dontRedraw) {
            var m = calendar.multiselect;
            if (m.indexOf(ev) === -1) {
                m.list.push(ev);
            }
            if (dontRedraw) {
                return;
            }
            m.redraw();
        };

        this.multiselect.remove = function(ev, dontRedraw) {
            var m = calendar.multiselect;
            var i = m.indexOf(ev);
            if (i !== -1) {
                m.list.splice(i, 1);
            }
        };

        this.multiselect.clear = function(dontRedraw) {
            var m = calendar.multiselect;
            m.list = [];

            if (dontRedraw) {
                return;
            }
            m.redraw();
        };

        this.multiselect.redraw = function() {
            //alert('redrawing');
            //calendar.debug("redrawing");
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
            var div = m._findContentDiv(div);
            DayPilot.Util.addClass(div, cn);
            m.divs.push(div);
        };

        this.multiselect._findContentDiv = function(div) {
            if (calendar.cssOnly) {
                return div;
            }
            return div.firstChild;
        };

        this.multiselect._divDeselectAll = function() {
            var m = calendar.multiselect;
            for (var i = 0; i < m.divs.length; i++) {
                var div = m.divs[i];
                m._divDeselect(div, true);
            }
            m.divs = [];
        };

        this.multiselect._divDeselect = function(div, dontRemoveFromCache) {
            var m = calendar.multiselect;
            var cn = calendar.cssOnly ? calendar._prefixCssClass("_selected") : calendar._prefixCssClass("selected");
            var c = m._findContentDiv(div);
            if (c && c.className && c.className.indexOf(cn) !== -1) {
                c.className = c.className.replace(cn, "");
            }

            if (dontRemoveFromCache) {
                return;
            }
            var i = DayPilot.indexOf(m.divs, div);
            if (i !== -1) {
                m.divs.splice(i, 1);
            }

        };

        this.multiselect.isSelected = function(ev) {
            //return calendar.multiselect.indexOf(ev) != -1;
            return calendar.multiselect._isInList(ev, calendar.multiselect.list);
        };

        this.multiselect.indexOf = function(ev) {
            return DayPilot.indexOf(calendar.multiselect.list, ev);
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
            if (!DayPilotMonth.eventResizing && !DayPilotMonth.eventMoving && !DayPilotMonth.timeRangeSelecting) {
                this.autoRefreshCount++;
                this.commandCallBack(this.autoRefreshCommand);
            }
            if (this.autoRefreshCount < this.autoRefreshMaxCount) {
                this.autoRefreshTimeout = window.setTimeout(function() { calendar._doRefresh(); }, this.autoRefreshInterval * 1000);
            }
        };

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

        this.update = function() {
            if (!this.cells) {  // not initialized yet
                return;
            }

            var full = true;
			
            calendar._deleteEvents();
            calendar._prepareRows();
            calendar._loadEvents();

            if (full) {
                calendar._clearTable();
                calendar._drawTable();
            }
            calendar._updateHeight();
            calendar._show();
            calendar._drawEvents();
        };
        
        this.dispose = function() {
            //var start = new Date();

            var c = calendar;
            if (!c.nav.top) {
                return;
            }
            
            c._stopAutoRefresh();
            c._deleteEvents();

            c.nav.top.removeAttribute("style");
            c.nav.top.removeAttribute("class");
            c.nav.top.innerHTML = '';
            c.nav.top.dp = null;
            c.nav.top = null;

            DayPilotMonth.unregister(c);
        };

        this._registerGlobalHandlers = function() {
            if (!DayPilotMonth.globalHandlers) {
                DayPilotMonth.globalHandlers = true;
                DayPilot.re(document, 'mouseup', DayPilotMonth.gMouseUp);
            }
            DayPilot.re(window, 'resize', this._onResize);
        };

        this._show = function() {
            if (this.nav.top.style.visibility === 'hidden') {
                this.nav.top.style.visibility = 'visible';
            }
        };
        
        this.show = function() {
            calendar.nav.top.style.display = '';
        };
        
        this.hide = function() {
            calendar.nav.top.style.display = 'none';
        };

        this._initShort = function() {

            this._prepareRows();
            this._drawTop();
            this._drawTable();
            this._registerGlobalHandlers();
            this._startAutoRefresh();
            this._callBack2('Init'); // load events
            
            DayPilotMonth.register(this);
        };
        
        this.init = function() {
            
            this.nav.top = document.getElementById(placeholder);
            
            if (!this.nav.top) {
                throw "DayPilot.Month.init(): The placeholder element not found: '" + id + "'.";
            }

            if (this.nav.top.dp) {
                return;
            }
            var loadFromServer = resolved.loadFromServer();

            if (loadFromServer) {
                //console.log("loading from server");
                this._initShort();
                this.initialized = true;
                return;
            }
            //console.log("loading now");

            this._prepareRows();
            this._loadEvents();
            this._drawTop();
            this._drawTable();
            this._show();
            this._drawEvents();
            this._updateHeight();

            this._registerGlobalHandlers();

            if (this.messageHTML) {
                this.message(this.messageHTML);
            }

            this._fireAfterRenderDetached(null, false);
            //this.afterRender(null, false);

            this._startAutoRefresh();
            DayPilotMonth.register(this);

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

        this.Init = this.init;
    };


    DayPilotMonth.register = function(calendar) {
        if (!DayPilotMonth.registered) {
            DayPilotMonth.registered = [];
        }
        for (var i = 0; i < DayPilotMonth.registered.length; i++) {
            if (DayPilotMonth.registered[i] === calendar) {
                return;
            }
        }
        DayPilotMonth.registered.push(calendar);

    };

    DayPilotMonth.unregister = function(calendar) {
        var a = DayPilotMonth.registered;
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
            DayPilot.ue(document, 'mouseup', DayPilotMonth.gMouseUp);
            DayPilotMonth.globalHandlers = false;
        }
    };


    DayPilotMonth.gMouseUp = function(ev) {
        
        //console.log("gMouseup");

        if (DayPilotMonth.movingEvent) {
            var src = DayPilotMonth.movingEvent;
            DayPilotMonth.movingEvent = null;

            if (!src.event || !src.event.calendar || !src.event.calendar.shadow || !src.event.calendar.shadow.start) {
                return;
            }

            // load ref
            var calendar = src.event.calendar;
            var e = src.event;
            var start = calendar.shadow.start;
            var position = calendar.shadow.position;
            var offset = src.offset;

            // cleanup
            calendar._clearShadow();
            //DayPilotMonth.movingEvent = null;

            var ev = ev || window.event;

            // fire the event
            calendar._eventMoveDispatch(e, start.x, start.y, offset, ev, position);

            ev.cancelBubble = true;
            if (ev.stopPropagation) {
                ev.stopPropagation();
            }
            return false;
        }
        else if (DayPilotMonth.resizingEvent) {
            var src = DayPilotMonth.resizingEvent;
            DayPilotMonth.resizingEvent = null;

            if (!src.event || !src.event.calendar || !src.event.calendar.shadow || !src.event.calendar.shadow.start) {
                return;
            }

            // load ref
            var calendar = src.event.calendar;

            var e = src.event;
            var start = calendar.shadow.start;
            var width = calendar.shadow.width;

            // cleanup
            calendar._clearShadow();

            // fire the event
            calendar._eventResizeDispatch(e, start, width);

            ev.cancelBubble = true;
            return false;
        }
        else if (DayPilotMonth.timeRangeSelecting) {
            //DayPilotMonth.cancelCellClick = true;
            if (DayPilotMonth.timeRangeSelecting.moved) {
                var sel = DayPilotMonth.timeRangeSelecting;
                var calendar = sel.root;

                var start = new DayPilot.Date(calendar._getDateFromCell(sel.from.x, sel.from.y));
                var end = start.addDays(sel.width);
                calendar._timeRangeSelectedDispatch(start, end);
            }
            DayPilotMonth.timeRangeSelecting = null;
        }
        /*
        DayPilotMonth.movingEvent = null;
        DayPilotMonth.resizingEvent = null;
        */
    };

    // publish the API 

    // (backwards compatibility)    
    //DayPilot.MonthVisible.dragStart = DayPilotMonth.dragStart;
    DayPilot.MonthVisible.Month = DayPilotMonth.Month;

    // current
    //DayPilot.Month = DayPilotMonth.Month;

    // experimental jQuery bindings
    if (typeof jQuery !== 'undefined') {
        (function($) {
            $.fn.daypilotMonth = function(options) {
                var first = null;
                var j = this.each(function() {
                    if (this.daypilot) { // already initialized
                        return;
                    };

                    var daypilot = new DayPilot.Month(this.id);
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