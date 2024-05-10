<?php

namespace App\Http\Controllers;

use App\Models\Booking;
use Illuminate\Http\Request;
use App\Utils\JsonResponses;

class BookingController
{
    public function index()
    {
        $data = Booking::all();
        return JsonResponses::ok(
            'Todos los registros de las reservas',
            $data
        );
    }

    public function store(Request $request)
    {
        $data_input = $request->input('data', null);
        if ($data_input) {
            $data = json_decode($data_input, true);
            $rules = [
                'lodging_id' => 'required|exists:lodging',
                'customer_id' => 'required|exists:person',
                'status_id' => ['Created', 'Confirmed', 'Cancelled', 'Finished'],
                'start_date' => 'required|date_format:Y-m-d H:i',
                'end_date' => 'required|date_format:Y-m-d H:i'
            ];

            $isValid = \validator($data, $rules);
            if (!$isValid->fails()) {
                $booking = new Booking();
                $booking->lodgingId = $data['lodging_id'];
                $booking->customerPersonId = $data['customer_id'];
                $booking->statusId = $data['status_id'];
                $booking->startDate = $data['start_date'];
                $booking->endDate = $data['end_date'];
                $booking->save();
                $response = JsonResponses::created(
                    'Reserva creada',
                    'booking',
                    $booking
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
        $data = Booking::find($id);
        if (is_object($data)) {
            $data = $data->load('lodging');
            $data = $data->load('person');
            $response = JsonResponses::ok(
                'Datos de la reserva',
                $data,
                'booking'
            );
        } else {
            $response = JsonResponses::notFound('Recurso no encontrado');
        }
        return $response;
    }

    
    public function destroy($id = null)
    {
        if (isset($id)) {
            $deleted = Booking::where('bookingId', $id)->delete();
            if ($deleted) {
                $response = JsonResponses::ok('Reserva eliminada');
            } else {
                $response = JsonResponses::badRequest(
                    'No se pudo eliminar el recurso, compruebe que exista');
            }
        } else {
            $response = JsonResponses::notAcceptable(
                'Falta el identificador del recurso a eliminar');
        }
        return $response;
    }
}
