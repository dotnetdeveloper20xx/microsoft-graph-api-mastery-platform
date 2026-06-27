import { Routes } from '@angular/router';
import { CeoCommandCentreOverviewComponent } from './ceo-command-centre-overview.component';
import { CeoTodayComponent } from './ceo-today.component';
import { CeoEmailsComponent } from './ceo-emails.component';
import { CeoTasksComponent } from './ceo-tasks.component';
import { CeoDocumentsComponent } from './ceo-documents.component';
import { CeoSecurityComponent } from './ceo-security.component';

export const CEO_COMMAND_CENTRE_ROUTES: Routes = [
  { path: '', component: CeoCommandCentreOverviewComponent },
  { path: 'today', component: CeoTodayComponent },
  { path: 'emails', component: CeoEmailsComponent },
  { path: 'tasks', component: CeoTasksComponent },
  { path: 'documents', component: CeoDocumentsComponent },
  { path: 'security', component: CeoSecurityComponent }
];
