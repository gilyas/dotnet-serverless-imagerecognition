using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.S3;
using ImageRecognition.API.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ImageRecognition.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class AlbumController : Controller
    {
        AppOptions _appOptions;
        IAmazonDynamoDB _ddbClient;
        IAmazonS3 _s3Client;
        DynamoDBContext _ddbContext;

        public AlbumController(IOptions<AppOptions> appOptions, IAmazonDynamoDB dbClient, IAmazonS3 s3Client)
        {
            _appOptions = appOptions.Value;
            _ddbClient = dbClient;
            _s3Client = s3Client;
            _ddbContext = new DynamoDBContext(this._ddbClient);
        }

        /// <summary>
        /// Get the list of albums the user has created.
        /// </summary>
        /// <param name="includePublic">If true then also include the galleries that have been marked as public.</param>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(200, Type = typeof(Album[]))]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<JsonResult> GetUserAlbums()
        {
            var userId = Utilities.GetUsername(this.HttpContext.User);

            var search = this._ddbContext.QueryAsync<Album>(userId);

            var albums = await search.GetRemainingAsync().ConfigureAwait(false);

            return new JsonResult(albums);
        }


        /// <summary>
        /// Create a new empty album.
        /// </summary>
        /// <param name="name">The name of the album.</param>
        /// <returns>The album id to use for adding photos to the album.</returns>
        [HttpPut("{name}")]
        [ProducesResponseType(200, Type = typeof(CreateAlbumResult))]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<JsonResult> CreateAlbum(string name)
        {
            var userId = Utilities.GetUsername(this.HttpContext.User);

            var album = new Album
            {
                AlbumId = name + "-" + Guid.NewGuid().ToString(),
                Name = name,
                UserId = userId,
                CreateDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };

            await this._ddbContext.SaveAsync(album).ConfigureAwait(false);

            return new JsonResult(new CreateAlbumResult { AlbumId = album.AlbumId });
        }

        class CreateAlbumResult
        {
            public string AlbumId { get; set; }
        }

        /// <summary>
        /// Delete a album.
        /// </summary>
        /// <param name="albumId">The id of the album to delete.</param>
        /// <returns></returns>
        [HttpDelete("{albumId}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> DeleteAlbum(string albumId)
        {
            var userId = Utilities.GetUsername(this.HttpContext.User);

            await this._ddbContext.DeleteAsync<Album>(userId, albumId);

            return Ok();
        }

    }
}
