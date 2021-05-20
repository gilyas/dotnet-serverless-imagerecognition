using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Amazon;
using Amazon.Runtime;
using Amazon.Runtime.Internal;
using Amazon.Runtime.Internal.Auth;
using Amazon.SecurityToken;
using Amazon.SecurityToken.Model;
using Amazon.Util;
using GraphQL;
using GraphQL.Client.Http;

namespace s3Trigger
{

    public static class GraphQLHttpClientOptionsExtension
    {
        private static Credentials cachedCredentials = null;

        public async static Task<GraphQLHttpClientOptions> ConfigureAppSync(
            this GraphQLHttpClientOptions options,
            string graphQlEndpoint,
            IClientConfig clientConfig
        )
        {
            var appSyncHost = new Uri(graphQlEndpoint).Host.Split('.');
            // set GraphQL endpoint
            options.EndPoint = new Uri(graphQlEndpoint);

            ImmutableCredentials credentials = await FallbackCredentialsFactory.GetCredentials().GetCredentialsAsync();

            //set pre-processor to add authentication HTTP header to request
            options.PreprocessRequest = (request, client) =>
            {
                return Task.FromResult((GraphQLHttpRequest)new AuthorizedAppSyncHttpRequest(request, clientConfig, credentials));
            };

            return options;
        }

    }
}