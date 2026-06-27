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
  selector: 'app-productivity-calendar',
  standalone: true,
  imports: [RouterLink, LoadingSkeletonComponent, ErrorStateComponent, EmptyStateComponent],
  template: `
    <div class="module-page">
      <div class="page-header">
        <a routerLink="/productivity-assistant" class="back-link">← Back to Overview</a>
        <h2>Calendar Events</h2>
        <p>Calendar events for the current week.</p>
      </div>

      @if (loading()) {
        <app-loading-skeleton [lines]="5" />
      } @else if (error()) {
        <app-error-state errorType="server" [message]="error()!" (retry)="loadCalendar()" />
      } @else if (events().length === 0) {
        <app-empty-state
          title="No Calendar Events"
          description="No events scheduled for this week."
          actionLabel="Back to Overview"
          actionRoute="/productivity-assistant" />
      } @else {
        <div class="events-list">
          @for (event of events(); track $index) {
            <div class="event-card">
              <div class="event-time">
                {{ formatDate(event.start) }}<br />
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
    .page-header p { color: #607d8b; }
    .events-list { display: flex; flex-direction: column; gap: 0.75rem; }
    .event-card { display: flex; gap: 1rem; padding: 1rem; border: 1px solid #e0e0e0; border-radius: 8px; }
    .event-time { min-width: 130px; color: #1a237e; font-weight: 500; font-size: 0.8rem; }
    .event-details h4 { margin: 0 0 0.25rem; color: #263238; }
    .attendees { color: #607d8b; font-size: 0.8rem; margin: 0; }
  `]
})
export class ProductivityCalendarComponent implements OnInit {
  private api = inject(ApiService);

  events = signal<CalendarEvent[]>([]);
  loading = signal(true);
  error = signal<string | null>(null);

  ngOnInit(): void {
    this.loadCalendar();
  }

  loadCalendar(): void {
    this.loading.set(true);
    this.error.set(null);
    this.api.get<CalendarEvent[]>('/productivity-assistant/calendar').subscribe({
      next: (data) => {
        this.events.set(data ?? []);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Failed to load calendar events.');
        this.loading.set(false);
      }
    });
  }

  formatDate(isoString: string): string {
    return new Date(isoString).toLocaleDateString([], { weekday: 'short', month: 'short', day: 'numeric' });
  }

  formatTime(isoString: string): string {
    return new Date(isoString).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
  }
}
