using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Amazon.AppSync;
using Amazon.Lambda.Core;
using GraphQL;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.SystemTextJson;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace store_image_metadata
{
    public class Function
    {

        public const string GRAPHQL_ENDPOINT = "GraphQLEndPoint";
        private IAmazonAppSync appSyncClient { get; set; }
        private GraphQL.Client.Http.GraphQLHttpClient _graphQlClient;
        
        public Function(){
            this.appSyncClient = new AmazonAppSyncClient();

        }

        /// <summary>
        /// A simple function that takes a string and returns both the upper and lower case version of the string.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task FunctionHandler(InputEvent input, ILambdaContext context)
        {
            string graphQlEndpoint = System.Environment.GetEnvironmentVariable(GRAPHQL_ENDPOINT);

            var options = await new GraphQLHttpClientOptions()
                .ConfigureAppSync(
                        graphQlEndpoint,
                        appSyncClient.Config
                    );

            _graphQlClient = new GraphQLHttpClient(options, new SystemTextJsonSerializer());


            const string UPDATE_PHOTO = @"
                mutation UpdatePhotoMutation(
                    $input: UpdatePhotoInput!
                    $condition: ModelPhotoConditionInput
                ) {
                    updatePhoto(input: $input, condition: $condition) {
                        id
                        fullsize {
                            key
                            width
                            height
                        }
                        thumbnail {
                            key
                            width
                            height
                        }
                        format
                        exifMake
                        exitModel
                        objectDetected
                        processingStatus
                        geoLocation {
                            latitude {
                                d
                                m
                                s
                                direction
                            }
                            longtitude {
                                d
                                m
                                s
                                direction
                            }
                        }
                    }
                }";

            var thumbnail = JsonSerializer.Deserialize<Thumbnail>(JsonSerializer.Serialize(input.ParallelResults[1]));

            List<Label> labels = JsonSerializer.Deserialize<List<Label>>(JsonSerializer.Serialize(input.ParallelResults[0]));

            var updatePhotoMutation = new GraphQLRequest
            {
                Query = UPDATE_PHOTO,
                OperationName = "UpdatePhotoMutation",
                Variables = new
                {
                    input = new
                    {
                        id = input.ObjectId,
                        processingStatus = "SUCCEEDED",
                        fullsize = new {
                            key = input.SourceKey,
                            width = input.ExtractedMetadata?.Dimensions?.Width,
                            height = input.ExtractedMetadata?.Dimensions?.Height,
                        },
                        format = input.ExtractedMetadata?.Format,
                        exifMake = input.ExtractedMetadata?.ExifMake,
                        exitModel = input.ExtractedMetadata?.ExifModel,
                        thumbnail = new {
                            key = thumbnail?.s3key,
                            width = thumbnail?.width,
                            height = thumbnail?.height,
                        },
                        objectDetected = labels.Select(l => l.Name).ToArray(),
                        geoLocation = input.ExtractedMetadata?.Geo
                    }
                }
            };
            
            
            var photoUpdateMutationResponse = await _graphQlClient.SendMutationAsync<object>(updatePhotoMutation);

            Console.WriteLine(JsonSerializer.Serialize(photoUpdateMutationResponse));
        }
    }
}
