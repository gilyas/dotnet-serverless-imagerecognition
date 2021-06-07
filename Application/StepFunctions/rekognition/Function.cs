using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

using Amazon.Lambda.Core;
using Amazon.Rekognition;
using Amazon.Rekognition.Model;
using Common;
using ImageRecognition.Communication.Manager;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace rekognition
{
    public class Function
    {
        /// <summary>
        /// The default minimum confidence used for detecting labels.
        /// </summary>
        public const float DEFAULT_MIN_CONFIDENCE = 60f;

        /// <summary>
        /// The name of the environment variable to set which will override the default minimum confidence level.
        /// </summary>
        public const string MIN_CONFIDENCE_ENVIRONMENT_VARIABLE_NAME = "MinConfidence";

        public const int MaxLabels = 10;

        IAmazonRekognition RekognitionClient { get; }

        float MinConfidence { get; set; } = DEFAULT_MIN_CONFIDENCE;

        public Function()
        {
            this.RekognitionClient = new AmazonRekognitionClient();

            var environmentMinConfidence = System.Environment.GetEnvironmentVariable(MIN_CONFIDENCE_ENVIRONMENT_VARIABLE_NAME);
            if (!string.IsNullOrWhiteSpace(environmentMinConfidence))
            {
                float value;
                if (float.TryParse(environmentMinConfidence, out value))
                {
                    this.MinConfidence = value;
                    Console.WriteLine($"Setting minimum confidence to {this.MinConfidence}");
                }
                else
                {
                    Console.WriteLine($"Failed to parse value {environmentMinConfidence} for minimum confidence. Reverting back to default of {this.MinConfidence}");
                }
            }
            else
            {
                Console.WriteLine($"Using default minimum confidence of {this.MinConfidence}");
            }
        }

        /// <summary>
        /// A simple function that takes a string and returns both the upper and lower case version of the string.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task<List<Label>> FunctionHandler(ExecutionInput input, ILambdaContext context)
        {
            var logger = new ImageRecognitionLogger(input, context);

            Console.WriteLine($"Looking for labels in image {input.Bucket}:{input.SourceKey}");

            string key = WebUtility.UrlDecode(input.SourceKey);

            var detectResponses = await this.RekognitionClient.DetectLabelsAsync(new DetectLabelsRequest
            {
                MinConfidence = MinConfidence,
                MaxLabels = MaxLabels,
                Image = new Image
                {
                    S3Object = new Amazon.Rekognition.Model.S3Object
                    {
                        Bucket = input.Bucket,
                        Name = key
                    }
                }
            });

            await logger.WriteMessageAsync(new MessageEvent { Message = "Photo labels extracted succesfully", CompleteEvent = true }, ImageRecognitionLogger.Target.All);

            return detectResponses.Labels;
        }
    }
}
