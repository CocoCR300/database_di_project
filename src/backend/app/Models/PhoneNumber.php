<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class PhoneNumber extends Model
{
    protected $table = 'phoneNumber';

    public $timestamps = false;

    protected $fillable = [
        'personId',
        'phoneNumber'
    ];

    public function person()
    {
        return $this->belongsTo(Person::class);
    }
}
