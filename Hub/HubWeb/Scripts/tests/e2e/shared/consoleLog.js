var consoleLog = function () {
    /* For browser console error logs*/
    browser.manage().logs().get('browser').then(function (browserLog) {
        var i = 0;
        for (i; i <= browserLog.length - 1; i++) {
            if (browserLog[i].level.name === 'SEVERE') {
                console.log('\n' + browserLog[i].level.name);
                console.log('(Possibly exception) \n' + browserLog[i].message)
                severWarnings = true;
            }
        }
        expect(severWarnings).toBe(false);
        done();
    });
};
module.exports = consoleLog;