import { Component, inject, signal, OnInit } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { ApiService } from '../../core/services/api.service';
import { LoadingSkeletonComponent, ErrorStateComponent } from '../../shared/components';

interface WeeklyReport {
  tasksToDo: number;
  tasksInProgress: number;
  tasksCompleted: number;
  milestonesDueThisWeek: string[];
  teamActivityCount: number;
}

@Component({
  selector: 'app-buildestate-report',
  standalone: true,
  imports: [RouterLink, LoadingSkeletonComponent, ErrorStateComponent],
  template: `
    <div class="module-page">
      <div class="page-header">
        <a [routerLink]="['/buildestate-projects', projectId]" class="back-link">← Back to Project</a>
        <h2>Weekly Report</h2>
      </div>

      @if (loading()) {
        <app-loading-skeleton [lines]="5" />
      } @else if (error()) {
        <app-error-state errorType="server" [message]="error()!" (retry)="loadReport()" />
      } @else if (report()) {
        <div class="report-content">
          <div class="stats-grid">
            <div class="stat-card">
              <span class="stat-value">{{ report()!.tasksToDo }}</span>
              <span class="stat-label">To Do</span>
            </div>
            <div class="stat-card">
              <span class="stat-value">{{ report()!.tasksInProgress }}</span>
              <span class="stat-label">In Progress</span>
            </div>
            <div class="stat-card">
              <span class="stat-value">{{ report()!.tasksCompleted }}</span>
              <span class="stat-label">Completed</span>
            </div>
            <div class="stat-card">
              <span class="stat-value">{{ report()!.teamActivityCount }}</span>
              <span class="stat-label">Team Activities</span>
            </div>
          </div>

          <div class="milestones-section">
            <h3>Milestones Due This Week</h3>
            @if (report()!.milestonesDueThisWeek.length === 0) {
              <p class="empty-text">No milestones due this week.</p>
            } @else {
              <ul>
                @for (milestone of report()!.milestonesDueThisWeek; track milestone) {
                  <li>{{ milestone }}</li>
                }
              </ul>
            }
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
    .stats-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(150px, 1fr)); gap: 1rem; margin-bottom: 1.5rem; }
    .stat-card { text-align: center; padding: 1rem; border: 1px solid #e0e0e0; border-radius: 8px; }
    .stat-value { display: block; font-size: 2rem; font-weight: 700; color: #1a237e; }
    .stat-label { display: block; font-size: 0.8rem; color: #607d8b; margin-top: 0.25rem; }
    .milestones-section h3 { color: #37474f; margin-bottom: 0.5rem; }
    .milestones-section ul { list-style: disc; padding-left: 1.5rem; }
    .milestones-section li { padding: 0.25rem 0; color: #455a64; }
    .empty-text { color: #9e9e9e; font-style: italic; }
  `]
})
export class BuildestateReportComponent implements OnInit {
  private api = inject(ApiService);
  private route = inject(ActivatedRoute);

  report = signal<WeeklyReport | null>(null);
  loading = signal(true);
  error = signal<string | null>(null);

  get projectId(): string {
    return this.route.snapshot.paramMap.get('id') ?? '';
  }

  ngOnInit(): void {
    this.loadReport();
  }

  loadReport(): void {
    this.loading.set(true);
    this.error.set(null);
    this.api.get<WeeklyReport>(`/buildestate-projects/${this.projectId}/weekly-report`).subscribe({
      next: (data) => {
        this.report.set(data);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Failed to load weekly report.');
        this.loading.set(false);
      }
    });
  }
}
