<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Factories\HasFactory;
use Illuminate\Database\Eloquent\Relations\HasOne;
use Illuminate\Foundation\Auth\User as Authenticatable;

class User extends Authenticatable
{
    use HasFactory;

    protected $table = 'user';

    protected $primaryKey = 'userName';
    
    public $timestamps = false;

    protected $fillable = [
        'userName',
        'userRoleId',
        'password'
    ];

    protected $hidden = [
        'password'
    ];

    public function userRole(): HasOne
    {
        return $this->hasOne(UserRole::class, 'roleId', 'roleId');
    }
}
