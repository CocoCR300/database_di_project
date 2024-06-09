import {Component, Inject} from '@angular/core';
import {CdkDragDrop, CdkDropList, CdkDrag, moveItemInArray, CdkDropListGroup} from '@angular/cdk/drag-drop';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import {MatButtonModule} from '@angular/material/button';
import { NgFor } from '@angular/common';
import { server } from '../../services/global';

@Component({
  selector: 'app-images-upload-dialog',
  standalone: true,
  imports: [CdkDrag, CdkDropList, CdkDropListGroup, MatButtonModule, MatDialogModule, NgFor],
  templateUrl: './images-upload-dialog.component.html',
  styleUrl: './images-upload-dialog.component.css'
})
export class ImagesUploadDialogComponent
{
  images: DialogImageData[] = [];
  newImages: any[] = [];
  imagesToDelete: string[] = [];

  public constructor(
    public dialogRef: MatDialogRef<ImagesUploadDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: ImagesUploadDialogData
  ) {
    for (const fileName of data.imageFileNames) {
      this.images.push(new DialogImageData(fileName, null));
    }
  }

  prepareDataForDisplay(imageData: DialogImageData) {
    let imageSrc = "";
    if (imageData.fileName) {
      imageSrc = `${server.lodgingImages}${imageData.fileName}`;
    }
    else {
      imageSrc = imageData.data;
    }

    return imageSrc;
  }

  deleteImage(imageData: DialogImageData) {
    const index = this.images.indexOf(imageData);
    const removedImageData = this.images.splice(index, 1);

    if (removedImageData.length > 0 && removedImageData[0].fileName) {
      this.imagesToDelete.push(removedImageData[0].fileName);
    }
  }

  onFilesSelected(event: any) {
    const files = event.target.files;

    for (const file of files) {
      const reader = new FileReader();
      reader.onload = e => {
        this.images.push(new DialogImageData(null, reader.result));
        this.newImages.push(file);
      };
      
      reader.readAsDataURL(file);
    }
  }

  cancelUpload() {
    this.dialogRef.close(new ImagesUploadDialogResult(false, [], [], []));
  }

  confirmUpload() {
    this.dialogRef.close(
      new ImagesUploadDialogResult(true, this.newImages, this.images, this.imagesToDelete));
  }

  dropListItemDropped(event: any) {
    moveItemInArray(this.images, event.item.data, event.container.data);
  }
}

export class DialogImageData
{
  public constructor(
    public fileName: string | null,
    public data: any,
  ) { }
}

export class ImagesUploadDialogData
{
  public constructor(
    public edit: boolean,
    public imageFileNames: string[]
  ) { }
}

export class ImagesUploadDialogResult
{
  public constructor(
    public confirmed: boolean,
    public newImages: File[],
    public updatedImages: DialogImageData[],
    public imagesToDelete: string[]
  ) { }
}