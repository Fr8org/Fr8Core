var MyAccountPage = function () {
    var selectDropDownByName = element(by.xpath('/html/body/div[1]/div/div[2]/div[1]/div/div[2]/ul/li/a/span'));
    var logoutButton = element(by.xpath('/html/body/div[1]/div/div[2]/div[1]/div/div[2]/ul/li/ul/li[3]/a'));

    this.get = function () {
        browser.ignoreSynchronization = true;
        browser.get(browser.baseUrl + '/dashboard/myaccount');
    };

    this.selectDropDownByName = function () {
        selectDropDownByName.click();
        return logoutButton.click();
    };

};
module.exports = MyAccountPage;