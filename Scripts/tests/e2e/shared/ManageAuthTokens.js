var UIHelpers = require('../shared/uiHelpers.js');

var ManageAuthTokens = function () {

    //PROPERTIES
    var uiHelpers = new UIHelpers();

    //FUNCTIONS
    this.revokeAuthTokens = function () {
        var revokeButton = element(by.buttonText("Revoke"));
        return uiHelpers.waitForElementToBePresent(revokeButton).then(function () {
            element.all(by.buttonText("Revoke")).each(function (button) {
                browser.wait(button.click());
            });
        }, function () { });
    }
};
module.exports = ManageAuthTokens;