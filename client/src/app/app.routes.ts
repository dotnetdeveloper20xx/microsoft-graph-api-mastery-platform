import { Routes } from '@angular/router';
import { LayoutComponent } from './core/layout/layout.component';

export const routes: Routes = [
  { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
  {
    path: '',
    component: LayoutComponent,
    children: [
      {
        path: 'dashboard',
        loadComponent: () => import('./features/dashboard/dashboard.component').then(m => m.DashboardComponent)
      },
      {
        path: 'onboarding',
        loadChildren: () => import('./features/onboarding/onboarding.routes').then(m => m.ONBOARDING_ROUTES)
      },
      {
        path: 'legal-matters',
        loadChildren: () => import('./features/legal-matters/legal-matters.routes').then(m => m.LEGAL_MATTERS_ROUTES)
      },
      {
        path: 'loan-approvals',
        loadChildren: () => import('./features/loan-approvals/loan-approvals.routes').then(m => m.LOAN_APPROVALS_ROUTES)
      },
      {
        path: 'buildestate-projects',
        loadChildren: () => import('./features/buildestate/buildestate.routes').then(m => m.BUILDESTATE_ROUTES)
      },
      {
        path: 'ceo-command-centre',
        loadChildren: () => import('./features/ceo-command-centre/ceo-command-centre.routes').then(m => m.CEO_COMMAND_CENTRE_ROUTES)
      },
      {
        path: 'productivity-assistant',
        loadChildren: () => import('./features/productivity/productivity.routes').then(m => m.PRODUCTIVITY_ROUTES)
      }
    ]
  }
];
