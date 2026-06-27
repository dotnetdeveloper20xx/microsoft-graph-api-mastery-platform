import { HttpInterceptorFn } from '@angular/common/http';

export const correlationInterceptor: HttpInterceptorFn = (req, next) => {
  const correlationId = crypto.randomUUID();
  const clonedReq = req.clone({
    setHeaders: { 'X-Correlation-ID': correlationId }
  });
  return next(clonedReq);
};
