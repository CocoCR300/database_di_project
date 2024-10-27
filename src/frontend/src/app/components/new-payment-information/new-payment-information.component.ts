import { Component, Inject } from '@angular/core';
import { FormControl, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButton } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { RouterLink } from '@angular/router';
import { PaymentInformation } from '../../models/payment_information';
import { provideNativeDateAdapter } from '@angular/material/core';

@Component({
  selector: 'app-new-payment-information',
  standalone: true,
  providers: [provideNativeDateAdapter()],
  imports: [FormsModule, MatButton, MatCardModule, MatDatepickerModule, MatFormFieldModule, MatInputModule, ReactiveFormsModule, RouterLink],
  templateUrl: './new-payment-information.component.html',
  styleUrl: './new-payment-information.component.css'
})
export class NewPaymentInformationComponent
{
  public cardInformation = new PaymentInformation(0, 0, 0, new Date(), ''); 
  public cardInformationFormGroup: FormGroup = this.buildFormGroup();

  public constructor(
    public dialogRef: MatDialogRef<NewPaymentInformationComponent>,
  )
  {
  }

  public addPaymentInformation() {

  }

  public cancel() {
    this.dialogRef.close();
  }

  private buildFormGroup() {
    return new FormGroup({
      cardNumber: new FormControl(this.cardInformation?.cardNumber, {
        nonNullable: true,
        validators: [ Validators.required ]
      }),
      cardSecurityCode: new FormControl(this.cardInformation?.cardSecurityCode, { nonNullable: true, validators: Validators.required }),
      cardExpiryDate: new FormControl(null, { nonNullable: true, validators: Validators.required }),
    });
  }

  async ngOnInit() {
    this.cardInformationFormGroup = this.buildFormGroup();
  }
}
