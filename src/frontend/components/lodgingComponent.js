async function refreshLodgings() {
    let lodgingsTable = $("#lodgings-table")
    let tableBody = lodgingsTable.children("tbody");
    tableBody.children().not(":first").remove();

    const response = await get('lodging');

    for (const lodging of response.data) {
        let row = `
        <tr>
            <td>${lodging.name}</td>
            <td>${lodging.address}</td>
            <td>${lodging.description}</td>
            <td>${lodging.lodgingType}</td>
            <td>${lodging.owner_person.firstName + " " + lodging.owner_person.lastName}</td>
        </tr>
        `;

        tableBody.append(row);
    }
}

refreshLodgings();