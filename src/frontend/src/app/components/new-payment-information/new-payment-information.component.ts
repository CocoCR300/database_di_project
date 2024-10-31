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
import { NotificationService } from '../../services/notification.service';
import { BaseService } from '../../services/base.service';
import { AppState } from '../../models/app_state';
import { firstValueFrom } from 'rxjs';
import Swal from 'sweetalert2';
import moment from 'moment';

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
  public cardInformationFormGroup: FormGroup = this.buildFormGroup();

  public constructor(
    public dialogRef: MatDialogRef<NewPaymentInformationComponent>,
    public appState: AppState,
    public service: BaseService,
    public notificationService: NotificationService
  ) { }

  public async addPaymentInformation() {
    const cardNumber = this.cardInformationFormGroup.get<string>('cardNumber')!;
    const cardSecurityCode = this.cardInformationFormGroup.get<string>('cardSecurityCode')!;
    const cardExpiryDate = this.cardInformationFormGroup.get<string>('cardExpiryDate')!;

    const cardNumberText = cardNumber.value.replaceAll(' ', '');
    if (this.cardInformationFormGroup.invalid) {
      if (cardNumber.hasError("required")) {
        this.notificationService.show("Debe ingresar el número de tarjeta.")
      }
      if (cardNumberText.length >= 12) {
        this.notificationService.show("El número de tarjeta debe 12 dígitos como mínimo.")
      }
      if (cardNumberText.length <= 16) {
        this.notificationService.show("El número de tarjeta debe tener 16 dígitos como máximo.")
      }
      if (!this.isNumber(cardNumberText)) {
        this.notificationService.show("El número de tarjeta solo puede contener caracteres númericos.")
      }

      if (cardSecurityCode.hasError("required")) {
        this.notificationService.show("Debe ingresar el código de seguridad de la tarjeta.")
      }
      if (cardSecurityCode.hasError("minLength")) {
        this.notificationService.show("El código de seguridad debe tener 3 dígitos como mínimo.")
      }
      if (cardSecurityCode.hasError("maxLength")) {
        this.notificationService.show("El código de seguridad debe tener 4 dígitos como máximo.")
      }
      if (cardSecurityCode.hasError("pattern")) {
        this.notificationService.show("El código de seguridad solo puede contener caracteres númericos.")
      }

      if (cardExpiryDate.hasError("required")) {
        this.notificationService.show("Debe ingresar la fecha de expiración de la tarjeta.")
      }

      return;
    }

    const response = await firstValueFrom(this.service.post("paymentinformation", true, {
      cardNumber: cardNumberText,
      cardSecurityCode: cardSecurityCode.value,
      cardExpiryDate: moment(cardExpiryDate.value).format('YYYY-MM-DD'),
      cardHolderName: this.appState.userName!
    }));
    if (!response.ok) {
      Swal.fire({
        title: "Ha ocurrido un error",
        icon: "error"
      });

      return;
    }

    Swal.fire({
      title: "El medio de pago ha sido agregado con éxito",
      icon: "success"
    });

    let paymentInformation = new PaymentInformation(
      response.body.id, cardNumber.value, cardSecurityCode.value, cardExpiryDate.value, this.appState.userName!
    );
    this.dialogRef.close(paymentInformation);
  }

  public cancel() {
    this.dialogRef.close(null);
  }

  private isNumber(text: string): boolean {
    for (let character of text) {
      if (character < '0' || character > '9') {
        return false;
      }
    }

    return true;
  }

  private buildFormGroup() {
    const formGroup = new FormGroup({
      cardNumber: new FormControl('', { nonNullable: true,
        validators: Validators.required
      }),
      cardSecurityCode: new FormControl('', { nonNullable: true,
        validators: [ Validators.required, Validators.minLength(3), Validators.maxLength(4), Validators.pattern("^[0-9]+$") ]
      }),
      cardExpiryDate: new FormControl('', { nonNullable: true, validators: Validators.required }),
    });

    const cardNumberFormControl = formGroup.get("cardNumber");
    cardNumberFormControl?.valueChanges.subscribe(text => {
        text = text.replaceAll(' ', '');
        let cardNumberDisplay = "";
        for (let i = 0; i < text.length; i += 4) {
          const chunk = text.substring(i, i + 4);
          cardNumberDisplay += chunk;
          
          if (chunk.length === 4) {
            cardNumberDisplay += " ";
          }
        }

        cardNumberFormControl.setValue(cardNumberDisplay.trimEnd(), { emitEvent: false });
    });

    return formGroup;
  }

  async ngOnInit() {
    this.cardInformationFormGroup = this.buildFormGroup();
  }
}
