var uiHelpers = function () {
    var defaultElementWaitTimeout = 5000;

    this.waitForElementToBePresent = function (element, timeout) {
        if (!timeout) {
            timeout = defaultElementWaitTimeout;
        }
        return browser.wait(function () {
            return element.isPresent();
        }, timeout);
    };

    // Helpers
    function _refreshPage() {
        // Swallow useless refresh page webdriver errors
        browser.navigate().refresh().then(function () { }, function (e) { });
    };

    // Config
    var specTimeoutMs = 10000; // 10 seconds

    this.waitReady = function (element, opt_optStr) {
        var self = element;
        var driverWaitIterations = 0;
        var lastWebdriverError;
        function _throwError() {
            throw new Error("Expected '" + self.locator().toString() +
                "' to be present and visible. " +
                "After " + driverWaitIterations + " driverWaitIterations. " +
                "Last webdriver error: " + lastWebdriverError);
        };

        function _isPresentError(err) {
            lastWebdriverError = (err != null) ? err.toString() : err;
            return false;
        };

        return browser.driver.wait(function () {
            driverWaitIterations++;
            if (opt_optStr === 'withRefresh') {
                // Refresh page after more than some retries
                if (driverWaitIterations > 7) {
                    _refreshPage();
                }
            }
            return self.isPresent().then(function (present) {
                if (present) {
                    return self.isDisplayed().then(function (visible) {
                        lastWebdriverError = 'visible:' + visible;
                        return visible;
                    }, _isPresentError);
                } else {
                    lastWebdriverError = 'present:' + present;
                    return false;
                }
            }, _isPresentError);
        }, specTimeoutMs).then(function (waitResult) {
            if (!waitResult) { _throwError() };
            return waitResult;
        }, function (err) {
            _isPresentError(err);
            _throwError();
            return false;
        });
    };

   
};
module.exports = uiHelpers;