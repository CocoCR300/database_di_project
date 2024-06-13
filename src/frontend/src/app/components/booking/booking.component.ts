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
import { Booking, BookingStatus } from '../../models/booking';
import { Lodging } from '../../models/lodging';
import { LodgingService } from '../../services/lodging.service';
import { FormControl, FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatFormFieldModule } from '@angular/material/form-field';

@Component({
  selector: 'app-booking',
  standalone: true,
  imports: [MatFormFieldModule, MatSelectModule, MatInputModule, FormsModule, ReactiveFormsModule, RouterOutlet, MatCheckboxModule, MatTableModule, MatIconModule, MatPaginator, NgIf],
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
  public lessorLodgings: Lodging[] = [];
  public selectedLodging!: Lodging;
  public lodgingFormControl = new FormControl<number | null>(null);
  public bookingDataSource: MatTableDataSource<any> = new MatTableDataSource<any>();
  public displayedColumns: string[] = ['booking_id', 'lodging', 'status', 'start_date', 'end_date', 'payment', 'actions'];
  @ViewChild(MatPaginator) paginator!: MatPaginator;
  
  constructor(
    private bookingService: BookingService,
    private appState: AppState,
    public dialog: MatDialog,
    public lodgingService: LodgingService,
    public notificationService: NotificationService
  ) { }
  ngAfterViewInit(): void {
    this.bookingDataSource.paginator = this.paginator;
  }

  async ngOnInit() {
    this.isLessor = this.appState.role == UserRoleEnum.Lessor;

    if (this.isLessor) {
      this.displayedColumns = ['booking_id', 'status', 'customer', 'start_date', 'end_date', 'payment', 'actions'];
    }
    else {
      this.displayedColumns = ['booking_id', 'lodging', 'status', 'customer', 'start_date', 'end_date', 'payment', 'actions'];
    }

    const userName = this.appState.userName!;
    this.lessorLodgings = await firstValueFrom(this.lodgingService.getLessorLodgings(userName));
    this.lodgingFormControl.setValue(this.lessorLodgings[0].id);
    this.lodgingFormControl.valueChanges.subscribe(_ => {
      this.loadBookings();
    });

    await this.loadBookings();
  }

  showActionsForBooking(booking: any): boolean {
    return booking.status != BookingStatus.Confirmed;
  }
  async loadBookings() {
    const userName = this.appState.userName!;
    let bookingsResponse: Booking[] = [];
    
    if (this.isLessor && this.lodgingFormControl.value != null) {
      bookingsResponse = await firstValueFrom(this.bookingService.getLodgingBookings(this.lodgingFormControl.value));
    }
    else {
      bookingsResponse = await firstValueFrom(this.bookingService.getBookingsByPersonId(userName));
    }

    this.bookings = bookingsResponse.map((booking: any) => {
      let bookingUserName: string;
      let lodgingName: string;
      if (this.isLessor) {
        bookingUserName = booking.userName;
        lodgingName = this.lessorLodgings.find(lodging => lodging.id == this.lodgingFormControl.value)!.name;
      }
      else {
        bookingUserName = userName;
        lodgingName = booking.name;
      }

      return {
        booking_id: booking.id,
        lodging: lodgingName,
        customer: bookingUserName, 
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
        let total = 0;

        for (const roomBooking of dialogResult.booking.roomBookings) {
          total += roomBooking.cost + roomBooking.fees;
        }

        const paymentAndConfirmationResponse = await firstValueFrom(this.bookingService.payBooking(dialogResult.booking.booking_id,
          moment.utc().format("YYYY-MM-DD HH:mm"),
          total,
          dialogResult.extraData
        ));

        if (paymentAndConfirmationResponse.ok) {
          this.notificationService.show("El pago ha sido realizado con éxito. La reserva ha sido confirmada.");
          this.loadBookings();
        }
        else {
          this.notificationService.show("Ha ocurrido un error al realizar el pago.");
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
    }
  }
}
