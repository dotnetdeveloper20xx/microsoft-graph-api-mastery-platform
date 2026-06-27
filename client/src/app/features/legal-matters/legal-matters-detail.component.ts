import { Component, inject, signal, OnInit } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../core/services/api.service';
import { LoadingSkeletonComponent, ErrorStateComponent } from '../../shared/components';

interface LegalMatterDetail {
  id: string;
  referenceNumber: string;
  clientName: string;
  matterType: string;
  assignedSolicitor: string;
  workspaceCreated: boolean;
  participantCount: number;
}

@Component({
  selector: 'app-legal-matters-detail',
  standalone: true,
  imports: [RouterLink, FormsModule, LoadingSkeletonComponent, ErrorStateComponent],
  template: `
    <div class="module-page">
      @if (loading()) {
        <app-loading-skeleton [lines]="6" />
      } @else if (error()) {
        <app-error-state [errorType]="'server'" [message]="error()!" (retry)="loadMatter()" />
      } @else if (matter()) {
        <div class="page-header">
          <a routerLink="/legal-matters" class="back-link">← Back to Legal Matters</a>
          <h2>{{ matter()!.clientName }}</h2>
          <div class="meta-row">
            <span class="reference">{{ matter()!.referenceNumber }}</span>
            <span class="matter-type">{{ matter()!.matterType }}</span>
            <span class="solicitor">Solicitor: {{ matter()!.assignedSolicitor }}</span>
          </div>
        </div>

        <div class="actions-section">
          <h3>Workspace Actions</h3>
          <div class="actions-grid">
            <div class="action-card">
              <h4>Create Workspace</h4>
              <p>Create SharePoint folder structure and Teams channel for this matter.</p>
              @if (matter()!.workspaceCreated) {
                <span class="status-done">✓ Workspace Created</span>
              } @else {
                <button class="btn-action" (click)="createWorkspace()" [disabled]="actionLoading()">
                  {{ actionLoading() ? 'Creating...' : 'Create Workspace' }}
                </button>
              }
            </div>

            <div class="action-card">
              <h4>Invite Participants</h4>
              <p>Invite internal and external participants to the workspace.</p>
              <div class="invite-form">
                <input
                  type="email"
                  [(ngModel)]="participantEmail"
                  placeholder="participant@example.com"
                  class="input-sm" />
                <button class="btn-action" (click)="inviteParticipants()" [disabled]="actionLoading() || !participantEmail">
                  Invite
                </button>
              </div>
              <p class="participant-count">Current participants: {{ matter()!.participantCount }}</p>
            </div>

            <div class="action-card">
              <h4>Schedule Kickoff</h4>
              <p>Schedule a kickoff meeting within 14 days with all participants.</p>
              <button class="btn-action" (click)="scheduleKickoff()" [disabled]="actionLoading()">
                Schedule Kickoff
              </button>
            </div>

            <div class="action-card">
              <h4>Documents</h4>
              <p>View the document folder tree for this matter's workspace.</p>
              <a [routerLink]="['documents']" class="btn-link">View Documents →</a>
            </div>
          </div>
        </div>

        @if (actionMessage()) {
          <div class="action-message" [class.success]="actionSuccess()" [class.error]="!actionSuccess()">
            {{ actionMessage() }}
          </div>
        }
      }
    </div>
  `,
  styles: [`
    .module-page { padding: 1.5rem; }
    .back-link { color: #5c6bc0; text-decoration: none; font-size: 0.9rem; }
    .back-link:hover { text-decoration: underline; }
    .page-header { margin-bottom: 2rem; }
    .page-header h2 { color: #1a237e; margin: 0.5rem 0; }
    .meta-row { display: flex; gap: 1rem; flex-wrap: wrap; color: #757575; font-size: 0.9rem; }
    .reference { color: #5c6bc0; font-weight: 600; }
    .actions-section h3 { color: #424242; margin-bottom: 1rem; }
    .actions-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(280px, 1fr)); gap: 1.25rem; }
    .action-card {
      padding: 1.25rem; border: 1px solid #e0e0e0; border-radius: 8px; background: #fff;
    }
    .action-card h4 { margin: 0 0 0.5rem; color: #1a237e; }
    .action-card p { color: #757575; font-size: 0.9rem; margin-bottom: 0.75rem; }
    .btn-action {
      padding: 0.5rem 1rem; background: #1a237e; color: #fff;
      border: none; border-radius: 6px; cursor: pointer; font-size: 0.85rem;
    }
    .btn-action:hover:not(:disabled) { background: #283593; }
    .btn-action:disabled { opacity: 0.6; cursor: not-allowed; }
    .btn-link { color: #1a237e; text-decoration: none; font-weight: 500; font-size: 0.9rem; }
    .btn-link:hover { text-decoration: underline; }
    .status-done { color: #2e7d32; font-weight: 500; }
    .invite-form { display: flex; gap: 0.5rem; margin-bottom: 0.5rem; }
    .input-sm { flex: 1; padding: 0.4rem 0.6rem; border: 1px solid #bdbdbd; border-radius: 6px; font-size: 0.85rem; }
    .participant-count { font-size: 0.8rem; color: #9e9e9e; margin: 0; }
    .action-message {
      margin-top: 1.5rem; padding: 0.75rem 1rem; border-radius: 6px; font-size: 0.9rem;
    }
    .action-message.success { background: #e8f5e9; color: #2e7d32; }
    .action-message.error { background: #ffebee; color: #c62828; }
  `]
})
export class LegalMattersDetailComponent implements OnInit {
  private api = inject(ApiService);
  private route = inject(ActivatedRoute);

  matter = signal<LegalMatterDetail | null>(null);
  loading = signal(true);
  error = signal<string | null>(null);
  actionLoading = signal(false);
  actionMessage = signal<string | null>(null);
  actionSuccess = signal(true);
  participantEmail = '';

  ngOnInit(): void {
    this.loadMatter();
  }

  loadMatter(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) return;

    this.loading.set(true);
    this.error.set(null);
    this.api.get<LegalMatterDetail>(`/legal-matters/${id}`).subscribe({
      next: (data) => {
        this.matter.set(data);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Failed to load legal matter details.');
        this.loading.set(false);
      }
    });
  }

  createWorkspace(): void {
    const id = this.matter()?.id;
    if (!id) return;

    this.actionLoading.set(true);
    this.actionMessage.set(null);
    this.api.post<unknown>(`/legal-matters/${id}/create-workspace`, {}).subscribe({
      next: () => {
        this.actionSuccess.set(true);
        this.actionMessage.set('Workspace created successfully — SharePoint folders and Teams channel configured.');
        this.actionLoading.set(false);
        this.loadMatter();
      },
      error: () => {
        this.actionSuccess.set(false);
        this.actionMessage.set('Failed to create workspace.');
        this.actionLoading.set(false);
      }
    });
  }

  inviteParticipants(): void {
    const id = this.matter()?.id;
    if (!id || !this.participantEmail) return;

    this.actionLoading.set(true);
    this.actionMessage.set(null);
    this.api.post<{ invitedCount: number }>(`/legal-matters/${id}/invite-participants`, {
      participants: [this.participantEmail]
    }).subscribe({
      next: (result) => {
        this.actionSuccess.set(true);
        this.actionMessage.set(`Successfully invited ${result?.invitedCount ?? 1} participant(s).`);
        this.participantEmail = '';
        this.actionLoading.set(false);
        this.loadMatter();
      },
      error: () => {
        this.actionSuccess.set(false);
        this.actionMessage.set('Failed to invite participants.');
        this.actionLoading.set(false);
      }
    });
  }

  scheduleKickoff(): void {
    const id = this.matter()?.id;
    if (!id) return;

    this.actionLoading.set(true);
    this.actionMessage.set(null);
    this.api.post<unknown>(`/legal-matters/${id}/schedule-kickoff`, {}).subscribe({
      next: () => {
        this.actionSuccess.set(true);
        this.actionMessage.set('Kickoff meeting scheduled within the next 14 days.');
        this.actionLoading.set(false);
      },
      error: () => {
        this.actionSuccess.set(false);
        this.actionMessage.set('Failed to schedule kickoff meeting.');
        this.actionLoading.set(false);
      }
    });
  }
}
