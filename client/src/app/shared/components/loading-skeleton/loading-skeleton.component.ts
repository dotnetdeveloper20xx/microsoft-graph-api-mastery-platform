import { Component, computed, input } from '@angular/core';

@Component({
  selector: 'app-loading-skeleton',
  standalone: true,
  templateUrl: './loading-skeleton.component.html',
  styleUrl: './loading-skeleton.component.css'
})
export class LoadingSkeletonComponent {
  readonly lines = input<number>(3);

  protected readonly skeletonLines = computed(() => {
    const count = this.lines();
    const widths = ['100%', '85%', '70%', '92%', '60%'];
    return Array.from({ length: count }, (_, i) => ({
      width: widths[i % widths.length]
    }));
  });
}
