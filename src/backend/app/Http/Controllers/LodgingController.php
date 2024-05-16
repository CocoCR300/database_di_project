<?php

namespace App\Http\Controllers;

use App\Models\Booking;
use App\Models\Lodging;
use App\Utils\Data;
use Illuminate\Http\Request;
use App\Utils\JsonResponses;

class LodgingController
{
    public function index()
    {
        $data = Lodging::with('ownerPerson')->get();
        return JsonResponses::ok(
            "Todos los registros de los alojamientos",
            $data,
        );
    }

    public function indexBooking($lodgingId)
    {
        $lodging= Lodging::where('lodgingId', $lodgingId)->first();

        if (!$lodging) {
            return JsonResponses::notFound('No existe un alojamiento con el identificador especificado');
        }

        $data = Booking::where('lodgingId', $lodging->lodgingId);
        return JsonResponses::ok(
            'Todos los registros de las reservas asociadas al alojamiento especificado',
            $data
        );
    }

    public function store(Request $request)
    {
        $data_input = $request->input('data', null);
        if ($data_input) {
            $data = Data::decodeJson($data_input);
            $rules = [
                'ownerpersonid' => 'required|exists:person,personId',
                'name' => 'required|string|max:100',
                'lodgingtype' => [
                    Lodging::LODGING_TYPE_APARTMENT,
                    Lodging::LODGING_TYPE_CABIN,
                    Lodging::LODGING_TYPE_HOTEL,
                    Lodging::LODGING_TYPE_SUMMER_HOUSE,
                ],
                'description' => 'required|string:1000',
                'address' => 'required|string|max:300',
            ];
            $isValid = \validator($data, $rules);
            if (!$isValid->fails()) {
                $lodging = new Lodging();
                $lodging->ownerPersonId= $data['ownerpersonid'];
                $lodging->name = $data['name'];
                $lodging->description = $data['description'];
                $lodging->address = $data['address'];
                $lodging->save();
                $response = JsonResponses::created(
                    'Alojamiento creado',
                    'lodging',
                    $lodging
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
        $data = Lodging::find($id);
        if (is_object($data)) {
            $data = $data->load('ownerPerson');

            $response = JsonResponses::ok(
                'Datos del alojamiento',
                $data,
                'lodging'
            );
        } else {
            $response = JsonResponses::notFound(
                'Recurso no encontrado'
            );
        }
        return $response;
    }

    public function destroy($id = null)
    {
        if (isset($id)) {
            $deleted = Lodging::where('lodgingId', $id)->delete();
            if ($deleted) {
                $response = JsonResponses::ok('Alojamiento eliminado');
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
