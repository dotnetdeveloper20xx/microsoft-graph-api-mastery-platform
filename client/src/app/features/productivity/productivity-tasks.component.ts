import { Component, inject, signal, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ApiService } from '../../core/services/api.service';
import { LoadingSkeletonComponent, ErrorStateComponent } from '../../shared/components';

interface TaskSummary {
  completed: number;
  overdue: number;
  inProgress: number;
}

@Component({
  selector: 'app-productivity-tasks',
  standalone: true,
  imports: [RouterLink, LoadingSkeletonComponent, ErrorStateComponent],
  template: `
    <div class="module-page">
      <div class="page-header">
        <a routerLink="/productivity-assistant" class="back-link">← Back to Overview</a>
        <h2>Task Summary</h2>
        <p>Task activity for the past 7 days.</p>
      </div>

      @if (loading()) {
        <app-loading-skeleton [lines]="3" />
      } @else if (error()) {
        <app-error-state errorType="server" [message]="error()!" (retry)="loadTasks()" />
      } @else if (taskSummary()) {
        <div class="stats-grid">
          <div class="stat-card completed">
            <span class="stat-value">{{ taskSummary()!.completed }}</span>
            <span class="stat-label">Completed</span>
          </div>
          <div class="stat-card in-progress">
            <span class="stat-value">{{ taskSummary()!.inProgress }}</span>
            <span class="stat-label">In Progress</span>
          </div>
          <div class="stat-card overdue">
            <span class="stat-value">{{ taskSummary()!.overdue }}</span>
            <span class="stat-label">Overdue</span>
          </div>
        </div>
      }
    </div>
  `,
  styles: [`
    .module-page { padding: 1.5rem; }
    .page-header { margin-bottom: 1.5rem; }
    .back-link { color: #1a237e; text-decoration: none; font-size: 0.875rem; }
    .page-header h2 { color: #1a237e; margin-top: 0.5rem; }
    .page-header p { color: #607d8b; }
    .stats-grid { display: grid; grid-template-columns: repeat(3, 1fr); gap: 1rem; }
    .stat-card { text-align: center; padding: 2rem; border-radius: 8px; }
    .stat-card.completed { background: #e8f5e9; border: 1px solid #c8e6c9; }
    .stat-card.in-progress { background: #e3f2fd; border: 1px solid #bbdefb; }
    .stat-card.overdue { background: #ffebee; border: 1px solid #ffcdd2; }
    .stat-value { display: block; font-size: 2.5rem; font-weight: 700; color: #1a237e; }
    .stat-card.completed .stat-value { color: #2e7d32; }
    .stat-card.in-progress .stat-value { color: #1565c0; }
    .stat-card.overdue .stat-value { color: #c62828; }
    .stat-label { display: block; font-size: 0.9rem; color: #455a64; margin-top: 0.5rem; }
  `]
})
export class ProductivityTasksComponent implements OnInit {
  private api = inject(ApiService);

  taskSummary = signal<TaskSummary | null>(null);
  loading = signal(true);
  error = signal<string | null>(null);

  ngOnInit(): void {
    this.loadTasks();
  }

  loadTasks(): void {
    this.loading.set(true);
    this.error.set(null);
    this.api.get<TaskSummary>('/productivity-assistant/tasks').subscribe({
      next: (data) => {
        this.taskSummary.set(data);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Failed to load task summary.');
        this.loading.set(false);
      }
    });
  }
}
