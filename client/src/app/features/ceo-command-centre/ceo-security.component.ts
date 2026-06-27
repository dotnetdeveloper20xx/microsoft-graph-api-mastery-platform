import { Component, inject, signal, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ApiService } from '../../core/services/api.service';
import { LoadingSkeletonComponent, ErrorStateComponent, EmptyStateComponent } from '../../shared/components';

interface SecuritySignal {
  title: string;
  severity: string;
  detectedAt: string;
  description: string;
}

@Component({
  selector: 'app-ceo-security',
  standalone: true,
  imports: [RouterLink, LoadingSkeletonComponent, ErrorStateComponent, EmptyStateComponent],
  template: `
    <div class="module-page">
      <div class="page-header">
        <a routerLink="/ceo-command-centre" class="back-link">← Back to Dashboard</a>
        <h2>Security Signals</h2>
        <p>Security alerts and sign-in anomalies from the past 24 hours.</p>
      </div>

      @if (loading()) {
        <app-loading-skeleton [lines]="5" />
      } @else if (error()) {
        <app-error-state errorType="server" [message]="error()!" (retry)="loadSignals()" />
      } @else if (signals().length === 0) {
        <app-empty-state
          title="No Security Signals"
          description="No security alerts or anomalies detected in the past 24 hours."
          actionLabel="Back to Dashboard"
          actionRoute="/ceo-command-centre" />
      } @else {
        <div class="signals-list">
          @for (signal of signals(); track $index) {
            <div class="signal-card" [class]="signal.severity.toLowerCase()">
              <div class="signal-header">
                <span class="severity-badge" [class]="signal.severity.toLowerCase()">{{ signal.severity }}</span>
                <span class="detected-at">{{ formatDate(signal.detectedAt) }}</span>
              </div>
              <h4>{{ signal.title }}</h4>
              <p>{{ signal.description }}</p>
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
    .signals-list { display: flex; flex-direction: column; gap: 0.75rem; }
    .signal-card { padding: 1rem; border: 1px solid #e0e0e0; border-radius: 8px; }
    .signal-card.high { border-left: 4px solid #c62828; }
    .signal-card.medium { border-left: 4px solid #ef6c00; }
    .signal-card.low { border-left: 4px solid #fbc02d; }
    .signal-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 0.5rem; }
    .severity-badge { padding: 0.2rem 0.5rem; border-radius: 4px; font-size: 0.7rem; text-transform: uppercase; font-weight: 600; }
    .severity-badge.high { background: #ffebee; color: #c62828; }
    .severity-badge.medium { background: #fff3e0; color: #ef6c00; }
    .severity-badge.low { background: #fffde7; color: #f9a825; }
    .detected-at { color: #9e9e9e; font-size: 0.75rem; }
    .signal-card h4 { margin: 0 0 0.25rem; color: #263238; }
    .signal-card p { color: #607d8b; font-size: 0.875rem; margin: 0; }
  `]
})
export class CeoSecurityComponent implements OnInit {
  private api = inject(ApiService);

  signals = signal<SecuritySignal[]>([]);
  loading = signal(true);
  error = signal<string | null>(null);

  ngOnInit(): void {
    this.loadSignals();
  }

  loadSignals(): void {
    this.loading.set(true);
    this.error.set(null);
    this.api.get<SecuritySignal[]>('/ceo-command-centre/security-signals').subscribe({
      next: (data) => {
        this.signals.set(data ?? []);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Failed to load security signals.');
        this.loading.set(false);
      }
    });
  }

  formatDate(isoString: string): string {
    return new Date(isoString).toLocaleString([], { dateStyle: 'short', timeStyle: 'short' });
  }
}
