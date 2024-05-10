<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class Lodging extends Model
{
    protected $table = 'lodging';

    protected $primaryKey = 'lodging_id';

    public $timestamps = false;

    protected $fillable = [
        'lessor_id',
        'name',
        'description',
        'address',
        'per_night_price',
        'available_rooms'
    ];

    public function lessor(){
        return $this->belongsTo(Lessor::class);
    }
}
