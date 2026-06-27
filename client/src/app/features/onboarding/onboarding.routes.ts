import { Routes } from '@angular/router';
import { OnboardingOverviewComponent } from './onboarding-overview.component';
import { OnboardingCreateComponent } from './onboarding-create.component';
import { OnboardingDetailComponent } from './onboarding-detail.component';

export const ONBOARDING_ROUTES: Routes = [
  { path: '', component: OnboardingOverviewComponent },
  { path: 'create', component: OnboardingCreateComponent },
  { path: ':id', component: OnboardingDetailComponent }
];
