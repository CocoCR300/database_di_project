import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, RouterStateSnapshot, Router, MaybeAsync, GuardResult } from '@angular/router';
import { AppState } from '../models/app_state';

@Injectable({
  providedIn: 'root'
})
export class AlreadyLoggedInGuard {
  constructor(
    private appState: AppState,
    private router: Router)
    {}

  // https://angular.dev/api/router/CanActivateFn 
  canActivate(
    next: ActivatedRouteSnapshot,
    state: RouterStateSnapshot): MaybeAsync<GuardResult> {
      if (this.appState.isUserLogged){
        this.router.navigate(['/home']);
      }

      return !this.appState.isUserLogged;
  }
}