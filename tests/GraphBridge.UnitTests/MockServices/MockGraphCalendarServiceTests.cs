using FluentAssertions;
using GraphBridge.Application.Dtos.Graph;
using GraphBridge.Infrastructure.MockServices;

namespace GraphBridge.UnitTests.MockServices;

/// <summary>
/// Verifies MockGraphCalendarService returns complete DTOs with non-null, non-empty values
/// and makes no network calls (no HttpClient dependency).
/// </summary>
public class MockGraphCalendarServiceTests
{
    private readonly MockGraphCalendarService _sut = new();

    [Fact]
    public async Task CreateEventAsync_ReturnsCompleteDto()
    {
        var request = new CreateCalendarEventRequest
        {
            Subject = "Test Meeting",
            Start = DateTime.UtcNow.AddHours(1),
            End = DateTime.UtcNow.AddHours(2),
            Attendees = new List<string> { "user1@test.com", "user2@test.com" }
        };

        var result = await _sut.CreateEventAsync(request);

        result.Should().NotBeNull();
        result.Subject.Should().NotBeNullOrEmpty();
        result.Start.Should().NotBe(default);
        result.End.Should().NotBe(default);
        result.Attendees.Should().NotBeNull();
        result.Attendees.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetEventsForDateRangeAsync_ReturnsPopulatedListWithCompleteDtos()
    {
        var start = DateTime.UtcNow.Date;
        var end = start.AddDays(7);

        var result = await _sut.GetEventsForDateRangeAsync(start, end);

        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
        foreach (var evt in result)
        {
            evt.Subject.Should().NotBeNullOrEmpty();
            evt.Start.Should().NotBe(default);
            evt.End.Should().NotBe(default);
            evt.Attendees.Should().NotBeNull();
            evt.Attendees.Should().NotBeEmpty();
        }
    }

    [Fact]
    public async Task GetTodayEventsAsync_ReturnsPopulatedListWithCompleteDtos()
    {
        var result = await _sut.GetTodayEventsAsync();

        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
        foreach (var evt in result)
        {
            evt.Subject.Should().NotBeNullOrEmpty();
            evt.Start.Should().NotBe(default);
            evt.End.Should().NotBe(default);
            evt.Attendees.Should().NotBeNull();
            evt.Attendees.Should().NotBeEmpty();
        }
    }

    [Fact]
    public void DoesNotUseHttpClient()
    {
        var serviceType = typeof(MockGraphCalendarService);
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
