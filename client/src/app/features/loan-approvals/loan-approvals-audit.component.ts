import { Component, inject, signal, OnInit } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { DatePipe } from '@angular/common';
import { ApiService } from '../../core/services/api.service';
import { LoadingSkeletonComponent, ErrorStateComponent, EmptyStateComponent } from '../../shared/components';

interface AuditEntry {
  actionType: string;
  timestamp: string;
  status: string;
}

@Component({
  selector: 'app-loan-approvals-audit',
  standalone: true,
  imports: [RouterLink, DatePipe, LoadingSkeletonComponent, ErrorStateComponent, EmptyStateComponent],
  template: `
    <div class="module-page">
      <div class="page-header">
        <a [routerLink]="['/loan-approvals', loanId]" class="back-link">← Back to Loan</a>
        <h2>Audit Trail</h2>
        <p>Chronological log of all communication actions taken for this loan approval.</p>
      </div>

      @if (loading()) {
        <app-loading-skeleton [lines]="5" />
      } @else if (error()) {
        <app-error-state [errorType]="'server'" [message]="error()!" (retry)="loadAudit()" />
      } @else if (entries().length === 0) {
        <app-empty-state
          title="No Audit Entries"
          description="Communication actions (email sent, team notified, follow-up scheduled) will be logged here."
          actionLabel="Back to Loan"
          [actionRoute]="'/loan-approvals/' + loanId" />
      } @else {
        <div class="audit-list">
          @for (entry of entries(); track $index) {
            <div class="audit-entry">
              <div class="entry-icon">
                @switch (entry.actionType) {
                  @case ('PackGenerated') { 📦 }
                  @case ('CustomerEmailSent') { ✉️ }
                  @case ('TeamNotified') { 💬 }
                  @case ('FollowUpScheduled') { 📅 }
                  @default { 📋 }
                }
              </div>
              <div class="entry-content">
                <span class="action-type">{{ formatActionType(entry.actionType) }}</span>
                <span class="timestamp">{{ entry.timestamp | date:'medium' }}</span>
              </div>
              <span class="entry-status" [class]="'status-' + entry.status.toLowerCase()">
                {{ entry.status }}
              </span>
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
    .audit-list { background: #fff; border: 1px solid #e0e0e0; border-radius: 8px; overflow: hidden; }
    .audit-entry {
      display: flex; align-items: center; gap: 1rem; padding: 1rem 1.25rem;
      border-bottom: 1px solid #f5f5f5;
    }
    .audit-entry:last-child { border-bottom: none; }
    .entry-icon { font-size: 1.3rem; width: 2rem; text-align: center; }
    .entry-content { flex: 1; display: flex; flex-direction: column; gap: 0.2rem; }
    .action-type { font-weight: 500; color: #212121; font-size: 0.95rem; }
    .timestamp { font-size: 0.8rem; color: #9e9e9e; }
    .entry-status { font-size: 0.75rem; padding: 0.2rem 0.5rem; border-radius: 4px; font-weight: 500; }
    .status-completed { background: #e8f5e9; color: #2e7d32; }
    .status-sent { background: #e8f5e9; color: #2e7d32; }
    .status-success { background: #e8f5e9; color: #2e7d32; }
    .status-failed { background: #ffebee; color: #c62828; }
    .status-pending { background: #fff3e0; color: #ef6c00; }
  `]
})
export class LoanApprovalsAuditComponent implements OnInit {
  private api = inject(ApiService);
  private route = inject(ActivatedRoute);

  loanId = '';
  entries = signal<AuditEntry[]>([]);
  loading = signal(true);
  error = signal<string | null>(null);

  ngOnInit(): void {
    this.loanId = this.route.snapshot.paramMap.get('id') ?? '';
    this.loadAudit();
  }

  loadAudit(): void {
    if (!this.loanId) return;

    this.loading.set(true);
    this.error.set(null);
    this.api.get<AuditEntry[]>(`/loan-approvals/${this.loanId}/audit`).subscribe({
      next: (data) => {
        this.entries.set(data ?? []);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Failed to load audit trail.');
        this.loading.set(false);
      }
    });
  }

  formatActionType(actionType: string): string {
    return actionType.replace(/([A-Z])/g, ' $1').trim();
  }
}
