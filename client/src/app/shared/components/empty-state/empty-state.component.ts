import { Component, input } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-empty-state',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './empty-state.component.html',
  styleUrl: './empty-state.component.css'
})
export class EmptyStateComponent {
  readonly title = input<string>('No data available');
  readonly description = input<string>('There is nothing to display at this time.');
  readonly actionLabel = input<string>('Get Started');
  readonly actionRoute = input<string>('/dashboard');
}
