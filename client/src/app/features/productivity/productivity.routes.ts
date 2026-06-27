import { Routes } from '@angular/router';
import { ProductivityOverviewComponent } from './productivity-overview.component';
import { ProductivityCalendarComponent } from './productivity-calendar.component';
import { ProductivityEmailsComponent } from './productivity-emails.component';
import { ProductivityTasksComponent } from './productivity-tasks.component';
import { ProductivityDocumentsComponent } from './productivity-documents.component';

export const PRODUCTIVITY_ROUTES: Routes = [
  { path: '', component: ProductivityOverviewComponent },
  { path: 'calendar', component: ProductivityCalendarComponent },
  { path: 'emails', component: ProductivityEmailsComponent },
  { path: 'tasks', component: ProductivityTasksComponent },
  { path: 'documents', component: ProductivityDocumentsComponent }
];
