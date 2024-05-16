async function modificarUsuario(id){
    routing('update');
    try{
        let response = await get(`user/${id}`);
        console.log(response);
        let data = await response;
        
        document.getElementById('userNameUpdate').value = data.user.userName;
        document.getElementById('firstNameUpdate').value = data.user.person.firstName;
        document.getElementById('lastNameUpdate').value = data.user.person.lastName;
        document.getElementById('emailUpdate').value = data.user.person.emailAddress;

    }catch(error){
        console.log(error);
    }
}

async function userPUT(){
    let usernameToModify = document.getElementById('userNameUpdate').value;
    let data = {
        username: document.getElementById('userNameUpdate').value,
        firstname: document.getElementById('firstNameUpdate').value,
        lastname: document.getElementById('lastNameUpdate').value,
        emailaddress: document.getElementById('emailUpdate').value
    };

    try{
        let response = await patch(`user/${usernameToModify}`,data);
        return await response;
    }catch(error){
        console.log(error);
    }
}