import { Component, input, signal } from '@angular/core';

export interface GraphPanelData {
  endpoints: string[];
  scopes: string[];
  descriptions: string[];
}

@Component({
  selector: 'app-graph-explanation-panel',
  standalone: true,
  templateUrl: './graph-explanation-panel.component.html',
  styleUrl: './graph-explanation-panel.component.css'
})
export class GraphExplanationPanelComponent {
  readonly panelData = input<GraphPanelData>({ endpoints: [], scopes: [], descriptions: [] });

  protected readonly expanded = signal(false);

  toggle(): void {
    this.expanded.update(v => !v);
  }
}
