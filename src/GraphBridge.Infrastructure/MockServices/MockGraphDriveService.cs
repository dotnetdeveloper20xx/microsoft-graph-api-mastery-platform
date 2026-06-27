using GraphBridge.Application.Dtos.Graph;
using GraphBridge.Application.Interfaces.Graph;

namespace GraphBridge.Infrastructure.MockServices;

/// <summary>
/// Mock implementation of IGraphDriveService for Demo_Mode.
/// Returns folder structures and document lists with realistic file names and dates.
/// No external HTTP or network calls are made.
/// </summary>
public class MockGraphDriveService : IGraphDriveService
{
    public Task<FolderStructureDto> CreateFolderStructureAsync(CreateFolderStructureRequest request, CancellationToken ct = default)
    {
        var root = new FolderStructureDto
        {
            Name = "Root",
            Children = request.FolderNames
                .Select(name => new FolderStructureDto
                {
                    Name = name,
                    Children = new List<FolderStructureDto>()
                })
                .ToList()
        };

        return Task.FromResult(root);
    }

    public Task<FolderStructureDto> GetFolderStructureAsync(string workspaceId, CancellationToken ct = default)
    {
        var folderStructure = new FolderStructureDto
        {
            Name = "Root",
            Children = new List<FolderStructureDto>
            {
                new()
                {
                    Name = "Correspondence",
                    Children = new List<FolderStructureDto>
                    {
                        new() { Name = "Inbound", Children = new List<FolderStructureDto>() },
                        new() { Name = "Outbound", Children = new List<FolderStructureDto>() }
                    }
                },
                new()
                {
                    Name = "Contracts",
                    Children = new List<FolderStructureDto>
                    {
                        new() { Name = "Drafts", Children = new List<FolderStructureDto>() },
                        new() { Name = "Signed", Children = new List<FolderStructureDto>() }
                    }
                },
                new()
                {
                    Name = "Evidence",
                    Children = new List<FolderStructureDto>()
                },
                new()
                {
                    Name = "Notes",
                    Children = new List<FolderStructureDto>()
                }
            }
        };

        return Task.FromResult(folderStructure);
    }

    public Task<IReadOnlyList<DocumentDto>> GetRecentDocumentsAsync(int days = 7, int limit = 50, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;

        var documents = new List<DocumentDto>
        {
            new()
            {
                Name = "Q4 Budget Report.xlsx",
                ModifiedBy = "Helen Clarke",
                ModifiedAt = now.AddDays(-1).AddHours(-3),
                Location = "/Finance/Reports"
            },
            new()
            {
                Name = "Contract Draft - Oakfield.docx",
                ModifiedBy = "Emma Roberts",
                ModifiedAt = now.AddDays(-1).AddHours(-6),
                Location = "/Legal Matters/Contracts/Drafts"
            },
            new()
            {
                Name = "Site Survey Results.pdf",
                ModifiedBy = "James Whitfield",
                ModifiedAt = now.AddDays(-2).AddHours(-2),
                Location = "/BuildEstate/Site Reports"
            },
            new()
            {
                Name = "Board Presentation - December.pptx",
                ModifiedBy = "Afzal Ahmed",
                ModifiedAt = now.AddDays(-2).AddHours(-8),
                Location = "/Executive/Presentations"
            },
            new()
            {
                Name = "Employee Handbook v3.2.pdf",
                ModifiedBy = "Sarah Khan",
                ModifiedAt = now.AddDays(-3).AddHours(-1),
                Location = "/HR/Policies"
            },
            new()
            {
                Name = "Riverside Heights - Planning Application.pdf",
                ModifiedBy = "David Thompson",
                ModifiedAt = now.AddDays(-4).AddHours(-5),
                Location = "/BuildEstate/Planning Documents"
            },
            new()
            {
                Name = "Client Meeting Notes - Greenway.docx",
                ModifiedBy = "Priya Patel",
                ModifiedAt = now.AddDays(-5).AddHours(-2),
                Location = "/Loan Approvals/Notes"
            },
            new()
            {
                Name = "IT Security Audit Report.pdf",
                ModifiedBy = "Marcus Johnson",
                ModifiedAt = now.AddDays(-6).AddHours(-4),
                Location = "/Security/Audits"
            }
        };

        IReadOnlyList<DocumentDto> result = documents.Take(limit).ToList();
        return Task.FromResult(result);
    }

    public Task<IReadOnlyList<DocumentDto>> GetPendingApprovalsAsync(int limit = 50, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;

        var pendingDocuments = new List<DocumentDto>
        {
            new()
            {
                Name = "Annual Leave Policy Update.docx",
                ModifiedBy = "Sarah Khan",
                ModifiedAt = now.AddDays(-1).AddHours(-4),
                Location = "/HR/Policies/Pending"
            },
            new()
            {
                Name = "Vendor Agreement - CloudTech Solutions.pdf",
                ModifiedBy = "Emma Roberts",
                ModifiedAt = now.AddDays(-2).AddHours(-1),
                Location = "/Contracts/Pending Approval"
            },
            new()
            {
                Name = "Capital Expenditure Request - Q1.xlsx",
                ModifiedBy = "David Thompson",
                ModifiedAt = now.AddDays(-3).AddHours(-6),
                Location = "/Finance/Approvals"
            }
        };

        IReadOnlyList<DocumentDto> result = pendingDocuments.Take(limit).ToList();
        return Task.FromResult(result);
    }
}
