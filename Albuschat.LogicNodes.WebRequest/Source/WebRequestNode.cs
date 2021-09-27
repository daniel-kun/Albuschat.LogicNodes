using LogicModule.Nodes.Helpers;
using LogicModule.ObjectModel;
using LogicModule.ObjectModel.TypeSystem;
using System.Net;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Text.RegularExpressions;
using System.Threading;

namespace d_albuschat_gmail_com.logic.WebRequest
{
    /// <summary>
    /// A logic node that can make arbitrary web requests.
    /// </summary>
    public class WebRequestNode : LogicNodeBase
    {
        public WebRequestNode(INodeContext context)
        : base(context)
        {
            context.ThrowIfNull("context");
            this.TypeService = context.GetService<ITypeService>();

            this.Trigger = this.TypeService.CreateBool(PortTypes.Binary, "Trigger");
            this.URL = this.TypeService.CreateString(PortTypes.String, "URL");
            this.URL.ValueSet += RefreshVariables;
            this.Method = this.TypeService.CreateEnum("HttpMethodType", "Method", new string[] {
                "GET", "POST", "HEAD", "PUT", "DELETE", "CONNECT", "OPTIONS", "PATCH"
            }, "GET");
            this.AuthType = this.TypeService.CreateEnum(
                "HttpAuthType",
                "AuthType",
                new string[]
                {
                    AuthCredentials.AuthType.NoAuth.ToString(),
                    AuthCredentials.AuthType.BasicAuth.ToString(),
                    AuthCredentials.AuthType.BearerToken.ToString()
                },
                AuthCredentials.AuthType.NoAuth.ToString());
            this.AuthType.ValueSet += AuthType_ValueSet;
            this.Method.ValueSet += UpdateBodyAndContentTypeInputVisibility;
            this.ContentType = null;
            this.Body = null;
            this.HeaderMode = this.TypeService.CreateEnum("HeaderModeType", "HeaderMode", new string[] {
                "HeaderMode.0",
                "HeaderMode.1",
                "HeaderMode.2",
                "HeaderMode.3",
                "HeaderMode.4",
                "HeaderMode.5",
                "HeaderMode.6",
                "HeaderMode.7",
                "HeaderMode.8",
                "HeaderMode.9",
                "HeaderMode.10"
            },
            "HeaderMode.0");
            this.HeaderMode.ValueSet += UpdateHeaders;
            this.Headers = new List<StringValueObject>();
            this.Variables = new List<StringValueObject>();
            this.Response = this.TypeService.CreateString(PortTypes.String, "Response");
            this.ErrorCode = this.TypeService.CreateInt(PortTypes.Integer, "ErrorCode");
            this.ErrorMessage = this.TypeService.CreateString(PortTypes.String, "ErrorMessage");
        }

        private void AuthType_ValueSet(object sender, ValueChangedEventArgs e)
        {
            var newValue = (string)e.NewValue;
            if (newValue != null && newValue == AuthCredentials.AuthType.BasicAuth.ToString())
            {
                this.AuthToken = null;
                this.AuthUserName = this.TypeService.CreateString(PortTypes.String, "AuthUserName");
                this.AuthPassword = this.TypeService.CreateString(PortTypes.String, "AuthPassword");
            }
            else if (newValue != null && newValue == AuthCredentials.AuthType.BearerToken.ToString())
            {
                this.AuthToken = this.TypeService.CreateString(PortTypes.String, "AuthToken");
                this.AuthUserName = null;
                this.AuthPassword = null;
            }
            else
            {
                this.AuthToken = null;
                this.AuthUserName = null;
                this.AuthPassword = null;
            }
        }

        private class MatchEqualityComparer : IEqualityComparer<Match>
        {
            public bool Equals(Match x, Match y)
            {
                return x.Value.Equals(y.Value);
            }

            public int GetHashCode(Match obj)
            {
                return obj.Value.GetHashCode();
            }
        }

        private void RefreshVariables(object sender, ValueChangedEventArgs e)
        {
            this.Variables.Clear();
            var uniqueVariables = new Dictionary<string, object>();

            void RefreshVariablesFrom(StringValueObject str)
            {
                if (str != null && str.HasValue)
                {
                    var matches = Regex.Matches(str.Value, "{[a-zA-Z][a-zA-Z0-9]*}");
                    foreach (Match variable in (new List<Match>(matches.OfType<Match>()).Distinct(new MatchEqualityComparer())))
                    {
                        string variableName = variable.Value.Substring(1, variable.Value.Length - 2); // Strip leading and trailing {}
                        if (!uniqueVariables.ContainsKey(variableName))
                        {
                            uniqueVariables.Add(variableName, null);
                        }
                    }
                }
            }
            RefreshVariablesFrom(this.URL);
            RefreshVariablesFrom(this.Body);
            foreach (var variable in uniqueVariables)
            {
                this.Variables.Add(this.TypeService.CreateString(PortTypes.String, variable.Key, string.Empty));
            }
        }

        private void UpdateBodyAndContentTypeInputVisibility(object sender, ValueChangedEventArgs e)
        {
            StringValueObject createBody()
            {
                if (this.Body != null)
                {
                    return this.Body;
                }
                else
                {
                    StringValueObject result = this.TypeService.CreateString(PortTypes.String, "Body", string.Empty);
                    result.ValueSet += RefreshVariables;
                    return result;
                }
            }
            EnumValueObject createContentType()
            {
                if (this.ContentType != null)
                {
                    return this.ContentType;
                }
                else
                {
                    return this.TypeService.CreateEnum("HttpContentType", "ContentType", new string[] {
                        "ContentType.Empty",
                        "text/plain",
                        "application/json",
                        "application/xml",
                        "application/x-www-form-urlencoded" },
                        "ContentType.Empty");
                }
            }
            var method = this.Method.HasValue ? this.Method.Value : string.Empty;
            this.Body = method != "GET" ? createBody() : null;
            this.ContentType = method != "GET" ? createContentType() : null;
        }

        private void UpdateHeaders(object sender, ValueChangedEventArgs e)
        {
            int TryParseHeaderMode(string headerMode)
            {
                int result = 0;
                if (headerMode.StartsWith("HeaderMode."))
                {
                    int.TryParse(headerMode.Substring("HeaderMode.".Length), out result);
                }
                return result;
            }

            if (e.OldValue is string oldValue && e.NewValue is string newValue)
            {
                int oldInt = TryParseHeaderMode(oldValue), newInt = TryParseHeaderMode(newValue);
                if (oldInt >= 0 && newInt >= 0)
                {
                    if (newInt < oldInt)
                    {
                        this.Headers.RemoveRange(newInt, this.Headers.Count - newInt);
                    }
                    else if (newInt > oldInt)
                    {
                        for (int i = this.Headers.Count; i < newInt; ++i)
                        {
                            this.Headers.Add(this.TypeService.CreateString(PortTypes.String, $"Headers{i + 1}", string.Empty));
                        }
                    }
                }
            }
        }

        private ITypeService TypeService;
        public Action _BeforeExecute;
        public Action _AfterExecute;

        /// <summary>
        /// A trigger used to execute the web request.
        /// </summary>
        [Input]
        public BoolValueObject Trigger { get; private set; }

        /// <summary>
        /// The URL to make a request to.
        /// </summary>
        [Parameter]
        public StringValueObject URL { get; private set; }

        /// <summary>
        /// The Method to use when making the request, i.e. GET, POST, DELETE, etc.
        /// If empty, GET is used.
        /// </summary>
        [Parameter]
        public EnumValueObject Method { get; private set; }

        /// <summary>
        /// Specifies the Authorization type to use, if any.
        /// </summary>
        [Parameter]
        public EnumValueObject AuthType { get; private set; }

        /// <summary>
        /// Specifies the Bearer Token when AuthType = BearerToken.
        /// </summary>
        [Input(IsInput = false, IsDefaultShown = true)]
        public StringValueObject AuthToken { get; private set; }

        /// <summary>
        /// Specifies the User Name when AuthType = BasicAuth.
        /// </summary>
        [Input(IsInput = false, IsDefaultShown = true)]
        public StringValueObject AuthUserName { get; private set; }

        /// <summary>
        /// Specifies the Password when AuthType = BasicAuth.
        /// </summary>
        [Input(IsInput = false, IsDefaultShown = true)]
        public StringValueObject AuthPassword { get; private set; }


        /// <summary>
        /// The content type of the request.
        /// </summary>
        [Parameter]
        public EnumValueObject ContentType { get; set; }

        /// <summary>
        /// The (optional) body of the request.
        /// </summary>
        [Parameter]
        public StringValueObject Body { get; private set; }

        /// <summary>
        /// Contains a list of variables that have been found in the URL or the body
        /// </summary>
        [Input(IsDefaultShown = true, IsInput = false, IsRequired = true)]
        public List<StringValueObject> Variables { get; private set; }

        /// <summary>
        /// Defines how many - if any - custom headers can be set.
        /// </summary>
        [Input(IsInput = false, IsDefaultShown = false)]
        public EnumValueObject HeaderMode { get; private set; }

        /// <summary>
        /// The (optional) additional headers of the request.
        /// </summary>
        [Parameter(IsRequired = true)]
        public List<StringValueObject> Headers { get; private set; }

        /// <summary>
        /// The response of the web request, if it was successful.
        /// </summary>
        [Output(IsRequired = false)]
        public StringValueObject Response { get; private set; }

        [Output(IsRequired = false, IsDefaultShown = false)]
        public IntValueObject ErrorCode { get; private set; }

        [Output(IsRequired = false, IsDefaultShown = false)]
        public StringValueObject ErrorMessage { get; private set; }

        /// <summary>
        /// Executes the web request with the given method and url.
        /// If it is a POST request, the body is sent.
        /// In URL and Body, the placeholders {{ValueN}} are replaced with the ValueN input's values.
        /// </summary>
        public override void Execute()
        {
            ExecuteImpl();
        }

        public void ExecuteImpl()
        {
            if (Trigger.HasValue && Trigger.Value == true && Trigger.WasSet)
            {
                string TranslateContentType(string contentTypeRaw)
                {
                    if (contentTypeRaw == "ContentType.Empty")
                    {
                        return null;
                    }
                    else
                    {
                        return contentTypeRaw;
                    }
                }
                string body = Body != null ? (Body.HasValue ? Body.Value : null) : null;
                string contentType = ContentType != null ? (ContentType.HasValue ? TranslateContentType(ContentType.Value) : null) : null;
                Dictionary<string, string> variables = BuildDictionaryFromVariables(this.Variables);

                var thread = new Thread(() =>
                {
                    _BeforeExecute?.Invoke();
                    ExecuteWebRequest(
                        Method.HasValue ? Method.Value : null,
                        URL.HasValue ? URL.Value : null,
                        AuthCredentials.FromNodeParameters(AuthType, AuthToken, AuthUserName, AuthPassword),
                        Headers.Select(e => e.HasValue ? e.Value : throw new Exception("Did not expect a NULL header")),
                        contentType,
                        body,
                        variables,
                        (errorCode, errorMessage, response) =>
                        {
                            if (errorCode != null)
                            {
                                ErrorCode.Value = errorCode.Value;
                            }
                            if (errorMessage != null)
                            {
                                ErrorMessage.Value = errorMessage;
                            }
                            if (response != null)
                            {
                                Response.Value = response;
                            }
                            _AfterExecute?.Invoke();
                        });
                });
                thread.Start();
            }
        }

        private Dictionary<string, string> BuildDictionaryFromVariables(List<StringValueObject> variables)
        {
            var result = new Dictionary<string, string>();
            foreach (var variable in variables)
            {
                result.Add(variable.Name, variable.Value);
            }
            return result;
        }

        private static String VerboseExceptionMessage(Exception e)
        {
            if (e.InnerException != null)
            {
                return $"{e.Message} ({e.InnerException.Message})";
            }
            else
            {
                return e.Message;
            }
        }

        public static void ExecuteWebRequest(string Method, string URL, AuthCredentials Auth, IEnumerable<string> Headers, string ContentType, string Body, Dictionary<string, string> Variables, Action<int?, string, string> SetResultCallback)
        {
            if (URL != null)
            {
                // These headers are handled specially by HttpWebRequest and
                // can not be set via the generic Headers property.
                var restrictedHeaders = new Dictionary<string, Action<HttpWebRequest, string>>
                {
                    { "Accept", (request, value) => { request.Accept = value; } },
                    { "Connection", (request, value) => { request.Connection = value; } },
                    { "Content-Length", (request, value) => { if (int.TryParse(value, out int length)) { request.ContentLength = length; } } },
                    { "Content-Type", (request, value) => { /* nop, setting Content-Type via custom Header is explicitly disallowed */ } },
                    { "Date", (request, value) => { DateTime date; if (DateTime.TryParse(value, out date)) { request.Date = date; } } },
                    { "Expect", (request, value) => { /* nop */ } },
                    { "Host", (request, value) => { request.Host = value; } },
                    { "If-Modified-Since", (request, value) => { DateTime date; if (DateTime.TryParse(value, out date)) { request.IfModifiedSince = date; } } },
                    { "Range", (request, value) => { /* nop */ } },
                    { "Referer", (request, value) => { request.Referer = value; } },
                    { "Transfer-Encoding", (request, value) => { /* nop */ } },
                    { "User-Agent", (request, value) => { request.UserAgent = value; } },
                    { "Proxy-Connection", (request, value) => { /* nop */ } }
                };

                bool SplitHeader(string header, ref string headerName, ref string headerValue)
                {
                    int sep = header.IndexOf(':');
                    if (sep > 0)
                    {
                        headerName = header.Substring(0, sep);
                        headerValue = header.Substring(sep + 1).Trim();
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }

                bool SetRestrictedHeader(string header, HttpWebRequest request)
                {
                    string headerName = null, headerValue = null;
                    if (SplitHeader(header, ref headerName, ref headerValue))
                    {
                        if (restrictedHeaders.ContainsKey(headerName))
                        {

                            restrictedHeaders[headerName](request, headerValue);
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }

                string method = Method != null ? Method.ToUpper() : "GET";
                try
                {
                    Uri uri = new Uri(ReplaceAllVars(URL, Variables));
                    if (uri.Scheme != "http" && uri.Scheme != "https")
                    {
                        SetResultCallback(997, $"Unsupported URI scheme \"{uri.Scheme}\". Only HTTP and HTTPS is supported.", null);
                        return;
                    }

                    HttpWebRequest client = (HttpWebRequest)HttpWebRequest.Create(uri);
                    Auth.ApplyTo(client);

                    Console.WriteLine($"Making web request {uri}");
                    foreach (var header in Headers)
                    {
                        if (header != null)
                        {
                            Console.WriteLine($"Using header {header}");
                            if (!SetRestrictedHeader(header, client))
                            {
                                client.Headers.Add(header);
                            }
                        }
                    }
                    client.Method = method;
                    if (ContentType != null)
                    {
                        client.ContentType = ContentType;
                    }
                    if (Body != null)
                    {
                        using (var request = client.GetRequestStream())
                        {
                            using (var writer = new StreamWriter(request))
                            {
                                writer.Write(ReplaceAllVars(Body, Variables));
                            }
                        }
                    }
                    var response = client.GetResponse();
                    using (var result = response.GetResponseStream())
                    {
                        using (var reader = new StreamReader(result))
                        {
                            SetResultCallback(null, null, TrimZeroPadding(reader.ReadToEnd()));
                        }
                    }
                }
                catch (WebException e)
                {
                    // An exception is also thrown when a valid response is received, but the
                    // response code indicates failure (such as "not authorized" or similar).
                    WebResponse response = e.Response;
                    if (response is HttpWebResponse errorResponse)
                    {
                        try
                        {
                            using (var result = errorResponse.GetResponseStream())
                            {
                                using (var reader = new StreamReader(result))
                                {
                                    SetResultCallback((int)errorResponse.StatusCode, TrimZeroPadding(reader.ReadToEnd()), null);
                                    return;
                                }
                            }
                        }
                        catch (Exception e2)
                        {
                            SetResultCallback(998, $"Failed to get response stream: {VerboseExceptionMessage(e2)}", null);
                            return;
                        }
                    }
                    else
                    {
                        if (e.Response == null)
                        {
                            SetResultCallback(998, $"Failed to make response: {VerboseExceptionMessage(e)}", null);
                        }
                        else
                        {
                            SetResultCallback(998, $"Unsupported response type {e.Response.GetType().FullName}: {VerboseExceptionMessage(e)}", null);
                        }
                        return;
                    }
                }
                catch (Exception e)
                {
                    SetResultCallback(999, VerboseExceptionMessage(e), null);
                    return;
                }
            }
        }

        private static string TrimZeroPadding(string str)
        {
            int zeroPadding = str.IndexOf('\0');
            if (zeroPadding >= 0)
            {
                return str.Substring(0, zeroPadding);
            }
            else
            {
                return str;
            }
        }

        /// <summary>
        /// Replaces all occurences of {{ValueN}} with the respective values of the properties Value1-ValueN.
        /// </summary>
        /// <param name="value">The string that may contain placeholders</param>
        /// <returns>The string `value`, with all occurences of placeholders replaced with the corresponding values.</returns>
        private static string ReplaceAllVars(string value, Dictionary<string, string> Variables)
        {
            string result = value;
            foreach (var variable in Variables)
            {
                result = result.Replace($"{{{variable.Key}}}", variable.Value);
            }
            return result;
        }

        public override string Localize(string language, string key)
        {
            var translations = new Dictionary<string, Dictionary<string, string>>
            {
                { "de", new Dictionary<string, string>
                    {
                        { "Method", "Methode" },
                        { "AuthType", "Authorization" },
                        { "NoAuth", "(keine)" },
                        { "BasicAuth", "Basic Auth" },
                        { "BearerToken", "Bearer Token" },
                        { "AuthToken", "Token" },
                        { "AuthUserName", "Benutzername" },
                        { "AuthPassword", "Passwort" },
                        { "HeaderMode", "Eigene Header setzen" },
                        { "HeaderMode.0", "Keine" },
                        { "HeaderMode.1", "Anzahl eigener Header: 1" },
                        { "HeaderMode.2", "Anzahl eigener Header: 2" },
                        { "HeaderMode.3", "Anzahl eigener Header: 3" },
                        { "HeaderMode.4", "Anzahl eigener Header: 4" },
                        { "HeaderMode.5", "Anzahl eigener Header: 5" },
                        { "HeaderMode.6", "Anzahl eigener Header: 6" },
                        { "HeaderMode.7", "Anzahl eigener Header: 7" },
                        { "HeaderMode.8", "Anzahl eigener Header: 8" },
                        { "HeaderMode.9", "Anzahl eigener Header: 9" },
                        { "HeaderMode.10", "Anzahl eigener Header: 10" },
                        { "Headers1", "Header #1" },
                        { "Headers2", "Header #2" },
                        { "Headers3", "Header #3" },
                        { "Headers4", "Header #4" },
                        { "Headers5", "Header #5" },
                        { "Headers6", "Header #6" },
                        { "Headers7", "Header #7" },
                        { "Headers8", "Header #8" },
                        { "Headers9", "Header #9" },
                        { "Headers10", "Header #10" },
                        { "Response", "Antwort" },
                        { "ErrorCode", "Fehlercode (HTTP Status)" },
                        { "ErrorMessage", "Fehlermeldung" },
                        { "ContentType", "Content-Type" },
                        { "ContentType.Empty", "Nicht setzen" }
                    }
                },
                { "en", new Dictionary<string, string>
                    {
                        { "Method", "Method" },
                        { "AuthType", "Authorization" },
                        { "NoAuth", "(none)" },
                        { "BasicAuth", "Basic Auth" },
                        { "BearerToken", "Bearer Token" },
                        { "AuthToken", "Token" },
                        { "AuthUserName", "Username" },
                        { "AuthPassword", "Password" },
                        { "HeaderMode", "Custom headers" },
                        { "HeaderMode.0", "No custom headers" },
                        { "HeaderMode.1", "Number of custom headers: 1" },
                        { "HeaderMode.2", "Number of custom headers: 2" },
                        { "HeaderMode.3", "Number of custom headers: 3" },
                        { "HeaderMode.4", "Number of custom headers: 4" },
                        { "HeaderMode.5", "Number of custom headers: 5" },
                        { "HeaderMode.6", "Number of custom headers: 6" },
                        { "HeaderMode.7", "Number of custom headers: 7" },
                        { "HeaderMode.8", "Number of custom headers: 8" },
                        { "HeaderMode.9", "Number of custom headers: 9" },
                        { "HeaderMode.10", "Number of custom headers: 10" },
                        { "Headers1", "Header #1" },
                        { "Headers2", "Header #2" },
                        { "Headers3", "Header #3" },
                        { "Headers4", "Header #4" },
                        { "Headers5", "Header #5" },
                        { "Headers6", "Header #6" },
                        { "Headers7", "Header #7" },
                        { "Headers8", "Header #8" },
                        { "Headers9", "Header #9" },
                        { "Headers10", "Header #10" },
                        { "Response", "Respose" },
                        { "ErrorCode", "Error-code (HTTP Status)" },
                        { "ErrorMessage", "Error message" },
                        { "ContentType", "Content-Type" },
                        { "ContentType.Empty", "Do not set" }
                    }
                }
            };
            if (translations.ContainsKey(language) && translations[language].ContainsKey(key))
            {
                return translations[language][key];
            }
            else if (translations["en"].ContainsKey(key))
            {
                return translations["en"][key];
            }
            else
            {
                return key;
            }
        }

    }
}
