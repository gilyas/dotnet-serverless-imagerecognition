using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Linq;
using System.Threading.Tasks;

using ImageRecognition.API.Client;
using Microsoft.AspNetCore.Components.Authorization;
using Amazon.AspNetCore.Identity.Cognito;
using Amazon.Extensions.CognitoAuthentication;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace ImageRecognition.BlazorFrontend
{

    public interface IServiceClientFactory
    {
        public Task<AlbumClient> CreateAlbumClient();

        public Task<PhotoClient> CreatePhotoClient();
    }

    public class ServiceClientFactory : IServiceClientFactory
    {
        AppOptions _appOptions;
        AuthenticationStateProvider _authenticationStateProvider;
        CognitoUserManager<CognitoUser> _cognitoUserManager;

        public ServiceClientFactory(IOptions<AppOptions> appOptions, AuthenticationStateProvider authenticationStateProvider, UserManager<CognitoUser> userManager)
        {
            this._appOptions = appOptions.Value;

            this._authenticationStateProvider = authenticationStateProvider;
            this._cognitoUserManager = userManager as CognitoUserManager<CognitoUser>;
        }

        public async Task<AlbumClient> CreateAlbumClient()
        {
            var httpClient = await ConstructHttpClient();
            var albumClient = new AlbumClient(httpClient)
            {
                BaseUrl = this._appOptions.ImageRecognitionApiUrl
            };


            return albumClient;
        }


        public async Task<PhotoClient> CreatePhotoClient()
        {
            var httpClient = await ConstructHttpClient();
            var photoClient = new PhotoClient(httpClient)
            {
                BaseUrl = this._appOptions.ImageRecognitionApiUrl
            };

            return photoClient;
        }


        private async Task<HttpClient> ConstructHttpClient()
        {
            var authState = await this._authenticationStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;
            if(!user.Identity.IsAuthenticated)
                throw new Exception();

            var userId = this._cognitoUserManager.GetUserId(user);
            if (string.IsNullOrEmpty(userId))
                throw new Exception();

            var cognitoUser = await this._cognitoUserManager.FindByIdAsync(userId);
            if (string.IsNullOrEmpty(cognitoUser?.SessionTokens.IdToken))
                throw new Exception();


            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse($"bearer {cognitoUser.SessionTokens.IdToken}");


            return httpClient;
        }       
    }
}
