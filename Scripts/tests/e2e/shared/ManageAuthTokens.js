var ManageAuthTokens = function () {
    var toolsButton = element(by.xpath('/html/body/div[1]/div/div[2]/div[2]/div/div/ul/li[5]/a'));
    var authTokensButton = element(by.xpath('/html/body/div[1]/div/div[2]/div[2]/div/div/ul/li[5]/ul/li[1]/a'));
    var revokeButton = element(by.className('btn btn-danger btn-sm'));
    
    var docuSignAccount = element(by.xpath('/html/body/div[2]/div[2]/div/div/div/div/div/div[1]/div[1]/div/div/div/div[1]/div[2]/table/tbody/tr/td[1]/span[1]'));
    var dropBoxAccount = element(by.xpath('/html/body/div[2]/div[2]/div/div/div/div/div/div[1]/div[1]/div/div/div/div[2]/div[2]/table/tbody/tr/td[1]/span[1]'));
    var googleAccount = element(by.xpath('/html/body/div[2]/div[2]/div/div/div/div/div/div[1]/div[1]/div/div/div/div[3]/div[2]/table/tbody/tr/td[1]/span'));
    var slackAccount = element(by.xpath('/html/body/div[2]/div[2]/div/div/div/div/div/div[1]/div[1]/div/div/div/div[6]/div[2]/table/tbody/tr/td[1]/span[1]'));

    this.get = function () {
        browser.ignoreSynchronization = true;
        browser.get(browser.baseUrl + '/dashboard/manageAuthTokens');
    };

    this.tools = function () {
        return toolsButton.click();
    };

    this.authTokens = function () {
        return authTokensButton.click();
    };

    this.revoke = function () {
        return revokeButton.click();
    };


    if (docuSignAccount == null ) {
        return;
    }
    else
        authTokens();

    if (dropBoxAccount === 'undefined') {
        return;
    }
    else
        authTokens();

    if(googleAccount == null) {
        return;
    } 
    else
        authTokens();


};
module.exports = ManageAuthTokens;