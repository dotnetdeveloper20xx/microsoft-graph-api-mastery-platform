import { Component, inject, signal, OnInit } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { DecimalPipe } from '@angular/common';
import { ApiService } from '../../core/services/api.service';
import { LoadingSkeletonComponent, ErrorStateComponent } from '../../shared/components';

interface LoanApprovalDetail {
  id: string;
  customerName: string;
  amount: number;
  propertyReference: string;
  status: string;
  packGenerated: boolean;
}

@Component({
  selector: 'app-loan-approvals-detail',
  standalone: true,
  imports: [RouterLink, DecimalPipe, LoadingSkeletonComponent, ErrorStateComponent],
  template: `
    <div class="module-page">
      @if (loading()) {
        <app-loading-skeleton [lines]="6" />
      } @else if (error()) {
        <app-error-state [errorType]="'server'" [message]="error()!" (retry)="loadLoan()" />
      } @else if (loan()) {
        <div class="page-header">
          <a routerLink="/loan-approvals" class="back-link">← Back to Loan Approvals</a>
          <h2>{{ loan()!.customerName }}</h2>
          <div class="meta-row">
            <span class="amount">£{{ loan()!.amount | number:'1.2-2' }}</span>
            <span class="property">{{ loan()!.propertyReference }}</span>
            <span class="status-badge" [class]="'status-' + loan()!.status.toLowerCase()">
              {{ loan()!.status }}
            </span>
          </div>
        </div>

        <div class="actions-section">
          <h3>Communication Actions</h3>
          <div class="actions-grid">
            <div class="action-card">
              <h4>Generate Communication Pack</h4>
              <p>Create customer email content, internal notification, and document checklist.</p>
              @if (loan()!.packGenerated) {
                <span class="status-done">✓ Pack Generated</span>
              } @else {
                <button class="btn-action" (click)="generatePack()" [disabled]="actionLoading()">
                  {{ actionLoading() ? 'Generating...' : 'Generate Pack' }}
                </button>
              }
            </div>

            <div class="action-card">
              <h4>Send Customer Email</h4>
              <p>Send the approval notification email to the customer.</p>
              <button
                class="btn-action"
                (click)="sendCustomerEmail()"
                [disabled]="actionLoading() || !loan()!.packGenerated">
                Send Customer Email
              </button>
              @if (!loan()!.packGenerated) {
                <p class="prereq-note">Requires communication pack to be generated first.</p>
              }
            </div>

            <div class="action-card">
              <h4>Notify Team</h4>
              <p>Send a Teams notification to the internal finance channel.</p>
              <button class="btn-action" (click)="notifyTeam()" [disabled]="actionLoading()">
                Notify Team
              </button>
            </div>

            <div class="action-card">
              <h4>Schedule Follow-Up</h4>
              <p>Create a calendar event for the customer follow-up meeting.</p>
              <button class="btn-action" (click)="scheduleFollowUp()" [disabled]="actionLoading()">
                Schedule Follow-Up
              </button>
            </div>

            <div class="action-card">
              <h4>Audit Trail</h4>
              <p>View a chronological log of all communication actions taken.</p>
              <a [routerLink]="['audit']" class="btn-link">View Audit Trail →</a>
            </div>
          </div>
        </div>

        @if (actionMessage()) {
          <div class="action-message" [class.success]="actionSuccess()" [class.error]="!actionSuccess()">
            {{ actionMessage() }}
          </div>
        }
      }
    </div>
  `,
  styles: [`
    .module-page { padding: 1.5rem; }
    .back-link { color: #5c6bc0; text-decoration: none; font-size: 0.9rem; }
    .back-link:hover { text-decoration: underline; }
    .page-header { margin-bottom: 2rem; }
    .page-header h2 { color: #1a237e; margin: 0.5rem 0; }
    .meta-row { display: flex; gap: 1rem; flex-wrap: wrap; align-items: center; }
    .amount { color: #2e7d32; font-weight: 600; font-size: 1.1rem; }
    .property { color: #757575; font-size: 0.9rem; }
    .status-badge { font-size: 0.75rem; padding: 0.2rem 0.5rem; border-radius: 4px; font-weight: 500; }
    .status-approved { background: #e8f5e9; color: #2e7d32; }
    .status-pending { background: #fff3e0; color: #ef6c00; }
    .status-rejected { background: #ffebee; color: #c62828; }
    .actions-section h3 { color: #424242; margin-bottom: 1rem; }
    .actions-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(280px, 1fr)); gap: 1.25rem; }
    .action-card {
      padding: 1.25rem; border: 1px solid #e0e0e0; border-radius: 8px; background: #fff;
    }
    .action-card h4 { margin: 0 0 0.5rem; color: #1a237e; }
    .action-card p { color: #757575; font-size: 0.9rem; margin-bottom: 0.75rem; }
    .btn-action {
      padding: 0.5rem 1rem; background: #1a237e; color: #fff;
      border: none; border-radius: 6px; cursor: pointer; font-size: 0.85rem;
    }
    .btn-action:hover:not(:disabled) { background: #283593; }
    .btn-action:disabled { opacity: 0.6; cursor: not-allowed; }
    .btn-link { color: #1a237e; text-decoration: none; font-weight: 500; font-size: 0.9rem; }
    .btn-link:hover { text-decoration: underline; }
    .status-done { color: #2e7d32; font-weight: 500; }
    .prereq-note { font-size: 0.8rem; color: #ef6c00; margin-top: 0.4rem; }
    .action-message {
      margin-top: 1.5rem; padding: 0.75rem 1rem; border-radius: 6px; font-size: 0.9rem;
    }
    .action-message.success { background: #e8f5e9; color: #2e7d32; }
    .action-message.error { background: #ffebee; color: #c62828; }
  `]
})
export class LoanApprovalsDetailComponent implements OnInit {
  private api = inject(ApiService);
  private route = inject(ActivatedRoute);

  loan = signal<LoanApprovalDetail | null>(null);
  loading = signal(true);
  error = signal<string | null>(null);
  actionLoading = signal(false);
  actionMessage = signal<string | null>(null);
  actionSuccess = signal(true);

  ngOnInit(): void {
    this.loadLoan();
  }

  loadLoan(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) return;

    this.loading.set(true);
    this.error.set(null);
    this.api.get<LoanApprovalDetail>(`/loan-approvals/${id}`).subscribe({
      next: (data) => {
        this.loan.set(data);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Failed to load loan approval details.');
        this.loading.set(false);
      }
    });
  }

  generatePack(): void {
    const id = this.loan()?.id;
    if (!id) return;

    this.actionLoading.set(true);
    this.actionMessage.set(null);
    this.api.post<unknown>(`/loan-approvals/${id}/generate-pack`, {}).subscribe({
      next: () => {
        this.actionSuccess.set(true);
        this.actionMessage.set('Communication pack generated successfully.');
        this.actionLoading.set(false);
        this.loadLoan();
      },
      error: () => {
        this.actionSuccess.set(false);
        this.actionMessage.set('Failed to generate communication pack. Ensure the loan status is "Approved".');
        this.actionLoading.set(false);
      }
    });
  }

  sendCustomerEmail(): void {
    const id = this.loan()?.id;
    if (!id) return;

    this.actionLoading.set(true);
    this.actionMessage.set(null);
    this.api.post<unknown>(`/loan-approvals/${id}/send-customer-email`, {}).subscribe({
      next: () => {
        this.actionSuccess.set(true);
        this.actionMessage.set('Customer email sent successfully.');
        this.actionLoading.set(false);
      },
      error: () => {
        this.actionSuccess.set(false);
        this.actionMessage.set('Failed to send customer email.');
        this.actionLoading.set(false);
      }
    });
  }

  notifyTeam(): void {
    const id = this.loan()?.id;
    if (!id) return;

    this.actionLoading.set(true);
    this.actionMessage.set(null);
    this.api.post<unknown>(`/loan-approvals/${id}/notify-team`, {}).subscribe({
      next: () => {
        this.actionSuccess.set(true);
        this.actionMessage.set('Team notification sent to the finance channel.');
        this.actionLoading.set(false);
      },
      error: () => {
        this.actionSuccess.set(false);
        this.actionMessage.set('Failed to notify team.');
        this.actionLoading.set(false);
      }
    });
  }

  scheduleFollowUp(): void {
    const id = this.loan()?.id;
    if (!id) return;

    this.actionLoading.set(true);
    this.actionMessage.set(null);
    this.api.post<unknown>(`/loan-approvals/${id}/schedule-follow-up`, {}).subscribe({
      next: () => {
        this.actionSuccess.set(true);
        this.actionMessage.set('Follow-up meeting scheduled.');
        this.actionLoading.set(false);
      },
      error: () => {
        this.actionSuccess.set(false);
        this.actionMessage.set('Failed to schedule follow-up meeting.');
        this.actionLoading.set(false);
      }
    });
  }
}
