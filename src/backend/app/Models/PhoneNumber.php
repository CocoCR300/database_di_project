<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class PhoneNumber extends Model
{
    protected $table = 'phoneNumber';

    protected $primaryKey = 'phoneNumberId';
    
    public $timestamps = false;

    protected $fillable = [
        'personId',
        'number'
    ];

    public function person()
    {
        return $this->belongsTo(Person::class);
    }
}
