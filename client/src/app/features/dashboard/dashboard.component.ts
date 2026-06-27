import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

interface ModuleCard {
  name: string;
  description: string;
  graphAreas: string[];
  route: string;
}

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [RouterLink],
  template: `
    <div class="dashboard">
      <h1 class="dashboard__title">GraphBridge Enterprise Suite</h1>
      <p class="dashboard__subtitle">
        A portfolio-grade Microsoft Graph API Mastery Platform comprising six distinct
        business application modules demonstrating enterprise-level integration with Microsoft 365.
      </p>

      <div class="dashboard__grid">
        @for (card of modules; track card.route) {
          <div class="module-card">
            <h2 class="module-card__name">{{ card.name }}</h2>
            <p class="module-card__description">{{ card.description }}</p>
            <div class="module-card__areas">
              @for (area of card.graphAreas; track area) {
                <span class="module-card__area-badge">{{ area }}</span>
              }
            </div>
            <a class="module-card__nav-btn" [routerLink]="card.route">
              Open Module
            </a>
          </div>
        }
      </div>
    </div>
  `,
  styles: [`
    .dashboard {
      max-width: 1200px;
    }

    .dashboard__title {
      font-size: 2rem;
      font-weight: 700;
      color: #1a237e;
      margin: 0 0 0.5rem;
    }

    .dashboard__subtitle {
      font-size: 1rem;
      color: #607d8b;
      line-height: 1.6;
      margin: 0 0 2rem;
    }

    .dashboard__grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(340px, 1fr));
      gap: 1.5rem;
    }

    .module-card {
      background: #ffffff;
      border: 1px solid #e0e0e0;
      border-radius: 8px;
      padding: 1.5rem;
      display: flex;
      flex-direction: column;
      gap: 0.75rem;
      transition: box-shadow 0.2s;
    }

    .module-card:hover {
      box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
    }

    .module-card__name {
      font-size: 1.25rem;
      font-weight: 600;
      color: #1a237e;
      margin: 0;
    }

    .module-card__description {
      font-size: 0.9rem;
      color: #607d8b;
      line-height: 1.5;
      margin: 0;
      flex: 1;
    }

    .module-card__areas {
      display: flex;
      flex-wrap: wrap;
      gap: 0.4rem;
    }

    .module-card__area-badge {
      font-size: 0.75rem;
      font-weight: 500;
      color: #1565c0;
      background: #e3f2fd;
      padding: 0.2rem 0.6rem;
      border-radius: 4px;
    }

    .module-card__nav-btn {
      display: inline-block;
      text-align: center;
      padding: 0.6rem 1.2rem;
      background: #1a237e;
      color: #ffffff;
      border-radius: 6px;
      text-decoration: none;
      font-size: 0.875rem;
      font-weight: 500;
      transition: background 0.2s;
      margin-top: 0.5rem;
    }

    .module-card__nav-btn:hover {
      background: #283593;
    }
  `]
})
export class DashboardComponent {
  readonly modules: ModuleCard[] = [
    {
      name: 'Employee Onboarding',
      description: 'Automate Microsoft 365 onboarding for new employees — group assignments, welcome emails, and induction scheduling.',
      graphAreas: ['Groups', 'Mail', 'Calendar'],
      route: '/onboarding'
    },
    {
      name: 'Legal Matters',
      description: 'Create Microsoft 365 workspaces for legal matters — SharePoint folders, Teams channels, participant access, and kickoff meetings.',
      graphAreas: ['Drive', 'Teams', 'Calendar'],
      route: '/legal-matters'
    },
    {
      name: 'Loan Approvals',
      description: 'Automate communication workflows when a loan is approved — customer notifications, team alerts, and follow-up scheduling.',
      graphAreas: ['Mail', 'Teams', 'Calendar'],
      route: '/loan-approvals'
    },
    {
      name: 'BuildEstate Projects',
      description: 'Launch full Microsoft 365 project workspaces on planning approval — task boards, documents, director notifications, and kickoffs.',
      graphAreas: ['Drive', 'Planner', 'Mail', 'Calendar'],
      route: '/buildestate-projects'
    },
    {
      name: 'CEO Command Centre',
      description: 'Aggregate signals from across Microsoft 365 into a single executive dashboard — meetings, emails, tasks, documents, and security alerts.',
      graphAreas: ['Calendar', 'Mail', 'Planner', 'Drive', 'Security'],
      route: '/ceo-command-centre'
    },
    {
      name: 'Productivity Assistant',
      description: 'Receive weekly productivity summaries gathered from Microsoft 365 — calendar, email, tasks, and document activity in one AI-ready package.',
      graphAreas: ['Calendar', 'Mail', 'Planner', 'Drive'],
      route: '/productivity-assistant'
    }
  ];
}
