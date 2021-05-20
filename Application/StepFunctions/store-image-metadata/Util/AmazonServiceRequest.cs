using Amazon;
using Amazon.Runtime;
using Amazon.Runtime.Internal;
using Amazon.Runtime.Internal.Auth;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace store_image_metadata
{
    internal class AmazonServiceRequest : IRequest
    {
        public AmazonServiceRequest(HttpRequestMessage request, IClientConfig config)
        {
            HttpMethod = request.Method.Method;
            Endpoint = new Uri(request.RequestUri.AbsoluteUri[..request.RequestUri.AbsoluteUri.LastIndexOf(request.RequestUri.AbsolutePath)]);
            ResourcePath = request.RequestUri.AbsolutePath;
            Headers = request.Headers.ToDictionary(e => e.Key, e => e.Value.FirstOrDefault());
            AuthenticationRegion = config.AuthenticationRegion;

        }

        public string RequestName { get; }
        public IDictionary<string, string> Headers { get; }
        public bool UseQueryString { get; set; }
        public IDictionary<string, string> Parameters { get; }
        public ParameterCollection ParameterCollection { get; }
        public IDictionary<string, string> SubResources { get; }
        public string HttpMethod { get; set; }
        public Uri Endpoint { get; set; }
        public string ResourcePath { get; set; }
        public IDictionary<string, string> PathResources { get; }
        public int MarshallerVersion { get; set; }
        public byte[] Content { get; set; }
        public bool SetContentFromParameters { get; set; }
        public Stream ContentStream { get; set; }
        public long OriginalStreamPosition { get; set; }
        public string OverrideSigningServiceName { get; set; }
        public string ServiceName { get; }
        public AmazonWebServiceRequest OriginalRequest { get; }
        public RegionEndpoint AlternateEndpoint { get; set; }
        public string HostPrefix { get; set; }
        public bool Suppress404Exceptions { get; set; }
        public AWS4SigningResult AWS4SignerResult { get; set; }
        public bool? DisablePayloadSigning { get; set; }
        public bool UseChunkEncoding { get; set; }
        public string CanonicalResourcePrefix { get; set; }
        public bool UseSigV4 { get; set; }
        public string AuthenticationRegion { get; set; }
        public string DeterminedSigningRegion { get; set; }

        public void AddPathResource(string key, string value)
        {
            throw new NotImplementedException();
        }

        public void AddSubResource(string subResource)
        {
            throw new NotImplementedException();
        }

        public void AddSubResource(string subResource, string value)
        {
            throw new NotImplementedException();
        }

        public string ComputeContentStreamHash()
        {
            throw new NotImplementedException();
        }

        public string GetHeaderValue(string headerName)
        {
            throw new NotImplementedException();
        }

        public bool HasRequestBody()
        {
            throw new NotImplementedException();
        }

        public bool IsRequestStreamRewindable()
        {
            throw new NotImplementedException();
        }

        public bool MayContainRequestBody()
        {
            throw new NotImplementedException();
        }
    }
}
