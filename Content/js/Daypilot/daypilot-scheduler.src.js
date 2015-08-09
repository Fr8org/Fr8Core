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
if (typeof DayPilotScheduler === 'undefined') {
    var DayPilotScheduler = DayPilot.SchedulerVisible = {};
}

(function() {

    if (typeof DayPilot.Scheduler !== 'undefined') {
        return;
    }
    
    // temp hack
    var android = (function() {
        var nua = navigator.userAgent;
        return ((nua.indexOf('Mozilla/5.0') > -1 && nua.indexOf('Android ') > -1 && nua.indexOf('AppleWebKit') > -1) && !(nua.indexOf('Chrome') > -1));
    })();

    var DayPilotScheduler = {};

    var doNothing = function() { };
    
    // register the default theme
    (function() {
        if (DayPilot.Global.defaultSchedulerCss) {
            return;
        }
        
        var sheet = DayPilot.sheet();
        
        sheet.add(".scheduler_default_selected .scheduler_default_event_inner", "background: #ddd;");
        sheet.add(".scheduler_default_main", "border: 1px solid #aaa;font-family: Tahoma, Arial, Helvetica, sans-serif; font-size: 12px;");
        sheet.add(".scheduler_default_timeheader", "cursor: default;color: #666;");
        sheet.add(".scheduler_default_message", "opacity: 0.9;filter: alpha(opacity=90);padding: 10px; color: #ffffff;background: #ffa216;");
        sheet.add(".scheduler_default_timeheadergroup,.scheduler_default_timeheadercol", "color: #666;background: #eee;");
        sheet.add(".scheduler_default_rowheader,.scheduler_default_corner", "color: #666;background: #eee;");
        sheet.add(".scheduler_default_rowheader_inner", "position: absolute;left: 0px;right: 0px;top: 0px;bottom: 0px;border-right: 1px solid #eee;padding: 2px;");
        sheet.add(".scheduler_default_timeheadergroup, .scheduler_default_timeheadercol", "text-align: center;");
        sheet.add(".scheduler_default_timeheadergroup_inner", "position: absolute;left: 0px;right: 0px;top: 0px;bottom: 0px;border-right: 1px solid #aaa;border-bottom: 1px solid #aaa;");
        sheet.add(".scheduler_default_timeheadercol_inner", "position: absolute;left: 0px;right: 0px;top: 0px;bottom: 0px;border-right: 1px solid #aaa;");
        sheet.add(".scheduler_default_divider", "background-color: #aaa;");
        sheet.add(".scheduler_default_divider_horizontal", "background-color: #aaa;");
        sheet.add(".scheduler_default_matrix_vertical_line", "background-color: #eee;");
        sheet.add(".scheduler_default_matrix_vertical_break", "background-color: #000;");
        sheet.add(".scheduler_default_matrix_horizontal_line", "background-color: #eee;");
        sheet.add(".scheduler_default_resourcedivider", "background-color: #aaa;");
        sheet.add(".scheduler_default_shadow_inner", "background-color: #666666;opacity: 0.5;filter: alpha(opacity=50);height: 100%;xborder-radius: 5px;");
        sheet.add(".scheduler_default_event", "font-size:12px;color:#666;");
        sheet.add(".scheduler_default_event_inner", "position:absolute;top:0px;left:0px;right:0px;bottom:0px;padding:5px 2px 2px 2px;overflow:hidden;border:1px solid #ccc;");
        sheet.add(".scheduler_default_event_bar", "top:0px;left:0px;right:0px;height:4px;background-color:#9dc8e8;");
        sheet.add(".scheduler_default_event_bar_inner", "position:absolute;height:4px;background-color:#1066a8;");
        sheet.add(".scheduler_default_event_inner", 'background:#fff;background: -webkit-gradient(linear, left top, left bottom, from(#ffffff), to(#eeeeee));background: -webkit-linear-gradient(top, #ffffff 0%, #eeeeee);background: -moz-linear-gradient(top, #ffffff 0%, #eeeeee);background: -ms-linear-gradient(top, #ffffff 0%, #eeeeee);background: -o-linear-gradient(top, #ffffff 0%, #eeeeee);background: linear-gradient(top, #ffffff 0%, #eeeeee);filter: progid:DXImageTransform.Microsoft.Gradient(startColorStr="#ffffff", endColorStr="#eeeeee");');
        sheet.add(".scheduler_default_event_float_inner", "padding:6px 2px 2px 8px;"); // space for arrow
        sheet.add(".scheduler_default_event_float_inner:after", 'content:"";border-color: transparent #666 transparent transparent;border-style:solid;border-width:5px;width:0;height:0;position:absolute;top:8px;left:-4px;');
        sheet.add(".scheduler_default_columnheader_inner", "font-weight: bold;");
        sheet.add(".scheduler_default_columnheader_splitter", "background-color: #666;opacity: 0.5;filter: alpha(opacity=50);");
        sheet.add(".scheduler_default_columnheader_cell_inner", "padding: 2px;");
        sheet.add(".scheduler_default_cell", "background-color: #f9f9f9;");
        sheet.add(".scheduler_default_cell.scheduler_default_cell_business", "background-color: #fff;");
        sheet.add(".scheduler_default_tree_image_no_children", "background-image: url(data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAkAAAAJCAIAAABv85FHAAAAKXRFWHRDcmVhdGlvbiBUaW1lAHDhIDMwIEkgMjAwOSAwODo0NjozMSArMDEwMClDkt4AAAAHdElNRQfZAR4HLzEyzsCJAAAACXBIWXMAAA7CAAAOwgEVKEqAAAAABGdBTUEAALGPC/xhBQAAADBJREFUeNpjrK6s5uTl/P75OybJ0NLW8h8bAIozgeSxAaA4E1A7VjmgOL31MeLxHwCeXUT0WkFMKAAAAABJRU5ErkJggg==);");
        sheet.add(".scheduler_default_tree_image_expand", "background-image: url(data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAkAAAAJCAIAAABv85FHAAAAKXRFWHRDcmVhdGlvbiBUaW1lAHDhIDMwIEkgMjAwOSAwODo0NjozMSArMDEwMClDkt4AAAAHdElNRQfZAR4HLyUoFBT0AAAACXBIWXMAAA7CAAAOwgEVKEqAAAAABGdBTUEAALGPC/xhBQAAAFJJREFUeNpjrK6s5uTl/P75OybJ0NLW8h8bAIozgeRhgJGREc4GijMBtTNgA0BxFog+uA4IA2gmUJwFog/IgUhAGBB9KPYhA3T74Jog+hjx+A8A1KRQ+AN5vcwAAAAASUVORK5CYII=);");
        sheet.add(".scheduler_default_tree_image_collapse", "background-image: url(data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAkAAAAJCAIAAABv85FHAAAAKXRFWHRDcmVhdGlvbiBUaW1lAHDhIDMwIEkgMjAwOSAwODo0NjozMSArMDEwMClDkt4AAAAHdElNRQfZAR4HLxB+p9DXAAAACXBIWXMAAA7CAAAOwgEVKEqAAAAABGdBTUEAALGPC/xhBQAAAENJREFUeNpjrK6s5uTl/P75OybJ0NLW8h8bAIozgeSxAaA4E1A7VjmgOAtEHyMjI7IE0EygOAtEH5CDqY9c+xjx+A8ANndK9WaZlP4AAAAASUVORK5CYII=);");
        sheet.add(".scheduler_default_event_move_left", 'box-sizing: border-box; padding:2px;border:1px solid #ccc;background:#fff;background: -webkit-gradient(linear, left top, left bottom, from(#ffffff), to(#eeeeee));background: -webkit-linear-gradient(top, #ffffff 0%, #eeeeee);background: -moz-linear-gradient(top, #ffffff 0%, #eeeeee);background: -ms-linear-gradient(top, #ffffff 0%, #eeeeee);background: -o-linear-gradient(top, #ffffff 0%, #eeeeee);background: linear-gradient(top, #ffffff 0%, #eeeeee);filter: progid:DXImageTransform.Microsoft.Gradient(startColorStr="#ffffff", endColorStr="#eeeeee");');
        sheet.add(".scheduler_default_event_move_right", 'box-sizing: border-box; padding:2px;border:1px solid #ccc;background:#fff;background: -webkit-gradient(linear, left top, left bottom, from(#ffffff), to(#eeeeee));background: -webkit-linear-gradient(top, #ffffff 0%, #eeeeee);background: -moz-linear-gradient(top, #ffffff 0%, #eeeeee);background: -ms-linear-gradient(top, #ffffff 0%, #eeeeee);background: -o-linear-gradient(top, #ffffff 0%, #eeeeee);background: linear-gradient(top, #ffffff 0%, #eeeeee);filter: progid:DXImageTransform.Microsoft.Gradient(startColorStr="#ffffff", endColorStr="#eeeeee");');
        sheet.commit();
        
        // define event height using css
        //sheet.add(".scheduler_default_header_height", "height:50px");
        
        DayPilot.Global.defaultSchedulerCss = true;
    })();
    
    DayPilot.Scheduler = function(id) {
        this.v = '800';

        var calendar = this;
        this.id = id; // referenced
        this.isScheduler = true;

        this.hideUntilInit = true;
        this.api = 2;

        // default values
        this.allowMultiSelect = true;
        this.allowDuplicateResources = false;
        this.autoRefreshCommand = 'refresh';
        this.autoRefreshEnabled = false;
        this.autoRefreshInterval = 60;
        this.autoRefreshMaxCount = 20;
        this.autoScroll = "Drag";
        this.borderColor = "#000000";
        this.businessBeginsHour = 9;
        this.businessEndsHour = 18;
        this.cellBackColor = "#FFFFD5";
        this.cellBackColorNonBusiness = "#FFF4BC";
        this.cellBorderColor = "#EAD098";
        this.cellDuration = 60;
        this.cellGroupBy = 'Day';
        this.cellSelectColor = "#316AC5";
        this.cellWidth = 40;
        this.cellWidthSpec = "Fixed";
        this.clientSide = true;
        this.crosshairColor = 'Gray';
        this.crosshairOpacity = 20;
        this.crosshairType = 'Header';
        this.debuggingEnabled = false;
        this.doubleClickTimeout = 300;
        this.dragOutAllowed = false;
        this.durationBarColor = 'blue';
        this.durationBarHeight = 3;
        this.durationBarVisible = true;
        this.durationBarMode = "Duration";
        this.durationBarDetached = false;
        this.days = 1;
        this.drawBlankCells = true;
        this.dynamicEventRendering = 'Progressive';
        this.dynamicEventRenderingMargin = 50;
        this.dynamicLoading = false;
        this.eventBorderColor = "#000000";
        this.eventBorderVisible = true;
        this.eventBackColor = "#FFFFFF";
        this.eventFontFamily = "Tahoma, Arial";
        this.eventFontSize = '8pt';
        this.eventFontColor = '#000000';
        this.eventHeight = 25;
        this.eventMoveMargin = 5;
        this.eventMoveToPosition = false;
        this.eventResizeMargin = 5;
        this.ganttAppendToResources = false;
        this.headerFontColor = "#000000";
        this.headerFontFamily = "Tahoma, Arial";
        this.headerFontSize = '8pt';
        this.headerHeight = 20;
        this.heightSpec = 'Auto';
        this.hourFontFamily = "Tahoma, Arial";
        this.hourFontSize = '10pt';
        this.hourNameBackColor = "#ECE9D8";
        this.hourNameBorderColor = "#ACA899";
        this.layout = 'Auto';
        this.locale = "en-us";
        this.loadingLabelText = "Loading...";
        this.loadingLabelVisible = true;
        this.loadingLabelBackColor = "orange";
        this.loadingLabelFontColor = "#ffffff";
        this.loadingLabelFontFamily = "Tahoma";
        this.loadingLabelFontSize = "10pt";
        this.messageHideAfter = 5000;
        this.moveBy = 'Full';
        this.notifyCommit = 'Immediate'; // or 'Queue'
        this.numberFormat = null;
        this.timeHeaders = [ {"groupBy": "Default"}, {"groupBy": "Cell"} ];
        this.treePreventParentUsage = false;
        this.rowHeaderWidth = 80;
        this.rowHeaderWidthAutoFit = true;
        this.rowHeaderCols = null;
        this.rowMarginBottom = 0;
        this.rowMinHeight = 0;
        this.scale = "CellDuration";
        this.scrollX = 0;
        this.scrollY = 0;
        this.shadow = "Fill";
        this.showBaseTimeHeader = true;
        this.showNonBusiness = true;
        this.showToolTip = true;
        this.snapToGrid = true;
        this.startDate = new DayPilot.Date().getDatePart();
        this.syncResourceTree = true;
        this.timeBreakColor = '#000000';
        this.treeEnabled = false;
        this.treeIndent = 20;
        this.treeImageMarginLeft = 2;
        this.treeImageMarginTop = 2;
        this.timeFormat = "Auto";
        this.useEventBoxes = 'Always';
        this.viewType = 'Resources';
        this.weekStarts = 'Auto'; // 0 = Sunday, 1 = Monday, ... 'Auto' = use .locale
        this.width = null;
        this.floatingEvents = true;
        this.floatingTimeHeaders = true;

        this.eventCorners = 'Regular'; ;

        this.separators = [];
        this.afterRender = function() { };
        this.cornerHtml = '';

        this.crosshairLastY = -1;
        this.crosshairLastX = -1;

        this.eventClickHandling = 'Enabled';
        this.eventHoverHandling = 'Bubble';
        this.eventDoubleClickHandling = 'Enabled';
        this.eventEditHandling = 'Update';
        this.eventMoveHandling = 'Update';  // TODO check how this works with api v1
        this.eventResizeHandling = 'Update'; // TODO check how this works with api v1
        this.eventRightClickHandling = 'ContextMenu';
        this.eventSelectHandling = 'Update';
        this.resourceHeaderClickHandling = 'Enabled';
        this.timeRangeDoubleClickHandling = 'Enabled';
        this.timeRangeSelectedHandling = 'Enabled';

        this.cssOnly = true;
        this.cssClassPrefix = "scheduler_default";

        // if null, ASP.NET callback will be used
        this.backendUrl = null;

        if (calendar.api === 1) {
            this.onEventMove = function() { };
            this.onEventResize = function() { };
            this.onResourceExpand = function() { };
            this.onResourceCollapse = function() { };
        }

        this.debugMessages = [];

        this.autoRefreshCount = 0;
        this.innerHeightTree = 0;
        this.rows = [];
        this.itline = [];
        //this.groupline = [];
        
        this.timeline = null;

        this.status = {};
        this.events = {};
        this.cells = {};

        // store the element references
        this.elements = {};
        this.elements.events = [];
        this.elements.bars = [];
        this.elements.cells = [];
        this.elements.linesVertical = [];
        //this.elements.linesHorizontal = [];
        this.elements.separators = [];
        this.elements.range = [];
        this.elements.breaks = [];

        this._cache = {};
        this._cache.cells = [];
        this._cache.linesVertical = [];
        this._cache.linesHorizontal = [];
        this._cache.timeHeaderGroups = [];
        this._cache.pixels = [];
        this._cache.breaks = [];
        this._cache.events = []; // processed using client-side beforeEventRender
        
        this.clientState = {};

        this.nav = {};

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


            if (typeof DayPilot.Bubble !== "undefined") {
                DayPilot.Bubble.hideActive();
            }

            if (typeof result.ClientState !== 'undefined') {
                calendar.clientState = result.ClientState;
            }

            if (result.UpdateType === "None") {
                calendar._loadingStop();

                //calendar.afterRender(result.CallBackData, true);

                if (result.Message) {
                    calendar.message(result.Message);
                }

                calendar._fireAfterRenderDetached(result.CallBackData, true);

                return;
            }

            // update config
            if (result.VsUpdate) {
                var vsph = document.createElement("input");
                vsph.type = 'hidden';
                vsph.name = calendar.id + "_vsupdate";
                vsph.id = vsph.name;
                vsph.value = result.VsUpdate;
                calendar.vsph.innerHTML = '';
                calendar.vsph.appendChild(vsph);
            }

            if (typeof result.TagFields !== 'undefined') {
                calendar.tagFields = result.TagFields;
            }

            if (typeof result.SortDirections !== 'undefined') {
                calendar.sortDirections = result.SortDirections;
            }

            if (result.UpdateType === "Full") {
                // generated
                calendar.resources = result.Resources;
                calendar.colors = result.Colors;
                calendar.palette = result.Palette;
                calendar.dirtyColors = result.DirtyColors;
                calendar.cellProperties = result.CellProperties;
                calendar.cellConfig = result.CellConfig;
                calendar.separators = result.Separators;
                calendar.timeline = result.Timeline;
                calendar.timeHeader = result.TimeHeader;
                calendar.timeHeaders = result.TimeHeaders;
                if (typeof result.RowHeaderColumns !== 'undefined') calendar.rowHeaderColumns = result.RowHeaderColumns;

                // properties
                calendar.startDate = result.StartDate ? new DayPilot.Date(result.StartDate) : calendar.startDate;
                calendar.days = result.Days ? result.Days : calendar.days;
                calendar.cellDuration = result.CellDuration ? result.CellDuration : calendar.cellDuration;
                calendar.cellGroupBy = result.CellGroupBy ? result.CellGroupBy : calendar.cellGroupBy;
                calendar.cellWidth = result.CellWidth ? result.CellWidth : calendar.cellWidth;
                // scrollX
                // scrollY
                calendar.viewType = result.ViewType ? result.ViewType : calendar.viewType;
                calendar.hourNameBackColor = result.HourNameBackColor ? result.HourNameBackColor : calendar.hourNameBackColor;
                calendar.showNonBusiness = result.ShowNonBusiness ? result.ShowNonBusiness : calendar.showNonBusiness;
                calendar.businessBeginsHour = result.BusinessBeginsHour ? result.BusinessBeginsHour : calendar.businessBeginsHour;
                calendar.businessEndsHour = result.BusinessEndsHour ? result.BusinessEndsHour : calendar.businessEndsHour;
                if (typeof result.DynamicLoading !== 'undefined') calendar.dynamicLoading = result.DynamicLoading;
                if (typeof result.TreeEnabled !== 'undefined') calendar.treeEnabled = result.TreeEnabled;
                calendar.backColor = result.BackColor ? result.BackColor : calendar.backColor;
                calendar.nonBusinessBackColor = result.NonBusinessBackColor ? result.NonBusinessBackColor : calendar.nonBusinessBackColor;
                calendar.locale = result.Locale ? result.Locale : calendar.locale;
                if (typeof result.TimeZone !== 'undefined') calendar.timeZone = result.TimeZone;
                calendar.timeFormat = result.TimeFormat ? result.TimeFormat : calendar.timeFormat;
                calendar.rowHeaderCols = result.RowHeaderCols ? result.RowHeaderCols : calendar.rowHeaderCols;
                if (typeof result.DurationBarMode !== "undefined") calendar.durationBarMode = result.DurationBarMode;
                if (typeof result.ShowBaseTimeHeader !== "undefined") calendar.showBaseTimeHeader = result.ShowBaseTimeHeader;

                calendar.cornerBackColor = result.CornerBackColor ? result.CornerBackColor : calendar.cornerBackColor;
                if (typeof result.CornerHTML !== 'undefined') { calendar.cornerHtml = result.CornerHTML; }

                calendar.hashes = result.Hashes;

                calendar._calculateCellWidth();
                calendar._prepareItline();

                calendar._loadResources();
                calendar._expandCellProperties();
            }
            var updatedRows = [];
            if (result.Action !== "Scroll") {
                updatedRows = calendar._loadEvents(result.Events);
            }

            if (result.UpdateType === 'Full') {
                calendar._drawResHeader();
                calendar._drawTimeHeader();
            }

            calendar._prepareRowTops();
            calendar._show();

            if (result.Action !== "Scroll") {
                calendar._updateRowHeaderHeights();
                calendar._updateHeaderHeight();

                if (calendar.heightSpec === 'Auto' || calendar.heightSpec === 'Max') {
                    calendar._updateHeight();
                }

                calendar._deleteCells();
                calendar._deleteEvents();
                calendar._deleteSeparators();

                calendar.multiselect.clear(true);
                calendar.multiselect.initList = result.SelectedEvents;

                calendar._drawCells();
                calendar._drawSeparators();
                calendar._drawEvents();
            }
            else {
                calendar.multiselect.clear(true);
                calendar.multiselect.initList = result.SelectedEvents;

                //calendar._updateRowsNoLoad(updatedRows, true);
                calendar._drawCells();
                calendar._loadEventsDynamic(result.Events);
            }

            if (calendar.timeRangeSelectedHandling !== "HoldForever") {
                calendar._deleteRange();
            }

            if (result.UpdateType === "Full") {
                calendar.setScroll(result.ScrollX, result.ScrollY);
                calendar._saveState();
            }
            
            calendar._updateFloats();
            
            calendar._deleteDragSource();

            calendar._loadingStop();
            
            if (result.UpdateType === 'Full' && navigator.appVersion.indexOf("MSIE 7.") !== -1) { // ugly bug, ugly fix - the time header disappears after expanding a dynamically loaded tree node
                window.setTimeout(function() {
                    calendar._drawResHeader();
                    calendar._updateHeight();
                }, 0);
            }

            calendar._startAutoRefresh();

            if (result.Message) {
                if (calendar.message) { // condition can be removed as soon as message() is implemented properly
                    calendar.message(result.Message);
                }
            }

            calendar._fireAfterRenderDetached(result.CallBackData, true);

            calendar._clearCachedValues();
        };
        
        this._deleteDragSource = function() {
            if (calendar.todo) {
                if (calendar.todo.del) {
                    var del = calendar.todo.del;
                    del.parentNode.removeChild(del);
                    calendar.todo.del = null;
                }
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

        this.scrollTo = function(date) {
            var pixels = this.getPixels(new DayPilot.Date(date)).left;
            this.setScrollX(pixels);
        };

        this.scrollToResource = function(id) {
            DayPilot.complete(function() {
                var row = calendar._findRowByResourceId(id);
                if (!row) {
                    return;
                }
                /*
                calendar.setScrollY(row.Top);
                calendar._onScroll();  // force synchronization of headers and grid
                */
                setTimeout(function() {
                    var scrollY = row.Top;
                    calendar.nav.scroll.scrollTop = scrollY;
                }, 100);

            });
        };
        
        this._findHeadersInViewPort = function() {
            
            if (!this.cssOnly) {
                return;
            }
            
            if (!this.floatingTimeHeaders) {
                return;
            }
            
            if (!this.timeHeader) {
                return;
            }
            
            //console.log("findHeadersInViewPort executing");
            
            this._deleteHeaderSections();
            
            var area = this._getDrawArea();

            var start = area.pixels.left;
            var end = area.pixels.right;
            
            var cells = [];
            
            for (var y = 0; y < this.timeHeader.length; y++) {
                for (var x = 0; x < this.timeHeader[y].length; x++) {
                    var h = this.timeHeader[y][x];
                    var left = h.left;
                    var right = h.left + h.width;
                    
                    var cell = null;
                    
                    if (left < start && start < right) {
                        cell = {};
                        cell.x = x;
                        cell.y = y;
                        cell.marginLeft = start - left;
                        cell.marginRight = 0;
                        cell.div = this._cache.timeHeader[x + "_" + y];
                        cells.push(cell);
                    }
                    if (left < end && end < right) {
                        
                        if (!cell) {
                            cell = {};
                            cell.x = x;
                            cell.y = y;
                            cell.marginLeft = 0;
                            cell.div = this._cache.timeHeader[x + "_" + y];
                            cells.push(cell);
                        }
                        cell.marginRight = right - end;
    
                        break; // end of this line
                    }
                }
            }
            
            for (var i = 0; i < cells.length; i++) {
                var cell = cells[i];
                this._createHeaderSection(cell.div, cell.marginLeft, cell.marginRight);
            }
            
        };
        
        this._updateFloats = function() {
            this._findHeadersInViewPort();
            this._findEventsInViewPort();
        };
        
        this._findEventsInViewPort = function() {
            
            if (!this.cssOnly) {
                return;
            }

            if (!this.floatingEvents) {
                return;
            }
            
            var area = this._getDrawArea();
            if (!area) {
                return;
            }
            
            var start = area.pixels.left;
            var end = area.pixels.right;
            
            this._deleteEventSections();
            
            for(var i = 0; i < calendar.elements.events.length; i++) {
                var e = calendar.elements.events[i];
                var data = e.event;
                var left = data.part.left;
                var right = data.part.left + data.part.width;
                
                if (left < start && start < right) {
                    var marginLeft = start - left;
                    this._createEventSection(e, marginLeft, 0);
                }
                /*
                if (left < end && end < right) {
                    //var pleft = left;
                    //var pwidth = end - start;
                    //console.log("e (right): " + left + " " + right);
                    
                    var marginRight = right - end;
                    this._createEventSection(e, 0, marginRight);
                }
                */
            }
        };
        
        this.elements.sections = [];
        this.elements.hsections = [];
        
        this._createHeaderSection = function(div, marginLeft, marginRight) {
            var section = document.createElement("div");
            section.setAttribute("unselectable", "on");
            section.className = this._prefixCssClass("_timeheader_float");
            section.style.position = "absolute";
            section.style.left = marginLeft + "px";
            section.style.right = marginRight + "px";
            section.style.top = "0px";
            section.style.bottom = "0px";
            //section.style.backgroundColor = "red";
            //section.style.color = "white";
            section.style.overflow = "hidden";
            
            var inner = document.createElement("div");
            inner.className = this._prefixCssClass("_timeheader_float_inner");
            inner.setAttribute("unselectable", "on");
            inner.innerHTML = div.cell.th.innerHTML;
            section.appendChild(inner);
            
            div.section = section;

            //div.appendChild(section);
            div.insertBefore(section, div.firstChild.nextSibling); // after inner
            div.firstChild.innerHTML = ''; // hide the content of inner temporarily
            
            this.elements.hsections.push(div);
        };
        
        this._deleteHeaderSections = function() {
            for (var i = 0; i < this.elements.hsections.length; i++) {
                var e = this.elements.hsections[i];
                
                // restore HTML in inner
                var data = e.cell;
                if (data && e.firstChild) { // might be deleted already
                    e.firstChild.innerHTML = data.th.innerHTML;  
                }
                
                DayPilot.de(e.section);
                e.section = null;
            }
            this.elements.hsections = [];
        };

        this._createEventSection = function(div, marginLeft, marginRight) {
            var section = document.createElement("div");
            section.setAttribute("unselectable", "on");
            section.className = this._prefixCssClass("_event_float");
            section.style.position = "absolute";
            section.style.left = marginLeft + "px";
            section.style.right = marginRight + "px";
            section.style.top = "0px";
            section.style.bottom = "0px";
            section.style.overflow = "hidden";

            var inner = document.createElement("div");
            inner.className = this._prefixCssClass("_event_float_inner");
            inner.setAttribute("unselectable", "on");
            inner.innerHTML = div.event.client.html();
            section.appendChild(inner);
            
            //section.innerHTML = div.event.text();
            
            div.section = section;
            //div.firstChild.appendChild(section);
            
            div.insertBefore(section, div.firstChild.nextSibling); // after inner
            div.firstChild.innerHTML = ''; // hide the content of inner temporarily
            
            this.elements.sections.push(div);
        };
        
        this._deleteEventSections = function() {
            for (var i = 0; i < this.elements.sections.length; i++) {
                var e = this.elements.sections[i];
                
                // restore HTML in inner
                var data = e.event;
                if (data) { // might be deleted already
                    e.firstChild.innerHTML = data.client.html();  
                }
                
                DayPilot.de(e.section);
                
                e.section = null;
            }
            this.elements.sections = [];
        };
        
        this.setScrollX = function(scrollX) {
            this.setScroll(scrollX, calendar.scrollY);
        };
        
        this.setScrollY = function(scrollY) {
            this.setScroll(calendar.scrollX, scrollY);
        };

        this.setScroll = function(scrollX, scrollY) {
            var scroll = calendar.nav.scroll;
            var maxHeight = calendar.innerHeightTree;
            var maxWidth = calendar._cellCount() * calendar.cellWidth;

            if (scroll.clientWidth + scrollX > maxWidth) {
                scrollX = maxWidth - scroll.clientWidth;
            }

            //var scrollY = result.ScrollY;
            if (scroll.clientHeight + scrollY > maxHeight) {
                scrollY = maxHeight - scroll.clientHeight;
            }

            calendar.divTimeScroll.scrollLeft = scrollX;
            calendar.divResScroll.scrollTop = scrollY;

            scroll.scrollLeft = scrollX;
            scroll.scrollTop = scrollY;
        };

        this.message = function(html, delay, foreColor, backColor) {
            if (html === null) {
                return;
            }

            var delay = delay || this.messageHideAfter || 2000;
            var foreColor = foreColor || "#ffffff";
            var backColor = backColor || "#000000";
            var opacity = 0.8;

            var top = this._getTotalHeaderHeight() + 1;
            var left = this._getTotalRowHeaderWidth() + 1;
            var right = DayPilot.sw(calendar.nav.scroll) + 1;
            var bottom = DayPilot.sh(calendar.nav.scroll) + 1;

            var div;
            
            if (!this.nav.message) {
                div = document.createElement("div");
                div.style.position = "absolute";
                //div.style.width = "100%";
                div.style.left = left + "px";
                div.style.right = right + "px";
                //div.style.height = "0px";
                //div.style.paddingLeft = left + "px";
                //div.style.paddingRight = right + "px";
                
                //div.style.opacity = 1;  
                //div.style.filter = "alpha(opacity=100)"; // enable fading in IE8
                div.style.display = 'none';
                
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

                this.nav.top.appendChild(div);
                this.nav.message = div;

            }

            var showNow = function() {
                //calendar.nav.message.style.opacity = opacity;

                var inner = calendar.nav.message.firstChild;

                if (!calendar.cssOnly) {
                    inner.style.backgroundColor = backColor;
                    inner.style.color = foreColor;
                }
                inner.innerHTML = html;

                // update the right margin (scrollbar width)
                var right = DayPilot.sw(calendar.nav.scroll) + 1;
                calendar.nav.message.style.right = right + "px";
                
                // always update the position
                var position = calendar.messageBarPosition || "Top";
                if (position === "Bottom") {
                    calendar.nav.message.style.bottom = bottom + "px";
                    calendar.nav.message.style.top = "";
                }
                else if (position === "Top") {
                    calendar.nav.message.style.bottom = "";
                    calendar.nav.message.style.top = top + "px";
                }

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

        this._hideMessage = function() {
            //var end =  function() { calendar.nav.top.removeChild(calendar.nav.message); };
            var end = function() { calendar.nav.message.style.display = 'none'; };
            DayPilot.fade(calendar.nav.message, -0.2, end);
        };

        this.message.show = function(html) {
            calendar.message(html);
        };
        
        this.message.hide = function() {
            calendar._hideMessage();
        };

        // updates the height after a resize
        this._updateHeight = function() {

            if (this.nav.scroll) { // only if the control is not disposed already

                if (this.heightSpec === 'Parent100Pct') {
                    // similar to setHeight()
                    this.height = parseInt(this.nav.top.clientHeight, 10) - (this._getTotalHeaderHeight() + 2);
                }

                // getting ready for the scrollbar
                //this.nav.scroll.style.height = '30px';

                var height = this._getScrollableHeight();
                //height = Math.max(1, height); // make sure it's not negative
                var dividerHeight = 1;
                var total = height + this._getTotalHeaderHeight() + dividerHeight;
                if (height > 0) {
                    this.nav.scroll.style.height = (height) + 'px';
                    this.scrollRes.style.height = (height) + 'px';
                }
                if (this.nav.divider) {
                    if (!total || isNaN(total) || total < 0) {
                        total = 0;
                    }
                    this.nav.divider.style.height = (total) + "px";
                }

                // required for table-based mode        
                if (this.heightSpec !== "Parent100Pct") {
                    this.nav.top.style.height = (total) + "px";
                }

                for (var i = 0; i < this.elements.separators.length; i++) {
                    this.elements.separators[i].style.height = this.innerHeightTree + 'px';
                }
                for (var i = 0; i < this.elements.linesVertical.length; i++) {
                    this.elements.linesVertical[i].style.height = this.innerHeightTree + 'px';
                }
            }

        };

        this._prepareItline = function() {
            this.endDate = (this.viewType !== 'Days') ? this.startDate.addDays(this.days) : this.startDate.addDays(1);

            this._cache.pixels = [];
            this.itline = [];
            
            var autoCellWidth = this.cellWidthSpec === "Auto";
            //var customWidth = false;
            
            /*
            var updateItlineForAutoCellWidth = function() {
                if (!autoCellWidth) {
                    return;
                }
                
                if (customWidth) {
                    calendar.debug.message("Cannot use automatic cell width, custom cell width is used in .timeline", "warning");
                    return;
                }
                
                calendar.debug.message("itline.length: " + calendar.itline.length);
                var w = Math.floor(calendar._getScrollableWidth() / calendar.itline.length); // can't use percentage because of the calculations
                calendar.debug.message("Calculated autocellwidth: " + w);
                for (var i = 0; i < calendar.itline.length; i++) {
                    var cell = calendar.itline[i];
                    cell.width = w;
                }
            };
            */
            
            var updateCellWidthForAuto = function() {
                if (!autoCellWidth) {
                    return;
                }
                var count = 0;
                if (calendar.timeHeader) {
                    if (calendar.timeline) {
                        count = calendar.timeline.length;
                    }
                    else {
                        var row = calendar.timeHeader[calendar.timeHeader.length - 1];
                        count = row.length;
                    }
                }
                else {
                    if (calendar.scale === "Manual") {
                        count = calendar.timeline.length;
                    }
                    else {
                        calendar._generateTimeline();  // hack
                        count = calendar.itline.length;
                        calendar.itline = [];
                    }
                }
                var width = calendar._getScrollableWidth();
                if (count > 0 && width > 0) {
                    //alert("getScrollableWidth: " + width + "/" + count);
                    calendar.cellWidth = width / count;
                }
                calendar.debug.message("updated cellWidth: " + calendar.cellWidth);
            };
            
            
            updateCellWidthForAuto();
            
            //calendar.debug.message("timeheader: " + this.timeHeader);
            
            // set on the server, copy from there
            if (this._serverBased()) {  // timeline supplied from the server
                if (this.timeline) {  // TODO dissolve 
                    calendar.debug.message("timeline received from server");
                    this.itline = [];
                    var lastEnd = null;
                    var left = 0;
                    for (var i = 0; i < this.timeline.length; i++) {
                        
                        /*
                        if (src.width) {
                            customWidth = true;
                        }*/
                        
                        var src = this.timeline[i];
                        var cell = {};
                        cell.start = new DayPilot.Date(src.start);
                        cell.end =  src.end ? new DayPilot.Date(src.end) : cell.start.addMinutes(this.cellDuration);

                        if (!src.width) {
                            var right = Math.floor(left + this.cellWidth);
                            var width = right - Math.floor(left);

                            cell.left = Math.floor(left);
                            cell.width = width;
                            left += this.cellWidth;
                        }
                        else {
                            cell.left = src.left || left; // left is optional TODO remove original syntax
                            cell.width = src.width || this.cellWidth; // width is optional TODO remove original syntax
                            left += cell.width;
                        }

                        /*
                        if (autoCellWidth) {
                            cell.left = Math.floor(left);
                            cell.width = Math.floor(this.cellWidth); // width is optional
                        }
                        else {
                            cell.left = src.left || left; // left is optional
                            cell.width = src.width || this.cellWidth; // width is optional
                        }
                        */
                        

                        cell.breakBefore = lastEnd && lastEnd.ticks !== cell.start.ticks;
                        lastEnd = cell.end;
                        
                        this.itline.push(cell);
                    }
                }

                if (autoCellWidth) {
                    this._updateHeaderGroupDimensions();
                }
                /*
                else {  
                    // backwards compatibility, read it from the time header
                    // TODO re-generate the timeHeader, this is a client-side update
                    var lastEndTicks = this.startDate.getTime();
                    var row = this.timeHeader[this.timeHeader.length - 1];
                    for (var i = 0; i < row.length; i++) {
                        var h = row[i];

                        var timeCell = {};
                        timeCell.start = new DayPilot.Date(h.start);
                        if (h.end) {
                            timeCell.end = new DayPilot.Date(h.end);
                        }
                        else {
                            timeCell.end = timeCell.start.addMinutes(this.cellDuration);
                        }
                        timeCell.width = h.width;
                        timeCell.left = h.left;
                        timeCell.breakBefore = timeCell.start.ticks !== lastEndTicks;

                        this.itline.push(timeCell);

                        lastEndTicks = timeCell.end.ticks;
                    }
                }
                */
                //updateItlineForAutoCellWidth();
            }
            else {
                this.timeHeader = [];

                if (this.scale === "Manual") {
                    this.itline = [];
                    var left = 0;
                    var lastEnd = null;
                    for (var i = 0; i < this.timeline.length; i++) {
                        var src = this.timeline[i];

                        /*
                        if (src.width) {
                            customWidth = true;
                        }
                        */

                        var cell = {};
                        cell.start = new DayPilot.Date(src.start);
                        cell.end =  src.end ? new DayPilot.Date(src.end) : cell.start.addMinutes(this.cellDuration);

                        var right = Math.floor(left + this.cellWidth);
                        var width = right - Math.floor(left);

                        cell.left = Math.floor(left);
                        cell.width = width;
                        left += this.cellWidth;
                        
                        // TODO custom width
                        
                        //cell.left = Math.floor(left);
                        //cell.width = Math.floor(src.width || this.cellWidth);
                        //cell.breakBefore = src.breakBefore;
                        
                        cell.breakBefore = lastEnd && lastEnd.ticks !== cell.start.ticks;
                        lastEnd = cell.end;

                        this.itline.push(cell);
                        
                        //left += cell.width;
                    }
                }
                else {
                    this._generateTimeline();
                }

                //updateItlineForAutoCellWidth();
                this._prepareHeaderGroups();
            }
        };
        
        this._generateTimeline = function() {

            var start = this.startDate;
            var end = this._addScaleSize(start); //
            var breakBefore = false;

            // groups
            var timeHeaders = this.timeHeaders;

            var left = 0;
            //var hrow = [];
            while (end.ticks <= this.endDate.ticks && end.ticks > start.ticks) {
                if (this._includeCell(start, end)) {
                    
                    var right = Math.floor(left + this.cellWidth);
                    var width = right - Math.floor(left);
                    
                    var timeCell = {};
                    timeCell.start = start;
                    timeCell.end = end;
                    timeCell.left = Math.floor(left);
                    timeCell.width = width;
                    timeCell.breakBefore = breakBefore;

                    this.itline.push(timeCell);
                    
                    //calendar.debug.message("using width: " + this.cellWidth);
                    
                    left += this.cellWidth;

                    /*
                    var i = this.itline.length - 1;

                    var h = {};
                    h.start = start;
                    h.end = end;
                    h.innerHTML = this._getCellName(start);
                    h.left = i * this.cellWidth;
                    h.width = this.cellWidth;

                    if (typeof this.onBeforeTimeHeaderRender === 'function') {
                        var cell = {};
                        cell.start = start;
                        cell.end = end;
                        cell.html = h.innerHTML;
                        cell.tooltip = h.innerHTML;
                        cell.backColor = null;
                        cell.level = timeHeaders.length;

                        var args = {};
                        args.header = cell;

                        this.onBeforeTimeHeaderRender(args);

                        h.innerHTML = cell.html;
                        h.backColor = cell.backColor;
                        h.toolTip = cell.tooltip;
                    }
                    */

                    //hrow.push(h);

                    breakBefore = false;
                }
                else {
                    breakBefore = true;
                }

                start = end;
                end = this._addScaleSize(start);
            }
        };

        this._updateHeaderGroupDimensions = function() {
            calendar.debug.message("updateHeaderGroupDimensions");
            if (!this.timeHeader) {
                return;
            }
            for (var y = 0; y < this.timeHeader.length; y++) {
                var row = this.timeHeader[y];
                for (var x = 0; x < row.length; x++) {
                    var h = row[x];

                    h.left = this.getPixels(new DayPilot.Date(h.start)).left;
                    var right = this.getPixels(new DayPilot.Date(h.end)).left;
                    var width = right - h.left;
                    h.width = width;
                    //calendar.debug.message("cell: " + h.start + "-" + h.end + " : left: " + h.left + " width:" + h.width);
                }
            }
        };
        
        this._prepareHeaderGroups = function() {
            var timeHeaders = this.timeHeaders;
            if (!timeHeaders) {
                timeHeaders = [
                    {"groupBy": this.cellGroupBy},
                    {"groupBy": "Cell"}
                ];
            }
            //var timeHeaders = this.timeHeaders;
            for (var i = 0; i < timeHeaders.length; i++) {
                var groupBy = timeHeaders[i].groupBy;
                var format = timeHeaders[i].format;
                
                if (groupBy === "Default") {
                    groupBy = this.cellGroupBy;
                }

                var start = this.startDate;
                var line = [];

                //var cell = {};
                var start = this.startDate;
                
                while (start.ticks < this.endDate.ticks) {
                    var h = {};
                    h.start = start;
                    h.end = this._addGroupSize(h.start, groupBy);
                    
                    if (h.start.ticks === h.end.ticks) {
                        break;
                    }
                    h.left = this.getPixels(h.start).left;
                    var right = this.getPixels(h.end).left;
                    var width = right - h.left;
                    h.width = width;
                    
                    h.colspan = Math.ceil(width / (1.0 * this.cellWidth));
                    if (format) {
                        h.innerHTML = h.start.toString(format, resolved.locale());
                    }
                    else {
                        h.innerHTML = this._getGroupName(h, groupBy);
                    }

                    if (width > 0) {

                        if (typeof this.onBeforeTimeHeaderRender === 'function') {
                            var cell = {};
                            cell.start = h.start;
                            cell.end = h.end;
                            cell.html = h.innerHTML;
                            cell.toolTip = h.innerHTML;
                            //cell.color = null;
                            cell.backColor = null;
                            if (!this.cssOnly) {
                                cell.backColor = this.hourNameBackColor;
                            }
                            cell.level = this.timeHeader.length; 

                            var args = {};
                            args.header = cell;

                            this.onBeforeTimeHeaderRender(args);

                            h.innerHTML = cell.html;
                            h.backColor = cell.backColor;
                            h.toolTip = cell.toolTip;
                        }

                        line.push(h);
                    }
                    start = h.end;
                }
                this.timeHeader.push(line);
            }
        };

        /*
        // internal
        // must be called after prepareTimeline
        this._prepareGroupline = function() {
            this.groupline = [];

            var endDate = this.endDate;

            var cell = {};
            var start = this.startDate;
            while (start.ticks < endDate.ticks) {
                var cell = {};
                cell.start = start;
                cell.end = this._addGroupSize(cell.start);
                cell.left = this.getPixels(cell.start).left;
                cell.width = this.getPixels(cell.end).left - cell.left;
                // DEBUG
                //cell.innerHTML = start.toStringSortable();

                if (cell.width > 0) {
                    this.groupline.push(cell);
                }
                start = cell.end;
            }
        };
        */

        this._includeCell = function(start, end) {

            if (typeof this.onIncludeTimeCell === 'function') {
                var cell = {};
                cell.start = start;
                cell.end = end;
                cell.visible = true;
                
                var args = {};
                args.cell = cell;
                
                this.onIncludeTimeCell(args);
                
                return cell.visible;
            }

            if (this.showNonBusiness) {
                return true;
            }

            if (start.d.getUTCDay() === 0) { // Sunday
                return false;
            }
            if (start.d.getUTCDay() === 6) { // Saturday
                return false;
            }

            var cellDuration = (end.getTime() - start.getTime()) / (1000 * 60);  // minutes
            if (cellDuration < 60 * 24) {  // cell is smaller than one day
                var startHour = start.d.getUTCHours();
                startHour += start.d.getUTCMinutes() / 60.0;
                startHour += start.d.getUTCSeconds() / 3600.0;
                startHour += start.d.getUTCMilliseconds() / 3600000.0;

                /*
                var endHour = end.d.getUTCHours();
                endHour += end.d.getUTCMinutes()/60;
                endHour += end.d.getUTCSeconds()/3600;
                endHour += end.d.getUTCMilliseconds()/3600000;
                */

                if (startHour < this.businessBeginsHour) {
                    return false;
                }

                if (this.businessEndsHour >= 24) {
                    return true;
                }
                if (startHour >= this.businessEndsHour) {
                    return false;
                }
            }
            return true;
        };


        this.getPixels = function(date) {
            var ticks = date.ticks;

            var cache = this._cache.pixels[ticks];
            if (cache) {
                return cache;
            }
            
            var previous = null;
            var previousEndTicks = 221876841600000;  // December 31, 9000

            if (this.itline.length === 0 || ticks < this.itline[0].start.ticks) {
                var result = {};
                result.cut = false;
                result.left = 0;
                result.boxLeft = result.left;
                result.boxRight = result.left;
                result.i = null; // not in range
                return result;
            }

            for (var i = 0; i < this.itline.length; i++) {
                var found = false;
                var cell = this.itline[i];

                var cellStartTicks = cell.start.ticks;
                var cellEndTicks = cell.end.ticks;

                if (cellStartTicks < ticks && ticks < cellEndTicks) {  // inside
                    var offset = ticks - cellStartTicks;

                    var result = {};
                    result.cut = false;
                    result.left = cell.left + this._ticksToPixels(cell, offset);
                    result.boxLeft = cell.left;
                    result.boxRight = cell.left + cell.width;
                    result.i = i;
                    break;
                }
                else if (cellStartTicks === ticks) {  // start
                    var result = {};
                    result.cut = false;
                    result.left = cell.left;
                    result.boxLeft = result.left;
                    result.boxRight = result.left + cell.width;
                    result.i = i;
                    break;
                }
                else if (cellEndTicks === ticks) {  // end
                    var result = {};
                    result.cut = false;
                    result.left = cell.left + cell.width;
                    result.boxLeft = result.left;
                    result.boxRight = result.left;
                    result.i = i + 1;
                    break;
                }
                else if (ticks < cellStartTicks && ticks > previousEndTicks) {  // hidden
                    var result = {};
                    result.cut = true;
                    result.left = cell.left;
                    result.boxLeft = result.left;
                    result.boxRight = result.left;
                    result.i = i;
                    break;
                }

                previousEndTicks = cellEndTicks;
            }

            if (!result) {
                var last = this.itline[this.itline.length - 1];
                
                var result = {};
                result.cut = true;
                result.left = last.left + last.width;
                result.boxLeft = result.left;
                result.boxRight = result.left;
                result.i = null; // not in range
                //this.log.c5 = this.log.c5 ? this.log.c5+1 : 1;
            }

            this._cache.pixels[ticks] = result;
            return result;
        };

        // left - pixel offset from start
        // precise - true: calculates exact date from pixels, false: the it's the cell start
        // isEnd - returns the end of the previous cell
        // 
        // isEnd and precise can't be combined
        this.getDate = function(left, precise, isEnd) {
            //var x = Math.floor(left / this.cellWidth);
            var position = this._getItlineCellFromPixels(left, isEnd);
            
            if (!position) {
                return null;
            }
            
            var x = position.x;
            
            var itc = this.itline[x];

            if (!itc) {
                return null;
            }

            //var start = (isEnd && x > 0) ? this.itline[x - 1].end : this.itline[x].start;
            var start = (isEnd && !precise) ? itc.end : itc.start;

            if (!precise) {
                return start;
            }
            else {
                return start.addTime(this._pixelsToTicks(position.cell, position.offset));
            }

        };
        
        this._getItlineCellFromPixels = function(pixels, isEnd) {
            var pos = 0;
            var previous = 0;
            for (var i = 0; i < this.itline.length; i++) {
                var cell = this.itline[i];
                var width = cell.width || this.cellWidth;
                pos += width;
                
                if ((pixels < pos) || (isEnd && pixels === pos)) {
                    var result = {};
                    result.x = i;
                    result.offset = pixels - previous;
                    result.cell = cell;
                    return result;
                }
                
                previous = pos;
            }
            var cell = this.itline[this.itline.length - 1];
            
            var result = {};
            result.x = this.itline.length - 1;
            result.offset = cell.width || this.cellWidth;
            result.cell = cell;
            return result;
        };

        this._getItlineCellFromTime = function(time) {
            var pos = new DayPilot.Date(time);
            //var previous = 0;
            for (var i = 0; i < this.itline.length; i++) {
                var cell = this.itline[i];
                
                if (cell.start.ticks <= pos.ticks && pos.ticks < cell.end.ticks) {
                    var result = {};
                    result.hidden = false;
                    result.current = cell;
                    return result;
                }
                if (pos.ticks < cell.start.ticks)   // it's a hidden cell
                {
                    var result = {};
                    result.hidden = true;
                    result.previous = i > 0 ? this.itline[i - 1] : null;
                    result.current = null;
                    result.next = this.itline[i];
                    return result;
                }
                
                //pos = pos.addMinutes(1);
            }
            var result = {};
            result.past = true;
            result.previous = this.itline[this.itline.length - 1];
            
            return result;
        };

        this._ticksToPixels = function(cell, ticks) { // DEBUG check that it's not used improperly (timeline)
            var width = cell.width || this.cellWidth;
            var duration = cell.end.ticks - cell.start.ticks;
            return Math.floor((width * ticks) / (duration));
        };

        this._pixelsToTicks = function(cell, pixels) {
            var duration = cell.end.ticks - cell.start.ticks;
            var width = cell.width || this.cellWidth;
            return Math.floor(pixels / width * duration );
        };
        
        this._onEventClick = function(ev) {
            if (touch.start) {
                return;
            }
            moving = {}; // clear
            calendar._eventClickDispatch(this, ev);  
        };

        this.eventClickPostBack = function(e, data) {
            this._postBack2('EventClick', e, data);
        };
        this.eventClickCallBack = function(e, data) {
            this._callBack2('EventClick', e, data);
        };

        this._eventClickDispatch = function(div, ev) {
            var e = div.event;

            var ev = ev || window.event;

            if (typeof (DayPilotBubble) !== 'undefined') {
                DayPilotBubble.hideActive();
            }

            //if (calendar.eventDoubleClickHandling === 'Disabled') {
            if (!e.client.doubleClickEnabled()) {
                calendar._eventClickSingle(div, ev.ctrlKey);
                return;
            }

            if (!calendar.timeouts.click) {
                calendar.timeouts.click = [];
            }

            var eventClickDelayed = function(div, ctrlKey) {
                return function() {
                    calendar._eventClickSingle(div, ctrlKey);
                };
            };

            calendar.timeouts.click.push(window.setTimeout(eventClickDelayed(div, ev.ctrlKey), calendar.doubleClickTimeout));

        };

        this._eventClickSingle = function(div, ctrlKey) {
            var e = div.event;
            //var data = div.data;
            
            if (!e.client.clickEnabled()) {
                return;
            }
            
            if (calendar._api2()) {
                
                var args = {};
                args.e = e;
                args.div = div;
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
                    case 'Edit':
                        calendar._divEdit(div);
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
                    case 'Edit':
                        calendar._divEdit(div);
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

        };

        // obsolete
        this.setHScrollPosition = function(pixels) {
            this.nav.scroll.scrollLeft = pixels;
        };

        this.getScrollX = function() {
            return this.nav.scroll.scrollLeft;
        };

        // obsolete
        this.getHScrollPosition = this.getScrollX;

        this.getScrollY = function() {
            return this.nav.scroll.scrollTop;
        };

        this._eventSelect = function(div, e, ctrlKey) {
            calendar._eventSelectDispatch(div, e, ctrlKey);
        };

        this.eventSelectPostBack = function(e, change, data) {
            var params = {};
            params.e = e;
            params.change = change;
            this._postBack2('EventSelect', params, data);
        };
        this.eventSelectCallBack = function(e, change, data) {
            var params = {};
            params.e = e;
            params.change = change;
            this._callBack2('EventSelect', params, data);
        };

        this._eventSelectDispatch = function(div, e, ctrlKey) {
            //var e = this.selectedEvent();

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


        };

        this.eventRightClickPostBack = function(e, data) {
            this._postBack2('EventRightClick', e, data);
        };
        this.eventRightClickCallBack = function(e, data) {
            this._callBack2('EventRightClick', e, data);
        };

        this._eventRightClickDispatch = function(ev) {
            var e = this.event;

            ev = ev || window.event;
            ev.cancelBubble = true;

            if (!this.event.client.rightClickEnabled()) {
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


            return false;
        };

        this.eventDoubleClickPostBack = function(e, data) {
            this._postBack2('EventDoubleClick', e, data);
        };
        this.eventDoubleClickCallBack = function(e, data) {
            this._callBack2('EventDoubleClick', e, data);
        };

        this._eventDoubleClickDispatch = function(ev) {

            if (typeof (DayPilotBubble) !== 'undefined') {
                DayPilotBubble.hideActive();
            }


            if (calendar.timeouts.click) {
                for (var toid in calendar.timeouts.click) {
                    window.clearTimeout(calendar.timeouts.click[toid]);
                }
                calendar.timeouts.click = null;
            }

            var ev = ev || window.event;
            var e = this.event;

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
                        calendar._divEdit(this);
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
                    case 'Edit':
                        calendar._divEdit(this);
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

        };

        this.eventResizePostBack = function(e, newStart, newEnd, data) {
            this._invokeEventResize("PostBack", e, newStart, newEnd, data);

        };
        this.eventResizeCallBack = function(e, newStart, newEnd, data) {
            this._invokeEventResize("CallBack", e, newStart, newEnd, data);
        };

        this._invokeEventResize = function(type, e, newStart, newEnd, data) {
            var params = {};
            params.e = e;
            params.newStart = newStart;
            params.newEnd = newEnd;

            this._invokeEvent(type, "EventResize", params, data);
        };


        this._eventResizeDispatch = function(e, newStart, newEnd) {

            if (this.eventResizeHandling === 'Disabled') {
                return;
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
                    case 'Update':
                        e.start(newStart);
                        e.end(newEnd);
                        calendar.events.update(e);
                        break;
                }                
            }
          
        };

        this.eventMovePostBack = function(e, newStart, newEnd, newResource, data, line) {
            this._invokeEventMove("PostBack", e, newStart, newEnd, newResource, data, line);
        };

        this.eventMoveCallBack = function(e, newStart, newEnd, newResource, data, line) {
            this._invokeEventMove("CallBack", e, newStart, newEnd, newResource, data, line);
        };

        this._invokeEventMove = function(type, e, newStart, newEnd, newResource, data, line) {
            var params = {};
            params.e = e;
            params.newStart = newStart;
            params.newEnd = newEnd;
            params.newResource = newResource;
            params.position = line;

            this._invokeEvent(type, "EventMove", params, data);
        };

        this._invokeEvent = function(type, action, params, data) {

            if (type === 'PostBack') {
                calendar._postBack2(action, params, data);
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

        this.eventMoveNotify = function(e, newStart, newEnd, newResource, data, line) {

            var old = new DayPilot.Event(e.copy(), this);

            var rows = calendar.events._removeFromRows(e.data);

            e.start(newStart);
            e.end(newEnd);
            e.resource(newResource);
            e.commit();

            rows = rows.concat(calendar.events._addToRows(e.data));
            calendar._loadRows(rows);

            calendar._updateRowHeights();

            calendar._updateRowsNoLoad(rows);

            this._invokeEventMove("Notify", old, newStart, newEnd, newResource, data, line);

        };

        this.eventResizeNotify = function(e, newStart, newEnd, data) {

            var old = new DayPilot.Event(e.copy(), this);

            var rows = calendar.events._removeFromRows(e.data);

            e.start(newStart);
            e.end(newEnd);
            e.commit();

            rows = rows.concat(calendar.events._addToRows(e.data));
            
            calendar._loadRows(rows);

            calendar._updateRowHeights();

            calendar._updateRowsNoLoad(rows);

            this._invokeEventResize("Notify", old, newStart, newEnd, data);

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
            // not implemented
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
                        m.clear();
                        if (count > 1) {
                            m.add(div.event, true);
                        }
                    }
                }
                else { // clear all
                    m.clear();
                }
            }
            else {
                if (calendar.allowMultiSelect) {
                    if (ctrl) {
                        m.add(div.event, true);
                    }
                    else {
                        m.clear();
                        m.add(div.event, true);
                    }
                }
                else {
                    m.clear();
                    m.add(div.event, true);
                }
            }
            //m.redraw();
            m._update(div);
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

            if (dontRedraw) {
                return;
            }
            m.redraw();
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

        // not used
        this.multiselect._updateEvent = function(ev) {
            var m = calendar.multiselect;
            var div = null;
            for (var i = 0; i < calendar.elements.events.length; i++) {
                if (m.isSelected(calendar.elements.events[i].event)) {
                    div = calendar.elements.events[i];
                    break;
                }
            }
            m._update(div);
        };

        // used for faster redraw
        this.multiselect._update = function(div) {
            if (!div) {
                return;
            }

            var m = calendar.multiselect;

            if (m.isSelected(div.event)) {
                m._divSelect(div);
            }
            else {
                m._divDeselect(div);
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
            return div;
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
            DayPilot.Util.removeClass(div, cn);

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
                if (typeof ei.id === 'function') {
                    if (ei.id() !== null && e.id() !== null && ei.id() === e.id()) {
                        return true;
                    }
                    if (ei.id() === null && e.id() === null && ei.recurrentMasterId() === e.recurrentMasterId() && e.start().toStringSortable() === ei.start().toStringSortable()) {
                        return true;
                    }
                }
                else {
                    if (ei.id !== null && e.id() !== null && ei.id === e.id()) {
                        return true;
                    }
                    if (ei.id === null && e.id() === null && ei.recurrentMasterId === e.recurrentMasterId() && e.start().toStringSortable() === ei.start) {
                        return true;
                    }
                }

            }

            return false;
        };

        // full update
        this.update = function() {
            var full = true;

            if (full) {
                if (!this._serverBased()) {
                    calendar.timeHeader = null;
                    calendar.cellProperties = {};
                }
                calendar._calculateCellWidth();
                calendar._prepareItline();
                calendar._loadResources();
            }

            this._loadEvents();

            if (full) {
                calendar._drawResHeader();
                calendar._drawTimeHeader();
                calendar._updateCorner();
            }

            calendar._prepareRowTops();
            calendar._updateRowHeaderHeights();
            calendar._updateHeaderHeight();

            if (calendar.heightSpec === 'Auto' || calendar.heightSpec === 'Max') {
                calendar._updateHeight();
            }

            this._deleteEvents();
            this._deleteSeparators();
            this._deleteCells();

            this._clearCachedValues();

            this._drawCells();
            this._drawSeparators();
            this._drawEvents();

            this._updateFloats();
        };

/*
        // full update
        this.updateDebug = function() {
            var full = true;

            if (full) {
                calendar._calculateCellWidth();
                calendar._prepareItline();

                calendar._loadResources();
            }

            this._loadEvents();

            if (full) {
                calendar._drawResHeader();
                calendar._drawTimeHeader();
            }

            calendar._prepareRowTops();
            calendar._updateRowHeaderHeights();
            calendar._updateHeaderHeight();

            if (calendar.heightSpec === 'Auto' || calendar.heightSpec === 'Max') {
                calendar._updateHeight();
            }

            this._deleteEvents();
            this._deleteSeparators();
            this._deleteCells();

            this._drawCells();
            this._drawSeparators();
            this._drawEvents();
        };
*/
        this._updateRowsNoLoad = function(rows, appendOnlyIfPossible, finishedCallBack) {
            //var start, end;

            rows = DayPilot.ua(rows);

            if (this.rowsDirty) {
                this._updateRowHeaderHeights();
                this._prepareRowTops();

                this._deleteCells();

                this._deleteSeparators();

                for (var i = 0; i < rows.length; i++) {
                    var ri = rows[i];
                    this._deleteEventsInRow(ri);
                }

                for (var i = 0; i < rows.length; i++) {
                    var ri = rows[i];
                    this._drawEventsInRow(ri);
                }

                this._drawCells();

                this._drawSeparators();
                this._updateEventTops();

            }
            else {
                var batch = true;
                
                if (batch) {
                    var doRow = function(i) {
                        if (i >= rows.length) {
                            return;
                        }
                        var ri = rows[i];
                        if (!appendOnlyIfPossible) {
                            calendar._deleteEventsInRow(ri);
                        }
                        calendar._drawEventsInRow(ri);
                        if (i + 1 < rows.length) {
                            setTimeout(function() { doRow(i+1); }, 10);
                        }
                        else {
                            calendar._findEventsInViewPort();
                            if (finishedCallBack) {
                                finishedCallBack();
                            }
                        }
                    };
                    doRow(0);
                }
                else {
                    for (var i = 0; i < rows.length; i++) {
                        var ri = rows[i];
                        if (!appendOnlyIfPossible) {
                            this._deleteEventsInRow(ri);
                        }
                        this._drawEventsInRow(ri);
                    }

                }
            }
            
            calendar._findEventsInViewPort();

            if (finishedCallBack) {
                finishedCallBack();
            }

            this._clearCachedValues();
            
        };

        this._eventMoveDispatch = function(e, newStart, newEnd, newResource, external, ev, line) {

            if (calendar.eventMoveHandling === 'Disabled') {
                return;
            }

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
                args.position = line;
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
                        calendar.eventMovePostBack(e, newStart, newEnd, newResource, null, line);
                        break;
                    case 'CallBack':
                        calendar.eventMoveCallBack(e, newStart, newEnd, newResource, null, line);
                        break;
                    case 'Notify':
                        calendar.eventMoveNotify(e, newStart, newEnd, newResource, null, line);
                        break;
                    case 'Update':
                        e.start(newStart);
                        e.end(newEnd);
                        e.resource(newResource);
                        if (external) {
                            e.commit();
                            calendar.events.add(e);
                        }
                        else {
                            calendar.events.update(e);
                        }
                        calendar._deleteDragSource();
                        break;
                }
                
                if (typeof calendar.onEventMoved === 'function') {
                    calendar.onEventMoved(args);
                }
            }
            else {
                switch (calendar.eventMoveHandling) {
                    case 'PostBack':
                        calendar.eventMovePostBack(e, newStart, newEnd, newResource, null, line);
                        break;
                    case 'CallBack':
                        calendar.eventMoveCallBack(e, newStart, newEnd, newResource, null, line);
                        break;
                    case 'JavaScript':
                        calendar.onEventMove(e, newStart, newEnd, newResource, external, ev ? ev.ctrlKey : false, ev ? ev.shiftKey : false, line);
                        break;
                    case 'Notify':
                        calendar.eventMoveNotify(e, newStart, newEnd, newResource, null, line);
                        break;
                    case 'Update':
                        e.start(newStart);
                        e.end(newEnd);
                        e.resource(newResource);
                        calendar.events.update(e);
                        break;
                }
            }
        };

/*
        // obsolete        
        this.eventBubbleCallBack = function(e, bubble) {
            var guid = this.recordBubbleCall(bubble);

            var params = {};
            params.e = e;
            params.guid = guid;

            this._callBack2("EventBubble", params);
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

        this.eventMenuClickPostBack = function(e, command, data) {
            var params = {};
            params.e = e;
            params.command = command;

            this._postBack2('EventMenuClick', params, data);
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

        this.timeRangeSelectedPostBack = function(start, end, resource, data) {
            var range = {};
            range.start = start;
            range.end = end;
            range.resource = resource;

            this._postBack2('TimeRangeSelected', range, data);
        };
        this.timeRangeSelectedCallBack = function(start, end, resource, data) {

            var range = {};
            range.start = start;
            range.end = end;
            range.resource = resource;

            this._callBack2('TimeRangeSelected', range, data);
        };

        this._timeRangeSelectedDispatch = function(start, end, resource) {
            
            if (calendar.timeRangeSelectedHandling === 'Disabled') {
                return;
            }
            
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
                        calendar.timeRangeSelectedPostBack(start, end, resource);
                        break;
                    case 'CallBack':
                        calendar.timeRangeSelectedCallBack(start, end, resource);
                        break;
                    case 'JavaScript':
                        calendar.onTimeRangeSelected(start, end, resource);
                        break;
                    case 'Hold':
                        break;
                }
            }
        };

        this.timeRangeDoubleClickPostBack = function(start, end, resource, data) {
            var range = {};
            range.start = start;
            range.end = end;
            range.resource = resource;

            this._postBack2('TimeRangeDoubleClick', range, data);
        };
        this.timeRangeDoubleClickCallBack = function(start, end, resource, data) {

            var range = {};
            range.start = start;
            range.end = end;
            range.resource = resource;

            this._callBack2('TimeRangeDoubleClick', range, data);
        };


        this._timeRangeDoubleClickDispatch = function(start, end, resource) {
            
            if (calendar._api2()) {

                
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
                        calendar.timeRangeDoubleClickPostBack(start, end, resource);
                        break;
                    case 'CallBack':
                        calendar.timeRangeDoubleClickCallBack(start, end, resource);
                        break;
                    case 'JavaScript':
                        calendar.onTimeRangeDoubleClick(start, end, resource);
                        break;
                }
                
            }

        };

        this.timeRangeMenuClickPostBack = function(e, command, data) {
            var params = {};
            params.selection = e;
            params.command = command;

            this._postBack2("TimeRangeMenuClick", params, data);
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

        this.resourceHeaderMenuClickPostBack = function(e, command, data) {
            var params = {};
            params.resource = e;
            params.command = command;

            this._postBack2("ResourceHeaderMenuClick", params, data);
        };
        this.resourceHeaderMenuClickCallBack = function(e, command, data) {
            var params = {};
            params.resource = e;
            params.command = command;

            this._callBack2("ResourceHeaderMenuClick", params, data);
        };

        this._resourceHeaderMenuClick = function(command, e, handling) {
            switch (handling) {
                case 'PostBack':
                    calendar.resourceHeaderMenuClickPostBack(e, command);
                    break;
                case 'CallBack':
                    calendar.resourceHeaderMenuClickCallBack(e, command);
                    break;
            }
        };

        this.resourceHeaderClickPostBack = function(e, data) {
            var params = {};
            params.resource = e;

            this._postBack2("ResourceHeaderClick", params, data);
        };
        this.resourceHeaderClickCallBack = function(e, data) {
            var params = {};
            params.resource = e;

            this._callBack2("ResourceHeaderClick", params, data);
        };

        this._resourceHeaderClickDispatch = function(e) {
            
            if (calendar._api2()) {
                
                var args = {};
                args.resource = e;
                args.preventDefault = function() {
                    this.preventDefault.value = true;
                };

                if (typeof calendar.onResourceHeaderClick === 'function') {
                    calendar.onResourceHeaderClick(args);
                    if (args.preventDefault.value) {
                        return;
                    }
                }
                
                switch (this.resourceHeaderClickHandling) {
                    case 'PostBack':
                        calendar.resourceHeaderClickPostBack(e);
                        break;
                    case 'CallBack':
                        calendar.resourceHeaderClickCallBack(e);
                        break;
                }
                
                if (typeof calendar.onResourceHeaderClicked === 'function') {
                    calendar.onResourceHeaderClicked(args);
                }                

            }
            else {
                switch (this.resourceHeaderClickHandling) {
                    case 'PostBack':
                        calendar.resourceHeaderClickPostBack(e);
                        break;
                    case 'CallBack':
                        calendar.resourceHeaderClickCallBack(e);
                        break;
                    case 'JavaScript':
                        calendar.onResourceHeaderClick(e);
                        break;
                }
                
            }
            
            
        };
        //

        this.timeHeaderClickPostBack = function(e, data) {
            var params = {};
            params.header = e;

            this._postBack2("TimeHeaderClick", params, data);
        };

        this.timeHeaderClickCallBack = function(e, data) {
            var params = {};
            params.header = e;

            this._callBack2("TimeHeaderClick", params, data);
        };

        this._timeHeaderClickDispatch = function(e) {
            if (calendar._api2()) {
                
                var args = {};
                args.header = e;
                /*
                 * start
                 * end
                 * level
                 * 
                 */
                args.preventDefault = function() {
                    this.preventDefault.value = true;
                };

                if (typeof calendar.onTimeHeaderClick === 'function') {
                    calendar.onTimeHeaderClick(args);
                    if (args.preventDefault.value) {
                        return;
                    }
                }
                
                switch (this.timeHeaderClickHandling) {
                    case 'PostBack':
                        calendar.timeHeaderClickPostBack(e);
                        break;
                    case 'CallBack':
                        calendar.timeHeaderClickCallBack(e);
                        break;
                }     
                
                if (typeof calendar.onTimeHeaderClicked === 'function') {
                    calendar.onTimeHeaderClicked(args);
                }                
            }
            else {
                switch (this.timeHeaderClickHandling) {
                    case 'PostBack':
                        calendar.timeHeaderClickPostBack(e);
                        break;
                    case 'CallBack':
                        calendar.timeHeaderClickCallBack(e);
                        break;
                    case 'JavaScript':
                        calendar.onTimeHeaderClick(e);
                        break;
                }
            }
        };

        //        
        this.resourceCollapsePostBack = function(e, data) {
            var params = {};
            params.resource = e;

            this._postBack2("ResourceCollapse", params, data);
        };
        this.resourceCollapseCallBack = function(e, data) {
            var params = {};
            params.resource = e;

            this._callBack2("ResourceCollapse", params, data);
        };

        this._resourceCollapseDispatch = function(e) {
            
            if (calendar._api2()) {
                
                var args = {};
                args.resource = e;
                args.preventDefault = function() {
                    this.preventDefault.value = true;
                };

                if (typeof calendar.onResourceCollapse === 'function') {
                    calendar.onResourceCollapse(args);
                    if (args.preventDefault.value) {
                        return;
                    }
                }
                
                switch (this.resourceCollapseHandling) {
                    case 'PostBack':
                        calendar.resourceCollapsePostBack(e);
                        break;
                    case 'CallBack':
                        calendar.resourceCollapseCallBack(e);
                        break;
                }                
            }
            else {
                switch (this.resourceCollapseHandling) {
                    case 'PostBack':
                        calendar.resourceCollapsePostBack(e);
                        break;
                    case 'CallBack':
                        calendar.resourceCollapseCallBack(e);
                        break;
                    case 'JavaScript':
                        calendar.onResourceCollapse(e);
                        break;
                }
            }
            
        };

        // expand
        this.resourceExpandPostBack = function(e, data) {
            var params = {};
            params.resource = e;

            this._postBack2("ResourceExpand", params, data);
        };
        this.resourceExpandCallBack = function(e, data) {
            var params = {};
            params.resource = e;

            this._callBack2("ResourceExpand", params, data);
        };

        this._resourceExpandDispatch = function(e) {
            
            if (calendar._api2()) {

                var args = {};
                args.resource = e;
                args.preventDefault = function() {
                    this.preventDefault.value = true;
                };

                if (typeof calendar.onResourceExpand === 'function') {
                    calendar.onResourceExpand(args);
                    if (args.preventDefault.value) {
                        return;
                    }
                }

                switch (this.resourceExpandHandling) {
                    case 'PostBack':
                        calendar.resourceExpandPostBack(e);
                        break;
                    case 'CallBack':
                        calendar.resourceExpandCallBack(e);
                        break;
                }

            }
            else {
                switch (this.resourceExpandHandling) {
                    case 'PostBack':
                        calendar.resourceExpandPostBack(e);
                        break;
                    case 'CallBack':
                        calendar.resourceExpandCallBack(e);
                        break;
                    case 'JavaScript':
                        calendar.onResourceExpand(e);
                        break;
                }
                
            }
            
        };

        this.eventEditPostBack = function(e, newText, data) {
            var params = {};
            params.e = e;
            params.newText = newText;

            this._postBack2("EventEdit", params, data);
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
        };

        this.commandCallBack = function(command, data) {
            this._invokeCommand("CallBack", command, data);
        };

        this.commandPostBack = function(command, data) {
            this._invokeCommand("PostBack", command, data);
        };

        this._invokeCommand = function(type, command, data) {
            var params = {};
            params.command = command;

            this._invokeEvent(type, "Command", params, data);
        };


        this._postBack2 = function(action, parameters, data) {
            var envelope = {};
            envelope.action = action;
            envelope.type = "PostBack";
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

            this._stopAutoRefresh();

            this.callbackTimeout = window.setTimeout(function() {
                calendar._loadingStart();
            }, 100);

            var envelope = {};

            envelope.action = action;
            envelope.type = type;
            envelope.parameters = parameters;
            envelope.data = data;
            envelope.header = this._getCallBackHeader();

            var json = DayPilot.JSON.stringify(envelope);

            var commandstring;
            if (typeof Iuppiter !== 'undefined' && Iuppiter.compress) {
                commandstring = "LZJB" + Iuppiter.Base64.encode(Iuppiter.compress(json));
            }
            else {
                commandstring = "JSON" + json;
            }

            var context = null;
            if (this.backendUrl) {
                DayPilot.request(this.backendUrl, this._callBackResponse, commandstring, this._ajaxError);
            }
            else if (typeof WebForm_DoCallback === 'function') {
                WebForm_DoCallback(this.uniqueID, commandstring, this._updateView, context, this.callbackError, true);
            }
        };
        
        this._serverBased = function() {
            if (this.backendUrl) {  // ASP.NET MVC, Java
                return true;
            }
            if (typeof WebForm_DoCallback === 'function' && this.uniqueID) {  // ASP.NET WebForms
                return true;
            }
            return false;
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
            h.control = "dps";
            h.id = this.id;

            // callback-changeable state
            h.startDate = calendar.startDate;
            h.days = calendar.days;
            h.cellDuration = calendar.cellDuration;
            h.cellGroupBy = calendar.cellGroupBy;
            h.cellWidth = calendar.cellWidth;
            h.cellWidthSpec = calendar.cellWidthSpec;

            // extra properties
            h.viewType = calendar.viewType; // serialize
            h.hourNameBackColor = calendar.hourNameBackColor;
            h.showNonBusiness = calendar.showNonBusiness;
            h.businessBeginsHour = calendar.businessBeginsHour;
            h.businessEndsHour = calendar.businessEndsHour;
            h.weekStarts = calendar.weekStarts;
            h.treeEnabled = calendar.treeEnabled;
            h.backColor = calendar.cellBackColor;
            h.nonBusinessBackColor = calendar.cellBackColorNonBusiness;
            h.locale = calendar.locale;
            h.timeZone = calendar.timeZone;
            h.tagFields = calendar.tagFields;
            h.timeHeaders = calendar.timeHeaders;
            h.cssOnly = calendar.cssOnly;
            h.cssClassPrefix = calendar.cssClassPrefix;
            h.durationBarMode = calendar.durationBarMode;
            h.showBaseTimeHeader = calendar.showBaseTimeHeader;
            h.rowHeaderColumns = calendar.rowHeaderColumns;
            h.scale = calendar.scale;

            // custom state
            h.clientState = calendar.clientState;

            // user-changeable state
            if (this.nav.scroll) {
                h.scrollX = this.nav.scroll.scrollLeft;
                h.scrollY = this.nav.scroll.scrollTop;
            }

            h.selected = calendar.multiselect.events();

            // special
            h.hashes = calendar.hashes;

            var area = calendar._getArea(h.scrollX, h.scrollY);
            var range = calendar._getAreaRange(area);
            var res = calendar._getAreaResources(area);

            //h.scrollX = calendar.scrollX;
            //h.scrollY = calendar.scrollY;

            h.rangeStart = range.start;
            h.rangeEnd = range.end;
            h.resources = res;
            h.dynamicLoading = calendar.dynamicLoading;

            if (this.syncResourceTree) {
                h.tree = this._getTreeState();
            }

            return h;
        };
        
        this.getViewPort = function() {
            var scrollX = this.nav.scroll.scrollLeft;
            var scrollY = this.nav.scroll.scrollTop;
            
            var area = this._getArea(scrollX, scrollY);
            var range = this._getAreaRange(area);
            var resources = this._getAreaResources(area);
            
            var result = {};
            result.start = range.start;
            result.end = range.end;
            result.resources = resources;
            
            return result;
        };

        this._getArea = function(scrollX, scrollY) {
            var area = {};
            area.start = {};
            area.end = {};

            area.start.x = Math.floor(scrollX / calendar.cellWidth);
            area.end.x = Math.floor((scrollX + calendar.nav.scroll.clientWidth) / calendar.cellWidth);

            area.start.y = calendar._getRow(scrollY).i;
            area.end.y = calendar._getRow(scrollY + calendar.nav.scroll.clientHeight).i;

            var maxX = this.itline.length;
            if (area.end.x >= maxX) {
                area.end.x = maxX - 1;
            }

            return area;
        };

        this._getAreaRange = function(area) {
            var result = {};

            if (this.itline.length <= 0) {
                result.start = this.startDate;
                result.end = this.startDate;
                return result;
            }

            if (!this.itline[area.start.x]) {
                throw 'Internal error: area.start.x is null.';
            }
            result.start = this.itline[area.start.x].start;
            result.end = this.itline[area.end.x].end;

            return result;
        };

        this._getAreaResources = function(area) {
            // this might not be necessary, ported from DPSD
            if (!area) {
                var area = this._getArea(this.nav.scroll.scrollLeft, this.nav.scroll.scrollTop);
            }

            var res = [];
            res.ignoreToJSON = true;  // preventing Gaia and prototype to mess up with Array serialization

            for (var i = area.start.y; i <= area.end.y; i++) {
                var r = calendar.rows[i];
                if (r && !r.Hidden) {
                    res.push(r.Value);
                }
            }
            return res;
        };


        this._getTreeState = function() {
            var tree = [];
            tree.ignoreToJSON = true; // preventing Gaia and prototype to mess up with Array serialization

            for (var i = 0; i < this.rows.length; i++) {
                if (this.rows[i].Level > 0) {
                    continue;
                }

                var node = this._getNodeState(i);
                tree.push(node);
            }
            return tree;
        };

        this._getNodeChildren = function(indices) {
            var children = [];
            children.ignoreToJSON = true; // preventing Gaia to mess up with Array serialization
            for (var i = 0; i < indices.length; i++) {
                children.push(this._getNodeState(indices[i]));
            }
            return children;
        };

        this._getNodeState = function(i) {
            var row = this.rows[i];

            var node = {};
            node.Value = row.Value;
            node.BackColor = row.BackColor;
            node.Name = row.Name;
            node.InnerHTML = row.InnerHTML;
            node.ToolTip = row.ToolTip;
            node.Expanded = row.Expanded;
            node.Children = this._getNodeChildren(row.Children);
            node.Loaded = row.Loaded;
            node.IsParent = row.IsParent;
            node.Columns = this._getNodeColumns(row);
            if (row.MinHeight !== calendar.rowMinHeight) {
                node.MinHeight = row.MinHeight;
            }
            if (row.MarginBottom !== calendar.rowMarginBottom) {
                node.MarginBottom = row.MarginBottom;
            }

            return node;
        };

        this._getNodeColumns = function(row) {

            if (!row.Columns || row.Columns.length === 0) {
                return null;
            }

            var columns = [];
            columns.ignoreToJSON = true; // preventing Gaia to mess up with Array serialization

            for (var i = 0; i < row.Columns.length; i++) {
                var c = {};
                c.InnerHTML = row.Columns[i].html;

                columns.push(c);
            }

            return columns;
        };

/*
        this.$ = function(subid) {
            return document.getElementById(id + "_" + subid);
        };
*/
        this._prefixCssClass = function(part) {
            var prefix = this.theme || this.cssClassPrefix;
            if (prefix) {
                return prefix + part;
            }
            else {
                return "";
            }
        };

        this._registerDispose = function() {
            var root = document.getElementById(id);
            root.dispose = this.dispose;
        };

        this.dispose = function() {

            var c = calendar;
            
            if (!c.nav.top) {
                return;
            }

            c._stopAutoRefresh();

            c._deleteEvents();
            c.divBreaks = null;
            c.divCells = null;
            c.divCorner = null;
            c.divCrosshair = null;
            c.divEvents = null;
            c.divHeader = null;
            c.divLines = null;
            c.divNorth = null;
            c.divRange = null;
            c.divResScroll = null;
            c.divSeparators = null;
            c.divSeparatorsAbove = null;
            c.divStretch = null;
            c.divTimeScroll = null;
            c.scrollRes = null;
            c.vsph = null;
            c.maind = null;

            c.nav.loading = null;

            c.nav.top.onmousemove = null;
            c.nav.top.dispose = null;
            c.nav.top.ontouchstart = null;
            c.nav.top.ontouchmove = null;
            c.nav.top.ontouchend = null;
            
            c.nav.top.removeAttribute('style');
            c.nav.top.removeAttribute('class');
            c.nav.top.innerHTML = "";
            c.nav.top.dp = null;
            c.nav.top = null;

            c.nav.scroll.onscroll = null;
            c.nav.scroll.root = null;
            c.nav.scroll = null;

            DayPilot.ue(window, 'resize', c._resize);

            DayPilotScheduler.unregister(c);
        };

        this._createShadowRange = function(object, type) {
            var maind = calendar.maind;
            var coords = calendar._getShadowCoords(object);
            var event = object.event;

            var height = event.part.height || calendar._resolved.eventHeight();
            var top = (event.part && event.part.top) ? (event.part.top + calendar.rows[event.part.dayIndex].Top) : coords.top;

            var shadow = document.createElement('div');
            shadow.setAttribute('unselectable', 'on');
            shadow.style.position = 'absolute';
            shadow.style.width = (coords.width) + 'px';
            shadow.style.height = height + 'px';
            shadow.style.left = coords.left + 'px';
            shadow.style.top = top + 'px';
            shadow.style.zIndex = 101;
            shadow.style.overflow = 'hidden';

            var inner = document.createElement("div");
            shadow.appendChild(inner);

            if (this.cssOnly) {
                shadow.className = this._prefixCssClass("_shadow");
                inner.className = this._prefixCssClass("_shadow_inner");
            }

            if (!this.cssOnly) {
                if (type === 'Fill') { // transparent shadow        
                    inner.style.backgroundColor = "#aaaaaa";
                    inner.style.opacity = 0.5;
                    inner.style.filter = "alpha(opacity=50)";
                    inner.style.height = "100%";
                    if (object && object.event && object.style) {
                        //shadow.style.overflow = 'hidden';
                        inner.style.fontSize = object.style.fontSize;
                        inner.style.fontFamily = object.style.fontFamily;
                        inner.style.color = object.style.color;
                        inner.innerHTML = object.event.client.innerHTML();
                    }
                }
                else {
                    shadow.style.paddingTop = "2px";
                    inner.style.border = '2px dotted #666666';
                }
            }

            maind.appendChild(shadow);
            shadow.calendar = calendar;

            return shadow;
        };

        this._createShadow = function(object, type) {
            var maind = calendar.maind;
            var coords = calendar._getShadowCoords(object);
            var event = object.event;

            var height = event.part.height || calendar._resolved.eventHeight();
            var top = (event.part && event.part.top) ? (event.part.top + calendar.rows[event.part.dayIndex].Top) : coords.top;

            var shadow = document.createElement('div');
            shadow.setAttribute('unselectable', 'on');
            shadow.style.position = 'absolute';
            shadow.style.width = (coords.width) + 'px';
            shadow.style.height = height + 'px';
            shadow.style.left = coords.left + 'px';
            shadow.style.top = top + 'px';
            shadow.style.zIndex = 101;
            shadow.style.overflow = 'hidden';

            var inner = document.createElement("div");
            shadow.appendChild(inner);

            if (this.cssOnly) {
                shadow.className = this._prefixCssClass("_shadow");
                inner.className = this._prefixCssClass("_shadow_inner");
            }

            if (!this.cssOnly) {
                if (type === 'Fill') { // transparent shadow        
                    inner.style.backgroundColor = "#aaaaaa";
                    inner.style.opacity = 0.5;
                    inner.style.filter = "alpha(opacity=50)";
                    inner.style.height = "100%";
                    if (object && object.event && object.style) {
                        //shadow.style.overflow = 'hidden';
                        inner.style.fontSize = object.style.fontSize;
                        inner.style.fontFamily = object.style.fontFamily;
                        inner.style.color = object.style.color;
                        inner.innerHTML = object.event.client.innerHTML();
                    }
                }
                else {
                    shadow.style.paddingTop = "2px";
                    inner.style.border = '2px dotted #666666';
                }
            }

            maind.appendChild(shadow);
            shadow.calendar = calendar;

            return shadow;
        };

        // y is in pixels, not row index
        this._getRow = function(y) {
            var result = {};
            var element;

            var top = 0;
            var rowEnd = 0;
            var iMax = this.rows.length; // maximum row index

            for (var i = 0; i < iMax; i++) {
                var row = this.rows[i];
                if (row.Hidden) {
                    continue;
                }
                rowEnd += row.Height;
                if (y < rowEnd || i === iMax - 1) {
                    top = rowEnd - row.Height;
                    element = row;
                    break;
                }
            }

            result.top = top;
            result.bottom = rowEnd;
            result.i = i;
            result.element = element;

            return result;
        };

        this._getRowByIndex = function(i) {
            var top = 0;
            var bottom = 0;
            var index = 0; // visible index

            if (i > this.rows.length - 1) {
                throw "Row index too high (DayPilotScheduler._getRowByIndex)";
            }

            for (var j = 0; j <= i; j++) {
                var row = this.rows[j];

                if (row.Hidden) {
                    continue;
                }

                bottom += row.Height;
                index++;
            }

            top = bottom - row.Height;

            var result = {};
            result.top = top;
            result.height = row.Height;
            result.bottom = bottom;
            result.i = index - 1;
            result.data = row;

            return result;

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

        // internal
        /*
        this.events.findRows = function(data) {  // event data is referenced correctly
        var rows = [];
        for (var i = 0; i < calendar.rows.length; i++) {
        var row = calendar.rows[i];
        for (var r = 0; r < row.events.length; r++) {
        if (row.events[i].data == data) {
        rows.push(i);
        }
        }
        }
        return rows;
        };
        */

        // internal
        this.events._removeFromRows = function(data) {
            var rows = [];
            for (var i = 0; i < calendar.rows.length; i++) {
                var row = calendar.rows[i];
                calendar._ensureRowData(i);
                for (var r = 0; r < row.events.length; r++) {
                    if (row.events[r].data === data) {
                        rows.push(i);
                        row.events.splice(r, 1);
                        break; // only once per row
                    }
                }
            }
            return rows;
        };

        // internal
        // fast, use instead of full loadEvents()
        this.events._addToRows = function(data) {
            var rows = [];
            for (var i = 0; i < calendar.rows.length; i++) {
                var row = calendar.rows[i];
                calendar._ensureRowData(i);
                var ep = calendar._loadEvent(data, row);
                if (ep) {
                    var index = DayPilot.indexOf(calendar.events.list, ep.data);
                    calendar._doBeforeEventRender(index);
                    if (typeof calendar.onBeforeEventRender === 'function') {
                        ep.cache = calendar._cache.events[index];
                    }
                    
                    rows.push(i);
                }
            }
            return rows;
        };


        this.events.update = function(e, data) {
            var params = {};
            params.oldEvent = new DayPilot.Event(e.copy(), calendar);
            params.newEvent = new DayPilot.Event(e.temp(), calendar);

            var action = new DayPilot.Action(calendar, "EventUpdate", params, data);

            var rows = calendar.events._removeFromRows(e.data);

            e.commit();

            rows = rows.concat(calendar.events._addToRows(e.data));
            
            calendar._loadRows(rows);
            calendar._updateRowHeights();
            calendar._updateRowsNoLoad(rows);
            calendar._updateHeight();

            return action;
        };


        this.events.remove = function(e, data) {

            var params = {};
            params.e = new DayPilot.Event(e.data, calendar);

            var action = new DayPilot.Action(calendar, "EventRemove", params, data);

            var index = DayPilot.indexOf(calendar.events.list, e.data);
            calendar.events.list.splice(index, 1);

            var rows = calendar.events._removeFromRows(e.data);

            calendar._loadRows(rows);

            calendar._updateRowHeights();

            calendar._updateRowsNoLoad(rows);

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

            //var ri = DayPilot.indexOf(calendar.rows, calendar._findRowByResourceId(e.resource()));
            //var row = calendar.rows[ri];

            // prepare
            //var start = new Date();

            var rows = calendar.events._addToRows(e.data);

            calendar._loadRows(rows);

            calendar._updateRowHeights();

            //var end = new Date();

            calendar._updateRowsNoLoad(rows);
            
            if (calendar.viewType === "Gantt" && calendar.initialized) {
                calendar.update();
            }

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

        this.cells.find = function(start, resource) {
            var pixels = calendar.getPixels(new DayPilot.Date(start));
            if (!pixels) {
                return Cells();
            }
            var x = pixels.i;
            
            var row = calendar._findRowByResourceId(resource);
            if (!row) {
                return Cells();
            }
            var top = row.Top;
            var y = calendar._getRow(top).i;
            
            return this.findXy(x, y);
        };
        
        this.cells.findByPixels = function(x, y) {
            var itc = calendar._getItlineCellFromPixels(x);
            if (!itc) {
                return Cells();
            }
            var x = itc.x;
            
            var row = calendar._getRow(y);
            if (!row) {
                return Cells();
            }
            var y = row.i;
            return this.findXy(x, y);
        };
        
        this.cells.all = function() {
            var list = [];
            // may require optimization
            var maxX = calendar.itline.length;
            var maxY = calendar.rows.length;
            for(var x = 0; x < maxX; x++) {
                for (var y = 0; y < maxY; y++) {
                    var cell = calendar.cells.findXy(x, y);
                    list.push(cell[0]);
                }
            }
            return Cells(list);
        };
        
        this.cells._cell = function(x, y) {
            var itc = calendar.itline[x];
            
            var cell = {};
            cell.x = x;
            cell.y = y;
            cell.i = x + "_" + y;
            cell.resource = calendar.rows[y].Value;
            cell.start = itc.start;
            cell.end = itc.end;
            cell.update = function() { // if visible

                if (!calendar.rows[cell.y].Hidden) {
                    var area = calendar._getDrawArea();
                    if (area.xStart <= cell.x && cell.x <= area.xEnd) {
                        if (area.yStart <= cell.y && cell.y <= area.yEnd) {
                            var old = calendar._cache.cells[cell.i];
                            calendar._deleteCell(old);
                            calendar._drawCell(cell.x, cell.y);
                        }
                    }
                }

            };
            cell.div = calendar._cache.cells[cell.i];
            if (!calendar.cellProperties) {
                calendar.cellProperties = [];
            }
            if (calendar.cellProperties) {
                var p = calendar._getCellProperties(x, y);
                if (!p) {
                    p = {};
                    calendar.cellProperties[cell.i] = p;
                }
                cell.properties = p;
            }
            return cell;
        };
        
        /* accepts findXy(0,0) or findXy([{x:0, y:0}, {x:0, y:1}]) */
        this.cells.findXy = function(x, y) {
            
            if (DayPilot.isArray(x)) {
                var cells = [];
                for (var i = 0; i < x.length; i++) {
                    var o = x[i];
                    cells.push(calendar.cells._cell(o.x, o.y));
                }
                return Cells(cells);
            } 
            else if (x === null || y === null) {
                return Cells(); // empty
            }
            var cell = calendar.cells._cell(x, y);
            return Cells(cell);            
        };
        
        var Cells = function(a) {
            var list = [];
            
            if (DayPilot.isArray(a)) {
                for (var i = 0; i < a.length; i++) {
                    list.push(a[i]);
                }
            }
            else if (typeof a === 'object') {
                list.push(a);
            }
            
            list.cssClass = function(css) {
                this.each(function(item) {
                    item.properties.cssClass = DayPilot.Util.addClassToString(item.properties.cssClass, css);
                    item.update();
/*
                    if (item.div) {
                        DayPilot.Util.addClass(item.div, css);
                    }*/
                });
                return this;
            };
            
            list.removeClass = function(css) {
                this.each(function(item) {
                    item.properties.cssClass = DayPilot.Util.removeClassFromString(item.properties.cssClass, css);
                    item.update();
                    /*
                    if (item.div) {
                        DayPilot.Util.removeClass(item.div, css);
                    }*/
                });
                return this;
            };
            
            list.addClass = list.cssClass;
            
            list.html = function(html) {
                this.each(function(item) {
                    item.properties.html = html;
                    item.update();
                    /*
                    if (item.div) {
                        item.div.innerHTML = html;
                    }*/
                });
                return this;
            };
            
            list.each = function(f) {
                if (!f) {
                    return;
                }
                for (var i = 0; i < this.length; i++) {
                    f(list[i]);
                }
            };

            return list;
        };

        this._debug = function(msg, append) {
            if (!this.debuggingEnabled) {
                return;
            }

            if (!calendar.debugMessages) {
                calendar.debugMessages = [];
            }
            calendar.debugMessages.push(msg);

        };

        this.showDebugMessages = function() {
            alert("Log:\n" + calendar.debugMessages.join("\n"));
        };
        
        this.debug = new DayPilot.Debug(this);
        
        this._getRowStartInDaysView = function(date) {
            if (calendar.viewType !== 'Days') {
                throw "Checking row start when viewType !== 'Days'";
            }
            for (var i = 0; i < calendar.rows.length; i++) {
                var row = calendar.rows[i];
                var data = row.element ? row.element.data : row.data;
                var start = data.start;
                if (date.getTime() > start.getTime() && date.getTime() < start.addDays(1).getTime()) {
                    return start;
                }
            }
            return null;
        };
        
        this._getBoxStart = function(date) {
            
            if (date.ticks === this.startDate.ticks) {
                return date;
            }

            var cursor = this.startDate;

            if (date.ticks < this.startDate.ticks) {
                var firstCellDuration = this.itline[0].end.ticks - this.itline[0].start.ticks;
                while (cursor.ticks > date.ticks) {
                    cursor = cursor.addTime(-firstCellDuration);
                }
                return cursor;
            }
            
            if (calendar.viewType === 'Days') {
                var rowStart = this._getRowStartInDaysView(date);
                var offset = rowStart.getTime() - calendar.startDate.getTime();

                var cell = this._getItlineCellFromTime(date.addTime(-offset));
                if (cell.current) {
                    return cell.current.start.addTime(offset);
                }
                if (cell.past) {
                    return cell.previous.end.addTime(offset);
                }
                throw "getBoxStart(): time not found";
                
            }
            else {
                var cell = this._getItlineCellFromTime(date);
                if (cell.current) {
                    return cell.current.start;
                }
                if (cell.past) {
                    return cell.previous.end;
                }
                if (cell.hidden) {
                    var diff = cell.next.start.getTime() - date.getTime();
                    var cellduration = cell.next.end.getTime() - cell.next.start.getTime();
                    var rounded = Math.ceil(diff / cellduration) * cellduration;
                    var result = cell.next.start.addTime(-rounded);
                    return result;
                }
                throw "getBoxStart(): time not found";
            }
            
            
            /*
            cursor = date;
            var cell = null;

            while (cell === null) {
                cell = this._getItlineCellFromTime(cursor);
                cursor = cursor.addMinutes(1);
            }

            return cell.start;
            */

        };

        this._getShadowCoords = function(object) {

            // get row
            var row = this._getRow(calendar.coords.y);

            //var object = DayPilotScheduler.moving;
            var e = object.event;
            if (typeof e.end !== 'function') {
                alert("e.end function is not defined");
            }
            if (!e.end()) {
                alert("e.end() returns null");
            }
            var duration = DayPilot.Date.diff(e.end().d, e.start().d);

            var useBox = resolved.useBox(duration);

            var day = e.start().getDatePart();
            var startOffsetTime = 0;

            var x = calendar.coords.x;
            if (calendar.scale === "Manual") {
                var minusDurationPx = (function() {
                    var end = calendar.getDate(calendar.coords.x, true, true);
                    var start = end.addTime(-duration);

                    var startPix = calendar.getPixels(start).boxLeft;
                    var endPix = calendar.getPixels(end).boxRight;

                    var end = Math.min(endPix, calendar.coords.x);

                    return end - startPix;
                })();

                var offset = Math.min(DayPilotScheduler.moveOffsetX, minusDurationPx);

                x = calendar.coords.x - offset;
                
            }

            if (useBox) {
                //startOffsetTime = e.start().getTime() - (day.getTime() + Math.floor((e.start().getHours() * 60 + e.start().getMinutes()) / calendar.cellDuration) * calendar.cellDuration * 60 * 1000);
                
                var cell = calendar._getItlineCellFromTime(e.start());
                var startInTimeline = !cell.hidden && !cell.past;
                
                startOffsetTime = e.start().getTime() - this._getBoxStart(e.start()).getTime();
                
                if (startInTimeline) {
                    startOffsetTime = (function(originalTime, offset) {
                        var oticks = calendar._getCellTicks(calendar._getItlineCellFromTime(originalTime).current);
                        var nticks = calendar._getCellTicks(calendar._getItlineCellFromPixels(x).cell);

                        if (oticks > nticks * 1.2) { // normally one would be fine but avoid month issues when moving to shorter month (28 vs 31 days)
                            var sign = offset > 0 ? 1 : -1;
                            var offset = Math.abs(offset);
                            while (offset >= nticks) {
                                offset -= nticks;
                            }
                            offset *= sign;
                        }
                        return offset;
                    })(e.start(), startOffsetTime);
                }
            } 

            var dragOffsetTime = 0;
            
            // this keeps the cell offset the same after moving
            if (DayPilotScheduler.moveDragStart && calendar.scale !== "Manual") {
                
                if (useBox) {
                    dragOffsetTime = DayPilotScheduler.moveDragStart.getTime() - this._getBoxStart(e.start()).getTime();
                    var cellDurationTicks = calendar._getCellDuration() * 60 * 1000;
                    dragOffsetTime = Math.floor(dragOffsetTime/cellDurationTicks) * cellDurationTicks;
                }
                else {
                    dragOffsetTime = DayPilotScheduler.moveDragStart.getTime() - e.start().getTime();
                }
            } 
            else { // external drag
                //dragOffsetTime = this.cellDuration * 60000 / 2; // half cell duration
                dragOffsetTime = 0; // half cell duration
            }
            if (this.eventMoveToPosition) {
                dragOffsetTime = 0;
            }
            
            var start = this.getDate(x, true).addTime(-dragOffsetTime);
            
            if (DayPilotScheduler.resizing) {
                start = e.start();
            }

            if (this.snapToGrid) { // limitation: this blocks moving events before startDate
                if (calendar.viewType === "Days") {
                    //var currentRowOffset = row.element.Start.getTime() - this.startDate.getTime();
                    start = this._getBoxStart(start);
                    //start = this._getBoxStart(start.addTime(-currentRowOffset));
                    //start.addTime(currentRowOffset);
                }
                else {
                    start = this._getBoxStart(start);
                }
            }
            
            start = start.addTime(startOffsetTime);
            var end = start.addTime(duration);

            var adjustedStart = start;
            var adjustedEnd = end;

            if (this.viewType === 'Days') {
                var rowOffset = this.rows[e.part.dayIndex].Start.getTime() - this.startDate.getTime();
                var adjustedStart = start.addTime(-rowOffset);
                var adjustedEnd = adjustedStart.addTime(duration);

                //var currentRowOffset = row.element.Start.getTime() - this.startDate.getTime();
                var currentRowOffset = row.element.data.start.getTime() - this.startDate.getTime();
                start = adjustedStart.addTime(currentRowOffset);
                end = start.addTime(duration);
            }

            var startPixels = this.getPixels(adjustedStart);
            var endPixels = this.getPixels(adjustedEnd);

            var left = (useBox) ? startPixels.boxLeft : startPixels.left;
            var width = (useBox) ? (endPixels.boxRight - left) : (endPixels.left - left);

            var coords = {};
            coords.top = row.top;
            coords.left = left;
            coords.row = row.element;
            coords.rowIndex = row.i;
            coords.width = width;
            coords.start = start;
            coords.end = end;
            coords.relativeY = calendar.coords.y - row.top;

            return coords;
        };
        
        this._getCellDuration = function() {  // approximate, needs to be updated for a specific time (used only for rounding in getShadowCoords
            switch (this.scale) {
                case "CellDuration":
                    return this.cellDuration;
                case "Minute":
                    return 1;
                case "Hour":
                    return 60;
                case "Day":
                    return 60*24;
                case "Week":
                    return 60*24*7;
                case "Month":
                    return 60*24*30;
                case "Year":
                    return 60*24*365;
            }
            throw "can't guess cellDuration value";
        };
        
        this._getCellTicks = function(itc) {
            return itc.end.ticks - itc.start.ticks;
        };

        this._isRowDisabled = function(y) {
            return this.treePreventParentUsage && this._isRowParent(y);
        };

        this._isRowParent = function(y) {
            var row = this.rows[y];
            if (row.IsParent) {
                return true;
            }
            if (this.treeEnabled) {
                if (row.Children && row.Children.length > 0) {
                    return true;
                }
            }
            return false;
        };
        
        this._autoexpand = {};
        this._expandParent = function() {
            var coords = this._getShadowCoords(DayPilotScheduler.moving);
            var y = coords.rowIndex;
            var isParent = this._isRowParent(y);
            
            var expand = this._autoexpand;
            if (expand.timeout && expand.y !== y) {
                clearTimeout(expand.timeout);
                expand.timeout = null;
            }
            
            if (isParent) {
                expand.y = y;
                if (!expand.timeout) {
                    expand.timeout = setTimeout(function() {
                        var collapsed = !calendar.rows[expand.y].Expanded;
                        if (collapsed) {
                            calendar._toggle(expand.y);
                            calendar._moveShadow();
                        }
                        expand.timeout = null;
                    }, 500);
                }
            }
        };

        this._moveShadow = function() {
            var scroll = this.nav.scroll;
            if (!calendar.coords) {
                return;
            }

            var shadow = DayPilotScheduler.movingShadow;
            var coords = this._getShadowCoords(DayPilotScheduler.moving);
            var ev = DayPilotScheduler.moving.event;

            var linepos = 0;
            (function() {
                //return;
                var y = coords.relativeY;
                var row = coords.row;
                var linesCount = row.lines.length;
                var top = 0;
                var lh = calendar._resolved.eventHeight();
                var max = row.lines.length;
                for (var i = 0; i < row.lines.length; i++) {
                    var line = row.lines[i];
                    //if (line.isFree(coords.left, coords.width)) {
                    if (line.isFree(coords.left, calendar.cellWidth)) {
                        max = i;
                        break;
                    }
                }

                var pos = Math.floor((y - top + lh / 2) / lh);  // rounded position
                var pos = Math.min(max, pos);  // no more than max
                var pos = Math.max(0, pos);  // no less then 0

                linepos = pos;

            })();

            var verticalAllowed = (ev.cache && typeof ev.cache.moveVDisabled !== 'undefined') ? !ev.cache.moveVDisabled : !ev.data.moveVDisabled;
            var horizontalAllowed = (ev.cache && typeof ev.cache.moveHDisabled !== 'undefined') ? !ev.cache.moveHDisabled :!ev.data.moveHDisabled;

            var relY = linepos * calendar._resolved.eventHeight();
            if (relY > 0) {
                relY -= 3;
            }

            if (verticalAllowed) {
                if (!this._isRowDisabled(coords.rowIndex)) {
                    shadow.row = coords.row;
                    shadow.style.height = Math.max(coords.row.Height, 0) + 'px';
                    shadow.style.top = (coords.top) + 'px';
                    if (calendar.eventMoveToPosition) {
                        shadow.style.top = (coords.top + relY) + "px";
                        shadow.style.height = "3px";
                        shadow.line = linepos;
                    }
                }
                else {
                    var oldRow = shadow.row;
                    var dir = coords.rowIndex < oldRow.Index ? 1 : -1;
                    for (var i = coords.rowIndex; i !== oldRow.Index; i += dir) {
                        var row = this.rows[i];
                        if (!this._isRowDisabled(i) && !row.Hidden) {
                            shadow.style.top = (row.Top) + 'px';
                            shadow.style.height = Math.max(row.Height, 0) + 'px';
                            shadow.row = row;

                            if (calendar.eventMoveToPosition) {
                                linepos = dir > 0 ? 0 : row.lines.length - 1;
                                shadow.style.top = (coords.top + relY) + "px";
                                shadow.style.height = "3px";
                                shadow.line = linepos;
                            }

                            break;
                        }
                    }
                }
            }
            else {
                var oldRow = this.rows[this._getRow(parseInt(shadow.style.top)).i];

                var max = oldRow.lines.length;
                for (var i = 0; i < oldRow.lines.length; i++) {
                    var line = oldRow.lines[i];
                    if (line.isFree(coords.left, calendar.cellWidth)) {
                        max = i;
                        break;
                    }
                }

                shadow.style.height = Math.max(oldRow.Height, 0) + 'px';
                shadow.style.top = (oldRow.Top) + 'px';
                shadow.row = oldRow;
                if (calendar.eventMoveToPosition) {
                    if (coords.row === oldRow) {
                        shadow.style.top = (oldRow.Top + relY) + "px";
                        shadow.style.height = "3px";
                        shadow.line = linepos;
                    }
                    else {
                        var pos = (coords.rowIndex > oldRow.Index && max > 0) ? max * calendar._resolved.eventHeight() - 3 : 0;
                        //document.title = "max:" + max + ", row.i:" + oldRow.Index;
                        shadow.style.top = (oldRow.Top + pos) + "px";
                        shadow.style.height = "3px";
                        shadow.line = 0;
                    }
                }
            }

            if (horizontalAllowed) {
                shadow.style.left = coords.left + 'px';
                if (calendar.eventMoveToPosition) {
                    shadow.style.width = (calendar.cellWidth) + 'px';
                }
                else {
                    shadow.style.width = (coords.width) + 'px';
                }
                shadow.start = coords.start;
                shadow.end = coords.end;
            }
            else {
                shadow.start = ev.start();
                shadow.end = ev.end();
            }

            //document.title = "pos:" + shadow.line;
            if (typeof calendar.onEventMoving === 'function') {
                
                (function() {
                    
                    var last = calendar._lastEventMoving;
                    
                    // don't fire the event if there is no change
                    if (last && last.start.getTime() === shadow.start.getTime() && last.end.getTime() === shadow.end.getTime() && last.resource === shadow.row.Value) {
                        return;
                    }
                
                    var args = {};
                    args.start = shadow.start;
                    args.end = shadow.end;
                    args.e = ev;
                    //args.row = shadow.row;
                    args.resource = shadow.row.Value;
                    args.position = shadow.line;
                    args.left = {};
                    args.left.html = args.start.toString("HH:mm");
                    args.left.width = 90;
                    args.right = {};
                    args.right.html = args.end.toString("HH:mm");
                    args.right.width = 90;

                    calendar._lastEventMoving = args;

                    calendar.onEventMoving(args);
                    
                    calendar._showShadowHover(args);
                })();
            }
        };
        
        
        this._showShadowHover = function(args) {
            var shadow = DayPilotScheduler.movingShadow;
            var space = 5;
            
            this._clearShadowHover();
            
            var pos = {};
            pos.left = parseInt(shadow.style.left);
            pos.top = parseInt(shadow.style.top);
            pos.right = pos.left + parseInt(shadow.style.width);
            
            var left = document.createElement("div");
            left.style.position = "absolute";
            left.style.left = (pos.left - args.left.width - space) + "px";
            left.style.top = pos.top + "px";
            left.style.height = calendar.eventHeight + "px";
            left.style.width = args.left.width + "px";
            //left.style.backgroundColor = "red";
            left.style.overflow = "hidden";
            left.innerHTML = args.left.html;
            left.className = this._prefixCssClass("_event_move_left");
            
            if (args.left.visible) {
                calendar.divHover.appendChild(left);
            }

            var right = document.createElement("div");
            right.style.position = "absolute";
            right.style.left = (pos.right + space) + "px";
            right.style.top = pos.top + "px";
            right.style.height = calendar.eventHeight + "px";
            right.style.width = args.right.width + "px";
            //right.style.backgroundColor = "red";
            right.style.overflow = "hidden";
            right.innerHTML = args.right.html;
            right.className = this._prefixCssClass("_event_move_right");
            
            if (args.right.visible) {
                calendar.divHover.appendChild(right);
            }

        };
        
        this._clearShadowHover = function() {
            calendar.divHover.innerHTML = ''; // clear
        };
        
        this._loadRowHeaderColumns = function() {
            if (this.rowHeaderColumns) {
                this.rowHeaderCols = DayPilot.Util.propArray(this.rowHeaderColumns, "width");
            }
        };

        this._getTotalRowHeaderWidth = function() {
            var totalWidth = 0;
            this._loadRowHeaderColumns();
            if (this.rowHeaderCols) {
                for (var i = 0; i < this.rowHeaderCols.length; i++) {
                    totalWidth += this.rowHeaderCols[i];
                }
            }
            else {
                totalWidth = this.rowHeaderWidth;
            }
            return totalWidth;
        };

        this._autoRowHeaderWidth = function() {
            if (!this._visible()) {   // not visible, doesn't make sense now
                return;
            }
            
            /*
            if (this.cellWidthSpec === 'Auto') {
                calendar.debug.message("AutoRowHeaderWidth turned off because CellWidthSpec is set to 'Auto'.", "warning");
                return;
            }
            */
            
            var table = this.divHeader;

            var max = [];
            for (var i = 0; i < table.rows.length; i++) {
                for (var j = 0; j < table.rows[i].cells.length; j++) {
                    var inner = table.rows[i].cells[j].firstChild.firstChild;
                    if (!inner || !inner.style) {
                        continue;
                    }
                    var oldWidth = inner.style.width;
                    var oldRight = inner.style.right;
                    inner.style.position = "absolute";
                    inner.style.width = "auto";
                    inner.style.right = "auto";
                    inner.style.whiteSpace = "nowrap";
                    var w = inner.offsetWidth + 2;
                    inner.style.position = "";
                    inner.style.width = oldWidth;
                    inner.style.right = oldRight;
                    inner.style.whiteSpace = "";
                    if (typeof max[j] === 'undefined') { max[j] = 0; }
                    max[j] = Math.max(max[j], w);
                }
            }
            var maxAll = 0;
            var needsUpdate = false;

            this._loadRowHeaderColumns();
            
            if (this.rowHeaderCols) {
                for (var i = 0; i < max.length; i++) {
                    if (this.rowHeaderCols[i]) {
                        if (max[i] > this.rowHeaderCols[i]) {
                            this.rowHeaderCols[i] = max[i];
                            needsUpdate = true;
                        }
                        maxAll += this.rowHeaderCols[i];
                    }
                }
            }
            else {
                if (this.rowHeaderWidth < max[0]) {
                    maxAll = max[0];
                    needsUpdate = true;
                }
            }

            if (needsUpdate) {
                if (this._splitter) {
                    // update header
                    this._splitter.widths = this.rowHeaderCols;
                    this._splitter.updateWidths();
                    // update cells
                    DayPilot.Util.updatePropsFromArray(this.rowHeaderColumns, "width", this.rowHeaderCols);
                }
                
                this.rowHeaderWidth = maxAll;
                this._updateRowHeaderWidth();
                
                this._updateAutoCellWidth();
            }
        };

        this._drawResHeader = function() {
            var parent = this.divResScroll;

            DayPilot.puc(parent);
            parent.innerHTML = '';

            this._loadRowHeaderColumns();

            var rowHeaderCols = this.rowHeaderCols;
            var columns = rowHeaderCols ? this.rowHeaderCols.length : 0;
            var totalWidth = this._getTotalRowHeaderWidth();

            var table = document.createElement("table");
            table.style.borderCollapse = "collapse";
            table.style.KhtmlUserSelect = "none";
            table.style.MozUserSelect = "none";
            table.style.webkitUserSelect = "none";
            table.style.width = totalWidth + "px";
            table.style.border = "0px none";
            table.cellSpacing = 0;
            table.cellPadding = 0;
            table.onmousemove = function() { calendar._out(); };
            if (!this.cssOnly) {
                table.className = this._prefixCssClass("resourceheader");
            }

            //table.id = this.id + "_header";

            this.divHeader = table;

            var m = this.rows.length;
            for (var i = 0; i < m; i++) {
                var row = this.rows[i];
                //var node = this.tree[i];
                if (row.Hidden) {
                    continue;
                }

                var r = table.insertRow(-1);
                var c = r.insertCell(-1);

                c.row = row;
                c.index = i;

                //c.style.width = (rowHeaderCols[0] - 1) + "px";
                var width = rowHeaderCols ? rowHeaderCols[0] : this.rowHeaderWidth;
                c.style.width = (width) + "px";
                //c.style.height = (row.Height) + "px";
                c.style.border = "0px none";

                if (!this.cssOnly) {
                    c.style.borderRight = "1px solid " + this.borderColor;
                    c.style.backgroundColor = typeof row.BackColor === 'undefined' ? calendar.hourNameBackColor : row.BackColor;
                    c.style.fontFamily = this.headerFontFamily;
                    c.style.fontSize = this.headerFontSize;
                    c.style.color = this.headerFontColor;
                    c.style.cursor = 'default';
                    c.style.padding = '0px';
                }
                if (row.ToolTip) {
                    c.title = row.ToolTip;
                }
                c.setAttribute("unselectable", "on");
                c.setAttribute('resource', row.Value);

                c.onmousemove = calendar._onResMouseMove;
                c.onmouseout = calendar._onResMouseOut;
                c.oncontextmenu = calendar._onResRightClick;
                c.onclick = calendar._onResClick;

                var div = document.createElement("div");
                div.style.width = (width) + "px";
                div.setAttribute("unselectable", "on");
                div.className = this.cssOnly ? this._prefixCssClass('_rowheader') : this._prefixCssClass('rowheader');
                if (row.CssClass) {
                    DayPilot.Util.addClass(div, row.CssClass);
                }
                if (row.BackColor) {
                    //div.style.backgroundColor = row.BackColor;
                    div.style.background = row.BackColor;
                }
                div.style.height = (row.Height) + "px";
                div.style.overflow = 'hidden';
                div.style.position = 'relative';

                var inner = document.createElement("div");
                //inner.style.width = "100%";
                inner.setAttribute("unselectable", "on");
                inner.className = this.cssOnly ? this._prefixCssClass('_rowheader_inner') : "";
                //inner.style.position = 'absolute';
                div.appendChild(inner);

                var border = document.createElement("div");
                border.style.position = "absolute";
                border.style.bottom = "0px";
                border.style.width = "100%";
                border.style.height = "1px";
                if (this.cssOnly) {
                    border.className = this._prefixCssClass("_resourcedivider");
                }
                else {
                    border.style.backgroundColor = this.borderColor;
                }
                div.appendChild(border);

                if (this.treeEnabled) {

                    var expand = document.createElement("div");

                    expand.style.width = "10px";
                    expand.style.height = "10px";
                    expand.style.backgroundRepeat = "no-repeat";
                    expand.style.position = 'absolute';
                    expand.style.left = (row.Level * this.treeIndent + this.treeImageMarginLeft) + 'px';
                    expand.style.top = this.treeImageMarginTop + "px";


                    if (!row.Loaded && row.Children.length === 0) {
                        if (this.treeImageExpand && !this.cssOnly) {
                            expand.style.backgroundImage = "url('" + this.treeImageExpand + "')";
                        }
                        expand.className = this.cssOnly ? this._prefixCssClass('_tree_image_expand') : this._prefixCssClass('tree_image_expand');
                        expand.style.cursor = 'pointer';
                        expand.index = i;
                        expand.onclick = function(ev) { calendar._loadNode(this.index); ev = ev || window.event; ev.cancelBubble = true; };
                    }
                    else if (row.Children.length > 0) {
                        if (row.Expanded) {
                            if (this.treeImageCollapse && !this.cssOnly) {
                                expand.style.backgroundImage = "url('" + this.treeImageCollapse + "')";
                            }
                            expand.className = this.cssOnly ? this._prefixCssClass('_tree_image_collapse') : this._prefixCssClass('tree_image_collapse');
                        }
                        else {
                            if (this.treeImageExpand && !this.cssOnly) {
                                expand.style.backgroundImage = "url('" + this.treeImageExpand + "')";
                            }
                            expand.className = this.cssOnly ? this._prefixCssClass('_tree_image_expand') : this._prefixCssClass('tree_image_expand');
                        }

                        expand.style.cursor = 'pointer';
                        expand.index = i;
                        expand.onclick = function(ev) { calendar._toggle(this.index); ev = ev || window.event; ev.cancelBubble = true; };
                    }
                    else {
                        if (this.treeImageNoChildren && !this.cssOnly) {
                            //expand.src = calendar.treeImageNoChildren;
                            expand.style.backgroundImage = "url('" + this.treeImageNoChildren + "')";
                        }
                        expand.className = this.cssOnly ? this._prefixCssClass('_tree_image_no_children') : this._prefixCssClass('tree_image_no_children');
                    }

                    inner.appendChild(expand);

                }

                var text = document.createElement("div");
                if (this.treeEnabled) {
                    text.style.marginLeft = ((row.Level + 1) * this.treeIndent) + 'px';
                }
                else if (!this.cssOnly) {
                    text.style.marginLeft = "4px";
                }
                text.innerHTML = row.InnerHTML;

                inner.appendChild(text);

                c.appendChild(div);


                if (row.areas) {
                    for (var j = 0; j < row.areas.length; j++) {
                        var area = row.areas[j];
                        if (area.v !== 'Visible') {
                            continue;
                        }
                        var a = DayPilot.Areas.createArea(div, row, area);
                        div.appendChild(a);
                    }
                }


                if (!row.Columns || row.Columns.length === 0) {
                    c.colSpan = columns > 0 ? columns : 1;
                    div.style.width = totalWidth + "px";
                }
                else {
                    for (var j = 1; j < columns; j++) {

                        var c = r.insertCell(-1);

                        c.row = row;
                        c.index = i;

                        //c.style.width = (rowHeaderCols[j]) + "px";
                        if (!this.cssOnly) {
                            c.style.borderRight = "1px solid " + this.borderColor;
                            //c.style.borderBottom = "1px solid " + this.borderColor;
                            c.style.backgroundColor = row.BackColor;
                            c.style.fontFamily = this.headerFontFamily;
                            c.style.fontSize = this.headerFontSize;
                            c.style.color = this.headerFontColor;
                            c.style.cursor = 'default';
                            c.style.padding = '0px';
                        }
                        if (row.ToolTip) {
                            c.title = row.ToolTip;
                        }
                        c.setAttribute("unselectable", "on");
                        if (!this.cssOnly) {
                            c.className = this._prefixCssClass('rowheader');
                        }
                        c.setAttribute('resource', row.Value);

                        c.onmousemove = calendar._onResMouseMove;
                        c.onmouseout = calendar._onResMouseOut;
                        c.oncontextmenu = calendar._onResRightClick;
                        c.onclick = calendar._onResClick;

                        var div = document.createElement("div");
                        var w = this.cssOnly ? rowHeaderCols[j] : rowHeaderCols[j] - 1;
                        if (row.BackColor) {
                            div.style.backgroundColor = row.BackColor;
                        }
                        div.style.width = w + "px";
                        div.style.height = (row.Height) + "px";
                        div.style.overflow = 'hidden';
                        div.style.position = 'relative';
                        div.setAttribute("unselectable", "on");
                        if (this.cssOnly) {
                            DayPilot.Util.addClass(div, this._prefixCssClass("_rowheader"));
                            DayPilot.Util.addClass(div, this._prefixCssClass("_rowheadercol"));
                            DayPilot.Util.addClass(div, this._prefixCssClass("_rowheadercol" + j));
                        }
                        if (row.CssClass) {
                            DayPilot.Util.addClass(div, row.CssClass);
                        }

                        var inner = document.createElement("div");
                        //inner.style.position = 'absolute';
                        inner.setAttribute("unselectable", "on");
                        if (this.cssOnly) {
                            inner.className = this._prefixCssClass("_rowheader_inner");
                        }
                        div.appendChild(inner);

                        var border = document.createElement("div");
                        border.style.position = "absolute";
                        border.style.bottom = "0px";
                        border.style.width = (rowHeaderCols[j] - 1) + "px";
                        border.style.height = "1px";
                        border.className = this._prefixCssClass("_resourcedivider");
                        if (!this.cssOnly) {
                            border.style.backgroundColor = this.borderColor;
                        }
                        div.appendChild(border);


                        var text = document.createElement("div");
                        if (!this.cssOnly) {
                            text.style.marginLeft = '4px';
                        }

                        var innerHTML = row.Columns[j - 1] ? row.Columns[j - 1].html : "";

                        text.innerHTML = innerHTML;

                        inner.appendChild(text);

                        c.appendChild(div);
                    }
                }

            }

            var r = table.insertRow(-1);
            var c = r.insertCell(-1);
            c.colSpan = columns + 1;
            c.style.width = totalWidth + "px";
            c.style.height = (parent.clientHeight + 20) + "px";
            //c.style.borderRight = "1px solid " + this.borderColor;
            if (!this.cssOnly) {
                c.style.backgroundColor = this.hourNameBackColor;
                c.style.cursor = 'default';
            }
            c.setAttribute("unselectable", "on");
            if (!this.cssOnly) {
                c.className = this._prefixCssClass('rowheader');
                c.style.fontSize = "1px";
                c.innerHTML = "&nbsp;";
            }

            if (this.cssOnly) {
                var div = document.createElement("div");
                div.style.position = "relative";
                div.style.height = "100%";
                div.className = this._prefixCssClass('_rowheader');
                c.appendChild(div);
            }

            parent.appendChild(table);

            if (this.rowHeaderWidthAutoFit) {
                this._autoRowHeaderWidth();
            }

        };

        this._onResRightClick = function() {
            var row = this.row;

            var r = {};
            r.type = 'resource';
            r.start = row.Start;
            r.name = row.Name;
            r.value = row.Value;
            r.id = row.Value;
            r.index = this.index;
            r.root = calendar;
            r.toJSON = function(key) {
                var json = {};
                json.start = this.start;
                json.name = this.name;
                json.value = this.value;
                json.index = this.index;

                return json;
            };


            if (row.contextMenu) {
                row.contextMenu.show(r);
            }
            return false;
        };

        this._onResClick = function(ev) {
            var row = this.row;
            var r = calendar._createDayPilotResource(row, this.index);

            calendar._resourceHeaderClickDispatch(r);
        };

        this._onTimeClick = function(ev) {
            var cell = {};
            
            cell.start = this.cell.start;
            cell.level = this.cell.level;
            if (!cell.end) {
                cell.end = new DayPilot.Date(cell.start).addMinutes(calendar.cellDuration);
            }

            calendar._timeHeaderClickDispatch(cell);
        };

        this._createDayPilotResource = function(row, index) {
            var r = {};
            r.type = 'resource';
            r.start = row.Start;
            r.name = row.Name;
            r.value = row.Value;
            r.id = row.Value;
            r.index = index;
            r.root = calendar;
            r.toJSON = function(key) {
                var json = {};
                json.start = this.start;
                json.name = this.name;
                json.value = this.value;
                json.id = this.id;
                json.index = this.index;

                return json;
            };

            return r;
        };
        
        this._ensureRowData = function(i) {
            var row = this.rows[i];
            
            if (!row.events) {
                row.events = [];
            }
            
            if (row.data) {
                return;
            }
            
            row.data = {};

            // to be used later during client-side operations
            // rowStart
            row.data.start = new DayPilot.Date(row.Start);
            // rowStartTicks
            row.data.startTicks = row.data.start.getTime();
            // rowEnd
            row.data.end = resolved.isResourcesView() ? this._visibleEnd() : row.data.start.addDays(1);
            // rowEndTicks 
            row.data.endTicks = row.data.end.getTime();
            // rowOffset
            row.data.offset = row.Start.getTime() - this._visibleStart().getTime();
            row.data.i = i;
        };

        // assumes rows collection is created
        this._loadEvents = function(events, add) {
            var loadCache = [];

            var updatedRows = [];

            var append = null;
            if (events && add) {
                // append new events
                var supplied = events;

                var append = [];
                for (var i = 0; i < supplied.length; i++) {
                    var e = supplied[i];
                    var found = false;
                    for (var j = 0; j < this.events.list.length; j++) {
                        var ex = this.events.list[j];
                        // this causes changed events to be rendered again
                        if (ex.id === e.id && ex.start.toString() === e.start.toString() && ex.resource === e.resource) {
                        //if (ex.id === e.id) {
                            var rows = calendar.events._removeFromRows(ex);
                            //console.log(rows);
                            updatedRows = updatedRows.concat(rows);
                            this.events.list[j] = e;
                            found = true;
                            break;
                        }
                    }
                    if (!found) {
                        append.push(e);
                    }
                }
                this.events.list = this.events.list.concat(append);
                //events = this.events; 
            }
            else if (events) {
                this.events.list = events;
            }
            else if (!this.events.list) {
                this.events.list = [];
            }

            var list = append || this.events.list;
            var eventsLength = list.length;

            if (typeof this.onBeforeEventRender === 'function') {
                var start = append ? this.events.list.length - append.length : 0;
                var end = this.events.list.length;
                for (var i = start; i < end; i++) {
                    this._doBeforeEventRender(i);
                }
            }

            var useLoadCache = !this._containsDuplicateResources();

            // first, load event parts into rows
            for (var i = 0; i < this.rows.length; i++) {
                var row = this.rows[i];
                if (!append || typeof row.events === "undefined") {
                    row.events = [];
                }
                //row.lines = [];
                
                calendar._ensureRowData(i);
                
                if (this._isRowDisabled(i)) {
                    continue;
                }

                for (var j = 0; list && j < eventsLength; j++) {
                    if (loadCache[j]) {
                        continue;
                    }
                    var e = list[j];

                    var ep = this._loadEvent(e, row);

                    if (!ep) {
                        continue;
                    }
                    
                    if (typeof this.onBeforeEventRender === 'function') {
                        ep.cache = this._cache.events[j + start];
                    }

                    updatedRows.push(i);

                    // load cache is disabled to allow rows with duplicate ids
                    if (useLoadCache) {
                        if (ep.data.resource !== "*" && ep.part.start.getTime() === ep.start().getTime() && ep.part.end.getTime() === ep.end().getTime()) {
                            loadCache[j] = true;
                        }
                    }
                }
            }

            // sort events inside rows
            for (var i = 0; i < this.rows.length; i++) {
                var row = this.rows[i];
                this._loadRow(row);
            }

            this._updateRowHeights();

            return DayPilot.ua(updatedRows);
        };
        
        this._containsDuplicateResources = function() {
            var idlist = {};
            
            if (calendar.viewType !== "Resources") {
                return false;
            }
            for (var i = 0; i < calendar.rows.length; i++) {
                var row = calendar.rows[i];
                var id = row.Value;
                if (idlist[id]) {
                    return true;
                }
                idlist[id] = true;
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

        // internal
        this._loadRow = function(row) {
            row.lines = [];
            
            if (this.sortDirections) {
                row.events.sort(this._eventComparerCustom);
            }
            else {
                row.events.sort(this._eventComparer);
            }

            // put into lines
            for (var j = 0; j < row.events.length; j++) {
                var e = row.events[j];
                row.putIntoLine(e);
            }
        };

        // internal
        this._loadRows = function(rows) {  // row indices
            rows = DayPilot.ua(rows); // unique
            
            for (var i = 0; i < rows.length; i++) {
                var ri = rows[i];
                calendar._loadRow(calendar.rows[ri]);
            }

        };

        // internal
        // returns ep if the event was added to this row, otherwise null
        this._loadEvent = function(e, row) {
            var start = new DayPilot.Date(e.start);
            var end = new DayPilot.Date(e.end);

            var startTicks = start.getTime();
            var endTicks = end.getTime();
            
            if (endTicks < startTicks) {  // skip invalid events
                return null;
            }

            // belongs here
            var belongsHere = false;
            switch (this.viewType) {
                case 'Days':
                    belongsHere = !(endTicks <= row.data.startTicks || startTicks >= row.data.endTicks) || (startTicks === endTicks && startTicks === row.data.startTicks);
                    break;
                case 'Resources':
                    belongsHere = (row.Value === e.resource || row.Value === "*" || e.resource === "*") && (!(endTicks <= row.data.startTicks || startTicks >= row.data.endTicks) || (startTicks === endTicks && startTicks === row.data.startTicks));
                    break;
                case 'Gantt':
                    belongsHere = (row.Value === e.id) && !(endTicks <= row.data.startTicks || startTicks >= row.data.endTicks);
                    break;

            }
            
            if (!belongsHere) {
                return null;
            }
            
            var ep = new DayPilot.Event(e, calendar); // event part
            ep.part.dayIndex = row.data.i;
            ep.part.start = row.data.startTicks < startTicks ? ep.start() : row.data.start;
            ep.part.end = row.data.endTicks > endTicks ? ep.end() : row.data.end;

            var partStartPixels = this.getPixels(ep.part.start.addTime(-row.data.offset));
            var partEndPixels = this.getPixels(ep.part.end.addTime(-row.data.offset));
            
            var left = partStartPixels.left;
            var right = partEndPixels.left;

            // events in the hidden areas
            if (left === right && (partStartPixels.cut || partEndPixels.cut)) {
                return null;
            }

            ep.part.box = resolved.useBox(endTicks - startTicks);

            if (ep.part.box) {
                var boxLeft = partStartPixels.boxLeft;
                var boxRight = partEndPixels.boxRight;
                //var itc = this._getItlineCellFromPixels()

                //ep.part.left = Math.floor(left / this.cellWidth) * this.cellWidth;
                ep.part.left = boxLeft;
                ep.part.width = boxRight - boxLeft;
                ep.part.barLeft = Math.max(left - ep.part.left, 0);  // minimum 0
                ep.part.barWidth = Math.max(right - left, 1);  // minimum 1
            }
            else {
                ep.part.left = left;
                ep.part.width = Math.max(right - left, 0);
                ep.part.barLeft = 0;
                ep.part.barWidth = Math.max(right - left - 1, 1);
            }

            row.events.push(ep);

            return ep;

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

        this._loadResources = function() {
            this.rows = [];
            this.hasChildren = false;
            
            var resources = this.resources;

            var force = this._serverBased();
            if (!force) {
                if (this.viewType === "Gantt") {
                    resources = this._loadResourcesGantt();
                }
                else if (this.viewType === "Days") {
                    resources = this._loadResourcesDays();
                }
            }

            // pass by reference
            var index = {};
            index.i = 0;

            this._loadResourceChildren(resources, index, 0, null, this.treeEnabled, false);
        };
        
        this._loadResourcesGantt = function() {
            var list = [];
            
            if (this.ganttAppendToResources && this.resources) {
                for (var i = 0; i < this.resources.length; i++) {
                    list.push(this.resources[i]);
                }
            }
            
            if (!this.events.list) {
                return;
            }
            
            //this.resources = [];
            for (var i = 0; i < this.events.list.length; i++) {
                var e = this.events.list[i];
                var r = {};
                r.id = e.id;
                r.name = e.text;
                list.push(r);
            }
            
            return list;
        };

        this._loadResourcesDays = function() {
            var list = [];
            var locale = this._resolved.locale();
            for (var i = 0; i < this.days; i++) {
                var d = this.startDate.addDays(i);
                var r = {};
                r.name = d.toString(locale.datePattern, locale);
                r.start = d;
                list.push(r);
            }
            return list;
        };
        
        this._visibleStart = function() {
            if (this.itline && this.itline.length > 0) {
                return this.itline[0].start;
            }
            return this.startDate;
        };
        
        this._visibleEnd = function() {
            if (this.itline && this.itline.length > 0) {
                return this.itline[this.itline.length - 1].end;
            }
            return this.startDate.addDays(this.days);
        };

        this._loadResourceChildren = function(resources, index, level, parent, recursively, hidden) {
            if (!resources) {
                return;
            }
            
            for (var i = 0; i < resources.length; i++) {
                if (!resources[i]) {
                    continue;
                }
                var additional = {};
                additional.level = level;
                additional.hidden = hidden;
                additional.index = index.i;
                
                var res = this._createBeforeResHeaderRenderArgs(resources[i], additional);

                var row = {};

                // defined values
                row.BackColor = res.backColor;
                row.CssClass = res.cssClass;
                row.Expanded = res.expanded;
                row.Name = res.name;
                row.InnerHTML = res.html ? res.html : row.Name;
                row.MinHeight = typeof res.minHeight !== 'undefined' ? res.minHeight : calendar.rowMinHeight;
                row.MarginBottom = typeof res.marginBottom !== 'undefined' ? res.marginBottom : calendar.rowMarginBottom;
                row.Loaded = !res.dynamicChildren;  // default is true
                row.Value = res.id || res.value;  // accept both id and value
                row.ToolTip = res.toolTip;
                row.Children = [];
                row.Columns = [];
                row.Start = res.start ? new DayPilot.Date(res.start) : this._visibleStart();
                row.IsParent = res.isParent;
                row.contextMenu = res.contextMenu ? eval(res.contextMenu) : this.contextMenuResource;
                row.areas = res.areas;

                // calculated
                row.Height = this._resolved.eventHeight();
                row.Hidden = hidden;
                row.Level = level;
                row.Index = index.i;
                
                // reference to resource
                row.Resource = resources[i];

                // later
                //row.Height = this.eventHeight + this.rowMarginBottom;  // TODO hardcoded, needs to be recalculated after events are loaded

                // event ordering
                row.lines = [];

                row.isRow = true;

                // functions
                row.height = function() {
                    var heightInLines = Math.max(1, this.lines.length);
                    var lineHeight = calendar.durationBarDetached ? (calendar._resolved.eventHeight() + 10) : calendar._resolved.eventHeight();
                    var height = heightInLines * lineHeight + this.MarginBottom;
                    return (height > this.MinHeight) ? height : this.MinHeight;
                };

                row.putIntoLine = function(ep) {
                    var thisRow = this;

                    for (var i = 0; i < this.lines.length; i++) {
                        var line = this.lines[i];
                        if (line.isFree(ep.part.left, ep.part.width)) {
                            line.push(ep);
                            return i;
                        }
                    }

                    var line = [];
                    line.isFree = function(colStart, colWidth) {
                        //var free = true;
                        var end = colStart + colWidth - 1;
                        var max = this.length;

                        for (var i = 0; i < max; i++) {
                            var e = this[i];
                            if (!(end < e.part.left || colStart > e.part.left + e.part.width - 1)) {
                                return false;
                            }
                        }

                        return true;
                    };

                    line.push(ep);

                    this.lines.push(line);

                    return this.lines.length - 1;
                };


                this.rows.push(row);

                if (parent !== null) {
                    parent.Children.push(index.i);
                }

                if (res.columns) {
                    for (var j = 0; j < res.columns.length; j++) {
                        row.Columns.push(res.columns[j]); // plain copy, it's the same structure
                    }
                }

                index.i++;

                if (recursively && res.children && res.children.length) {
                    this.hasChildren = true;
                    var hiddenChildren = hidden || !row.Expanded;
                    this._loadResourceChildren(res.children, index, level + 1, row, true, hiddenChildren);
                }
            }
        };
        
        this._createBeforeResHeaderRenderArgs = function(res, additional) {
            var r = {};
            
            // shallow copy
            // TODO resolve children, columns
            for (var name in res) {
                r[name] = res[name];
            }
            
            if (typeof res.html === 'undefined') {
                r.html = res.name;
            }

            // extra properties like level, index, hidden
            for (var name in additional) {
                r[name] = additional[name];
            }

            if (typeof this.onBeforeResHeaderRender === 'function') {
                // TODO check for additional.hidden here
                
                var args = {};
                args.resource = r;
                this.onBeforeResHeaderRender(args);
            }
            
            return r;
        };


        this._drawTop = function() {
            this.nav.top = document.getElementById(this.id);
            this.nav.top.dp = this;
            this.nav.top.innerHTML = "";  // TODO remove
            if (!this.cssOnly) {
                this.nav.top.style.border = "1px solid " + this.borderColor;
            }
            else {
                DayPilot.Util.addClass(this.nav.top, this._prefixCssClass("_main"));
            }

            this.nav.top.style.MozUserSelect = 'none';
            this.nav.top.style.KhtmlUserSelect = 'none';
            this.nav.top.style.webkitUserSelect = 'none';
            if (this.width) {
                this.nav.top.style.width = this.width;
            }
            if (this.heightSpec === "Parent100Pct") {
                this.nav.top.style.height = "100%";
            }
            this.nav.top.style.lineHeight = "1.2";
            this.nav.top.style.position = "relative";
            
            this.nav.top.onmousemove = function(ev) {
                ev = ev || window.event;
                ev.insideMainD = true;
                if (window.event) {
                    window.event.srcElement.inside = true;
                }
            };
            
            this.nav.top.ontouchstart = touch.onMainTouchStart;
            this.nav.top.ontouchmove = touch.onMainTouchMove;
            this.nav.top.ontouchend = touch.onMainTouchEnd;

            if (this.hideUntilInit) {
                this.nav.top.style.visibility = 'hidden';
            }
            var rowHeaderWidth = this._getTotalRowHeaderWidth();

            var layout = this._resolved.layout();
            if (layout === 'DivBased') {
                // left
                var left = document.createElement("div");
                //left.style.cssFloat = "left";
                //left.style.styleFloat = "left";  // IE7
                left.style.position = "absolute";
                left.style.left = "0px";
                left.style.width = (rowHeaderWidth) + "px";

                left.appendChild(this._drawCorner());

                // divider horizontal
                var dh1 = document.createElement("div");
                dh1.style.height = "1px";
                dh1.className = this._prefixCssClass("_divider_horizontal");
                if (!this.cssOnly) {
                    dh1.style.backgroundColor = this.borderColor;
                }
                left.appendChild(dh1);
                this.nav.dh1 = dh1;

                left.appendChild(this._drawResourceHeader());
                this.nav.left = left;

                // divider                
                var divider = document.createElement("div");
                divider.style.position = "absolute";
                divider.style.left = (rowHeaderWidth) + "px";
                //divider.style.cssFloat = "left";
                //divider.style.styleFloat = "left";  // IE7
                divider.style.width = "1px";
                divider.style.height = (this._getTotalHeaderHeight() + this._getScrollableHeight()) + "px";
                divider.className = this._prefixCssClass("_divider");
                if (!this.cssOnly) {
                    divider.style.backgroundColor = this.borderColor;
                }
                this.nav.divider = divider;

                // right
                var right = document.createElement("div");
                right.style.marginLeft = (rowHeaderWidth + 1) + "px";
                right.style.marginRight = '1px';
                right.style.position = 'relative';

                right.appendChild(this._drawTimeHeaderDiv());
                this.nav.right = right;

                // divider horizontal #2
                var dh2 = document.createElement("div");
                dh2.style.height = "1px";
                dh2.style.position = "absolute";
                dh2.style.top = this._getTotalHeaderHeight() + "px";
                dh2.style.width = "100%";
                dh2.className = this._prefixCssClass("_divider_horizontal");
                if (!this.cssOnly) {
                    dh2.style.backgroundColor = this.borderColor;
                }
                right.appendChild(dh2);
                this.nav.dh2 = dh2;

                right.appendChild(this._drawMainContent());

                // clear
                var clear = document.createElement("div");
                clear.style.clear = 'left';

                // add all at once
                this.nav.top.appendChild(left);
                this.nav.top.appendChild(divider);
                this.nav.top.appendChild(right);
                this.nav.top.appendChild(clear);
            }
            else {
                var table = document.createElement("table");
                table.cellPadding = 0;
                table.cellSpacing = 0;
                table.border = 0;

                // required for proper width measuring (onresize)
                table.style.position = 'absolute';
                if (!this.cssOnly) {
                    table.style.backgroundColor = this.hourNameBackColor;
                }

                var row1 = table.insertRow(-1);

                var td1 = row1.insertCell(-1);
                td1.appendChild(this._drawCorner());

                var td2 = row1.insertCell(-1);
                td2.appendChild(this._drawTimeHeaderDiv());

                var row2 = table.insertRow(-1);

                var td3 = row2.insertCell(-1);
                td3.appendChild(this._drawResourceHeader());

                var td4 = row2.insertCell(-1);
                td4.appendChild(this._drawMainContent());

                this.nav.top.appendChild(table);
            }

            // hidden fields
            this.vsph = document.createElement("div");
            //this.vsph.id = this.id + "_vsph";
            this.vsph.style.display = "none";
            this.nav.top.appendChild(this.vsph);

            var stateInput = document.createElement("input");
            stateInput.type = "hidden";
            stateInput.id = this.id + "_state";
            stateInput.name = this.id + "_state";
            this.nav.state = stateInput;
            this.nav.top.appendChild(stateInput);

            var loading = document.createElement("div");
            loading.style.position = 'absolute';
            loading.style.left = (this._getTotalRowHeaderWidth() + 5) + "px";
            loading.style.top = (this._getTotalHeaderHeight() + 5) + "px";
            loading.style.display = 'none';
            if (!this.cssOnly) {
                loading.style.backgroundColor = this.loadingLabelBackColor;
                loading.style.fontSize = this.loadingLabelFontSize;
                loading.style.fontFamily = this.loadingLabelFontFamily;
                loading.style.color = this.loadingLabelFontColor;
                loading.style.padding = '2px';
            }
            loading.innerHTML = this.loadingLabelText;

            DayPilot.Util.addClass(loading, this._prefixCssClass("_loading"));

            this.nav.loading = loading;
            this.nav.top.appendChild(loading);


            // TODO probably can be removed
            //this.elements.events = [];
            //this.elements.separators = [];
        };
        

        // update all positions that depend on header height
        this._updateHeaderHeight = function() {
            var height = this._getTotalHeaderHeight();

            this.nav.corner.style.height = (height) + "px";
            //this.divCorner.style.height = (height) + "px";

            this.divTimeScroll.style.height = height + "px";
            this.divNorth.style.height = height + "px";
            if (this.nav.dh1 && this.nav.dh2) {
                this.nav.dh1.style.top = height + "px";
                this.nav.dh2.style.top = height + "px";
            }

            this.nav.loading.style.top = (height + 5) + "px";
            this.nav.scroll.style.top = (height + 1) + "px";
        };

        this._updateRowHeaderWidth = function() {
            this._loadRowHeaderColumns();

            var width = this._getTotalRowHeaderWidth();
            var total = width;
            this.nav.corner.style.width = width + "px";
            this.divCorner.style.width = width + "px";
            this.divResScroll.style.width = width + "px";
            this.nav.left.style.width = (width) + "px";
            this.nav.divider.style.left = (width) + "px";
            this.nav.right.style.marginLeft = (width + 1) + "px";
            if (this.nav.message) {
                this.nav.message.style.paddingLeft = width + "px";
            }
            if (this.nav.loading) {
                this.nav.loading.style.left = (width + 5) + "px";
            }

            var updateCell = function(inner, width) {
                if (!inner || !inner.style) {
                    return;
                }
                cell.style.width = width + "px";
                inner.style.width = width + "px";
                var divider = inner.lastChild;
                if (divider) {
                    divider.style.width = width + "px";
                }
            };

            var table = this.divHeader;
            table.style.width = width + "px";
            
            for (var i = 0; i < table.rows.length; i++) {
                var cell = table.rows[i].cells[0];
                if (cell.colSpan > 1) {
                    var inner = table.rows[i].cells[0].firstChild;
                    updateCell(inner, total);
                }
                else {
                    if (this.rowHeaderCols) {
                        for (var j = 0; j < table.rows[i].cells.length; j++) {
                            var width = this.rowHeaderCols[j];
                            var inner = table.rows[i].cells[j].firstChild;
                            updateCell(inner, width);
                        }
                    }
                    else {
                        var width = this.rowHeaderWidth;
                        var inner = table.rows[i].cells[0].firstChild;
                        updateCell(inner, width);
                    }
                }

            }

            this._crosshairHide(); // update

        };
        
        
        this._drawHeaderColumns = function() {
            var div = calendar.nav.corner;
            /*
            var sampleProps = [
                { title: 'Event', width: 150 },
                { title: 'Duration', width: 100 },
            ];
            */
            
            var props = this.rowHeaderColumns;
            
            var row = document.createElement("div");
            row.style.position = "absolute";
            row.style.bottom = "0px";
            row.style.left = "0px";
            row.style.width = "5000px";
            row.style.height = resolved.headerHeight() + "px";
            //row.style.backgroundColor = "red";
            row.style.overflow = "hidden";
            row.className = this._prefixCssClass("_columnheader");
            div.appendChild(row);
            
            var inner = document.createElement("div");
            inner.style.position = "absolute";
            inner.style.top = "0px";
            inner.style.bottom = "0px";
            inner.style.left = "0px";
            inner.style.right = "0px";
            inner.className = this._prefixCssClass("_columnheader_inner");
            row.appendChild(inner);
            
            var splitter = new DayPilot.Splitter(inner);
            splitter.widths = DayPilot.Util.propArray(props, "width");
            splitter.height = resolved.headerHeight();
            splitter.css.title = this._prefixCssClass("_columnheader_cell");
            splitter.css.titleInner = this._prefixCssClass("_columnheader_cell_inner");
            splitter.css.splitter = this._prefixCssClass("_columnheader_splitter");
            splitter.titles = DayPilot.Util.propArray(props, "title");
            splitter.updating = function(args) { 
                //calendar.rowHeaderCols = this.widths;
                DayPilot.Util.updatePropsFromArray(calendar.rowHeaderColumns, "width", this.widths);
                calendar._updateRowHeaderWidth();
            };
            splitter.color = '#000000';
            splitter.opacity = 30;
            //splitter.height = 19;
            splitter.init();
            
            this._splitter = splitter;
        };
        
        this._updateCorner = function() {
            var div = this.nav.corner;
            div.innerHTML = '';
            div.className = this.cssOnly ? this._prefixCssClass('_corner') : this._prefixCssClass('corner');
            if (!this.cssOnly) {
                div.style.backgroundColor = this.hourNameBackColor;
                div.style.fontFamily = this.hourFontFamily;
                div.style.fontSize = this.hourFontSize;
                div.style.cursor = 'default';
            }

            var inner = document.createElement("div");
            inner.style.position = "absolute";
            inner.style.top = "0px";
            inner.style.left = "0px";
            inner.style.right = "0px";
            inner.style.bottom = "0px";
            
            if (this.cssOnly) {
                inner.className = this._prefixCssClass('_corner_inner');
            }
            this.divCorner = inner;
            inner.innerHTML = '&nbsp;';
    
            if (this.rowHeaderColumns && this.rowHeaderColumns.length > 0) {
                var mini = document.createElement("div");
                mini.style.position = "absolute";
                mini.style.top = "0px";
                mini.style.left = "0px";
                mini.style.right = "0px";
                mini.style.bottom = (resolved.headerHeight() + 1) + "px";
                div.appendChild(mini);
                
                var divider = document.createElement("div");
                divider.style.position = "absolute";
                divider.style.left = "0px";
                divider.style.right = "0px";
                divider.style.height = "1px";
                divider.style.bottom = (resolved.headerHeight()) + "px";
                divider.className = this._prefixCssClass("_divider");
                div.appendChild(divider);
                
                mini.appendChild(inner);

                this._drawHeaderColumns();
            }
            else {
                div.appendChild(inner);
            }

            var inner2 = document.createElement("div");
            inner2.style.position = 'absolute';
            inner2.style.padding = '2px';
            inner2.style.top = '0px';
            inner2.style.left = '1px';
            inner2.style.backgroundColor = "#FF6600";
            inner2.style.color = "white";
            inner2.innerHTML = "\u0044\u0045\u004D\u004F";

            if (this.numberFormat) div.appendChild(inner2);

        };


        this._drawCorner = function() {
            var rowHeaderWidth = this._getTotalRowHeaderWidth();

            var div = document.createElement("div");
            calendar.nav.corner = div;
            div.style.width = rowHeaderWidth + "px";
            div.style.height = (this._getTotalHeaderHeight()) + "px";
            div.style.overflow = 'hidden';
            div.style.position = 'relative';
            div.setAttribute("unselectable", "on");
            div.onmousemove = function() { calendar._out(); };
            div.oncontextmenu = function() { return false; };
            
            this._updateCorner();

            return div;
        };

        this._getTotalHeaderHeight = function() {
            if (this.timeHeader) {
                var lines = this.timeHeader.length;
                if (!this.showBaseTimeHeader) {
                    lines -= 1;
                }
                return lines * resolved.headerHeight();
            }
            return 2 * resolved.headerHeight();
        };


        this._drawResourceHeader = function() {
            var div = document.createElement("div");
            if (!this.cssOnly) {
                //div.style.border = "1px solid " + this.borderColor;
                div.style.backgroundColor = this.hourNameBackColor;
            }
            div.style.width = (this._getTotalRowHeaderWidth()) + "px";
            div.style.height = this._getScrollableHeight() + "px";
            div.style.overflow = 'hidden';
            div.style.position = 'relative';
            //div.id = this.id + "_resscroll";
            div.onmousemove = function() { calendar._out(); };
            div.oncontextmenu = function() { return false; };

            this.divResScroll = div;

            this.scrollRes = div;

            return div;
        };

        this._setRightColWidth = function(div) {
            if (resolved.layout() === 'TableBased') {
                var width = parseInt(this.width, 10);
                var isPercentage = (this.width.indexOf("%") !== -1);
                var isIE = /MSIE/i.test(navigator.userAgent);
                var rowHeaderWidth = this._getTotalRowHeaderWidth();

                if (isPercentage) {
                    if (this.nav.top && this.nav.top.offsetWidth > 0) {
                        div.style.width = (this.nav.top.offsetWidth - 6 - rowHeaderWidth) + "px";
                    }
                }
                else {  // fixed
                    div.style.width = (width - rowHeaderWidth) + "px";
                }
            }
        };

        this._resize = function() {
            if (calendar._resolved.layout() === 'TableBased') {
                calendar._setRightColWidth(calendar.nav.scroll);
                calendar._setRightColWidth(calendar.divTimeScroll);
            }

            calendar._updateHeight();
            calendar._updateAutoCellWidth();
            
            calendar._cache.drawArea = null;
            calendar._findHeadersInViewPort();
        };
        
        this._updateAutoCellWidth = function() {
            if (calendar.cellWidthSpec !== 'Auto') {
                return;
            }
            // TODO detect a real dimension change
            calendar.debug.message("cell width recalc in _resize");
            calendar._calculateCellWidth();
            calendar._prepareItline();
            calendar._drawTimeHeader();
            calendar._deleteCells();
            calendar._drawCells();
            calendar._deleteSeparators();
            calendar._drawSeparators();
            calendar._deleteEvents();
            calendar._loadEvents();
            calendar._drawEvents();
        };

        this._calculateCellWidth = function() {
            return; // disabled TODO remove calls
            
            // only valid for automatic cell width
            if (this.cellWidthSpec !== 'Auto') {
                return;
            }
            var total = this.nav.top.clientWidth;
            var header = this._getTotalRowHeaderWidth();
            var full = total - header;
            var cell = full / this._cellCount();
            this.cellWidth = Math.floor(cell);
            
            calendar.debug.message("AutoCellWidth: " + this.cellWidth);
            //alert("cellwidth: " + this.cellWidth);
        };
        
        this._getScrollableWidth = function() {  // only the visible part
            /*
            if (this.nav.scroll) {
                this.debug.message("scrollableWidth/clientWidth: " + this.nav.scroll.clientWidth);
                return this.nav.scroll.clientWidth;
            }
            */
            
            //
            // TODO get directly from nav.scroll (but it may not be ready yet)
            var total = this.nav.top.clientWidth;
            var header = this._getTotalRowHeaderWidth();
            var manualAdjustment = 2;
            
            
            var height = this._getScrollableHeight();
            var innerHeight = this._getScrollableInnerHeight();
            var scrollBarWidth = 0;
            if (innerHeight > height) {
                scrollBarWidth = DayPilot.swa();
            }
            
            var full = total - header - manualAdjustment - scrollBarWidth;
            this.debug.message("scrollableWidth: " + full);
            return full;
        };

        this._drawTimeHeaderDiv = function() {

            var div = document.createElement("div");
            div.style.overflow = 'hidden';
            if (!this.cssOnly) {
                div.style.backgroundColor = this.hourNameBackColor;
                //div.style.borderRight = "1px solid " + this.borderColor;
            }
            div.style.position = 'absolute';
            div.style.display = 'block';
            div.style.top = "0px";
            div.style.width = "100%";
            div.style.height = this._getTotalHeaderHeight() + "px";
            div.style.overflow = "hidden";
            div.onmousemove = function() { calendar._out(); if (calendar.cellBubble) { calendar.cellBubble.delayedHide(); } };

            this._setRightColWidth(div);

            this.divTimeScroll = div;

            var inner = document.createElement("div");
            /*
            if (!this.cssOnly) {
            inner.style.borderTop = "1px solid " + this.borderColor;
            }
            */
            inner.style.width = (this._cellCount() * this.cellWidth + 5000) + "px";

            this.divNorth = inner;

            div.appendChild(inner);

            return div;
        };

        this._getScrollableHeight = function() {
            var height = 0;
            if (this.heightSpec === 'Fixed' || this.heightSpec === "Parent100Pct") {
                return this.height ? this.height : 0;
            }
            else {
                height = this._getScrollableInnerHeight();
            }

            if (this.heightSpec === 'Max' && height > this.height) {
                return this.height;
            }

            return height;
        };
        
        this._getScrollableInnerHeight = function() {
            var height;
            if (this.innerHeightTree) {
                var scrollHeight = DayPilot.sh(calendar.nav.scroll);
                if (scrollHeight === 0) { // no horizontal scrollbar
                    height = this.innerHeightTree;
                }
                else {
                    height = this.innerHeightTree + scrollHeight; // guessing that the vertical scrollbar width is the same as horizontal scrollbar height, used to be 18 hardcoded
                }
            }
            else {
                height = this.rows.length * this._resolved.eventHeight();
            }
            return height;
        };

/*
        this._findGrouplineX = function(left) {
            for (var i = 0; i < this.groupline.length; i++) {
                var cell = this.groupline[i];
                if (left >= cell.left && left < cell.left + cell.width) {
                    return i;
                }
            }
            return this.groupline.length - 1;
        };
*/
        this._out = function() {
            this._crosshairHide();
            this._stopScroll();
            this._cellhoverout();
        };

        this._drawMainContent = function() {

            var div = document.createElement("div");
            /*
            if (this.cellWidthSpec === "Auto") {
                div.style.overflowX = "hidden";
                div.style.overflowY = "auto";
            }
            else {*/
                div.style.overflow = "auto";
                div.style.overflowX = "auto";
                div.style.overflowY = "auto";
            //}
            //div.style.overflow = "scroll";
            div.style.position = "absolute";
            div.style.height = (this._getScrollableHeight()) + "px";
            div.style.top = (this._getTotalHeaderHeight() + 1) + "px";
            div.style.width = "100%";
            if (!this.cssOnly) {
                div.style.backgroundColor = this.emptyBackColor;
            }
            div.className = this._prefixCssClass("_scrollable");
            div.oncontextmenu = function() { return false; };

            this._setRightColWidth(div);

            //this.divScroll = div;
            this.nav.scroll = div;

            this.maind = document.createElement("div");
            this.maind.style.MozUserSelect = "none";
            this.maind.style.KhtmlUserSelect = "none";
            this.maind.style.webkitUserSelect = "none";
            
            // Android browser bug
            if (android) {
                this.maind.style.webkitTransform = "translateZ(0px)";
            }
            
            // no longer using background to draw cell borders
            /*
            if (!this.cssOnly) {
            this.maind.style.backgroundColor = this.cellBorderColor;
            }*/
            this.maind.style.position = 'absolute';
            
            var gridwidth = this._getGridWidth();
            if (gridwidth > 0 && !isNaN(gridwidth)) {
                this.maind.style.width = (gridwidth) + "px";
            }
            this.maind.setAttribute("unselectable", "on");

            this.maind.onmousedown = this._onMaindMouseDown;
            this.maind.onmousemove = this._onMaindMouseMove;
            this.maind.oncontextmenu = this._onMaindRightClick;
            this.maind.ondblclick = this._onMaindDblClick;

            this.maind.className = this._prefixCssClass("_matrix");

            this.divStretch = document.createElement("div");
            this.divStretch.style.position = 'absolute';
            this.divStretch.style.height = '1px';
            this.maind.appendChild(this.divStretch);

            this.divCells = document.createElement("div");
            this.divCells.style.position = 'absolute';
            this.divCells.oncontextmenu = this._onMaindRightClick;
            this.maind.appendChild(this.divCells);

            this.divLines = document.createElement("div");
            this.divLines.style.position = 'absolute';
            this.divLines.oncontextmenu = this._onMaindRightClick;
            this.maind.appendChild(this.divLines);

            this.divBreaks = document.createElement("div");
            this.divBreaks.style.position = 'absolute';
            this.divBreaks.oncontextmenu = this._onMaindRightClick;
            this.maind.appendChild(this.divBreaks);

            this.divSeparators = document.createElement("div");
            this.divSeparators.style.position = 'absolute'; 
            this.divSeparators.oncontextmenu = this._onMaindRightClick;
            this.maind.appendChild(this.divSeparators);

            this.divCrosshair = document.createElement("div");
            this.divCrosshair.style.position = 'absolute';
            this.divCrosshair.ondblclick = this._onMaindDblClick;
            this.maind.appendChild(this.divCrosshair);

            this.divRange = document.createElement("div");
            this.divRange.style.position = 'absolute'; 
            this.divRange.oncontextmenu = this._onMaindRightClick;
            this.maind.appendChild(this.divRange);

            this.divEvents = document.createElement("div");
            this.divEvents.style.position = 'absolute';
            this.maind.appendChild(this.divEvents);

            this.divSeparatorsAbove = document.createElement("div");
            this.divSeparatorsAbove.style.position = 'absolute';
            this.divSeparatorsAbove.oncontextmenu = this._onMaindRightClick;
            this.maind.appendChild(this.divSeparatorsAbove);
            
            this.divHover = document.createElement("div");
            this.divHover.style.position = 'absolute';
            this.maind.appendChild(this.divHover);

            // TODO add scroll labels

            div.appendChild(this.maind);

            return div;
        };

        this._loadingStart = function() {
            if (this.loadingLabelVisible) {
                calendar.nav.loading.innerHTML = this.loadingLabelText;
                calendar.nav.loading.style.display = '';
            }
        };

        this._loadingStop = function(msg) {
            calendar.status.loadingEvents = false;
            if (this.callbackTimeout) {
                window.clearTimeout(this.callbackTimeout);
            }

            if (msg) {
                this.nav.loading.innerHTML = msg;
                window.setTimeout(function() { calendar._loadingStop(); }, 1000);
            }
            else {
                this.nav.loading.style.display = 'none';
            }
        };

        this._prepareVariables = function() {
            this.startDate = new DayPilot.Date(this.startDate).getDatePart();
            //this._getEventHeightFromCss();
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
            // skip if an operation is active
            if (!DayPilotScheduler.resizing && !DayPilotScheduler.moving && !DayPilotScheduler.drag && !DayPilotScheduler.range) {
                var skip = false;
                if (typeof this.onAutoRefresh === 'function') {
                    var args = {};
                    args.i = this.autoRefreshCount;
                    args.preventDefault = function() {
                        this.preventDefault.value = true;
                    };

                    calendar.onAutoRefresh(args);
                    if (args.preventDefault.value) {
                        skip = true;
                    }
                }
                if (!skip) {
                    this.commandCallBack(this.autoRefreshCommand);
                }
                this.autoRefreshCount++;
            }
            if (this.autoRefreshCount < this.autoRefreshMaxCount) {
                this.autoRefreshTimeout = window.setTimeout(function() { calendar._doRefresh(); }, this.autoRefreshInterval * 1000);
            }
        };

        this._registerGlobalHandlers = function() {
            if (!DayPilotScheduler.globalHandlers) {
                DayPilotScheduler.globalHandlers = true;
                DayPilot.re(document, 'mousemove', DayPilotScheduler.gMouseMove);
                DayPilot.re(document, 'mouseup', DayPilotScheduler.gMouseUp);
                //DayPilot.re(window, 'unload', DayPilotScheduler.gUnload);
            }
            DayPilot.re(window, 'resize', this._resize);
        };


        this._registerOnScroll = function() {
            this.nav.scroll.root = this;  // might not be necessary
            this.nav.scroll.onscroll = this._onScroll;

            calendar.scrollPos = this.nav.scroll.scrollLeft;
            calendar.scrollTop = this.nav.scroll.scrollTop;
            calendar.scrollWidth = this.divNorth.clientWidth; // this is always available, divScroll might be not (if there are no resources)
        };

        this._saveState = function() {
            var start = new Date();
            var state = {};
            state.scrollX = this.nav.scroll.scrollLeft;
            state.scrollY = this.nav.scroll.scrollTop;

            if (this.syncResourceTree) {
                state.tree = this._getTreeState();
            }

            this.nav.state.value = DayPilot.he(DayPilot.JSON.stringify(state));
            var end = new Date();
        };

        this._drawSeparators = function() {
            if (!this.separators) {
                return;
            }
            for (var i = 0; i < this.separators.length; i++) {
                this._drawSeparator(i);
            }
        };
        
        this.batch = {};
        this.batch.step = 300;
        this.batch.delay = 10;
        this.batch.mode = "display";
        this.batch.layers = true;

        // batch rendering flushes events in 10-item batches
        this._drawEvents = function(batch) {
            var step = this.batch.step;  // batch size
            var layers = this.batch.layers;
            
            // experimental
            if (layers) {
                // create a new layer 
                calendar.divEvents = document.createElement("div");
                calendar.divEvents.style.position = 'absolute';  // relative
//                calendar.maind.appendChild(this.divEvents);

                calendar.maind.insertBefore(this.divEvents, this.divSeparatorsAbove);

            }
            else {
            }

            if (this.batch.mode === 'display') {
                this.divEvents.style.display = 'none';
            }
            else if (this.batch.mode === 'visibility') {
                this.divEvents.style.visibility = 'hidden';
            }

            var dynamic = this.dynamicEventRendering === 'Progressive';
            var area = this._getDrawArea();
            var top = area.pixels.top;
            var bottom = area.pixels.bottom;
            
            var eventMarginTop = this.durationBarDetached ? 10 : 0;

            if (this.clientSide) {
                for (var i = 0; i < this.rows.length; i++) {

                    var row = this.rows[i];

                    var rowTop = row.Top - this.dynamicEventRenderingMargin;
                    var rowBottom = rowTop + row.Height + 2 * this.dynamicEventRenderingMargin;
                    if (dynamic && (bottom <= rowTop || top >= rowBottom)) {
                        continue;
                    }
                    
                    for (var j = 0; j < row.lines.length; j++) {
                        var line = row.lines[j];
                        for (var k = 0; k < line.length; k++) {
                            var e = line[k];

                            // do something faster instead, probably move it to another function
                            if (!e.part.top) {
                                e.part.line = j;
                                e.part.top = j * (this._resolved.eventHeight() + eventMarginTop) + eventMarginTop;
                                e.part.detachedBarTop = e.part.top - eventMarginTop;
                                e.part.height = this._resolved.eventHeight();
                                e.part.right = e.part.left + e.part.width;
                                e.part.fullTop = this.rows[e.part.dayIndex].Top + e.Top;
                                e.part.fullBottom = e.part.fullTop + e.part.height;
                            }
                            var rendered = this._drawEvent(e);

                            if (batch && rendered) {
                                step--;
                                // flush
                                if (step <= 0) {
                                    this.divEvents.style.visibility = '';
                                    this.divEvents.style.display = '';
                                    window.setTimeout(function() { calendar._drawEvents(batch); }, calendar.batch.delay);
                                    return;
                                }
                            }
                        }
                    }
                }
            }
            else {
                var eventCount = this.events.list.length;
                for (var i = 0; i < eventCount; i++) {
                    this._drawEvent(this.events.list[i]);
                }
            }
            
            this.divEvents.style.display = '';
            this._findEventsInViewPort();
            this._loadingStop();

        };

        this._drawEventsInRow = function(rowIndex) {
            // not hiding and showing this.divEvents, caused flickering

            var row = this.rows[rowIndex];

            // create new layer
            this.divEvents = document.createElement("div");
            this.divEvents.style.position = 'absolute';  // relative
            this.divEvents.style.display = 'none';
            //this.maind.appendChild(this.divEvents);
            
            this.maind.insertBefore(this.divEvents, this.divSeparatorsAbove);

            var eventMarginTop = this.durationBarDetached ? 10 : 0;

            for (var j = 0; j < row.lines.length; j++) {
                var line = row.lines[j];
                for (var k = 0; k < line.length; k++) {
                    var e = line[k];

                    // this must always be perfomed during row redrawing                    
                    e.part.line = j;
                    e.part.top = j * (this._resolved.eventHeight() + eventMarginTop) + eventMarginTop;
                    e.part.detachedBarTop = e.part.top - eventMarginTop;
                    e.part.height = this._resolved.eventHeight();
                    e.part.right = e.part.left + e.part.width;
                    e.part.fullTop = this.rows[e.part.dayIndex].Top + e.Top;
                    e.part.fullBottom = e.part.fullTop + e.part.height;

                    // batch rendering not supported here
                    this._drawEvent(e);
                }
            }
            this.divEvents.style.display = '';

            //this._findEventsInViewPort();
            //this.multiselect.redraw();
        };

        this._deleteEvents = function() {
            if (this.elements.events) {
                var length = this.elements.events.length;

                for (var i = 0; i < length; i++) {
                    var div = this.elements.events[i];
                    this._deleteEvent(div);
                }
            }
            this.elements.events = [];
        };

        this._deleteEventsInRow = function(rowIndex) {
            //var count = 0;
            if (this.elements.events) {
                var length = this.elements.events.length;
                var removed = [];

                for (var i = 0; i < length; i++) {
                    var div = this.elements.events[i];
                    if (div.row === rowIndex) {
                        this._deleteEvent(div);
                        removed.push(i);
                        //count += 1;
                    }
                }

                for (var i = removed.length - 1; i >= 0; i--) {
                    this.elements.events.splice(removed[i], 1);
                }
            }

        };

        this._deleteEvent = function(div) {

            // direct event handlers
            div.onclick = null;
            div.oncontextmenu = null;
            div.onmouseover = null;
            div.onmouseout = null;
            div.onmousemove = null;
            div.onmousedown = null;
            div.ondblclick = null;

            if (div.event) {
                if (!div.isBar) {
                    div.event.rendered = null;
                }
                div.event = null;
            }

            if (div.bar) {
                DayPilot.de(div.bar);
            }
            if (div.parentNode) { div.parentNode.removeChild(div); }
        };

        // deletes events that are out of the current view
        // keeps the last "keepPlus" number of events outside of the view
        this._deleteOldEvents = function(keepPlus) {
            if (!keepPlus) {
                keepPlus = 0;
            }
            
            if (this.dynamicEventRendering !== 'Progressive') {
                return;
            }

            this.divEvents.style.display = 'none';

            var updated = [];

            var deleted = 0;

            var length = this.elements.events.length;
            for (var i = length - 1; i >= 0; i--) {
                var div = this.elements.events[i];
                if (this._oldEvent(div.event)) {
                    if (keepPlus > 0) {
                        keepPlus--;
                        updated.unshift(div);
                    }
                    else {
                        this._deleteEvent(div);
                        deleted++;
                    }
                }
                else {
                    updated.unshift(div);
                }
            }

            this.elements.events = updated;
            
            this.divEvents.style.display = '';
        };
        
        this._deleteOldCells = function(keepPlus) {
            var updated = [];

            var deleted = 0;
            
            var area = this._getDrawArea();

            var length = this.elements.cells.length;
            for (var i = length - 1; i >= 0; i--) {
                var div = this.elements.cells[i];
                
                var visible = (area.xStart < div.coords.x && div.coords.x <= area.xEnd) && (area.yStart < div.coords.y && div.coords.y <= area.yEnd);
                
                if (!visible) {
                    if (keepPlus > 0) {
                        keepPlus--;
                        updated.unshift(div);
                    }
                    else {
                        this._deleteCell(div);
                        deleted++;
                    }
                }
                else {
                    updated.unshift(div);
                }
            }
            
        };
        
        this._deleteCell = function(div) {
            if (!div) {
                return;
            }
            var x = div.coords.x;
            var y = div.coords.y;

            // remove div
            var index = DayPilot.indexOf(calendar.elements.cells, div);
            calendar.elements.cells.splice(index, 1);
            if (div.parentNode) { div.parentNode.removeChild(div); }
            
            // remove from cache
            calendar._cache.cells[x + "_" + y] = null;
        };

        this._deleteSeparators = function() {
            if (this.elements.separators) {
                for (var i = 0; i < this.elements.separators.length; i++) {
                    var div = this.elements.separators[i];
                    DayPilot.pu(div); // not necessary
                    div.parentNode.removeChild(div);
                }
            }
            this.elements.separators = [];
        };


        this._hiddenEvents = function() {
            var dynamic = this.dynamicEventRendering === 'Progressive';

            if (!this.nav.scroll) {
                return false;
            }
            var top = this.nav.scroll.scrollTop;
            var bottom = top + this.nav.scroll.clientHeight;

            for (var i = 0; i < this.rows.length; i++) {

                var row = this.rows[i];

                var rowTop = row.Top;
                var rowBottom = row.Top + row.Height;
                if (dynamic && (bottom <= rowTop || top >= rowBottom)) {
                    continue;
                }

                for (var j = 0; j < row.lines.length; j++) {
                    var line = row.lines[j];
                    for (var k = 0; k < line.length; k++) {
                        var e = line[k];
                        if (this._hiddenEvent(e)) {
                            return true;
                        }
                    }
                }

            }

            return false;
        };

        this._hiddenEvent = function(data) {
            if (data.rendered) {
                return false;
            }

            var dynamic = this.dynamicEventRendering === 'Progressive';
            var left = this.nav.scroll.scrollLeft;
            var right = left + this.nav.scroll.clientWidth;
            var eventLeft = data.Left;
            var eventRight = data.Left + data.Width;
            if (dynamic && (right <= eventLeft || left >= eventRight)) {
                return false;
            }
            return true;
        };

        this._oldEvent = function(ev) {
            if (!ev.rendered) {  // just for the case, these events might not have Top defined
                return true;
            }

            var top = this.nav.scroll.scrollTop;
            var bottom = top + this.nav.scroll.clientHeight;
            var left = this.nav.scroll.scrollLeft;
            var right = left + this.nav.scroll.clientWidth;

            var eventLeft = ev.part.left;
            var eventRight = ev.part.right;
            var eventTop = ev.part.fullTop;
            var eventBottom = ev.part.fullBottom;

            if (right <= eventLeft || left >= eventRight) {
                return true;
            }

            if (bottom <= eventTop || top >= eventBottom) {
                return true;
            }

            return false;
        };

        // returns true if the event was actually rendered
        this._drawEvent = function(data) {
            if (data.rendered) {
                return false;
            }

            var dynamic = this.dynamicEventRendering === 'Progressive' && !this.dynamicLoading;

            /*
            var left = this.nav.scroll.scrollLeft - this.dynamicEventRenderingMargin;
            var right = left + this.nav.scroll.clientWidth + 2 * this.dynamicEventRenderingMargin;
            */
            var area = this._getDrawArea();
            var left = area.pixels.left;
            var right = area.pixels.right;

            var eventLeft = data.part.left;
            var eventRight = data.part.left + data.part.width;
            if (dynamic && (right <= eventLeft || left >= eventRight)) { // dynamic rendering, event outside of the current view
                return false;
            }

            var rowIndex = data.part.dayIndex;
            var borders = !this.cssOnly && this.eventBorderVisible;
            var width = data.part.width;
            var height = data.part.height;
            if (borders) {
                width -= 2;
                height -= 2;
            }
            
            var cache = data.cache || data.data;
            
            // make sure it's not negative
            width = Math.max(0, width);
            height = Math.max(0, height);

            var row = this.rows[rowIndex];
            if (row.Hidden) {
                return false;
            }

            // prepare the top position
            var rowTop = row.Top;
            //var line = data.part.top / this.eventHeight;

            var div = document.createElement("div");

            var barDetached = this.durationBarDetached;
            
            if (barDetached) {
                var bar = document.createElement("div");
                bar.style.position = 'absolute';
                bar.style.left = (data.part.left + data.part.barLeft) + 'px';
                bar.style.top = (rowTop + data.part.detachedBarTop) + 'px';
                bar.style.width = data.part.barWidth + 'px';
                bar.style.height = 5 + 'px';
                
                bar.style.backgroundColor = "red";
                div.bar = bar;
                
                // add it to the events collection
                this.elements.bars.push(bar);

                // draw the div
                this.divEvents.appendChild(bar);                
            }
            
            var top = rowTop + data.part.top;
            
            //alert("width: " + this.cellWidth);

            //div.data = data;
            div.style.position = 'absolute';
            div.style.left = data.part.left + 'px';
            div.style.top = (rowTop + data.part.top) + 'px';
            div.style.width = width + 'px';
            div.style.height = height + 'px';
            if (!this.cssOnly) {
                if (calendar.eventBorderVisible) {
                    div.style.border = '1px solid ' + (cache.borderColor || this.eventBorderColor);
                }
                div.style.backgroundColor = data.client.backColor();
                div.style.fontSize = this.eventFontSize;
                div.style.cursor = 'default';
                div.style.fontFamily = this.eventFontFamily;
                div.style.color = cache.fontColor || this.eventFontColor;  // TODO move inside Event as fontColor()

                if (cache.backImage && !this.durationBarVisible) {
                    div.style.backgroundImage = "url(" + cache.backImage + ")";
                    if (cache.backRepeat) {
                        div.style.backgroundRepeat = cache.backRepeat;
                    }
                }

                if (this._resolved.rounded()) {
                    div.style.MozBorderRadius = "5px";
                    div.style.webkitBorderRadius = "5px";
                    div.style.borderRadius = "5px";
                }

            }
            div.style.whiteSpace = 'nowrap';
            div.style.overflow = 'hidden';
            div.className = this.cssOnly ? this._prefixCssClass("_event") : this._prefixCssClass("event");
            if (cache.cssClass) {
                DayPilot.Util.addClass(div, cache.cssClass);
            }
            var lineClasses = true;
            if (lineClasses) {
                DayPilot.Util.addClass(div, this._prefixCssClass("_event_line" + data.part.line));
            }
            div.setAttribute("unselectable", "on");

            if (this.showToolTip && !this.bubble) {
                div.title = data.client.toolTip();
            }

            div.onmousemove = this._onEventMouseMove;
            div.onmouseout = this._onEventMouseOut;
            div.onmousedown = this._onEventMouseDown;
            
            div.ontouchstart = touch.onEventTouchStart;
            div.ontouchmove = touch.onEventTouchMove;
            div.ontouchend = touch.onEventTouchEnd;

            if (data.client.clickEnabled()) {
                div.onclick = this._onEventClick;
            }

            if (data.client.doubleClickEnabled()) {
                div.ondblclick = this._eventDoubleClickDispatch;
            }

            div.oncontextmenu = this._eventRightClickDispatch;

            var inside = [];
            var durationBarHeight = 0;

            // inner divs

            if (this.cssOnly) {
                var inner = document.createElement("div");
                inner.setAttribute("unselectable", "on");
                inner.className = calendar._prefixCssClass("_event_inner");
                inner.innerHTML = data.client.innerHTML();

                if (cache.backColor) {
                    inner.style.background = cache.backColor;
                    if (DayPilot.browser.ie9 || DayPilot.browser.ielt9) {
                        inner.style.filter = '';
                    }
                }
                if (cache.fontColor) {
                    inner.style.color = cache.fontColor;
                }
                if (cache.borderColor) {
                    inner.style.borderColor = cache.borderColor;
                }
                if (cache.backImage) {
                    inner.style.backgroundImage = "url(" + cache.backImage + ")";
                    if (cache.backRepeat) {
                        inner.style.backgroundRepeat = cache.backRepeat;
                    }
                }

                div.appendChild(inner);

                var startsHere = data.start().getTime() === data.part.start.getTime();
                var endsHere = data.end().getTime() === data.part.end.getTime();

                if (!startsHere) {
                    DayPilot.Util.addClass(div, this._prefixCssClass("_event_continueleft"));
                }
                if (!endsHere) {
                    DayPilot.Util.addClass(div, this._prefixCssClass("_event_continueright"));
                }

                if (data.client.barVisible() && width > 0) {
                    var barLeft = 100 * data.part.barLeft / (width); // %
                    var barWidth = Math.ceil(100 * data.part.barWidth / (width)); // %

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
                    barInner.style.left = barLeft + "%";
                    //barInner.setAttribute("barWidth", data.part.barWidth);  // debug
                    if (0 < barWidth && barWidth <= 1) {
                        barInner.style.width = "1px";
                    }
                    else {
                        barInner.style.width = barWidth + "%";
                    }
                    
                    if (cache.barColor) {
                        barInner.style.backgroundColor = cache.barColor;
                    }

                    bar.appendChild(barInner);
                    div.appendChild(bar);
                }
            }
            else {
                if (data.client.barVisible()) {
                    durationBarHeight = calendar.durationBarHeight;

                    // white space left
                    inside.push("<div unselectable='on' style='left:0px;background-color:white;width:");
                    inside.push(data.part.barLeft);
                    inside.push("px;height:2px;font-size:1px;position:absolute'></div>");

                    // white space right
                    inside.push("<div unselectable='on' style='left:");
                    inside.push(data.part.barLeft + data.part.barWidth);
                    inside.push("px;background-color:white;width:");
                    inside.push(width - (data.part.barLeft + data.part.barWidth));
                    inside.push("px;height:2px;font-size:1px;position:absolute'></div>");

                    if (this.durationBarMode === "Duration") {
                        inside.push("<div unselectable='on' style='width:");
                        inside.push(data.part.barWidth);
                        inside.push("px;margin-left:");
                        inside.push(data.part.barLeft);
                        inside.push("px;height:");
                    }
                    else {
                        inside.push("<div unselectable='on' style='width:");
                        inside.push(cache.complete);
                        inside.push("%;margin-left:0px;height:");
                    }
                    inside.push(durationBarHeight - 1);
                    inside.push("px;background-color:");
                    inside.push(data.client.barColor());

                    if (cache.barImageUrl) {
                        inside.push(";background-image:url(");
                        inside.push(cache.barImageUrl);
                        inside.push(")");
                    }

                    inside.push(";font-size:1px;position:relative'></div>");
                    inside.push("<div unselectable='on' style='width:");
                    inside.push(width);
                    inside.push("px;height:1px;background-color:");
                    inside.push((cache.borderColor || this.eventBorderColor));
                    inside.push(";font-size:1px;overflow:hidden;position:relative'></div>");
                }

                inside.push("<div unselectable='on' style='padding-left:1px;width:");
                inside.push(width - 1);
                inside.push("px;height:");
                inside.push(height - durationBarHeight);
                inside.push("px;");
                if (calendar.rtl) {
                    inside.push("direction:rtl;");
                }
                if (cache.backImage && this.durationBarVisible) {
                    inside.push("background-image:url(");
                    inside.push(cache.backImage);
                    inside.push(");");
                    if (cache.backRepeat) {
                        inside.push("background-repeat:");
                        inside.push(cache.backRepeat);
                        inside.push(";");
                    }
                }
                inside.push("'>");
                inside.push(data.client.innerHTML());
                inside.push("</div>");

                div.innerHTML = inside.join('');
            }
            div.row = rowIndex;

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

            // add it to the events collection
            this.elements.events.push(div);

            // draw the div
            this.divEvents.appendChild(div);

            data.rendered = true;

            // init code for the event
            //div.event = new DayPilotScheduler.Event(div.data, calendar);
            div.event = data;

            if (calendar.multiselect._shouldBeSelected(div.event)) {
                calendar.multiselect.add(div.event, true);
                calendar.multiselect._update(div);
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

            return true;
        };
        
        this._api2 = function() {
            return calendar.api === 2;
        };

        this._updateEventTops = function() {
            for (var i = 0; i < this.elements.events.length; i++) {
                var div = this.elements.events[i];
                var event = div.event;
                var rowIndex = event.part.dayIndex;
                var row = this.rows[rowIndex];
                var rowTop = row.Top;
                div.style.top = (rowTop + event.part.top) + 'px';
                
                /*
                if (div.bar) {
                    div.bar.style.top = (rowTop + event.part.top + 10) + 'px'; // HACK
                }*/
            }
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

        this._onEventMouseOut = function(ev) {
            var div = this;

            DayPilot.Areas.hideAreas(div, ev);

            if (calendar.cssOnly) {
                DayPilot.Util.removeClass(div, calendar._prefixCssClass("_event_hover"));
            }

            //div.active = false;

            if (calendar.bubble && calendar.eventHoverHandling === 'Bubble') {
                calendar.bubble.hideOnMouseOut();
            }

        };

        this._onEventMouseMove = function(ev) {
            ev = ev || window.event;

            if (calendar.cellBubble) { calendar.cellBubble.delayedHide(); }

            var div = this;
            while (div && !div.event) { // make sure it's the top event div
                div = div.parentNode;
            }

            calendar._eventUpdateCursor(div, ev);

            if (!div.active) {
                DayPilot.Areas.showAreas(div, div.event);
                if (calendar.cssOnly) {
                    DayPilot.Util.addClass(div, calendar._prefixCssClass("_event_hover"));
                }
            }

            if (ev.srcElement) {
                ev.srcElement.insideEvent = true;
            }
            else {
                ev.insideEvent = true;
            }

            // bubbling must be alowed, required for moving and resizing 
            //ev.cancelBubble = true;

        };
        
        
        this._moving = {};
        var moving = this._moving;

        this._onEventMouseDown = function(ev) {
            calendar._out();
            /*
            calendar._crosshairHide();
            calendar._stopScroll();
            */
            
            if (typeof DayPilot.Bubble !== 'undefined') {
                DayPilot.Bubble.hideActive();
            }

            ev = ev || window.event;
            var button = ev.which || ev.button;
            
            if (button === 1) {
                if (this.style.cursor === 'w-resize' || this.style.cursor === 'e-resize') {

                    // set
                    DayPilotScheduler.resizing = this;
                    DayPilotScheduler.originalMouse = DayPilot.mc(ev);

                    // cursor
                    document.body.style.cursor = this.style.cursor;
                }
                else if (this.style.cursor === 'move') {
                    //var mv = this;
                    DayPilotScheduler.moving = this;
                    DayPilotScheduler.originalMouse = DayPilot.mc(ev);

                    DayPilotScheduler.moveOffsetX = DayPilot.mo3(this, ev).x;
                    DayPilotScheduler.moveDragStart = calendar.getDate(calendar.coords.x, true);
                    
                    // cursor
                    document.body.style.cursor = 'move';
                }
                else if (calendar.moveBy === 'Full' && this.event.client.moveEnabled()) {
                    moving.start = true;
                    moving.moving = this;
                    moving.originalMouse = DayPilot.mc(ev);
                    moving.moveOffsetX = DayPilot.mo3(this, ev).x;
                    moving.moveDragStart = calendar.getDate(calendar.coords.x, true);
                }
            }

            ev.cancelBubble = true;

            return false;
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
                
                calendar.coords = touch.relativeCoords(ev);
                touch.startMoving(div, ev);

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
            touch.timeouts.push(window.setTimeout(function() {
                touch.active = true;
                touch.start = false;

                ev.preventDefault();

                calendar.coords = touch.relativeCoords(ev);
                touch.range = calendar._rangeFromCoords();
            }, holdfor));
        };
        
        touch.onMainTouchMove = function(ev) {
            touch.clearTimeouts();

            touch.start = false;

            if (touch.active) {
                ev.preventDefault();
                
                calendar.coords = touch.relativeCoords(ev);

                if (DayPilotScheduler.moving) {
                    touch.updateMoving();
                    return;
                }

                if (touch.range) {
                    var range = touch.range;
                    range.end = {
                        x: Math.floor(calendar.coords.x / calendar.cellWidth)
                    };

                    calendar._drawRange(range);
                }
            }
            
        };
        
        touch.onMainTouchEnd = function(ev) {
            touch.clearTimeouts();
            
            if (touch.active) {
                if (DayPilotScheduler.moving) {
                    ev.preventDefault();

                    var e = DayPilotScheduler.moving.event;

                    var newStart = DayPilotScheduler.movingShadow.start;
                    var newEnd = DayPilotScheduler.movingShadow.end;
                    var newResource = (calendar.viewType !== 'Days') ? DayPilotScheduler.movingShadow.row.Value : null;
                    var external = DayPilotScheduler.drag ? true : false;
                    //var line = DayPilotScheduler.movingShadow.line;

                    // clear the moving state            
                    DayPilot.de(DayPilotScheduler.movingShadow);
                    calendar._clearShadowHover();
                    DayPilotScheduler.movingShadow.calendar = null;
                    document.body.style.cursor = '';
                    DayPilotScheduler.moving = null;
                    DayPilotScheduler.movingShadow = null;

                    calendar._eventMoveDispatch(e, newStart, newEnd, newResource, external);

                }

                if (touch.range) {
                    var sel = calendar._getSelection(touch.range);
                    touch.range = null;
                    calendar._timeRangeSelectedDispatch(sel.start, sel.end, sel.resource);
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
            var ref = calendar.maind;
            
            var x = ev.touches[0].pageX;
            var y = ev.touches[0].pageY;

            var abs = DayPilot.abs(ref);
            var coords = {x: x - abs.x, y: y - abs.y, toString: function() { return "x: " + this.x + ", y:" + this.y; } };
            return coords;
        };
        
        // coords - page coords
        touch.startMoving = function(div, ev) {
            var coords = { x: ev.touches[0].pageX, y : ev.touches[0].pageY };

            DayPilotScheduler.moving = div;
            DayPilotScheduler.originalMouse = coords;
            
            var absE = DayPilot.abs(div);
            DayPilotScheduler.moveOffsetX = coords.x - absE.x;
            
            //var absR = DayPilot.abs(calendar.maind);
            //var x = coords.x - absR.x;
            DayPilotScheduler.moveDragStart = calendar.getDate(calendar.coords.x, true);
            
            DayPilotScheduler.movingShadow = calendar._createShadow(div, calendar.shadow);
            
            // update dimensions
            calendar._moveShadow();

        };
        
        // coords - relative to maind
        touch.updateMoving = function() {
            if (DayPilotScheduler.movingShadow && DayPilotScheduler.movingShadow.calendar !== calendar) {
                DayPilotScheduler.movingShadow.calendar = null;
                DayPilot.de(DayPilotScheduler.movingShadow);
                DayPilotScheduler.movingShadow = null;
            }
            if (!DayPilotScheduler.movingShadow) {
                var mv = DayPilotScheduler.moving;
                DayPilotScheduler.movingShadow = calendar._createShadow(mv, calendar.shadow);
            }

            DayPilotScheduler.moving.target = calendar; //might not be necessary, the target is in DayPilotScheduler.activeCalendar
            calendar._moveShadow();
        };

        this._eventUpdateCursor = function(div, ev) {

/*
            if (calendar.moveBy === "Disabled" || calendar.moveBy === "None") {
                return;
            }
*/
            // const
            var resizeMargin = this.eventResizeMargin;
            var moveMargin = this.eventMoveMargin;

            var object = div;

            if (typeof (DayPilotScheduler) === 'undefined') {
                return;
            }

            // position
            var offset = DayPilot.mo3(div, ev);
            if (!offset) {
                //document.title = "null";
                return;
            }

            //document.title = "offset:" + offset.x + "," + offset.y;

            calendar.eventOffset = offset;

            if (DayPilotScheduler.resizing) {
                return;
            }

            var isFirstPart = div.getAttribute("dpStart") === div.getAttribute("dpPartStart");
            var isLastPart = div.getAttribute("dpEnd") === div.getAttribute("dpPartEnd");


            // top
            if (calendar.moveBy === 'Top' && offset.y <= moveMargin && object.event.client.moveEnabled() && calendar.eventMoveHandling !== 'Disabled') {  // TODO disabled check not necessary
                if (isFirstPart) {
                    div.style.cursor = 'move';
                }
                else {
                    div.style.cursor = 'not-allowed';
                }
            }
            // left resizing
            else if ((calendar.moveBy === 'Top' || calendar.moveBy === 'Full') && offset.x <= resizeMargin && object.event.client.resizeEnabled() && calendar.eventResizeHandling !== 'Disabled') {  // TODO disabled check not necessary
                if (isFirstPart) {
                    div.style.cursor = "w-resize";
                    div.dpBorder = 'left';
                }
                else {
                    div.style.cursor = 'not-allowed';
                }
            }
            // left moving
            else if (calendar.moveBy === 'Left' && offset.x <= moveMargin && object.event.client.moveEnabled() && calendar.eventMoveHandling !== 'Disabled') {  // TODO disabled check not necessary
                if (isFirstPart) {
                    div.style.cursor = "move";
                }
                else {
                    div.style.cursor = 'not-allowed';
                }
            }
            // right
            else if (div.offsetWidth - offset.x <= resizeMargin && object.event.client.resizeEnabled() && calendar.eventResizeHandling !== 'Disabled') {  // TODO disabled check not necessary
                if (isLastPart) {
                    div.style.cursor = "e-resize";
                    div.dpBorder = 'right';
                }
                else {
                    div.style.cursor = 'not-allowed';
                }
            }
            else if (!DayPilotScheduler.resizing && !DayPilotScheduler.moving) {
                if (object.event.client.clickEnabled() && calendar.eventClickHandling !== 'Disabled') {  // TODO disabled check not necessary
                    div.style.cursor = 'pointer';
                }
                else {
                    div.style.cursor = 'default';
                }
            }

            
            if (typeof (DayPilotBubble) !== 'undefined' && calendar.bubble && calendar.eventHoverHandling === 'Bubble') {
                if (div.style.cursor === 'default' || div.style.cursor === 'pointer') {
                    // preventing Chrome bug
                    var notMoved = this._lastOffset && offset.x === this._lastOffset.x && offset.y === this._lastOffset.y;
                    if (!notMoved) {
                        this._lastOffset = offset;
                        calendar.bubble.showEvent(div.event);
                    }
                }
                else {
                    /*
                    // disabled, now it is hidden on click
                    DayPilotBubble.hideActive();
                    */
                }
            }
        };
        
        this._cellCount = function() {
            if (this.viewType !== 'Days') {
                return this.itline.length;

                // TODO broken for nonbusiness
                //return Math.floor(this.days * 24 * 60 / this.cellDuration);
            }
            else {
                return Math.floor(24 * 60 / this.cellDuration);
            }
        };

        this._getSelection = function(range) {
            //var range = DayPilotScheduler.range;

            var range = range || DayPilotScheduler.range || DayPilotScheduler.rangeHold;

            if (!range) {
                return null;
            }

            var row = calendar.rows[range.start.y];

            if (!row) {
                return null;
            }

            var resource = row.Value;
            var startX = range.end.x > range.start.x ? range.start.x : range.end.x;
            var endX = (range.end.x > range.start.x ? range.end.x : range.start.x);

            var rowOffset = row.Start.getTime() - this._visibleStart().getTime();

            var start = this.itline[startX].start.addTime(rowOffset);
            var end = this.itline[endX].end.addTime(rowOffset);

            return new DayPilot.Selection(start, end, resource, calendar);
        };

        this._createEdit = function(object) {
            var parentTd = object.parentNode;

            var edit = document.createElement('textarea');
            edit.style.position = 'absolute';
            edit.style.width = ((object.offsetWidth < 100) ? 100 : (object.offsetWidth - 2)) + 'px';
            edit.style.height = (object.offsetHeight - 2) + 'px'; //offsetHeight
            edit.style.fontFamily = DayPilot.gs(object, 'fontFamily') || DayPilot.gs(object, 'font-family');
            edit.style.fontSize = DayPilot.gs(object, 'fontSize') || DayPilot.gs(object, 'font-size');
            edit.style.left = object.offsetLeft + 'px';
            edit.style.top = object.offsetTop + 'px';
            edit.style.border = '1px solid black';
            edit.style.padding = '0px';
            edit.style.marginTop = '0px';
            edit.style.backgroundColor = 'white';
            edit.value = DayPilot.tr(object.event.text());

            edit.event = object.event;
            parentTd.appendChild(edit);
            return edit;
        };


        this._divEdit = function(object) {
            if (DayPilotScheduler.editing) {
                DayPilotScheduler.editing.blur();
                return;
            }

            var edit = this._createEdit(object);
            DayPilotScheduler.editing = edit;

            edit.onblur = function() {
                //var id = object.event.value();
                //var tag = object.event.tag();
                var oldText = object.event.text();
                var newText = edit.value;

                DayPilotScheduler.editing = null;
                if (edit.parentNode) {
                    edit.parentNode.removeChild(edit);
                }

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
                    DayPilotScheduler.editing = false;
                }

                return true;
            };

            edit.select();
            edit.focus();
        };

        this._onResMouseMove = function(ev) {
            var td = this;
            if (typeof (DayPilotBubble) !== 'undefined') {
                if (calendar.cellBubble) {
                    calendar.cellBubble.hideOnMouseOut();
                }
                if (calendar.resourceBubble) {
                    var res = {};
                    res.calendar = calendar;
                    res.id = td.getAttribute("resource");
                    res.toJSON = function() {
                        var json = {};
                        json.id = this.id;
                        return json;
                    };
                    calendar.resourceBubble.showResource(res);
                }
            }

            var div = td.firstChild; // rowheader
            if (!div.active) {
                DayPilot.Areas.showAreas(div, calendar.rows[td.index]); // TODO replace with custom object
            }

        };

        this._onResMouseOut = function(ev) {
            var td = this;
            if (typeof (DayPilotBubble) !== 'undefined' && calendar.resourceBubble) {
                calendar.resourceBubble.hideOnMouseOut();
            }

            var div = td.firstChild;

            DayPilot.Areas.hideAreas(div, ev);
            div.data = null;
        };

/*
        this._onCellMouseOut = function() {
            if (typeof (DayPilotBubble) !== 'undefined' && calendar.cellBubble) {
                calendar.cellBubble.hideOnMouseOut();
            }
        };
*/
/*
        this._updateHeaders = function() {
            var inner = [];

            inner.push("<table cellspacing='0' cellpadding='0' style='color:");
            inner.push(calendar.headerFontColor);
            inner.push(";'><tr>");
            inner.push(calendar.row1);
            inner.push("</tr><tr>");
            inner.push(calendar.row2);
            inner.push("</tr></table>");

            DayPilot.puc(calendar.divNorth);
            calendar.divNorth.innerHTML = inner.join('');

            var corner = calendar.divCorner;
            if (!this.cssOnly) {
                corner.style.backgroundColor = calendar.cornerBackColor;
            }
            corner.innerHTML = calendar.cornerHTML;
        };
*/
        this._drawTimeHeader = function() {
            
            this._drawTimeHeader2();
            return;
/*
            var table = document.createElement("table");
            table.cellSpacing = 0;
            table.cellPadding = 0;
            if (!this.cssOnly) {
                table.className = this._prefixCssClass("timeheader");
            }
            table.style.borderCollapse = "collapse";
            if (!this.cssOnly) {
                table.style.color = calendar.headerFontColor;
                table.style.border = "0px none";
            }

            for (var i = 0; i < this.timeHeader.length - 1; i++) {
                var row1 = table.insertRow(-1);
                this._drawTimeHeaderGroups(row1, i);
            }

            var row2 = table.insertRow(-1);
            this._drawTimeHeaderCols(row2);

            var north = this.divNorth;
            DayPilot.puc(north);
            north.innerHTML = '';
            north.appendChild(table);
            north.style.width = (this._cellCount() * this.cellWidth + 5000) + "px";

            var corner = this.divCorner;
            if (!this.cssOnly) {
                corner.style.backgroundColor = this.cornerBackColor;
            }
            if (this.cornerHtml) {
                corner.innerHTML = this.cornerHtml;
            }
            else {
                corner.innerHTML = '';
            }

            this.divStretch.style.width = (this._getGridWidth()) + "px";
*/
        };

        this._drawTimeHeader2 = function() {
            
            if (!this.timeHeader) {
                calendar.debug.message("drawTimeHeader: timeHeader not available");
                return; // mvc shortInit
            }
            
            this._cache.timeHeader = {};
            
            var header = document.createElement("div");
            header.style.position = "relative";
            this.nav.timeHeader = header;

            for (var y = 0; y < this.timeHeader.length; y++) {
                var row = this.timeHeader[y];
                for (var x = 0; x < row.length; x++) {
                    this._drawTimeHeaderCell2(x, y);
                }
            }

            var north = this.divNorth;
            DayPilot.puc(north);
            north.innerHTML = '';
            north.appendChild(header);
            //north.style.width = (this._cellCount() * this.cellWidth + 5000) + "px";
            var gridwidth = this._getGridWidth();
            north.style.width = (this._getGridWidth() + 5000) + "px";

            var corner = this.divCorner;
            if (!this.cssOnly) {
                corner.style.backgroundColor = this.cornerBackColor;
            }
            if (this.cornerHtml) {
                corner.innerHTML = this.cornerHtml;
            }
            else {
                corner.innerHTML = '';
            }

            var gridwidth = this._getGridWidth();
            
            if (gridwidth > 0) {
                this.divStretch.style.width = (this._getGridWidth()) + "px";
            }

        };

/*
        this._drawTimeHeaderGroups = function(row, index) {
            if (this.timeHeader) {
                if (this.timeHeader.length - 1 <= index) {
                    throw "Not enough timeHeader rows";
                }
                for (var i = 0; i < this.timeHeader[index].length; i++) {
                    // TODO don't change the timeHeader?
                    var cell = this.timeHeader[index][i];
                    if (!cell.toolTip) {
                        cell.toolTip = cell.innerHTML;
                    }
                    if (!this.cssOnly) {
                        if (!cell.backColor) {
                            cell.backColor = this.hourNameBackColor;
                        }
                    }
                    if (!cell.colspan) {
                        cell.colspan = Math.ceil(cell.width / (1.0 * this.cellWidth));
                    }
                    // should be already there
                    cell.level = index;
                    this._drawTimeHeaderCell(row, cell);
                }
            }
            else {
                throw ".groupline is not used anymore";
            }
        };
*/
        this._getGroupName = function(h, cellGroupBy) {
            var html = null;
            var locale = this._resolved.locale();

            var cellGroupBy = cellGroupBy || this.cellGroupBy;
            
            var from = h.start;
            var to = h.end;
            var locale = this._resolved.locale();

            switch (cellGroupBy) {
                case 'Hour':
                    html = from.toString("H"); // TODO format 
                    break;
                case 'Day':
                    html = from.toString(locale.datePattern);
                    break;
                case 'Week':
                    html = resolved.weekStarts() === 1 ? from.weekNumberISO() : from.weekNumber(); // TODO format 
                    break;
                case 'Month':
                    html = from.toString("MMMM yyyy", locale);
                    break;
                case 'Year':
                    html = from.toString("yyyy");
                    break;
                case 'None':
                    html = '';
                    break;
                case 'Cell':
                    if (this.scale === 'Manual' || this.scale === 'CellDuration') {  // hard-to-guess cell sizes
                        var duration = (h.end.ticks - h.start.ticks) / 60000;
                        html = this._getCellName(from, duration);
                    }
                    else {
                        html = this._getGroupName(h, this.scale);
                    }
                    break;
                default:
                    throw 'Invalid cellGroupBy value';
            }

            return html;
        };
        
        this._getCellName = function(start, duration) {
            var locale = this._resolved.locale();
            var duration = duration || this.cellDuration;
            if (duration < 60) // smaller than hour, use minutes
            {
                return start.toString("mm"); //String.Format("{0:00}", from.Minute);
            }
            else if (duration < 1440) // smaller than day, use hours
            {
                //return DayPilot.Date.hours(start.d, calendar._resolved.timeFormat() === 'Clock12Hours');
                return calendar._resolved.timeFormat() === 'Clock12Hours' ? start.toString("H tt", locale) : start.toString("H", locale);
            }
            else if (duration < 10080) // use days
            {
                return start.toString("d");
            }
            else if (duration === 10080) {
                return resolved.weekStarts() === 1 ? start.weekNumberISO() : start.weekNumber(); // TODO format 
            }
            else
            {
                return start.toString("MMMM yyyy", locale);
            }
        };
        
        this._addScaleSize = function(from) {
            var scale = this.scale;
            switch (scale) {
                case "Cell":
                    throw "Invalid scale: Cell";
                case "Manual":
                    throw "Internal error (addScaleSize in Manual mode)";
                case "Minute":
                    return from.addMinutes(1);
                case "CellDuration":
                    return from.addMinutes(this.cellDuration);
                default:
                    return this._addGroupSize(from, scale);
            }
        };

        this._addGroupSize = function(from, cellGroupBy) {
            var daysHorizontally = this.viewType !== 'Days' ? this.days : 1;
            var endDate = this.startDate.addDays(daysHorizontally);

            var cellGroupBy = cellGroupBy || this.cellGroupBy;
            var cellDuration = 60; // dummy value to make sure it's aligned properly
			
            switch (cellGroupBy) {
                case 'Hour':
                    to = from.addHours(1);
                    break;
                case 'Day':
                    to = from.addDays(1);
                    break;
                case 'Week':
                    to = from.addDays(1);
                    while (to.dayOfWeek() !== resolved.weekStarts()) {
                        to = to.addDays(1);
                    }
                    break;
                case 'Month':
                    to = from.addMonths(1);
                    to = to.firstDayOfMonth();

                    //var minDiff = 
                    var isInt = (DayPilot.Date.diff(to.d, from.d) / (1000.0 * 60)) % cellDuration === 0;
                    while (!isInt) {
                        to = to.addHours(1);
                        isInt = (DayPilot.Date.diff(to.d, from.d) / (1000.0 * 60)) % cellDuration === 0;
                    }
                    break;
                case 'Year':
                    to = from.addYears(1);
                    to = to.firstDayOfYear();

                    var isInt = (DayPilot.Date.diff(to.d, from.d) / (1000.0 * 60)) % cellDuration === 0;
                    while (!isInt) {
                        to = to.addHours(1);
                        isInt = (DayPilot.Date.diff(to.d, from.d) / (1000.0 * 60)) % cellDuration === 0;
                    }
                    break;
                case 'None':
                    to = endDate;
                    break;
                case 'Cell':
                    var cell = this._getItlineCellFromTime(from);
                    if (cell.current)
                    {
                        to = cell.current.end;
                    }
                    else
                    {
                        if (cell.past) {
                            to = cell.previous.end;
                        }
                        else {
                            to = cell.next.start;
                        }
                        /*
                        var cursor = from;
                        while (cell === null)
                        {
                            cursor = cursor.addMinutes(1);
                            cell = this._getItlineCellFromTime(cursor);
                        }
                        to = cell.start;
                                                */
                    }
                    break;
                default:
                    throw 'Invalid cellGroupBy value';
            }
            if (to.getTime() > endDate.getTime()) {
                to = endDate;
            }

            return to;
        };
        
        this._drawTimeHeaderCell2 = function(x, y) {
            
            var header = this.nav.timeHeader;
            
            var p = this.timeHeader[y][x];
            
            var isGroup = y < this.timeHeader.length - 1;
            var top = y * resolved.headerHeight();
            var left = p.left;
            var width = p.width;
            var height = resolved.headerHeight();
            
            var cell = document.createElement("div");
            cell.style.position = "absolute";
            cell.style.top = top + "px";
            cell.style.left = left + "px";
            cell.style.width = width + "px";
            cell.style.height = height + "px";
            if (p.toolTip) {
                cell.title = p.toolTip;
            }
            
            cell.setAttribute("unselectable", "on");
            cell.style.KhtmlUserSelect = 'none';
            cell.style.MozUserSelect = 'none';
            cell.style.webkitUserSelect = 'none';
            
            cell.oncontextmenu = function() { return false; };
            cell.cell = {};
            cell.cell.start = p.start;
            cell.cell.end = p.end;
            cell.cell.level = y;
            cell.cell.th = p;
            cell.onclick = this._onTimeClick;

            cell.style.overflow = 'hidden';

            if (!this.cssOnly) {
                var isLast = y === this.timeHeader.length - 1;
                cell.style.textAlign = "center";
                cell.style.backgroundColor = (typeof cell.backColor === 'undefined') ? calendar.hourNameBackColor : cell.backColor;
                cell.style.fontFamily = this.hourFontFamily;
                cell.style.fontSize = this.hourFontSize;
                cell.style.color = this.headerFontColor;
                cell.style.cursor = 'default';
                cell.style.border = '0px none';
                if (!isLast) {
                    cell.style.height = (height - 1) + "px";
                    cell.style.borderBottom = "1px solid " + this.borderColor;
                }
                cell.style.width = (width - 1) + "px";
                cell.style.borderRight = "1px solid " + this.hourNameBorderColor;
                cell.style.whiteSpace = 'nowrap';
                cell.className = this._prefixCssClass('timeheadergroup');
            }
            
            var inner = document.createElement("div");
            inner.setAttribute("unselectable", "on");
            inner.innerHTML = p.innerHTML;
            
            if (p.backColor) {
                inner.style.background = p.backColor;
            }

            if (this.cssOnly) {
                var cl = this._prefixCssClass("_timeheadercol");
                var cli = this._prefixCssClass("_timeheadercol_inner");
                if (isGroup) {
                    cl = this._prefixCssClass("_timeheadergroup");
                    cli = this._prefixCssClass("_timeheadergroup_inner");
                }
                DayPilot.Util.addClass(cell, cl);
                DayPilot.Util.addClass(inner, cli);
            }
            
            cell.appendChild(inner);
            
            this._cache.timeHeader[x + "_" + y] = cell;
            
            header.appendChild(cell);
        };

/*
        this._drawTimeHeaderCell = function(row, cell) {
            var td = row.insertCell(-1);
            td.colSpan = cell.colspan;
            td.setAttribute("unselectable", "on");
            var height = Math.max(0, resolved.headerHeight());
            if (!this.cssOnly) {
                td.style.height = (height - 1) + "px";
                td.style.textAlign = "center";
                td.style.backgroundColor = cell.backColor;
                td.style.fontFamily = this.hourFontFamily;
                td.style.fontSize = this.hourFontSize;
                td.style.color = this.headerFontColor;
                td.style.cursor = 'default';
                td.style.border = '0px none';
                td.style.borderBottom = "1px solid " + this.borderColor;
                td.style.padding = '0px';
            }
            else {
                td.style.height = (height) + "px";
            }
            td.style.KhtmlUserSelect = 'none';
            td.style.MozUserSelect = 'none';
            td.style.webkitUserSelect = 'none';
            td.style.whiteSpace = 'nowrap';
            //td.style.overflow = 'hidden';
            if (cell.width) {
                td.style.width = cell.width + "px";
            }
            if (!this.cssOnly) {
                td.className = this._prefixCssClass('timeheadergroup');
            }
            td.oncontextmenu = function() { return false; };
            td.cell = cell;
            td.onclick = this._onTimeClick;

            var target = null;
            var div = document.createElement("div");
            div.setAttribute("unselectable", "on");
            if (!this.cssOnly) {
                div.style.borderRight = "1px solid " + this.hourNameBorderColor;
                div.style.width = (cell.width - 1) + "px";
                div.style.height = (height - 1) + "px";
                target = div;
            }
            else {
                div.className = this._prefixCssClass('_timeheadergroup');
                if (cell.backColor) {
                    div.style.background = cell.backColor;
                }
                div.style.position = "relative";
                if (cell.width) {
                    div.style.width = (cell.width) + "px";
                }
                div.style.height = (height) + "px";

                var inner = document.createElement("div");
                inner.setAttribute("unselectable", "on");
                inner.className = this._prefixCssClass("_timeheadergroup_inner");
                div.appendChild(inner);
                target = inner;
            }
            div.style.overflow = 'hidden';
            div.title = cell.toolTip;

            target.innerHTML = cell.innerHTML;

            td.appendChild(div);
        };
*/
/*
        this._drawTimeHeaderCols = function(row) {
            var cellCount = this._cellCount();

            var td = document.createElement("td");
            td.setAttribute("unselectable", "on");
            if (!this.cssOnly) {
                td.style.borderTop = "0px none";
                td.style.borderBottom = "0px none";
                td.style.borderLeft = "0px none";
                td.style.borderRight = "0px none";
                td.style.textAlign = 'center';
                td.style.fontFamily = this.hourFontFamily;
                td.style.fontSize = this.hourFontSize;
                td.style.color = this.headerFontColor;
                td.style.cursor = 'default';
                td.style.padding = '0px';
            }
            td.style.width = this.cellWidth + "px";
            td.style.height = (resolved.headerHeight()) + "px";
            td.style.overflow = 'hidden';
            td.style.KhtmlUserSelect = 'none';
            td.style.MozUserSelect = 'none';
            td.style.webkitUserSelect = 'none';
            if (!this.cssOnly) {
                td.className = this._prefixCssClass('timeheadercol');
            }

            //var target = null;
            var div = document.createElement("div");
            div.setAttribute("unselectable", "on");
            div.style.height = (resolved.headerHeight()) + "px";
            div.style.overflow = 'hidden';
            div.style.position = 'relative';
            if (this.cssOnly) {
                div.style.width = (this.cellWidth) + "px";
                div.className = this._prefixCssClass('_timeheadercol');
            }
            else {
                div.style.borderRight = "1px solid " + this.hourNameBorderColor;
                div.style.width = (this.cellWidth - 1) + "px";
            }

            var inner = document.createElement("div");
            inner.setAttribute("unselectable", "on");
            if (this.cssOnly) {
                inner.className = this._prefixCssClass("_timeheadercol_inner");
            }
            div.appendChild(inner);

            td.appendChild(div);

            var index = this.timeHeader.length - 1;

            for (var i = 0; i < cellCount; i++) {
                var cell;
                if (this.timeHeader) {
                    cell = this.timeHeader[index][i];
                    if (!cell.backColor) {
                        if (!this.cssOnly) {
                            cell.backColor = this.hourNameBackColor;
                        }
                    }
                    if (!cell.toolTip) {
                        cell.toolTip = cell.innerHTML;
                    }
                }
                // TODO remove, this.timeHeader is now always available
                else {
                    //var from = this.startDate.addMinutes(this.cellDuration * i);
                    //var to = from.addMinutes(this.cellDuration);
                    var c = this.itline[i];

                    var html = this._getColName(c.start);

                    var cell = {};
                    cell.innerHTML = html;
                    cell.toolTip = html;
                    cell.start = c.start;
                    cell.end = c.end;
                    cell.type = 'Cell';
                    cell.backColor = this.hourNameBackColor;

                    var returned = this.beforeTimeHeaderRender ? this.beforeTimeHeaderRender(cell) : null;
                    if (returned) {
                        cell = returned;
                    }
                }
                cell.level = index;

                var thisTd = td.cloneNode(true);
                if (!this.cssOnly) {
                    thisTd.style.backgroundColor = cell.backColor;
                }
                else {
                    if (cell.backColor) {
                        thisTd.firstChild.style.backgroundColor = cell.backColor;
                    }
                }
                thisTd.firstChild.title = cell.toolTip;
                thisTd.firstChild.firstChild.innerHTML = cell.innerHTML;
                thisTd.oncontextmenu = function() { return false; };

                thisTd.cell = cell;
                thisTd.onclick = this._onTimeClick;

                row.appendChild(thisTd);
            }
        };
*/

/*
        // TODO remove
        this._getColName = function(from) {
            var html = null;
            if (this.cellDuration < 60) {
                html = from.getMinutes();
            }
            else if (this.cellDuration < 1440) {
                html = from.getHours();
            }
            else {
                html = from.getDay();
            }
            return html;
        };
*/
        this._updateRowHeights = function() {
            for (var i = 0; i < this.rows.length; i++) {
                var row = this.rows[i];
                var updated = row.height() + row.MarginBottom;
                if (row.Height !== updated) {
                    this.rowsDirty = true;
                }
                row.Height = updated;
            }
        };

        this._updateRowHeaderHeights = function() {
            var header = this.divHeader;

            if (!header) {
                return false;
            }

            var len = this.rows.length;

            var columns = this.rowHeaderCols ? this.rowHeaderCols.length : 1;
            //var updated = false;

            // row headers
            var j = 0; // real TR index
            for (var i = 0; i < len; i++) {
                var row = this.rows[i];
                if (row.Hidden) {
                    continue;
                }

                for (var c = 0; c < header.rows[j].cells.length; c++) {
                    var headerCell = header.rows[j].cells[c];
                    var newHeight = row.Height;

                    if (headerCell && headerCell.firstChild && parseInt(headerCell.firstChild.style.height, 10) !== newHeight) {
                        headerCell.firstChild.style.height = newHeight + "px";
                    }
                }

                j++;
            }

        };

        this._drawSeparator = function(index) {
            var s = this.separators[index];
            
            
            // fix
            s.location = s.location || s.Location;
            s.color = s.color || s.Color;
            s.layer = s.layer || s.Layer;
            s.width = s.width || s.Width;
            s.opacity = s.opacity || s.Opacity;

            var time = new DayPilot.Date(s.location);
            var color = s.color;
            var width = s.width ? s.width : 1;
            var above = s.layer ? s.layer === 'AboveEvents' : false;
            var opacity = s.opacity ? s.opacity : 100;

            // check the start and end dates of the visible area
            if (time.getTime() < this.startDate.getTime()) {
                return;
            }
            if (time.getTime() >= this.startDate.addDays(this.days).getTime()) {
                return;
            }

            var pixels = this.getPixels(time);

            // check if it's in the hidden area, don't show in that case
            if (pixels.cut) {
                return;
            }

            if (pixels.left < 0) {
                return;
            }
            if (pixels.left > this._cellCount() * this.cellWidth) {
                return;
            }

            var line = document.createElement("div");
            line.style.width = width + 'px';
            line.style.height = calendar.innerHeightTree + 'px';
            line.style.position = 'absolute';
            line.style.left = (pixels.left - 1) + 'px'; 
            line.style.top = '0px';
            line.style.backgroundColor = color;
            line.style.opacity = opacity / 100;
            line.style.filter = "alpha(opacity=" + opacity + ")";

            if (above) {
                this.divSeparatorsAbove.appendChild(line);
            }
            else {
                this.divSeparators.appendChild(line);
            }

            this.elements.separators.push(line);
        };


        this._onMaindDblClick = function(ev) {
            if (calendar.timeRangeDoubleClickHandling === 'Disabled') {
                return false;
            }

            if (DayPilotScheduler.timeRangeTimeout) {
                clearTimeout(DayPilotScheduler.timeRangeTimeout);
                DayPilotScheduler.timeRangeTimeout = null;
            }

            var range = {};

            // make sure that coordinates are set                
            if (!calendar.coords) {
                var ref = calendar.maind;
                calendar.coords = DayPilot.mo3(ref, ev);
            }

            ev = ev || window.event;
            //var button = ev.which || ev.button;

            // only process left and right button outside of selection
            if (calendar._isWithinRange(calendar.coords)) {
                var sel = calendar._getSelection(DayPilotScheduler.rangeHold);
                calendar._timeRangeDoubleClickDispatch(sel.start, sel.end, sel.resource);
            }
            else {
                DayPilotScheduler.range = calendar._rangeFromCoords();
                var sel = calendar._getSelection(DayPilotScheduler.range);
                calendar._timeRangeDoubleClickDispatch(sel.start, sel.end, sel.resource);
            }

            DayPilotScheduler.rangeHold = DayPilotScheduler.range;
            DayPilotScheduler.range = null;

        };

/*
        this._clearDblClickTimeout = function() {
            calendar.dblclick = null;
        };
*/
        // handles:
        // - TimeRangeSelected
        this._onMaindMouseDown = function(ev) {
            
            if (touch.start) {
                return;
            }

            if (DayPilotScheduler.timeRangeTimeout && false) {
                clearTimeout(DayPilotScheduler.timeRangeTimeout);
                DayPilotScheduler.timeRangeTimeout = null;
            }

            calendar._crosshairHide();
            calendar._stopScroll();

            // make sure that coordinates are set                
            if (!calendar.coords) {
                var ref = calendar.maind;
                calendar.coords = DayPilot.mo3(ref, ev);
            }

            ev = ev || window.event;
            var button = ev.which || ev.button;

            if (button === 2 || (button === 3 && calendar._isWithinRange(calendar.coords))) {
                return false;
            }

            if (calendar.timeRangeSelectedHandling === 'Disabled') {
                return false;
            }

            DayPilotScheduler.range = calendar._rangeFromCoords();

            return false; // prevent FF3 bug (?), dragging is otherwise activated and DayPilot.mo2 gives incorrect results
        };

        // creates a single cell range selection at the current position (calendar.coords)
        this._rangeFromCoords = function() {
            var range = {};
            
            var cx = this._getItlineCellFromPixels(calendar.coords.x).x;

            range.start = {
                y: calendar._getRow(calendar.coords.y).i,
                x: cx
            };

            range.end = {
                x: cx
            };

            if (this._isRowDisabled(calendar._getRow(calendar.coords.y).i)) {
                return;
            }

            //return false;

            range.calendar = calendar;
            //DayPilotScheduler.range = range;

            calendar._drawRange(range);
            
            return range;
        };

        // handles:
        // - EventMove (including external)
        // - EventResize
        // - TimeRangeSelected
        //
        // saves calendar.coords
        this._onMaindMouseMove = function(ev) {
            if (touch.active) {
                return;
            }

            DayPilotScheduler.activeCalendar = calendar; // required for moving
            ev = ev || window.event;
            var mousePos = DayPilot.mc(ev);

            calendar.coords = DayPilot.mo3(calendar.maind, ev);
            
            if (moving.start) {
                if (moving.originalMouse.x !== mousePos.x || moving.originalMouse.y !== mousePos.y) {
                    DayPilot.Util.copyProps(moving, DayPilotScheduler);
                    document.body.style.cursor = 'move';
                    moving = {};
                }
            }

            if (DayPilotScheduler.resizing) {
                if (!DayPilotScheduler.resizingShadow) {
                    DayPilotScheduler.resizingShadow = calendar._createShadow(DayPilotScheduler.resizing, calendar.shadow);
                }
                var _step = DayPilotScheduler.resizing.event.calendar.cellWidth;
                var originalWidth = DayPilotScheduler.resizing.event.part.width;
                var originalLeft = DayPilotScheduler.resizing.event.part.left;
                var _startOffset = 0;
                var delta = (mousePos.x - DayPilotScheduler.originalMouse.x);
                
                if (DayPilotScheduler.resizing.dpBorder === 'right') {
                    var newWidth;
                    if (calendar.snapToGrid) {
                        //newWidth = Math.ceil(((originalWidth + originalLeft + delta)) / _step) * _step - originalLeft;
                        var itc =  calendar._getItlineCellFromPixels(originalWidth + originalLeft + delta).cell;
                        var newRight = itc.left + itc.width;
                        newWidth = newRight - originalLeft;
                        
                        if (newWidth < _step) {
                            newWidth = _step;
                        }
                        
                    }
                    else {
                        newWidth = originalWidth + delta;
                    }


                    var max = calendar._getGridWidth();
                    
                    if (originalLeft + newWidth > max) {
                        newWidth = max - originalLeft;
                    }

                    DayPilotScheduler.resizingShadow.style.width = (newWidth) + 'px';
                }
                else if (DayPilotScheduler.resizing.dpBorder === 'left') {
                    var newLeft;
                    if (calendar.snapToGrid) {
                        newLeft = Math.floor(((originalLeft + delta) + 0) / _step) * _step;
                        if (newLeft < _startOffset) {
                            newLeft = _startOffset;
                        }
                    }
                    else {
                        newLeft = originalLeft + delta;
                    }

                    var newWidth = originalWidth - (newLeft - originalLeft);
                    var right = originalLeft + originalWidth;

                    if (calendar.snapToGrid) {
                        if (newWidth < _step) {
                            newWidth = _step;
                            newLeft = right - newWidth;
                        }
                    }

                    DayPilotScheduler.resizingShadow.style.left = newLeft + 'px';
                    DayPilotScheduler.resizingShadow.style.width = (newWidth) + 'px';
                }
            }
            else if (DayPilotScheduler.moving) {
                if (DayPilotScheduler.movingShadow && DayPilotScheduler.movingShadow.calendar !== calendar) {
                    DayPilotScheduler.movingShadow.calendar = null;
                    DayPilot.de(DayPilotScheduler.movingShadow);
                    DayPilotScheduler.movingShadow = null;
                }
                if (!DayPilotScheduler.movingShadow) {
                    var mv = DayPilotScheduler.moving;
                    DayPilotScheduler.movingShadow = calendar._createShadow(mv, calendar.shadow);
                }
                
                calendar._expandParent();

                DayPilotScheduler.moving.target = calendar; //might not be necessary, the target is in DayPilotScheduler.activeCalendar
                calendar._moveShadow();
            }
            else if (DayPilotScheduler.range) {

                var range = DayPilotScheduler.range;
                range.end = {
                    //x: Math.floor(calendar.coords.x / calendar.cellWidth)
                    x: calendar._getItlineCellFromPixels(calendar.coords.x).x
                };

                calendar._drawRange(range);
            }
            else if (calendar.crosshairType !== 'Disabled') {  // crosshair
                calendar._updateCrosshairPosition();
            }
            
            calendar._cellhover();

            var insideEvent = ev.insideEvent;
            if (window.event) {
                insideEvent = window.event.srcElement.insideEvent;
            }

            // cell bubble            
            if (calendar.cellBubble && calendar.coords && calendar.rows && calendar.rows.length > 0 && !insideEvent) {
                var x = Math.floor(calendar.coords.x / calendar.cellWidth);
                var y = calendar._getRow(calendar.coords.y).i;
                if (y < calendar.rows.length) {
                    var cell = {};
                    cell.calendar = calendar;
                    cell.start = calendar.itline[x].start;
                    cell.end = calendar.itline[x].end;
                    cell.resource = calendar.rows[y].Value;
                    cell.toJSON = function() {
                        var json = {};
                        json.start = this.start;
                        json.end = this.end;
                        json.resource = this.resource;
                        return json;
                    };

                    calendar.cellBubble.showCell(cell);
                }
            }

            if (DayPilotScheduler.drag) {
                calendar._crosshairHide();
                if (DayPilotScheduler.gShadow) {
                    document.body.removeChild(DayPilotScheduler.gShadow);
                }
                DayPilotScheduler.gShadow = null;

                if (!DayPilotScheduler.movingShadow && calendar.coords && calendar.rows.length > 0) {
                    //if (DayPilotScheduler.movingShadow) { // can be null if the location is forbidden (first two rows in IE)
                    if (!DayPilotScheduler.moving) { // can be null if the location is forbidden (first two rows in IE)
                        DayPilotScheduler.moving = {};

                        var event = DayPilotScheduler.drag.event;
                        if (!event) {
                            //var now = new DayPilot.Date().getDatePart();
                            var now = calendar.itline[0].start;
                            calendar.debug.message("external start:" + now);
                            var ev = { 'id': DayPilotScheduler.drag.id, 'start': now, 'end': now.addSeconds(DayPilotScheduler.drag.duration), 'text': DayPilotScheduler.drag.text };
                            event = new DayPilot.Event(ev);
                            event.calendar = calendar;
                        }
                        DayPilotScheduler.moving.event = event;
                    }
                    //DayPilotScheduler.movingShadow = calendar.createShadow(DayPilotScheduler.drag.duration, calendar.shadow, DayPilotScheduler.drag.shadowType);
                    //DayPilotScheduler.movingShadow = calendar.createShadow(calendar.shadow, DayPilotScheduler.drag.shadowType);
                    DayPilotScheduler.movingShadow = calendar._createShadow(DayPilotScheduler.moving, DayPilotScheduler.drag.shadowType);
                }

                ev.cancelBubble = true;
            }

            // autoscroll
            if (calendar.autoScroll === "Always" ||
                    (calendar.autoScroll === "Drag" && (DayPilotScheduler.moving || DayPilotScheduler.resizing || DayPilotScheduler.range))
                    ) {
                var scrollDiv = calendar.nav.scroll;
                var coords = { x: calendar.coords.x, y: calendar.coords.y };
                coords.x -= scrollDiv.scrollLeft;
                coords.y -= scrollDiv.scrollTop;

                var width = scrollDiv.clientWidth;
                var height = scrollDiv.clientHeight;

                var border = 20;

                var left = coords.x < border;
                var right = width - coords.x < border;

                var top = coords.y < border;
                var bottom = height - coords.y < border;

                var x = 0;
                var y = 0;

                if (left) {
                    x = -5;
                }
                if (right) {
                    x = 5;
                }
                if (top) {
                    y = -5;
                }
                if (bottom) {
                    y = 5;
                }


                if (x || y) {
                    calendar._startScroll(x, y);
                }
                else {
                    calendar._stopScroll();
                }
            }

            // don't cancel the event bubbling here, it will hurt position detection used in DayPilot ContextMenu and DayPilot Bubble
            //ev.cancelBubble = true;
        };
        
        this._cellhover = function() {
            var x, y;
            if (calendar.coords && calendar.rows && calendar.rows.length > 0) {
                x = calendar._getItlineCellFromPixels(calendar.coords.x).x;
                y = calendar._getRow(calendar.coords.y).i;
                if (y >= calendar.rows.length) {
                    return;
                }
            }
            else {
                return;
            }

            var row = this._getRowByIndex(y);
            var itc = this.itline[x];
            
            var cell = {};
            cell.x = x;
            cell.y = y;
            cell.start = itc.start;
            cell.end = itc.end;
            cell.resource = row ? row.data.Value : null;

            if (this.hover.cell) {
                if (this.hover.cell.x === cell.x && this.hover.cell.y === cell.y) {
                    return;
                }
                this._cellhoverout();
            }
            
            this.hover.cell = cell;
            
            if (typeof this.onCellMouseOver === 'function') {
                var args = {};
                args.cell = cell;
                this.onCellMouseOver(args);
            }

        };
        
        this._cellhoverout = function() {
            if (typeof this.onCellMouseOut === 'function') {
                var args = {};
                args.cell = this.hover.cell;
                this.onCellMouseOut(args);
            }
            this.hover.cell = null;
        };
        
        this.hover = {};

        this._updateCrosshairPosition = function() {
            this._crosshair();
        };

        this._crosshairHide = function() {
            this.divCrosshair.innerHTML = '';
            this.crosshairVertical = null;
            this.crosshairHorizontal = null;

            if (this.crosshairTop && this.crosshairTop.parentNode) {
                this.crosshairTop.parentNode.removeChild(this.crosshairTop);
                this.crosshairTop = null;
            }

            if (this.crosshairLeft) {
                for (var i = 0; i < this.crosshairLeft.length; i++) {
                    var ch = this.crosshairLeft[i];
                    if (ch.parentNode) {
                        ch.parentNode.removeChild(ch);
                    }
                }
                this.crosshairLeft = null;
            }

            this.crosshairLastX = -1;
            this.crosshairLastY = -1;
        };

        this._crosshair = function() {
            var x, y;
            if (calendar.coords && calendar.rows && calendar.rows.length > 0) {
                x = calendar._getItlineCellFromPixels(calendar.coords.x).x;
                y = calendar._getRow(calendar.coords.y).i;
                if (y >= calendar.rows.length) {
                    return;
                }
            }
            else {
                return;
            }
            
            var type = this.crosshairType;

            var row = this._getRowByIndex(y);

            if (type === 'Full') {
                // vertical
                var itc = this.itline[x];
                
                var left = itc.left;

                var line = this.crosshairVertical;
                if (!line) {
                    var line = document.createElement("div");
                    line.style.height = calendar.innerHeightTree + 'px';
                    line.style.position = 'absolute';
                    line.style.top = '0px';
                    line.style.backgroundColor = this.crosshairColor;
                    line.style.opacity = this.crosshairOpacity / 100;
                    line.style.filter = "alpha(opacity=" + this.crosshairOpacity + ")";
                    this.crosshairVertical = line;
                    this.divCrosshair.appendChild(line);
                }

                line.style.left = left + 'px';
                line.style.width = itc.width + 'px';

                // horizontal        
                var top = row.top;
                var height = row.height;
                //var width = this._cellCount() * this.cellWidth;
                var width = this._getGridWidth();

                var line = this.crosshairHorizontal;
                if (!line) {
                    var line = document.createElement("div");
                    line.style.width = width + 'px';
                    line.style.height = height + 'px';
                    line.style.position = 'absolute';
                    line.style.top = top + 'px';
                    line.style.left = '0px';
                    line.style.backgroundColor = this.crosshairColor;
                    line.style.opacity = this.crosshairOpacity / 100;
                    line.style.filter = "alpha(opacity=" + this.crosshairOpacity + ")";
                    this.crosshairHorizontal = line;
                    this.divCrosshair.appendChild(line);
                }

                //document.title = line.style.left + line.style.top + line.style.width + line.style.height;
                line.style.top = top + 'px';
                line.style.height = height + 'px';

            }

            var thc = this._getTimeHeaderCell(this.coords.x);
            if (thc && this.crosshairLastX !== thc.x) {
                if (this.crosshairTop && this.crosshairTop.parentNode) {
                    this.crosshairTop.parentNode.removeChild(this.crosshairTop);
                    this.crosshairTop = null;
                }
                
                // top
                var line = document.createElement("div");
                line.style.width = thc.cell.width + "px";
                line.style.height = resolved.headerHeight() + "px";
                line.style.left = '0px';
                line.style.top = '0px';
                line.style.position = 'absolute';
                line.style.backgroundColor = this.crosshairColor;
                line.style.opacity = this.crosshairOpacity / 100;
                line.style.filter = "alpha(opacity=" + this.crosshairOpacity + ")";

                this.crosshairTop = line;
                var north = this.divNorth;
                var lastHeader = this.timeHeader ? this.timeHeader.length - 1 : 1;
                if (this.nav.timeHeader) {
                    this._cache.timeHeader[thc.x + "_" + lastHeader].appendChild(line);
                }
                else {
                    if (north.firstChild.rows[lastHeader].cells[x]) {
                        north.firstChild.rows[lastHeader].cells[x].firstChild.appendChild(line);
                    }
                }
            }

            if (this.crosshairLastY !== y) {

                if (this.crosshairLeft) {
                    for (var i = 0; i < this.crosshairLeft.length; i++) {
                        var ch = this.crosshairLeft[i];
                        if (ch.parentNode) {
                            ch.parentNode.removeChild(ch);
                        }
                    }
                    this.crosshairLeft = null;
                }

                // left    
                var columns = this.rowHeaderCols ? this.rowHeaderCols.length : 1;

                this.crosshairLeft = [];
                for (var i = 0; i < this.divHeader.rows[row.i].cells.length; i++) {
                    //var width = this.rowHeaderCols ? this.rowHeaderCols[i] : this.rowHeaderWidth;
                    var width = this.divHeader.rows[row.i].cells[i].clientWidth;

                    var line = document.createElement("div");
                    line.style.width = width + "px";
                    line.style.height = row.height + "px";
                    line.style.left = '0px';
                    line.style.top = '0px';
                    line.style.position = 'absolute';
                    line.style.backgroundColor = this.crosshairColor;
                    line.style.opacity = this.crosshairOpacity / 100;
                    line.style.filter = "alpha(opacity=" + this.crosshairOpacity + ")";

                    this.crosshairLeft.push(line);
                    this.divHeader.rows[row.i].cells[i].firstChild.appendChild(line);
                }
            }

            if (thc) {
                this.crosshairLastX = thc.x;
            }
            this.crosshairLastY = y;
        };
        
        this._getTimeHeaderCell = function(pixels) {
            var last = this.timeHeader[this.timeHeader.length - 1];
            for (var i = 0; i < last.length; i++) {
                var cell = last[i];
                if (pixels >= cell.left && pixels < cell.left + cell.width) {
                    var result = {};
                    result.cell = cell;
                    result.x = i;
                    return result;
                }
            }
            return null;
        };

        this._onMaindRightClick = function(ev) {

            ev = ev || window.event;

            if (calendar.timeRangeSelectedHandling === 'Disabled') {
                return false;
            }

            if (!calendar._isWithinRange(calendar.coords)) {
                calendar._onMaindClick(ev);
            }

            if (calendar.contextMenuSelection) {
                var selection = calendar._getSelection(DayPilotScheduler.rangeHold);
                calendar.contextMenuSelection.show(selection);
            }

            ev.cancelBubble = true;
            
            if (!calendar.allowDefaultContextMenu) {
                return false;
            }
        };

        this._isWithinRange = function(coords) {
            var range = DayPilotScheduler.rangeHold;

            if (!range || !range.start || !range.end) {
                return false;
            }

            var row = this._getRowByIndex(range.start.y);

            var leftToRight = range.start.x < range.end.x;

            var rangeLeft = (leftToRight ? range.start.x : range.end.x) * this.cellWidth;
            var rangeRight = (leftToRight ? range.end.x : range.start.x) * this.cellWidth + this.cellWidth;
            var rangeTop = row.top;
            var rangeBottom = row.bottom;

            if (coords.x >= rangeLeft && coords.x <= rangeRight && coords.y >= rangeTop && coords.y <= rangeBottom) {
                return true;
            }

            return false;
        };

        this._drawRange = function(range) {
            var range = range || DayPilotScheduler.range;

            var startX = range.end.x > range.start.x ? range.start.x : range.end.x;
            var endX = (range.end.x > range.start.x ? range.end.x : range.start.x) + 1;

            this._deleteRange();

            for (var x = startX; x < endX; x++) {
                this._drawRangeCell(x, range.start.y);
            }

        };

        this._onMaindClick = function(ev) {
            
            if (calendar.timeRangeSelectedHandling === 'Disabled') {
                return false;
            }

            ev = ev || window.event;
            var button = ev.which || ev.button;

            if (DayPilotScheduler.range) { // time range selecting already active
                return;
            }

            if (DayPilotScheduler.rangeHold && calendar._isWithinRange(calendar.coords) && (button === 3 || button === 2)) {
                return;
            }

            var range = {};
            
            var cx = this._getItlineCellFromPixels(calendar.coords.x).x;

            range.start = {
                y: calendar._getRow(calendar.coords.y).i,
                x: cx
            };

            range.end = {
                x: cx
            };

            calendar._drawRange(range);

            var sel = calendar._getSelection(range);
            calendar._timeRangeSelectedDispatch(sel.start, sel.end, sel.resource);

            // TEST the default behavior is now Hold
            DayPilotScheduler.rangeHold = range;

        };

/*
        this.cellClick = function() {
            var o = this.style.backgroundColor;
            this.style.backgroundColor = '#316AC5';

            calendar._timeRangeSelectedDispatch(this.start, this.end, this.row);
            this.style.backgroundColor = o;
        };
*/

        this.timeouts = {};
        this.timeouts.drawEvents = null;
        this.timeouts.drawCells = null;
        this.timeouts.click = null;

        this._onScroll = function(ev) {
            calendar._clearCachedValues();
            
            if (calendar.dynamicLoading) {
                calendar._onScrollDynamic();
                return;
            }
            var divScroll = calendar.nav.scroll;

            calendar.scrollPos = divScroll.scrollLeft;
            calendar.scrollTop = divScroll.scrollTop;
            calendar.scrollWidth = divScroll.clientWidth;

            calendar.divTimeScroll.scrollLeft = calendar.scrollPos;
            calendar.divResScroll.scrollTop = calendar.scrollTop;

            
            if (calendar.timeouts.drawEvents) {
                clearTimeout(calendar.timeouts.drawEvents);
            }
            calendar.timeouts.drawEvents = setTimeout(calendar._delayedDrawEvents(), 200);
            
            /*
            if (calendar.timeouts.drawCells) {
                clearTimeout(calendar.timeouts.drawCells);
            }
            calendar.timeouts.drawCells = setTimeout(calendar._delayedDrawCells(), 50);
*/

/*
            if (calendar.refreshTimeout) {
                clearTimeout(calendar.refreshTimeout);
            }
            calendar.refreshTimeout = setTimeout(calendar._delayedRefresh(), 500);
*/

            calendar._saveState();
            calendar._drawCells();
            
            // _findEventsInViewPort() will be called once again as soon as the events are updated [ in _delayedRefresh() ]
            //calendar._findEventsInViewPort();
            /*
            if (calendar.timeouts.updateSections) {
                clearTimeout(calendar.timeouts.updateSections);
            }
            calendar.timeouts.updateSections = setTimeout(function() {
                calendar._updateSections();
            }, 50);
            */

            calendar._updateFloats();

            calendar.onScrollCalled = true;
        };

        this._delayedRefresh = function() {
            return function() {
                calendar._saveState();
                calendar._drawCells();
                calendar.refreshTimeout = window.setTimeout(calendar._delayedDrawEvents(), 200); // chain update
            };
        };

        this._delayedDrawCells = function() {
            return function() {
                calendar._saveState();
                calendar._drawCells();
            };
        };


        this._delayedDrawEvents = function() {
            var batch = true; // turns on batch rendering
            var deleteOld = calendar.dynamicEventRenderingCacheSweeping;  // deletes old events (outside of the visible area)
            var keepOld = calendar.dynamicEventRenderingCacheSize;  // how many old events should be kept visible (cached)

            return function() {
                if (calendar._hiddenEvents()) {
                    calendar._loadingStart();
                    
                    window.setTimeout(function() {
                        if (deleteOld) calendar._deleteOldEvents(keepOld);
                        window.setTimeout(function() { calendar._drawEvents(batch); }, 50);
                    }, 50);
                }
                else {
                    calendar._findEventsInViewPort();
                }
            };
        };
        
        this._clearCachedValues = function() {
            this._cache.eventHeight = null;
            this._cache.drawArea = null;
        };
        
        this.show = function() {
            calendar.nav.top.style.display = '';
            this._resize();
            calendar._onScroll();
        };
        
        this.hide = function() {
            calendar.nav.top.style.display = 'none';
        };

        this._onScrollDynamic = function() {
            var divScroll = calendar.nav.scroll;

            calendar.scrollPos = divScroll.scrollLeft;
            calendar.scrollTop = divScroll.scrollTop;
            calendar.scrollWidth = divScroll.clientWidth;

            calendar.divTimeScroll.scrollLeft = calendar.scrollPos;
            calendar.divResScroll.scrollTop = calendar.scrollTop;

            if (calendar.refreshTimeout) {
                window.clearTimeout(calendar.refreshTimeout);
            }

            var delay = calendar.scrollDelay || 500;
            calendar.refreshTimeout = window.setTimeout(calendar._delayedRefreshDynamic(divScroll.scrollLeft, divScroll.scrollTop), delay);
            
            calendar._updateFloats();
        };
        
        this._findEventInList = function(e) {
            for (var j = 0; j < this.events.list.length; j++) {
                var ex = this.events.list[j];
                if (ex.id === e.id && ex.start.toString() === e.start.toString() && ex.resource === e.resource) {
                    var result = {};
                    result.ex = ex;
                    result.index = j;
                    result.modified = !calendar._equalObjectFlat(e, ex);
                    return result;
                }
            }
            return null;
        };
        
        this._equalObjectFlat = function(first, second) {
            for (var name in first) {
                if (first[name] !== second[name]) {
                    return false;
                }
            }
            
            for (var name in second) {
                if (first[name] !== second[name]) {
                    return false;
                }
            }
            
            return true;
        };
        
        this._loadEventsDynamic = function(supplied, finished) {
            var updatedRows = [];
            
            for (var i = 0; i < supplied.length; i++) {
                var e = supplied[i];
                
                var found = calendar._findEventInList(e);
                
                var update = found && found.modified;
                var add = !found;

                if (update) {
                    // update it directly in list
                    this.events.list[found.index] = e;

                    // remove it from rows
                    var rows = calendar.events._removeFromRows(found.ex);
                    updatedRows = updatedRows.concat(rows);
                }
                else if (add) {
                    this.events.list.push(e);
                }
                
                if (update || add) {
                    updatedRows = updatedRows.concat(calendar.events._addToRows(e));
                }
            }
            
            calendar._loadRows(updatedRows);
            calendar._updateHeight();
            calendar._updateRowsNoLoad(updatedRows, false, finished);
        
        };

        this._delayedRefreshDynamic = function(scrollX, scrollY) {
            if (!calendar._serverBased()) {
                return function() {
                    if (typeof calendar.onScroll === 'function') {
                        // make sure the background is rendered immediately
                        calendar._drawCells();
                        
                        var update = function(events) {
                            //var updatedRows = calendar._loadEvents(events, true);
                            var finished = function() {
                                if (calendar._api2()) {
                                    if (typeof calendar.onAfterRender === 'function') {
                                        var args = {};
                                        args.isCallBack = false;
                                        args.isScroll = true;
                                        args.data = null;

                                        calendar.onAfterRender(args);
                                    }
                                }
                            };
                            
                            calendar._loadEventsDynamic(events, finished);
                        };
                        
                        var area = calendar._getArea(scrollX, scrollY);
                        var range = calendar._getAreaRange(area);
                        var res = calendar._getAreaResources(area);

                        var args = {};
                        args.viewport = {};
                        args.viewport.start = range.start;
                        args.viewport.end = range.end;
                        args.viewport.resources = res;
                        args.async = false;
                        args.events = [];
                        args.loaded = function() {
                            if (this.async) {
                                update(this.events);
                            }
                        };

                        calendar.onScroll(args);

                        if (!args.async) {
                            update(args.events);
                        }
                        
                    }
                };
            }
            else {
                return function() {
                    calendar.scrollX = scrollX;
                    calendar.scrollY = scrollY;
                    calendar._callBack2('Scroll');
                };
            }
        };

/*
        this._delayedRefresh = function() {
            return function() {
                
                // moved to ._onScroll directly for faster feedback
                //calendar._drawCells();
                calendar.refreshTimeout = window.setTimeout(calendar._delayedDrawEvents(), 200); // chain update
            };
        };
*/

        this._drawCellsFull = function() {
            var area = this._getDrawArea();
            
            var cellLeft = area.xStart;
            var cellWidth = area.xEnd - area.xStart;
            var cellTop = area.yStart;
            var cellHeight = area.yEnd - area.yStart;
            
            // initialize for client-side processing
            //if (typeof this.onBeforeCellRender === 'function') {
                if (!this.cellProperties) {
                    this.cellProperties = [];
                }
            //}
            
            //this.elements.cells = [];
            //this.elements.linesVertical = [];
            for (var i = 0; i < cellWidth; i++) {
                var x = cellLeft + i;
                for (var j = 0; j < cellHeight; j++) {
                    var y = cellTop + j;
                    if (!this.rows[y].Hidden) {
                        this._drawCell(x, y);
                    }
                }
                this._drawLineVertical(x);
            }

            // full height
            for (var y = 0; y < this.rows.length; y++) {
                if (!this.rows[y].Hidden) {
                    this._drawLineHorizontal(y);
                }
            }

        };
        
        this.sweepCells = true;

        this._drawCells = function() {
            if (this.rows !== null && this.rows.length > 0) {
                
                //var handlerExists = typeof this.onBeforeCellRender === 'function';
                //var dirtyColors = this.cellConfig && !this.cellConfig.vertical;
                
                if (this.sweepCells) {
                    var keepOld = 0;
                    this._deleteOldCells(keepOld);
                }

                this._drawCellsFull();
/*
                if (dirtyColors || this.hasChildren || handlerExists) {
                    this._drawCellsFull();
                }
                else {
                    this._drawCellsFast();
                }
*/
                this._drawTimeBreaks();

            }

            var width = this._getGridWidth();

            this.maind.style.height = this.innerHeightTree + "px";
            this.maind.style.width = width + "px";

            this.rowsDirty = false;

        };

        this._drawTimeBreaks = function() {

            //this.elements.cells = [];  // just to make sure
            var area = this._getDrawArea();

            for (var x = area.xStart; x < area.xEnd; x++) {
                var breaks = (x < this.itline.length - 1) ? this.itline[x + 1].breakBefore : false;
                if (breaks) {
                    this._drawTimeBreak(x);
                }
            }
        };

        this._drawTimeBreak = function(x) {
            var index = "x" + x;
            if (this._cache.breaks[index]) {
                return;
            }

            //var left = x * this.cellWidth + this.cellWidth - 1;
            var left = this.itline[x + 1].left - 1;
            var height = this.innerHeightTree;

            var line = document.createElement("div");
            line.style.left = left + "px";
            line.style.top = "0px";
            line.style.width = "1px";
            line.style.height = height + "px";
            line.style.fontSize = '1px';
            line.style.lineHeight = '1px';
            line.style.overflow = 'hidden';
            line.style.position = 'absolute';
            line.setAttribute("unselectable", "on");
            //cell.className = this._prefixCssClass('cellbackground');

            if (this.cssOnly) {
                line.className = this._prefixCssClass("_matrix_vertical_break");
                //line.className = this._prefixCssClass("_matrix
            }
            else {
                line.style.backgroundColor = this.timeBreakColor;
            }

            this.divBreaks.appendChild(line);
            this.elements.breaks.push(line);

            this._cache.breaks[index] = line;

        };

        this._getDrawArea = function() {
            
            if (calendar._cache.drawArea) {
                return calendar._cache.drawArea;
            }
            
            if (!this.nav.scroll) {
                return null;
            }
            
            // const
            var preCache = 30;
            var scrollTop = calendar.scrollTop;

            var area = {};

            //var divScroll = calendar.divScroll;
            var visibleLeft = Math.floor(this.scrollPos / this.cellWidth);  // first visible index column
            var visibleWidth = Math.ceil(this.scrollWidth / this.cellWidth) + 1; // number of columns visible
            var totalWidth = this._cellCount();
            var start = visibleLeft - preCache; // pre-caching one screen on each side
            var end = start + 2 * preCache + visibleWidth;
            end = Math.min(end, totalWidth); // make sure it's within the boundaries
            start = Math.max(start, 0); // check the left side

            var cellTop = this._getRow(scrollTop).i;
            var cellBottom = this._getRow(scrollTop + this.nav.scroll.offsetHeight).i;
            if (cellBottom < this.rows.length) {
                cellBottom++;
            }
            var cellHeight = cellBottom - cellTop; // unused

            area.xStart = start;
            area.xEnd = end;
            area.yStart = cellTop;
            area.yEnd = cellBottom;
            
            area.pixels = {};
            area.pixels.left = this.nav.scroll.scrollLeft;
            area.pixels.right = this.nav.scroll.scrollLeft + this.nav.scroll.clientWidth;
            area.pixels.top = this.nav.scroll.scrollTop;
            area.pixels.bottom = this.nav.scroll.scrollTop + this.nav.scroll.clientHeight;
            area.pixels.width = this.nav.scroll.scrollWidth;
            
            calendar._cache.drawArea = area;
            
            return area;
        };

        this._getGridWidth = function() {
            var result = 0;
            if (this.viewType === "Days") {
                result = 24*60/this.cellDuration*this.cellWidth;
            }
            else {
                var last = this.itline[this.itline.length - 1];
                if (!last) {
                    result = 0;
                }
                else {
                    result = last.left + last.width;
                }
            }
            /*
            if (isNaN(result)) {
                alert("problem with width: " + result);
            }
            */
            if (result < 0 || isNaN(result)) {
                result = 0;
            }
            return result;
        };

        this._drawLineHorizontal = function(y) {
            var index = "y" + y;

            if (this._cache.linesHorizontal[y]) {
                calendar.debug.message("skiping horiz line: " + y);
                return;
            }

            var top = this.rows[y].Top + this.rows[y].Height - 1;
            //var width = this._cellCount() * this.cellWidth;
            var width = this._getGridWidth();

            var line = document.createElement("div");
            line.style.left = "0px";
            line.style.top = top + "px";
            line.style.width = width + "px";
            line.style.height = "1px";
            line.style.fontSize = '1px';
            line.style.lineHeight = '1px';
            line.style.overflow = 'hidden';
            line.style.position = 'absolute';
            if (!this.cssOnly) {
                line.style.backgroundColor = this.cellBorderColor;
            }
            line.setAttribute("unselectable", "on");
            if (this.cssOnly) {
                line.className = this._prefixCssClass("_matrix_horizontal_line");
            }

            this.divLines.appendChild(line);
            //this.elements.cells.push(line);

            this._cache.linesHorizontal[index] = line;

        };

        this._drawLineVertical = function(x) {

            var itc = this.itline[x];
            if (!itc) {
                return;
            }
            
            var index = "x" + x;
            if (this._cache.linesVertical[index]) {
                return;
            }

            //var left = (x + 1) * this.cellWidth - 1;
            var left = itc.left + itc.width - 1;

            var line = document.createElement("div");
            line.style.left = left + "px";
            line.style.top = "0px";
            line.style.width = "1px";
            line.style.height = calendar.innerHeightTree + "px";
            line.style.fontSize = '1px';
            line.style.lineHeight = '1px';
            line.style.overflow = 'hidden';
            line.style.position = 'absolute';
            if (!this.cssOnly) {
                line.style.backgroundColor = this.cellBorderColor;
            }
            line.setAttribute("unselectable", "on");
            if (this.cssOnly) {
                line.className = this._prefixCssClass("_matrix_vertical_line");
            }

            this.divLines.appendChild(line);
            this.elements.linesVertical.push(line);

            this._cache.linesVertical[index] = line;
        };

        this._drawCellColumn = function(x) {
            var index = "x" + x;
            var height = this.innerHeightTree;

            if (this._cache.cells[index]) {
                return;
            }

            // only if not dirty
            var color = this._getColor(x, 0);
            var breaks = (x < this.itline.length - 1) ? this.itline[x + 1].breakBefore : false;

            var cell = document.createElement("div");
            cell.style.left = (x * this.cellWidth) + "px";
            cell.style.top = "0px";
            cell.style.width = (this.cellWidth) + "px";
            cell.style.height = height + "px";
            cell.style.position = 'absolute';
            cell.style.backgroundColor = color;
            cell.setAttribute("unselectable", "on");
            cell.className = this.cssOnly ? this._prefixCssClass('_cellcolumn') : this._prefixCssClass('cellbackground');

            // TEST
            cell.onclick = this._onMaindClick;

            this.divCells.appendChild(cell);
            this.elements.cells.push(cell);

            this._cache.cells[index] = '1';

        };

/*
        this._hideRows = function(from, to) {
            if (!to) {
                to = from;
            }
            if (to < from) {
                return;
            }
            if (from < 0 || from >= this.rows.length) {
                return;
            }

            for (var i = from; i <= to; i++) {
                this.row[i].Hidden = true;
            }

            this.deleteCells(); // don't confuse the cache
            this._drawCells();
            this._deleteEvents();
            this._drawEvents();

            this._drawResHeader();
        };
*/
        this._toggle = function(index) {
            var row = this.rows[index];
            var expanded = !row.Expanded;

            this.rows[index].Expanded = expanded;
            var rows = this._updateChildren(index, row.Expanded);

            if (!expanded) {
                for (var i = 0; i < rows.length; i++) {
                    var ri = rows[i];
                    this._deleteEventsInRow(ri);
                }
            }

            this._prepareRowTops();

            // the height needs to be updated before drawing cells
            this._drawResHeader();
            this._updateHeight();

            this._deleteCells(); // don't confuse the cache
            this._drawCells();

            if (expanded) {
                for (var i = 0; i < rows.length; i++) {
                    var ri = rows[i];
                    this._drawEventsInRow(ri);
                }
                this._findEventsInViewPort();
            }

            this._updateEventTops();

            this._saveState();

            var r = this._createDayPilotResource(row, index);
            if (expanded) {
                this._resourceExpandDispatch(r);
            }
            else {
                this._resourceCollapseDispatch(r);
            }

            this._clearCachedValues();
        };

        this._loadNode = function(i) {
            var params = {};
            params.index = i;
            
            if (typeof this.onLoadNode === 'function') {
                var args = {};
                var resource = this.rows[i].Resource;
                args.resource = resource;
                args.async = false;
                args.loaded = function() {
                    if (this.async) {
                        resource.dynamicChildren = false;
                        resource.expanded = true;
                        calendar.update();
                    }
                };
                
                this.onLoadNode(args);
                
                if (!args.async) {
                    resource.dynamicChildren = false;
                    resource.expanded = true;
                    this.update();
                }
            }
            else {
                this._callBack2('LoadNode', params);
            }

        };

        this._updateChildren = function(i, topExpanded) {
            var row = this.rows[i];
            var changed = [];
            //var node = this.tree[i];

            if (row.Children === null || row.Children.length === 0) {
                return changed;
            }

            for (var k = 0; k < row.Children.length; k++) {
                var index = row.Children[k];
                this.rows[index].Hidden = topExpanded ? !row.Expanded : true; // show/hide but don't change Expanded state
                if (topExpanded === !this.rows[index].Hidden) {
                    changed.push(index);
                }
                var uchildren = this._updateChildren(index, topExpanded);
                if (uchildren.length > 0) {
                    changed = changed.concat(uchildren);
                }
            }

            return changed;
        };

        this._startScroll = function(stepX, stepY) {
            //var step = 10;
            this._stopScroll();
            this._scrollabit(stepX, stepY);
        };

        this._scrollabitX = function(step) {
            if (!step) {
                return false;
            }
            var total = this.nav.scroll.scrollWidth;
            var start = this.nav.scroll.scrollLeft;
            var width = this.nav.scroll.clientWidth;
            var right = start + width;

            if (step < 0 && start <= 0) {
                return false;
            }

            if (step > 0 && right >= total) {
                return false;
            }

            this.nav.scroll.scrollLeft += step;
            // this is not necessary, it's linked using nav.scroll.onscroll
            //this.divTimeScroll.scrollLeft = this.nav.scroll.scrollLeft;

            return true;
        };

        this._scrollabitY = function(step) {
            if (!step) {
                return false;
            }
            var total = this.nav.scroll.scrollHeight;
            var start = this.nav.scroll.scrollTop;
            var height = this.nav.scroll.clientHeight;
            var bottom = start + height;

            if (step < 0 && start <= 0) {
                return false;
            }

            if (step > 0 && bottom >= total) {
                return false;
            }

            this.nav.scroll.scrollTop += step;
            // this is not necessary, it's linked using nav.scroll.onscroll
            //this.divTimeScroll.scrollTop = this.nav.scroll.scrollTop;

            return true;
        };

        this._scrollabit = function(stepX, stepY) {
            var moved = this._scrollabitX(stepX) || this._scrollabitY(stepY);
            if (!moved) {
                return;
            }

            var delayed = function(stepX, stepY) {
                return function() {
                    calendar._scrollabit(stepX, stepY);
                };
            };

            this.scrolling = window.setTimeout(delayed(stepX, stepY), 100);

        };

        this._stopScroll = function() {
            if (this.scrolling) {
                window.clearTimeout(this.scrolling);
                this.scrolling = null;
            }
        };

        this._prepareRowTops = function() {
            var top = 0;
            for (var i = 0; i < this.rows.length; i++) {
                var row = this.rows[i];
                if (!row.Hidden) {
                    row.Top = top;
                    top += row.Height;
                }
            }
            this.innerHeightTree = top;
            //return top; // now it's the bottom of the last visible row
        };

        this._deleteCells = function() {
            this.elements.cells = [];
            this.elements.linesVertical = [];
            this.elements.breaks = [];
            this._cache.cells = [];
            this._cache.linesVertical = [];
            this._cache.linesHorizontal = [];
            this._cache.breaks = [];
            this.divCells.innerHTML = '';
            this.divLines.innerHTML = '';
            this.divBreaks.innerHTML = '';
        };

        this._drawCell = function(x, y) {

            var itc = this.itline[x];
            if (!itc) {
                return;
            }

            var index = x + '_' + y;
            if (this._cache.cells[index]) {
                return;
            }
            
            if (typeof this.onBeforeCellRender === 'function') {
                var row = calendar.rows[y];
                var resource = row.Value;
                var rowOffset = row.Start.getTime() - this.startDate.getTime();
                var start = itc.start.addTime(rowOffset);
                var end = itc.end.addTime(rowOffset);
                
                var cell = this.cellProperties[index];
                if (!cell) {
                    cell = {};
                    
                    var ibi = {};
                    ibi.start = start;
                    ibi.end = end;
                    ibi.resource = resource;

                    cell.cssClass = null;
                    cell.html = null;
                    cell.backImage = null;
                    cell.backRepeat = null;
                    cell.backColor = null;
                    cell.business = this.isBusiness(ibi);
                    if (!this.cssOnly) {
                        cell.backColor = cell.business ? this.cellBackColor : this.cellBackColorNonBusiness;
                    }
                    
                    this.cellProperties[index] = cell;
                }
                cell.resource = resource;
                cell.start = start;
                cell.end = end;
                
                var args = {};
                args.cell = cell;
                
                this.onBeforeCellRender(args);
                
            }
            
            if (!this.cellProperties[index]) {
                var row = calendar.rows[y];
                var resource = row.Value;
                var rowOffset = row.Start.getTime() - this.startDate.getTime();
                var start = itc.start.addTime(rowOffset);
                var end = itc.end.addTime(rowOffset);

                var ibj = {};
                ibj.start = start;
                ibj.end = end;
                ibj.resource = resource;

                var cell = {};
                cell.business = this.isBusiness(ibj);
                if (!this.cssOnly) {
                    cell.backColor = cell.business ? this.cellBackColor : this.cellBackColorNonBusiness;
                }
                this.cellProperties[index] = cell;
            }

            //var color = this._getColor(x, y);
            var p = this._getCellProperties(x, y);
            
            //calendar.debug.message("cellProperties: " + p);
            
            // don't draw cells with no/default properties
            if (!this.drawBlankCells) {
                var isDefault = false;
                if (this.cssOnly && this._isRowParent(y)) {
                    isDefault = false;
                }
                else if (!this._hasProps(p, ['html', 'cssClass', 'backColor', 'backImage', 'backRepeat'])) {
                    isDefault = true;
                }
                if (isDefault) {
                    return;
                }
            }
            
            var cell = document.createElement("div");
            cell.style.left = (itc.left) + "px";
            cell.style.top = this.rows[y].Top + "px";
            cell.style.width = (itc.width) + "px";
            cell.style.height = (this.rows[y].Height) + "px";
            cell.style.position = 'absolute';
            if (p && p.backColor) {
                cell.style.backgroundColor = p.backColor;
            }
            cell.setAttribute("unselectable", "on");
            cell.className = this.cssOnly ? this._prefixCssClass('_cell') : this._prefixCssClass('cellbackground');
            
            cell.coords = {};
            cell.coords.x = x;
            cell.coords.y = y;

            if (this.cssOnly && this._isRowParent(y)) {
                DayPilot.Util.addClass(cell, this._prefixCssClass("_cellparent"));
            }

            if (p) {
                if (p.cssClass) {
                    if (this.cssOnly) {
                        DayPilot.Util.addClass(cell, p.cssClass);
                    }
                    else {
                        DayPilot.Util.addClass(cell, calendar._prefixCssClass(p.cssClass));
                    }
                }
                if (p.html) {
                    cell.innerHTML = p.html;
                }
                if (p.backImage) {
                    cell.style.backgroundImage = "url(\"" + p.backImage + "\")";
                }
                if (p.backRepeat) {
                    cell.style.backgroundRepeat = p.backRepeat;
                }
                if (p.business) {
                    DayPilot.Util.addClass(cell, calendar._prefixCssClass("_cell_business"));
                }
            }
            
            // TEST
            cell.onclick = this._onMaindClick;
            
            this.divCells.appendChild(cell);
            this.elements.cells.push(cell);

            this._cache.cells[index] = cell;

        };
        
        this._hasProps = function(object, props) {
            if (props) {
                for (var i = 0; i < props.length; i++) {
                    if (object[props[i]]) {
                        return true;
                    }
                }
            }
            else {
                for (var name in object) {
                    if (object[name]) {
                        return true;
                    }
                }
            }
            return false;
        };

        this._drawRangeCell = function(x, y) {
            
            var itc = this.itline[x];

            var cell = document.createElement("div");
            cell.style.left = (itc.left) + "px";
            cell.style.top = this.rows[y].Top + "px";
            cell.style.width = (itc.width - 1) + "px";
            cell.style.height = (this.rows[y].Height - 1) + "px";
            cell.style.position = 'absolute';
            cell.style.backgroundColor = calendar.cellSelectColor;
            cell.setAttribute("unselectable", "on");
            //cell.oncontextmenu = function () {return false;};

            this.divRange.appendChild(cell);
            this.elements.range.push(cell);

        };

        this.clearSelection = function() {
            this._deleteRange();
        };

        this.cleanSelection = this.clearSelection;

        this._deleteRange = function() {
            // IE doesn't like the div empty
            this.divRange.innerHTML = '<div style="position:absolute; left:0px; top:0px; width:0px; height:0px;"></div>';
            this.elements.range = [];

            DayPilotScheduler.rangeHold = null;
        };
	
        this._resolved = {};
        var resolved = this._resolved;

        resolved.locale = function() {
            return DayPilot.Locale.find(calendar.locale);
        };

        resolved.timeFormat = function() {
            if (calendar.timeFormat !== 'Auto') {
                    return calendar.timeFormat;
            }
            return resolved.locale().timeFormat;
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
        
        resolved.rounded = function() {
            return calendar.eventCorners === 'Rounded';
        };
        
        resolved.layout = function() {
            var isIE6 = /MSIE 6/i.test(navigator.userAgent);
            if (calendar.layout === 'Auto') {
                if (isIE6) {
                    return 'TableBased';
                }
                else {
                    return 'DivBased';
                }
            }
            return calendar.layout;
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
        
        resolved.isResourcesView = function() {
            return calendar.viewType !== 'Days';
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
	
        this._getColor = function(x, y) {
            var index = x + '_' + y;
            if (this.cellProperties && this.cellProperties[index]) {
                return this.cellProperties[index].backColor;
            }
            return null;
        };

        this._getCellProperties = function(x, y) {
            var index = x + '_' + y;
            if (this.cellProperties && this.cellProperties[index]) {
                return this.cellProperties[index];
            }
            /*
            var expanded = this._getExpandedCell(x, y);
            if (expanded) {
                if (!this.cellProperties) {
                    this.cellProperties = [];
                }
                this.cellProperties[index] = {};
                DayPilot.Util.copyProps(expanded, this.cellProperties[index], ['html', 'cssClass', 'backColor', 'backImage', 'backRepeat']);
                return this.cellProperties[index];
            }
            */
            
            return null;
        };
        
        this._copyCellProperties = function(source, x, y) {
            var index = x + '_' + y;
            this.cellProperties[index] = {};
            DayPilot.Util.copyProps(source, this.cellProperties[index], ['html', 'cssClass', 'backColor', 'backImage', 'backRepeat', 'business']);
            //DayPilot.Util.copyProps(source, this.cellProperties[index]);
            return this.cellProperties[index];
        };
        
        this._getExpandedCell = function(x, y) {
            if (!this.cellConfig) {
                return;
            }

            var config = this.cellConfig;
            
            if (config.vertical) {
                return this.cellProperties[x + "_0"];
            }
            
            if (config.horizontal) {
                return this.cellProperties["0_" + y];
            }
            
            if (config["default"]) {
                return config["default"];
            }

        };

        this._expandCellProperties = function() {
            // disabled
            //return;
            if (!this.cellConfig) {
                return;
            }
            
            var config = this.cellConfig;
            
            if (config.vertical) {
                for (var x = 0; x < config.x; x++) {
                    var def = this.cellProperties[x + "_0"];
                    if (!def) {
                        continue;
                    }
                    for (var y = 1; y < config.y; y++) {
                        this._copyCellProperties(def, x, y);
                    }
                }
            }
            
            if (config.horizontal) {
                for (var y = 0; y < config.y; y++) {
                    var def = this.cellProperties["0_" + y];
                    if (!def) {
                        continue;
                    }
                    for (var x = 1; x < config.x; x++) {
                        this._copyCellProperties(def, x, y);
                        //this.cellProperties[x + "_" + y] = def;
                    }
                }
            }
            
            if (config["default"]) {
                var def = config["default"];
                for (var y = 0; y < config.y; y++) {
                    for (var x = 0; x < config.x; x++) {
                        if (!this.cellProperties[x + "_" + y]) {
                            this._copyCellProperties(def, x, y);
    //                        this.cellProperties[x + "_" + y] = def;
                        }
                    }
                }                
            } 
        };

        this.isBusiness = function(cell) {

            if (cell.start.dayOfWeek() === 0 || cell.start.dayOfWeek() === 6) {
                return false;
            }
            
            var start = cell.start;
            var end = cell.end;

            var cellDuration = (end.getTime() - start.getTime()) / (1000 * 60);  // minutes
            if (cellDuration < 720) {
                if (cell.start.getHours() < this.businessBeginsHour || cell.start.getHours() >= this.businessEndsHour) {
                    return false;
                }
                else {
                    return true;
                }
            }

            return true;
        };

        this._show = function() {
            if (this.nav.top.style.visibility === 'hidden') {
                this.nav.top.style.visibility = 'visible';
            }
        };
        
        this._visible = function() {
            var el = calendar.nav.top;
            return el.offsetWidth > 0 && el.offsetHeight > 0;
        };
        
        this._waitForVisibility = function() {
            var visible = calendar._visible;
            
            if (!visible()) {
                calendar.debug.message("Not visible during init, starting visibilityInterval");
                calendar._visibilityInterval = setInterval(function() {
                    if (visible()) {
                        calendar.debug.message("Made visible, calling .show()");
                        calendar.show();
                        calendar._autoRowHeaderWidth();
                        clearInterval(calendar._visibilityInterval);
                    }
                }, 100);
            }
        };

        // sets the total height
        this._setHeight = function(pixels) {
            if (this.heightSpec !== "Parent100Pct") {
                this.heightSpec = "Fixed";
            }
            this.height = pixels - (this._getTotalHeaderHeight() + 2);
            this._updateHeight();
        };
        
        this.setHeight = this._setHeight;
        
        this._findRowByResourceId = function(id) {
            for (var i = 0; i < this.rows.length; i++) {
                if (this.rows[i].Value === id) {
                    return this.rows[i];
                }
            }
            return null;
        };

        this._shortInit = function() {
            this._prepareVariables();
            this._loadResources();
            this._drawTop();
            this._resize();
            this._registerGlobalHandlers();
            this._registerDispose();
            DayPilotScheduler.register(this);
            this._fireAfterRenderDetached(null, false);
            this._registerOnScroll();
            this._waitForVisibility();
            this._startAutoRefresh();
            this._callBack2('Init');
        };

        this.init = function() {

            this.nav.top = document.getElementById(id);

            if (!this.nav.top) {
                throw "DayPilot.Scheduler.init(): The placeholder element not found: '" + id + "'.";
            }
            
            if (this.nav.top.dp) {
                return;
            }
            var loadFromServer = this._loadFromServer();
            //var eventsAvailable = this.events.list !== null;

            if (loadFromServer) {
                this._shortInit();
                this.initialized = true;
                this._clearCachedValues();
                return;
            }

            this._prepareVariables();
            this._prepareItline();

            this._loadResources();
            this._expandCellProperties();
            this._drawTop();
            // resize can't be here because of autocellwidth mode, not all variables are ready
            this._calculateCellWidth();

            this._drawTimeHeader();

            this._loadingStart();

            this._loadEvents();

            this._prepareRowTops();
            this._drawResHeader();
            this._updateHeight();

            this._drawSeparators();

            this._resize();
            this._show();

            this._registerGlobalHandlers();
            this._registerDispose();
            DayPilotScheduler.register(this);

            //this.afterRender(null, false);

            this._registerOnScroll();

            this._loadingStop();

            if (calendar.scrollToDate) {
                calendar.scrollTo(calendar.scrollToDate);
            }
            else {
                calendar.setScroll(calendar.scrollX, calendar.scrollY);
            }
            if (!calendar.onScrollCalled) {
                calendar._onScroll();  // renders cells
            }

            var setScrollY = function() {
                if (calendar.scrollY) {
                    calendar.setScroll(calendar.scrollX, calendar.scrollY);
                }
            };

            window.setTimeout(setScrollY, 200);

            if (this.messageHTML) {
                var showMessage = function(msg) {
                    return function() {
                        calendar.message(msg);
                    };
                };
                window.setTimeout(showMessage(this.messageHTML), 100);
                //this.message(this.messageHTML, 5000);                
            }
            
            this._waitForVisibility();

            this._startAutoRefresh();

            this.initialized = true;
            
            this._clearCachedValues();
            this._fireAfterRenderDetached(null, false);
            this.debug.message("Init complete.");
            
        };
        
        
        this.temp = {};
        
        this.temp.getPosition = function() {
            var x = Math.floor(calendar.coords.x / calendar.cellWidth);
            var y = calendar._getRow(calendar.coords.y).i;
            if (y < calendar.rows.length) {
                var cell = {};
                cell.start = calendar.itline[x].start;
                cell.end = calendar.itline[x].end;
                cell.resource = calendar.rows[y].Value;
                return cell;
            }
            else {
                return null;
            }
        };
		
        // communication between components
        this.internal = {}; 
        // DayPilot.Action
        this.internal.invokeEvent = this._invokeEvent;
        // DayPilot.Menu
        this.internal.eventMenuClick = this._eventMenuClick;
        this.internal.timeRangeMenuClick = this._timeRangeMenuClick;
        this.internal.resourceHeaderMenuClick = this._resourceHeaderMenuClick;
        // DayPilot.Bubble
        this.internal.bubbleCallBack = this._bubbleCallBack;
        this.internal.findEventDiv = this._findEventDiv;

        this.Init = this.init;

    };

    // internal moving
    DayPilotScheduler.moving = null;

    // internal resizing
    DayPilotScheduler.originalMouse = null;
    DayPilotScheduler.resizing = null;

    DayPilotScheduler.globalHandlers = false;
    DayPilotScheduler.timeRangeTimeout = null;

    // selecting
    DayPilotScheduler.selectedCells = null;

    DayPilotScheduler.dragStart = function(element, duration, id, text, type) {
        DayPilot.us(element);

        var drag = DayPilotScheduler.drag = {};
        drag.element = element;
        drag.duration = duration;
        drag.text = text;
        drag.id = id;
        drag.shadowType = type ? type : 'Fill';  // default value

        return false;
    };
    
    /*
     * options: {
     *      element: dom element,
     *      duration: duration in minutes,
     *      
     * }
     */
    DayPilot.Scheduler.makeDraggable = function(options) {
        var element = options.element;
        var duration = options.duration || 1;
        element.ontouchstart = function(ev) {
            
            var holdfor = 500;
            
            window.setTimeout(function() {
                var drag = DayPilotScheduler.drag = {};
                drag.element = element;
                drag.duration = duration;
                drag.shadowType = "Fill";
                
                //alert("activated");

                ev.preventDefault();
            }, holdfor);
            
            ev.preventDefault();
        };
    };

    DayPilotScheduler.dragStop = function() {
        if (DayPilotScheduler.gShadow) {
            document.body.removeChild(DayPilotScheduler.gShadow);
            DayPilotScheduler.gShadow = null;
        }
        DayPilotScheduler.drag = null;
    };

    DayPilotScheduler.register = function(calendar) {
        if (!DayPilotScheduler.registered) {
            DayPilotScheduler.registered = [];
        }
        for (var i = 0; i < DayPilotScheduler.registered.length; i++) {
            if (DayPilotScheduler.registered[i] === calendar) {
                return;
            }
        }
        DayPilotScheduler.registered.push(calendar);

    };

    DayPilotScheduler.unregister = function(calendar) {
        var a = DayPilotScheduler.registered;
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
            DayPilot.ue(document, 'mousemove', DayPilotScheduler.gMouseMove);
            DayPilot.ue(document, 'mouseup', DayPilotScheduler.gMouseUp);
            //DayPilot.ue(window, 'unload', DayPilotScheduler.gUnload);
            DayPilotScheduler.globalHandlers = false;
        }
    };

    DayPilotScheduler.gMouseMove = function(ev) {
        if (typeof (DayPilotScheduler) === 'undefined') {
            return;
        }

        ev = ev || window.event;

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

        /*
        if (typeof(DayPilotBubble) != 'undefined') {
        DayPilotBubble.cancelTimeout();
        if (DayPilotBubble.active) {
        DayPilotBubble.active.delayedHide();
        }
        }
        */
        var mousePos = DayPilot.mc(ev);

        if (DayPilotScheduler.drag) {
            document.body.style.cursor = 'move';
            if (!DayPilotScheduler.gShadow) {
                DayPilotScheduler.gShadow = DayPilotScheduler.createGShadow(DayPilotScheduler.drag.shadowType);
            }

            var shadow = DayPilotScheduler.gShadow;
            shadow.style.left = mousePos.x + 'px';
            shadow.style.top = mousePos.y + 'px';

            // it's being moved outside, delete the inside shadow
            DayPilotScheduler.moving = null;
            if (DayPilotScheduler.movingShadow) {
                DayPilotScheduler.movingShadow.calendar = null;
                DayPilot.de(DayPilotScheduler.movingShadow);
                DayPilotScheduler.movingShadow = null;
            }

        }
        else if (DayPilotScheduler.moving && DayPilotScheduler.moving.event.calendar.dragOutAllowed && !DayPilotScheduler.drag) {
            var cal = DayPilotScheduler.moving.event.calendar; // source
            var ev = DayPilotScheduler.moving.event;

            // clear target
            DayPilotScheduler.moving.target = null;

            document.body.style.cursor = 'move';
            if (!DayPilotScheduler.gShadow) {
                DayPilotScheduler.gShadow = DayPilotScheduler.createGShadow(cal.shadow);
            }

            var shadow = DayPilotScheduler.gShadow;
            shadow.style.left = mousePos.x + 'px';
            shadow.style.top = mousePos.y + 'px';

            // it's being moved outside, delete the inside shadow
            DayPilotScheduler.drag = {};
            var drag = DayPilotScheduler.drag;
            drag.element = null;
            drag.duration = (ev.end().getTime() - ev.start().getTime()) / 1000;
            drag.text = ev.text();
            drag.id = ev.value();
            drag.shadowType = cal.shadow;
            drag.event = ev;
            //DayPilotScheduler.moving = null;
            DayPilot.de(DayPilotScheduler.movingShadow);
            DayPilotScheduler.movingShadow.calendar = null;
            DayPilotScheduler.movingShadow = null;
        }

        for (var i = 0; i < DayPilotScheduler.registered.length; i++) {
            if (DayPilotScheduler.registered[i]._out) {
                DayPilotScheduler.registered[i]._out();
            }
        }
    };

    DayPilotScheduler.gUnload = function(ev) {

        if (!DayPilotScheduler.registered) {
            return;
        }
        for (var i = 0; i < DayPilotScheduler.registered.length; i++) {
            var c = DayPilotScheduler.registered[i];
            //c.dispose();

            DayPilotScheduler.unregister(c);
        }
    };
    
    DayPilotScheduler.gMouseUp = function(ev) {

        if (DayPilotScheduler.resizing) {

            if (!DayPilotScheduler.resizingShadow) {
                document.body.style.cursor = '';
                DayPilotScheduler.resizing = null;
                return;
            }

            var e = DayPilotScheduler.resizing.event;
            var calendar = e.calendar;

            var shadowWidth = DayPilotScheduler.resizingShadow.clientWidth;
            var shadowLeft = DayPilotScheduler.resizingShadow.offsetLeft;
            var border = DayPilotScheduler.resizing.dpBorder;

            // TODO involve rowStart for Days mode
            //var rowStart = new DayPilot.Date(calendar.rows[DayPilotScheduler.resizing.data.DayIndex].Start);
            var row = calendar.rows[DayPilotScheduler.resizing.event.part.dayIndex];
            var rowOffset = row.Start.getTime() - calendar._visibleStart().getTime();

            var newStart = null;
            var newEnd = null;
            
            var exact = !calendar.snapToGrid;

            if (border === 'left') {
                newStart = calendar.getDate(shadowLeft, exact).addTime(rowOffset);
                newEnd = e.end();
            }
            else if (border === 'right') {
                newStart = e.start();
                newEnd = calendar.getDate(shadowLeft + shadowWidth, exact, true).addTime(rowOffset);
            }

            // stop resizing on the client
            DayPilot.de(DayPilotScheduler.resizingShadow);
            DayPilotScheduler.resizing = null;
            DayPilotScheduler.resizingShadow = null;

            document.body.style.cursor = '';

            // action here
            calendar._eventResizeDispatch(e, newStart, newEnd);
        }
        else if (DayPilotScheduler.moving) {
            if (!DayPilotScheduler.movingShadow) {
                document.body.style.cursor = '';
                DayPilotScheduler.moving = null;
                return;
            }

            var e = DayPilotScheduler.moving.event;
            //var calendar = e.calendar;  // doesn't work for drag&drop between two schedulers, this is the source
            var calendar = DayPilotScheduler.moving.target;


            if (!calendar) {
                DayPilot.de(DayPilotScheduler.movingShadow);
                DayPilotScheduler.movingShadow.calendar = null;
                document.body.style.cursor = '';
                DayPilotScheduler.moving = null;
                return;
            }

            var newStart = DayPilotScheduler.movingShadow.start;
            var newEnd = DayPilotScheduler.movingShadow.end;
            var newResource = (calendar.viewType !== 'Days') ? DayPilotScheduler.movingShadow.row.Value : null;
            var external = DayPilotScheduler.drag ? true : false;
            var line = DayPilotScheduler.movingShadow.line;
            //var left = DayPilotScheduler.movingShadow.offsetLeft;

            if (DayPilotScheduler.drag) {
                if (!calendar.todo) {
                    calendar.todo = {};
                }
                calendar.todo.del = DayPilotScheduler.drag.element;
                DayPilotScheduler.drag = null;
            }

            // clear the moving state            
            DayPilot.de(DayPilotScheduler.movingShadow);
            calendar._clearShadowHover();

            DayPilotScheduler.movingShadow.calendar = null;
            document.body.style.cursor = '';
            DayPilotScheduler.moving = null;
            DayPilotScheduler.movingShadow = null;

            var ev = ev || window.event;
            calendar._eventMoveDispatch(e, newStart, newEnd, newResource, external, ev, line);
        }
        else if (DayPilotScheduler.range) {
            
            ev = ev || window.event;
            var button = ev.which || ev.button;

            var range = DayPilotScheduler.range;
            var calendar = range.calendar;
            
            if (DayPilotScheduler.timeRangeTimeout) {
                clearTimeout(DayPilotScheduler.timeRangeTimeout);
                DayPilotScheduler.timeRangeTimeout = null;
                calendar._onMaindDblClick(ev);
                return;
            }

            DayPilotScheduler.rangeHold = range;

            // must be cleared before dispatching
            DayPilotScheduler.range = null;

            var delayed = function(sel) {
                return function() {
                    DayPilotScheduler.timeRangeTimeout = null;
                    calendar._timeRangeSelectedDispatch(sel.start, sel.end, sel.resource);

                    if (calendar.timeRangeSelectedHandling !== "Hold" && calendar.timeRangeSelectedHandling !== "HoldForever") {
                        doNothing();
                        //calendar.deleteRange();
                    }
                    else {
                        DayPilotScheduler.rangeHold = range;
                    }
                };
            };

            var sel = calendar._getSelection(range);

            if (button !== 1) { // only left-click
                DayPilotScheduler.timeRangeTimeout = null;
                return;
            }

            if (calendar.timeRangeDoubleClickHandling === 'Disabled') {
                delayed(sel)();

                var ev = ev || window.event;
                ev.cancelBubble = true;
                return false;  // trying to prevent onmaindclick
            }
            else {
                DayPilotScheduler.timeRangeTimeout = setTimeout(delayed(sel), calendar.doubleClickTimeout);  // 300 ms
            }

        }

        // clean up external drag helpers
        if (DayPilotScheduler.drag) {
            DayPilotScheduler.drag = null;

            document.body.style.cursor = '';
        }

        if (DayPilotScheduler.gShadow) {
            document.body.removeChild(DayPilotScheduler.gShadow);
            DayPilotScheduler.gShadow = null;
        }

        DayPilotScheduler.moveOffsetX = null; // clean for next external drag    
        DayPilotScheduler.moveDragStart = null;
    };

    // global shadow, external drag&drop
    DayPilotScheduler.createGShadow = function(type) {

        var shadow = document.createElement('div');
        shadow.setAttribute('unselectable', 'on');
        shadow.style.position = 'absolute';
        shadow.style.width = '100px';
        shadow.style.height = '20px';
        shadow.style.border = '2px dotted #666666';
        shadow.style.zIndex = 101;

        if (type === 'Fill') {       // transparent shadow    
            shadow.style.backgroundColor = "#aaaaaa";
            shadow.style.opacity = 0.5;
            shadow.style.filter = "alpha(opacity=50)";
            shadow.style.border = '2px solid #aaaaaa';
        }

        document.body.appendChild(shadow);

        return shadow;
    };

    // publish the API 

    // (backwards compatibility)    
    DayPilot.SchedulerVisible.dragStart = DayPilotScheduler.dragStart;
    DayPilot.SchedulerVisible.dragStop = DayPilotScheduler.dragStop;
    DayPilot.SchedulerVisible.Scheduler = DayPilotScheduler.Scheduler;
    DayPilot.SchedulerVisible.globalHandlers = DayPilotScheduler.globalHandlers;

    // current
    //DayPilot.Scheduler = DayPilotScheduler.Scheduler;


    // experimental jQuery bindings
    if (typeof jQuery !== 'undefined') {
        (function($) {
            $.fn.daypilotScheduler = function(options) {
                var first = null;
                var j = this.each(function() {
                    if (this.daypilot) { // already initialized
                        return;
                    };

                    var daypilot = new DayPilot.Scheduler(this.id);
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