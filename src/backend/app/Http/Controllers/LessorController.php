<?php

namespace App\Http\Controllers;

use App\Models\Person;
use App\Models\UserRole;
use Illuminate\Http\Request;
use App\Utils\JsonResponses;

class LessorController
{
    /**
     * Metodo GET
     */
    public function index()
    {
        $data = Person::where('roleId', UserRole::LESSOR_USER_ROLE_ID);
        return JsonResponses::ok(
            "Todos los registros de arrendadores",
            $data
        );
    }

    /**
     * Metodo POST
     */
    public function store(Request $request)
    {
        $data_input = $request->input('data', null);
        if ($data_input) {
            $data = json_decode($data_input, true);
            $data = array_map('trim', $data);
            $rules = [
                'user_name' => 'required|alpha|exist:user',
                'first_name' => 'required|alpha',
                'last_name' => 'required|alpha',
                'phone_number' => 'required|numeric',
                'email_address' => 'required|alpha|email:rfc,dns'
            ];
            $isValid = \validator($data, $rules);
            if (!$isValid->fails()) {
                $lessor = new Person();
                $lessor->userName = $data('user_name');
                $lessor->firstName = $data('first_name');
                $lessor->lastName = $data('last_name');
                $lessor->phoneNumber = $data('phone_number');
                $lessor->emailAddress = $data('email_address');
                $lessor->save();
                $response = JsonResponses::created(
                    'Arrendador creado',
                    'lessor',
                    $lessor
                );
            } else {
                $response = JsonResponses::notAcceptable(
                    'Datos inválidos',
                    'errors',
                    $isValid->errors()
                );
            }
        } else {
            $response = JsonResponses::badRequest('No se encontró el objeto data');
        }

        return $response;
    }

    public function show($id)
    {
        $data = Person::find($id);
        if (is_object($data)) {
            $data = $data->load('user');
            $response = JsonResponses::ok(
                'Datos del arrendador',
                $data,
                'lessor'
            );
        } else {
            $response = JsonResponses::notFound('Recurso no encontrado');
        }
        return $response;
    }

    public function destroy($id = null)
    {
        if (isset($id)) {
            $deleted = Person::where('personId', $id)->delete();
            if ($deleted) {
                $response = JsonResponses::ok('Arrendante eliminado');
            } else {
                $response = JsonResponses::badRequest(
                    'No se pudo eliminar el recurso, compruebe que exista'
                );
            }
        } else {
            $response = JsonResponses::notAcceptable(
                'Falta el identificador del recurso a eliminar'
            );
        }
        return $response;
    }
}
