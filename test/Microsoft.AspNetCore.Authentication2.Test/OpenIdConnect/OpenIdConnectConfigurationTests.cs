// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication2.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.Testing.xunit;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.AspNetCore.Authentication2.Test.OpenIdConnect
{
    public class OpenIdConnectConfigurationTests
    {
        [ConditionalFact(Skip = "need way to restore access to options instance")]
        public void MetadataAddressIsGeneratedFromAuthorityWhenMissing()
        {
            //BuildTestServer(o =>
            //{
            //    o.Authority = TestServerBuilder.DefaultAuthority;
            //    o.ClientId = Guid.NewGuid().ToString();
            //    o.SignInScheme = Guid.NewGuid().ToString()
            //});

            //Assert.Equal($"{TestServerBuilder.DefaultAuthority}/.well-known/openid-configuration", options.MetadataAddress);
        }

        [Fact]
        public Task ThrowsWhenSignInSchemeIsMissing()
        {
            return TestConfigurationException<ArgumentException>(
                o =>
                {
                    o.ClientId = "Test Id";
                    o.Authority = TestServerBuilder.DefaultAuthority;
                },
                ex => Assert.Equal("SignInScheme", ex.ParamName));
        }

        [Fact]
        public Task ThrowsWhenClientIdIsMissing()
        {
            return TestConfigurationException<ArgumentException>(
                o =>
                {
                    o.SignInScheme = "TestScheme";
                    o.Authority = TestServerBuilder.DefaultAuthority;
                },
                ex => Assert.Equal("ClientId", ex.ParamName));
        }

        [Fact]
        public Task ThrowsWhenAuthorityIsMissing()
        {
            return TestConfigurationException<InvalidOperationException>(
                o =>
                {
                    o.SignInScheme = "TestScheme";
                    o.ClientId = "Test Id";
                },
                ex => Assert.Equal("Provide Authority, MetadataAddress, Configuration, or ConfigurationManager to OpenIdConnectOptions", ex.Message)
            );
        }

        [Fact]
        public Task ThrowsWhenAuthorityIsNotHttps()
        {
            return TestConfigurationException<InvalidOperationException>(
                o =>
                {
                    o.SignInScheme = "TestScheme";
                    o.ClientId = "Test Id";
                    o.MetadataAddress = "http://example.com";
                },
                ex => Assert.Equal("The MetadataAddress or Authority must use HTTPS unless disabled for development by setting RequireHttpsMetadata=false.", ex.Message)
            );
        }

        [Fact]
        public Task ThrowsWhenMetadataAddressIsNotHttps()
        {
            return TestConfigurationException<InvalidOperationException>(
                o =>
                {
                    o.SignInScheme = "TestScheme";
                    o.ClientId = "Test Id";
                    o.MetadataAddress = "http://example.com";
                },
                ex => Assert.Equal("The MetadataAddress or Authority must use HTTPS unless disabled for development by setting RequireHttpsMetadata=false.", ex.Message)
            );
        }

        private TestServer BuildTestServer(Action<OpenIdConnectOptions> options)
        {
            var builder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddCookieAuthentication();
                    services.AddOpenIdConnectAuthentication(options);
                })
                .Configure(app => app.UseAuthentication());

            return new TestServer(builder);
        }

        private async Task TestConfigurationException<T>(
            Action<OpenIdConnectOptions> options,
            Action<T> verifyException)
            where T : Exception
        {
            var exception = await Assert.ThrowsAsync<T>(() => BuildTestServer(options).SendAsync(@"https://example.com"));
            verifyException(exception);
        }
    }
}
