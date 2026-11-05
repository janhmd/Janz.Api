using System.Net;

namespace Janz.Tests.Fakes
{
    // Xarici API-yə (Gemini) getmədən hazır cavab qaytaran HttpClient
    public class FakeHttpClientFactory : IHttpClientFactory
    {
        private readonly HttpStatusCode _status;
        private readonly string _body;

        public string? LastRequestBody { get; private set; }
        public string? LastApiKey { get; private set; }
        public int Calls { get; private set; }

        public FakeHttpClientFactory(HttpStatusCode status = HttpStatusCode.OK, string body = "{}")
        {
            _status = status;
            _body = body;
        }

        public HttpClient CreateClient(string name) => new HttpClient(new Handler(this));

        private class Handler : HttpMessageHandler
        {
            private readonly FakeHttpClientFactory _owner;

            public Handler(FakeHttpClientFactory owner)
            {
                _owner = owner;
            }

            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                _owner.Calls++;
                _owner.LastApiKey = request.Headers.TryGetValues("x-goog-api-key", out var values) ? values.FirstOrDefault() : null;
                _owner.LastRequestBody = request.Content is null ? null : await request.Content.ReadAsStringAsync(cancellationToken);

                return new HttpResponseMessage(_owner._status) { Content = new StringContent(_owner._body) };
            }
        }
    }
}
