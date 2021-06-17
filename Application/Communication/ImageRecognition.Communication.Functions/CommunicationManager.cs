using System;

using Amazon;
using Amazon.Runtime;

using Amazon.ApiGatewayManagementApi;
using Amazon.ApiGatewayManagementApi.Model;

using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using System.Threading.Tasks;
using System.Collections.Generic;

using Newtonsoft.Json;
using System.IO;
using System.Text;
using Microsoft.Extensions.Caching.Memory;

namespace ImageRecognition.Communication.Functions
{
    public class CommunicationManager
    {
        const string ConnectionIdField = "connectionId";
        const string UsernameField = "username";
        const string EndpointField = "endpoint";
        const string LoginDateField = "logindate";


        string _ddbTableName;
        IAmazonDynamoDB _ddbClient;

        MemoryCache _connectionsCache = new MemoryCache(new MemoryCacheOptions());

        private CommunicationManager(AWSCredentials awsCredentials, RegionEndpoint region, string ddbTableName)
        {
            _ddbTableName = ddbTableName;
            _ddbClient = new AmazonDynamoDBClient(awsCredentials, region);
        }


        public static CommunicationManager CreateManager(AWSCredentials awsCredentials, RegionEndpoint region, string ddbTableName)
        {
            return new CommunicationManager(awsCredentials, region, ddbTableName);
        }

        public static CommunicationManager CreateManager(string ddbTableName)
        {
            return CreateManager(FallbackCredentialsFactory.GetCredentials(), FallbackRegionFactory.GetRegionEndpoint(), ddbTableName);
        }


        public async Task LoginAsync(string connectionId, string endpoint, string username)
        {
            if (string.IsNullOrEmpty(_ddbTableName))
                return;

            var putRequest = new PutItemRequest
            {
                TableName = _ddbTableName,
                Item = new Dictionary<string, AttributeValue>
                    {
                        {ConnectionIdField, new AttributeValue{ S = connectionId}},
                        {EndpointField, new AttributeValue{ S = endpoint}},
                        {UsernameField, new AttributeValue{ S = username}},
                        {LoginDateField, new AttributeValue{S = DateTime.UtcNow.ToString()}}
                    }
            };

            await _ddbClient.PutItemAsync(putRequest);
        }

        public async Task LogoffAsync(string connectionId)
        {
            if (string.IsNullOrEmpty(_ddbTableName))
                return;

            var deleteRequest = new DeleteItemRequest
            {
                TableName = _ddbTableName,
                Key = new Dictionary<string, AttributeValue>
                {
                    {ConnectionIdField, new AttributeValue{ S = connectionId}}
                }
            };

            await _ddbClient.DeleteItemAsync(deleteRequest);
        }
    }
}
