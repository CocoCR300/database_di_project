import * as api from '../js/api.js'

async function refreshLodgings() {
    let lodgingsTable = $("#lodgings-table")
    let tableBody = lodgingsTable.children("tbody");

    const response = await api.get('lodging');

    for (const lodging of response.data) {
        let row = `
        <tr>
            <td>${lodging.name}</td>
            <td>${lodging.address}</td>
            <td>${lodging.description}</td>
            <td>${lodging.lodgingType.type}</td>
            <td>${lodging.ownerPersonId}</td>
        </tr>
        `;

        tableBody.append(row);
    }
}

$("#refresh-button").on('click', refreshLodgings);