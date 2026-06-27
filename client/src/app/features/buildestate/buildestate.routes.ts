import { Routes } from '@angular/router';
import { BuildestateOverviewComponent } from './buildestate-overview.component';
import { BuildestateCreateComponent } from './buildestate-create.component';
import { BuildestateDetailComponent } from './buildestate-detail.component';
import { BuildestateReportComponent } from './buildestate-report.component';

export const BUILDESTATE_ROUTES: Routes = [
  { path: '', component: BuildestateOverviewComponent },
  { path: 'create', component: BuildestateCreateComponent },
  { path: ':id/weekly-report', component: BuildestateReportComponent },
  { path: ':id', component: BuildestateDetailComponent }
];
