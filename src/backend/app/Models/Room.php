<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class Room extends Model
{
    protected $table = 'room';

    protected $primaryKey = ['lodgingId', 'roomNumber'];
    
    public $timestamps = false;

    protected $fillable = [
        'lodgingId',
        'roomNumber',
        'occupied',
        'perNightPrice',
        'capacity'
    ];

    public function lodging()
    {
        return $this->belongsTo(Lodging::class);
    }
}
