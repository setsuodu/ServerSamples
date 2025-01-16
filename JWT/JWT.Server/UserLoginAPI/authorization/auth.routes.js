
const AuthController = require('./auth.controller');
const AuthTool = require('./auth.tool');

exports.route = function (app) {
	
    //Body: username, password
    app.post('/auth', app.auth_limiter, [
        AuthTool.isLoginValid,
        AuthController.login
    ]);

    //Body: refresh_token
    app.post('/auth/refresh', app.auth_limiter, [
        AuthTool.isRefreshValid,
        AuthController.login
    ]);

    app.get('/auth/validate',[ 
        AuthTool.isValidJWT,
        AuthController.validatetoken
    ]);

    app.get("/auth/proof/create", [
        AuthTool.isValidJWT,
        AuthController.createProof
    ]);

    app.get("/auth/proof/:username/:proof", [
        AuthTool.isValidJWT,
        AuthController.validateProof
    ]);

    app.get('/version', [
        AuthController.get_version
    ]);

    
};