import { ComponentFixture, TestBed } from '@angular/core/testing';

import { NewPaymentInformationComponent } from './new-payment-information.component';

describe('NewPaymentInformationComponent', () => {
  let component: NewPaymentInformationComponent;
  let fixture: ComponentFixture<NewPaymentInformationComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [NewPaymentInformationComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(NewPaymentInformationComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
