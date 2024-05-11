<?php

namespace App\Http\Controllers;

use App\Models\Booking;
use App\Models\Payment;
use Illuminate\Http\Request;
use App\Utils\JsonResponses;

class BookingController
{
    public function index()
    {
        $data = Booking::with('payment')->get();
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
                'lodgingid' => 'required|exists:lodging',
                'customerid' => 'required|exists:person',
                'status' => [
                    Booking::BOOKING_STATUS_CREATED,
                    Booking::BOOKING_STATUS_CONFIRMED,
                    Booking::BOOKING_STATUS_CANCELLED,
                    Booking::BOOKING_STATUS_FINISHED
                ],
                'startdate' => 'required|date_format:Y-m-d H:i|after_or_equal:today',
                'enddate' => 'required|date_format:Y-m-d H:i|after:startdate',
                'fees' => 'required|decimal:2,6|gt:0'
            ];

            $isValid = \validator($data, $rules);
            if (!$isValid->fails()) {
                $booking = new Booking();
                $booking->lodgingId = $data['lodgingid'];
                $booking->customerPersonId = $data['customerid'];
                $booking->statusId = $data['status'];
                $booking->startDate = $data['startdate'];
                $booking->endDate = $data['enddate'];
                $booking->fees = $data['fees'];
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

    public function storePayment(Request $request)
    {
        $data_input = $request->input('data', null);
        $bookingId = $request->route('booking_id');
        $booking = Booking::find($bookingId);

        if (!$booking) {
            return JsonResponses::notFound('No existe una reservación con el identificador especificado');
        }

        $payment = $booking->load('payment');
        if (!$payment) {
            return JsonResponses::notAcceptable('Ya existe un pago asociado a esta reservación');
        }

        if ($data_input) {
            $data = json_decode($data_input, true);
            $rules = [
                'dateandtime' => 'required|date_format:Y-m-d H:i|after_or_equal:today',
            ];

            $validation = \validator($data, $rules);
            if (!$validation->fails()) {
                $payment = new Payment();
                $payment->bookingId = $bookingId;
                $payment->dateAndTime = $data['dateandtime'];
                $payment->save();

                $booking->status = Booking::BOOKING_STATUS_FINISHED;
                $booking->save();

                $response = JsonResponses::created(
                    'Pago registrado con éxito',
                    'payment',
                    $payment
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

    public function show($id)
    {
        $data = Booking::find($id);
        if (is_object($data)) {
            $data = $data->load(['customer', 'lodging', 'payment']);
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
