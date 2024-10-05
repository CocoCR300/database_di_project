import { Component } from '@angular/core';
import { Router, RouterLink, RouterOutlet, NavigationEnd} from '@angular/router';
import { CommonModule } from '@angular/common';
import { AppState } from './models/app_state';
import { NotificationComponent } from './components/notification/notification.component';
import { UserService } from './services/user.service';
import { UserRoleEnum } from './models/user';
import { firstValueFrom } from 'rxjs';
import Swal from 'sweetalert2';

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
    return this.appState.role == UserRoleEnum.Administrator;
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

  public async createDatabase() {
    const response = await firstValueFrom(this.userService.post("database/create", false, null));

    if (response.ok) {
      Swal.fire({
          icon: "success",
          title: "La base de datos ha sido con exito."
      });
    }
    else if (response.status === 409) {
      Swal.fire({
          icon: "info",
          title: "La base de datos ya existe."
      });
    }
    else {
      Swal.fire({
          icon: "error",
          title: "Ha ocurrido un error."
      });
    }
  }

  public async dropDatabase() {
    const response = await firstValueFrom(this.userService.post("database/drop", false, null));

    if (response.ok) {
      Swal.fire({
        icon: "success",
        title: "La base de datos ha sido eliminada con exito."
      });
    }
    else if (response.status === 404) {
      Swal.fire({
        icon: "info",
        title: "La base de datos no existe."
      });
    }
    else {
      Swal.fire({
        icon: "error",
        title: "Ha ocurrido un error."
      });
    }
  }

  public deleteDatabase() {

  }
}