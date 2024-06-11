import { AfterViewInit, Component, OnInit, ViewChild } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { BookingService } from '../../services/booking.service';
import { AppState } from '../../models/app_state';
import { firstValueFrom } from 'rxjs';
import { MAT_CHECKBOX_DEFAULT_OPTIONS, MatCheckboxDefaultOptions, MatCheckboxModule} from '@angular/material/checkbox';
import { UserService } from '../../services/user.service';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { MatIconModule } from '@angular/material/icon';
import { MatDialog } from '@angular/material/dialog';
import { BookingDialogComponent, BookingDialogResult, BookingDialogResultEnum } from '../booking-dialog/booking-dialog.component';
import { NotificationService } from '../../services/notification.service';
import { MatPaginator } from '@angular/material/paginator';
import { UserRoleEnum } from '../../models/user';
import moment from 'moment';
import { NgIf } from '@angular/common';
import { BookingStatus } from '../../models/booking';

@Component({
  selector: 'app-booking',
  standalone: true,
  imports: [RouterOutlet, MatCheckboxModule, MatTableModule, MatIconModule, MatPaginator, NgIf],
  templateUrl: './booking.component.html',
  styleUrls: ['./booking.component.css'],
  providers: [
    {provide: MAT_CHECKBOX_DEFAULT_OPTIONS, useValue: { clickAction: 'noop' } as MatCheckboxDefaultOptions}
  ]
})
export class BookingComponent implements AfterViewInit, OnInit {
  public bookingStatuses = ['Creado', 'Confirmado', 'Cancelado', 'Finalizado'];

  public isLessor: boolean = false;
  public bookings: any[] = [];
  public bookingDataSource: MatTableDataSource<any> = new MatTableDataSource<any>();
  public displayedColumns: string[] = ['booking_id', 'lodging', 'status', 'start_date', 'end_date', 'payment', 'actions'];
  @ViewChild(MatPaginator) paginator!: MatPaginator;
  
  constructor(
    private bookingService: BookingService,
    private appState: AppState,
    private userService: UserService,
    public dialog: MatDialog,
    public notificationService: NotificationService
  ) {
  }
  ngAfterViewInit(): void {
    this.bookingDataSource.paginator = this.paginator;
  }

  async ngOnInit() {
    this.isLessor = this.appState.role == UserRoleEnum.Lessor;

    if (this.isLessor) {
      this.displayedColumns = ['booking_id', 'lodging', 'status', 'customer', 'start_date', 'end_date', 'payment', 'actions'];
    }

    await this.loadBookings();
  }

  showActionsForBooking(booking: any): boolean {
    return booking.status != BookingStatus.Confirmed;
  }
  async loadBookings() {
    const user = await firstValueFrom(this.userService.getUser(this.appState.userName!));
    const bookingsResponse = await firstValueFrom(this.bookingService.getBookingsByPersonId(user.userName!));

    this.bookings = bookingsResponse.map((booking: any) => {
      return {
        booking_id: booking.id,
        lodging: booking.name,
        customer: user.userName, 
        status: booking.roomBookings[0]?.status,
        statusDisplay: this.bookingStatuses[booking.roomBookings[0]?.status],
        start_date: booking.roomBookings[0]?.startDate,
        end_date: booking.roomBookings[0]?.endDate,
        payment: booking.payment,
        roomBookings: booking.roomBookings
      };
    });

    this.bookingDataSource.data = this.bookings;
  }

  async onAction(booking: any) {
    const dialogRef = this.dialog.open(BookingDialogComponent, {
      data: { booking }
    });

    const dialogResult: BookingDialogResult = await firstValueFrom(dialogRef.afterClosed());
    if (dialogResult) {
      const result = dialogResult.result;

      if (result === BookingDialogResultEnum.Confirm) {
        const response = await firstValueFrom(this.bookingService.confirmBooking(
          this.appState.userName!, dialogResult.booking.booking_id));

        if (response.ok) {
          this.notificationService.show("La reserva ha sido confirmada.");
          this.loadBookings();
        }
        else {
          this.notificationService.show("Ha ocurrido un error al confirmar la reserva.");
        }
      }
      else if (result === BookingDialogResultEnum.Delete) {
        const response = await firstValueFrom(this.bookingService.deleteBookings(this.appState.userName!, [booking.booking_id]));

        if (response.ok) {
          this.notificationService.show("La reservación ha sido eliminada con éxito.");
          this.loadBookings(); 
        }
        else {
          this.notificationService.show("Ha ocurrido un error al eliminar la reservación.");
        }
      }
      else if (result === BookingDialogResultEnum.Pay) {
        let total = 0;

        for (const roomBooking of dialogResult.booking.roomBookings) {
          total += roomBooking.cost + roomBooking.fees;
        }

        const response = await firstValueFrom(this.bookingService.payBooking(dialogResult.booking.booking_id,
          moment.utc().format("YYYY-MM-DD HH:mm"),
          total,
          dialogResult.extraData
        ));

        if (response.ok) {
          this.notificationService.show("El pago ha sido realizado con éxito.");
          this.loadBookings();
        }
        else {
          this.notificationService.show("Ha ocurrido un error al realizar el pago");
        }
      }
    }
  }
}
