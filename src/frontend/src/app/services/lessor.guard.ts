import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, RouterStateSnapshot, Router, MaybeAsync, GuardResult } from '@angular/router';
import { AppState } from '../models/app_state';
import { UserRoleEnum } from '../models/user';

@Injectable({
  providedIn: 'root'
})
export class LessorGuard {
  constructor(
    private appState: AppState,
    private router: Router)
    {}

  // https://angular.dev/api/router/CanActivateFn 
  canActivate(
    next: ActivatedRouteSnapshot,
    state: RouterStateSnapshot): MaybeAsync<GuardResult> {
      return this.appState.role == UserRoleEnum.Lessor;
  }
}