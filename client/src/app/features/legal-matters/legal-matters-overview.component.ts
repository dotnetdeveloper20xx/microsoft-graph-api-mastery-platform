import { Component, inject, signal, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ApiService } from '../../core/services/api.service';
import { LoadingSkeletonComponent, ErrorStateComponent, EmptyStateComponent } from '../../shared/components';
import { GraphExplanationPanelComponent, GraphPanelData } from '../../shared/components/graph-explanation-panel/graph-explanation-panel.component';

export interface LegalMatterSummary {
  id: string;
  referenceNumber: string;
  clientName: string;
  matterType: string;
  assignedSolicitor: string;
  workspaceCreated: boolean;
  participantCount: number;
}

@Component({
  selector: 'app-legal-matters-overview',
  standalone: true,
  imports: [RouterLink, LoadingSkeletonComponent, ErrorStateComponent, EmptyStateComponent, GraphExplanationPanelComponent],
  template: `
    <div class="module-page">
      <div class="page-header">
        <h2>Legal Matter Workspace Automation</h2>
        <p>Create Microsoft 365 workspaces for legal matters — SharePoint folders, Teams channels, participant access, and kickoff meetings.</p>
        <a routerLink="create" class="btn-primary">+ New Legal Matter</a>
      </div>

      @if (loading()) {
        <app-loading-skeleton [lines]="5" />
      } @else if (error()) {
        <app-error-state [errorType]="'server'" [message]="error()!" (retry)="loadMatters()" />
      } @else if (matters().length === 0) {
        <app-empty-state
          title="No Legal Matters"
          description="Legal matters with automated Microsoft 365 workspaces will appear here once created."
          actionLabel="Create Legal Matter"
          actionRoute="create" />
      } @else {
        <div class="matters-grid">
          @for (matter of matters(); track matter.id) {
            <div class="matter-card">
              <div class="matter-header">
                <span class="reference">{{ matter.referenceNumber }}</span>
                @if (matter.workspaceCreated) {
                  <span class="badge badge-success">Workspace Active</span>
                } @else {
                  <span class="badge badge-pending">Pending Setup</span>
                }
              </div>
              <h3>{{ matter.clientName }}</h3>
              <p class="matter-type">{{ matter.matterType }}</p>
              <p class="solicitor">Solicitor: {{ matter.assignedSolicitor }}</p>
              <p class="participants">Participants: {{ matter.participantCount }}</p>
              <a [routerLink]="[matter.id]" class="btn-link">View Details →</a>
            </div>
          }
        </div>
      }

      <app-graph-explanation-panel [panelData]="graphPanelData" />
    </div>
  `,
  styles: [`
    .module-page { padding: 1.5rem; }
    .page-header { margin-bottom: 2rem; }
    .page-header h2 { color: #1a237e; margin-bottom: 0.5rem; }
    .page-header p { color: #607d8b; margin-bottom: 1rem; }
    .btn-primary {
      display: inline-block; padding: 0.6rem 1.2rem;
      background: #1a237e; color: #fff; border-radius: 6px;
      text-decoration: none; font-weight: 500;
    }
    .btn-primary:hover { background: #283593; }
    .matters-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(320px, 1fr)); gap: 1.5rem; }
    .matter-card {
      padding: 1.25rem; border: 1px solid #e0e0e0; border-radius: 8px;
      background: #fff; transition: box-shadow 0.2s;
    }
    .matter-card:hover { box-shadow: 0 2px 8px rgba(0,0,0,0.1); }
    .matter-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 0.75rem; }
    .reference { font-size: 0.85rem; color: #5c6bc0; font-weight: 600; }
    .badge { font-size: 0.75rem; padding: 0.2rem 0.5rem; border-radius: 4px; font-weight: 500; }
    .badge-success { background: #e8f5e9; color: #2e7d32; }
    .badge-pending { background: #fff3e0; color: #ef6c00; }
    .matter-card h3 { margin: 0 0 0.5rem; color: #212121; font-size: 1.1rem; }
    .matter-type { color: #757575; font-size: 0.9rem; margin-bottom: 0.25rem; }
    .solicitor { color: #757575; font-size: 0.85rem; margin-bottom: 0.25rem; }
    .participants { color: #757575; font-size: 0.85rem; margin-bottom: 0.75rem; }
    .btn-link { color: #1a237e; text-decoration: none; font-weight: 500; font-size: 0.9rem; }
    .btn-link:hover { text-decoration: underline; }
  `]
})
export class LegalMattersOverviewComponent implements OnInit {
  private api = inject(ApiService);

  readonly graphPanelData: GraphPanelData = {
    endpoints: ['POST /drive/root/children', 'POST /teams/{id}/channels', 'POST /me/events'],
    scopes: ['Files.ReadWrite.All', 'Channel.Create', 'Calendars.ReadWrite'],
    descriptions: [
      'Create SharePoint folder structures for matter documents',
      'Create Teams channels for matter collaboration',
      'Schedule kickoff meetings with participants'
    ]
  };

  matters = signal<LegalMatterSummary[]>([]);
  loading = signal(true);
  error = signal<string | null>(null);

  ngOnInit(): void {
    this.loadMatters();
  }

  loadMatters(): void {
    this.loading.set(true);
    this.error.set(null);
    this.api.get<LegalMatterSummary[]>('/legal-matters/overview').subscribe({
      next: (data) => {
        this.matters.set(data ?? []);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Failed to load legal matters.');
        this.loading.set(false);
      }
    });
  }
}
