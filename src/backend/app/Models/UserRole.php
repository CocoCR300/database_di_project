<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class UserRole extends Model
{
    const ADMINISTRATOR_USER_ROLE_ID = 1;
    const CUSTOMER_USER_ROLE_ID = 2;
    const LESSOR_USER_ROLE_ID = 3;

    protected $table = 'userRole';
    
    public $timestamps = false;
    
    protected $fillable = [
        'roleId',
        'type'
    ];
    
}
