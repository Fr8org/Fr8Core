var UIHelpers = require('../shared/uiHelpers.js');

var PlansPage = function () {

    /* Login Properties */
    var emailInput = element(by.id('Email'));
    var passwordInput = element(by.id('Password'));
    var loginButton = element(by.xpath('//*[@id="loginform"]/form/div[2]/div/div/button'));

    /* General Properties */
    var uiHelpers = new UIHelpers();
    var addPlanButton = element(by.xpath('//*[@id="Myfr8lines"]/h3/a'));
    var addActivityButton = element(by.className('action-add-button-link'));

    /* Add Google Activity Properties */
    var googleActivityButton = element(by.xpath('/html/body/div[2]/div[2]/div/div/div/div/div[1]/div[1]/div[1]/action-picker-panel/div/div[2]/div/div[13]/div[1]/img'));
    var googleSheetActivityButton = element(by.xpath('/html/body/div[2]/div[2]/div/div/div/div/div[1]/div[1]/div[1]/action-picker-panel/div/div[2]/div/ul/li[1]/a/span'));

    /* SalesForce Activity Properties */
    var salesForceActivityButton = element(by.xpath('/html/body/div[2]/div[2]/div/div/div/div/div[1]/div[1]/div[1]/action-picker-panel/div/div[2]/div/div[22]/div[1]/img'));
    var salesForceGetDataActivityButton = element(by.xpath('/html/body/div[2]/div[2]/div/div/div/div/div[1]/div[1]/div[1]/action-picker-panel/div/div[2]/div/ul/li[1]/a/span'));


    this.get = function () {
        browser.ignoreSynchronization = true;
        browser.get(browser.baseUrl + '/dashboard/myaccount');
    };

    this.setEmail = function (email) {
        emailInput.sendKeys(email);
    };

    this.setPassword = function (password) {
        passwordInput.sendKeys(password);
    };

    this.login = function () {
        return loginButton;
    };

    this.addPlan = function () {
        return uiHelpers.waitReady(addPlanButton).then(function () {
            return addPlanButton.click();
        });
    };

    this.addActivity = function () {
        return uiHelpers.waitForElementToBePresent(addActivityButton).then(function () {
            return addActivityButton.click();
        });
    };

    this.addGoogleActivity = function () {
        return uiHelpers.waitReady(googleActivityButton).then(function () {
            return googleActivityButton.click();
        });
    };

    this.getGoogleSheetActivity = function () {
        return uiHelpers.waitReady(googleSheetActivityButton).then(function () {
            return googleSheetActivityButton.click();
        });
    };

    this.addSalesForceActivity = function () {
        return uiHelpers.waitReady(salesForceActivityButton).then(function () {
            return salesForceActivityButton.click();
        });
    };

    this.getSalesForceGetDataActivity = function () {
        return uiHelpers.waitReady(salesForceGetDataActivityButton).then(function () {
            return salesForceGetDataActivityButton.click();
        });
    };

};
module.exports = PlansPage;