//-----------------------------------------------------------------------------
// <auto-generated>
//     This file was generated by the C# SDK Code Generator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//-----------------------------------------------------------------------------


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Scripting;
using Unity.Services.Relay.Models;
using Unity.Services.Relay.Scheduler;
using Unity.Services.Authentication.Internal;

namespace Unity.Services.Relay.RelayAllocations
{
    internal static class JsonSerialization
    {
        public static byte[] Serialize<T>(T obj)
        {
            return Encoding.UTF8.GetBytes(SerializeToString(obj));
        }

        public static string SerializeToString<T>(T obj)
        {
            return JsonConvert.SerializeObject(obj, new JsonSerializerSettings{ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore});
        }
    }

    /// <summary>
    /// RelayAllocationsApiBaseRequest class
    /// </summary>
    [Preserve]
    internal class RelayAllocationsApiBaseRequest
    {
        /// <summary>
        /// Helper function to add a provided key and value to the provided
        /// query params and to escape the values correctly if it is a URL.
        /// </summary>
        /// <param name="queryParams">A `List/<string/>` of the query parameters.</param>
        /// <param name="key">The key to be added.</param>
        /// <param name="value">The value to be added.</param>
        /// <returns>Returns a `List/<string/>` with the `key` and `value` added to the provided `queryParams`.</returns>
        [Preserve]
        public List<string> AddParamsToQueryParams(List<string> queryParams, string key, string value)
        {
            key = UnityWebRequest.EscapeURL(key);
            value = UnityWebRequest.EscapeURL(value);
            queryParams.Add($"{key}={value}");

            return queryParams;
        }

        /// <summary>
        /// Helper function to add a provided key and list of values to the
        /// provided query params and to escape the values correctly if it is a
        /// URL.
        /// </summary>
        /// <param name="queryParams">A `List/<string/>` of the query parameters.</param>
        /// <param name="key">The key to be added.</param>
        /// <param name="values">List of values to be added.</param>
        /// <param name="style">string for defining the style, currently unused.</param>
        /// <param name="explode">True if query params should be escaped and added separately.</param>
        /// <returns>Returns a `List/<string/>`</returns>
        [Preserve]
        public List<string> AddParamsToQueryParams(List<string> queryParams, string key, List<string> values, string style, bool explode)
        {
            if (explode)
            {
                foreach(var value in values)
                {
                    string escapedValue = UnityWebRequest.EscapeURL(value);
                    queryParams.Add($"{UnityWebRequest.EscapeURL(key)}={escapedValue}");
                }
            }
            else
            {
                string paramString = $"{UnityWebRequest.EscapeURL(key)}=";
                foreach(var value in values)
                {
                    paramString += UnityWebRequest.EscapeURL(value) + ",";
                }
                paramString = paramString.Remove(paramString.Length - 1);
                queryParams.Add(paramString);
            }

            return queryParams;
        }

        /// <summary>
        /// Helper function to add a provided map of keys and values, representing a model, to the
        /// provided query params.
        /// </summary>
        /// <param name="queryParams">A `List/<string/>` of the query parameters.</param>
        /// <param name="modelVars">A `Dictionary` representing the vars of the model</param>
        /// <returns>Returns a `List/<string/>`</returns>
        [Preserve]
        public List<string> AddParamsToQueryParams(List<string> queryParams, Dictionary<string, string> modelVars)
        {
            foreach(var key in modelVars.Keys)
            {
                string escapedValue = UnityWebRequest.EscapeURL(modelVars[key]);
                queryParams.Add($"{UnityWebRequest.EscapeURL(key)}={escapedValue}");
            }

            return queryParams;
        }

        /// <summary>
        /// Helper function to add a provided key and value to the provided
        /// query params and to escape the values correctly if it is a URL.
        /// </summary>
        /// <param name="queryParams">A `List/<string/>` of the query parameters.</param>
        /// <param name="key">The key to be added.</param>
        /// <typeparam name="T">The type of the value to be added.</typeparam>
        /// <param name="value">The value to be added.</param>
        /// <returns>Returns a `List/<string/>`</returns>
        [Preserve]
        public List<string> AddParamsToQueryParams<T>(List<string> queryParams, string key, T value)
        {
            if (queryParams == null)
            {
                queryParams = new List<string>();
            }

            key = UnityWebRequest.EscapeURL(key);
            string valueString = UnityWebRequest.EscapeURL(value.ToString());
            queryParams.Add($"{key}={valueString}");
            return queryParams;
        }

        /// <summary>
        /// Constructs a string representing an array path parameter.
        /// </summary>
        /// <param name="pathParam">The list of values to convert to string.</param>
        /// <returns>String representing the param.</returns>
        [Preserve]
        public string GetPathParamString(List<string> pathParam)
        {
            string paramString = "";
            foreach(var value in pathParam)
            {
                paramString += UnityWebRequest.EscapeURL(value) + ",";
            }
            paramString = paramString.Remove(paramString.Length - 1);
            return paramString;
        }

        /// <summary>
        /// Constructs the body of the request based on IO stream.
        /// </summary>
        /// <param name="stream">The IO stream to use.</param>
        /// <returns>Byte array representing the body.</returns>
        public byte[] ConstructBody(System.IO.Stream stream)
        {
            if (stream != null)
            {
                using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
                {
                    stream.CopyTo(ms);
                    return ms.ToArray();
                }
            }
            return null;
        }

        /// <summary>
        /// Construct the request body based on string value.
        /// </summary>
        /// <param name="s">The input body.</param>
        /// <returns>Byte array representing the body.</returns>
        public byte[] ConstructBody(string s)
        {
            return System.Text.Encoding.UTF8.GetBytes(s);
        }

        /// <summary>
        /// Construct request body based on generic object.
        /// </summary>
        /// <param name="o">The object to use.</param>
        /// <returns>Byte array representing the body.</returns>
        public byte[] ConstructBody(object o)
        {
            return JsonSerialization.Serialize(o);
        }

        /// <summary>
        /// Generate an accept header.
        /// </summary>
        /// <param name="accepts">list of accepts objects.</param>
        /// <returns>The generated accept header.</returns>
        public string GenerateAcceptHeader(string[] accepts)
        {
            if (accepts.Length == 0)
            {
                return null;
            }
            for (int i = 0; i < accepts.Length; ++i)
            {
                if (string.Equals(accepts[i], "application/json", System.StringComparison.OrdinalIgnoreCase))
                {
                    return "application/json";
                }
            }
            return string.Join(", ", accepts);
        }

        private static readonly Regex JsonRegex = new Regex(@"application\/json(;\s)?((charset=utf8|q=[0-1]\.\d)(\s)?)*");

        /// <summary>
        /// Generate Content Type Header.
        /// </summary>
        /// <param name="contentTypes">The content types.</param>
        /// <returns>The Content Type Header.</returns>
        public string GenerateContentTypeHeader(string[] contentTypes)
        {
            if (contentTypes.Length == 0)
            {
                return null;
            }

            for(int i = 0; i < contentTypes.Length; ++i)
            {
                if (!string.IsNullOrWhiteSpace(contentTypes[i]) && JsonRegex.IsMatch(contentTypes[i]))
                {
                    return contentTypes[i];
                }
            }
            return contentTypes[0];
        }

        /// <summary>
        /// Generate multipart form file section.
        /// </summary>
        /// <param name="paramName">The parameter name.</param>
        /// <param name="stream">The file stream to use.</param>
        /// <param name="contentType">The content type.</param>
        /// <returns>Returns a multipart form section.</returns>
        public IMultipartFormSection GenerateMultipartFormFileSection(string paramName, System.IO.FileStream stream, string contentType)
        {
            return new MultipartFormFileSection(paramName, ConstructBody(stream), GetFileName(stream.Name), contentType);
        }

        /// <summary>
        /// Generate multipart form file section.
        /// </summary>
        /// <param name="paramName">The parameter name.</param>
        /// <param name="stream">The IO stream to use.</param>
        /// <param name="contentType">The content type.</param>
        /// <returns>Returns a multipart form section.</returns>
        public IMultipartFormSection GenerateMultipartFormFileSection(string paramName, System.IO.Stream stream, string contentType)
        {
            return new MultipartFormFileSection(paramName, ConstructBody(stream), Guid.NewGuid().ToString(), contentType);
        }

        private string GetFileName(string filePath)
        {
            return System.IO.Path.GetFileName(filePath);
        }
    }

    /// <summary>
    /// CreateAllocationRequest
    /// Create an allocation.
    /// </summary>
    [Preserve]
    internal class CreateAllocationRequest : RelayAllocationsApiBaseRequest
    {
        /// <summary>Accessor for allocationRequest </summary>
        [Preserve]
        public Unity.Services.Relay.Models.AllocationRequest AllocationRequest { get; }
        string PathAndQueryParams;

        /// <summary>
        /// CreateAllocation Request Object.
        /// Create an allocation.
        /// </summary>
        /// <param name="allocationRequest">AllocationRequest param</param>
        [Preserve]
        public CreateAllocationRequest(Unity.Services.Relay.Models.AllocationRequest allocationRequest)
        {
            AllocationRequest = allocationRequest;
            PathAndQueryParams = $"/v1/allocate";


        }

        /// <summary>
        /// Helper function for constructing URL from request base path and
        /// query params.
        /// </summary>
        /// <param name="requestBasePath"></param>
        /// <returns></returns>
        public string ConstructUrl(string requestBasePath)
        {
            return requestBasePath + PathAndQueryParams;
        }

        /// <summary>
        /// Helper for constructing the request body.
        /// </summary>
        /// <returns>A list of IMultipartFormSection representing the request body.</returns>
        public byte[] ConstructBody()
        {
            return ConstructBody(AllocationRequest);
        }

        /// <summary>
        /// Helper function for constructing the headers.
        /// </summary>
        /// <param name="accessToken">The auth access token to use.</param>
        /// <param name="operationConfiguration">The operation configuration to use.</param>
        /// <returns>A dictionary representing the request headers.</returns>
        public Dictionary<string, string> ConstructHeaders(IAccessToken accessToken,
            Configuration operationConfiguration = null)
        {
            var headers = new Dictionary<string, string>();
            if(!string.IsNullOrEmpty(accessToken.AccessToken))
            {
                headers.Add("authorization", "Bearer " + accessToken.AccessToken);
            }

            // Analytics headers
            headers.Add("Unity-Client-Version", Application.unityVersion);
            headers.Add("Unity-Client-Mode", Scheduler.EngineStateHelper.IsPlaying ? "play" : "edit");

            string[] contentTypes = {
                "application/json"
            };

            string[] accepts = {
                "application/json",
                "application/problem+json"
            };

            var acceptHeader = GenerateAcceptHeader(accepts);
            if (!string.IsNullOrEmpty(acceptHeader))
            {
                headers.Add("Accept", acceptHeader);
            }
            var httpMethod = "POST";
            var contentTypeHeader = GenerateContentTypeHeader(contentTypes);
            if (!string.IsNullOrEmpty(contentTypeHeader))
            {
                headers.Add("Content-Type", contentTypeHeader);
            }
            else if (httpMethod == "POST" || httpMethod == "PATCH")
            {
                headers.Add("Content-Type", "application/json");
            }


            // We also check if there are headers that are defined as part of
            // the request configuration.
            if (operationConfiguration != null && operationConfiguration.Headers != null)
            {
                foreach (var pair in operationConfiguration.Headers)
                {
                    headers[pair.Key] = pair.Value;
                }
            }

            return headers;
        }
    }
    /// <summary>
    /// CreateJoincodeRequest
    /// Create a join code.
    /// </summary>
    [Preserve]
    internal class CreateJoincodeRequest : RelayAllocationsApiBaseRequest
    {
        /// <summary>Accessor for joinCodeRequest </summary>
        [Preserve]
        public Unity.Services.Relay.Models.JoinCodeRequest JoinCodeRequest { get; }
        string PathAndQueryParams;

        /// <summary>
        /// CreateJoincode Request Object.
        /// Create a join code.
        /// </summary>
        /// <param name="joinCodeRequest">JoinCodeRequest param</param>
        [Preserve]
        public CreateJoincodeRequest(Unity.Services.Relay.Models.JoinCodeRequest joinCodeRequest)
        {
            JoinCodeRequest = joinCodeRequest;
            PathAndQueryParams = $"/v1/joincode";


        }

        /// <summary>
        /// Helper function for constructing URL from request base path and
        /// query params.
        /// </summary>
        /// <param name="requestBasePath"></param>
        /// <returns></returns>
        public string ConstructUrl(string requestBasePath)
        {
            return requestBasePath + PathAndQueryParams;
        }

        /// <summary>
        /// Helper for constructing the request body.
        /// </summary>
        /// <returns>A list of IMultipartFormSection representing the request body.</returns>
        public byte[] ConstructBody()
        {
            return ConstructBody(JoinCodeRequest);
        }

        /// <summary>
        /// Helper function for constructing the headers.
        /// </summary>
        /// <param name="accessToken">The auth access token to use.</param>
        /// <param name="operationConfiguration">The operation configuration to use.</param>
        /// <returns>A dictionary representing the request headers.</returns>
        public Dictionary<string, string> ConstructHeaders(IAccessToken accessToken,
            Configuration operationConfiguration = null)
        {
            var headers = new Dictionary<string, string>();
            if(!string.IsNullOrEmpty(accessToken.AccessToken))
            {
                headers.Add("authorization", "Bearer " + accessToken.AccessToken);
            }

            // Analytics headers
            headers.Add("Unity-Client-Version", Application.unityVersion);
            headers.Add("Unity-Client-Mode", Scheduler.EngineStateHelper.IsPlaying ? "play" : "edit");

            string[] contentTypes = {
                "application/json"
            };

            string[] accepts = {
                "application/json",
                "application/problem+json"
            };

            var acceptHeader = GenerateAcceptHeader(accepts);
            if (!string.IsNullOrEmpty(acceptHeader))
            {
                headers.Add("Accept", acceptHeader);
            }
            var httpMethod = "POST";
            var contentTypeHeader = GenerateContentTypeHeader(contentTypes);
            if (!string.IsNullOrEmpty(contentTypeHeader))
            {
                headers.Add("Content-Type", contentTypeHeader);
            }
            else if (httpMethod == "POST" || httpMethod == "PATCH")
            {
                headers.Add("Content-Type", "application/json");
            }


            // We also check if there are headers that are defined as part of
            // the request configuration.
            if (operationConfiguration != null && operationConfiguration.Headers != null)
            {
                foreach (var pair in operationConfiguration.Headers)
                {
                    headers[pair.Key] = pair.Value;
                }
            }

            return headers;
        }
    }
    /// <summary>
    /// JoinRelayRequest
    /// Join a Relay server.
    /// </summary>
    [Preserve]
    internal class JoinRelayRequest : RelayAllocationsApiBaseRequest
    {
        /// <summary>Accessor for joinRequest </summary>
        [Preserve]
        public Unity.Services.Relay.Models.JoinRequest JoinRequest { get; }
        string PathAndQueryParams;

        /// <summary>
        /// JoinRelay Request Object.
        /// Join a Relay server.
        /// </summary>
        /// <param name="joinRequest">JoinRequest param</param>
        [Preserve]
        public JoinRelayRequest(Unity.Services.Relay.Models.JoinRequest joinRequest)
        {
            JoinRequest = joinRequest;
            PathAndQueryParams = $"/v1/join";


        }

        /// <summary>
        /// Helper function for constructing URL from request base path and
        /// query params.
        /// </summary>
        /// <param name="requestBasePath"></param>
        /// <returns></returns>
        public string ConstructUrl(string requestBasePath)
        {
            return requestBasePath + PathAndQueryParams;
        }

        /// <summary>
        /// Helper for constructing the request body.
        /// </summary>
        /// <returns>A list of IMultipartFormSection representing the request body.</returns>
        public byte[] ConstructBody()
        {
            return ConstructBody(JoinRequest);
        }

        /// <summary>
        /// Helper function for constructing the headers.
        /// </summary>
        /// <param name="accessToken">The auth access token to use.</param>
        /// <param name="operationConfiguration">The operation configuration to use.</param>
        /// <returns>A dictionary representing the request headers.</returns>
        public Dictionary<string, string> ConstructHeaders(IAccessToken accessToken,
            Configuration operationConfiguration = null)
        {
            var headers = new Dictionary<string, string>();
            if(!string.IsNullOrEmpty(accessToken.AccessToken))
            {
                headers.Add("authorization", "Bearer " + accessToken.AccessToken);
            }

            // Analytics headers
            headers.Add("Unity-Client-Version", Application.unityVersion);
            headers.Add("Unity-Client-Mode", Scheduler.EngineStateHelper.IsPlaying ? "play" : "edit");

            string[] contentTypes = {
                "application/json"
            };

            string[] accepts = {
                "application/json",
                "application/problem+json"
            };

            var acceptHeader = GenerateAcceptHeader(accepts);
            if (!string.IsNullOrEmpty(acceptHeader))
            {
                headers.Add("Accept", acceptHeader);
            }
            var httpMethod = "POST";
            var contentTypeHeader = GenerateContentTypeHeader(contentTypes);
            if (!string.IsNullOrEmpty(contentTypeHeader))
            {
                headers.Add("Content-Type", contentTypeHeader);
            }
            else if (httpMethod == "POST" || httpMethod == "PATCH")
            {
                headers.Add("Content-Type", "application/json");
            }


            // We also check if there are headers that are defined as part of
            // the request configuration.
            if (operationConfiguration != null && operationConfiguration.Headers != null)
            {
                foreach (var pair in operationConfiguration.Headers)
                {
                    headers[pair.Key] = pair.Value;
                }
            }

            return headers;
        }
    }
    /// <summary>
    /// ListRegionsRequest
    /// List available Relay regions.
    /// </summary>
    [Preserve]
    internal class ListRegionsRequest : RelayAllocationsApiBaseRequest
    {
        string PathAndQueryParams;

        /// <summary>
        /// ListRegions Request Object.
        /// List available Relay regions.
        /// </summary>
        [Preserve]
        public ListRegionsRequest()
        {
            PathAndQueryParams = $"/v1/regions";


        }

        /// <summary>
        /// Helper function for constructing URL from request base path and
        /// query params.
        /// </summary>
        /// <param name="requestBasePath"></param>
        /// <returns></returns>
        public string ConstructUrl(string requestBasePath)
        {
            return requestBasePath + PathAndQueryParams;
        }

        /// <summary>
        /// Helper for constructing the request body.
        /// </summary>
        /// <returns>A list of IMultipartFormSection representing the request body.</returns>
        public byte[] ConstructBody()
        {
            return null;
        }

        /// <summary>
        /// Helper function for constructing the headers.
        /// </summary>
        /// <param name="accessToken">The auth access token to use.</param>
        /// <param name="operationConfiguration">The operation configuration to use.</param>
        /// <returns>A dictionary representing the request headers.</returns>
        public Dictionary<string, string> ConstructHeaders(IAccessToken accessToken,
            Configuration operationConfiguration = null)
        {
            var headers = new Dictionary<string, string>();
            if(!string.IsNullOrEmpty(accessToken.AccessToken))
            {
                headers.Add("authorization", "Bearer " + accessToken.AccessToken);
            }

            // Analytics headers
            headers.Add("Unity-Client-Version", Application.unityVersion);
            headers.Add("Unity-Client-Mode", Scheduler.EngineStateHelper.IsPlaying ? "play" : "edit");

            string[] contentTypes = {
            };

            string[] accepts = {
                "application/json",
                "application/problem+json"
            };

            var acceptHeader = GenerateAcceptHeader(accepts);
            if (!string.IsNullOrEmpty(acceptHeader))
            {
                headers.Add("Accept", acceptHeader);
            }
            var httpMethod = "GET";
            var contentTypeHeader = GenerateContentTypeHeader(contentTypes);
            if (!string.IsNullOrEmpty(contentTypeHeader))
            {
                headers.Add("Content-Type", contentTypeHeader);
            }
            else if (httpMethod == "POST" || httpMethod == "PATCH")
            {
                headers.Add("Content-Type", "application/json");
            }


            // We also check if there are headers that are defined as part of
            // the request configuration.
            if (operationConfiguration != null && operationConfiguration.Headers != null)
            {
                foreach (var pair in operationConfiguration.Headers)
                {
                    headers[pair.Key] = pair.Value;
                }
            }

            return headers;
        }
    }
}
