<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class Booking extends Model
{
    protected $table = 'booking';

    protected $primaryKey='booking_id';

    public $timestamps=false;

    protected $fillable = [
        'lodging_id',
        'customer_id',
        'status_id',
        'start_date',
        'end_date'
    ];

    // Esto son relaciones dentro de la base de datos

    public function lodging()
    {
        return $this->belongsTo(Lodging::class);
    }


    public function customer()
    {
        return $this->belongsTo(Customer::class);
    }


    public function BookingStatus()
    {
        return $this->belongsTo(BookingStatus::class);
    }
}
