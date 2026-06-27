using GraphBridge.Application.Dtos.Graph;

namespace GraphBridge.Application.Interfaces.Graph;

/// <summary>
/// Abstraction for Microsoft Graph Drive (SharePoint/OneDrive) API operations.
/// Implementations exist for both Live (Graph SDK) and Demo (mock data) modes.
/// </summary>
public interface IGraphDriveService
{
    Task<FolderStructureDto> CreateFolderStructureAsync(CreateFolderStructureRequest request, CancellationToken ct = default);
    Task<FolderStructureDto> GetFolderStructureAsync(string workspaceId, CancellationToken ct = default);
    Task<IReadOnlyList<DocumentDto>> GetRecentDocumentsAsync(int days = 7, int limit = 50, CancellationToken ct = default);
    Task<IReadOnlyList<DocumentDto>> GetPendingApprovalsAsync(int limit = 50, CancellationToken ct = default);
}
