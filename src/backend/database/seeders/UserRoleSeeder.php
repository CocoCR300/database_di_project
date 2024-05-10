<?php

namespace Database\Seeders;

use Illuminate\Database\Seeder;
use App\Models\UserRole;

class UserRoleSeeder extends Seeder
{
    public function run(): void
    {
        if(!UserRole::where('roleId', 1)->exists()){
            UserRole::create([
                'roleId' => 1,
                'type'=> 'Administrator',
            ]);
        }
        if (!UserRole::where('roleId', 2)->exists()) {
            UserRole::create([
                'roleId' => 2,
                'type' => 'Customer',
            ]);
        }

        if (!UserRole::where('roleId', 3)->exists()) {
            UserRole::create([
                'roleId' => 3,
                'type' => 'Lessor',
            ]);
        }
    }
}
