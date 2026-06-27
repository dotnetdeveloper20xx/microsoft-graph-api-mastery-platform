import { Routes } from '@angular/router';
import { LegalMattersOverviewComponent } from './legal-matters-overview.component';
import { LegalMattersCreateComponent } from './legal-matters-create.component';
import { LegalMattersDetailComponent } from './legal-matters-detail.component';
import { LegalMattersDocumentsComponent } from './legal-matters-documents.component';

export const LEGAL_MATTERS_ROUTES: Routes = [
  { path: '', component: LegalMattersOverviewComponent },
  { path: 'create', component: LegalMattersCreateComponent },
  { path: ':id', component: LegalMattersDetailComponent },
  { path: ':id/documents', component: LegalMattersDocumentsComponent }
];
