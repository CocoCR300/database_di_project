import { Component, OnInit } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { BookingService } from '../../services/booking.service';
import { AppState } from '../../models/app_state';
import { firstValueFrom } from 'rxjs';
import { UserService } from '../../services/user.service';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { MatIconModule } from '@angular/material/icon';
import { MatDialog } from '@angular/material/dialog';
import { BookingDialogComponent } from '../booking-dialog/booking-dialog.component';
import { NotificationService } from '../../services/notification.service';

@Component({
  selector: 'app-booking',
  standalone: true,
  imports: [RouterOutlet, MatTableModule, MatIconModule],
  templateUrl: './booking.component.html',
  styleUrls: ['./booking.component.css']
})
export class BookingComponent implements OnInit {
  public bookings: any[] = [];
  public bookingDataSource: MatTableDataSource<any> = new MatTableDataSource<any>();
  public displayedColumns: string[] = ['booking_id', 'lodging', 'customer', 'status', 'start_date', 'end_date', 'actions'];

  constructor(
    private bookingService: BookingService,
    private appState: AppState,
    private userService: UserService,
    public dialog: MatDialog,
    public notificationService: NotificationService
  ) {}

  async ngOnInit() {
    await this.loadBookings();
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
        start_date: booking.roomBookings[0]?.startDate,
        end_date: booking.roomBookings[0]?.endDate
      };
    });

    this.bookingDataSource.data = this.bookings;
  }

  onAction(booking: any) {
    const dialogRef = this.dialog.open(BookingDialogComponent, {
      width: '500px',
      data: { booking }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result === 'Reserva eliminada exitosamente') {
        this.notificationService.show(result);
        this.loadBookings(); 
      } else if (result) {
        this.notificationService.show(result);
      }
    });
  }
}
