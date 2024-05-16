async function getUserData()
{
	try {
		const response = await fetch('http://localhost:8000/api/v1/user');
		if (!response.ok) {
			throw new Error('Hubo un problema con la solicitud.');
		}
		const data = await response.json();
		return data;
	} catch (error) {
		console.error('Se produjo un error:', error);
		return null;
	}
}

async function getAndPopulateData()
{
	try {
		const data = await getUserData();
		const table = document.getElementById("userDataTable");

		while (table.rows.length > 1) {
			table.deleteRow(1);
		}

		data.forEach(user => {
			const row = table.insertRow();
			row.innerHTML = `
                <td>${user.userName}</td>
                <td>${user.person.firstName}</td>
                <td>${user.person.lastName}</td>
                <td>${user.person.emailAddress}</td>
                <td>${user.person.phone_numbers[0] !== undefined ? user.person.phone_numbers[0].phoneNumber : "Sin números de teléfono"}</td>
                <td>${user.user_role.type}</td>
                <td><button class="fas fa-edit button" id= "btn-update" onclick="modificarUsuario('${user.userName}')"></button></td>
                <td><button class="fas fa-trash button" id= "btn-delete" onclick="eliminarUsuario('${user.userName}')"></button></td>
            `;
		});
	} catch (error) {
		console.error('Se produjo un error:', error);
	}
}
getAndPopulateData();