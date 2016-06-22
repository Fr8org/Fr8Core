var LinksPage = function () {
    /* for navigation menu items */
    var howItWorksButton = element(by.xpath('//*[@id="main-nav"]/ul/li[1]/a'));
    var developerButton = element(by.xpath('//*[@id="main-nav"]/ul/li[3]/a'));
    var blogButton = element(by.xpath('//*[@id="main-nav"]/ul/li[6]/a'));
    /* for company dropdown */
    var servicesButton = element(by.xpath('//*[@id="main-nav"]/ul/li[2]/a'));
    var docuSignButton = element(by.xpath('//*[@id="main-nav"]/ul/li[2]/ul/li[1]/a'));
    var salesForceButton = element(by.xpath('//*[@id="main-nav"]/ul/li[2]/ul/li[2]/a'));
    /* for company dropdown */
    var companyButton = element(by.xpath('//*[@id="main-nav"]/ul/li[4]/a'));
    var visionButton = element(by.xpath('//*[@id="main-nav"]/ul/li[4]/ul/li[1]/a'));
    var teamButton = element(by.xpath('//*[@id="main-nav"]/ul/li[4]/ul/li[2]/a'));
    var pressReleasesButton = element(by.xpath('//*[@id="main-nav"]/ul/li[4]/ul/li[3]/a'));
    var locationButton = element(by.xpath('//*[@id="main-nav"]/ul/li[4]/ul/li[4]/a'));
    var jobsButton = element(by.xpath('//*[@id="main-nav"]/ul/li[4]/ul/li[5]/a'));

    var learnMoreButton = element(by.xpath('//*[@id="about"]/div/div[2]/div/div/p[2]/a'));
    var tryItNowButton = element(by.id('btn-contact'));
    var readMoreButton = element(by.id('btn-about'));
    var twitterButton = element(by.xpath('//*[@id="footer"]/div/div/a/i'));

    /* read more buttons */
    var readMoreDocuSignButton = element(by.xpath('//*[@id="services"]/div/div[2]/div[1]/div[1]/div/div/a/span'));
    var readMoreSalesForceButton = element(by.xpath('//*[@id="services"]/div/div[2]/div[1]/div[2]/div/div/a/span'));

    var PressLogo = element(by.xpath('//*[@id="site-header"]/nav/div/div[1]/a/img'));

    this.get = function () {
        browser.ignoreSynchronization = true;
        browser.get(browser.baseUrl);
    };
    
    this.readMoreSalesForce = function () {
        return readMoreSalesForceButton.click();
    };

    this.readMoreDocuSing = function () {
        return readMoreDocuSignButton.click();
    };

    this.logo = function () {
        return PressLogo.click();
    };

    this.howItWorks = function () {
        return howItWorksButton.click();
    };
    
    this.learnMore = function () {
        return learnMoreButton.click();
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

    this.developer = function () {
        return developerButton.click();
    };

    this.blog = function () {
        return blogButton.click();
    };

    this.company = function () {
        return companyButton.click();
    };

    this.vision = function () {
        return visionButton.click();
    };

    this.team = function () {
        return teamButton.click();
    };

    this.pressReleases = function () {
        return pressReleasesButton.click();
    };

    this.location = function () {
        return locationButton.click();
    };

    this.jobs = function () {
        return jobsButton.click();
    };

    this.tryItNow = function () {
        return tryItNowButton.click();
    };

    this.readMore = function () {
        return readMoreButton.click();
    };

    this.twitter = function () {
        return twitterButton.click();
    };
};

module.exports = LinksPage;