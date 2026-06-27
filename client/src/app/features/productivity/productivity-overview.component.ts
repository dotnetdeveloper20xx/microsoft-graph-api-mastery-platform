import { Component, inject, signal, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ApiService } from '../../core/services/api.service';
import { LoadingSkeletonComponent, ErrorStateComponent } from '../../shared/components';
import { GraphExplanationPanelComponent, GraphPanelData } from '../../shared/components/graph-explanation-panel/graph-explanation-panel.component';

interface ProductivityOverview {
  weeklyEvents: unknown[];
  emailSummary: { totalSent: number; totalReceived: number; unreadCount: number };
  taskSummary: { completed: number; overdue: number; inProgress: number };
  recentDocuments: unknown[];
  unavailableSections: { section: string; errorMessage: string }[];
}

@Component({
  selector: 'app-productivity-overview',
  standalone: true,
  imports: [RouterLink, LoadingSkeletonComponent, ErrorStateComponent, GraphExplanationPanelComponent],
  template: `
    <div class="module-page">
      <div class="page-header">
        <h2>AI Meeting & Productivity Assistant</h2>
        <p>Weekly productivity summaries — calendar, email, tasks, and document activity aggregated for AI consumption.</p>
        <button class="btn-primary" (click)="generateSummary()" [disabled]="generating()">
          {{ generating() ? 'Generating...' : 'Generate Weekly Summary' }}
        </button>
      </div>

      @if (summaryMessage()) {
        <div class="summary-message" [class.success]="!summaryError()" [class.error]="summaryError()">
          {{ summaryMessage() }}
        </div>
      }

      @if (loading()) {
        <app-loading-skeleton [lines]="5" />
      } @else if (error()) {
        <app-error-state errorType="server" [message]="error()!" (retry)="loadOverview()" />
      } @else if (overview()) {
        <div class="sections-grid">
          <a routerLink="calendar" class="section-card">
            <span class="section-icon">📅</span>
            <span class="section-label">Calendar</span>
            <span class="section-count">{{ overview()!.weeklyEvents.length }} events</span>
          </a>
          <a routerLink="emails" class="section-card">
            <span class="section-icon">📧</span>
            <span class="section-label">Emails</span>
            <span class="section-count">{{ overview()!.emailSummary.totalReceived }} received</span>
          </a>
          <a routerLink="tasks" class="section-card">
            <span class="section-icon">✅</span>
            <span class="section-label">Tasks</span>
            <span class="section-count">{{ overview()!.taskSummary.completed }} completed</span>
          </a>
          <a routerLink="documents" class="section-card">
            <span class="section-icon">📁</span>
            <span class="section-label">Documents</span>
            <span class="section-count">{{ overview()!.recentDocuments.length }} recent</span>
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
    .page-header p { color: #607d8b; margin-bottom: 1rem; }
    .btn-primary { padding: 0.5rem 1rem; background: #1a237e; color: #fff; border: none; border-radius: 4px; cursor: pointer; }
    .btn-primary:disabled { opacity: 0.6; cursor: not-allowed; }
    .summary-message { padding: 0.5rem 1rem; border-radius: 4px; margin-bottom: 1rem; }
    .summary-message.success { background: #e8f5e9; color: #2e7d32; }
    .summary-message.error { background: #ffebee; color: #c62828; }
    .sections-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(200px, 1fr)); gap: 1rem; }
    .section-card { display: flex; flex-direction: column; align-items: center; padding: 1.5rem; border: 1px solid #e0e0e0; border-radius: 8px; text-decoration: none; transition: box-shadow 0.2s; }
    .section-card:hover { box-shadow: 0 2px 8px rgba(0,0,0,0.1); }
    .section-icon { font-size: 2rem; margin-bottom: 0.5rem; }
    .section-label { font-weight: 600; color: #1a237e; margin-bottom: 0.25rem; }
    .section-count { font-size: 0.8rem; color: #607d8b; }
    .warnings { margin-top: 1.5rem; padding: 1rem; background: #fff3e0; border-radius: 4px; }
    .warnings h4 { color: #e65100; margin-bottom: 0.5rem; }
    .warning-item { color: #bf360c; font-size: 0.875rem; margin-bottom: 0.25rem; }
  `]
})
export class ProductivityOverviewComponent implements OnInit {
  private api = inject(ApiService);

  readonly graphPanelData: GraphPanelData = {
    endpoints: ['GET /me/calendarView', 'GET /me/mailFolders/inbox/messages', 'GET /me/planner/tasks', 'GET /me/drive/recent'],
    scopes: ['Calendars.Read', 'Mail.Read', 'Tasks.Read', 'Files.Read.All'],
    descriptions: [
      'Aggregate weekly calendar events',
      'Analyze email volume and patterns',
      'Track task completion metrics',
      'Monitor document activity'
    ]
  };

  overview = signal<ProductivityOverview | null>(null);
  loading = signal(true);
  error = signal<string | null>(null);
  generating = signal(false);
  summaryMessage = signal<string | null>(null);
  summaryError = signal(false);

  ngOnInit(): void {
    this.loadOverview();
  }

  loadOverview(): void {
    this.loading.set(true);
    this.error.set(null);
    this.api.get<ProductivityOverview>('/productivity-assistant/overview').subscribe({
      next: (data) => {
        this.overview.set(data);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Failed to load productivity overview.');
        this.loading.set(false);
      }
    });
  }

  generateSummary(): void {
    this.generating.set(true);
    this.summaryMessage.set(null);
    this.summaryError.set(false);
    this.api.post<unknown>('/productivity-assistant/weekly-summary', {}).subscribe({
      next: () => {
        this.summaryMessage.set('Weekly summary generated successfully.');
        this.summaryError.set(false);
        this.generating.set(false);
        this.loadOverview();
      },
      error: () => {
        this.summaryMessage.set('Failed to generate weekly summary.');
        this.summaryError.set(true);
        this.generating.set(false);
      }
    });
  }
}
