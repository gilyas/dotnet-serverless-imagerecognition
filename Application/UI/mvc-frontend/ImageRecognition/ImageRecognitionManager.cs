using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.StepFunctions;
using ImageRecognition.Frontend.Models;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ImageRecognition.Frontend
{
    public class ImageRecognitionManager
    {
        AppOptions _appOptions;
        IAmazonDynamoDB _ddbClient;
        IAmazonS3 _s3Client;
        IAmazonStepFunctions _stepClient;
        DynamoDBContext _ddbContext;

        public ImageRecognitionManager(IOptions<AppOptions> appOptions, IAmazonDynamoDB dbClient, IAmazonS3 s3Client, IAmazonStepFunctions stepClient)
        {
            _appOptions = appOptions.Value;
            _ddbClient = dbClient;
            _s3Client = s3Client;
            _stepClient = stepClient;
            _ddbContext = new DynamoDBContext(this._ddbClient);
        }
        // Add photo to Album.
        // insert into photo dynamo table as pending
        // upload to S3
        public async Task<Photo> AddPhoto(string albumId, string userId, string name, Stream stream)
        {
            var tempFile = Path.GetTempFileName();
            try 
            {
                var photoId = name + "-" + Guid.NewGuid().ToString();

                var photo = new Photo { 
                    AlbumId = albumId,
                    Bucket = _appOptions.PhotoStorageBucket,
                    CreatedDate = DateTime.UtcNow,
                    Id = photoId,
                    Owner = userId,
                    ProcessingStatus = ProcessingStatus.Pending,
                    
                };

                await this._ddbContext.SaveAsync(photo).ConfigureAwait(false);

                using (var fileStream = File.OpenWrite(tempFile))
                {
                    Utilities.CopyStream(stream, fileStream);
                }

                var putRequest = new PutObjectRequest
                {
                    BucketName = this._appOptions.PhotoStorageBucket,
                    Key = $"private/upload/{photoId}",
                    FilePath = tempFile
                };
                await this._s3Client.PutObjectAsync(putRequest).ConfigureAwait(false);

                return photo;
            }
            finally
            {
                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                }
            }

        }

        // Get Step function status.


        // Create Album
        //  insert entry into Album dynamo db table.
        public async Task<Album> CreateAlbum(string userId, string albumName)
        {
            var album = new Album { 
                Id = albumName + "-" + new Guid().ToString(),
                Name = albumName,
                Owner = userId,
                CreateDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };

            await this._ddbContext.SaveAsync(album).ConfigureAwait(false);

            return album;
        }

        // Get Albums by user
        public async Task<IList<Album>> GetAlbums(string userId)
        {
            var search = this._ddbContext.QueryAsync<Album>(userId);

            return await search.GetRemainingAsync().ConfigureAwait(false);
        }


        // Get Album Details.
        public async Task<IList<Photo>> GetAlbumDetails(string userId, string albumId)
        {
            var search = this._ddbContext.QueryAsync<Photo>(albumId);

            var photos = await search.GetRemainingAsync().ConfigureAwait(false);

            return photos.Where(p => p.Owner == userId).ToList();
        }


        public async Task<Photo> GetPhotoDetails(string userId, string photoId)
        {
            var search = this._ddbContext.QueryAsync<Photo>(photoId);

            var photos = await search.GetRemainingAsync().ConfigureAwait(false);

            return photos.Where(p => p.Owner == userId).FirstOrDefault();
        }

    }
}
