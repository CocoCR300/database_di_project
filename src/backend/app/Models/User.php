<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Relations\HasOne;
use Illuminate\Foundation\Auth\User as Authenticatable;

class User extends Authenticatable
{
    protected $table = 'user';

    protected $primaryKey = 'name';
    
    public $timestamps = false;

    protected $fillable = [
        'name',
        'role_id',
        'password',
        'email_address'
    ];

    protected $hidden = [
        'password'
    ];

    public function userRole(): HasOne
    {
    return $this->hasOne(UserRole::class, 'role_id', 'role_id');
    }

}
