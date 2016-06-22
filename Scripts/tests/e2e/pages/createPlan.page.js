var CreatePlanPage = function () {
    var servicesButton = element(by.xpath('//*[@id="main-nav"]/ul/li[2]/a'));
    var docuSignButton = element(by.xpath('//*[@id="main-nav"]/ul/li[2]/ul/li[1]/a'));
    var salesForceButton = element(by.xpath('//*[@id="main-nav"]/ul/li[2]/ul/li[2]/a'));
    var logoButton = element(by.xpath('/html/body/div[1]/div[2]/div/div/div/ul/li[1]/a/img'));
    var tryItNowButton = element(by.xpath('//*[@id="btn-contact"]'));
    var docuSignSolutionButton = element(by.xpath('//*[@id="hero-unit"]/div[2]/div[2]/div[1]/a/div/div[1]'));
    
    /* Plan create buttons */
    var extractDataFromEnvelopeButton = element(by.xpath('//*[@id="wrap"]/div[1]/div[1]/div[1]/a'));
    var trackDocuSignRecipientsButton = element(by.xpath('//*[@id="wrap"]/div[1]/div[5]/div[1]/a'));
    var mailMergeIntoDocuSignButton = element(by.xpath('//*[@id="wrap"]/div[1]/div[9]/div[1]/a'));
    var mailMergeFromSalesForceButton = element(by.xpath('//*[@id="wrap"]/div[1]/div[1]/div[1]/a'));
    

    this.get = function () {
        browser.ignoreSynchronization = true;
        browser.get(browser.baseUrl);
    };
    
    this.docuSignSolution = function () {
        return docuSignSolutionButton.click();
    };

    this.tryItNow = function () {
        return tryItNowButton.click();
    };

    this.logo = function () {
        return logoButton.click();
    };

    this.service = function () {
        return servicesButton.click();
    };

    this.docuSign = function () {
        return docuSignButton.click();
    };

    this.salesForce = function () {
        return salesForceButton.click();
    };

    this.extractDataFromEnvelope = function () {
        return extractDataFromEnvelopeButton.click();
    };

    this.trackDocuSignRecipients = function () {
        return trackDocuSignRecipientsButton.click();
    };

    this.mailMergeIntoDocuSign = function () {
        return mailMergeIntoDocuSignButton.click();
    };

    this.mailMergeFromSalesForce = function () {
        return mailMergeFromSalesForceButton.click();
    };
};
module.exports = CreatePlanPage;
