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
};
module.exports = uiHelpers;