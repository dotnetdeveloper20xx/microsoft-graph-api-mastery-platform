import { Component, inject, signal, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ApiService } from '../../core/services/api.service';
import { LoadingSkeletonComponent, ErrorStateComponent } from '../../shared/components';
import { GraphExplanationPanelComponent, GraphPanelData } from '../../shared/components/graph-explanation-panel/graph-explanation-panel.component';

interface CeoOverview {
  todayMeetingsCount: number;
  unreadEmailsCount: number;
  pendingTasksCount: number;
  pendingDocumentApprovalsCount: number;
  activeSecuritySignalsCount: number;
  unavailableSections: { section: string; errorMessage: string }[];
}

@Component({
  selector: 'app-ceo-command-centre-overview',
  standalone: true,
  imports: [RouterLink, LoadingSkeletonComponent, ErrorStateComponent, GraphExplanationPanelComponent],
  template: `
    <div class="module-page">
      <div class="page-header">
        <h2>CEO Command Centre Dashboard</h2>
        <p>Aggregated executive view — meetings, emails, tasks, documents, and security alerts in one dashboard.</p>
      </div>

      @if (loading()) {
        <app-loading-skeleton [lines]="5" />
      } @else if (error()) {
        <app-error-state errorType="server" [message]="error()!" (retry)="loadOverview()" />
      } @else if (overview()) {
        <div class="dashboard-grid">
          <a routerLink="today" class="dashboard-card">
            <span class="card-value">{{ overview()!.todayMeetingsCount }}</span>
            <span class="card-label">Today's Meetings</span>
          </a>
          <a routerLink="emails" class="dashboard-card">
            <span class="card-value">{{ overview()!.unreadEmailsCount }}</span>
            <span class="card-label">Unread Emails</span>
          </a>
          <a routerLink="tasks" class="dashboard-card">
            <span class="card-value">{{ overview()!.pendingTasksCount }}</span>
            <span class="card-label">Pending Tasks</span>
          </a>
          <a routerLink="documents" class="dashboard-card">
            <span class="card-value">{{ overview()!.pendingDocumentApprovalsCount }}</span>
            <span class="card-label">Document Approvals</span>
          </a>
          <a routerLink="security" class="dashboard-card alert">
            <span class="card-value">{{ overview()!.activeSecuritySignalsCount }}</span>
            <span class="card-label">Security Signals</span>
          </a>
        </div>

        @if (overview()!.unavailableSections.length > 0) {
          <div class="warnings">
            <h4>Unavailable Sections</h4>
            @for (section of overview()!.unavailableSections; track section.section) {
              <p class="warning-item">{{ section.section }}: {{ section.errorMessage }}</p>
            }
          </div>
        }
      }

      <app-graph-explanation-panel [panelData]="graphPanelData" />
    </div>
  `,
  styles: [`
    .module-page { padding: 1.5rem; }
    .page-header { margin-bottom: 1.5rem; }
    .page-header h2 { color: #1a237e; margin-bottom: 0.5rem; }
    .page-header p { color: #607d8b; }
    .dashboard-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(180px, 1fr)); gap: 1rem; }
    .dashboard-card { display: flex; flex-direction: column; align-items: center; padding: 1.5rem; border: 1px solid #e0e0e0; border-radius: 8px; text-decoration: none; transition: box-shadow 0.2s; }
    .dashboard-card:hover { box-shadow: 0 2px 8px rgba(0,0,0,0.1); }
    .dashboard-card.alert { border-color: #ffcdd2; }
    .card-value { font-size: 2.5rem; font-weight: 700; color: #1a237e; }
    .alert .card-value { color: #c62828; }
    .card-label { font-size: 0.85rem; color: #607d8b; margin-top: 0.5rem; text-align: center; }
    .warnings { margin-top: 1.5rem; padding: 1rem; background: #fff3e0; border-radius: 4px; }
    .warnings h4 { color: #e65100; margin-bottom: 0.5rem; }
    .warning-item { color: #bf360c; font-size: 0.875rem; margin-bottom: 0.25rem; }
  `]
})
export class CeoCommandCentreOverviewComponent implements OnInit {
  private api = inject(ApiService);

  readonly graphPanelData: GraphPanelData = {
    endpoints: ['GET /me/calendarView', 'GET /me/messages', 'GET /me/planner/tasks', 'GET /me/drive/recent', 'GET /security/alerts'],
    scopes: ['Calendars.Read', 'Mail.Read', 'Tasks.Read', 'Files.Read.All', 'SecurityEvents.Read.All'],
    descriptions: [
      'Read today\'s calendar events',
      'Retrieve email summaries',
      'List pending planner tasks',
      'Access recent documents',
      'Monitor security alerts'
    ]
  };

  overview = signal<CeoOverview | null>(null);
  loading = signal(true);
  error = signal<string | null>(null);

  ngOnInit(): void {
    this.loadOverview();
  }

  loadOverview(): void {
    this.loading.set(true);
    this.error.set(null);
    this.api.get<CeoOverview>('/ceo-command-centre/overview').subscribe({
      next: (data) => {
        this.overview.set(data);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Failed to load CEO dashboard overview.');
        this.loading.set(false);
      }
    });
  }
}
