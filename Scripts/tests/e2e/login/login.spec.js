var LoginPage = require('../pages/login.page.js');
var AccountHelper = require('../shared/accountHelper.js');
var UIHelpers = require('../shared/uiHelpers.js');

//PROPERTIES
var uiHelpers = new UIHelpers();
var loginPage = new LoginPage();
var accountHelper = new AccountHelper();

describe('login page tests', function () {

    beforeEach(function () {
        loginPage.get();
    });

    

    //afterEach(function () {
    //    accountHelper.logout();
    //});

    it('should login', function () {
       return accountHelper.login().then(function () {
                expect(browser.getCurrentUrl()).toContain('/Welcome');
       });
    });

});

describe('logout test', function () {

    afterEach(function () {
        browser.ignoreSynchronization = true;
        browser.get(browser.baseUrl + '/dashboard/myaccount');
        //accountHelper.logout();
    });

    //it('should logout', function () {
    //    return uiHelpers.waitForElementToBePresent(loginPage.selectDropDownByName).then(function () {
    //        expect(browser.getCurrentUrl()).toContain('/DockyardAccount');
    //        //return loginPage.logo().then(function () {
    //        //    expect(element(by.className('Connect Your Cloud Services')));
    //        //});
    //    }, function () { });
    //});

    it('should logout', function () {
        return loginPage.selectDropDownByName().then(function () {
            expect(browser.getCurrentUrl()).toContain('/DockyardAccount');
        });
    });

});
   