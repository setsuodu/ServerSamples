const UsersController = require('./users.controller');
const UsersFriendsController = require("./users.friends.controller");
const AuthTool = require('../authorization/auth.tool');
const config = require('../config');

const ADMIN = config.permissions.ADMIN; //Highest permision, can read and write all users
const SERVER = config.permissions.SERVER; //Middle permission, can read all users and grant rewards
const USER = config.permissions.USER; //Lowest permision, can only do things on same user

exports.route = function (app) {

  //Body: username, email, password, avatar
  app.post("/users/register", app.auth_limiter, [
    UsersController.registerUser,
  ]);

  app.get("/users", [
    AuthTool.isValidJWT,
    AuthTool.isPermissionLevel(USER),
    UsersController.getAll,
  ]);

  app.get("/users/:userId", [
    AuthTool.isValidJWT,
    AuthTool.isPermissionLevel(USER),
    UsersController.getUser,
  ]);

  // USER - EDITS ----------------------

  //Body: avatar, userId allows an admin to edit another user
  app.post("/users/edit/:userId", app.post_limiter, [
    AuthTool.isValidJWT,
    AuthTool.isPermissionLevel(USER),
    AuthTool.isSameUserOr(ADMIN),
    UsersController.editUserAccount,
  ]);

  //Body: permission
  app.post("/users/permission/edit/:userId", app.post_limiter, [
    AuthTool.isValidJWT,
    AuthTool.isPermissionLevel(ADMIN),
    UsersController.editPermissions,
  ]);

  //Body: email
  app.post("/users/email/edit", app.auth_limiter, [
    AuthTool.isValidJWT,
    AuthTool.isPermissionLevel(USER),
    UsersController.editEmail,
  ]);

  //Body: password_previous, password_new
  app.post("/users/password/edit", app.auth_limiter, [
    AuthTool.isValidJWT,
    AuthTool.isPermissionLevel(USER),
    UsersController.editPassword,
  ]);

  //Body: email
  app.post("/users/password/reset", app.auth_limiter, [
    UsersController.resetPassword,
  ]);

  //body: email, code, password   (password is the new one)
  app.post("/users/password/reset/confirm", app.auth_limiter, [
    UsersController.resetPasswordConfirm,
  ]);
  
  /*app.post("/users/delete/:userId", app.post_limiter, [
    AuthTool.isValidJWT,
    AuthTool.isPermissionLevel(ADMIN),
    UsersController.removeById,
  ]);*/
  
  // USER - EMAIL CONFIRMATION ---------------------------

  //Email confirm
  app.get("/users/email/confirm/:userId/:code", [
    UsersController.confirmEmail,
  ]);

  //Ask to resend confirmation email
  app.post("/users/email/resend", app.auth_limiter, [
    AuthTool.isValidJWT,
    AuthTool.isPermissionLevel(USER),
    UsersController.resendEmail,
  ]);

  //Send a test email to one email address
  //body: title, text, email
  app.post("/users/email/send", app.auth_limiter, [
    AuthTool.isValidJWT,
    AuthTool.isPermissionLevel(ADMIN),
    UsersController.sendEmail,
  ]);

  // USER - Friends --------------------------------------

  //body: username (friend username)
  app.post("/users/friends/add/", app.post_limiter, [
    AuthTool.isValidJWT,
    AuthTool.isPermissionLevel(USER),
    UsersFriendsController.AddFriend,
  ]);

  //body: username (friend username)
  app.post("/users/friends/remove/", app.post_limiter, [
    AuthTool.isValidJWT,
    AuthTool.isPermissionLevel(USER),
    UsersFriendsController.RemoveFriend,
  ]);

  app.get("/users/friends/list/", [
    AuthTool.isValidJWT,
    AuthTool.isPermissionLevel(USER),
    UsersFriendsController.ListFriends,
  ]);

  // USER - REWARDS ---------------------------
  // Permissions set to game server so the user cant give to himself

  app.post("/users/rewards/gain/:userId", app.auth_limiter, [
    AuthTool.isValidJWT,
    AuthTool.isPermissionLevel(SERVER),
    UsersController.gainReward,
  ]);

  // USER - STATS ---------------------------

  app.get("/online", [
    UsersController.getOnline
  ]);

};