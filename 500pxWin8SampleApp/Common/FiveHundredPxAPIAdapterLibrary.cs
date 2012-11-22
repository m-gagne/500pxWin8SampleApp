using _500pxWin8SampleApp.Common;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;


namespace _500pxWin8SampleApp.Common
{
    /// <summary>
    /// The enumeration of HTTP Methods used by the API
    /// </summary>
    public enum HttpMethods
    {
        GET,
        POST,
        PUT,
        DELETE
    }

    /// <summary>
    /// This class is used to make 500px API calls 
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// <seealso cref="https://github.com/500px/api-documentation"/>

    public sealed class FiveHundredPxAPIClient
    {
        /// <summary>
        /// Creates an instance of this class for use with making API Calls
        /// </summary>
        /// <param name="sdkKey">the sdk key required to make the API Calls</param>
        public FiveHundredPxAPIClient(string sdkKey)
        {
            this.SDKKey = sdkKey;
        }

        /// <summary>
        /// Creates an instance of this class for use with making API Calls
        /// </summary>
        /// <param name="sdkKey">the sdk key required to make the API Calls</param>
        /// <param name="translator">the translator used to transform the data between your C# client code and the Shopify API</param>
        public FiveHundredPxAPIClient(string sdkKey, IDataTranslator translator)
        {
            this.SDKKey = sdkKey;
            this.Translator = translator;
        }

        /// <summary>
        /// Make an HTTP Request to the 500px API
        /// </summary>
        /// <param name="method">method to be used in the request</param>
        /// <param name="path">the path that should be requested</param>
        /// <seealso cref="http://developers.500px.com/"/>
        /// <returns>the server response</returns>
        public IAsyncOperation<object> Call(HttpMethods method, string path)
        {
            return Call(method, path, null);
        }

        /// <summary>
        /// Make an HTTP Request to the 500px API
        /// </summary>
        /// <param name="method">method to be used in the request</param>
        /// <param name="path">the path that should be requested</param>
        /// <param name="callParams">any parameters needed or expected by the API</param>
        /// <seealso cref="http://developers.500px.com/"/>
        /// <returns>the server response</returns>
        public IAsyncOperation<object> Call(HttpMethods method, string path, object callParams)
        {
            return (CallAsync(method, path, callParams)).AsAsyncOperation<object>();
        }

        private async Task<object> CallAsync(HttpMethods method, string path, object callParams)
        {
            string url = String.Format("https://api.500px.com{0}&sdk_key={1}", path, this.SDKKey);
            var req = new HttpClient();

            HttpResponseMessage response = null;
            if (method == HttpMethods.GET || method == HttpMethods.DELETE)
            {
                // if no translator assume data is a query string
                url = String.Format("{0}&{1}", url, callParams != null ? callParams.ToString() : null);

                if (method == HttpMethods.GET)
                {
                    response = await req.GetAsync(url);
                }
                else
                {
                    response = await req.DeleteAsync(url);
                }
            }
            else if (method == HttpMethods.POST || method == HttpMethods.PUT)
            {
                string requestBody;
                // put params into post body
                if (Translator == null)
                {
                    //assume it's a string
                    requestBody = callParams.ToString();
                }
                else
                {
                    requestBody = Translator.Encode(callParams);
                }


                if (method == HttpMethods.POST)
                {
                    response = await req.PostAsync(url, new StringContent(requestBody));
                }
                else
                {
                    response = await req.PutAsync(url, new StringContent(requestBody));
                }
            }

            if (response != null && response.IsSuccessStatusCode)
            {
                //response.Content.Headers.Add("Content-Type", GetRequestContentType());
                var result = await response.Content.ReadAsStringAsync();

                if (Translator != null)
                    return Translator.Decode(result);

                return result;
            }

            return null;
        }

        /// <summary>
        /// Make a Get method HTTP request to the 500px API
        /// </summary>
        /// <param name="path">the path where the API call will be made.</param>
        /// <seealso cref="http://developers.500px.com/"/>
        /// <returns>the server response</returns>
        public IAsyncOperation<object> Get(string path)
        {
            return Get(path, null);
        }

        /// <summary>
        /// Make a Get method HTTP request to the 500px API
        /// </summary>
        /// <param name="path">the path where the API call will be made.</param>
        /// <param name="callParams">the querystring params</param>
        /// <seealso cref="http://developers.500px.com/"/>
        /// <returns>the server response</returns>
        public IAsyncOperation<object> Get(string path, IReadOnlyDictionary<string, string> callParams)
        {
            return Call(HttpMethods.GET, path, callParams);
        }

        /// <summary>
        /// Make a Post method HTTP request to the 500px API
        /// </summary>
        /// <param name="path">the path where the API call will be made.</param>
        /// <param name="data">the data that this path will be expecting</param>
        /// <seealso cref="http://developers.500px.com/"/>
        /// <returns>the server response</returns>
        public IAsyncOperation<object> Post(string path, object data)
        {
            return Call(HttpMethods.POST, path, data);
        }

        /// <summary>
        /// Make a Put method HTTP request to the 500px API
        /// </summary>
        /// <param name="path">the path where the API call will be made.</param>
        /// <param name="data">the data that this path will be expecting</param>
        /// <seealso cref="http://developers.500px.com/"/>
        /// <returns>the server response</returns>
        public IAsyncOperation<object> Put(string path, object data)
        {
            return Call(HttpMethods.PUT, path, data);
        }

        /// <summary>
        /// Make a Delete method HTTP request to the 500px API
        /// </summary>
        /// <param name="path">the path where the API call will be made.</param>
        /// <seealso cref="http://developers.500px.com/"/>
        /// <returns>the server response</returns>
        public IAsyncOperation<object> Delete(string path)
        {
            return Call(HttpMethods.DELETE, path);
        }

        /// <summary>
        /// Get the content type that should be used for HTTP Requests
        /// </summary>
        private string GetRequestContentType()
        {
            if (Translator == null)
                return DefaultContentType;
            return Translator.GetContentType();
        }

        /// <summary>
        /// The default content type used on the HTTP Requests to the 500px API
        /// </summary>
        protected static readonly string DefaultContentType = "application/json";

        /// <summary>
        /// The JavaScript SDK Key
        /// </summary>
        protected string SDKKey { get; set; }

        /// <summary>
        /// Used to translate the data sent and recieved by the 500px API
        /// </summary>
        /// <example>
        /// This could be used to translate from C# objects to XML or JSON.  Thus making your code
        /// that consumes this class much more clean
        /// </example>
        protected IDataTranslator Translator { get; set; }
    }


}
