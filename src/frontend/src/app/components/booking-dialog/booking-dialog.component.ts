import { Component,Inject, OnInit} from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA} from '@angular/material/dialog';
import { Booking, BookingStatus } from '../../models/booking';
import { MatCardModule } from '@angular/material/card';
import { NgIf } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { NotificationService } from '../../services/notification.service';

@Component({
  selector: 'app-booking-dialog',
  standalone: true,
  imports: [MatButtonModule, MatCardModule, NgIf],
  templateUrl: './booking-dialog.component.html',
  styleUrl: './booking-dialog.component.css'
})
export class BookingDialogComponent {
  public canPay: boolean = false;
  public canCancelOrConfirm: boolean = false;
  public canDelete: boolean = false;
  public paymentProcess: boolean = false;
  public title: string = "Acciones  de reserva";
  public invoiceFileDisplay: string = "Adjunte el comprobante de pago";
  public booking: any;
  invoiceFile: File | null = null;

  constructor(
    public dialogRef: MatDialogRef<BookingDialogComponent>,
    public notificationService: NotificationService,
    @Inject(MAT_DIALOG_DATA) public data: { booking: any }
  ) {
    this.booking = data.booking;

    this.canCancelOrConfirm = this.booking.status == BookingStatus.Created;
    this.canPay = this.booking.status == BookingStatus.Confirmed && this.booking.payment == null;
    this.canDelete = this.booking.status == BookingStatus.Cancelled || this.booking.status == BookingStatus.Finished;
  }

  cancelBooking() {
    this.dialogRef.close(new BookingDialogResult(BookingDialogResultEnum.Cancel,
      this.data.booking, null
    ));
  }
  
  confirmBooking() {
    this.title = "Proceso de pago";
    this.paymentProcess = true;

    console.log('Realizar Pago');

  }

  submitImageFile(event: any) {
    const files: File[] = event.target.files;

    for (const file of files) {
      this.invoiceFile = file;
      this.invoiceFileDisplay = file.name;
      break;
    }
  }

  processPayment() {
    if (!this.invoiceFile) {
      this.notificationService.show("Debe adjuntar el comprobante.");
      return;
    }

    this.dialogRef.close(new BookingDialogResult(BookingDialogResultEnum.Confirm,
      this.data.booking, this.invoiceFile 
    ));
  }
  
  cancelPayment() {
    this.title = "Acciones de reserva";
    this.paymentProcess = false;
    this.invoiceFile = null;
    this.invoiceFileDisplay = "Adjunte el comprobante de pago";
  }

  public async onCancel() {
    console.log('Eliminar reserva');
    this.dialogRef.close(
      new BookingDialogResult(BookingDialogResultEnum.Delete, this.data.booking, null));
  }

  onClose() {
    this.dialogRef.close();
  }
}

export class BookingDialogResult
{
  public constructor(
    public result: BookingDialogResultEnum,
    public booking: any,
    public extraData: any
  ) { }
}

export enum BookingDialogResultEnum
{
  Cancel,
  Confirm,
  Delete
}
