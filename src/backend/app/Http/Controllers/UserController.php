<?php

namespace App\Http\Controllers;

use App\Models\Administrator;
use App\Models\Customer;
use App\Models\Lessor;
use App\Models\Person;
use App\Models\User;
use Illuminate\Http\Request;
use Illuminate\Support\Facades\Validator;

class UserController
{
    public function index()
    {
        $users = User::with('userRole')->get();
        return $users;
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

                $response = [
                    'message' => 'El usuario se ha agregado correctamente.',
                    'status' => 200
                ];
            } else {
                $response = [
                    'message' => 'Error al ingresar los datos.',
                    'status' => 400
                ];
            }
        } else {
            $response = [
                'message' => 'Error al ingresar los datos.',
                'status' => 400
            ];
        }

        return response()->json($response, $response['status']);
    }
    public function destroy(Request $request, $id)
    {
        $user = User::find($id);

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
                $model->delete();
                $user->delete();

                $response = [
                    'message' => 'Usuario y modelo asociado eliminados exitosamente.',
                    'status' => 200
                ];
            }
        }

        return response()->json($response, $response['status']);
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
