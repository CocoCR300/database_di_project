function userPOST(){
    let data = {
        data: {
            username: document.getElementById('userName').value,
            password: document.getElementById('password').value,
            firstname: document.getElementById('firstName').value,
            lastname: document.getElementById('lastName').value,
            emailaddress: document.getElementById('email').value,
            phonenumbers: [document.getElementById('phoneNumber').value],
            roleid: 3
        }
    };

    post('user', data)
        .then(result => {
            if(result.status != 200){
                throw Error(result);
            }
            return result;
        })
        .then(dataSent => {
            console.log(dataSent);
        })
        .catch(error => {
            console.error('Error:', error);
        });
}