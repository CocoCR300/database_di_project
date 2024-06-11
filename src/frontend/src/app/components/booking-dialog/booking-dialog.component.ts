import { Component,Inject} from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA} from '@angular/material/dialog';
import { Booking } from '../../models/booking';
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
  public paymentProcess = false;
  public title: string = "Acciones  de reserva";
  invoiceFile: File | null = null;

  constructor(
    public dialogRef: MatDialogRef<BookingDialogComponent>,
    public notificationService: NotificationService,
    @Inject(MAT_DIALOG_DATA) public data: { booking: Booking }
  ) { }

  confirmBooking() {
    this.dialogRef.close(new BookingDialogResult(BookingDialogResultEnum.Confirm,
      this.data.booking, null
    ));
  }

  onPay() {
    this.title = "Proceso de pago";
    this.paymentProcess = true;

    console.log('Realizar Pago');
  }

  submitImageFile(event: any) {
    const files = event.target.files;

    for (const file of files) {
      this.invoiceFile = file;
      break;
    }
  }

  processPayment() {
    if (!this.invoiceFile) {
      this.notificationService.show("Debe adjuntar el comprobante.");
      return;
    }

    this.dialogRef.close(
      new BookingDialogResult(BookingDialogResultEnum.Pay, this.data.booking, this.invoiceFile));
  }
  
  cancelPayment() {
    this.title = "Acciones de reserva";
    this.paymentProcess = false;
    this.invoiceFile = null;
  }

  public async onCancel() {
    console.log('Eliminar reserva');
    this.dialogRef.close(
      new BookingDialogResult(BookingDialogResultEnum.Delete, this.data.booking, null));
  }
  onClose(){
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
  Confirm,
  Delete,
  Pay
}
