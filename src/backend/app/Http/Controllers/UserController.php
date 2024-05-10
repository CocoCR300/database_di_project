<?php

namespace App\Http\Controllers;

use App\Models\Administrator;
use App\Models\Customer;
use App\Models\Lessor;
use App\Models\User;
use Illuminate\Http\Request;
use Illuminate\Support\Facades\Validator;

class UserController extends Controller
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
            $isValid = \Validator::make($data, $rules);
            if (!$isValid->fails()) {
                $user = new User();
                $user->name = $data['name'];
                $user->password = hash('sha256', $data['password']);
                $user->role_id = $data['role_id'];
                $user->email_address = $data['email_address'];
                $user->save();

                if ($data['role_id'] == 1) {
                    $administrator = new Administrator();
                    $administrator->user_name = $data['name'];
                    $administrator->first_name = $data['first_name'];
                    $administrator->last_name = $data['last_name'];
                    $administrator->phone_number = $data['phone_number'];
                    $administrator->save();
                } elseif ($data['role_id'] == 2) {
                    $customer = new Customer();
                    $customer->user_name = $data['name'];
                    $customer->first_name = $data['first_name'];
                    $customer->last_name = $data['last_name'];
                    $customer->phone_number = $data['phone_number'];
                    $customer->save();
                } elseif ($data['role_id'] == 3) {
                    $lessor = new Lessor();
                    $lessor->user_name = $data['name'];
                    $lessor->first_name = $data['first_name'];
                    $lessor->last_name = $data['last_name'];
                    $lessor->phone_number = $data['phone_number'];
                    $lessor->save();
                }

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
            switch ($user->role_id) {
                case 1:
                    $model = Administrator::where('user_name', $user->name)->first();
                    break;
                case 2:
                    $model = Customer::where('user_name', $user->name)->first();
                    break;
                case 3:
                    $model = Lessor::where('user_name', $user->name)->first();
                    break;
                default:
                    $model = null;
                    break;
            }
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

        $user = User::where('name', $name)->first();
    
        if (!$user) {
            $response = [
                'message' => 'El usuario no existe.',
                'status' => 404
            ];
        } else {
            switch ($user->role_id) {
                case 1:
                    $model = Administrator::where('user_name', $user->name)->first();
                    break;
                case 2:
                    $model = Customer::where('user_name', $user->name)->first();
                    break;
                case 3:
                    $model = Lessor::where('user_name', $user->name)->first();
                    break;
                default:
                    $model = null;
                    break;
            }

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
