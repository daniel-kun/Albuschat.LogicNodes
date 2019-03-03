using System;
using System.Net;
using System.Text;
using LogicModule.ObjectModel.TypeSystem;

namespace Albuschat.LogicNodes.WebRequest
{
    public class AuthCredentials
    {
        public static AuthCredentials FromNodeParameters(
            EnumValueObject authType,
            StringValueObject token,
            StringValueObject userName,
            StringValueObject password)
        {
            if (authType.HasValue && authType.Value == AuthType.BasicAuth.ToString())
            {
                return new AuthCredentials
                {
                    Type = AuthType.BasicAuth,
                    Username = userName.HasValue ? userName.Value : string.Empty,
                    Password = password.HasValue ? password.Value : string.Empty
                };
            } else if (authType.HasValue && authType.Value == AuthType.BearerToken.ToString())
            {
                return new AuthCredentials
                {
                    Type = AuthType.BearerToken,
                    Token = token.HasValue ? token.Value : string.Empty
                };
            }
            else
            {
                return new AuthCredentials
                {
                    Type = AuthType.NoAuth
                };
            }
        }

        public enum AuthType
        {
            NoAuth,
            BasicAuth,
            BearerToken
        };

        public AuthType Type;

        // Only used when AuthType = BasicAuth:
        public string Username;
        public string Password;

        // Only used when AuthType = BearerToken:
        public string Token;

        public void ApplyTo(HttpWebRequest client)
        {
            switch (Type)
            {
                case AuthType.BearerToken:
                    client.Headers.Add("Authorization", $"Bearer {Token}");
                    break;
                case AuthType.BasicAuth:
                    client.Headers.Add("Authorization", $"Basic {Convert.ToBase64String(Encoding.UTF8.GetBytes($"{Username}:{Password}"))}");
                    break;
            }
        }
    }
}
