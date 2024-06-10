import Swal from 'sweetalert2';

export class Dialogs 
{
    public static async showConfirmDialog(
        title: string,
        text: string
    ): Promise<boolean> {
        const result = await Swal.fire({
            title: title,
            text: text,
            showCancelButton: true,
            showConfirmButton: true,
            cancelButtonText: "No",
            confirmButtonText: "Sí",
        });

        return result.isConfirmed;
    }
}