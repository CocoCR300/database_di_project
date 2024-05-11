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
                'name' => 'required',
                'password' => 'required',
                'role_id' => 'numeric|between:0,3',
                'email_address' => 'required|email|unique:user'
            ];
            $isValid = Validator::make($data, $rules);
            if (!$isValid->fails()) {
                $user = new User();
                $user->userName = $data['name'];
                $user->password = hash('sha256', $data['password']);
                $user->roleId = $data['role_id'];
                $user->save();

                $person = new Person();
                $person->userName = $data['name'];
                $person->firstName = $data['first_name'];
                $person->lastName = $data['last_name'];
                $person->emailddress = $data['email_address'];
                $person->save();

                $response = JsonResponses::ok('El usuario ha sido creado con éxito');
            } else {
                $response = JsonResponses::notAcceptable(
                    'Error al ingresar los datos',
                    'errors',
                    $isValid->errors()
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

    public function updatePartial(Request $request, $name) {

        $user = User::where('userName', $name)->first();
    
        if (!$user) {
            $response = [
                'message' => 'El usuario no existe.',
                'status' => 404
            ];
        } else {
            $model = Person::where('userName', $user->userName)->first();

            if (!$model) {
                $response = [
                    'message' => 'No se pudo encontrar el modelo asociado al usuario.',
                    'status' => 404
                ];
            } else {
                $data = $request->only(['user_name', 'first_name', 'last_name', 'phone_number', 'email_address']);
    
                if (empty($data)) {
                    $response = [
                        'message' => 'No se proporcionaron datos para actualizar.',
                        'status' => 400
                    ];
                } else {
                    $model->fill($data);
                    $model->save();

                    $response = [
                        'message' => 'Usuario y modelo asociado actualizados correctamente.',
                        'status' => 200
                    ];
                }
            }
        }
        return response()->json($response, $response['status']);
    }
}
