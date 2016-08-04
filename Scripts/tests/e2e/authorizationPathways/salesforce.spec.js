var PlansPage = require('../pages/plans.page.js');
var ManageAuthTokens = require('../shared/manageAuthTokens.js');
var AccountHelper = require('../shared/accountHelper.js');
var MyAccountPage = require('../pages/myAccount.page.js');
var UIHelpers = require('../shared/uiHelpers.js');

escribe('SalesForce Authorization pathway test', function () {
    var plansPage = new PlansPage();
    var manageAuthTokens = new ManageAuthTokens();
    var accountHelper = new AccountHelper();
    var myAccountPage = new MyAccountPage();
    var uiHelpers = new UIHelpers();

    it('should login', function () {
        return accountHelper.login().then(function () {
            plansPage.setEmail(browser.params.username);
            plansPage.setPassword(browser.params.password);
            return plansPage.login().click().then(function () {
            });
        });
    });

    it('should go to myAccount page', function () {
        return myAccountPage.get();
        expect(browser.getCurrentUrl()).toEqual('http://dev.fr8.co/dashboard/myaccount');
    });

    it('should add plan', function () {
        return plansPage.addPlan().then(function () {
            return plansPage.addActivity();
        });
    });

    it('should add salesForce activity', function () {
        return plansPage.addSalesForceActivity().then(function () {
            return plansPage.getSalesForceGetDataActivity();
        });
    });

});