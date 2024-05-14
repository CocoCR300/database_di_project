<?php

namespace Database\Factories;

use App\Models\UserRole;
use Illuminate\Database\Eloquent\Factories\Factory;
use Illuminate\Support\Str;

/**
 * @extends \Illuminate\Database\Eloquent\Factories\Factory<\App\Models\User>
 */
class UserFactory extends Factory
{
    protected static ?string $password;

    public function definition(): array
    {
        return [
            'userName' => fake()->name(),
            'password' => 'password',
            'userRoleId' => fake()->randomElement([
                UserRole::ADMINISTRATOR_USER_ROLE_ID,
                UserRole::CUSTOMER_USER_ROLE_ID,
                UserRole::LESSOR_USER_ROLE_ID
            ]),
            'remember_token' => Str::random(10)
        ];
    }
}
