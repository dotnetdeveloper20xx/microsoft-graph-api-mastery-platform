using FluentAssertions;
using GraphBridge.Infrastructure.MockServices;

namespace GraphBridge.UnitTests.MockServices;

/// <summary>
/// Verifies MockGraphMailService returns complete DTOs with non-null, non-empty values
/// and makes no network calls (no HttpClient dependency).
/// </summary>
public class MockGraphMailServiceTests
{
    private readonly MockGraphMailService _sut = new();

    [Fact]
    public async Task GetRecentEmailsAsync_ReturnsPopulatedListWithCompleteDtos()
    {
        var result = await _sut.GetRecentEmailsAsync();

        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
        foreach (var email in result)
        {
            email.Id.Should().NotBeNullOrEmpty();
            email.Subject.Should().NotBeNullOrEmpty();
            email.From.Should().NotBeNullOrEmpty();
            email.Priority.Should().NotBeNullOrEmpty();
            email.ReceivedAt.Should().NotBe(default);
        }
    }

    [Fact]
    public async Task GetEmailVolumeAsync_ReturnsCompleteDto()
    {
        var result = await _sut.GetEmailVolumeAsync();

        result.Should().NotBeNull();
        result.TotalSent.Should().BeGreaterThan(0);
        result.TotalReceived.Should().BeGreaterThan(0);
        result.UnreadCount.Should().BeGreaterThanOrEqualTo(0);
        result.TopSenders.Should().NotBeNull();
        result.TopSenders.Should().NotBeEmpty();
        foreach (var sender in result.TopSenders)
        {
            sender.SenderName.Should().NotBeNullOrEmpty();
            sender.MessageCount.Should().BeGreaterThan(0);
        }
    }

    [Fact]
    public async Task GetUnreadCountAsync_ReturnsNonNegativeValue()
    {
        var result = await _sut.GetUnreadCountAsync();

        result.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public void DoesNotUseHttpClient()
    {
        var serviceType = typeof(MockGraphMailService);
        var constructors = serviceType.GetConstructors();

        foreach (var ctor in constructors)
        {
            var parameters = ctor.GetParameters();
            parameters.Should().NotContain(p => p.ParameterType == typeof(HttpClient),
                "mock services must not use HttpClient");
        }

        var fields = serviceType.GetFields(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Static);
        fields.Should().NotContain(f => f.FieldType == typeof(HttpClient),
            "mock services must not have HttpClient fields");
    }
}
