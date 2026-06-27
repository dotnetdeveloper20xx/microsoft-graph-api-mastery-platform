import { Component, inject, signal, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ApiService } from '../../core/services/api.service';
import { LoadingSkeletonComponent, ErrorStateComponent } from '../../shared/components';

interface EmailVolume {
  totalSent: number;
  totalReceived: number;
  unreadCount: number;
  topSenders: { senderName: string; messageCount: number }[];
}

@Component({
  selector: 'app-productivity-emails',
  standalone: true,
  imports: [RouterLink, LoadingSkeletonComponent, ErrorStateComponent],
  template: `
    <div class="module-page">
      <div class="page-header">
        <a routerLink="/productivity-assistant" class="back-link">← Back to Overview</a>
        <h2>Email Volume</h2>
        <p>Email activity for the past 7 days.</p>
      </div>

      @if (loading()) {
        <app-loading-skeleton [lines]="5" />
      } @else if (error()) {
        <app-error-state errorType="server" [message]="error()!" (retry)="loadEmails()" />
      } @else if (emailVolume()) {
        <div class="stats-grid">
          <div class="stat-card">
            <span class="stat-value">{{ emailVolume()!.totalSent }}</span>
            <span class="stat-label">Sent</span>
          </div>
          <div class="stat-card">
            <span class="stat-value">{{ emailVolume()!.totalReceived }}</span>
            <span class="stat-label">Received</span>
          </div>
          <div class="stat-card">
            <span class="stat-value">{{ emailVolume()!.unreadCount }}</span>
            <span class="stat-label">Unread</span>
          </div>
        </div>

        @if (emailVolume()!.topSenders.length > 0) {
          <div class="top-senders">
            <h3>Top Senders</h3>
            <div class="senders-list">
              @for (sender of emailVolume()!.topSenders; track sender.senderName) {
                <div class="sender-row">
                  <span class="sender-name">{{ sender.senderName }}</span>
                  <span class="sender-count">{{ sender.messageCount }} messages</span>
                </div>
              }
            </div>
          </div>
        }
      }
    </div>
  `,
  styles: [`
    .module-page { padding: 1.5rem; }
    .page-header { margin-bottom: 1.5rem; }
    .back-link { color: #1a237e; text-decoration: none; font-size: 0.875rem; }
    .page-header h2 { color: #1a237e; margin-top: 0.5rem; }
    .page-header p { color: #607d8b; }
    .stats-grid { display: grid; grid-template-columns: repeat(3, 1fr); gap: 1rem; margin-bottom: 1.5rem; }
    .stat-card { text-align: center; padding: 1.5rem; border: 1px solid #e0e0e0; border-radius: 8px; }
    .stat-value { display: block; font-size: 2rem; font-weight: 700; color: #1a237e; }
    .stat-label { display: block; font-size: 0.8rem; color: #607d8b; margin-top: 0.25rem; }
    .top-senders h3 { color: #37474f; margin-bottom: 0.75rem; }
    .senders-list { display: flex; flex-direction: column; gap: 0.5rem; }
    .sender-row { display: flex; justify-content: space-between; padding: 0.5rem 0.75rem; border: 1px solid #e0e0e0; border-radius: 4px; }
    .sender-name { color: #263238; font-weight: 500; }
    .sender-count { color: #607d8b; font-size: 0.875rem; }
  `]
})
export class ProductivityEmailsComponent implements OnInit {
  private api = inject(ApiService);

  emailVolume = signal<EmailVolume | null>(null);
  loading = signal(true);
  error = signal<string | null>(null);

  ngOnInit(): void {
    this.loadEmails();
  }

  loadEmails(): void {
    this.loading.set(true);
    this.error.set(null);
    this.api.get<EmailVolume>('/productivity-assistant/emails').subscribe({
      next: (data) => {
        this.emailVolume.set(data);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Failed to load email volume.');
        this.loading.set(false);
      }
    });
  }
}
