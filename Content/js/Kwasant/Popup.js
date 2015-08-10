if (typeof(Kwasant) === 'undefined') {
    Kwasant = {};
}

if (typeof (Kwasant.IFrame) === 'undefined') {
    Kwasant.IFrame = {};
}

(function () {

    var spinnerImage = new Image();
    spinnerImage.src = '/Content/img/ajax-loader.gif'; //Pre-load the spinner

    function setDefault(options) {
        if (options === undefined || options === null)
            options = {};

        if (options.paddingAmount === undefined)
            options.paddingAmount = 7;

        if (options.horizontalAlign === undefined || (options.horizontalAlign != 'middle' && options.horizontalAlign != 'left' && options.horizontalAlign != 'right'))
            options.horizontalAlign = 'middle';

        if (options.verticalAlign === undefined || (options.verticalAlign != 'centre' && options.verticalAlign != 'top' && options.verticalAlign != 'bottom'))
            options.verticalAlign = 'centre';

        if (options.modal === undefined)
            options.modal = false;
        
        if (options.pinned === undefined)
            options.pinned = false;

        if (typeof(options.callback) !== 'function')
            options.callback = null;

        if (typeof(options.statechange) !== 'function')
            options.statechange = null;

        if (options.displayLoadingSpinner === undefined)
            options.displayLoadingSpinner = true;
       
        return options;
    }

    function getScrollbarWidth() {
        var outer = document.createElement("div");
        outer.style.visibility = "hidden";
        outer.style.width = "100px";
        outer.style.msOverflowStyle = "scrollbar"; // needed for WinJS apps

        document.body.appendChild(outer);

        var widthNoScroll = outer.offsetWidth;
        // force scrollbars
        outer.style.overflow = "scroll";

        // add innerdiv
        var inner = document.createElement("div");
        inner.style.width = "100%";
        outer.appendChild(inner);

        var widthWithScroll = inner.offsetWidth;

        // remove divs
        outer.parentNode.removeChild(outer);

        return widthNoScroll - widthWithScroll;
    }

    var activePopups = [];

    Kwasant.IFrame.RegisterCloseEvent = function (iframe, callback) {
        //Since we're working in iframes, we have different copies of our popup object. We want to make sure we always use the top documents object when firing events
        if (document !== top.document) {
            top.Kwasant.IFrame.RegisterCloseEvent(iframe, callback);
            return;
        }
        
        activePopups.push({ document: $(iframe).contents().get(0), iframe: iframe, callback: callback });
        $(document.body).on('popupFormClosing', function(document, args) {
            close(args.document, args.args);
        });
    };
    
    Kwasant.IFrame.RegisterStateChangeEvent = function (iframe, callback) {
        //Since we're working in iframes, we have different copies of our popup object. We want to make sure we always use the top documents object when firing events
        if (document !== top.document) {
            top.Kwasant.IFrame.RegisterStateChangeEvent(iframe, callback);
            return;
        }

        $(document.body).on('stateChange', function (document, args) {
            callback(args.args);
        });
    };
    
    Kwasant.IFrame.PopupsActive = function () {
        return activePopups.length > 0;
    };

    Kwasant.IFrame.CloseMe = function (args, doc) {
        if (doc == null)
            doc = document;
        
        if (document !== top.document) {
            top.Kwasant.IFrame.CloseMe(args, doc);
            return;
        }

        parent.$('body').trigger('popupFormClosing', { document: doc, args: args });
    };
    
    Kwasant.IFrame.DispatchStateChange = function (args, doc) {
        if (doc == null)
            doc = document;

        if (document !== top.document) {
            top.Kwasant.IFrame.StateChange(args, doc);
            return;
        }

        parent.$('body').trigger('stateChange', { document: doc, args: args });
    };

    function close(document, args) {
        for (var i = activePopups.length - 1; i >= 0 ; i--) {
            var obj = activePopups[i];
            if (obj.document == document) {
                if (obj.callback !== null && obj.callback !== undefined) {
                    obj.callback(args);
                }
                obj.iframe.hide();
                if (obj.iframe.mask)
                    obj.iframe.mask.hide();

                activePopups.splice(i, 1);
                break;
            }
        }
    };

    Kwasant.IFrame.DispatchPostRequest = function (url, data, callback) {
        var spinner = Kwasant.IFrame.DisplaySpinner();
        $.post(url, data)
            .success(callback)
            .fail(function() {
                alert('Error connecting to server. Your changes were not saved.');
            }).always(function() {
                if (spinner !== null)
                    spinner.hide();
            });
    };

    Kwasant.IFrame.DispatchUrlRequest = function(url, callback, type) {
        var spinner = Kwasant.IFrame.DisplaySpinner();
        $.ajax(url, { type: type })
            .success(callback)
            .fail(function() {
                alert('Error connecting to server. Your changes were not saved.');
            }).always(function () {
                if(spinner !== null)
                    spinner.hide();
            });
    };
    
    Kwasant.IFrame.DisplaySpinner = function () {
        return displayLoadingSpinner();
    };

    function displayLoadingSpinner() {
        var mask = $('<div></div>');
        mask.css({ 'position': 'absolute', 'z-index': 9999, 'background-color': '#FFF', 'display': 'none' });
        $(top.document.body).append(mask);

        var maskHeight = $(top.document).height();
        var maskWidth = $(top.document).width();
        mask.css({ 'left': 0, 'top': 0, 'width': maskWidth, 'height': maskHeight, 'margin': '0px', 'padding': '0px' });
        mask.fadeTo("fast", 0.8);

        var spinner = $('<img src="/Content/img/ajax-loader.gif"></a>');
        mask.append(spinner);
        spinner.show();

        var winH = $(top).height();
        var winW = $(top).width();

        var scrollTop = $(top).scrollTop();
        var scrollLeft = $(top).scrollLeft();

        var topPos = scrollTop + (winH - spinner.height()) / 2;
        var leftPos = scrollLeft + (winW - spinner.width()) / 2;

        spinner.css('position', 'absolute');
        spinner.css('top', topPos);
        spinner.css('left', leftPos);

        spinner.fadeTo("fast", 1);

        return mask;
    }

    Kwasant.IFrame.Display = function displayForm(url, options) {
        options = setDefault(options);

        var spinner;
        if (options.displayLoadingSpinner)
            spinner = displayLoadingSpinner();
        
        var iframe = $('<iframe/>', {
            src: url,
            style: 'position: absolute;width: 500px;background-color: #FFFFFF;display: none;z-index: 9999;border: 1px solid #333;-moz-box-shadow:0 0 10px #000;-webkit-box-shadow:0 0 10px #000;box-shadow: #000 0px 0px 10px;padding:' + options.paddingAmount + 'px;',
            load: function () {
                try {
                    var contentWindow = this.contentWindow;
                    var iframeDoc = $(this).contents();

                    //This is a workaround, as iframes dont let us access the status code.
                    //Therefore, we stick meta tags in our error pages
                    //If we find the statusCode meta frame, and its value is not 200, then it's an error.
                    //In this case, we hide the spinner and display an error (the form is not displayed)./
                    var statusCodeMetaTag = $(iframeDoc.get(0).head).find('meta[name="statusCode"]').get(0);
                    if (statusCodeMetaTag !== undefined && statusCodeMetaTag !== null) {
                        var statusCode = statusCodeMetaTag.content;
                        if (statusCode != 200) {
                            if (spinner)
                                spinner.hide();
                            
                            //Now it's an error...
                            alert('Problem connecting to the server. Please try again later.');
                            iframe.remove(); //Delete the iframe
                            return;
                        }
                    }

                    var that = $(this);
                    var reposition = function() {
                        var winH = $(top).height();
                        var winW = $(top).width();

                        var scrollTop = $(top).scrollTop();
                        var scrollLeft = $(top).scrollLeft();
                    
                        var iframeWidth = iframeDoc.get(0).body.offsetWidth + (options.paddingAmount * 2);
                        var iframeHeight = iframeDoc.get(0).body.offsetHeight + (options.paddingAmount * 2);

                        if (options.minWidth !== null && options.minWidth !== undefined && iframeWidth < options.minWidth)
                            iframeWidth = options.minWidth;
                    
                        if (options.minHeight !== null && options.minHeight !== undefined && iframeHeight < options.minHeight)
                            iframeHeight = options.minHeight;

                        var sidePadding = 2;
                        var topPos;
                        iframeDoc.find('body').addClass('iframe-body');

                        if (options.verticalAlign === 'top') {
                            topPos = scrollTop + sidePadding;
                        } else if (options.verticalAlign === 'centre') {
                            topPos = scrollTop + (winH - that.height()) / 2;
                        } else {
                            topPos = scrollTop + (winH - that.height()) - getScrollbarWidth() - sidePadding;
                        }

                        if (topPos < 10)
                            topPos = 10;

                        var leftPos;
                        if (options.horizontalAlign === 'right') {
                            leftPos = scrollLeft + (winW - that.width()) - getScrollbarWidth() - sidePadding;
                        } else if (options.horizontalAlign === 'middle') {
                            leftPos = scrollLeft + (winW - that.width()) / 2;
                        } else {
                            leftPos = scrollLeft + sidePadding;
                        }

                        if (options.width)
                            iframeWidth = options.width;
                    
                        if (options.height)
                            iframeHeight = options.height;

                        that.css('top', topPos);
                        that.css('left', leftPos);

                        that.css('width', iframeWidth + 'px');
                        that.css('minWidth', iframeWidth + 'px');
                    
                        that.css('height', iframeHeight + 'px');
                        that.css('minHeight', iframeHeight + 'px');
                    };

                    if (options.pinned) {
                        $(top).resize(reposition);
                        $(top).scroll(reposition);
                    }

                    if (options.modal) {
                        that.mask = $('<div></div>');
                        that.mask.css({ 'position': 'absolute', 'z-index': 9999, 'background-color': '#FFF', 'display': 'none' });
                        iframe.before(that.mask);

                        var maskHeight = $(top.document).height();
                        var maskWidth = $(top.document).width();
                        that.mask.css({ 'left': 0, 'top': 0, 'width': maskWidth, 'height': maskHeight });
                        that.mask.fadeTo("fast", 0.8);
                    }

                    $(this).fadeTo("fast", 1, function() {
                        //We need to position it twice. The first position allows the browser to calculate the dimensions. The second reposition moves it based on dimensions.
                        reposition();
                        reposition();

                        //We need a third time if we're sticking to the bottom. This is because the reposition has the potential to mess with the scroll bars - so we need to re-calculate it with that in mind.
                        if (options.verticalAlign === 'bottom') {
                            setTimeout(reposition, 100);
                        }
                        if (spinner)
                            spinner.hide();

                    
                        setTimeout(function () {
                            contentWindow.focus();
                            if (options.focusElement)
                            
                            var focusElem = iframeDoc.find(options.focusElement).get(0);
                            if (focusElem)
                                focusElem.focus();
                        }, 100);
                    });

                    Kwasant.IFrame.RegisterCloseEvent(that, options.callback);
                    Kwasant.IFrame.RegisterStateChangeEvent(that, options.statechange);
                } catch (e) {
                    if (spinner)
                        spinner.hide();

                    alert('Problem connecting to the server. Please try again later.');
                }
            }
        });

        $(top.document.body).append(iframe);
        iframe.hide();
    };
})();