import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ImagesUploadDialogComponentComponent } from './images-upload-dialog.component';

describe('ImagesUploadDialogComponentComponent', () => {
  let component: ImagesUploadDialogComponentComponent;
  let fixture: ComponentFixture<ImagesUploadDialogComponentComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ImagesUploadDialogComponentComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(ImagesUploadDialogComponentComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
