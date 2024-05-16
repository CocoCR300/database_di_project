async function eliminarUsuario(id)
{
    try {
        const response = await destroy(`user/${id}`);
        console.log(response);
    } catch (error) {
        console.log(error);
    }
}