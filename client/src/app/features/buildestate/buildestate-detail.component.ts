import { Component, inject, signal, OnInit } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { ApiService } from '../../core/services/api.service';
import { LoadingSkeletonComponent, ErrorStateComponent } from '../../shared/components';

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
  selector: 'app-buildestate-detail',
  standalone: true,
  imports: [RouterLink, LoadingSkeletonComponent, ErrorStateComponent],
  template: `
    <div class="module-page">
      @if (loading()) {
        <app-loading-skeleton [lines]="6" />
      } @else if (error()) {
        <app-error-state errorType="server" [message]="error()!" (retry)="loadProject()" />
      } @else if (project()) {
        <div class="project-header">
          <h2>{{ project()!.name }}</h2>
          <p class="location">{{ project()!.location }}</p>
          <span class="badge" [class.active]="project()!.planningStatus === 'Approved'">{{ project()!.planningStatus }}</span>
        </div>

        <div class="directors-section">
          <h3>Directors</h3>
          <ul>
            @for (director of project()!.directors; track director) {
              <li>{{ director }}</li>
            }
          </ul>
        </div>

        <div class="actions-section">
          <h3>Project Actions</h3>
          <div class="actions-grid">
            <button class="action-btn" (click)="launchWorkspace()" [disabled]="project()!.workspaceLaunched || actionInProgress()">
              {{ project()!.workspaceLaunched ? '✓ Workspace Launched' : 'Launch Workspace' }}
            </button>
            <button class="action-btn" (click)="createTaskBoard()" [disabled]="project()!.taskBoardCreated || actionInProgress()">
              {{ project()!.taskBoardCreated ? '✓ Task Board Created' : 'Create Task Board' }}
            </button>
            <button class="action-btn" (click)="notifyDirectors()" [disabled]="actionInProgress()">
              Notify Directors
            </button>
            <button class="action-btn" (click)="scheduleKickoff()" [disabled]="actionInProgress()">
              Schedule Kickoff
            </button>
          </div>
        </div>

        @if (actionMessage()) {
          <div class="action-message" [class.success]="!actionError()" [class.error]="actionError()">
            {{ actionMessage() }}
          </div>
        }

        <div class="links-section">
          <a [routerLink]="['/buildestate-projects', project()!.id, 'weekly-report']" class="btn-secondary">View Weekly Report</a>
        </div>
      }
    </div>
  `,
  styles: [`
    .module-page { padding: 1.5rem; }
    .project-header h2 { color: #1a237e; margin-bottom: 0.25rem; }
    .location { color: #607d8b; margin-bottom: 0.5rem; }
    .badge { display: inline-block; padding: 0.25rem 0.5rem; border-radius: 4px; font-size: 0.75rem; background: #e0e0e0; color: #616161; }
    .badge.active { background: #c8e6c9; color: #2e7d32; }
    .directors-section, .actions-section, .links-section { margin-top: 1.5rem; }
    .directors-section h3, .actions-section h3 { color: #37474f; margin-bottom: 0.5rem; }
    .directors-section ul { list-style: none; padding: 0; }
    .directors-section li { padding: 0.25rem 0; color: #455a64; }
    .actions-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(200px, 1fr)); gap: 0.75rem; }
    .action-btn { padding: 0.6rem 1rem; border: 1px solid #1a237e; background: #fff; color: #1a237e; border-radius: 4px; cursor: pointer; font-size: 0.875rem; }
    .action-btn:hover:not(:disabled) { background: #e8eaf6; }
    .action-btn:disabled { opacity: 0.5; cursor: not-allowed; }
    .action-message { margin-top: 1rem; padding: 0.5rem; border-radius: 4px; }
    .action-message.success { background: #e8f5e9; color: #2e7d32; }
    .action-message.error { background: #ffebee; color: #c62828; }
    .btn-secondary { display: inline-block; padding: 0.5rem 1rem; background: #eceff1; color: #37474f; border-radius: 4px; text-decoration: none; }
    .links-section { margin-top: 1.5rem; }
  `]
})
export class BuildestateDetailComponent implements OnInit {
  private api = inject(ApiService);
  private route = inject(ActivatedRoute);

  project = signal<BuildEstateProject | null>(null);
  loading = signal(true);
  error = signal<string | null>(null);
  actionInProgress = signal(false);
  actionMessage = signal<string | null>(null);
  actionError = signal(false);

  private get projectId(): string {
    return this.route.snapshot.paramMap.get('id') ?? '';
  }

  ngOnInit(): void {
    this.loadProject();
  }

  loadProject(): void {
    this.loading.set(true);
    this.error.set(null);
    this.api.get<BuildEstateProject>(`/buildestate-projects/${this.projectId}`).subscribe({
      next: (data) => {
        this.project.set(data);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Failed to load project details.');
        this.loading.set(false);
      }
    });
  }

  launchWorkspace(): void {
    this.performAction(`/buildestate-projects/${this.projectId}/launch-workspace`, 'Workspace launched successfully.');
  }

  createTaskBoard(): void {
    this.performAction(`/buildestate-projects/${this.projectId}/create-task-board`, 'Task board created successfully.');
  }

  notifyDirectors(): void {
    this.performAction(`/buildestate-projects/${this.projectId}/notify-directors`, 'Directors notified successfully.');
  }

  scheduleKickoff(): void {
    this.performAction(`/buildestate-projects/${this.projectId}/schedule-kickoff`, 'Kickoff meeting scheduled.');
  }

  private performAction(endpoint: string, successMessage: string): void {
    this.actionInProgress.set(true);
    this.actionMessage.set(null);
    this.actionError.set(false);
    this.api.post<unknown>(endpoint, {}).subscribe({
      next: () => {
        this.actionMessage.set(successMessage);
        this.actionError.set(false);
        this.actionInProgress.set(false);
        this.loadProject();
      },
      error: () => {
        this.actionMessage.set('Action failed. Please try again.');
        this.actionError.set(true);
        this.actionInProgress.set(false);
      }
    });
  }
}
