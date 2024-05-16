const API_URL = "http://localhost:8000/api/v1";

async function get(route)
{
    const promise = fetch(`${API_URL}/${route}`, {
        headers: {
            "Accept": "application/json"
        }
    });
    try {
        const response = await promise;
        const responseData = await response.json();
        return responseData;
    }
    catch (error) {
    }

    return null;
}

async function post(route, data)
{
    const promise = fetch(`${API_URL}/${route}`, {
        method: "POST",
        headers: {
            "Accept": "application/json",
            "Content-Type": "application/json"
        },
        body: JSON.stringify(data)
    });
    try {
        const response = await promise;
        const responseData = await response.json();
        return responseData;
    }
    catch (error) {
        console.log(error);
    }

    return null;
}