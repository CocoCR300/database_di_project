<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class Payment extends Model
{
    protected $table = 'payment';
    
    public $timestamps = false;
    protected $fillable = [
        'booking_id',
        'date',
        'total_amount'
    ];
    public function booking()
    {
        return $this->belongsTo(Booking::class);
    }

}
