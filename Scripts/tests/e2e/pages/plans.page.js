var UIHelpers = require('../shared/uiHelpers.js');

var PlansPage = function () {

    //Remove Token Properties
    var manageAuthTokensButton = element(by.xpath('/html/body/div[1]/div/div[2]/div[2]/div/div/ul/li[5]/ul/li[1]/a'));

    /* Login Properties */
    var emailInput = element(by.id('Email'));
    var passwordInput = element(by.id('Password'));
    var loginButton = element(by.xpath('//*[@id="loginform"]/form/div[2]/div/div/button'));

    /* General Properties */
    var uiHelpers = new UIHelpers();
    var addPlanButton = element(by.xpath('//*[@id="Myfr8lines"]/h3/div/a'));
    var addActivityButton = element(by.className('action-add-button-link'));
    //var addAccountButton = element(by.className('.auth-link-account'));

    /* Add Google Activity Properties */
    var googleActivityButton = element(by.xpath('/html/body/div[2]/div[2]/div/div/div/div/div[1]/div[1]/div[1]/action-picker-panel/div/div[2]/div/div[11]/div[1]/img'));
    var googleSheetActivityButton = element(by.xpath('/html/body/div[2]/div[2]/div/div/div/div/div[1]/div[1]/div[1]/action-picker-panel/div/div[2]/div/ul/li[1]/a/span'));

    /* SalesForce Activity Properties */
    var salesForceActivityButton = element(by.xpath('/html/body/div[2]/div[2]/div/div/div/div/div[1]/div[1]/div[1]/action-picker-panel/div/div[2]/div/div[15]/div[1]/img'));
    var salesForceGetDataActivityButton = element(by.xpath('/html/body/div[2]/div[2]/div/div/div/div/div[1]/div[1]/div[1]/action-picker-panel/div/div[2]/div/ul/li[1]/a/span'));

    /*DocuSign Activity Properties */
    var docuSignActivityButton = element(by.xpath('/html/body/div[2]/div[2]/div/div/div/div/div[1]/div[1]/div[1]/action-picker-panel/div/div[2]/div/div[6]/div[1]/img'));
    var docuSignMonitorActivityButton = element(by.xpath('/html/body/div[2]/div[2]/div/div/div/div/div[1]/div[1]/div[1]/action-picker-panel/div/div[2]/div/ul/li[1]/a/span'));

    /* Slack Activity Properties */
    var slackActivityButton = element(by.xpath('/html/body/div[2]/div[2]/div/div/div/div/div[1]/div[1]/div[1]/action-picker-panel/div/div[2]/div/div[17]/div[1]/img'));
    var slackMonitorActivityButton = element(by.xpath('/html/body/div[2]/div[2]/div/div/div/div/div[1]/div[1]/div[1]/action-picker-panel/div/div[2]/div/ul/li[1]/a/span'));

    this.get = function () {
        browser.ignoreSynchronization = true;
        browser.get(browser.baseUrl + '/dashboard/myaccount');
    };

    this.manageAuthTokens = function () {
        return manageAuthTokensButton.click();
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

    //this.addAccount = function () {
    //    //return browser.wait(EC.visibilityOf((googlePage.addAccountButton)), 5000).then(function () {
    //    //});
    //    //return browser.wait(element(by.cssContainingText('.auth-link-account'))).then(function () {
    //    //    return addAccountButton.click();
    //    //});

    //    return uiHelpers.waitReady(addAccountButton).then(function () {
    //        return addAccountButton.click();
    //    });
    //};

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
    
    this.addDocuSignActivity = function () {
        return uiHelpers.waitReady(docuSignActivityButton).then(function () {
            return docuSignActivityButton.click();
        });
    };

    this.getDocuSignActivity = function () {
        return uiHelpers.waitReady(docuSignMonitorActivityButton).then(function () {
            return docuSignMonitorActivityButton.click();
        });
    };

    this.addSlackActivity = function () {
        return uiHelpers.waitReady(slackActivityButton).then(function () {
            return slackActivityButton.click();
        });
    };

    this.getSlackMonitorActivity = function () {
        return uiHelpers.waitReady(slackMonitorActivityButton).then(function () {
            return slackMonitorActivityButton.click();
        });
    };
};
module.exports = PlansPage;