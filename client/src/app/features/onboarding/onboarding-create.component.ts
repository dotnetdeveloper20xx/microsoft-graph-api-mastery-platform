import { Component, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../core/services/api.service';

interface CreateEmployeeRequest {
  name: string;
  role: string;
  department: string;
  managerName: string;
  email: string;
}

interface EmployeeResponse {
  id: string;
  name: string;
  role: string;
  department: string;
  managerName: string;
  email: string;
}

@Component({
  selector: 'app-onboarding-create',
  standalone: true,
  imports: [FormsModule, RouterLink],
  template: `
    <div class="onboarding-create">
      <h2 class="onboarding-create__title">Create New Employee</h2>
      <p class="onboarding-create__subtitle">Fill in the details to start the automated onboarding workflow.</p>

      <form class="onboarding-create__form" (ngSubmit)="onSubmit()" #form="ngForm">
        <div class="form-field">
          <label class="form-field__label" for="name">Full Name</label>
          <input
            class="form-field__input"
            id="name"
            name="name"
            [(ngModel)]="formData.name"
            required
            minlength="1"
            maxlength="100"
            placeholder="e.g. Sarah Khan"
          />
        </div>

        <div class="form-field">
          <label class="form-field__label" for="role">Role</label>
          <input
            class="form-field__input"
            id="role"
            name="role"
            [(ngModel)]="formData.role"
            required
            minlength="1"
            maxlength="100"
            placeholder="e.g. Senior Developer"
          />
        </div>

        <div class="form-field">
          <label class="form-field__label" for="department">Department</label>
          <input
            class="form-field__input"
            id="department"
            name="department"
            [(ngModel)]="formData.department"
            required
            minlength="1"
            maxlength="50"
            placeholder="e.g. Engineering"
          />
        </div>

        <div class="form-field">
          <label class="form-field__label" for="managerName">Manager Name</label>
          <input
            class="form-field__input"
            id="managerName"
            name="managerName"
            [(ngModel)]="formData.managerName"
            required
            minlength="1"
            maxlength="100"
            placeholder="e.g. Afzal Ahmed"
          />
        </div>

        <div class="form-field">
          <label class="form-field__label" for="email">Email Address</label>
          <input
            class="form-field__input"
            id="email"
            name="email"
            type="email"
            [(ngModel)]="formData.email"
            required
            placeholder="e.g. sarah.khan&#64;company.com"
          />
        </div>

        @if (errorMessage()) {
          <div class="onboarding-create__error">{{ errorMessage() }}</div>
        }

        <div class="onboarding-create__actions">
          <button
            class="onboarding-create__submit-btn"
            type="submit"
            [disabled]="submitting() || form.invalid"
          >
            {{ submitting() ? 'Creating...' : 'Create Employee' }}
          </button>
          <a class="onboarding-create__cancel-btn" routerLink="/onboarding">Cancel</a>
        </div>
      </form>
    </div>
  `,
  styles: [`
    .onboarding-create {
      max-width: 600px;
    }

    .onboarding-create__title {
      font-size: 1.5rem;
      font-weight: 700;
      color: #1a237e;
      margin: 0 0 0.5rem;
    }

    .onboarding-create__subtitle {
      color: #607d8b;
      margin: 0 0 2rem;
    }

    .onboarding-create__form {
      display: flex;
      flex-direction: column;
      gap: 1.25rem;
    }

    .form-field {
      display: flex;
      flex-direction: column;
      gap: 0.35rem;
    }

    .form-field__label {
      font-size: 0.875rem;
      font-weight: 500;
      color: #455a64;
    }

    .form-field__input {
      padding: 0.6rem 0.75rem;
      border: 1px solid #cfd8dc;
      border-radius: 6px;
      font-size: 0.9rem;
      color: #263238;
      transition: border-color 0.2s;
    }

    .form-field__input:focus {
      outline: none;
      border-color: #1a237e;
    }

    .onboarding-create__error {
      padding: 0.75rem 1rem;
      background: #ffebee;
      color: #c62828;
      border-radius: 6px;
      font-size: 0.875rem;
    }

    .onboarding-create__actions {
      display: flex;
      align-items: center;
      gap: 1rem;
      margin-top: 0.5rem;
    }

    .onboarding-create__submit-btn {
      padding: 0.6rem 1.5rem;
      background: #1a237e;
      color: #ffffff;
      border: none;
      border-radius: 6px;
      font-size: 0.875rem;
      font-weight: 500;
      cursor: pointer;
      transition: background 0.2s;
    }

    .onboarding-create__submit-btn:hover:not(:disabled) {
      background: #283593;
    }

    .onboarding-create__submit-btn:disabled {
      opacity: 0.6;
      cursor: not-allowed;
    }

    .onboarding-create__cancel-btn {
      color: #607d8b;
      text-decoration: none;
      font-size: 0.875rem;
    }

    .onboarding-create__cancel-btn:hover {
      color: #455a64;
    }
  `]
})
export class OnboardingCreateComponent {
  private api = inject(ApiService);
  private router = inject(Router);

  formData: CreateEmployeeRequest = {
    name: '',
    role: '',
    department: '',
    managerName: '',
    email: ''
  };

  readonly submitting = signal(false);
  readonly errorMessage = signal<string | null>(null);

  onSubmit(): void {
    this.submitting.set(true);
    this.errorMessage.set(null);

    this.api.post<EmployeeResponse>('/onboarding/employees', this.formData).subscribe({
      next: (result) => {
        this.submitting.set(false);
        if (result?.id) {
          this.router.navigate(['/onboarding', result.id]);
        }
      },
      error: () => {
        this.errorMessage.set('Failed to create employee. Please check the form and try again.');
        this.submitting.set(false);
      }
    });
  }
}
