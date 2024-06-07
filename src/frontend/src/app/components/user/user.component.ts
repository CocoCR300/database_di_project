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
  public constructor(private _userService: UserService, private router: Router)
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

  updateTable(){
    this._userService.getUsers().subscribe((result: User[]) => {
      this.usersTable = result
      this.dataSource.data = this.usersTable;
    });
  }

  public async delete(username:string): Promise<void> {
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
                map(users => users.filter(user => user.name !== username))
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
                "text": response.message
              })
          }
        }
      )

    }
    

    columnsToDisplay =['user_name', 'first_name', 'last_name', 'phone_number', 'email_address', 'role', 'image'];
}
