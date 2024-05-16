async function getUserData() {
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
