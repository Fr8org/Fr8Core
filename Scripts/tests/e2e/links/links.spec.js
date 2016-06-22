var LinksPage = require('../pages/links.page.js');

describe('test all links in application' , function(){
    var linksPage;
    linksPage = new LinksPage();
    linksPage.get();

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


    it('should be open how it works page', function () {
        browser.sleep(2000);
        linksPage.howItWorks();
        browser.sleep(2000);
        linksPage.learnMore();
        browser.waitForAngular();
        expect(browser.getCurrentUrl()).toContain('/how-it-works');
        browser.navigate().back();
        browser.sleep(3000);
    });

    it('should direct to DocuSign', function () {
        linksPage.service();
        //browser.actions().mouseMove(element(by.xpath('//*[@id="main-nav"]/ul/li[2]/a'))).perform();
        browser.sleep(2000);
        linksPage.service();
        browser.sleep(1000);
        linksPage.docuSign();
        browser.waitForAngular();
        expect(browser.getCurrentUrl()).toContain('/Services/DocuSign');
        browser.sleep(1000);
        linksPage.logo();
        browser.waitForAngular();
        expect(browser.baseUrl);
        browser.sleep(2000);
    });

    it('should direct to SalesForce', function () {
        linksPage.service();
        browser.sleep(2000);
        linksPage.service();
        browser.sleep(1000);
        linksPage.readMoreSalesForce();
        browser.waitForAngular();
        expect(browser.getCurrentUrl()).toContain('/Services/Salesforce');
        browser.sleep(2000);
        linksPage.logo();
        browser.waitForAngular();
        expect(browser.baseUrl);
        browser.sleep(2000);
    });

    it('should direct to vision', function () {
        linksPage.company();
        browser.sleep(1000);
        linksPage.company();
        browser.sleep(2000);
        linksPage.vision();
        browser.waitForAngular();
        expect(browser.getCurrentUrl()).toContain('/Company/#vision');
        browser.sleep(2000);
        linksPage.logo();
        browser.waitForAngular();
        expect(browser.baseUrl);
        browser.sleep(2000);
    });

    it('should direct to team', function () {
        linksPage.company();
        browser.sleep(1000);
        linksPage.company();
        browser.sleep(2000);
        linksPage.team();
        browser.waitForAngular();
        expect(browser.getCurrentUrl()).toContain('/Company/#team');
        browser.sleep(2000);
        linksPage.logo();
        browser.waitForAngular();
        expect(browser.baseUrl);
        browser.sleep(2000);
    });

    it('should direct to Press Releases', function () {
        linksPage.company();
        browser.sleep(2000);
        linksPage.company();
        browser.sleep(2000);
        linksPage.pressReleases();
        browser.waitForAngular();
        expect(browser.getCurrentUrl()).toContain('/Company/#press');
        browser.sleep(2000);
        linksPage.logo();
        browser.waitForAngular();
        expect(browser.baseUrl);
        browser.sleep(2000);
    });

    it('should direct to location', function () {
        linksPage.company();
        browser.sleep(2000);
        linksPage.company();
        browser.sleep(2000);
        linksPage.location();
        browser.waitForAngular();
        expect(browser.getCurrentUrl()).toContain('/Company/#location');
        browser.sleep(2000);
        linksPage.logo();
        browser.waitForAngular();
        expect(browser.baseurl);
        browser.sleep(2000);
    });

    it('should direct to Jobs', function () {
        linksPage.company();
        browser.sleep(2000);
        linksPage.company();
        browser.sleep(2000);
        linksPage.jobs();
        browser.waitForAngular();
        expect(browser.getCurrentUrl()).toContain('/Company/#jobs');
        browser.sleep(2000);
        linksPage.logo();
        browser.waitForAngular();
        expect(browser.baseUrl);
        browser.sleep(2000);
    });

    it('should direct to DocuSign and SalesForce with readmore buttons', function () {
        linksPage.service();
        browser.sleep(2000);
        linksPage.readMoreDocuSing();
        browser.waitForAngular();
        expect(browser.getCurrentUrl()).toContain('/Services/DocuSign');
        browser.sleep(2000);
        linksPage.logo();
        browser.waitForAngular();
        expect(browser.baseUrl);
        browser.sleep(2000);
        linksPage.service();
        browser.sleep(1000);
        linksPage.readMoreSalesForce();
        browser.waitForAngular();
        expect(browser.getCurrentUrl()).toContain('/Services/Salesforce');
        browser.sleep(2000);
        linksPage.logo();
        browser.waitForAngular();
        expect(browser.baseUrl);
        browser.sleep(3000);
    });

    it('should direct to Developers Page', function () {
        browser.sleep(2000);
        linksPage.developer();
        browser.sleep(3000);
        expect(browser.getCurrentUrl()).toContain('/github.com');
        browser.sleep(2000);
        browser.navigate().back();
        browser.sleep(3000);
    });

    it('should direct to blog page', function () {
        linksPage.blog();
        browser.sleep(2000);
        browser.getAllWindowHandles().then(function (handles) {
            browser.driver.switchTo().window(handles[1]);
        });
        //browser.actions().keyDown(protractor.Key.CONTROL).sendKeys('w').perform(); // if we need  close current tab
        browser.sleep(2000);
        expect(element(by.xpath('//*[@id="post-731"]/header/h2/a')));
        browser.sleep(2000);
        browser.getAllWindowHandles().then(function (handles) {
            browser.driver.switchTo().window(handles[0]);
        });
        browser.sleep(2000);
    });

    it('should direct to twitter page', function () {
        linksPage.twitter();
        browser.sleep(3000);
        expect(browser.getCurrentUrl()).toContain('/TheFr8Company');
        browser.sleep(3000);
    });

});