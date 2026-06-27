import { Component, inject, signal, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ApiService } from '../../core/services/api.service';
import { LoadingSkeletonComponent, ErrorStateComponent, EmptyStateComponent } from '../../shared/components';
import { GraphExplanationPanelComponent, GraphPanelData } from '../../shared/components/graph-explanation-panel/graph-explanation-panel.component';

interface BuildEstateProject {
  id: string;
  name: string;
  location: string;
  planningStatus: string;
  directors: string[];
  workspaceLaunched: boolean;
  taskBoardCreated: boolean;
}

@Component({
  selector: 'app-buildestate-overview',
  standalone: true,
  imports: [RouterLink, LoadingSkeletonComponent, ErrorStateComponent, EmptyStateComponent, GraphExplanationPanelComponent],
  template: `
    <div class="module-page">
      <div class="page-header">
        <h2>BuildEstate Project Launch Workspace</h2>
        <p>Launch full Microsoft 365 project workspaces — task boards, SharePoint documents, director notifications, and kickoff scheduling.</p>
        <a routerLink="create" class="btn-primary">+ New Project</a>
      </div>

      @if (loading()) {
        <app-loading-skeleton [lines]="5" />
      } @else if (error()) {
        <app-error-state errorType="server" [message]="error()!" (retry)="loadProjects()" />
      } @else if (projects().length === 0) {
        <app-empty-state
          title="No Projects"
          description="No BuildEstate projects have been created yet. Create your first project to launch a workspace."
          actionLabel="Create Project"
          actionRoute="create" />
      } @else {
        <div class="projects-grid">
          @for (project of projects(); track project.id) {
            <div class="project-card">
              <h3><a [routerLink]="project.id">{{ project.name }}</a></h3>
              <p class="location">{{ project.location }}</p>
              <div class="badges">
                <span class="badge" [class.active]="project.planningStatus === 'Approved'">{{ project.planningStatus }}</span>
                @if (project.workspaceLaunched) {
                  <span class="badge active">Workspace</span>
                }
                @if (project.taskBoardCreated) {
                  <span class="badge active">Task Board</span>
                }
              </div>
              <p class="directors">Directors: {{ project.directors.join(', ') }}</p>
            </div>
          }
        </div>
      }

      <app-graph-explanation-panel [panelData]="graphPanelData" />
    </div>
  `,
  styles: [`
    .module-page { padding: 1.5rem; }
    .page-header { margin-bottom: 1.5rem; }
    .page-header h2 { color: #1a237e; margin-bottom: 0.5rem; }
    .page-header p { color: #607d8b; margin-bottom: 1rem; }
    .btn-primary { display: inline-block; padding: 0.5rem 1rem; background: #1a237e; color: #fff; border-radius: 4px; text-decoration: none; }
    .projects-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(300px, 1fr)); gap: 1rem; }
    .project-card { border: 1px solid #e0e0e0; border-radius: 8px; padding: 1rem; }
    .project-card h3 { margin-bottom: 0.5rem; }
    .project-card h3 a { color: #1a237e; text-decoration: none; }
    .location { color: #607d8b; font-size: 0.875rem; margin-bottom: 0.5rem; }
    .badges { display: flex; gap: 0.5rem; margin-bottom: 0.5rem; flex-wrap: wrap; }
    .badge { padding: 0.25rem 0.5rem; border-radius: 4px; font-size: 0.75rem; background: #e0e0e0; color: #616161; }
    .badge.active { background: #c8e6c9; color: #2e7d32; }
    .directors { font-size: 0.875rem; color: #455a64; }
  `]
})
export class BuildestateOverviewComponent implements OnInit {
  private api = inject(ApiService);

  readonly graphPanelData: GraphPanelData = {
    endpoints: ['POST /drive/root/children', 'POST /planner/plans/{id}/buckets', 'POST /me/sendMail', 'POST /me/events'],
    scopes: ['Files.ReadWrite.All', 'Tasks.ReadWrite', 'Mail.Send', 'Calendars.ReadWrite'],
    descriptions: [
      'Create SharePoint workspace folders',
      'Create Planner task boards with buckets',
      'Notify directors via email',
      'Schedule project kickoff events'
    ]
  };

  projects = signal<BuildEstateProject[]>([]);
  loading = signal(true);
  error = signal<string | null>(null);

  ngOnInit(): void {
    this.loadProjects();
  }

  loadProjects(): void {
    this.loading.set(true);
    this.error.set(null);
    this.api.get<BuildEstateProject[]>('/buildestate-projects/overview').subscribe({
      next: (data) => {
        this.projects.set(data ?? []);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Failed to load BuildEstate projects.');
        this.loading.set(false);
      }
    });
  }
}
