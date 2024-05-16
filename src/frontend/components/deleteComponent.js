async function eliminarUsuario(id){
    try{
        let response = await destroy(`user/${id}`);
        return await response;
    }catch(error){
        console.log(error);
    }
}