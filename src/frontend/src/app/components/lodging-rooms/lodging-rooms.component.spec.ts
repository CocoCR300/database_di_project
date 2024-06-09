import { ComponentFixture, TestBed } from '@angular/core/testing';

import { LodgingInfoComponent } from './lodging-rooms.component';

describe('LodgingInfoComponent', () => {
  let component: LodgingInfoComponent;
  let fixture: ComponentFixture<LodgingInfoComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [LodgingInfoComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(LodgingInfoComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
