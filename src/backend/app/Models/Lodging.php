<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Factories\HasFactory;
use Illuminate\Database\Eloquent\Model;

class Lodging extends Model
{
    use HasFactory;

    const LODGING_TYPE_APARTMENT = 'Apartment';
    const LODGING_TYPE_CABIN = 'Cabin';
    const LODGING_TYPE_HOTEL = 'Hotel';
    const LODGING_TYPE_SUMMER_HOUSE = 'Summer house';

    protected $table = 'lodging';

    protected $primaryKey = 'lodgingId';

    public $timestamps = false;

    protected $fillable = [
        'ownerPersonId',
        'name',
        'description',
        'lodgingType',
        'address',
    ];

    public function ownerPerson() {
        return $this->belongsTo(Person::class, 'ownerPersonId');
    }
}
