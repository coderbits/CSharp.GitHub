﻿#region License

/*
 * Copyright 2002-2012 the original author or authors.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

#endregion

using System;
using System.Net;
using Spring.Http;
using Spring.Rest.Client;
using Spring.Rest.Client.Support;

namespace CSharp.GitHub.Api.Impl
{
    /// <summary>
    /// Implementation of the <see cref="IResponseErrorHandler"/> that handles errors from GitHub's REST API, 
    /// interpreting them into appropriate exceptions.
    /// </summary>
    /// <author>Scott Smith</author>
    class GitHubErrorHandler : DefaultResponseErrorHandler
    {
    	/// <summary>
        /// Handles the error in the given response. 
        /// <para/>
        /// This method is only called when HasError() method has returned <see langword="true"/>.
        /// </summary>
        /// <remarks>
        /// This implementation throws appropriate exception if the response status code 
        /// is a client code error (4xx) or a server code error (5xx). 
        /// </remarks>
        /// <param name="requestUri">The request URI.</param>
        /// <param name="requestMethod">The request method.</param>
        /// <param name="response">The response message with the error.</param>
        public override void HandleError(Uri requestUri, HttpMethod requestMethod, HttpResponseMessage<byte[]> response)
        {
            var type = (int)response.StatusCode / 100;
            if (type == 4)
            {
                HandleClientErrors(response);
            }
            else if (type == 5)
            {
                HandleServerErrors(response.StatusCode);
            }

            // if not otherwise handled, do default handling and wrap with GitHubApiException
            try
            {
                base.HandleError(requestUri, requestMethod, response);
            }
            catch (Exception ex)
            {
                throw new GitHubApiException("Error consuming GitHub REST API.", ex);
            }
        }

        private void HandleClientErrors(HttpResponseMessage<byte[]> response)
        {
        	if (response == null) throw new ArgumentNullException("response");

        	if (response.StatusCode == HttpStatusCode.BadRequest)
			{
				throw new GitHubApiException(
					"The server could not understand your request. Verify that request parameters (and content, if any) are valid.",
					GitHubApiError.BadRequest);
			}

        	if (response.StatusCode == HttpStatusCode.Unauthorized)
        	{
        		throw new GitHubApiException(
        			"Authentication failed or was not provided. Verify that you have sent valid credentials.",
        			GitHubApiError.AuthorizationRequired);
        	}
        	
			if (response.StatusCode == HttpStatusCode.Forbidden)
        	{
        		throw new GitHubApiException(
        			"The server understood your request and verified your credentials, but you are not allowed to perform the requested action.",
        			GitHubApiError.Forbidden);
        	}
        	
			if (response.StatusCode == HttpStatusCode.NotFound)
        	{
        		throw new GitHubApiException(
        			"The resource that you requested does not exist.",
        			GitHubApiError.NotFound);
        	}
        	
			if (response.StatusCode == HttpStatusCode.Conflict)
        	{
        		throw new GitHubApiException(
        			"The resource that you are trying to create already exists. This should also provide a Location header to the resource in question.",
        			GitHubApiError.Conflict);
        	}
        }

    	private void HandleServerErrors(HttpStatusCode statusCode)
        {
		    if (statusCode == HttpStatusCode.InternalServerError) 
            {
                throw new GitHubApiException(
					"An unknown error has occurred.", 
                    GitHubApiError.InternalServerError);
		    }
	    }
    }
}