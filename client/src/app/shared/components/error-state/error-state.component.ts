import { Component, computed, input, output } from '@angular/core';

export type ErrorType = 'network' | 'validation' | 'server';

@Component({
  selector: 'app-error-state',
  standalone: true,
  templateUrl: './error-state.component.html',
  styleUrl: './error-state.component.css'
})
export class ErrorStateComponent {
  readonly errorType = input<ErrorType>('server');
  readonly message = input<string>('');
  readonly retry = output<void>();

  protected readonly errorIcon = computed(() => {
    switch (this.errorType()) {
      case 'network': return '🌐';
      case 'validation': return '⚠️';
      case 'server': return '🔧';
    }
  });

  protected readonly errorTitle = computed(() => {
    switch (this.errorType()) {
      case 'network': return 'Network Error';
      case 'validation': return 'Validation Error';
      case 'server': return 'Server Error';
    }
  });

  protected readonly defaultMessage = computed(() => {
    switch (this.errorType()) {
      case 'network': return 'Unable to connect to the server. Please check your network connection and try again.';
      case 'validation': return 'The submitted data contains errors. Please review and correct the highlighted fields.';
      case 'server': return 'An unexpected server error occurred. Please try again later or contact support.';
    }
  });

  protected readonly displayMessage = computed(() => this.message() || this.defaultMessage());

  onRetry(): void {
    this.retry.emit();
  }
}
