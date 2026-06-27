import { Component, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { ApiService } from '../../core/services/api.service';
import { LoadingSkeletonComponent, ErrorStateComponent } from '../../shared/components';

interface EmployeeDetail {
  id: string;
  name: string;
  role: string;
  department: string;
  managerName: string;
  email: string;
}

interface OnboardingStatus {
  profileCreated: boolean;
  groupsAssigned: boolean;
  welcomeEmailSent: boolean;
  inductionScheduled: boolean;
}

@Component({
  selector: 'app-onboarding-detail',
  standalone: true,
  imports: [RouterLink, LoadingSkeletonComponent, ErrorStateComponent],
  template: `
    <div class="onboarding-detail">
      <a class="onboarding-detail__back" routerLink="/onboarding">&larr; Back to Overview</a>

      @if (loading()) {
        <app-loading-skeleton [lines]="6" />
      } @else if (error()) {
        <app-error-state [errorType]="'server'" [message]="error()!" (retry)="loadEmployee()" />
      } @else if (employee()) {
        <div class="onboarding-detail__content">
          <div class="employee-info">
            <h2 class="employee-info__name">{{ employee()!.name }}</h2>
            <div class="employee-info__details">
              <div class="employee-info__row">
                <span class="employee-info__label">Role</span>
                <span class="employee-info__value">{{ employee()!.role }}</span>
              </div>
              <div class="employee-info__row">
                <span class="employee-info__label">Department</span>
                <span class="employee-info__value">{{ employee()!.department }}</span>
              </div>
              <div class="employee-info__row">
                <span class="employee-info__label">Manager</span>
                <span class="employee-info__value">{{ employee()!.managerName }}</span>
              </div>
              <div class="employee-info__row">
                <span class="employee-info__label">Email</span>
                <span class="employee-info__value">{{ employee()!.email }}</span>
              </div>
            </div>
          </div>

          <div class="onboarding-status">
            <h3 class="onboarding-status__title">Onboarding Status</h3>
            <div class="onboarding-status__grid">
              <div class="status-badge" [class.status-badge--done]="status()?.profileCreated">
                <span class="status-badge__icon">{{ status()?.profileCreated ? '✓' : '○' }}</span>
                <span class="status-badge__label">Profile Created</span>
              </div>
              <div class="status-badge" [class.status-badge--done]="status()?.groupsAssigned">
                <span class="status-badge__icon">{{ status()?.groupsAssigned ? '✓' : '○' }}</span>
                <span class="status-badge__label">Groups Assigned</span>
              </div>
              <div class="status-badge" [class.status-badge--done]="status()?.welcomeEmailSent">
                <span class="status-badge__icon">{{ status()?.welcomeEmailSent ? '✓' : '○' }}</span>
                <span class="status-badge__label">Welcome Email Sent</span>
              </div>
              <div class="status-badge" [class.status-badge--done]="status()?.inductionScheduled">
                <span class="status-badge__icon">{{ status()?.inductionScheduled ? '✓' : '○' }}</span>
                <span class="status-badge__label">Induction Scheduled</span>
              </div>
            </div>
          </div>

          <div class="onboarding-actions">
            <h3 class="onboarding-actions__title">Actions</h3>
            <div class="onboarding-actions__buttons">
              <button
                class="action-btn"
                [disabled]="actionInProgress()"
                (click)="assignGroups()"
              >
                {{ actionInProgress() === 'assign-groups' ? 'Assigning...' : 'Assign Groups' }}
              </button>
              <button
                class="action-btn"
                [disabled]="actionInProgress()"
                (click)="sendWelcomeEmail()"
              >
                {{ actionInProgress() === 'send-welcome-email' ? 'Sending...' : 'Send Welcome Email' }}
              </button>
              <button
                class="action-btn"
                [disabled]="actionInProgress()"
                (click)="scheduleInduction()"
              >
                {{ actionInProgress() === 'schedule-induction' ? 'Scheduling...' : 'Schedule Induction' }}
              </button>
            </div>
          </div>

          @if (actionMessage()) {
            <div class="onboarding-detail__message" [class.onboarding-detail__message--error]="actionIsError()">
              {{ actionMessage() }}
            </div>
          }
        </div>
      }
    </div>
  `,
  styles: [`
    .onboarding-detail {
      max-width: 800px;
    }

    .onboarding-detail__back {
      display: inline-block;
      color: #607d8b;
      text-decoration: none;
      font-size: 0.875rem;
      margin-bottom: 1.5rem;
    }

    .onboarding-detail__back:hover {
      color: #1a237e;
    }

    .onboarding-detail__content {
      display: flex;
      flex-direction: column;
      gap: 2rem;
    }

    .employee-info__name {
      font-size: 1.5rem;
      font-weight: 700;
      color: #1a237e;
      margin: 0 0 1rem;
    }

    .employee-info__details {
      display: flex;
      flex-direction: column;
      gap: 0.5rem;
    }

    .employee-info__row {
      display: flex;
      gap: 1rem;
    }

    .employee-info__label {
      font-size: 0.85rem;
      font-weight: 500;
      color: #90a4ae;
      min-width: 100px;
    }

    .employee-info__value {
      font-size: 0.9rem;
      color: #263238;
    }

    .onboarding-status__title,
    .onboarding-actions__title {
      font-size: 1.1rem;
      font-weight: 600;
      color: #1a237e;
      margin: 0 0 0.75rem;
    }

    .onboarding-status__grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(180px, 1fr));
      gap: 0.75rem;
    }

    .status-badge {
      display: flex;
      align-items: center;
      gap: 0.5rem;
      padding: 0.6rem 1rem;
      background: #f5f5f5;
      border: 1px solid #e0e0e0;
      border-radius: 6px;
    }

    .status-badge--done {
      background: #e8f5e9;
      border-color: #a5d6a7;
    }

    .status-badge__icon {
      font-size: 1rem;
    }

    .status-badge--done .status-badge__icon {
      color: #2e7d32;
    }

    .status-badge__label {
      font-size: 0.8rem;
      font-weight: 500;
      color: #455a64;
    }

    .onboarding-actions__buttons {
      display: flex;
      flex-wrap: wrap;
      gap: 0.75rem;
    }

    .action-btn {
      padding: 0.6rem 1.2rem;
      background: #1565c0;
      color: #ffffff;
      border: none;
      border-radius: 6px;
      font-size: 0.85rem;
      font-weight: 500;
      cursor: pointer;
      transition: background 0.2s;
    }

    .action-btn:hover:not(:disabled) {
      background: #0d47a1;
    }

    .action-btn:disabled {
      opacity: 0.6;
      cursor: not-allowed;
    }

    .onboarding-detail__message {
      padding: 0.75rem 1rem;
      background: #e8f5e9;
      color: #2e7d32;
      border-radius: 6px;
      font-size: 0.875rem;
    }

    .onboarding-detail__message--error {
      background: #ffebee;
      color: #c62828;
    }
  `]
})
export class OnboardingDetailComponent implements OnInit {
  private api = inject(ApiService);
  private route = inject(ActivatedRoute);

  readonly employee = signal<EmployeeDetail | null>(null);
  readonly status = signal<OnboardingStatus | null>(null);
  readonly loading = signal(true);
  readonly error = signal<string | null>(null);
  readonly actionInProgress = signal<string | null>(null);
  readonly actionMessage = signal<string | null>(null);
  readonly actionIsError = signal(false);

  private employeeId = '';

  ngOnInit(): void {
    this.employeeId = this.route.snapshot.paramMap.get('id') ?? '';
    this.loadEmployee();
  }

  loadEmployee(): void {
    this.loading.set(true);
    this.error.set(null);

    this.api.get<EmployeeDetail>(`/onboarding/employees/${this.employeeId}`).subscribe({
      next: (data) => {
        this.employee.set(data);
        this.loading.set(false);
        this.loadStatus();
      },
      error: () => {
        this.error.set('Failed to load employee details.');
        this.loading.set(false);
      }
    });
  }

  assignGroups(): void {
    this.performAction('assign-groups', 'Groups assigned successfully.');
  }

  sendWelcomeEmail(): void {
    this.performAction('send-welcome-email', 'Welcome email sent successfully.');
  }

  scheduleInduction(): void {
    this.performAction('schedule-induction', 'Induction meeting scheduled successfully.');
  }

  private loadStatus(): void {
    this.api.get<OnboardingStatus>(`/onboarding/employees/${this.employeeId}/status`).subscribe({
      next: (data) => this.status.set(data),
      error: () => {} // status load failure is non-critical
    });
  }

  private performAction(action: string, successMsg: string): void {
    this.actionInProgress.set(action);
    this.actionMessage.set(null);
    this.actionIsError.set(false);

    this.api.post<unknown>(`/onboarding/employees/${this.employeeId}/${action}`, {}).subscribe({
      next: () => {
        this.actionMessage.set(successMsg);
        this.actionIsError.set(false);
        this.actionInProgress.set(null);
        this.loadStatus();
      },
      error: () => {
        this.actionMessage.set(`Failed to ${action.replace(/-/g, ' ')}. Please try again.`);
        this.actionIsError.set(true);
        this.actionInProgress.set(null);
      }
    });
  }
}
