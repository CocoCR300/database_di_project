<?php

namespace Database\Seeders;

use App\Models\Lodging;
use App\Models\Person;
use App\Models\User;
use App\Models\UserRole;
use Illuminate\Database\Seeder;

class DatabaseSeeder extends Seeder
{
    public function run(): void
    {
        $this->call(UserRoleSeeder::class);

        $people = [];
        $adminUsers = User::factory()
            ->count(5)
            ->create([
                'userRoleId' => UserRole::ADMINISTRATOR_USER_ROLE_ID
            ])->toArray();

        $customerUsers = User::factory()
            ->count(5)
            ->create([
                'userRoleId' => UserRole::CUSTOMER_USER_ROLE_ID
            ])->toArray();

        $lessorUsers = User::factory()
            ->count(5)
            ->create([
                'userRoleId' => UserRole::LESSOR_USER_ROLE_ID
            ])->toArray();

        foreach (array_merge($adminUsers, $customerUsers, $lessorUsers)
                 as $user) {
            array_push($people, Person::factory()
                ->for($user['userName'])
                ->create());

            if ($user->userRoleId == UserRole::LESSOR_USER_ROLE_ID) {
                array_push($lodging, Lodging::factory()
                    ->for($user)
                    ->create());
            }
            //else ($user->userRoleId == UserRole::CUSTOMER_USER_ROLE_ID) {
                
            //}
        }
    }
}
