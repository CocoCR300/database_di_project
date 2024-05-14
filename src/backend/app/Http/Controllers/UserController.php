<?php

namespace App\Http\Controllers;

use App\Models\Booking;
use App\Models\Person;
use App\Models\PhoneNumber;
use App\Models\User;
use App\Utils\Data;
use App\Utils\JsonResponses;
use Illuminate\Http\Request;
use Illuminate\Support\Arr;
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
            $data = Data::trimValues($data);

            $rules = [
                'username' => 'required|alpha_num|max:50',
                'firstname' => 'required|string|max:50',
                'lastname' => 'required|string|max:50',
                'password' => 'required|alpha_num|max:50',
                'roleid' => 'numeric|exists:userrole,userRoleId',
                'emailaddress' => 'required|email|unique:person|max:200',
                'phonenumbers' => 'array'
            ];
            $validation = Validator::make($data, $rules);
            if (!$validation->fails()) {
                $user = new User();
                $user->userName = $data['username'];
                $user->password = hash('SHA256', $data['password']);
                $user->userRoleId = $data['roleid'];
                $user->save();

                $person = new Person();
                $person->userName = $data['username'];
                $person->firstName = $data['firstname'];
                $person->lastName = $data['lastname'];
                $person->emailaddress = $data['emailaddress'];
                $person->save();

                $phoneNumbers = [];
                foreach ($data['phonenumbers'] as $number) {
                    array_push($phoneNumbers, array(
                        'personId'  => $person->personId,
                        'phoneNumber'    => $number
                    ));
                }
                PhoneNumber::insert($phoneNumbers);

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

    public function storePhoneNumber(Request $request)
    {
        $userName = $request->route('name');
        $phoneNumbersAsJson = $request->input('phone_numbers');

        if ($phoneNumbersAsJson) {
            $phoneNumbers = json_decode($phoneNumbersAsJson);
            $person = Person::where('userName', $userName)->with('phoneNumbers')->first();

            if ($person) {
                if (empty($phoneNumbers)) {
                    $response = JsonResponses::badRequest('El objeto "phone_numbers" está vacío');
                }
                else {
                    $phoneNumbers = array_udiff($phoneNumbers, $person->phoneNumbers->toArray(),
                        function($item1, $item2) {
                            return $item1 == $item2;
                        });
                    
                    if (!empty($phoneNumbers)) {
                        $phoneNumbersToAdd = Arr::map($phoneNumbers, function($value) use ($person) {
                            return array(
                                'personId' => $person->personId,
                                'phoneNumber' => $value
                            );
                        });

                        PhoneNumber::insert($phoneNumbersToAdd);
                        $response = JsonResponses::ok('Los números de teléfono ha sido agregados con éxito');
                    }
                    else {
                        $response = JsonResponses::ok('Los números de teléfono ya están asociados al usuario');
                    }
                }
            }
            else {
                $response = JsonResponses::notFound("No existe un usuario con el nombre especificado");
            }
        }
        else {
            $response = JsonResponses::badRequest('No se especificó el objeto "phone_numbers" en la solicitud');
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
                $phoneNumbers = PhoneNumber::where('personId', $person->personId);
                foreach ($phoneNumbers as $phoneNumber) {
                    $phoneNumber->delete();
                }

                $person ->delete();
                $user->delete();

                $response = JsonResponses::ok(
                    'El usuario y la información de la persona asociada han sido eliminados con éxito.',
                );
            }
        }

        return $response;
    }

    public function destroyPhoneNumber(Request $request)
    {
        $userName = $request->route('name');
        $phoneNumbersAsJson = $request->input('phone_numbers');

        if ($phoneNumbersAsJson) {
            if (empty($phoneNumbers)) {
                $response = JsonResponses::badRequest('El objeto "phone_numbers" está vacío');
            }
            else {
                $phoneNumbers = json_decode($phoneNumbersAsJson);
                $person = Person::where('userName', $userName)->first();

                if ($person) {
                    $deleted= PhoneNumber::where('personId', $person->personId)
                        ->whereIn('phoneNumber', $phoneNumbers)
                        ->delete();
                    if ($deleted) {
                        $response = JsonResponses::ok('Los números de teléfono ha sido eliminados con éxito');
                    }
                    else {
                        $response = JsonResponses::notFound('El usuario especificado no posee los números de teléfono a eliminar');
                    }
                }
                else {
                    $response = JsonResponses::notFound("No existe un usuario con el nombre especificado");
                }
            }
        }
        else {
            $response = JsonResponses::badRequest('No se especificó el objeto "phone_numbers" en la solicitud');
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
                $response = JsonResponses::notFound('No se pudo encontrar la persona asociada al usuario');
            }
            else {
                $data = $request->only(['username', 'firstname', 'lastname', 'emailaddress']);
    
                if (empty($data)) {
                    return JsonResponses::badRequest('No se proporcionaron datos para actualizar');
                }

                $data = array_map('trim', $data);
                $rules = [
                    'username' => 'alpha_num|max:50',
                    'firstname' => 'string|max:50',
                    'lastname' => 'string|max:50',
                    'emailaddress' => 'email|unique:person|max:200',
                ];
                $validation = validator($data, $rules);
                if (!$validation->fails()) {
                    $response = JsonResponses::notAcceptable(
                        'Datos inválidos',
                        'errors',
                        $validation->errors()
                    );
                }
                else if ($userName != $data['username'] && Person::find($data['username'])) {
                    $response = JsonResponses::notAcceptable('Ya existe un usuario con el nombre especificado');
                }
                else {
                    $person->fill($data);
                    $person->save();

                    $response = JsonResponses::ok('Datos del usuario y persona asociada actualizados correctamente');
                }
            }
        }
        return $response;
    }
}
