<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class RoomBooking extends Model
{
    protected $table = 'roomBooking';

    protected $primaryKey = 'roomBookingId';
    
    public $timestamps = false;

    protected $fillable = [
        'roomNumber',
        'bookingId',
        'cost',
        'fees'
    ];

    public function booking()
    {
        return $this->belongsTo(Booking::class);
    }

    public function Room()
    {
        return $this->belongsTo(Room::class);
    }
}
