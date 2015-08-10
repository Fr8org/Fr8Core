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
if (typeof DayPilotNavigator === 'undefined') {
	var DayPilotNavigator = DayPilot.NavigatorVisible = {};
}

(function() {

    if (typeof DayPilot.Navigator !== 'undefined') {
        return;
    }

    (function registerDefaultTheme() {
        if (DayPilot.Global.defaultNavigatorCss) {
            return;
        }
        
        var sheet = DayPilot.sheet();
        
        sheet.add(".navigator_default_main", "border-left: 1px solid #A0A0A0;border-right: 1px solid #A0A0A0;border-bottom: 1px solid #A0A0A0;background-color: white;color: #000000;");
        sheet.add(".navigator_default_month", "font-family: Tahoma;font-size: 11px;");
        sheet.add(".navigator_default_day", "color: black;");
        sheet.add(".navigator_default_weekend", "background-color: #f0f0f0;");
        sheet.add(".navigator_default_dayheader", "color: black;");
        sheet.add(".navigator_default_line", "border-bottom: 1px solid #A0A0A0;");
        sheet.add(".navigator_default_dayother", "color: gray;");
        sheet.add(".navigator_default_todaybox", "border: 1px solid red;");
        sheet.add(".navigator_default_select, .navigator_default_weekend.navigator_default_select", "background-color: #FFE794;");
        sheet.add(".navigator_default_title, .navigator_default_titleleft, .navigator_default_titleright", 'border-top: 1px solid #A0A0A0;color: #666;background: #eee;background: -webkit-gradient(linear, left top, left bottom, from(#eeeeee), to(#dddddd));background: -webkit-linear-gradient(top, #eeeeee 0%, #dddddd);background: -moz-linear-gradient(top, #eeeeee 0%, #dddddd);background: -ms-linear-gradient(top, #eeeeee 0%, #dddddd);background: -o-linear-gradient(top, #eeeeee 0%, #dddddd);background: linear-gradient(top, #eeeeee 0%, #dddddd);filter: progid:DXImageTransform.Microsoft.Gradient(startColorStr="#eeeeee", endColorStr="#dddddd");');
        sheet.add(".navigator_default_busy", "font-weight: bold;");
        sheet.commit();
        
        DayPilot.Global.defaultNavigatorCss = true;
    })();

    DayPilotNavigator = {};
    DayPilot.Navigator = function(id, options) {
        this.v = '800';
        var calendar = this;
        this.id = id;
        this.api = 2;
        this.isNavigator = true;

        this.weekStarts = 'Auto'; // 0 = Sunday, 1 = Monday, ... 'Auto' = according to locale
        this.selectMode = 'day'; // day/week/month/none
        this.titleHeight = 20;
        this.dayHeaderHeight = 20;
        this.cellWidth = 20;
        this.cellHeight = 20;
        this.cssOnly = true;
        this.cssClassPrefix = "navigator_default";
        this.selectionStart = new DayPilot.Date().getDatePart();  // today
        this.selectionEnd = null;
        this.selectionDay = null;
        this.showMonths = 1;
        this.skipMonths = 1;
        this.command = "navigate";
        this.year = new DayPilot.Date().getYear();
        this.month = new DayPilot.Date().getMonth() + 1;
        this.showWeekNumbers = false;
        this.weekNumberAlgorithm = 'Auto';
        this.rowsPerMonth = 'Six';  // Six, Auto
        this.orientation = "Vertical";
        this.locale = "en-us";

        this.timeRangeSelectedHandling = "Bind";
        this.visibleRangeChangedHandling = "Enabled";

        this._prepare = function() {
            
            this.root.dp = this;
            
            if (this.cssOnly) {
                this.root.className = this._prefixCssClass('_main');
            }
            else {
                this.root.className = this._prefixCssClass('main');
            }

            if (this.orientation === "Horizontal") {
                this.root.style.width = this.showMonths * (this.cellWidth * 7 + this._weekNumberWidth()) + 'px';
                this.root.style.height = (this.cellHeight*6 + this.titleHeight + this.dayHeaderHeight) + 'px';
            }
            else {
                this.root.style.width = (this.cellWidth * 7 + this._weekNumberWidth()) + 'px';
            }
            //this.root.style.height = (this.showMonths*(this.cellHeight*6 + this.titleHeight + this.dayHeaderHeight)) + 'px';
            
            this.root.style.position = "relative";

            var vsph = document.createElement("input");
            vsph.type = 'hidden';
            vsph.name = calendar.id + "_state";
            vsph.id = vsph.name;
            //vsph.value = result.VsUpdate;
            this.root.appendChild(vsph);
            this.state = vsph;

            if (!this.startDate) {
                this.startDate = new DayPilot.Date(DayPilot.Date.firstDayOfMonth(this.year, this.month));
            }
            else { // make sure it's the first day
                this.startDate = new DayPilot.Date(this.startDate).firstDayOfMonth();
            }

            this.calendars = [];
            this.selected = [];
            this.months = [];
            /*
            var bound = eval(this.bound);
            if (bound && typeof (bound.listener) == 'function') {
            bound.listener(this);
            }*/
        };

        this._api2 = function() {
            return calendar.api === 2;
        };
        
        this._clearTable = function() {
            // TODO do something smarter here
            this.root.innerHTML = '';
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

        this._addClass = function(object, name) {
            var fullName = this.cssOnly ? this._prefixCssClass("_" + name) : this._prefixCssClass(name);
            DayPilot.Util.addClass(object, fullName);
        };

        this._removeClass = function(object, name) {
            var fullName = this.cssOnly ? this._prefixCssClass("_" + name) : this._prefixCssClass(name);
            DayPilot.Util.removeClass(object, fullName);
        };

        this._drawTable = function(j, showLinks) {
            var month = {};
            month.cells = [];
            month.days = [];
            month.weeks = [];

            var startDate = this.startDate.addMonths(j);
            
            var showBefore = showLinks.before;
            var showAfter = showLinks.after;

            var firstOfMonth = startDate.firstDayOfMonth();
            var first = firstOfMonth.firstDayOfWeek(resolved.weekStarts());

            var last = firstOfMonth.addMonths(1);
            var days = DayPilot.Date.daysDiff(first.d, last.d);

            var rowCount = (this.rowsPerMonth === "Auto") ? Math.ceil(days / 7) : 6;
            month.rowCount = rowCount;
            var today = (new DayPilot.Date()).getDatePart();
            
            var width = this.cellWidth * 7 + this._weekNumberWidth();
            var height = this.cellHeight * rowCount + this.titleHeight + this.dayHeaderHeight;
            month.height = height;

            var main = document.createElement("div");
            main.style.width = (width) + 'px';
            main.style.height = (height) + 'px';
            
            
            if (this.orientation === "Horizontal") {
                main.style.position = "absolute";
                main.style.left = (width * j) + "px";
                main.style.top = "0px";
            }
            else {
                main.style.position = 'relative';
            }
            if (this.cssOnly) {
                main.className = this._prefixCssClass('_month');
            }
            else {
                main.className = this._prefixCssClass('month');
            }
            
            main.style.cursor = 'default';
            main.style.MozUserSelect = 'none';
            main.style.KhtmlUserSelect = 'none';
            main.style.WebkitUserSelect = 'none';
            
            main.month = month;

            this.root.appendChild(main);

            var totalHeaderHeight = this.titleHeight + this.dayHeaderHeight;

            // title left
            var tl = document.createElement("div");
            tl.style.position = 'absolute';
            tl.style.left = '0px';
            tl.style.top = '0px';
            tl.style.width = this.cellWidth + 'px';
            tl.style.height = this.titleHeight + 'px';
            tl.style.lineHeight = this.titleHeight + 'px';
            tl.style.textAlign = 'left';
            tl.setAttribute("unselectable", "on");
            if (this.cssOnly) {
                tl.className = this._prefixCssClass('_titleleft');
            }
            else {
                tl.className = this._prefixCssClass('titleleft');
            }
            if (showLinks.left) {
                tl.style.cursor = 'pointer';
                tl.innerHTML = "<span style='margin-left:2px;'>&lt;</span>";
                tl.onclick = this._clickLeft;
            }
            main.appendChild(tl);
            this.tl = tl;

            // title center
            var ti = document.createElement("div");
            ti.style.position = 'absolute';
            ti.style.left = this.cellWidth + 'px';
            ti.style.top = '0px';
            ti.style.width = (this.cellWidth * 5 + this._weekNumberWidth()) + 'px';
            ti.style.height = this.titleHeight + 'px';
            ti.style.lineHeight = this.titleHeight + 'px';
            ti.style.textAlign = 'center';
            ti.setAttribute("unselectable", "on");
            if (this.cssOnly) {
                ti.className = this._prefixCssClass('_title');
            }
            else {
                ti.className = this._prefixCssClass('title');
            }
            ti.innerHTML = resolved.locale().monthNames[startDate.getMonth()] + ' ' + startDate.getYear();
            main.appendChild(ti);
            this.ti = ti;

            // title right
            var tr = document.createElement("div");
            tr.style.position = 'absolute';
            tr.style.left = (this.cellWidth * 6 + this._weekNumberWidth()) + 'px';
            tr.style.top = '0px';
            tr.style.width = this.cellWidth + 'px';
            tr.style.height = this.titleHeight + 'px';
            tr.style.lineHeight = this.titleHeight + 'px';
            tr.style.textAlign = 'right';
            tr.setAttribute("unselectable", "on");
            if (this.cssOnly) {
                tr.className = this._prefixCssClass('_titleright');
            }
            else {
                tr.className = this._prefixCssClass('titleright');
            }
            if (showLinks.right) {
                tr.style.cursor = 'pointer';
                tr.innerHTML = "<span style='margin-right:2px;'>&gt;</span>";
                tr.onclick = this._clickRight;
            }
            main.appendChild(tr);
            this.tr = tr;


            var xOffset = this._weekNumberWidth();
            if (this.showWeekNumbers) {
                for (var y = 0; y < rowCount; y++) {
                    var day = first.addDays(y * 7);
                    var weekNumber = null;
                    switch (this.weekNumberAlgorithm) {
                        case "Auto":
                            weekNumber = (resolved.weekStarts() === 0) ? day.weekNumber() : day.weekNumberISO();
                            break;
                        case "US":
                            weekNumber = day.weekNumber();
                            break;
                        case "ISO8601":
                            weekNumber = day.weekNumberISO();
                            break;
                        default:
                            throw "Unknown weekNumberAlgorithm value.";
                    }

                    var dh = document.createElement("div");
                    dh.style.position = 'absolute';
                    dh.style.left = (0) + 'px';
                    dh.style.top = (y * this.cellHeight + totalHeaderHeight) + 'px';
                    dh.style.width = this.cellWidth + 'px';
                    dh.style.height = this.cellHeight + 'px';
                    dh.style.lineHeight = this.cellHeight + 'px';
                    dh.style.textAlign = 'right';
                    dh.setAttribute("unselectable", "on");
                    if (this.cssOnly) {
                        dh.className = this._prefixCssClass('_weeknumber');
                    }
                    else {
                        dh.className = this._prefixCssClass('weeknumber');
                    }
                    dh.innerHTML = "<span style='margin-right: 2px'>" + weekNumber + "</span>";
                    main.appendChild(dh);
                    month.weeks.push(dh);
                }
            }


            for (var x = 0; x < 7; x++) {
                month.cells[x] = [];

                // day header
                var dh = document.createElement("div");
                dh.style.position = 'absolute';
                dh.style.left = (x * this.cellWidth + xOffset) + 'px';
                dh.style.top = this.titleHeight + 'px';
                dh.style.width = this.cellWidth + 'px';
                dh.style.height = this.dayHeaderHeight + 'px';
                dh.style.lineHeight = this.dayHeaderHeight + 'px';
                dh.style.textAlign = 'right';
                dh.setAttribute("unselectable", "on");
                if (this.cssOnly) {
                    dh.className = this._prefixCssClass('_dayheader');
                }
                else {
                    dh.className = this._prefixCssClass('dayheader');
                }
                dh.innerHTML = "<span style='margin-right: 2px'>" + this._getDayName(x) + "</span>";
                main.appendChild(dh);
                month.days.push(dh);

                for (var y = 0; y < rowCount; y++) {
                    var day = first.addDays(y * 7 + x);

                    var isSelected = this._isSelected(day) && this.selectMode !== 'none';

                    var isCurrentMonth = day.getMonth() === startDate.getMonth();
                    var isPrevMonth = day.getTime() < startDate.getTime();
                    //var isNextMonth = day.getYear() > startDate.getYear() || (day.getYear() == startDate.getYear() && day.getMonth() > startDate.getMonth());
                    var isNextMonth = day.getTime() > startDate.getTime();

                    var dayClass;

                    var dc = document.createElement("div");

                    dc.day = day;
                    dc.x = x;
                    dc.y = y;
                    dc.isCurrentMonth = isCurrentMonth;

                    if (this.cssOnly) {
                        dc.className = this._prefixCssClass((isCurrentMonth ? '_day' : '_dayother'));
                    }
                    else {
                        dc.className = this._prefixCssClass((isCurrentMonth ? 'day' : 'dayother'));
                    }
                    if (day.getTime() === today.getTime() && isCurrentMonth) {
                        this._addClass(dc, 'today');
                    }
                    if (day.dayOfWeek() === 0 || day.dayOfWeek() === 6) {
                        this._addClass(dc, 'weekend');
                    }

                    dc.style.position = 'absolute';
                    dc.style.left = (x * this.cellWidth + xOffset) + 'px';
                    dc.style.top = (y * this.cellHeight + totalHeaderHeight) + 'px';
                    dc.style.width = this.cellWidth + 'px';
                    dc.style.height = this.cellHeight + 'px';
                    dc.style.lineHeight = this.cellHeight + 'px'; // vertical alignment
                    dc.style.textAlign = 'right';
                    //dc.style.border = '1px solid white';

                    var inner = document.createElement("div");
                    inner.style.position = 'absolute';
                    if (this.cssOnly) {
                        inner.className = (day.getTime() === today.getTime() && isCurrentMonth) ? this._prefixCssClass('_todaybox') : this._prefixCssClass('_daybox');
                    }
                    else {
                        inner.className = (day.getTime() === today.getTime() && isCurrentMonth) ? this._prefixCssClass('todaybox') : this._prefixCssClass('daybox');
                    }
                    inner.style.left = '0px';
                    inner.style.top = '0px';
                    inner.style.width = (this.cellWidth - 2) + 'px';
                    inner.style.height = (this.cellHeight - 2) + 'px';
                    dc.appendChild(inner);

                    /*
                    if (isCurrentMonth) {
                    dc.style.cursor = 'pointer';
                    }
                    */

                    var cell = null;
                    if (this.cells && this.cells[day.toStringSortable()]) {
                        cell = this.cells[day.toStringSortable()];
                        if (cell.css) {
                            this._addClass(dc, cell.css);
                        }
                    }

                    var span = null;
                    if (isCurrentMonth || (showBefore && isPrevMonth) || (showAfter && isNextMonth)) {
                        span = document.createElement("span");
                        span.innerHTML = day.getDay();

                        dc.style.cursor = 'pointer';
                        dc.isClickable = true;
                        if (isSelected) {
                            this._addClass(dc, 'select');
                        }

                        if (cell && cell.html) {
                            span.innerHTML = cell.html;
                        }

                        span.style.marginRight = '2px';
                        dc.appendChild(span);

                    }


                    dc.setAttribute("unselectable", "on");

                    dc.onclick = this._cellClick;
                    dc.onmousedown = this._cellMouseDown;
                    dc.onmousemove = this._cellMouseMove;

                    if (isSelected) {
                        this.selected.push(dc);
                    }

                    main.appendChild(dc);

                    month.cells[x][y] = dc;
                }
            }

            var line = document.createElement("div");
            line.style.position = 'absolute';
            line.style.left = '0px';
            line.style.top = (totalHeaderHeight - 2) + 'px';
            line.style.width = (this.cellWidth * 7 + this._weekNumberWidth()) + 'px';
            line.style.height = '1px';
            line.style.fontSize = '1px';
            line.style.lineHeight = '1px';
            if (this.cssOnly) {
                line.className = this._prefixCssClass("_line");
            }
            else {
                line.className = this._prefixCssClass("line");
            }
            //line.style.borderBottom = '1px solid black';

            main.appendChild(line);
            this.months.push(month);
        };

        this._weekNumberWidth = function() {
            if (this.showWeekNumbers) {
                return this.cellWidth;
            }
            return 0;
        };

        this._updateFreeBusy = function() {
            if (!this.items) {
                return;
            }

            for (var j = 0; j < this.showMonths; j++) {
                for (var x = 0; x < 7; x++) {
                    for (var y = 0; y < 6; y++) {
                        var cell = this.months[j].cells[x][y];
                        if (!cell) {
                            continue;
                        }
                        if (this.items[cell.day.toStringSortable()] === 1) {
                            this._addClass(cell, 'busy');
                        }
                        else {
                            this._removeClass(cell, 'busy');
                        }
                    }
                }
            }
        };


        this._saveState = function() {
            var s = {};
            s.startDate = calendar.startDate;
            s.selectionStart = calendar.selectionStart;
            s.selectionEnd = calendar.selectionEnd.addDays(1);
            calendar.state.value = DayPilot.JSON.stringify(s);
        };

        this._adjustSelection = function() {
            // ignores selectionEnd
            // uses selectMode
            switch (this.selectMode) {
                case 'day':
                    this.selectionEnd = this.selectionStart;
                    break;
                case 'week':
                    this.selectionStart = this.selectionStart.firstDayOfWeek(resolved.weekStarts());
                    this.selectionEnd = this.selectionStart.addDays(6);
                    break;
                case 'month':
                    this.selectionStart = this.selectionStart.firstDayOfMonth();
                    this.selectionEnd = this.selectionStart.lastDayOfMonth();
                    break;
                case 'none':
                    this.selectionEnd = this.selectionStart;
                    break;
                default:
                    throw "Unkown selectMode value.";
            }

        };

        this.select = function(date) {
            var focus = true;

            var originalStart = this.selectionStart;
            var originalEnd = this.selectionEnd;

            this.selectionStart = new DayPilot.Date(date).getDatePart();
            this.selectionDay = this.selectionStart;

            var startChanged = false;
            if (focus) {

                var newStart = this.startDate;
                if (this.selectionStart.getTime() < this.visibleStart().getTime() || this.selectionStart.getTime() > this.visibleEnd().getTime()) {
                    newStart = this.selectionStart.firstDayOfMonth();
                }

                if (newStart.toStringSortable() !== this.startDate.toStringSortable()) {
                    startChanged = true;
                }

                this.startDate = newStart;
            }

            this._adjustSelection();

            // redraw
            this._clearTable();
            this._prepare();
            this._drawMonths();
            this._updateFreeBusy();
            this._saveState();

            if (!originalStart.equals(this.selectionStart) || !originalEnd.equals(this.selectionEnd)) {
                //alert('time range');
                this._timeRangeSelectedDispatch();
            }

            if (startChanged) {
                //alert('visible range');
                this._visibleRangeChangedDispatch();
            }
        };
        
        this.update = function() {
            // redraw
            this._clearTable();
            this._prepare();
            this._drawMonths();
            this._loadEvents();
            this._updateFreeBusy();
            this._saveState();
        };

        this._callBack2 = function(action, data, parameters) {
            var envelope = {};
            envelope.action = action;
            envelope.parameters = parameters;
            envelope.data = data;
            envelope.header = this._getCallBackHeader();

            var commandstring = "JSON" + DayPilot.JSON.stringify(envelope);
            
            var context = null;
            if (this.backendUrl) {
                DayPilot.request(this.backendUrl, this._callBackResponse, commandstring, this._ajaxError);
            }
            else {
                WebForm_DoCallback(this.uniqueID, commandstring, this._updateView, context, this.callbackError, true);
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

        this._postBack2 = function(action, data, parameters) {
            var envelope = {};
            envelope.action = action;
            envelope.parameters = parameters;
            envelope.data = data;
            envelope.header = this._getCallBackHeader();

            var commandstring = "JSON" + DayPilot.JSON.stringify(envelope);
            __doPostBack(calendar.uniqueID, commandstring);
        };

        this._getCallBackHeader = function() {
            var h = {};
            h.v = this.v;
            h.startDate = this.startDate;
            h.selectionStart = this.selectionStart;
            return h;
        };

        this._listen = function(action, data) {
            if (action === 'refresh') {
                this._visibleRangeChangedDispatch();
            }
        };

        this._getDayName = function(i) {
            var x = i + resolved.weekStarts();
            if (x > 6) {
                x -= 7;
            }
            return resolved.locale().dayNamesShort[x];

        };

        this._isSelected = function(date) {
            if (this.selectionStart === null || this.selectionEnd === null) {
                return false;
            }

            if (this.selectionStart.getTime() <= date.getTime() && date.getTime() <= this.selectionEnd.getTime()) {
                return true;
            }

            return false;
        };

        this._cellMouseDown = function(ev) {
        };

        this._cellMouseMove = function(ev) {
        };

        this._cellClick = function(ev) {
            var month = this.parentNode.month;

            var x = this.x;
            var y = this.y;
            var day = month.cells[x][y].day;

            if (!month.cells[x][y].isClickable) {
                return;
            }

            calendar.clearSelection();
            
            calendar.selectionDay = day;

            var day = calendar.selectionDay;
            switch (calendar.selectMode) {
                case 'none':
                    //var s = month.cells[x][y];
                    calendar.selectionStart = day;
                    calendar.selectionEnd = day;
                    break;
                case 'day':
                    var s = month.cells[x][y];
                    calendar._addClass(s, 'select');
                    calendar.selected.push(s);
                    calendar.selectionStart = s.day;
                    calendar.selectionEnd = s.day;
                    break;
                case 'week':
                    for (var j = 0; j < 7; j++) {
                        calendar._addClass(month.cells[j][y], 'select');
                        calendar.selected.push(month.cells[j][y]);
                    }
                    calendar.selectionStart = month.cells[0][y].day;
                    calendar.selectionEnd = month.cells[6][y].day;
                    break;
                case 'month':
                    var start = null;
                    var end = null;
                    for (var y = 0; y < 6; y++) {
                        for (var x = 0; x < 7; x++) {
                            var s = month.cells[x][y];
                            if (!s) {
                                continue;
                            }
                            if (s.day.getYear() === day.getYear() && s.day.getMonth() === day.getMonth()) {
                                calendar._addClass(s, 'select');
                                calendar.selected.push(s);
                                if (start === null) {
                                    start = s.day;
                                }
                                end = s.day;
                            }
                        }
                    }
                    calendar.selectionStart = start;
                    calendar.selectionEnd = end;
                    break;
                default:
                    throw 'unknown selectMode';
            }

            calendar._saveState();

            calendar._timeRangeSelectedDispatch();
        };

        this._timeRangeSelectedDispatch = function() {
            var start = calendar.selectionStart;
            var end = calendar.selectionEnd.addDays(1);
            var days = DayPilot.Date.daysDiff(start.d, end.d);
            var day = calendar.selectionDay;

            if (calendar._api2()) {
                
                var args = {};
                args.start = start;
                args.end = end;
                args.day = day;
                args.days =  days;
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
                    case 'Bind':
                        var bound = eval(calendar.bound);
                        if (bound) {
                            var selection = {};
                            selection.start = start;
                            selection.end = end;
                            selection.days = days;
                            selection.day = day;
                            bound.commandCallBack(calendar.command, selection);
                        }
                        break;
                    case 'None':
                        break;
                    case 'PostBack':
                        calendar.timeRangeSelectedPostBack(start, end, day);
                        break;
                }
                
                if (typeof calendar.onTimeRangeSelected === 'function') {
                    calendar.onTimeRangeSelected(args);
                }
                
            }
            else {
                switch (calendar.timeRangeSelectedHandling) {
                    case 'Bind':
                        var bound = eval(calendar.bound);
                        if (bound) {
                            var selection = {};
                            selection.start = start;
                            selection.end = end;
                            selection.days = days;
                            selection.day = day;
                            bound.commandCallBack(calendar.command, selection);
                        }
                        break;
                    case 'JavaScript':
                        calendar.onTimeRangeSelected(start, end, day);
                        break;
                    case 'None':
                        break;
                    case 'PostBack':
                        calendar.timeRangeSelectedPostBack(start, end, day);
                        break;
                }
            }
            
            
            /*
            switch (calendar.timeRangeSelectedHandling) {
                case 'Bind':
                    var bound = eval(calendar.bound);
                    if (bound) {
                        var selection = {};
                        selection.start = start;
                        selection.end = end;
                        selection.days = days;
                        selection.day = day;
                        bound.commandCallBack(calendar.command, selection);
                    }
                    break;
                case 'JavaScript':
                    calendar.onTimeRangeSelected(start, end, day);
                    break;
                case 'None':
                    break;
                case 'PostBack':
                    calendar.timeRangeSelectedPostBack(start, end, day);
                    break;
            }
            */
        };

        this.timeRangeSelectedPostBack = function(start, end, data, day) {
            var params = {};
            params.start = start;
            params.end = end;
            params.day = day;

            this._postBack2('TimeRangeSelected', data, params);
        };

        this._clickRight = function(ev) {
            calendar._moveMonth(calendar.skipMonths);
        };

        this._clickLeft = function(ev) {
            calendar._moveMonth(-calendar.skipMonths);
        };

        this._moveMonth = function(i) {
            this.startDate = this.startDate.addMonths(i);
            this._clearTable();
            this._prepare();
            this._drawMonths();

            this._saveState();

            this._visibleRangeChangedDispatch();
            this._updateFreeBusy();
        };

        this.visibleStart = function() {
            return calendar.startDate.firstDayOfMonth().firstDayOfWeek(resolved.weekStarts());
        };

        this.visibleEnd = function() {
            return calendar.startDate.firstDayOfMonth().addMonths(this.showMonths - 1).firstDayOfWeek(resolved.weekStarts()).addDays(42);
        };

        this._visibleRangeChangedDispatch = function() {
            var start = this.visibleStart();
            var end = this.visibleEnd();

            if (calendar._api2()) {
                
                var args = {};
                args.start = start;
                args.end = end;
                args.preventDefault = function() {
                    this.preventDefault.value = true;
                };
                
                if (typeof calendar.onVisibleRangeChange === 'function') {
                    calendar.onVisibleRangeChange(args);
                    if (args.preventDefault.value) {
                        return;
                    }
                }

                // now perform the default builtin action
                switch (this.visibleRangeChangedHandling) {
                    case "CallBack":
                        this.visibleRangeChangedCallBack(null);
                        break;
                    case "PostBack":
                        this.visibleRangeChangedPostBack(null);
                        break;
                    case "Disabled":
                        break;
                }
                
                if (typeof calendar.onVisibleRangeChanged === 'function') {
                    calendar.onVisibleRangeChanged(args);
                }
                
            }
            else {
                switch (this.visibleRangeChangedHandling) {
                    case "CallBack":
                        this.visibleRangeChangedCallBack(null);
                        break;
                    case "PostBack":
                        this.visibleRangeChangedPostBack(null);
                        break;
                    case "JavaScript":
                        this.onVisibleRangeChanged(start, end);
                        break;
                    case "Disabled":
                        break;
                }
            }

/*
            switch (this.visibleRangeChangedHandling) {
                case "CallBack":
                    this.visibleRangeChangedCallBack(null);
                    break;
                case "PostBack":
                    this.visibleRangeChangedPostBack(null);
                    break;
                case "JavaScript":
                    this.onVisibleRangeChanged(start, end);
                    break;
                case "Disabled":
                    break;
            }
            */
        };


        this.visibleRangeChangedCallBack = function(data) {
            var parameters = {};
            this._callBack2("Visible", data, parameters);
        };

        this.visibleRangeChangedPostBack = function(data) {
            var parameters = {};
            this._postBack2("Visible", data, parameters);
        };

        this._updateView = function(result, context) {
            var result = eval("(" + result + ")");
            calendar.items = result.Items;
            calendar.cells = result.Cells;
            calendar._updateFreeBusy();
        };

        this._drawMonths = function() {
            for (var j = 0; j < this.showMonths; j++) {
                var showLinks = this._getShowLinks(j);
                this._drawTable(j, showLinks);
            }
			
    	    this.root.style.height = this._getHeight() + "px"; 
			/*
            var div = document.createElement("div");
            div.style.clear = "left";
            div.style.height = "0px";
            div.style.width = "0px";
            this.root.appendChild(div);
            */

        };
        
        this._getHeight = function() {
            if (this.orientation === "Horizontal") {
                var max = 0;
                for (var i = 0; i < this.months.length; i++) {
                    var month = this.months[i];
                    if (month.height > max) {
                        max = month.height;
                    }
                }
                return max;
            }
            else {
                var total = 0;
                for (var i = 0; i < this.months.length; i++) {
                    var month = this.months[i];
                    //total += this.showMonths*(this.cellHeight*month.rowCount + this.titleHeight + this.dayHeaderHeight);
                    total += month.height;
                }
                return total;
            }
        };
        
        this._getShowLinks = function(j) {
            if (this.internal.showLinks) {
                return this.internal.showLinks;
            }

            var showLinks = {};
            showLinks.left = (j === 0);
            showLinks.right = (j === 0);
            showLinks.before = j === 0;
            showLinks.after = j === this.showMonths - 1;

            if (this.orientation === "Horizontal") {
                showLinks.right = (j === this.showMonths - 1);
            }
            
            return showLinks;
        };
        
        this.internal = {};
		
        this._resolved = {};
        var resolved = this._resolved;
        
        resolved.locale = function() {
            return DayPilot.Locale.find(calendar.locale);
        };
        
        resolved.weekStarts = function() {
            if (calendar.weekStarts === 'Auto') {
                var locale = resolved.locale();
                if (locale) {
                    return locale.weekStarts;
                }
                else {
                    return 0; // Sunday
                }
            }
            else {
                return calendar.weekStarts;
            }
        };

        this.clearSelection = function() {
            for (var j = 0; j < this.selected.length; j++) {
                this._removeClass(this.selected[j], 'select');
            }
            this.selected = [];
        };
        
        this._loadFromServer = function() {
            // make sure it has a place to ask
            if (this.backendUrl || typeof WebForm_DoCallback === 'function') {
                return (typeof calendar.items === 'undefined') || (!calendar.items);
            }
            else {
                return false;
            }
        };
        
        this.events = {};
        
        this._loadEvents = function() {
            if (!DayPilot.isArray(this.events.list)) {
                return;
            }
            
            this.items = {};
            
            for(var i = 0; i < this.events.list.length; i++) {
                var e = this.events.list[i];
                var days = this._eventDays(e);
                for(var name in days) {
                    this.items[name] = 1;
                }
            }
        };
        
        this._eventDays = function(e) {
            var start = new DayPilot.Date(e.start);
            var end = new DayPilot.Date(e.end);
            
            var days = {};
            
            var d = start.getDatePart();
            while (d.getTime() < end.getTime()) {
                days[d.toStringSortable()] = 1;
                d = d.addDays(1);
            }
            
            return days;
        };
        

        this.init = function() {
            this.root = document.getElementById(id);
            
            if (!this.root) {
                throw "DayPilot.Navigator.init(): The placeholder element not found: '" + id + "'.";
            }

            if (this.root.dp) { // already initialized
                return;
            }
            
            this._adjustSelection();
            this._prepare();
            this._drawMonths();
            this._loadEvents();
            this._updateFreeBusy();
            this._registerDispose();
            
            var loadFromServer = this._loadFromServer();
            if (loadFromServer) {
                this._visibleRangeChangedDispatch(); // TODO change to "Init"?
            }
            this.initialized = true;
           
        };
        
        this.dispose = function() {
            var c = calendar;
            
            if (!c.root) {
                return;
            }
            
            c.root.removeAttribute("style");
            c.root.removeAttribute("class");
            c.root.dp = null;
            c.root.innerHTML = null;
            c.root = null;
            
        };
        
        this._registerDispose = function() {
            var root = document.getElementById(id);
            root.dispose = this.dispose;
        };        
		
        this.Init = this.init;

    };

    // publish the API 

    // (backwards compatibility)    
    DayPilot.NavigatorVisible.Navigator = DayPilotNavigator.Navigator;

    // current
    //DayPilot.Navigator = DayPilotNavigator.Navigator;

    // experimental jQuery bindings
    if (typeof jQuery !== 'undefined') {
        (function($) {
            $.fn.daypilotNavigator = function(options) {
                var first = null;
                var j = this.each(function() {
                    if (this.daypilot) { // already initialized
                        return;
                    };

                    var daypilot = new DayPilot.Navigator(this.id);
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
