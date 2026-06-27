import { Component, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../core/services/api.service';

interface CreateLegalMatterRequest {
  clientName: string;
  matterType: string;
  assignedSolicitor: string;
}

@Component({
  selector: 'app-legal-matters-create',
  standalone: true,
  imports: [FormsModule],
  template: `
    <div class="module-page">
      <div class="page-header">
        <h2>Create Legal Matter</h2>
        <p>Open a new legal matter and configure automated workspace creation.</p>
      </div>

      <form class="form-card" (ngSubmit)="onSubmit()" #form="ngForm">
        <div class="form-group">
          <label for="clientName">Client Name</label>
          <input
            id="clientName"
            type="text"
            [(ngModel)]="formData.clientName"
            name="clientName"
            required
            maxlength="200"
            placeholder="e.g. Oakfield Estates Ltd" />
        </div>

        <div class="form-group">
          <label for="matterType">Matter Type</label>
          <select id="matterType" [(ngModel)]="formData.matterType" name="matterType" required>
            <option value="" disabled>Select matter type</option>
            <option value="Commercial Property">Commercial Property</option>
            <option value="Residential Conveyancing">Residential Conveyancing</option>
            <option value="Corporate M&A">Corporate M&amp;A</option>
            <option value="Dispute Resolution">Dispute Resolution</option>
            <option value="Employment Law">Employment Law</option>
            <option value="Intellectual Property">Intellectual Property</option>
          </select>
        </div>

        <div class="form-group">
          <label for="assignedSolicitor">Assigned Solicitor</label>
          <input
            id="assignedSolicitor"
            type="text"
            [(ngModel)]="formData.assignedSolicitor"
            name="assignedSolicitor"
            required
            placeholder="e.g. Sarah Thompson" />
        </div>

        @if (errorMessage()) {
          <div class="error-banner">{{ errorMessage() }}</div>
        }

        <div class="form-actions">
          <button type="submit" class="btn-primary" [disabled]="submitting()">
            {{ submitting() ? 'Creating...' : 'Create Legal Matter' }}
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
export class LegalMattersCreateComponent {
  private api = inject(ApiService);
  private router = inject(Router);

  formData: CreateLegalMatterRequest = {
    clientName: '',
    matterType: '',
    assignedSolicitor: ''
  };

  submitting = signal(false);
  errorMessage = signal<string | null>(null);

  onSubmit(): void {
    if (!this.formData.clientName || !this.formData.matterType || !this.formData.assignedSolicitor) {
      this.errorMessage.set('Please fill in all required fields.');
      return;
    }

    this.submitting.set(true);
    this.errorMessage.set(null);

    this.api.post<{ id: string }>('/legal-matters', this.formData).subscribe({
      next: (result) => {
        this.submitting.set(false);
        this.router.navigate(['/legal-matters', result?.id ?? '']);
      },
      error: () => {
        this.errorMessage.set('Failed to create legal matter. Please try again.');
        this.submitting.set(false);
      }
    });
  }

  onCancel(): void {
    this.router.navigate(['/legal-matters']);
  }
}
