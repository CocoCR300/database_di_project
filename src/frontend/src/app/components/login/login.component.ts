import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { User } from '../../models/user';
import { UserService } from '../../services/user.service';
import { RouterLink, RouterOutlet } from '@angular/router';
import { AppState } from '../../models/app_state';
import { firstValueFrom } from 'rxjs';
import { NotificationService } from '../../services/notification.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [FormsModule, RouterOutlet, RouterLink],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css'],
  providers: [UserService]
})
export class LoginComponent {
  public status: number;
  public user: User;

  constructor(
    private _appState: AppState,
    private _userService: UserService,
    private _notificationService: NotificationService
  ) {
    this.status = -1;
    this.user = new User("", "", 0, 0, "", "", "", null, null);
  }

  async onsubmit(form: any) {
    let response: any;
    try {
      response = await firstValueFrom(this._userService.login(this.user));
      if (response.token) {
        const token = response.token; 
        sessionStorage.setItem('authToken', token);  
        try {
          const identity = await firstValueFrom(this._userService.getIdentityFromApi(token));
          this._appState.logIn(token, identity.userName, identity.roleId);
        } catch (error) {
          console.log('Error al obtener la identidad', error);
        }
      } else {
        this._notificationService.show("Usuario y/o contraseña incorrectos");
        this.status = 0;
      }
    } catch (error) {
      console.log('Error en el login', error);
      this._notificationService.show("Usuario y/o contraseña incorrectos");
      this.status = 0;
    }
  }
}
