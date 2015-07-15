using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Web;
using System.Web.Security;
using CfgDotNet;
using CommandR.Authentication;
using JWT;

namespace CommanderDemo.Web
{
    /// <summary>
    /// NOT FOR PROD: This class is only for the demo which integrates, MVC, WebAPI, and FluentScheduler
    /// Manages Tokens for provided context data like Username. Store as little as possibly in the token,
    /// it can get big and is sent on each request.
    /// </summary>
    internal class TokenService : ITokenService
    {
        private readonly Settings _settings;

        public TokenService(Settings settings)
        {
            _settings = settings;
        }

        public string CreateToken(IDictionary<string, object> data)
        {
            //MVC
            var username = (string)data["Username"];
            FormsAuthentication.SetAuthCookie(username, true);

            //JWT
            var tokenId = JsonWebToken.Encode(data, _settings.Key, _settings.Algorithm);

            //Session
            if (HttpContext.Current.Session != null)
                HttpContext.Current.Session["TokenId"] = tokenId;

            return tokenId;
        }

        public IDictionary<string, object> GetTokenData(string tokenId)
        {
            if (string.IsNullOrWhiteSpace(tokenId))
                return new Dictionary<string, object>();

            try
            {
                var data = (IDictionary<string, object>)JsonWebToken.DecodeToObject(tokenId, _settings.Key, verify: true);
                return new Dictionary<string, object>
                {
                    {"Username", TryGet(data, "Username")},
                    {"Roles", ParseRoles(TryGet(data, "Roles"))},

                };
            }
            catch
            {
                return new Dictionary<string, object>();
            }
        }

        public void DeleteToken(string tokenId)
        {
            FormsAuthentication.SignOut();
            if (HttpContext.Current.Session != null)
            {
                HttpContext.Current.Session.Abandon();
            }
        }

        private static object TryGet(IDictionary<string, object> dict, string key)
        {
            return dict.ContainsKey(key) ? dict[key] : null;
        }

        //For some reason JWT stores a string array as "ArrayList" so must be parsed
        private static string[] ParseRoles(Object roles)
        {
            var arrayList = roles as ArrayList;
            if (arrayList == null)
                return new string[0];

            return (string[])arrayList.ToArray(typeof (string));
        }

        //CfgDotNet strongly-typed settingss
        internal class Settings : BaseSettings
        {
            public string Key { get; set; }
            public JwtHashAlgorithm Algorithm { get; set; }

            public override void Validate()
            {
                //if (string.IsNullOrWhiteSpace(Key))
                //    Key = GenerateKey();

                if (string.IsNullOrWhiteSpace(Key))
                    throw new ApplicationException("Invalid TokenService+Settings.Key");
            }

            //REF: http://stackoverflow.com/questions/16574655/generate-a-128-bit-string-in-c-sharp
            private static string GenerateKey()
            {
                var bytes = new byte[100];
                var rng = new RNGCryptoServiceProvider();
                rng.GetBytes(bytes);
                var key = Convert.ToBase64String(bytes);
                return key.Substring(0, 64);
            }
        };
    };
}
