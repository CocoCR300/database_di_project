<?php

namespace App\Http\Controllers;

use App\Models\Booking;
use App\Models\Person;
use App\Models\User;
use App\Utils\JsonResponses;
use Illuminate\Http\Request;
use Illuminate\Support\Facades\Validator;

class UserController
{
    public function index()
    {
        $users = User::with(['userRole', 'person', 'person.phoneNumbers'])->get();
        return $users;
    }

    public function indexBooking($userName)
    {
        $person = Person::where('userName', $userName)->first();

        if (!$person) {
            return JsonResponses::notFound('No existe un usuario con el identificador especificado');
        }

        $data = Booking::where('customerPersonId', $person->personId);
        return JsonResponses::ok(
            'Todos los registros de las reservas asociadas al usuario especificado',
            $data
        );
    }

    public function store(Request $request)
    {
        $data_input = $request->input('data', null);

        if ($data_input) {
            $data = json_decode($data_input, true);
            $data = array_map('trim', $data);
            $rules = [
                'username' => 'required|alpha_num|max:50',
                'firstname' => 'required|string|max:50',
                'lastname' => 'required|string|max:50',
                'password' => 'required|alpha_num|max:50',
                'roleid' => 'numeric|between:0,3',
                'emailaddress' => 'required|email|unique:person|max:200'
            ];
            $validation = Validator::make($data, $rules);
            if (!$validation->fails()) {
                $user = new User();
                $user->userName = $data['name'];
                $user->password = hash('SHA256', $data['password']);
                $user->roleId = $data['roleid'];
                $user->save();

                $person = new Person();
                $person->userName = $data['name'];
                $person->firstName = $data['firstname'];
                $person->lastName = $data['lastname'];
                $person->emailaddress = $data['emailaddress'];
                $person->save();

                $response = JsonResponses::ok('El usuario ha sido creado con éxito');
            } else {
                $response = JsonResponses::notAcceptable(
                    'Error al ingresar los datos',
                    'errors',
                    $validation->errors()
                );
            }
        } else {
            $response = JsonResponses::badRequest('No se especificó el objeto "data" en la solicitud');
        }

        return $response;
    }

    public function show($userName)
    {
        $data = User::find($userName);

        if (is_object($data)) {
            $data = $data->load(['person', 'userRole', 'person.phoneNumbers']);

            $response = JsonResponses::ok(
                'Datos del usuario',
                $data,
                'user'
            );
        } else {
            $response = JsonResponses::notFound(
                'No existe una usuario con el identificador especificado'
            );
        }
        return $response;
    }

    public function destroy($userName = null)
    {
        if (!isset($userName)) {
            return JsonResponses::notAcceptable(
                'No se especificó el identificador del usuario a eliminar'
            );
        }

        $user = User::find($userName);

        if (!$user) {
            $response = JsonResponses::notFound(
                'No existe un usuario con el identificador especificado'
            );
        } else {
            $person = Person::where('userName', $user->userName)->first();

            if (!$person) {
                $response = JsonResponses::notFound(
                    'No se encuentra la información de la persona asociada al usuario',
                );
            } else {
                $person ->delete();
                $user->delete();

                $response = JsonResponses::ok(
                    'El usuario y la información de la persona asociada han asociado eliminados con éxito.',
                );
            }
        }

        return $response;
    }

    public function updatePartial(Request $request, $userName) {

        $user = User::find($userName);
    
        if (!$user) {
            $response = JsonResponses::notFound('No existe un usuario con el identificador especificado');
        }
        else {
            $person = Person::where('userName', $userName)->first();

            if (!$person) {
                $response = JsonResponses::notFound('No se pudo encontrar la persona asociada al usuario.');
            }
            else {
                $data = $request->only(['username', 'firstname', 'lastname', 'emailaddress']);
    
                if (empty($data)) {
                    $response = JsonResponses::badRequest('No se proporcionaron datos para actualizar.');
                }
                else {
                    $person->fill($data);
                    $person->save();

                    $response = JsonResponses::ok('Datos del usuario y persona asociada actualizados correctamente.');
                }
            }
        }
        return $response;
    }
}
