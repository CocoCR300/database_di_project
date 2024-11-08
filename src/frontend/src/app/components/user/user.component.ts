import { AfterViewInit, Component, OnInit, ViewChild} from '@angular/core';
import { UserService } from '../../services/user.service';
import { User } from '../../models/user';
import { AsyncPipe, CurrencyPipe, NgFor } from '@angular/common';
import { Observable, map } from 'rxjs';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { MatPaginator } from '@angular/material/paginator';
import { Router } from '@angular/router';
import { Dialogs } from '../../util/dialogs';
import { AppResponse } from '../../models/app_response';
import Swal from 'sweetalert2';
import { AppState } from '../../models/app_state';

@Component({
  selector: 'app-user',
  standalone: true,
  imports: [AsyncPipe, CurrencyPipe, NgFor, MatTableModule, MatPaginator],
  templateUrl: './user.component.html',
  styleUrl: './user.component.css',
  providers: [UserService]
})
export class UserComponent implements OnInit, AfterViewInit{
  usersTable!: User[];
  users!: Observable<User[]>;
  public constructor(
    private _appState: AppState,
    private _userService: UserService,
    private router: Router)
  { }
  
  dataSource = new MatTableDataSource<User>(this.usersTable);
  @ViewChild(MatPaginator)
  paginator!: MatPaginator;
  
  ngAfterViewInit() {
    this.dataSource.paginator = this.paginator;
  }
  
  ngOnInit(): void{
    this.users = this._userService.getUsers();
    this.updateTable();
  }

  phoneNumbersFormat(username:string): string{
    let user = this.usersTable.find((userAux: User) => userAux.userName === username) as User;
    let phoneCollectionAux = '';
    user.phoneNumbers?.forEach((phoneNumber: string) => {
      if(phoneCollectionAux === '') {
        phoneCollectionAux = phoneNumber;
      }else{
        phoneCollectionAux = phoneCollectionAux + " | " + phoneNumber;
      }
    })
    return phoneCollectionAux;
  }

  updateTable(){
    this._userService.getUsers().subscribe((result: User[]) => {
      this.usersTable = result
      this.usersTable.forEach((user: User) => {

        if(user.userName === "root" || user.userName === "generic_customer" || user.userName === "generic_lessor"){
          this.usersTable = this.usersTable.filter((userAux => user.userName !== userAux.userName));
        }

        if(user.roleId === 1) {
          user.roleName = "Administrador";
        }else if(user.roleId === 2){
          user.roleName = "Cliente";
        }else {
          user.roleName = "Arrendador";
        }
      })
      this.dataSource.data = this.usersTable;
    });
  }

  public async delete(username:string): Promise<void> {
    if (this._appState.userName == username) {
      await Swal.fire({
        icon: "warning",
        title: "Advertencia",
        text: "No puedes eliminarte a tí mismo desde esta tabla, hazlo desde la sección de configuración."
      });

      return;
    }

    let deleteUser = await Dialogs.showConfirmDialog(
      "¿Está seguro de que desea eliminar este usuario?",
      "Esta acción no se puede revertir."
    );
    
      if(!deleteUser){
        return;
      }

      this._userService.deleteUser(username).subscribe(
        (response: AppResponse) => {
          if(response.ok) {
              this.users = this.users.pipe(
                map(users => users.filter(user => user.userName !== username))
              );
              
              Swal.fire({
                icon: "info",
                title: "Usuario eliminado exitosamente",
              });
              console.log("Eliminado");

              //Refresh the table
              this.updateTable()
          }else{
              Swal.fire({
                icon: "error",
                "title": "Ha ocurrido un error",
                "text": response.body.message
              })
          }
        }
      )

    }
    

    columnsToDisplay =['user_name', 'first_name', 'last_name', 'phone_numbers', 'email_address', 'role'];
}
