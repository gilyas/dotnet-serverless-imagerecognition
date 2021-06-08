using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
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
                    UpdatedDate = DateTime.UtcNow,
                    PhotoId = photoId,
                    Owner = userId,
                    ProcessingStatus = ProcessingStatus.Pending,
                    UploadTime = DateTime.UtcNow
                };

                await this._ddbContext.SaveAsync(photo).ConfigureAwait(false);

                using (var fileStream = File.OpenWrite(tempFile))
                {
                    Utilities.CopyStream(stream, fileStream);
                }

                var putRequest = new PutObjectRequest
                {
                    BucketName = this._appOptions.PhotoStorageBucket,
                    Key = $"private/uploads/{userId}/{photoId}",
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
                AlbumId = albumName + "-" + Guid.NewGuid().ToString(),
                Name = albumName,
                UserId = userId,
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
        public async Task<Album> GetAlbumDetails(string userId, string albumId)
        {
            var albumQuery = 
                this._ddbContext.QueryAsync<Album>(userId, QueryOperator.Equal, new[] { albumId });

            var albums = await albumQuery.GetRemainingAsync().ConfigureAwait(false);

            Album album = albums.FirstOrDefault();

            if (album != null)
            {
                var photoQuery = this._ddbContext.QueryAsync<Photo>(albumId, new DynamoDBOperationConfig { IndexName = "albumID-uploadTime-index" });
                album.Photos = await photoQuery.GetRemainingAsync().ConfigureAwait(false);
                foreach (var photo in album.Photos)
                {
                    if (photo.ProcessingStatus == ProcessingStatus.Succeeded)
                    {
                        photo.Thumbnail.Url = _s3Client.GetPreSignedURL(new GetPreSignedUrlRequest
                        {
                            BucketName = _appOptions.PhotoStorageBucket,
                            Key = $"private/resized/{userId}/{photo.PhotoId}",
                            Expires = DateTime.UtcNow.AddHours(1)
                        });
                        photo.FullSize.Url = _s3Client.GetPreSignedURL(new GetPreSignedUrlRequest
                        {
                            BucketName = _appOptions.PhotoStorageBucket,
                            Key = $"private/uploads/{userId}/{photo.PhotoId}",
                            Expires = DateTime.UtcNow.AddHours(1)
                        });
                    }
                }
            }
            return album;
        }


        public async Task<Photo> GetPhotoDetails(string userId, string photoId)
        {
            var search = this._ddbContext.QueryAsync<Photo>(photoId);

            var photos = await search.GetRemainingAsync().ConfigureAwait(false);

            return photos.Where(p => p.Owner == userId).FirstOrDefault();
        }

    }
}
