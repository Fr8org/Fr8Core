var LoginPage = require('../pages/login.page.js');

describe('login page tests', function () {
    var loginPage;

    //beforeEach(function () {
        loginPage = new LoginPage();
        loginPage.get();
    //});

    /* For browser console error logs*/
        afterEach(function (done) {
            browser.manage().logs().get('browser').then(function (browserLog) {
                var i = 0,
                severWarnings = false;
                for (i; i <= browserLog.length - 1; i++) {
                    if (browserLog[i].level.name === 'SEVERE') {
                        console.log('\n' + browserLog[i].level.name);
                        console.log('(Possibly exception) \n' + browserLog[i].message)
                        severWarnings = true;
                    }
                }
                expect(severWarnings).toBe(true);
                done();
            });
        });


    it('should login', function () {
        loginPage.setEmail("integration_test_runner@fr8.company");
        loginPage.setPassword("fr8#s@lt!");
        loginPage.login();
        browser.waitForAngular();
        expect(browser.getCurrentUrl()).toContain('Welcome');
    });

    it('should open my plans page', function () {
        browser.sleep(2000);
        loginPage.myAccount();
        browser.waitForAngular();
        expect(browser.getCurrentUrl()).toContain('/myaccount');
    });

    it('should be logout', function () {
        browser.sleep(4000);
        loginPage.selectDropDownByName();
        browser.waitForAngular();
        expect(browser.getCurrentUrl()).toContain('/DockyardAccount');
    });

});
 
   