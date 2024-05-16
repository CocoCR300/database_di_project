function userPOST(){
    let data = {
        data: {
            username: document.getElementById('userName').value,
            password: document.getElementById('password').value,
            firstname: document.getElementById('firstName').value,
            lastname: document.getElementById('lastName').value,
            emailaddress: document.getElementById('email').value,
            phonenumbers: [document.getElementById('phoneNumber').value],
            roleid: 1
        }
    };

    post('user', data)
        .then(result => {
            console.log('Probando');
            if(!result.ok){
                throw Error(result);
            }
            return result.json();
        })
        .then(dataSent => {
            console.log(dataSent);
        })
        .catch(error => {
            console.error('Error:', error);
        });
}

$('#create-user-button').on('click', userPOST);