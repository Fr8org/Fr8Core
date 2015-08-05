// Tests for RxJS-BackPressure TypeScript definitions
// Tests by Igor Oleinikov <https://github.com/Igorbek>
///<reference path="rx.d.ts" />
///<reference path="rx.backpressure.d.ts" />
function testPausable() {
    var o;
    var pauser = new Rx.Subject();
    var p = o.pausable(pauser);
    p = o.pausableBuffered(pauser);
}
function testControlled() {
    var o;
    var c = o.controlled();
    var d = c.request();
    d = c.request(5);
}
//# sourceMappingURL=rx.backpressure-tests.js.map