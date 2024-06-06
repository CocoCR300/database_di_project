import Swal from 'sweetalert2';

export class Dialogs 
{
    public static async showConfirmDialog(
        title: string,
        text: string
    ): Promise<boolean> {
        const result = await Swal.fire({
            title: "¿Está seguro de que desea eliminar este alojamiento?",
            text: "Está acción no se puede revertir.",
            showCancelButton: true,
            showConfirmButton: true,
            cancelButtonText: "No",
            confirmButtonText: "Sí",
        });

        return result.isConfirmed;
    }
}