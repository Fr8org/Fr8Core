var CreatePlanPage = require('../pages/createPlan.page.js');

describe('testing for adding DocuSign and SalesForce Solution plan', function () {
    var createPlanPage;
    createPlanPage = new CreatePlanPage();
    createPlanPage.get();
    var url = "http://dev.fr8.co/Welcome";

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


    it('should create Extract Data From Envelope Plan without login', function () {
        browser.sleep(2000);
        createPlanPage.tryItNow();
        browser.sleep(2000);
        expect(browser.getCurrentUrl()).toContain('/Welcome');
        browser.sleep(1000);
        createPlanPage.docuSignSolution();
        browser.sleep(2000);
        expect(browser.getCurrentUrl()).toContain('/Services/DocuSign');
        browser.sleep(2000);
        createPlanPage.extractDataFromEnvelope();
        browser.sleep(2000);
        expect(element(by.xpath('/html/body/div[4]/div/div/div[1]/h4')));
        browser.sleep(2000);
    });

    it('should click logo button for retun home page', function () {
        browser.get(url);
        browser.sleep(2000);
        expect(element(by.className('hero-heading')));
        browser.sleep(2000);
    });

    it('should create Track DocuSign Recipients Plan without login', function () {
        createPlanPage.docuSignSolution();
        browser.sleep(2000);
        expect(browser.getCurrentUrl()).toContain('/Services/DocuSign');
        browser.sleep(2000);
        createPlanPage.trackDocuSignRecipients();
        browser.sleep(2000);
        expect(element(by.xpath('/html/body/div[4]/div/div/div[2]/div[2]/div[1]')));
        browser.sleep(2000);
    });

    it('should click logo button for retun home page', function () {
        browser.get(url);
        browser.sleep(2000);
        expect(element(by.className('hero-heading')));
        browser.sleep(1000);
    });

    it('should create Mail Merge into DocuSign Plan without login', function () {
        browser.sleep(2000);
        createPlanPage.docuSignSolution();
        browser.sleep(2000);
        expect(browser.getCurrentUrl()).toContain('/Services/DocuSign');
        browser.sleep(2000);
        browser.executeScript('window.scrollTo(0,10000);');
        expect(element(by.className('col-md-11 h2')));
        browser.sleep(2000);
        createPlanPage.mailMergeIntoDocuSign();
        browser.sleep(2000);
        expect(element(by.xpath('/html/body/div[4]/div/div/div[2]/div[2]/div[1]')));
        browser.sleep(2000);
    });

    it('should click logo button for retun home page', function () {
        browser.get(url);
        browser.sleep(2000);
        expect(element(by.className('hero-heading')));
        browser.sleep(1000);
    });

    it('should create Mail Merge from SalesForce plan withot log in', function () {
        //browser.sleep(2000);
        //createPlanPage.tryItNow();
        //browser.sleep(2000);
        createPlanPage.service();
        browser.sleep(2000);
        createPlanPage.service();
        browser.sleep(2000);
        createPlanPage.salesForce();
        browser.sleep(1000);
        expect(browser.getCurrentUrl()).toContain('/Services/Salesforce');
        browser.sleep(1000);
        createPlanPage.mailMergeFromSalesForce();
        expect(element(by.xpath('/html/body/div[4]/div/div/div[2]/div[2]/div[1]')));
        browser.sleep(1000);
    });

    

});