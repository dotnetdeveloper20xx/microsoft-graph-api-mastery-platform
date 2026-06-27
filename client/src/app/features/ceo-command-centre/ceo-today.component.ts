import { Component, inject, signal, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ApiService } from '../../core/services/api.service';
import { LoadingSkeletonComponent, ErrorStateComponent, EmptyStateComponent } from '../../shared/components';

interface CalendarEvent {
  subject: string;
  start: string;
  end: string;
  attendees: string[];
}

@Component({
  selector: 'app-ceo-today',
  standalone: true,
  imports: [RouterLink, LoadingSkeletonComponent, ErrorStateComponent, EmptyStateComponent],
  template: `
    <div class="module-page">
      <div class="page-header">
        <a routerLink="/ceo-command-centre" class="back-link">← Back to Dashboard</a>
        <h2>Today's Schedule</h2>
      </div>

      @if (loading()) {
        <app-loading-skeleton [lines]="5" />
      } @else if (error()) {
        <app-error-state errorType="server" [message]="error()!" (retry)="loadToday()" />
      } @else if (events().length === 0) {
        <app-empty-state
          title="No Events Today"
          description="Your calendar is clear today."
          actionLabel="Back to Dashboard"
          actionRoute="/ceo-command-centre" />
      } @else {
        <div class="events-list">
          @for (event of events(); track $index) {
            <div class="event-card">
              <div class="event-time">
                {{ formatTime(event.start) }} – {{ formatTime(event.end) }}
              </div>
              <div class="event-details">
                <h4>{{ event.subject }}</h4>
                @if (event.attendees.length > 0) {
                  <p class="attendees">{{ event.attendees.join(', ') }}</p>
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
    .events-list { display: flex; flex-direction: column; gap: 0.75rem; }
    .event-card { display: flex; gap: 1rem; padding: 1rem; border: 1px solid #e0e0e0; border-radius: 8px; }
    .event-time { min-width: 120px; color: #1a237e; font-weight: 500; font-size: 0.875rem; }
    .event-details h4 { margin: 0 0 0.25rem; color: #263238; }
    .attendees { color: #607d8b; font-size: 0.8rem; margin: 0; }
  `]
})
export class CeoTodayComponent implements OnInit {
  private api = inject(ApiService);

  events = signal<CalendarEvent[]>([]);
  loading = signal(true);
  error = signal<string | null>(null);

  ngOnInit(): void {
    this.loadToday();
  }

  loadToday(): void {
    this.loading.set(true);
    this.error.set(null);
    this.api.get<CalendarEvent[]>('/ceo-command-centre/today').subscribe({
      next: (data) => {
        this.events.set(data ?? []);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Failed to load today\'s schedule.');
        this.loading.set(false);
      }
    });
  }

  formatTime(isoString: string): string {
    return new Date(isoString).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
  }
}
