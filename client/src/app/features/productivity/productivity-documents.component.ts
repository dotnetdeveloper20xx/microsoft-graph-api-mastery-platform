import { Component, inject, signal, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ApiService } from '../../core/services/api.service';
import { LoadingSkeletonComponent, ErrorStateComponent, EmptyStateComponent } from '../../shared/components';

interface Document {
  name: string;
  modifiedBy: string;
  modifiedAt: string;
  location: string;
}

@Component({
  selector: 'app-productivity-documents',
  standalone: true,
  imports: [RouterLink, LoadingSkeletonComponent, ErrorStateComponent, EmptyStateComponent],
  template: `
    <div class="module-page">
      <div class="page-header">
        <a routerLink="/productivity-assistant" class="back-link">← Back to Overview</a>
        <h2>Documents</h2>
        <p>Documents accessed or modified within the past 7 days.</p>
      </div>

      @if (loading()) {
        <app-loading-skeleton [lines]="5" />
      } @else if (error()) {
        <app-error-state errorType="server" [message]="error()!" (retry)="loadDocuments()" />
      } @else if (documents().length === 0) {
        <app-empty-state
          title="No Recent Documents"
          description="No documents have been accessed or modified in the past 7 days."
          actionLabel="Back to Overview"
          actionRoute="/productivity-assistant" />
      } @else {
        <div class="documents-list">
          @for (doc of documents(); track $index) {
            <div class="document-card">
              <div class="doc-icon">📄</div>
              <div class="doc-content">
                <h4>{{ doc.name }}</h4>
                <p class="modified">Modified by {{ doc.modifiedBy }} · {{ formatDate(doc.modifiedAt) }}</p>
                <p class="location">{{ doc.location }}</p>
              </div>
            </div>
          }
        </div>
      }
    </div>
  `,
  styles: [`
    .module-page { padding: 1.5rem; }
    .page-header { margin-bottom: 1.5rem; }
    .back-link { color: #1a237e; text-decoration: none; font-size: 0.875rem; }
    .page-header h2 { color: #1a237e; margin-top: 0.5rem; }
    .page-header p { color: #607d8b; }
    .documents-list { display: flex; flex-direction: column; gap: 0.5rem; }
    .document-card { display: flex; gap: 0.75rem; padding: 0.75rem 1rem; border: 1px solid #e0e0e0; border-radius: 8px; align-items: flex-start; }
    .doc-icon { font-size: 1.5rem; }
    .doc-content h4 { margin: 0 0 0.25rem; color: #263238; font-size: 0.9rem; }
    .modified { color: #455a64; font-size: 0.8rem; margin: 0; }
    .location { color: #9e9e9e; font-size: 0.75rem; margin: 0.25rem 0 0; }
  `]
})
export class ProductivityDocumentsComponent implements OnInit {
  private api = inject(ApiService);

  documents = signal<Document[]>([]);
  loading = signal(true);
  error = signal<string | null>(null);

  ngOnInit(): void {
    this.loadDocuments();
  }

  loadDocuments(): void {
    this.loading.set(true);
    this.error.set(null);
    this.api.get<Document[]>('/productivity-assistant/documents').subscribe({
      next: (data) => {
        this.documents.set(data ?? []);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Failed to load documents.');
        this.loading.set(false);
      }
    });
  }

  formatDate(isoString: string): string {
    return new Date(isoString).toLocaleDateString([], { dateStyle: 'medium' });
  }
}
