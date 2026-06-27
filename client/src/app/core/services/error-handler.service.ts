import { Injectable, signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';

export interface AppError {
  type: 'network' | 'validation' | 'server';
  message: string;
}

@Injectable({ providedIn: 'root' })
export class ErrorHandlerService {
  private _lastError = signal<AppError | null>(null);
  readonly lastError = this._lastError.asReadonly();

  handleError(error: HttpErrorResponse): void {
    if (error.status === 0) {
      this._lastError.set({ type: 'network', message: 'Network error. Please check your connection.' });
    } else if (error.status === 400) {
      this._lastError.set({ type: 'validation', message: error.error?.message || 'Validation failed.' });
    } else {
      this._lastError.set({ type: 'server', message: error.error?.message || 'An unexpected error occurred.' });
    }
  }

  clearError(): void {
    this._lastError.set(null);
  }
}
