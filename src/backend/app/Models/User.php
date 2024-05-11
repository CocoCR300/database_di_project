<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Factories\HasFactory;
use Illuminate\Database\Eloquent\Relations\HasOne;
use Illuminate\Foundation\Auth\User as Authenticatable;

class User extends Authenticatable
{
    use HasFactory;

    // https://stackoverflow.com/a/34715309/21037183 
    public $incrementing = false;

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

    public function person(): HasOne
    {
        return $this->hasOne(Person::class, 'userName', 'userName');
    }

    public function userRole(): HasOne
    {
        return $this->hasOne(UserRole::class, 'userRoleId', 'userRoleId');
    }
}
