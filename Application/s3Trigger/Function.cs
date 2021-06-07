using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Lambda.Core;
using Amazon.Lambda.S3Events;
using Amazon.Lambda.Serialization.SystemTextJson;
using Amazon.StepFunctions;
using Amazon.StepFunctions.Model;
using Amazon.Util;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(DefaultLambdaJsonSerializer))]

namespace s3Trigger
{
    public class Function
    {
        private string StateMachineArn { get; set; }

        private const string STATE_MACHINE_ARN = "STATE_MACHINE_ARN";
        private const string PHOTO_TABLE = "PHOTO_TABLE";

        private static IAmazonDynamoDB _ddbClient = new AmazonDynamoDBClient();
        private static IAmazonStepFunctions _stepClient = new AmazonStepFunctionsClient();

        DynamoDBContext _ddbContext;

        public Function()
        {
            StateMachineArn = Environment.GetEnvironmentVariable(STATE_MACHINE_ARN);
            
            AWSConfigsDynamoDB.Context
                .AddMapping(new TypeMapping(typeof(Photo), Environment.GetEnvironmentVariable(PHOTO_TABLE)));
            
            _ddbContext = new DynamoDBContext(_ddbClient);
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
            string key = WebUtility.UrlDecode(evnt.Records[0].S3.Object.Key);

            Console.WriteLine(bucket);
            Console.WriteLine(key);

            string objectId = key.Substring(key.LastIndexOf("/") + 1);

            Console.WriteLine(objectId);

            var input = new
            {
                Bucket = bucket,
                SourceKey = key,
                PhotoId = objectId,
                TablePhoto = Environment.GetEnvironmentVariable(PHOTO_TABLE)
            };

            var stepResponse = await _stepClient.StartExecutionAsync(new StartExecutionRequest
            {
                StateMachineArn = StateMachineArn,
                Name = $"{MakeSafeName(key, 80)}",
                Input = JsonConvert.SerializeObject(input)
            }).ConfigureAwait(false);

            Photo photo = new Photo
            {
                PhotoId = objectId,
                SfnExecutionArn = stepResponse.ExecutionArn,
                ProcessingStatus = ProcessingStatus.Running,
                UpdatedDate = DateTime.UtcNow
            };

            await this._ddbContext.SaveAsync(photo).ConfigureAwait(false);
        }

        public static string MakeSafeName(string displayName, int maxSize)
        {
            var builder = new StringBuilder();
            foreach (char c in displayName)
            {
                if (char.IsLetterOrDigit(c))
                {
                    builder.Append(c);
                }
                else
                {
                    builder.Append('-');
                }
            }

            var name = builder.ToString();

            if (maxSize < name.Length)
            {
                name = name.Substring(0, maxSize);
            }

            return name;
        }
    }
}
