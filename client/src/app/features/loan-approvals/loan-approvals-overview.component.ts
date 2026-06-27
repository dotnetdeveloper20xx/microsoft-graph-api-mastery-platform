import { Component, inject, signal, OnInit } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { ApiService } from '../../core/services/api.service';
import { LoadingSkeletonComponent, ErrorStateComponent, EmptyStateComponent } from '../../shared/components';
import { GraphExplanationPanelComponent, GraphPanelData } from '../../shared/components/graph-explanation-panel/graph-explanation-panel.component';

export interface LoanApprovalSummary {
  id: string;
  customerName: string;
  amount: number;
  propertyReference: string;
  status: string;
  packGenerated: boolean;
}

@Component({
  selector: 'app-loan-approvals-overview',
  standalone: true,
  imports: [RouterLink, DecimalPipe, LoadingSkeletonComponent, ErrorStateComponent, EmptyStateComponent, GraphExplanationPanelComponent],
  template: `
    <div class="module-page">
      <div class="page-header">
        <h2>Loan Approval Communication Hub</h2>
        <p>Automate communication workflows for loan approvals — customer emails, team notifications, and follow-up scheduling.</p>
        <a routerLink="create" class="btn-primary">+ New Loan Approval</a>
      </div>

      @if (loading()) {
        <app-loading-skeleton [lines]="5" />
      } @else if (error()) {
        <app-error-state [errorType]="'server'" [message]="error()!" (retry)="loadLoans()" />
      } @else if (loans().length === 0) {
        <app-empty-state
          title="No Loan Approvals"
          description="Loan approvals with automated communication workflows will appear here once recorded."
          actionLabel="Create Loan Approval"
          actionRoute="create" />
      } @else {
        <div class="loans-grid">
          @for (loan of loans(); track loan.id) {
            <div class="loan-card">
              <div class="loan-header">
                <span class="status-badge" [class]="'status-' + loan.status.toLowerCase()">
                  {{ loan.status }}
                </span>
                @if (loan.packGenerated) {
                  <span class="badge-pack">Pack Ready</span>
                }
              </div>
              <h3>{{ loan.customerName }}</h3>
              <p class="amount">£{{ loan.amount | number:'1.2-2' }}</p>
              <p class="property">{{ loan.propertyReference }}</p>
              <a [routerLink]="[loan.id]" class="btn-link">View Details →</a>
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
    .loans-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(320px, 1fr)); gap: 1.5rem; }
    .loan-card {
      padding: 1.25rem; border: 1px solid #e0e0e0; border-radius: 8px;
      background: #fff; transition: box-shadow 0.2s;
    }
    .loan-card:hover { box-shadow: 0 2px 8px rgba(0,0,0,0.1); }
    .loan-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 0.75rem; }
    .status-badge {
      font-size: 0.75rem; padding: 0.2rem 0.5rem; border-radius: 4px; font-weight: 500;
    }
    .status-approved { background: #e8f5e9; color: #2e7d32; }
    .status-pending { background: #fff3e0; color: #ef6c00; }
    .status-rejected { background: #ffebee; color: #c62828; }
    .badge-pack { font-size: 0.75rem; padding: 0.2rem 0.5rem; border-radius: 4px; background: #e3f2fd; color: #1565c0; }
    .loan-card h3 { margin: 0 0 0.5rem; color: #212121; font-size: 1.1rem; }
    .amount { color: #2e7d32; font-weight: 600; font-size: 1.1rem; margin-bottom: 0.25rem; }
    .property { color: #757575; font-size: 0.85rem; margin-bottom: 0.75rem; }
    .btn-link { color: #1a237e; text-decoration: none; font-weight: 500; font-size: 0.9rem; }
    .btn-link:hover { text-decoration: underline; }
  `]
})
export class LoanApprovalsOverviewComponent implements OnInit {
  private api = inject(ApiService);

  readonly graphPanelData: GraphPanelData = {
    endpoints: ['POST /me/sendMail', 'POST /teams/{id}/channels/{id}/messages', 'POST /me/events'],
    scopes: ['Mail.Send', 'ChannelMessage.Send', 'Calendars.ReadWrite'],
    descriptions: [
      'Send approval notifications to customers',
      'Post internal notifications to Teams channels',
      'Schedule follow-up calendar events'
    ]
  };

  loans = signal<LoanApprovalSummary[]>([]);
  loading = signal(true);
  error = signal<string | null>(null);

  ngOnInit(): void {
    this.loadLoans();
  }

  loadLoans(): void {
    this.loading.set(true);
    this.error.set(null);
    this.api.get<LoanApprovalSummary[]>('/loan-approvals/overview').subscribe({
      next: (data) => {
        this.loans.set(data ?? []);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Failed to load loan approvals.');
        this.loading.set(false);
      }
    });
  }
}
