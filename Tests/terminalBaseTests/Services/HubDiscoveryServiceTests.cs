using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Text;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Interfaces;
using Fr8.Infrastructure.Security;
using Fr8.TerminalBase.Interfaces;
using Fr8.TerminalBase.Services;
using Fr8.Testing.Unit;
using Moq;
using NUnit.Framework;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace terminaBaselTests.Services
{
    [TestFixture]
    [Category("HubDiscovery")]
    public class HubDiscoveryServiceTests : BaseTest
    {
        private class ActivityStoreStub : IActivityStore
        {
            public TerminalDTO Terminal { get; }

            public ActivityStoreStub(TerminalDTO terminal)
            {
                Terminal = terminal;
            }

            public void RegisterActivity(ActivityTemplateDTO activityTemplate, IActivityFactory activityFactory)
            {
            }

            public void RegisterActivity<T>(ActivityTemplateDTO activityTemplate) where T : IActivity
            {
            }

            public IActivityFactory GetFactory(string name, string version)
            {
                return null;
            }

            public List<ActivityTemplateDTO> GetAllTemplates()
            {
                return new List<ActivityTemplateDTO>();
            }
        }

        private class RestfulClientStub : IRestfulServiceClient
        {
            public int PostCallCount;

            public Task<Stream> DownloadAsync(Uri requestUri, string CorrelationId = null, Dictionary<string, string> headers = null)
            {
                throw new NotImplementedException();
            }

            public Task<TResponse> GetAsync<TResponse>(Uri requestUri, string CorrelationId = null, Dictionary<string, string> headers = null)
            {
                throw new NotImplementedException();
            }

            public Task<string> GetAsync(Uri requestUri, string CorrelationId = null, Dictionary<string, string> headers = null)
            {
                throw new NotImplementedException();
            }

            public Task<TResponse> PostAsync<TResponse>(Uri requestUri, string CorrelationId = null, Dictionary<string, string> headers = null)
            {
                PostCallCount++;
                return Task.FromResult(default(TResponse));
            }

            public Task<string> PostAsync(Uri requestUri, string CorrelationId = null, Dictionary<string, string> headers = null)
            {
                PostCallCount++;
                return Task.FromResult(string.Empty);
            }

            public Task<string> PostAsync<TContent>(Uri requestUri, TContent content, string CorrelationId = null, Dictionary<string, string> headers = null)
            {
                PostCallCount++;
                return Task.FromResult(string.Empty);
            }

            public Task<TResponse> PostAsync<TContent, TResponse>(Uri requestUri, TContent content, string CorrelationId = null, Dictionary<string, string> headers = null)
            {
                PostCallCount++;
                return Task.FromResult(default(TResponse));
            }

            public Task DeleteAsync(Uri requestUri, string CorrelationId = null, Dictionary<string, string> headers = null)
            {
                return Task.FromResult(0);
            }

            public Task<TResponse> PutAsync<TContent, TResponse>(Uri requestUri, TContent content, string CorrelationId = null, Dictionary<string, string> headers = null)
            {
                throw new NotImplementedException();
            }

            public Task<string> PutAsync<TContent>(Uri requestUri, TContent content, string CorrelationId = null, Dictionary<string, string> headers = null)
            {
                throw new NotImplementedException();
            }

            public Task<string> PostAsync(Uri requestUri, HttpContent content, string CorrelationId = null, Dictionary<string, string> headers = null)
            {
                return Task.FromResult(string.Empty);
            }

            public Task<TResponse> PostAsync<TResponse>(Uri requestUri, HttpContent content, string CorrelationId = null, Dictionary<string, string> headers = null)
            {
                PostCallCount++;
                return Task.FromResult(default(TResponse));
            }

            public Task<TResponse> PutAsync<TResponse>(Uri requestUri, HttpContent content, string CorrelationId = null, Dictionary<string, string> headers = null)
            {
                throw new NotImplementedException();
            }

            public Task<string> PutAsync(Uri requestUri, HttpContent content, string CorrelationId = null, Dictionary<string, string> headers = null)
            {
                throw new NotImplementedException();
            }

            public void AddRequestSignature(IRequestSignature signature)
            {
                throw new NotImplementedException();
            }

            public void ClearSignatures()
            {
                throw new NotImplementedException();
            }
        }

        class FakeRestfulServiceClientFactory : IRestfulServiceClientFactory
        {
            private readonly RestfulClientStub _clientStub;

            public FakeRestfulServiceClientFactory(RestfulClientStub clientStub)
            {
                _clientStub = clientStub;
            }

            public IRestfulServiceClient Create(IRequestSignature signature)
            {
                return _clientStub;
            }
        }


        private HubDiscoveryService _hubDiscoveryService;
        private RestfulClientStub _restfullServiceClient;

        [SetUp]
        public void Setup()
        {
            var activityStore = new ActivityStoreStub(new TerminalDTO
            {
                Endpoint = "http://test",
                Name = "test"
            });
            _restfullServiceClient = new RestfulClientStub();
            _hubDiscoveryService = new HubDiscoveryService(new FakeRestfulServiceClientFactory(_restfullServiceClient), activityStore, new SingleRunRetryPolicy());
        }

        [Test]
        public async Task RequestDiscoverOnceForNewHub()
        {
            var task = Task.Run(async () => await _hubDiscoveryService.GetHubCommunicator("http://hub1"));
            var task2 = Task.Run(async () => await _hubDiscoveryService.GetHubCommunicator("http://hub1"));

            await Task.Delay(500);

            Assert.AreEqual(1, _restfullServiceClient.PostCallCount, "Invalid number of force discovery calls");

            _hubDiscoveryService.SetHubSecret("http://hub1", string.Empty);
        }

    }
}
