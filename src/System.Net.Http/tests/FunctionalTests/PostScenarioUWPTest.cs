// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace System.Net.Http.Functional.Tests
{
    using Configuration = System.Net.Test.Common.Configuration;

    public class PostScenarioUWPTest : RemoteExecutorTestBase
    {
        private readonly ITestOutputHelper _output;

        public PostScenarioUWPTest(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void Authentication_UseStreamContent_Throws()
        {
            RemoteInvoke(async () =>
            {
                // This test validates the current limitation of CoreFx's NetFxToWinRtStreamAdapter
                // which throws exceptions when trying to rewind a .NET Stream when it needs to be
                // re-POST'd to the server.
                string username = "testuser";
                string password = "password";
                Uri uri = Configuration.Http.BasicAuthUriForCreds(secure: false, userName: username, password: password);            
                var handler = new HttpClientHandler();
                handler.Credentials = new NetworkCredential(username, password);

                using (var client = new HttpClient(handler))
                {
                    byte[] postData = Encoding.UTF8.GetBytes("This is data to post.");
                    var stream = new MemoryStream(postData, false);
                    var content = new StreamContent(stream);

                    await Assert.ThrowsAsync<HttpRequestException>(() => client.PostAsync(uri, content));
                }

                return SuccessExitCode;
            }).Dispose();
        }

        [Fact]
        public void Authentication_UseMultiInterfaceNonRewindableStreamContent_Throws()
        {
            RemoteInvoke(async () =>
            {
                string username = "testuser";
                string password = "password";
                Uri uri = Configuration.Http.BasicAuthUriForCreds(secure: false, userName: username, password: password);            
                var handler = new HttpClientHandler();
                handler.Credentials = new NetworkCredential(username, password);

                using (var client = new HttpClient(handler))
                {
                    byte[] postData = Encoding.UTF8.GetBytes("This is data to post.");
                    var stream = new MultiInterfaceNonRewindableReadOnlyStream(postData);
                    var content = new MultiInterfaceStreamContent(stream);

                    await Assert.ThrowsAsync<HttpRequestException>(() => client.PostAsync(uri, content));
                }

                return SuccessExitCode;
            }).Dispose();
        }

        [Fact]
        public void Authentication_UseMultiInterfaceStreamContent_Success()
        {
            RemoteInvoke(async () =>
            {
                string username = "testuser";
                string password = "password";
                Uri uri = Configuration.Http.BasicAuthUriForCreds(secure: false, userName: username, password: password);            
                var handler = new HttpClientHandler();
                handler.Credentials = new NetworkCredential(username, password);

                using (var client = new HttpClient(handler))
                {
                    byte[] postData = Encoding.UTF8.GetBytes("This is data to post.");
                    var stream = new MultiInterfaceReadOnlyStream(postData);
                    var content = new MultiInterfaceStreamContent(stream);

                    using (HttpResponseMessage response = await client.PostAsync(uri, content))
                    {
                        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                        string responseContent = await response.Content.ReadAsStringAsync();
                    }
                }

                return SuccessExitCode;
            }).Dispose();
        }

        [Fact]
        public void Authentication_UseMemoryStreamVisibleBufferContent_Success()
        {
            RemoteInvoke(async () =>
            {
                string username = "testuser";
                string password = "password";
                Uri uri = Configuration.Http.BasicAuthUriForCreds(secure: false, userName: username, password: password);            
                var handler = new HttpClientHandler();
                handler.Credentials = new NetworkCredential(username, password);

                using (var client = new HttpClient(handler))
                {
                    byte[] postData = Encoding.UTF8.GetBytes("This is data to post.");
                    var stream = new MemoryStream(postData, 0, postData.Length, false, true);
                    var content = new MultiInterfaceStreamContent(stream);

                    using (HttpResponseMessage response = await client.PostAsync(uri, content))
                    {
                        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                        string responseContent = await response.Content.ReadAsStringAsync();
                    }
                }

                return SuccessExitCode;
            }).Dispose();
        }

        [Fact]
        public void Authentication_UseMemoryStreamNotVisibleBufferContent_Success()
        {
            RemoteInvoke(async () =>
            {
                string username = "testuser";
                string password = "password";
                Uri uri = Configuration.Http.BasicAuthUriForCreds(secure: false, userName: username, password: password);            
                var handler = new HttpClientHandler();
                handler.Credentials = new NetworkCredential(username, password);

                using (var client = new HttpClient(handler))
                {
                    byte[] postData = Encoding.UTF8.GetBytes("This is data to post.");
                    var stream = new MemoryStream(postData, 0, postData.Length, false, false);
                    var content = new MultiInterfaceStreamContent(stream);

                    using (HttpResponseMessage response = await client.PostAsync(uri, content))
                    {
                        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                        string responseContent = await response.Content.ReadAsStringAsync();
                    }
                }

                return SuccessExitCode;
            }).Dispose();
        }
    }
}
