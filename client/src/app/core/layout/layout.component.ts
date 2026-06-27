import { Component, signal } from '@angular/core';
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';

@Component({
  selector: 'app-layout',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './layout.component.html',
  styleUrl: './layout.component.css'
})
export class LayoutComponent {
  protected readonly sidebarCollapsed = signal(false);

  protected readonly navItems = [
    { path: '/dashboard', label: 'Dashboard', icon: '🏠' },
    { path: '/onboarding', label: 'Onboarding', icon: '👤' },
    { path: '/legal-matters', label: 'Legal Matters', icon: '⚖️' },
    { path: '/loan-approvals', label: 'Loan Approvals', icon: '💰' },
    { path: '/buildestate-projects', label: 'BuildEstate', icon: '🏗️' },
    { path: '/ceo-command-centre', label: 'CEO Command Centre', icon: '📊' },
    { path: '/productivity-assistant', label: 'Productivity', icon: '🤖' }
  ];

  toggleSidebar(): void {
    this.sidebarCollapsed.update(v => !v);
  }
}
