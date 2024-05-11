<?php

namespace App\Http\Controllers;

use App\Models\Lodging;
use Illuminate\Http\Request;
use App\Models\Room;
use App\Utils\JsonResponses;

class RoomController
{
    public function index($lodgingId)
    {
        $lodging = Lodging::find($lodgingId);
        if (!$lodging) {
            return JsonResponses::notFound("No existe un alojamiento con el identificador especificado");
        }

        $data = Room::where('lodgingId', $lodgingId)->get();
        return JsonResponses::ok(
            "Todos los registros de las habitaciones del alojamiento",
            $data,
        );
    }

    public function store(Request $request)
    {
        $data_input = $request->input('data', null);
        $lodgingId = $request->route('lodging_id');

        if ($data_input) {
            $data = json_decode($data_input, true);

            $rules = [
                'roomnumber' => 'required|integer',
                'occupied' => 'boolean',
                'pernightprice' => 'required|decimal:2,6',
                'capacity' => 'required|integer'
            ];
            $validation= \validator($data, $rules);
            if (Lodging::find($lodgingId) && !$validation->fails()) {

                $occupied = false;
                if (array_key_exists('occupied', $data)) {
                    $occupied = $data['occupied'];
                }

                $room = new Room();
                $room->lodgingId = $lodgingId;
                $room->roomNumber = $data['roomnumber'];
                $room->occupied = $occupied;
                $room->perNightPrice = $data['pernightprice'];
                $room->capacity = $data['capacity'];
                $room->save();
                $response = JsonResponses::created(
                    'Habitación creada para el alojamiento',
                    'room',
                    $room
                );
            } else {
                $response = JsonResponses::notAcceptable(
                    'Datos inválidos',
                    'errors',
                    $validation->errors()
                );
            }
        } else {
            $response = JsonResponses::badRequest('No se especificó el objeto "data" en la solicitud');
        }
        return $response;
    }

    public function show($lodgingId, $roomNumber)
    {
        $data = Room::find([ $lodgingId, $roomNumber ]);
        if (is_object($data)) {
            $data = $data->load('lodging');

            $response = JsonResponses::ok(
                'Datos de la habitación',
                $data,
                'room'
            );
        } else {
            $response = JsonResponses::notFound(
                'No existe una habitación con el número especificado'
            );
        }
        return $response;
    }

    public function destroy($lodgingId, $roomNumber = null)
    {
        if (isset($roomNumber)) {
            $deleted = Room::find([ $lodgingId, $roomNumber ])->delete();

            if ($deleted) {
                $response = JsonResponses::ok('Habitación eliminada');
            } else {
                $response = JsonResponses::badRequest(
                    'No existe una habitación con el número especificado'
                );
            }
        } else {
            $response = JsonResponses::notAcceptable(
                'No se especificó el número de habitación a eliminar'
            );
        }
        return $response;
    }
}
