import { Component, inject, signal, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ApiService } from '../../core/services/api.service';
import { LoadingSkeletonComponent, ErrorStateComponent, EmptyStateComponent } from '../../shared/components';

interface EmailSummary {
  subject: string;
  from: string;
  receivedAt: string;
  priority: string;
  isRead: boolean;
}

@Component({
  selector: 'app-ceo-emails',
  standalone: true,
  imports: [RouterLink, LoadingSkeletonComponent, ErrorStateComponent, EmptyStateComponent],
  template: `
    <div class="module-page">
      <div class="page-header">
        <a routerLink="/ceo-command-centre" class="back-link">← Back to Dashboard</a>
        <h2>Email Summary</h2>
        <p>Emails received in the past 24 hours, grouped by priority.</p>
      </div>

      @if (loading()) {
        <app-loading-skeleton [lines]="5" />
      } @else if (error()) {
        <app-error-state errorType="server" [message]="error()!" (retry)="loadEmails()" />
      } @else if (emails().length === 0) {
        <app-empty-state
          title="No Recent Emails"
          description="No emails received in the past 24 hours."
          actionLabel="Back to Dashboard"
          actionRoute="/ceo-command-centre" />
      } @else {
        <div class="emails-list">
          @for (email of emails(); track $index) {
            <div class="email-card" [class.unread]="!email.isRead">
              <div class="email-priority" [class]="email.priority">{{ email.priority }}</div>
              <div class="email-content">
                <h4>{{ email.subject }}</h4>
                <p class="from">From: {{ email.from }}</p>
                <p class="time">{{ formatTime(email.receivedAt) }}</p>
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
    .emails-list { display: flex; flex-direction: column; gap: 0.5rem; }
    .email-card { display: flex; gap: 1rem; padding: 0.75rem 1rem; border: 1px solid #e0e0e0; border-radius: 8px; align-items: flex-start; }
    .email-card.unread { border-left: 3px solid #1a237e; background: #f5f7ff; }
    .email-priority { padding: 0.2rem 0.5rem; border-radius: 4px; font-size: 0.7rem; text-transform: uppercase; font-weight: 600; }
    .email-priority.high { background: #ffebee; color: #c62828; }
    .email-priority.normal { background: #e8f5e9; color: #2e7d32; }
    .email-priority.low { background: #eceff1; color: #607d8b; }
    .email-content h4 { margin: 0 0 0.25rem; color: #263238; font-size: 0.9rem; }
    .from { color: #455a64; font-size: 0.8rem; margin: 0; }
    .time { color: #9e9e9e; font-size: 0.75rem; margin: 0.25rem 0 0; }
  `]
})
export class CeoEmailsComponent implements OnInit {
  private api = inject(ApiService);

  emails = signal<EmailSummary[]>([]);
  loading = signal(true);
  error = signal<string | null>(null);

  ngOnInit(): void {
    this.loadEmails();
  }

  loadEmails(): void {
    this.loading.set(true);
    this.error.set(null);
    this.api.get<EmailSummary[]>('/ceo-command-centre/emails').subscribe({
      next: (data) => {
        this.emails.set(data ?? []);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Failed to load email summary.');
        this.loading.set(false);
      }
    });
  }

  formatTime(isoString: string): string {
    return new Date(isoString).toLocaleString([], { dateStyle: 'short', timeStyle: 'short' });
  }
}
