const UserModel = require('./users.model');
const UserTool = require('./users.tool');
const DateTool = require('../tools/date.tool');
const Activity = require("../activity/activity.model");
const Validator = require('../tools/validator.tool');
const AuthTool = require('../authorization/auth.tool');
const Email = require('../tools/email.tool');
const config = require('../config');

//Register new user
exports.registerUser = async (req, res, next) => {

    if(!req.body.email || !req.body.username || !req.body.password){
        return res.status(400).send({error: 'Invalid parameters'});
    }

    var email = req.body.email;
    var username = req.body.username;
    var password = req.body.password;
    var avatar = req.body.avatar || "";

    if(!Validator.validateUsername(username)){
        return res.status(400).send({error: 'Invalid username'});
    }

    if(!Validator.validateEmail(email)){
        return res.status(400).send({error: 'Invalid email'});
    }

    if(!Validator.validatePassword(password)){
        return res.status(400).send({error: 'Invalid password'});
    }

    if(avatar && typeof avatar !== "string")
        return res.status(400).json({error: "Invalid avatar"});

    var user_username = await UserModel.getByUsername(username);
    var user_email = await UserModel.getByEmail(email);

    if(user_username)
        return res.status(400).send({error: 'Username already exists'});
    if(user_email)
        return res.status(400).send({error: 'Email already exists'});

    var user = {};

    user.username = username;
    user.email = email;
    user.avatar = avatar;
    user.permission_level = 1;
    user.validation_level = 0;

    user.account_create_time = new Date();
    user.last_login_time = new Date();
    user.email_confirm_key = UserTool.generateID(20);

    UserTool.setUserPassword(user, password);

    //Create user
    var nUser = await UserModel.createUser(user);
    if(!nUser)
        return res.status(500).send({ error: "Unable to create user" });
    
    //Send confirm email
    UserTool.sendEmailConfirmKey(nUser, user.email, user.email_confirm_key);

    // Activity Log -------------
    const activityData = {username: user.username, email: user.email };
    const act = await Activity.LogActivity("register", user.username, activityData);
    if (!act) return res.status(500).send({ error: "Failed to log activity!!" });

    //Return response
    return res.status(200).send({ success: true, id: nUser._id });
};

exports.getAll = async(req, res) => {

    let user_permission_level = parseInt(req.jwt.permission_level);
    let is_admin = (user_permission_level >= config.permissions.SERVER);
    
    var list = await UserModel.list();
    for(var i=0; i<list.length; i++){
        if(is_admin)
            list[i] = list[i].deleteSecrets();
        else
            list[i] = list[i].deleteAdminOnly();
    }

    return res.status(200).send(list);
};

exports.getUser = async(req, res) => {
    var user = await UserModel.getById(req.params.userId);
    if(!user)
        user = await UserModel.getByUsername(req.params.userId);

    if(!user)
        return res.status(404).send({error: "User not found " + req.params.userId});
    
    let user_permission_level = parseInt(req.jwt.permission_level);    
    let is_admin = (user_permission_level >= config.permissions.SERVER);
    if(is_admin || req.params.userId == req.jwt.userId || req.params.userId == req.jwt.username)
        user = user.deleteSecrets();
    else
        user = user.deleteAdminOnly();

    user.server_time = new Date(); //Return server time
    return res.status(200).send(user);
};

exports.editUserAccount = async(req, res) => {

    var userId = req.params.userId;
    var avatar = req.body.avatar;

    if(!userId || typeof userId !== "string")
        return res.status(400).json({error: "Invalid parameters"});

    if(avatar && typeof avatar !== "string")
        return res.status(400).json({error: "Invalid avatar"});

    var user = await UserModel.getById(userId);
    if(!user)
        return res.status(404).send({error: "User not found: " + userId});

    var userData = {};

    //Change avatar
    if(avatar && avatar.length < 50)
        userData.avatar = avatar;

    //Add other variables you'd like to be able to edit here
    //Avoid allowing changing username, email or password here, since those require additional security validations and should have their own functions

    //Update user
    var result = await UserModel.update(user, userData);
    if(!result)
        return res.status(400).send({error: "Error updating user: " + userId});

    return res.status(200).send(result.deleteAdminOnly());
};

exports.editEmail = async(req, res) => {

    var userId = req.jwt.userId;
    var email = req.body.email;

    if(!userId || typeof userId !== "string")
        return res.status(400).json({error: "Invalid parameters"});

    if(!email || !Validator.validateEmail(email))
        return res.status(400).json({error: "Invalid email"});

    var user = await UserModel.getById(userId);
    if(!user)
        return res.status(404).send({error: "User not found: " + userId});

    if(email == user.email)
        return res.status(400).send({error: "Email unchanged"});

    //Find email
    var foundUserEmail = await UserModel.getByEmail(email);
    if(foundUserEmail)
        return res.status(403).json({error: "Email already exists"});

    var prev_email = user.email;
    var userData = {};
    userData.email = email;
    userData.validation_level = 0;
    userData.email_confirm_key = UserTool.generateID(20);
    
    //Update user
    var result = await UserModel.update(user, userData);
    if(!result)
        return res.status(400).send({error: "Error updating user email: " + userId});

    //Send confirmation email
    UserTool.sendEmailConfirmKey(user, email, userData.email_confirm_key);
    UserTool.sendEmailChangeEmail(user, prev_email, email);

    // Activity Log -------------
    const activityData = {prev_email: prev_email, new_email: email };
    const a = await Activity.LogActivity("edit_email", req.jwt.username, {activityData});
    if (!a) return res.status(500).send({ error: "Failed to log activity!!" });

    return res.status(200).send(result.deleteAdminOnly());
};

exports.editPassword = async(req, res) => {

    var userId = req.jwt.userId;
    var password = req.body.password_new;
    var password_previous = req.body.password_previous;

    if(!userId || typeof userId !== "string")
        return res.status(400).json({error: "Invalid parameters"});

    if(!password || !password_previous || typeof password !== "string" || typeof password_previous !== "string")
        return res.status(400).json({error: "Invalid parameters"});
    
    var user = await UserModel.getById(userId);
    if(!user)
        return res.status(404).send({error: "User not found: " + userId});

    let validPass = AuthTool.validatePassword(user, password_previous);
    if(!validPass)
        return res.status(401).json({error: "Invalid previous password"});

    UserTool.setUserPassword(user, password);

    var result = await UserModel.save(user, ["password", "refresh_key", "password_recovery_key"]);
    if(!result)
        return res.status(500).send({error: "Error updating user password: " + userId});

    //Send confirmation email
    UserTool.sendEmailChangePassword(user, user.email);

    // Activity Log -------------
    const a = await Activity.LogActivity("edit_password", req.jwt.username, {});
    if (!a) return res.status(500).send({ error: "Failed to log activity!!" });

    return res.status(204).send({});
};

exports.editPermissions = async(req, res) => {

    var userId = req.params.userId;
    var permission_level = req.body.permission_level;

    if(!userId || typeof userId !== "string")
        return res.status(400).json({error: "Invalid parameters"});

    if(!Validator.isInteger(permission_level))
        return res.status(400).json({error: "Invalid permission"});

    var user = await UserModel.getById(userId);
    if(!user)
        return res.status(404).send({error: "User not found: " + userId});

    var userData = {};

    //Change avatar
    userData.permission_level = permission_level;

    //Update user
    var result = await UserModel.update(user, userData);
    if(!result)
        return res.status(400).send({error: "Error updating user: " + userId});

    // Activity Log -------------
    const activityData = {username: user.username, permission_level: userData.permission_level };
    const act = await Activity.LogActivity("edit_permission", req.jwt.username, activityData);
    if (!act) return res.status(500).send({ error: "Failed to log activity!!" });

    return res.status(200).send(result.deleteSecrets());
};

exports.resetPassword = async(req, res) => {

    var email = req.body.email;

    if(!config.smtp_enabled)
        return res.status(400).json({error: "Email SMTP is not configured"});

    if(!email || typeof email !== "string")
        return res.status(400).json({error: "Invalid parameters"});

    var user = await UserModel.getByEmail(email);
    if(!user)
        return res.status(404).send({error: "User not found: " + email});

    user.password_recovery_key = UserTool.generateID(10, true);
    await UserModel.save(user, ["password_recovery_key"]);

    UserTool.sendEmailPasswordRecovery(user, email);

    return res.status(204).send({});
};


exports.resetPasswordConfirm = async(req, res) => {

    var email = req.body.email;
    var code = req.body.code;
    var password = req.body.password;

    if(!email || typeof email !== "string")
        return res.status(400).json({error: "Invalid parameters"});

    if(!code || typeof code !== "string")
        return res.status(400).json({error: "Invalid parameters"});

    if(!password || typeof password !== "string")
        return res.status(400).json({error: "Invalid parameters"});
    
    var user = await UserModel.getByEmail(email);
    if(!user)
        return res.status(404).send({error: "User not found: " + email});

    if(!user.password_recovery_key || user.password_recovery_key.toUpperCase() != code)
        return res.status(403).json({error: "Invalid Recovery Code"});
    
    UserTool.setUserPassword(user, password);

    var result = await UserModel.save(user, ["password", "refresh_key", "password_recovery_key"]);
    if(!result)
        return res.status(500).send({error: "Error updating user password: " + email});

    return res.status(204).send({});
};

exports.removeById = async(req, res) => {
    UserModel.removeById(req.params.userId);
    return res.status(204).send({});
};

exports.gainReward = async(req, res) =>
{
    var userId = req.params.userId;
    var coin = req.body.coin || 0;
    var xp = req.body.xp || 0;

    if(coin && typeof coin !== "number")
        return res.status(400).json({error: "Invalid parameters"});
        
    if(xp && typeof xp !== "number")
        return res.status(400).json({error: "Invalid parameters"});

    var user = await UserModel.getById(userId);
    if(!user)
        return res.status(404).send({error: "User not found: " + userId});

    //Add other rewards to gain here
    user.coin += coin;
    user.xp += xp;

    var result = await UserModel.save(user, ["coin", "xp"]);
    if(!result)
        return res.status(500).send({error: "Error gaining user reward"});

    // Activity Log -------------
    const activityData = { username: user.username, coin: coin, xp: xp };
    const act = await Activity.LogActivity("gain_reward", req.jwt.username, activityData);
    if (!act) return res.status(500).send({ error: "Failed to log activity!!" });

    return res.status(200).send(result);
};

//In this function all message are returned in direct text because the email link is accessed from browser
exports.confirmEmail = async (req, res) =>{

    if(!req.params.userId || !req.params.code){
        return res.status(404).send("Code invalid");
    }

    var user = await UserModel.getById(req.params.userId);
    if(!user)
        return res.status(404).send("Code invalid");

    if(user.email_confirm_key != req.params.code)
        return res.status(404).send("Code invalid");

    if(user.validation_level >= 1)
        return res.status(400).send("Email already confirmed!");

    //Code valid!
    var data = {validation_level: Math.max(user.validation_level, 1)};
    await UserModel.update(user, data);

    return res.status(200).send("Email confirmed!");
};

exports.resendEmail = async(req, res) => 
{
    var userId = req.jwt.userId;
    var user = await UserModel.getById(userId);
    if(!user)
        return res.status(404).send({error: "User not found " + userId});

    if(user.validation_level > 0)
        return res.status(403).send({error: "Email already confirmed"});

    UserTool.sendEmailConfirmKey(user, user.email, user.email_confirm_key);

    return res.status(200).send();
}

exports.sendEmail = async (req, res) =>{

    var subject = req.body.title;
    var text = req.body.text;
    var email = req.body.email;

    if(!subject || typeof subject !== "string")
        return res.status(400).json({error: "Invalid parameters"});

    if(!text || typeof text !== "string")
        return res.status(400).json({error: "Invalid parameters"});

    if(!email || typeof email !== "string")
        return res.status(400).json({error: "Invalid parameters"});

    Email.SendEmail(email, subject, text, function(result){
        console.log("Sent email to: " + email + ": " + result);
        return res.status(200).send({success: result});
    });
};

exports.getOnline = async(req, res) =>
{
    //Count online users
    var time = new Date();
    time = DateTool.addMinutes(time, -10);

    var count = 0;
    var users = await UserModel.list();
    var usernames = [];
    for(var i=0; i<users.length; i++)
    {
        var user = users[i];
        if(user.last_login_time > time)
        {
            usernames.push(user.username);
            count++;
        }
    }
    return res.status(200).send({online: count, total: users.length, users: usernames});
};