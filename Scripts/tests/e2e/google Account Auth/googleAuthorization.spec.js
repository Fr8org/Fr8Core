var GoogleAuthorizationPage = require('../pages/googleAuthorization.page.js');
var ManageAuthTokens = require('../shared/manageAuthTokens.js');

describe('Google Authorization pathway test', function () {
    var googleAuthorizationPage;
    googleAuthorizationPage = new GoogleAuthorizationPage();
    googleAuthorizationPage.get();

    var manageAuthTokens;
    manageAuthTokens = new ManageAuthTokens();

    it('should login', function () {
        googleAuthorizationPage.setEmail("integration_test_runner@fr8.company");
        googleAuthorizationPage.setPassword("fr8#s@lt!");
        googleAuthorizationPage.login();
        browser.waitForAngular();
        expect(browser.getCurrentUrl()).toContain('Welcome');
    });

    it('should open my plans page', function () {
        browser.sleep(2000);
        googleAuthorizationPage.myAccount();
        browser.waitForAngular();
        expect(browser.getCurrentUrl()).toContain('/myaccount');
    });

    it('should add a new plan', function () {
        browser.sleep(3000);
        googleAuthorizationPage.addPlans();
        browser.waitForAngular();
        expect(browser.getCurrentUrl()).toContain('/plans/add');
    });

    it('should enter plan builder', function () {
        googleAuthorizationPage.setPlanName("test plan");
        browser.waitForAngular();
        googleAuthorizationPage.saveChanges();
        browser.sleep(3000);
        expect(element(by.className('fa fa-download action-category-icon')));
    });

    it('should get data from google', function () {
        browser.sleep(1000);
        browser.actions().mouseMove(element(by.className('fa fa-download action-category-icon'))).perform();
        googleAuthorizationPage.getDataGoogle();
        browser.waitForAngular();
        expect(element(by.className('modal-title')));
        browser.sleep(1000);
        browser.executeScript('window.scrollTo(0,0);');
        browser.sleep(1000);
        googleAuthorizationPage.googleActivity();
        browser.sleep(500);
        googleAuthorizationPage.getGoogleSheetData();
        browser.waitForAngular();
        expect(element(by.className('panel-heading ng-binding')));
        browser.sleep(3000);
    });

    it('should authorize to Google activity', function () {
        googleAuthorizationPage.addAccount();
        browser.sleep(1000);
        googleAuthorizationPage.setEmailPopupInput("fr8test1@gmail.com");
        googleAuthorizationPage.continuePopup();
        browser.sleep(1000);
        googleAuthorizationPage.setPasswordPopupInput("volant34");
        googleAuthorizationPage.signInPopup();
        browser.waitForAngular();
        expect(element(by.xpath('//*[@id="grant_heading"]')));
        browser.waitForAngular();
        googleAuthorizationPage.allow();
        //expect(element(by.xpath('//*[@id="ap-pane"]/div/div[1]/configuration-control/div/div/label')));
    });



});