var co = require('co');
var db = require('./db');

// errors can be try/catched
co(function *() {
    try {
        console.log(yield db.users.find({}));
    } catch (err) {
        console.error(err.message); // "boom"
    }
}).catch(onerror);

function onerror(err) {
    // log any uncaught errors
    // co will not throw any errors you do not handle!!!
    // HANDLE ALL YOUR ERRORS!!!
    console.error(err.stack);
}
