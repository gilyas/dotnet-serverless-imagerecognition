using Amazon.AppSync;
using Amazon.Lambda.Core;
using Amazon.Lambda.S3Events;
using Amazon.Lambda.Serialization.SystemTextJson;
using GraphQL;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.SystemTextJson;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(DefaultLambdaJsonSerializer))]

namespace s3Trigger
{
    public class Function
    {
        IAmazonAppSync appSyncClient { get; set; }
        
        private string GraphQLEndpoint { get; set; }

        private string StateMachineArn { get; set; }

        private GraphQL.Client.Http.GraphQLHttpClient _graphQlClient;

        public Function()
        {
            this.appSyncClient = new AmazonAppSyncClient();

            //TODO: Change this to environment Variables.
            StateMachineArn = "arn:aws:states:us-east-1:882525684088:stateMachine:PhotoProcessingWorkflow-dotnet";
            GraphQLEndpoint = "https://v35urcizvvaapoe6uumkqjz5em.appsync-api.us-east-1.amazonaws.com/graphql";
        }

        /// <summary>
        /// A simple function that takes a string and returns both the upper and lower case version of the string.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task FunctionHandler(S3Event evnt, ILambdaContext context)
        {
            string bucket = evnt.Records[0].S3.Bucket.Name;
            string key = evnt.Records[0].S3.Object.Key;

            Console.WriteLine(bucket);
            Console.WriteLine(key);

            string objectId = key.Substring(key.LastIndexOf("/") + 1);

            Console.WriteLine(objectId);

            var options = await new GraphQLHttpClientOptions()
                .ConfigureAppSync(
                        GraphQLEndpoint,
                        appSyncClient.Config
                    );

            _graphQlClient = new GraphQLHttpClient(options, new SystemTextJsonSerializer());

            var sfnInput = new SfnExecutionInput() {
                objectId = objectId,
                Bucket = bucket,
                SourceKey = key
            };

            var startWorkflowMutation = new GraphQLRequest
            {
                Query = @"
                mutation StartSfnExecution(
                    $input: StartSfnExecutionInput!
                ) {
                    startSfnExecution(input: $input) {
                        executionArn
                        startDate
                    }
                }",
                OperationName = "StartSfnExecution",
                Variables = new
                {
                    input = new
                    {
                        input = JsonSerializer.Serialize(sfnInput),
                        stateMachineArn = StateMachineArn
                    },
                }
            };

            var workflowMutationResponse = await _graphQlClient.SendMutationAsync<StartWorkflowResult>(startWorkflowMutation);

            Console.WriteLine(JsonSerializer.Serialize(workflowMutationResponse.Data.startSfnExecution));

            var updatePhotoRequest = new GraphQLRequest
            {
                Query = @"
                mutation UpdatePhotoMutation(
                    $input: UpdatePhotoInput!
                    $condition: ModelPhotoConditionInput
                ) {
                    updatePhoto(input: $input, condition: $condition) {
                        id
                        albumId
                        owner
                        uploadTime
                        bucket
                        sfnExecutionArn
                        processingStatus
                    }
                }",
                OperationName = "UpdatePhotoMutation",
                Variables = new
                {
                    input = new
                    {
                        id = objectId,
                        sfnExecutionArn = workflowMutationResponse.Data.startSfnExecution.executionArn,
                        processingStatus = "RUNNING"
                    }
                }
            };

            var photoUpdateMutationResponse = await _graphQlClient.SendMutationAsync<UpdatePhotoResponse>(updatePhotoRequest);

            Console.WriteLine(JsonSerializer.Serialize(photoUpdateMutationResponse.Data));

        }

        public class SfnExecutionInput { 
            public string Bucket { get; set; }
            public string SourceKey { get; set; }
            public string objectId { get; set; }
        }

        public class StartWorkflowResult
        {
            public StartSfnExecution startSfnExecution { get; set; }
        }

        public class StartSfnExecution
        {
            public string executionArn { get; set; }
        }

        public class UpdatePhotoResponse
        {
            public Photo Result { get; set; }
        }

        public class Photo
        {
            public string id { get; set; }
        }
    }
}
