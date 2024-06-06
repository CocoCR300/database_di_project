import { Component } from '@angular/core';
import { Router, RouterLink, RouterOutlet, NavigationEnd} from '@angular/router';
import { CommonModule } from '@angular/common';
import { AppState } from './models/app_state';
import { NotificationComponent } from './components/notification/notification.component';
import { UserService } from './services/user.service';
import { UserRole } from './models/user';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, RouterOutlet, RouterLink, NotificationComponent],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css',
  providers: [UserService]
})
export class AppComponent {
  title = 'frontend';
  currentRoute: string = '';

  constructor(
    private userService: UserService,
    private appState: AppState,
    private router: Router) {
    this.router.events.subscribe(event => {
      if (event instanceof NavigationEnd) {
        this.currentRoute = event.url;
      }
    });
  }

  public get isAdmin() {
    return this.appState.role == UserRole.Administrator;
  }

  public get isUserLogged() {
    return this.appState.isUserLogged;
  }

  public get showNavbar() {
    return this.router.url !== "/login" && this.router.url !== "/register";
  }

  public get userName() {
    return this.appState.userName;
  }

  logOut(){
    this.userService.logOut();
  }

  public async editUser(userName: string) {
    this.router.navigate(["user/settings", userName]);
  }
}