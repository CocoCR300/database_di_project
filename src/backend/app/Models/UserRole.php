<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class UserRole extends Model
{
    protected $table = 'userRole';
    
    public $timestamps = false;
    
    protected $fillable = [
        'roleId',
        'type'
    ];
    
}
