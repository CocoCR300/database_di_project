<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class Booking extends Model
{
    protected $table = 'booking';

    protected $primaryKey = 'bookingId';

    public $timestamps = false;

    protected $fillable = [
        'lodgingId',
        'customerId',
        'statusId',
        'startDate',
        'endDate',
        'status'
    ];

    public function lodging()
    {
        return $this->belongsTo(Lodging::class);
    }

    public function customer()
    {
        return $this->belongsTo(Person::class);
    }
}
