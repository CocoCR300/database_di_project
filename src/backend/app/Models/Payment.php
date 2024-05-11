<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class Payment extends Model
{
    protected $table = 'payment';
    
    protected $primaryKey = 'paymentId';
    
    public $timestamps = false;

    protected $fillable = [
        'bookingId',
        'dateAndTime'
    ];

    public function booking()
    {
        return $this->belongsTo(Booking::class);
    }
}
