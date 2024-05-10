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
            return JsonResponses::notFound("No se encontró el alojamiento");
        }

        $data = Room::where('lodgingId', $lodgingId);
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
                $room = new Room();

                if (array_key_exists('occupied', $data)) {
                    $occupied = $data['occupied'];
                }

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
            $response = JsonResponses::badRequest('No se encontró el objeto data');
        }
        return $response;
    }

    public function show($lodgingId, $roomNumber)
    {
        $data = Room::find([
            'lodgingId' => $lodgingId,
            'roomNumber' => $roomNumber
            ]);
        if (is_object($data)) {
            $data = $data->load('lodging');

            $response = JsonResponses::ok(
                'Datos de la habitación',
                $data,
                'room'
            );
        } else {
            $response = JsonResponses::notFound(
                'Recurso no encontrado'
            );
        }
        return $response;
    }

    public function destroy($lodgingId = null, $roomNumber = null)
    {
        if (isset($id)) {
            $deleted = Room::find([
                'lodgingId' => $lodgingId,
                'roomNumber' => $roomNumber
            ])->delete();
            if ($deleted) {
                $response = JsonResponses::ok('Habitación eliminada');
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
