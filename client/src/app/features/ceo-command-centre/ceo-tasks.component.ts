import { Component, inject, signal, OnInit } from '@angular/core';
import { DatePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { ApiService } from '../../core/services/api.service';
import { LoadingSkeletonComponent, ErrorStateComponent, EmptyStateComponent } from '../../shared/components';

interface PlannerTask {
  title: string;
  status: string;
  assignedTo: string;
  dueDate: string;
}

@Component({
  selector: 'app-ceo-tasks',
  standalone: true,
  imports: [RouterLink, DatePipe, LoadingSkeletonComponent, ErrorStateComponent, EmptyStateComponent],
  template: `
    <div class="module-page">
      <div class="page-header">
        <a routerLink="/ceo-command-centre" class="back-link">← Back to Dashboard</a>
        <h2>Pending Tasks</h2>
      </div>

      @if (loading()) {
        <app-loading-skeleton [lines]="5" />
      } @else if (error()) {
        <app-error-state errorType="server" [message]="error()!" (retry)="loadTasks()" />
      } @else if (tasks().length === 0) {
        <app-empty-state
          title="No Pending Tasks"
          description="All tasks are completed. Great work!"
          actionLabel="Back to Dashboard"
          actionRoute="/ceo-command-centre" />
      } @else {
        <div class="tasks-list">
          @for (task of tasks(); track $index) {
            <div class="task-card">
              <div class="task-status" [class]="task.status.toLowerCase().replace(' ', '-')">
                {{ task.status }}
              </div>
              <div class="task-content">
                <h4>{{ task.title }}</h4>
                <p class="assigned">Assigned to: {{ task.assignedTo }}</p>
                @if (task.dueDate) {
                  <p class="due">Due: {{ task.dueDate | date:'mediumDate' }}</p>
                }
              </div>
            </div>
          }
        </div>
      }
    </div>
  `,
  styles: [`
    .module-page { padding: 1.5rem; }
    .page-header { margin-bottom: 1.5rem; }
    .back-link { color: #1a237e; text-decoration: none; font-size: 0.875rem; }
    .page-header h2 { color: #1a237e; margin-top: 0.5rem; }
    .tasks-list { display: flex; flex-direction: column; gap: 0.5rem; }
    .task-card { display: flex; gap: 1rem; padding: 0.75rem 1rem; border: 1px solid #e0e0e0; border-radius: 8px; align-items: flex-start; }
    .task-status { padding: 0.2rem 0.5rem; border-radius: 4px; font-size: 0.7rem; text-transform: uppercase; font-weight: 600; white-space: nowrap; }
    .task-status.not-started { background: #eceff1; color: #607d8b; }
    .task-status.in-progress { background: #e3f2fd; color: #1565c0; }
    .task-status.overdue { background: #ffebee; color: #c62828; }
    .task-content h4 { margin: 0 0 0.25rem; color: #263238; font-size: 0.9rem; }
    .assigned { color: #455a64; font-size: 0.8rem; margin: 0; }
    .due { color: #9e9e9e; font-size: 0.75rem; margin: 0.25rem 0 0; }
  `]
})
export class CeoTasksComponent implements OnInit {
  private api = inject(ApiService);

  tasks = signal<PlannerTask[]>([]);
  loading = signal(true);
  error = signal<string | null>(null);

  ngOnInit(): void {
    this.loadTasks();
  }

  loadTasks(): void {
    this.loading.set(true);
    this.error.set(null);
    this.api.get<PlannerTask[]>('/ceo-command-centre/tasks').subscribe({
      next: (data) => {
        this.tasks.set(data ?? []);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Failed to load tasks.');
        this.loading.set(false);
      }
    });
  }
}
