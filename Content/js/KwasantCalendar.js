(function ($) {
    var settings;
    var storedCalendars = [];
    var calendar;
    var nav;


    $.fn.KCalendar = function (options) {
        
        //Setup defaults
        settings = $.extend({
            topElement: this,

            showDay: true,
            showWeek: true,
            showMonth: true,

            showNavigator: true,

            getCalendarBackendURL: function() { alert('getCalendarBackendURL must be set in the options.'); },
            getMonthBackendURL: function() { alert('getMonthBackendURL must be set in the options.'); },
            getNavigatorBackendURL: function() { alert('getNavigatorBackendURL must be set in the options.'); },
            
            getEditURL: function (id) { alert('getEditURL function must be set in the options, unless providing an onEdit function override.'); },
            getNewURL: function (start, end) { alert('getNewURL function must be set in the options, unless providing an onEdit function override.'); },
            getDeleteURL: function (id) { alert('getDeleteURL function must be set in the options, unless providing an onEdit function override.'); },
            getMoveURL: function (id, newstart, newend) { alert('getMoveURL function must be set in the options, unless providing an onEdit function override.'); },
            
            editRequiresConfirmation: true,
            newRequiresConfirmation: true,
            deleteRequiresConfirmation: true,
            moveRequiresConfirmation: true,

            onEventClick: onEventClick,
            onEventNew: onEventNew,
            onEventDelete: onEventDelete,
            onEventMove: onEventMove,

        }, options);

        createCalendars();

        this.getEvents = function() {
            if (storedCalendars.length > 0) {
                return storedCalendars[0].events.list;
            }
            return [];
        };

        //This causes all the widgets (all three month controls and the navigator) to reload data from the server
        this.refreshCalendars = function () {
            $.each(storedCalendars, function (i, ele) {
                ele.commandCallBack('refresh');
            });

            if (nav !== null && nav !== undefined)
                nav.visibleRangeChangedCallBack(); // This actually causes the navigator to reload events from the server, despite the weird name.
        };

        //This allows us to update the backend urls of our widgets. This is used for our booking request switcher
        this.updateBackendURLs = function(calendarBackend, monthBackend, navigatorBackend) {
            $.each(storedCalendars, function (i, ele) {
                if (calendarBackend !== undefined && calendarBackend !== null && ele instanceof DayPilot.Calendar)
                    ele.backendUrl = calendarBackend;
                else if (monthBackend !== undefined && monthBackend !== null && ele instanceof DayPilot.Month)
                    ele.backendUrl = monthBackend;
            });
            if (navigatorBackend !== undefined && navigatorBackend !== null && nav !== undefined && nav !== null)
                nav.backendUrl = navigatorBackend;
                
            this.refreshCalendars();
        };
        calendar = this;

        this.getStoredCalendar = function () {
            return storedCalendars;
        };

        this.getNavigator = function () {
            return nav;
        };

        return this;
    };

    var createCalendars = function() {
        //First, setup the HTML

        //This displays the toolbar to swap between day, week and month
        var toolbar = $("<div class='toolbar'></div>");
        var inner = $("<div class='divCalender-inner'></div>");

        var switcher = new DayPilot.Switcher();
        storedCalendars = [];

        var queueCalendarForInit = function(createFunc, name) {
            var calendarPair = createFunc();

            if (calendarPair === null ||
                calendarPair === undefined ||
                calendarPair.dp === null ||
                calendarPair.dp === undefined) {
                return;
            }

            var swapButton = $("<a id=" + getRandomID() + " href='#'>" + name + "</a>");
            storedCalendars.push(calendarPair.dp);
            switcher.addView(calendarPair.dp);
            switcher.addButton(swapButton.get(0), calendarPair.dp);

            inner.append(calendarPair.div);
            toolbar.append(swapButton);
        };

        if (settings.showDay) {
            queueCalendarForInit(createDayCalendar, 'Day');
        }
        if (settings.showWeek) {
            queueCalendarForInit(createWeekCalendar, 'Week');
        }
        if (settings.showMonth) {
            queueCalendarForInit(createMonthCalendar, 'Month');
        }

        var toolbarRow = $("<div class='row toolbar-section'></div>");
        toolbarRow.append(toolbar);


        var calendarRow = $("<div class='row'></div>");
        calendarRow.append(inner);

        var calendarBox = $("<div class='divCalender calendar-inner'></div>");
        calendarBox.append(toolbarRow);
        calendarBox.append(calendarRow);

        settings.topElement.append(calendarBox);

        var firstToDisplay = null;
        for (var i = 0; i < storedCalendars.length; i++) {
            storedCalendars[i].init();
            if (i == 0)
                firstToDisplay = storedCalendars[0];
        }
        
        if (settings.showNavigator) {
            var navi = createNavigator();
            var wrapper = $('<div class="calendar-area">');
            wrapper.append(navi.div);            
            settings.topElement.append(wrapper);
            toolbarRow.append(wrapper);
            navi.dp.init();
            switcher.addNavigator(navi.dp);
            nav = navi.dp;
        }

        

        if (firstToDisplay !== null)
            switcher.show(firstToDisplay);

    };

    var getRandomID = function() {
        var idLength = 10;
        return new Array(idLength + 1).join((Math.random().toString(36) + '00000000000000000').slice(2, 18)).slice(0, idLength);
    };

    var createDayCalendar = function () {
        var cal = createDefaultCalendar();
        cal.dp.viewType = 'Day';
        
        return cal;
    };
    var createWeekCalendar = function () {
        var cal = createDefaultCalendar();
        cal.dp.viewType = 'Week';
        cal.dp.headerHeight = 30;
        
        return cal;
    };
    var createMonthCalendar = function () {
        var id = getRandomID();
        var divHolder = $("<div id='" + id + "'></div>");

        var dp = new DayPilot.Month(id);
        dp.onAjaxError = function (args) { var request = args.request; if (DayPilot.Modal) { new DayPilot.Modal().showHtml(args.request.responseText); } else { alert('AJAX callback error (500)'); }; };
        dp.allowMultiSelect = true;
        dp.afterEventRender = function (e, div) {; };
        dp.afterRender = function (data, isCallBack) {; };
        dp.api = 1;
        dp.autoRefreshCommand = 'refresh';
        dp.autoRefreshEnabled = false;
        dp.autoRefreshInterval = 60;
        dp.autoRefreshMaxCount = 20;
        dp.backColor = '#FFFFD5';
        dp.borderColor = 'Black';
        dp.cellHeaderBackColor = '';
        dp.cellHeaderFontColor = null;
        dp.cellHeaderFontFamily = 'Open Sans, Helvetica, Helvetica, Arial, sans-serif';
        dp.cellHeaderFontSize = '10pt';
        dp.cellHeight = 90;
        dp.cellHeaderHeight = 16;
        dp.cellMode = false;
        dp.theme = 'calendar_green';
        dp.cssOnly = false;
        dp.eventBackColor = 'White';
        dp.eventBorderColor = 'Black';
        dp.eventCorners = 'Rounded';
        dp.eventFontColor = '#000000';
        dp.eventFontFamily = 'Open Sans, Helvetica, Helvetica, Arial, sans-serif';
        dp.eventFontSize = '10px';
        dp.eventHeight = 50;
        dp.eventMoveToPosition = false;
        dp.eventStartTime = true;
        dp.eventEndTime = true;
        dp.eventTextLayer = 'Bottom';
        dp.eventTextAlignment = 'Center';
        dp.eventTextLeftIndent = 50;
        dp.innerBorderColor = '#CCCCCC';
        dp.headerBackColor = '#ECE9D8';
        dp.headerFontColor = '#000000';
        dp.headerFontSize = '12px';
        dp.headerHeight = 50;
        dp.heightSpec = 'Auto';
        dp.height = '550';
        dp.hideUntilInit = false;
        dp.locale = 'en-us';
        dp.messageHideAfter = 5000;
        dp.nonBusinessBackColor = '#FFF4BC';
        dp.shadowType = 'Fill';
        dp.showWeekend = true;
        dp.showToolTip = true;
        dp.timeFormat = 'Auto';
        dp.viewType = 'Month';
        dp.weekStarts = 0;
        dp.width = '100%';
        dp.weeks = 1;
        dp.eventClickHandling = 'JavaScript';
        dp.eventDoubleClickHandling = 'JavaScript';
        dp.eventSelectHandling = 'JavaScript';
        dp.eventMoveHandling = 'JavaScript';
        dp.eventResizeHandling = 'JavaScript';
        dp.eventRightClickHandling = 'ContextMenu';
        dp.headerClickHandling = 'JavaScript';
        dp.timeRangeDoubleClickHandling = 'Disabled';
        dp.timeRangeSelectedHandling = 'JavaScript';
        dp.onEventDoubleClick = function (e) {; };
        dp.onEventSelect = function (e, change) {; };
        dp.onEventResize = function (e, newStart, newEnd) { settings.onEventMove(e.id(), newStart, newEnd); };
        dp.onEventRightClick = function (e) {; };
        dp.onHeaderClick = function (e) { var day = e.day;; };
        dp.onTimeRangeDoubleClick = function (start, end) {; };
        DayPilot.Locale.register(new DayPilot.Locale('en-us', { 'dayNames': ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'], 'dayNamesShort': ['Su', 'Mo', 'Tu', 'We', 'Th', 'Fr', 'Sa'], 'monthNames': ['January', 'February', 'March', 'April', 'May', 'June', 'July', 'August', 'September', 'October', 'November', 'December', ''], 'monthNamesShort': ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec', ''], 'timePattern': 'h:mm tt', 'datePattern': 'M/d/yyyy', 'dateTimePattern': 'M/d/yyyy h:mm tt', 'timeFormat': 'Clock12Hours', 'weekStarts': 0 }));


        dp.onEventClick = function (e) { settings.onEventClick(e.id()); };
        dp.onTimeRangeSelected = function (start, end) { settings.onEventNew(start, end); };
        dp.onEventDelete = function (e) { settings.onEventDelete(e.id()); };;
        dp.onEventMove = function (e, newStart, newEnd) { settings.onEventMove(e.id(), newStart, newEnd); };;
        dp.backendUrl = settings.getMonthBackendURL();
        
        dp.contextMenu = new DayPilot.Menu([
            { text: "Delete", onclick: function () { settings.onEventDelete(this.source.value()); } }
        ]);

        return { dp: dp, div: divHolder };
    };
    
    var createDefaultCalendar = function() {
        var id = getRandomID();
        var divHolder = $("<div id='" + id + "'></div>");

        var dp = new DayPilot.Calendar(id);
       // f0f0f0
        dp.allDayEnd = 'DateTime';
        dp.allDayEventBackColor = 'white';
        dp.allDayEventBorderColor = '#e6e6e6';
        dp.allDayEventFontFamily = 'Open Sans, Helvetica, Helvetica, Arial, sans-serif';
        dp.allDayEventFontSize = '8pt';
        dp.allDayEventFontColor = '#000';
        dp.allDayEventHeight = 25;
        dp.allowMultiSelect = true;
        dp.api = 1;
        dp.autoRefreshCommand = 'refresh';
        dp.autoRefreshEnabled = false;
        dp.autoRefreshInterval = 60;
        dp.autoRefreshMaxCount = 20;
        dp.borderColor = '#e6e6e6';
        dp.businessBeginsHour = 9;
        dp.businessEndsHour = 18;
        dp.cellBackColor = '#FFFFFF';
        dp.cellBackColorNonBusiness = '#f0f0f0';
        dp.cellBorderColor = '#DEDFDE';
        dp.cellHeight = 25;
        dp.cellDuration = 30;
        dp.columnMarginRight = 5;
        dp.columnWidthSpec = 'Auto';
        dp.columnWidth = 200;
        dp.cornerBackColor = '#f8f8f8';
        dp.crosshairColor = 'gray';
        dp.crosshairOpacity = 20;
        dp.crosshairType = 'Header';
        dp.theme = 'calendar_green';
        dp.cssOnly = false;
        dp.deleteImageUrl = null;
        dp.dayBeginsHour = 0;
        dp.dayEndsHour = 24;
        dp.days = 1;
        dp.durationBarColor = '#0000ff';
        dp.durationBarVisible = false;
        dp.durationBarWidth = 5;
        dp.durationBarImageUrl = null;
        dp.eventArrangement = 'SideBySide';
        dp.eventBackColor = '#000';
        dp.eventBorderColor = '#0e0e0e';
        dp.eventFontFamily = 'Open Sans, Helvetica, Helvetica, Arial, sans-serif';
        dp.eventFontSize = '8pt';
        dp.eventFontColor = '#ffffff';
        dp.eventHeaderFontSize = '8pt';
        dp.eventHeaderFontColor = '#ffffff';
        dp.eventHeaderHeight = 14;
        dp.eventHeaderVisible = false;
        dp.eventSelectColor = '#0000ff';
        dp.headerFontSize = '12pt';
        dp.headerFontFamily = 'Open Sans, Helvetica, Helvetica, Arial, sans-serif';
        dp.headerFontColor = '#333';
        dp.headerHeight = 50;
        dp.headerHeightAutoFit = false;
        dp.headerLevels = 1;
        dp.height = 550;
        dp.heightSpec = 'BusinessHours';
        dp.hideFreeCells = false;
        dp.hideUntilInit = false;
        dp.hourHalfBorderColor = '#EBEDEB';
        dp.hourBorderColor = '#DEDFDE';
        dp.hourFontColor = '#333';
        dp.hourFontFamily = 'Open Sans, Helvetica, Helvetica, Arial, sans-serif';
        dp.hourNameBackColor = '#f8f8f8';
        dp.hourNameBorderColor = '#DEDFDE';
        dp.hourWidth = 45;
        dp.initScrollPos = '450';
        dp.loadingLabelText = 'Loading...';
        dp.loadingLabelVisible = true;
        dp.loadingLabelFontSize = '10pt';
        dp.loadingLabelFontFamily = 'Open Sans, Helvetica, Helvetica, Arial, sans-serif';
        dp.loadingLabelFontColor = '#ffffff';
        dp.loadingLabelBackColor = '#ff0000';
        dp.locale = 'en-us';
        dp.messageHideAfter = 5000;
        dp.moveBy = 'Full';
        dp.rtl = false;
        dp.roundedCorners = true;
        dp.scrollLabelsVisible = true;
        dp.scrollDownUrl = null;
        dp.scrollUpUrl = null;
        dp.selectedColor = '#316AC5';
        dp.shadow = 'Fill';
        dp.showToolTip = false;
        dp.showAllDayEvents = false;
        dp.showAllDayEventStartEnd = false;
        dp.showHeader = true;
        dp.showHours = true;
        dp.timeFormat = 'Clock12Hours';
        dp.timeHeaderCellDuration = 60;
        dp.useEventBoxes = 'Always';
        dp.useEventSelectionBars = false;
        dp.viewType = 'Day';
        dp.onAjaxError = function (args) { var request = args.request; if (DayPilot.Modal) { new DayPilot.Modal().showHtml(args.request.responseText); } else { alert('AJAX callback error (500)'); }; };
        dp.afterRender = function (data, isCallBack) {; };
        dp.eventClickHandling = 'JavaScript';
        dp.eventSelectHandling = 'Disabled';
        dp.eventDeleteHandling = 'JavaScript';
        dp.eventDoubleClickHandling = 'Disabled';
        dp.eventEditHandling = 'CallBack';
        dp.eventHoverHandling = 'Bubble';
        dp.eventMoveHandling = 'JavaScript';
        dp.eventResizeHandling = 'JavaScript';
        dp.eventRightClickHandling = 'ContextMenu';
        dp.headerClickHandling = 'JavaScript';
        dp.timeRangeDoubleClickHandling = 'Disabled';
        dp.timeRangeSelectedHandling = 'JavaScript';
        dp.onEventSelect = function (e, change) {; };
        dp.onEventDoubleClick = function (e) {; };
        dp.onEventEdit = function (e, newText) {; };
        dp.onEventResize = function (e, newStart, newEnd) { settings.onEventMove(e.id(), newStart, newEnd); };
        dp.onEventRightClick = function (e) {; };
        dp.onHeaderClick = function (c) {; };
        dp.onTimeRangeDoubleClick = function (start, end, resource) {; };
        
        DayPilot.Locale.register(new DayPilot.Locale('en-us', { 'dayNames': ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'], 'dayNamesShort': ['Su', 'Mo', 'Tu', 'We', 'Th', 'Fr', 'Sa'], 'monthNames': ['January', 'February', 'March', 'April', 'May', 'June', 'July', 'August', 'September', 'October', 'November', 'December', ''], 'monthNamesShort': ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec', ''], 'timePattern': 'h:mm tt', 'datePattern': 'M/d/yyyy', 'dateTimePattern': 'M/d/yyyy h:mm tt', 'timeFormat': 'Clock12Hours', 'weekStarts': 0 }));

        dp.backendUrl = settings.getCalendarBackendURL();

        dp.onEventClick = function(e) { settings.onEventClick(e.id()); };
        dp.onTimeRangeSelected = function(start, end) { settings.onEventNew(start, end); };
        dp.onEventDelete = function (e) { settings.onEventDelete(e.id()); };
        dp.onEventMove = function (e, newStart, newEnd) { settings.onEventMove(e.id(), newStart, newEnd); };;

        dp.contextMenu = new DayPilot.Menu([
            { text: "Delete", onclick: function() { settings.onEventDelete(this.source.value()); } }
        ]);

        return { dp: dp, div: divHolder };
    };

    var createNavigator = function() {
        var id = getRandomID();
        var divHolder = $("<div id='" + id + "'></div>");
        var date = new Date();
        var dp_navigator = new DayPilot.Navigator(id);
        dp_navigator.backendUrl = settings.getNavigatorBackendURL();
        dp_navigator.api = 1;
        dp_navigator.cellHeight = 20;
        dp_navigator.cellWidth = 20;
        dp_navigator.cellWidthSpec = "Auto";
        dp_navigator.command = 'navigate';
        dp_navigator.theme = 'navigator_green';
        dp_navigator.cssOnly = true;
        dp_navigator.dayHeaderHeight = 20;
        dp_navigator.locale = 'en-us';
        dp_navigator.month = date.getMonth() + 1; // Plus 1 because daypilot is 1-based index
        dp_navigator.orientation = 'Vertical';
        dp_navigator.rowsPerMonth = 'Six';
        dp_navigator.selectMode = 'day';
        dp_navigator.showMonths = 1;
        dp_navigator.showWeekNumbers = false;
        dp_navigator.skipMonths = 1;
        dp_navigator.titleHeight = 20;
        dp_navigator.weekStarts = 0;
        dp_navigator.weekNumberAlgorithm = 'Auto';
        dp_navigator.year = date.getFullYear();
        dp_navigator.onAjaxError = function(args) {
            var request = args.request;
            if (DayPilot.Modal) {
                new DayPilot.Modal().showHtml(args.request.responseText);
            } else {
                alert('AJAX callback error (500)');
            }
        };

        dp_navigator.visibleRangeChangedHandling = "CallBack";
        DayPilot.Locale.register(new DayPilot.Locale('en-us', { 'dayNames': ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'], 'dayNamesShort': ['Su', 'Mo', 'Tu', 'We', 'Th', 'Fr', 'Sa'], 'monthNames': ['January', 'February', 'March', 'April', 'May', 'June', 'July', 'August', 'September', 'October', 'November', 'December', ''], 'monthNamesShort': ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec', ''], 'timePattern': 'h:mm tt', 'datePattern': 'M/d/yyyy', 'dateTimePattern': 'M/d/yyyy h:mm tt', 'timeFormat': 'Clock12Hours', 'weekStarts': 0 }));


        return { dp: dp_navigator, div: divHolder };
    };

    var onEventClick = function(id) {
        if (Kwasant.IFrame.PopupsActive()) {
            return;
        }
        var url = settings.getEditURL(id);
        if (url === null || url === undefined || url === '')
            return;

        if (settings.editRequiresConfirmation) {
            Kwasant.IFrame.Display(url,
                {
                    horizontalAlign: 'right',
                    callback: calendar.refreshCalendars
                });
        } else {
            Kwasant.IFrame.DispatchUrlRequest(url);
        }
    };
    var onEventNew = function(start, end) {
        if (Kwasant.IFrame.PopupsActive()) {
            return;
        }

        if (settings.newRequiresConfirmation) {
            Kwasant.IFrame.Display(settings.getNewURL(start, end),
                {
                    horizontalAlign: 'right',
                    callback: calendar.refreshCalendars
                });
        } else {
            Kwasant.IFrame.DispatchUrlRequest(settings.getNewURL(start, end), calendar.refreshCalendars);
        }
    };
    var onEventMove = function(id, newStart, newEnd) {
        if (Kwasant.IFrame.PopupsActive()) {
            return;
        }

        if (settings.moveRequiresConfirmation) {
            Kwasant.IFrame.Display(settings.getMoveURL(id, newStart, newEnd),
                {
                    modal: true,
                    callback: calendar.refreshCalendars
                });
        } else {
            Kwasant.IFrame.DispatchUrlRequest(settings.getMoveURL(id, newStart, newEnd), calendar.refreshCalendars);
        }
    };
    var onEventDelete = function (id) {
        if (settings.deleteRequiresConfirmation) {
            Kwasant.IFrame.Display(settings.getDeleteURL(id),
                {
                    modal: true,
                    callback: calendar.refreshCalendars
                });
        } else {
            Kwasant.IFrame.DispatchUrlRequest(settings.getDeleteURL(id), calendar.refreshCalendars);
        }
    };
}(jQuery));