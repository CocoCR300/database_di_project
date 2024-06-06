import { ComponentFixture, TestBed } from '@angular/core/testing';

import { LodgingComponent } from './lodging.component';

describe('LodgingComponent', () => {
  let component: LodgingComponent;
  let fixture: ComponentFixture<LodgingComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [LodgingComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(LodgingComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
