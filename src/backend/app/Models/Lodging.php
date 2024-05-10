<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class Lodging extends Model
{
    protected $table = 'lodging';

    protected $primaryKey = 'lodgingId';

    public $timestamps = false;

    protected $fillable = [
        'lessorId',
        'name',
        'description',
        'lodgingType',
        'address',
    ];

    public function lessor() {
        return $this->belongsTo(Person::class);
    }
}
