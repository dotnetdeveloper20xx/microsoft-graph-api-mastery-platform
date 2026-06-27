import { Component, inject, signal, input, OnInit } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { ApiService } from '../../core/services/api.service';
import { LoadingSkeletonComponent, ErrorStateComponent, EmptyStateComponent } from '../../shared/components';

interface DocumentTreeNode {
  folderName: string;
  children: DocumentTreeNode[];
}

interface FlatNode {
  name: string;
  depth: number;
}

@Component({
  selector: 'app-legal-matters-documents',
  standalone: true,
  imports: [RouterLink, LoadingSkeletonComponent, ErrorStateComponent, EmptyStateComponent],
  template: `
    <div class="module-page">
      <div class="page-header">
        <a [routerLink]="['/legal-matters', matterId]" class="back-link">← Back to Matter</a>
        <h2>Document Tree</h2>
        <p>Workspace folder structure for this legal matter.</p>
      </div>

      @if (loading()) {
        <app-loading-skeleton [lines]="4" />
      } @else if (error()) {
        <app-error-state [errorType]="'server'" [message]="error()!" (retry)="loadDocuments()" />
      } @else if (flatNodes().length === 0) {
        <app-empty-state
          title="No Documents"
          description="The workspace folder structure will appear here once the workspace has been created."
          actionLabel="Back to Matter"
          [actionRoute]="'/legal-matters/' + matterId" />
      } @else {
        <div class="tree-container">
          @for (node of flatNodes(); track $index) {
            <div class="tree-item" [style.padding-left.rem]="node.depth * 1.5">
              <span class="folder-icon">📁</span>
              <span class="folder-name">{{ node.name }}</span>
            </div>
          }
        </div>
      }
    </div>
  `,
  styles: [`
    .module-page { padding: 1.5rem; }
    .back-link { color: #5c6bc0; text-decoration: none; font-size: 0.9rem; }
    .back-link:hover { text-decoration: underline; }
    .page-header { margin-bottom: 1.5rem; }
    .page-header h2 { color: #1a237e; margin: 0.5rem 0; }
    .page-header p { color: #607d8b; }
    .tree-container { background: #fff; border: 1px solid #e0e0e0; border-radius: 8px; padding: 1.25rem; }
    .tree-item { display: flex; align-items: center; gap: 0.5rem; padding: 0.4rem 0; }
    .folder-icon { font-size: 1.1rem; }
    .folder-name { color: #424242; font-size: 0.95rem; }
  `]
})
export class LegalMattersDocumentsComponent implements OnInit {
  private api = inject(ApiService);
  private route = inject(ActivatedRoute);

  matterId = '';
  flatNodes = signal<FlatNode[]>([]);
  loading = signal(true);
  error = signal<string | null>(null);

  ngOnInit(): void {
    this.matterId = this.route.snapshot.paramMap.get('id') ?? '';
    this.loadDocuments();
  }

  loadDocuments(): void {
    if (!this.matterId) return;

    this.loading.set(true);
    this.error.set(null);
    this.api.get<DocumentTreeNode[]>(`/legal-matters/${this.matterId}/documents`).subscribe({
      next: (data) => {
        this.flatNodes.set(this.flattenTree(data ?? [], 0));
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Failed to load document tree.');
        this.loading.set(false);
      }
    });
  }

  private flattenTree(nodes: DocumentTreeNode[], depth: number): FlatNode[] {
    const result: FlatNode[] = [];
    for (const node of nodes) {
      result.push({ name: node.folderName, depth });
      if (node.children?.length) {
        result.push(...this.flattenTree(node.children, depth + 1));
      }
    }
    return result;
  }
}
