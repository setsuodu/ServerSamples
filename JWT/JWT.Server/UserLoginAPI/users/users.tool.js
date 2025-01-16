const config = require('../config.js');
const crypto = require('crypto');
const Email = require('../tools/email.tool');
const AuthTool = require('../authorization/auth.tool');

const UserTool = exports;

UserTool.generateID = function(length, easyRead) {
    var result           = '';
    var characters       = 'abcdefghijklmnopqrstuvwxyz0123456789';
    if(easyRead)
        characters  = 'abcdefghijklmnpqrstuvwxyz123456789'; //Remove confusing characters like 0 and O
    var charactersLength = characters.length;
    for ( var i = 0; i < length; i++ ) {
       result += characters.charAt(Math.floor(Math.random() * charactersLength));
    }
    return result;
 }

 UserTool.sendEmailConfirmKey = (user, email, email_confirm_key) => {

    if(!email || !user) return;

    var subject = config.api_title + " - Email Confirmation";
    var http = config.allow_https ? "https://" : "http://";
    var confirm_link = http + config.api_url + "/users/email/confirm/" + user.id + "/" + email_confirm_key;

    var text = "Hello " + user.username + "<br>";
    text += "Welcome! <br><br>";
    text += "To confirm your email, click here: <br><a href='" + confirm_link + "'>" + confirm_link + "</a><br><br>";
    text += "Thank you and see you soon!<br>";

    Email.SendEmail(email, subject, text, function(result){
        console.log("Sent email to: " + email + ": " + result);
    });

};

UserTool.sendEmailChangeEmail = (user, email, new_email) => {

    if(!email || !user) return;

    var subject = config.api_title + " - Email Changed";

    var text = "Hello " + user.username + "<br>";
    text += "Your email was succesfully changed to: " + new_email + "<br>";
    text += "If you believe this is an error, please contact support immediately.<br><br>"
    text += "Thank you and see you soon!<br>";
    
    Email.SendEmail(email, subject, text, function(result){
        console.log("Sent email to: " + email + ": " + result);
    });
};

UserTool.sendEmailChangePassword = (user, email) => {

    if(!email || !user) return;

    var subject = config.api_title + " - Password Changed";

    var text = "Hello " + user.username + "<br>";
    text += "Your password was succesfully changed<br>";
    text += "If you believe this is an error, please contact support immediately.<br><br>"
    text += "Thank you and see you soon!<br>";

    Email.SendEmail(email, subject, text, function(result){
        console.log("Sent email to: " + email + ": " + result);
    });

};

UserTool.sendEmailPasswordRecovery = (user, email) => {

    if(!email || !user) return;

    var subject = config.api_title + " - Password Recovery";

    var text = "Hello " + user.username + "<br>";
    text += "Here is your password recovery code: " + user.password_recovery_key.toUpperCase() + "<br><br>";
    text += "Thank you and see you soon!<br>";

    Email.SendEmail(email, subject, text, function(result){
        console.log("Sent email to: " + email + ": " + result);
    });
};

UserTool.setUserPassword = (user, password) =>
{
    let newPass = AuthTool.hashPassword(password);
    user.password = newPass;
    user.password_recovery_key = ""; //After changing password, disable recovery until changed again
    user.refresh_key = crypto.randomBytes(16).toString('base64'); //Logout previous logins by changing the refresh_key
}

module.exports = UserTool;