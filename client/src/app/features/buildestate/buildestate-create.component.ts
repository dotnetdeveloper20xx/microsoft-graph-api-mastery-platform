import { Component, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../core/services/api.service';

interface CreateProjectRequest {
  name: string;
  location: string;
  planningStatus: string;
  directors: string[];
}

@Component({
  selector: 'app-buildestate-create',
  standalone: true,
  imports: [FormsModule],
  template: `
    <div class="module-page">
      <h2>Create New BuildEstate Project</h2>
      <form (ngSubmit)="onSubmit()" class="form">
        <div class="form-group">
          <label for="name">Project Name</label>
          <input id="name" type="text" [(ngModel)]="name" name="name" required maxlength="200" />
        </div>
        <div class="form-group">
          <label for="location">Location</label>
          <input id="location" type="text" [(ngModel)]="location" name="location" required maxlength="200" />
        </div>
        <div class="form-group">
          <label for="planningStatus">Planning Status</label>
          <select id="planningStatus" [(ngModel)]="planningStatus" name="planningStatus" required>
            <option value="">Select status</option>
            <option value="Pending">Pending</option>
            <option value="Submitted">Submitted</option>
            <option value="Approved">Approved</option>
            <option value="Rejected">Rejected</option>
          </select>
        </div>
        <div class="form-group">
          <label>Directors</label>
          <div class="directors-list">
            @for (director of directors(); track $index) {
              <div class="director-item">
                <input type="text" [value]="director" (input)="updateDirector($index, $event)" placeholder="Director name" />
                <button type="button" (click)="removeDirector($index)" class="btn-remove">×</button>
              </div>
            }
          </div>
          <button type="button" (click)="addDirector()" class="btn-secondary">+ Add Director</button>
        </div>

        @if (errorMessage()) {
          <div class="error-message">{{ errorMessage() }}</div>
        }

        <div class="form-actions">
          <button type="submit" class="btn-primary" [disabled]="submitting()">
            {{ submitting() ? 'Creating...' : 'Create Project' }}
          </button>
        </div>
      </form>
    </div>
  `,
  styles: [`
    .module-page { padding: 1.5rem; max-width: 600px; }
    h2 { color: #1a237e; margin-bottom: 1.5rem; }
    .form-group { margin-bottom: 1rem; }
    .form-group label { display: block; font-weight: 500; margin-bottom: 0.25rem; color: #37474f; }
    .form-group input, .form-group select { width: 100%; padding: 0.5rem; border: 1px solid #cfd8dc; border-radius: 4px; font-size: 0.875rem; }
    .directors-list { display: flex; flex-direction: column; gap: 0.5rem; margin-bottom: 0.5rem; }
    .director-item { display: flex; gap: 0.5rem; }
    .director-item input { flex: 1; }
    .btn-remove { background: #ef5350; color: #fff; border: none; border-radius: 4px; width: 30px; cursor: pointer; }
    .btn-secondary { padding: 0.4rem 0.8rem; background: #eceff1; border: 1px solid #cfd8dc; border-radius: 4px; cursor: pointer; }
    .btn-primary { padding: 0.5rem 1.5rem; background: #1a237e; color: #fff; border: none; border-radius: 4px; cursor: pointer; }
    .btn-primary:disabled { opacity: 0.6; cursor: not-allowed; }
    .form-actions { margin-top: 1.5rem; }
    .error-message { color: #c62828; background: #ffebee; padding: 0.5rem; border-radius: 4px; margin-top: 1rem; }
  `]
})
export class BuildestateCreateComponent {
  private api = inject(ApiService);
  private router = inject(Router);

  name = '';
  location = '';
  planningStatus = '';
  directors = signal<string[]>(['']);
  submitting = signal(false);
  errorMessage = signal<string | null>(null);

  addDirector(): void {
    this.directors.update(list => [...list, '']);
  }

  removeDirector(index: number): void {
    this.directors.update(list => list.filter((_, i) => i !== index));
  }

  updateDirector(index: number, event: Event): void {
    const value = (event.target as HTMLInputElement).value;
    this.directors.update(list => list.map((d, i) => i === index ? value : d));
  }

  onSubmit(): void {
    const filteredDirectors = this.directors().filter(d => d.trim() !== '');
    if (!this.name || !this.location || !this.planningStatus || filteredDirectors.length === 0) {
      this.errorMessage.set('Please fill in all fields and add at least one director.');
      return;
    }

    this.submitting.set(true);
    this.errorMessage.set(null);

    const request: CreateProjectRequest = {
      name: this.name,
      location: this.location,
      planningStatus: this.planningStatus,
      directors: filteredDirectors
    };

    this.api.post<{ id: string }>('/buildestate-projects', request).subscribe({
      next: (result) => {
        this.submitting.set(false);
        this.router.navigate(['/buildestate-projects', result?.id ?? '']);
      },
      error: () => {
        this.errorMessage.set('Failed to create project. Please try again.');
        this.submitting.set(false);
      }
    });
  }
}
