import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';

export interface ApiEnvelope<T> {
  success: boolean;
  message: string;
  data: T | null;
  errors: { field: string; detail: string }[];
  timestamp: string;
  correlationId: string;
}

@Injectable({ providedIn: 'root' })
export class ApiService {
  private http = inject(HttpClient);
  private baseUrl = '/api';

  get<T>(path: string): Observable<T> {
    return this.http.get<ApiEnvelope<T>>(`${this.baseUrl}${path}`).pipe(
      map(envelope => envelope.data as T)
    );
  }

  post<T>(path: string, body: unknown): Observable<T> {
    return this.http.post<ApiEnvelope<T>>(`${this.baseUrl}${path}`, body).pipe(
      map(envelope => envelope.data as T)
    );
  }
}
