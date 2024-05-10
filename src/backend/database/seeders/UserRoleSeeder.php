<?php

namespace Database\Seeders;

use Illuminate\Database\Console\Seeds\WithoutModelEvents;
use Illuminate\Database\Seeder;
use App\Models\UserRole;

class UserRoleSeeder extends Seeder
{
    /**
     * Run the database seeds.
     */
    public function run(): void
    {
        if(!UserRole::where('role_id', 1)->exists()){
            UserRole::create([
                'role_id' => 1,
                'type'=> 'administrador',
            ]);
        }
        if (!UserRole::where('role_id', 2)->exists()) {
            UserRole::create([
                'role_id' => 2,
                'type' => 'cliente',
            ]);
        }

        if (!UserRole::where('role_id', 3)->exists()) {
            UserRole::create([
                'role_id' => 3,
                'type' => 'arrendador',
            ]);
        }
    }
}
