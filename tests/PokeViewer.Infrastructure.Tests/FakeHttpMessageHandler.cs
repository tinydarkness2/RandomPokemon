using System.Net;
using System.Text;

namespace PokeViewer.Infrastructure.Tests;

/// <summary>Returns canned responses so tests never touch the live PokeAPI.</summary>
internal sealed class FakeHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> respond) : HttpMessageHandler
{
    public List<Uri> RequestedUris { get; } = [];

    public static FakeHttpMessageHandler Json(string json, HttpStatusCode status = HttpStatusCode.OK) =>
        new(_ => new HttpResponseMessage(status)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json"),
        });

    public static FakeHttpMessageHandler Status(HttpStatusCode status) =>
        new(_ => new HttpResponseMessage(status));

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        RequestedUris.Add(request.RequestUri!);
        return Task.FromResult(respond(request));
    }
}
