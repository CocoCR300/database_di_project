import { Component,Inject, OnInit } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialog } from '@angular/material/dialog';
import { BookingStatus } from '../../models/booking';
import { MatCardModule } from '@angular/material/card';
import { NgFor, NgIf } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { NotificationService } from '../../services/notification.service';
import { NewPaymentInformationComponent } from '../new-payment-information/new-payment-information.component';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatOptionModule } from '@angular/material/core';
import { PaymentInformation } from '../../models/payment_information';
import { BaseService } from '../../services/base.service';
import { FormControl, FormsModule, ReactiveFormsModule } from '@angular/forms';
import moment from 'moment';

@Component({
  selector: 'app-booking-dialog',
  standalone: true,
  imports: [FormsModule, MatButtonModule, MatCardModule, MatFormFieldModule, MatSelectModule, MatOptionModule, NgFor, NgIf, ReactiveFormsModule],
  templateUrl: './booking-dialog.component.html',
  styleUrl: './booking-dialog.component.css'
})
export class BookingDialogComponent implements OnInit
{
  public canPay: boolean = false;
  public canCancelOrConfirm: boolean = false;
  public canDelete: boolean = false;
  public paymentProcess: boolean = false;
  public selectedPaymentInformation: FormControl<PaymentInformation | null> = new FormControl(null);
  public title: string = "Acciones  de reserva";
  public booking: any;
  public paymentInformationArray: PaymentInformation[] = [];

  constructor(
    public dialogRef: MatDialogRef<BookingDialogComponent>,
    public dialog: MatDialog,
    public notificationService: NotificationService,
    public service: BaseService,
    @Inject(MAT_DIALOG_DATA) public data: { booking: any }
  ) {
    this.booking = data.booking;

    this.canCancelOrConfirm = this.booking.status == BookingStatus.Created;
    this.canPay = this.booking.status == BookingStatus.Confirmed && this.booking.payment == null;
    this.canDelete = this.booking.status == BookingStatus.Cancelled || this.booking.status == BookingStatus.Finished;
  }

  ngOnInit(): void {
    this.service.get<PaymentInformation[]>('paymentinformation', true).subscribe(array => {
      this.paymentInformationArray = array;
    });
  }

  paymentInformationDisplay(paymentInformation: PaymentInformation) {
    let cardNumber = paymentInformation.cardNumber;
    let cardNumberDisplay = "";
    for (let i = 0; i < cardNumber.length; i += 4) {
      const chunk = cardNumber.substring(i, i + 4);
      cardNumberDisplay += chunk;

      if (chunk.length === 4) {
        cardNumberDisplay += " ";
      }
    }

    return `${cardNumberDisplay} (${moment(paymentInformation.cardExpiryDate).format("DD-MM-YYYY")})`
  }

  openPaymentInformationDialog(): void {
    this.dialog
    .open(NewPaymentInformationComponent)
    .afterClosed()
    .subscribe(response => {
      if (response !== null) {
        this.paymentInformationArray.push(response);
      }
    });
  }

  cancelBooking() {
    this.dialogRef.close(new BookingDialogResult(BookingDialogResultEnum.Cancel,
      this.data.booking, null
    ));
  }
  
  confirmBooking() {
    this.title = "Proceso de pago";
    this.paymentProcess = true;
  }

  processPayment() {
    if (this.selectedPaymentInformation.hasError("required")) {
      this.notificationService.show("Debe seleccionar un medio de pago.");
      return;
    }

    this.dialogRef.close(new BookingDialogResult(BookingDialogResultEnum.Confirm,
      this.data.booking, this.selectedPaymentInformation.value
    ));
  }
  
  cancelPayment() {
    this.title = "Acciones de reserva";
    this.paymentProcess = false;
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
