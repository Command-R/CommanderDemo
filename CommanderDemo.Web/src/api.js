angular.module("app").service("api", [
    'jsonRpc',
    function (jsonRpc) {
        var self = {};
        self.LoginUser = function() {
            return jsonRpc.send("LoginUser", Array.prototype.slice.call(arguments));
        };
        self.LogoutUser = function() {
            return jsonRpc.send("LogoutUser", Array.prototype.slice.call(arguments));
        };
        self.DeleteContact = function() {
            return jsonRpc.send("DeleteContact", Array.prototype.slice.call(arguments));
        };
        self.GetContact = function() {
            return jsonRpc.send("GetContact", Array.prototype.slice.call(arguments));
        };
        self.QueryContacts = function() {
            return jsonRpc.send("QueryContacts", Array.prototype.slice.call(arguments));
        };
        self.SaveContact = function() {
            return jsonRpc.send("SaveContact", Array.prototype.slice.call(arguments));
        };
        self.DeleteUser = function() {
            return jsonRpc.send("DeleteUser", Array.prototype.slice.call(arguments));
        };
        self.GetUser = function() {
            return jsonRpc.send("GetUser", Array.prototype.slice.call(arguments));
        };
        self.QueryUsers = function() {
            return jsonRpc.send("QueryUsers", Array.prototype.slice.call(arguments));
        };
        self.SaveUser = function() {
            return jsonRpc.send("SaveUser", Array.prototype.slice.call(arguments));
        };
        return self;
    }
]);
