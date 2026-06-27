import { Component, inject, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ApiService } from '../../core/services/api.service';
import { LoadingSkeletonComponent, ErrorStateComponent, EmptyStateComponent } from '../../shared/components';
import { GraphExplanationPanelComponent, GraphPanelData } from '../../shared/components/graph-explanation-panel/graph-explanation-panel.component';

interface EmployeeOverviewItem {
  id: string;
  name: string;
  role: string;
  department: string;
  email: string;
}

@Component({
  selector: 'app-onboarding-overview',
  standalone: true,
  imports: [RouterLink, LoadingSkeletonComponent, ErrorStateComponent, EmptyStateComponent, GraphExplanationPanelComponent],
  template: `
    <div class="onboarding-overview">
      <div class="onboarding-overview__header">
        <h2 class="onboarding-overview__title">Employee Onboarding Automation</h2>
        <p class="onboarding-overview__subtitle">
          Automate Microsoft 365 onboarding for new employees — group assignments, welcome emails, and induction scheduling.
        </p>
        <a class="onboarding-overview__create-btn" routerLink="create">+ New Employee</a>
      </div>

      @if (loading()) {
        <app-loading-skeleton [lines]="5" />
      } @else if (error()) {
        <app-error-state [errorType]="'server'" [message]="error()!" (retry)="loadEmployees()" />
      } @else if (employees().length === 0) {
        <app-empty-state
          title="No employees onboarded yet"
          description="Create a new employee to start their automated Microsoft 365 onboarding workflow."
          actionLabel="Create Employee"
          actionRoute="create"
        />
      } @else {
        <div class="onboarding-overview__list">
          @for (emp of employees(); track emp.id) {
            <a class="employee-card" [routerLink]="emp.id">
              <div class="employee-card__info">
                <span class="employee-card__name">{{ emp.name }}</span>
                <span class="employee-card__role">{{ emp.role }}</span>
              </div>
              <div class="employee-card__meta">
                <span class="employee-card__department">{{ emp.department }}</span>
                <span class="employee-card__email">{{ emp.email }}</span>
              </div>
            </a>
          }
        </div>
      }
      <app-graph-explanation-panel [panelData]="graphPanelData" />
    </div>
  `,
  styles: [`
    .onboarding-overview {
      max-width: 900px;
    }

    .onboarding-overview__header {
      margin-bottom: 2rem;
    }

    .onboarding-overview__title {
      font-size: 1.75rem;
      font-weight: 700;
      color: #1a237e;
      margin: 0 0 0.5rem;
    }

    .onboarding-overview__subtitle {
      color: #607d8b;
      margin: 0 0 1rem;
    }

    .onboarding-overview__create-btn {
      display: inline-block;
      padding: 0.6rem 1.2rem;
      background: #1a237e;
      color: #ffffff;
      border-radius: 6px;
      text-decoration: none;
      font-size: 0.875rem;
      font-weight: 500;
      transition: background 0.2s;
    }

    .onboarding-overview__create-btn:hover {
      background: #283593;
    }

    .onboarding-overview__list {
      display: flex;
      flex-direction: column;
      gap: 0.75rem;
    }

    .employee-card {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 1rem 1.25rem;
      background: #ffffff;
      border: 1px solid #e0e0e0;
      border-radius: 8px;
      text-decoration: none;
      color: inherit;
      transition: box-shadow 0.2s;
    }

    .employee-card:hover {
      box-shadow: 0 2px 8px rgba(0, 0, 0, 0.08);
    }

    .employee-card__info {
      display: flex;
      flex-direction: column;
      gap: 0.25rem;
    }

    .employee-card__name {
      font-weight: 600;
      color: #1a237e;
    }

    .employee-card__role {
      font-size: 0.85rem;
      color: #607d8b;
    }

    .employee-card__meta {
      display: flex;
      flex-direction: column;
      align-items: flex-end;
      gap: 0.25rem;
    }

    .employee-card__department {
      font-size: 0.8rem;
      font-weight: 500;
      color: #1565c0;
      background: #e3f2fd;
      padding: 0.15rem 0.5rem;
      border-radius: 4px;
    }

    .employee-card__email {
      font-size: 0.8rem;
      color: #90a4ae;
    }
  `]
})
export class OnboardingOverviewComponent implements OnInit {
  private api = inject(ApiService);

  readonly graphPanelData: GraphPanelData = {
    endpoints: ['GET /users', 'POST /groups/{id}/members', 'POST /me/sendMail', 'POST /me/events'],
    scopes: ['User.Read.All', 'Group.ReadWrite.All', 'Mail.Send', 'Calendars.ReadWrite'],
    descriptions: [
      'Read user profiles from directory',
      'Assign users to Microsoft 365 groups',
      'Send welcome emails via Exchange Online',
      'Create induction calendar events'
    ]
  };

  readonly employees = signal<EmployeeOverviewItem[]>([]);
  readonly loading = signal(true);
  readonly error = signal<string | null>(null);

  ngOnInit(): void {
    this.loadEmployees();
  }

  loadEmployees(): void {
    this.loading.set(true);
    this.error.set(null);

    this.api.get<EmployeeOverviewItem[]>('/onboarding/overview').subscribe({
      next: (data) => {
        this.employees.set(data ?? []);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Failed to load onboarding employees.');
        this.loading.set(false);
      }
    });
  }
}
