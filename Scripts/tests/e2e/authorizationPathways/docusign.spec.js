var PlansPage = require('../pages/plans.page.js');
var ManageAuthTokens = require('../shared/manageAuthTokens.js');
var AccountHelper = require('../shared/accountHelper.js');
var MyAccountPage = require('../pages/myAccount.page.js');
var UIHelpers = require('../shared/uiHelpers.js');
var RegistrationPage = require('../pages/registration.page.js');

describe('DocuSign Authorization pathway test', function () {
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

    //it('should control and remove tokens', function () {
    //    return browser.driver.get(browser.baseUrl + '/dashboard/manageAuthTokens').then(function () {
    //        expect(element(by.xpath('/html/body/div[2]/div[2]/div/div/div/div/div/div[1]/div[1]/div/div/div/div[1]/div[2]/table/thead/tr/th[1]')));
    //        return manageAuthTokens.revokeAuthTokens();
    //    });
    //});

    it('should go to myAccount page', function () {
        return myAccountPage.get();
        expect(browser.getCurrentUrl()).toEqual('http://dev.fr8.co/dashboard/myaccount');
    });

    it('should add plan', function () {
        return plansPage.addPlan().then(function () {
            return plansPage.addActivity();
        });
    });

    it('should add DocuSign Monitor activity', function () {
        return plansPage.addDocuSignActivity().then(function () {
            return plansPage.getDocuSignActivity();
        });
    });

    describe('should logout', function () {

        var registrationPage = new RegistrationPage();

        it('should logout', function () {
            return accountHelper.logout().then(function () {
                   expect(browser.getCurrentUrl()).toContain('/Account');
            });
        });
     });

});