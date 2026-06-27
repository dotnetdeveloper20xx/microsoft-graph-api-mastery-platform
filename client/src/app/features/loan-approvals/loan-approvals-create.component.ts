import { Component, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../core/services/api.service';

interface CreateLoanApprovalRequest {
  customerName: string;
  amount: number | null;
  propertyReference: string;
  status: string;
}

@Component({
  selector: 'app-loan-approvals-create',
  standalone: true,
  imports: [FormsModule],
  template: `
    <div class="module-page">
      <div class="page-header">
        <h2>Record Loan Approval</h2>
        <p>Record a new loan approval to trigger automated communication workflows.</p>
      </div>

      <form class="form-card" (ngSubmit)="onSubmit()" #form="ngForm">
        <div class="form-group">
          <label for="customerName">Customer Name</label>
          <input
            id="customerName"
            type="text"
            [(ngModel)]="formData.customerName"
            name="customerName"
            required
            maxlength="200"
            placeholder="e.g. Greenway Property Holdings" />
        </div>

        <div class="form-group">
          <label for="amount">Loan Amount (£)</label>
          <input
            id="amount"
            type="number"
            [(ngModel)]="formData.amount"
            name="amount"
            required
            min="0.01"
            max="999999999.99"
            step="0.01"
            placeholder="e.g. 450000.00" />
        </div>

        <div class="form-group">
          <label for="propertyReference">Property Reference</label>
          <input
            id="propertyReference"
            type="text"
            [(ngModel)]="formData.propertyReference"
            name="propertyReference"
            required
            maxlength="100"
            placeholder="e.g. PR-2024-0847" />
        </div>

        <div class="form-group">
          <label for="status">Approval Status</label>
          <select id="status" [(ngModel)]="formData.status" name="status" required>
            <option value="" disabled>Select status</option>
            <option value="Pending">Pending</option>
            <option value="Approved">Approved</option>
            <option value="Conditionally Approved">Conditionally Approved</option>
            <option value="Rejected">Rejected</option>
          </select>
        </div>

        @if (errorMessage()) {
          <div class="error-banner">{{ errorMessage() }}</div>
        }

        <div class="form-actions">
          <button type="submit" class="btn-primary" [disabled]="submitting()">
            {{ submitting() ? 'Recording...' : 'Record Loan Approval' }}
          </button>
          <button type="button" class="btn-secondary" (click)="onCancel()">Cancel</button>
        </div>
      </form>
    </div>
  `,
  styles: [`
    .module-page { padding: 1.5rem; }
    .page-header h2 { color: #1a237e; margin-bottom: 0.5rem; }
    .page-header p { color: #607d8b; margin-bottom: 1.5rem; }
    .form-card {
      max-width: 560px; padding: 1.5rem; background: #fff;
      border: 1px solid #e0e0e0; border-radius: 8px;
    }
    .form-group { margin-bottom: 1.25rem; }
    .form-group label { display: block; font-weight: 500; color: #424242; margin-bottom: 0.4rem; }
    .form-group input, .form-group select {
      width: 100%; padding: 0.6rem 0.8rem; border: 1px solid #bdbdbd;
      border-radius: 6px; font-size: 0.95rem;
    }
    .form-group input:focus, .form-group select:focus { outline: none; border-color: #1a237e; }
    .error-banner { padding: 0.75rem; background: #ffebee; color: #c62828; border-radius: 6px; margin-bottom: 1rem; }
    .form-actions { display: flex; gap: 0.75rem; }
    .btn-primary {
      padding: 0.6rem 1.2rem; background: #1a237e; color: #fff;
      border: none; border-radius: 6px; font-weight: 500; cursor: pointer;
    }
    .btn-primary:hover:not(:disabled) { background: #283593; }
    .btn-primary:disabled { opacity: 0.6; cursor: not-allowed; }
    .btn-secondary {
      padding: 0.6rem 1.2rem; background: #f5f5f5; color: #424242;
      border: 1px solid #bdbdbd; border-radius: 6px; cursor: pointer;
    }
    .btn-secondary:hover { background: #eeeeee; }
  `]
})
export class LoanApprovalsCreateComponent {
  private api = inject(ApiService);
  private router = inject(Router);

  formData: CreateLoanApprovalRequest = {
    customerName: '',
    amount: null,
    propertyReference: '',
    status: ''
  };

  submitting = signal(false);
  errorMessage = signal<string | null>(null);

  onSubmit(): void {
    if (!this.formData.customerName || !this.formData.amount || !this.formData.propertyReference || !this.formData.status) {
      this.errorMessage.set('Please fill in all required fields.');
      return;
    }

    this.submitting.set(true);
    this.errorMessage.set(null);

    this.api.post<{ id: string }>('/loan-approvals', this.formData).subscribe({
      next: (result) => {
        this.submitting.set(false);
        this.router.navigate(['/loan-approvals', result?.id ?? '']);
      },
      error: () => {
        this.errorMessage.set('Failed to record loan approval. Please try again.');
        this.submitting.set(false);
      }
    });
  }

  onCancel(): void {
    this.router.navigate(['/loan-approvals']);
  }
}
