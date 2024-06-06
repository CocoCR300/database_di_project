import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink, RouterOutlet } from '@angular/router';
import { User } from '../../models/user';
import { AppState } from '../../models/app_state';
import { UserService } from '../../services/user.service';
import { NotificationService } from '../../services/notification.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [FormsModule, RouterOutlet, RouterLink],
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent {
  public user: User;


  constructor(
    private _appState: AppState,
    private _userService: UserService,
    private _notificationService: NotificationService,
    private _router: Router
  ) {
    this.user = new User("", "", "", "", "", "", "", "");
  }
  


  onSubmit() {
    if (this.isFormValid()) {
      this._userService.register(this.user).subscribe(
        response => {
          this._notificationService.show("Registro exitoso");
          this._router.navigate(['/login']);
        },
        error => {
          this._notificationService.show("Error en el registro");
          console.error(error);
        }
      );
    } else {
      this._notificationService.show("Por favor, complete todos los campos requeridos");
    }
  }
  isFormValid(): boolean {
    return this.user.name.trim() !== '' &&
           this.user.email_address.trim() !== '' &&
           this.user.password.trim() !== '' &&
           this.user.first_name.trim() !== '' &&
           this.user.last_name.trim() !== '' &&
           this.user.phone_number.trim() !== '' &&
           this.user.role_id.trim() !== '';
  }
}