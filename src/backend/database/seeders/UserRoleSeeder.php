<?php

namespace Database\Seeders;

use Illuminate\Database\Seeder;
use App\Models\UserRole;

class UserRoleSeeder extends Seeder
{
    public function run(): void
    {
        if(!UserRole::where('userRoleId', 1)->exists()){
            UserRole::create([
                'userRoleId' => UserRole::ADMINISTRATOR_USER_ROLE_ID,
                'type'=> 'Administrator',
            ]);
        }
        if (!UserRole::where('userRoleId', 2)->exists()) {
            UserRole::create([
                'userRoleId' => UserRole::CUSTOMER_USER_ROLE_ID,
                'type' => 'Customer',
            ]);
        }

        if (!UserRole::where('userRoleId', 3)->exists()) {
            UserRole::create([
                'userRoleId' => UserRole::LESSOR_USER_ROLE_ID,
                'type' => 'Lessor',
            ]);
        }
    }
}
