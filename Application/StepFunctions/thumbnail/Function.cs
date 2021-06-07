using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

using Amazon.Lambda.Core;
using Amazon.S3;
using Amazon.S3.Model;
using Common;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]
namespace thumbnail
{
    public class Function
    {
        IAmazonS3 S3Client { get; set; }

        private const int MAX_WIDTH = 250;
        private const int MAX_HEIGHT = 250;

        public Function()
        {
            this.S3Client = new AmazonS3Client();
        }

        /// <summary>
        /// A simple function that takes a string and returns both the upper and lower case version of the string.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task<ThumbnailInfo> FunctionHandler(Input input, ILambdaContext context)
        {
            var logger = new ImageRecognitionLogger(input, context);

            Dimensions size = input.ExtractedMetadata.Dimensions;

            decimal scalingFactor = Math.Min(
              MAX_WIDTH / size.Width,
              MAX_HEIGHT / size.Height
            );

            int width = Convert.ToInt32(scalingFactor * size.Width);
            int height = Convert.ToInt32(scalingFactor * size.Height);

            ThumbnailImage image = await GenerateThumbnail(input.Bucket, input.SourceKey, width, height);

            string keyPrefix = input.SourceKey.Substring(0, input.SourceKey.IndexOf("/uploads/"));
            string orignalPhotoName = input.SourceKey.Substring(input.SourceKey.LastIndexOf("/") + 1);

            string destinationKey = ThumbnailKey(keyPrefix, orignalPhotoName);

            using(var inStream = image.thumbnailImage)
            using (var stream = new MemoryStream())
            {
                image.thumbnailImage.Save(stream, image.format);
                stream.Position = 0;

                context.Logger.LogLine($"Saving thumbnail to {destinationKey} with size {stream.Length}");
                await this.S3Client.PutObjectAsync(new PutObjectRequest
                {
                    BucketName = input.Bucket,
                    Key = destinationKey,
                    InputStream = stream
                });

                await logger.WriteMessageAsync(new MessageEvent { Message = "Photo thumbnail created", CompleteEvent = true }, ImageRecognitionLogger.Target.All);

                return new ThumbnailInfo(width, height, destinationKey, input.Bucket);
            }
        }


        private string ThumbnailKey(string keyPrefix, string fileName)
        {
            return $"{keyPrefix}/resized/{fileName}";
        }

        private async Task<ThumbnailImage> GenerateThumbnail(string s3Bucket, string srcKey, int width, int height)
        {
            srcKey = WebUtility.UrlDecode(srcKey.Replace("+", " "));
            using (var response = await S3Client.GetObjectAsync(s3Bucket, srcKey))
            {
                var image = Image.Load(response.ResponseStream, out IImageFormat format);
                image.Mutate(x => x.Resize(new ResizeOptions
                {
                    Size = new Size { Width = width, Height = height },
                    Mode = ResizeMode.Stretch
                }));

                return new ThumbnailImage(image, format);
            }
        }
    }

    public record ThumbnailImage(Image thumbnailImage, IImageFormat format);

    public record ThumbnailInfo(int width, int height, string s3key, string s3Bucket);
}
