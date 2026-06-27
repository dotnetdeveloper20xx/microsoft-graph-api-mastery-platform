import { Routes } from '@angular/router';
import { LoanApprovalsOverviewComponent } from './loan-approvals-overview.component';
import { LoanApprovalsCreateComponent } from './loan-approvals-create.component';
import { LoanApprovalsDetailComponent } from './loan-approvals-detail.component';
import { LoanApprovalsAuditComponent } from './loan-approvals-audit.component';

export const LOAN_APPROVALS_ROUTES: Routes = [
  { path: '', component: LoanApprovalsOverviewComponent },
  { path: 'create', component: LoanApprovalsCreateComponent },
  { path: ':id', component: LoanApprovalsDetailComponent },
  { path: ':id/audit', component: LoanApprovalsAuditComponent }
];
