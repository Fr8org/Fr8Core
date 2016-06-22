var SalesForceAuthorizationPage = require('../pages/salesForceAuthorization.page.js');

describe('SalesForce Authorization pathway test', function () {
    var salesForceAuthorizationPage;
    salesForceAuthorizationPage = new SalesForceAuthorizationPage();
    salesForceAuthorizationPage.get();

    /* For browser console error logs*/
    //afterEach(function (done) {
    //    browser.manage().logs().get('browser').then(function (browserLog) {
    //        var i = 0,
    //        severWarnings = false;
    //        for (i; i <= browserLog.length - 1; i++) {
    //            if (browserLog[i].level.name === 'SEVERE') {
    //                console.log('\n' + browserLog[i].level.name);
    //                console.log('(Possibly exception) \n' + browserLog[i].message)
    //                severWarnings = true;
    //            }
    //        }
    //        expect(severWarnings).toBe(true);
    //        done();
    //    });
    //});

    it('should login', function () {
        salesForceAuthorizationPage.setEmail("integration_test_runner@fr8.company");
        salesForceAuthorizationPage.setPassword("fr8#s@lt!");
        salesForceAuthorizationPage.login();
        browser.waitForAngular();
        expect(browser.getCurrentUrl()).toContain('Welcome');
    });

    it('should open my plans page', function () {
        browser.sleep(2000);
        salesForceAuthorizationPage.myAccount();
        browser.waitForAngular();
        expect(browser.getCurrentUrl()).toContain('/myaccount');
    });

    it('should add a new plan', function () {
        browser.sleep(3000);
        salesForceAuthorizationPage.addPlans();
        browser.waitForAngular();
        expect(browser.getCurrentUrl()).toContain('/plans/add');
    });

    it('should enter plan builder', function () {
        salesForceAuthorizationPage.setPlanName("test plan");
        browser.waitForAngular();
        salesForceAuthorizationPage.saveChanges();
        browser.sleep(3000);
        expect(element(by.className('fa fa-download action-category-icon')));
    });

    it('should add Monitor SalesForce activity', function () {
        browser.sleep(1000);
        browser.actions().mouseMove(element(by.className('fa fa-download action-category-icon'))).perform();
        salesForceAuthorizationPage.monitorIcon();
        browser.waitForAngular();
        expect(element(by.className('modal-title')));
        browser.sleep(1000);
        browser.executeScript('window.scrollTo(0,0);');
        browser.sleep(1000);
        salesForceAuthorizationPage.salesForceActivity();
        browser.sleep(500);
        salesForceAuthorizationPage.monitorSalesForce();
        browser.waitForAngular();
        expect(element(by.className('panel-heading ng-binding')));
        browser.sleep(3000);        
    });

    //it('should authorize to SalesForce', function () {
    //    salesForceAuthorizationPage.addAccount();
    //    browser.sleep(1000);
    //    salesForceAuthorizationPage.setUsername("alex@dockyard.company");
    //    salesForceAuthorizationPage.setPasswordPopup("thales@123");
    //    salesForceAuthorizationPage.loginButtonPopup();
    //    browser.waitForAngular();
    //    expect(element(by.xpath('/html/body/div[4]/div/div/div[2]/div[2]/div[2]/div[1]/p')));
    //    salesForceAuthorizationPage.ok();
    //    browser.waitForAngular();
    //    expect(element(by.xpath('//*[@id="ap-pane"]/div/div[1]/configuration-control/div/div/label')));
    //});


    //this.selectWindow = function (index) {
    //    // wait for handels[index] to exists
    //    browser.driver.wait(function () {
    //        return browser.driver.getAllWindowHandles().then(function (handles) {
    //            /**
    //             * Assume that handles.length >= 1 and index >=0.
    //             * So when i call selectWindow(index) i return
    //             * true if handles contains that window.
    //             */
    //            if (handles.length > index) {
    //                return true;
    //            }
    //        });
    //    });
    //    // here i know that the requested window exists

    //    // switch to the window
    //    return browser.driver.getAllWindowHandles().then(function (handles) {
    //        return browser.driver.switchTo().window(handles[index]);
    //    });
    //};

});