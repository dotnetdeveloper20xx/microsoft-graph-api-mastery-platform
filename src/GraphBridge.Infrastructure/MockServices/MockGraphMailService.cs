using GraphBridge.Application.Dtos.Graph;
using GraphBridge.Application.Interfaces.Graph;

namespace GraphBridge.Infrastructure.MockServices;

/// <summary>
/// Mock implementation of IGraphMailService for Demo_Mode.
/// Returns realistic email data without any external HTTP or network calls.
/// </summary>
public class MockGraphMailService : IGraphMailService
{
    public Task SendEmailAsync(SendEmailRequest request, CancellationToken ct = default)
    {
        // No-op in demo mode — email is conceptually sent without side effects
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<EmailSummaryDto>> GetRecentEmailsAsync(int hours = 24, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;

        var emails = new List<EmailSummaryDto>
        {
            new EmailSummaryDto
            {
                Id = "mail-001",
                Subject = "Q4 Budget Review Meeting",
                From = "Afzal Ahmed",
                Priority = "High",
                ReceivedAt = now.AddHours(-1),
                IsRead = false
            },
            new EmailSummaryDto
            {
                Id = "mail-002",
                Subject = "New Employee Onboarding - Priya Patel",
                From = "Sarah Khan",
                Priority = "Normal",
                ReceivedAt = now.AddHours(-2),
                IsRead = true
            },
            new EmailSummaryDto
            {
                Id = "mail-003",
                Subject = "Legal Matter REF-2024-0042 Update",
                From = "James Wilson",
                Priority = "High",
                ReceivedAt = now.AddHours(-3),
                IsRead = false
            },
            new EmailSummaryDto
            {
                Id = "mail-004",
                Subject = "Infrastructure Maintenance Window - Saturday",
                From = "Emma Thompson",
                Priority = "Normal",
                ReceivedAt = now.AddHours(-5),
                IsRead = true
            },
            new EmailSummaryDto
            {
                Id = "mail-005",
                Subject = "Riverside Heights Project - Weekly Progress",
                From = "David Chen",
                Priority = "Normal",
                ReceivedAt = now.AddHours(-6),
                IsRead = true
            },
            new EmailSummaryDto
            {
                Id = "mail-006",
                Subject = "Loan Approval - Greenway Property Holdings",
                From = "Afzal Ahmed",
                Priority = "High",
                ReceivedAt = now.AddHours(-8),
                IsRead = false
            },
            new EmailSummaryDto
            {
                Id = "mail-007",
                Subject = "Team Building Event - Save the Date",
                From = "Sarah Khan",
                Priority = "Low",
                ReceivedAt = now.AddHours(-10),
                IsRead = true
            },
            new EmailSummaryDto
            {
                Id = "mail-008",
                Subject = "Security Alert - Unusual Sign-in Activity",
                From = "Emma Thompson",
                Priority = "High",
                ReceivedAt = now.AddHours(-12),
                IsRead = false
            },
            new EmailSummaryDto
            {
                Id = "mail-009",
                Subject = "Contract Review - Oakfield Estates",
                From = "James Wilson",
                Priority = "Normal",
                ReceivedAt = now.AddHours(-15),
                IsRead = true
            },
            new EmailSummaryDto
            {
                Id = "mail-010",
                Subject = "Operations Dashboard - Daily Summary",
                From = "David Chen",
                Priority = "Low",
                ReceivedAt = now.AddHours(-18),
                IsRead = true
            }
        };

        return Task.FromResult<IReadOnlyList<EmailSummaryDto>>(emails.AsReadOnly());
    }

    public Task<EmailVolumeDto> GetEmailVolumeAsync(int days = 7, CancellationToken ct = default)
    {
        var volume = new EmailVolumeDto
        {
            TotalSent = 45,
            TotalReceived = 128,
            UnreadCount = 12,
            TopSenders = new List<SenderSummaryDto>
            {
                new SenderSummaryDto { SenderName = "Sarah Khan", MessageCount = 24 },
                new SenderSummaryDto { SenderName = "Afzal Ahmed", MessageCount = 19 },
                new SenderSummaryDto { SenderName = "James Wilson", MessageCount = 16 },
                new SenderSummaryDto { SenderName = "Emma Thompson", MessageCount = 14 },
                new SenderSummaryDto { SenderName = "David Chen", MessageCount = 12 },
                new SenderSummaryDto { SenderName = "Priya Patel", MessageCount = 11 },
                new SenderSummaryDto { SenderName = "Robert Hughes", MessageCount = 9 },
                new SenderSummaryDto { SenderName = "Maria Garcia", MessageCount = 8 },
                new SenderSummaryDto { SenderName = "Thomas Brown", MessageCount = 7 },
                new SenderSummaryDto { SenderName = "Lisa Anderson", MessageCount = 5 }
            }
        };

        return Task.FromResult(volume);
    }

    public Task<int> GetUnreadCountAsync(CancellationToken ct = default)
    {
        return Task.FromResult(12);
    }
}
