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
        return await response.json();
    }
    catch (error) {
        console.log(error);
    }

    return null;
}

async function patch(route, data) 
{
        const promise = fetch(`${API_URL}/${route}`, {
            method: "PATCH",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify(data)
        });
        try{
            const response = await promise;
            return await response;
        }
        catch (error) {
            console.error(error);
            throw error;
        }
}

async function destroy(route)
{
    const promise = fetch(`${API_URL}/${route}`, {
        method: "DELETE",
        body: JSON.stringify(null)
    });
    try{
        const response = await promise;
        return await response;
    }catch(error){
        console.error(error);
        throw error;
    }
}