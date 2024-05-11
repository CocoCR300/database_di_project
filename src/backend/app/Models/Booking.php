<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class Booking extends Model
{
    const BOOKING_STATUS_CREATED    = 'Created';
    const BOOKING_STATUS_CONFIRMED  = 'Confirmed';
    const BOOKING_STATUS_CANCELLED  = 'Cancelled';
    const BOOKING_STATUS_FINISHED   = 'Finished';

    protected $table = 'booking';

    protected $primaryKey = 'bookingId';

    public $timestamps = false;

    protected $fillable = [
        'lodgingId',
        'customerId',
        'status',
        'startDate',
        'endDate',
        'fees'
    ];

    public function lodging()
    {
        return $this->belongsTo(Lodging::class, 'lodgingId');
    }

    public function customer()
    {
        return $this->belongsTo(Person::class, 'customerPersonId');
    }

    public function payment()
    {
        return $this->hasOne(Payment::class, 'bookingId');
    }
}
