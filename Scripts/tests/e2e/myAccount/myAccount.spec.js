var MyAccountPage = require('../pages/myAccount.page.js');
var AccountHelper = require('../shared/accountHelper.js');
var UIHelpers = require('../shared/uiHelpers.js');

describe('registration page tests', function () {

    var myAccountPage = new MyAccountPage();
    var uiHelpers = new UIHelpers();
    var accountHelper = new AccountHelper();

    myAccountPage.get();

    it('should logout', function () {
        return uiHelpers.waitForElementToBePresent(myAccountPage.selectDropDownByName).then(function () {
            //expect(browser.getCurrentUrl()).toContain('/DockyardAccount');
            return myAccountPage.logo().then(function () {
                expect(element(by.className('Connect Your Cloud Services')));
            });
        }, function () { });
    });


});