<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Factories\HasFactory;
use Illuminate\Database\Eloquent\Model;
use Illuminate\Database\Eloquent\Relations\HasMany;

class Person extends Model
{
    use HasFactory;

    protected $table = 'person';

    protected $primaryKey = 'personId';
    
    public $timestamps = false;

    protected $fillable = [
        'userName',
        'firstName',
        'lastName',
        'emailAddress'
    ];

    public function user()
    {
        return $this->belongsTo(User::class);
    }

    public function phoneNumbers(): HasMany
    {
        return $this->hasMany(PhoneNumber::class, 'personId');
    }
}
