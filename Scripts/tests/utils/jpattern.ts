module dockyard.tests.utils {
    // Method that can be used for checking structure of some object
    export function matchObjectWithPattern(pattern: any, obj: any): any {
        var keys = Object.keys(pattern);

        for (var keyIndex = 0; keyIndex < keys.length; keyIndex++) {
            var x = keys[keyIndex];
            var objValue = obj[x];
            var patternValue = pattern[x];

            if (objValue === undefined) return x + ": " + objValue + " != undefined";

            if (typeof objValue === 'string' && typeof patternValue == 'object') {
                try {
                    objValue = JSON.parse(objValue);
                } catch (e) {
                    return x + " -> not a valid JSON";
                }
            }

            if (typeof objValue !== typeof patternValue) return x + " -> incompatible types " + typeof objValue + " != " + typeof patternValue;

            if (objValue instanceof Array && patternValue instanceof Array) {

                for (var i = 0; i < patternValue.length; i++) {
                    var result;

                    for (var j = 0; j < objValue.length; j++) {

                        result = matchObjectWithPattern(patternValue[i], objValue[j]);
                        if (result === true) break;
                    }

                    if (result !== true) return x + " -> " + result;
                }

                continue;
            }

            if (objValue instanceof Array || patternValue instanceof Array) return x + " -> can't match variables of type " + typeof objValue + " and " + typeof patternValue;

            if (typeof objValue === 'object') {
                var res = matchObjectWithPattern(patternValue, objValue);

                if (res !== true) {
                    return x + " -> " + res;
                }

                continue;
            }

            if (objValue !== patternValue) {
                return x + " -> " + patternValue + " != " + objValue;
            }

            continue;
        }

        return true;
    }
}