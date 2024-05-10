<?php

namespace Database\Factories;

use Illuminate\Database\Eloquent\Factories\Factory;
use Illuminate\Support\Facades\Hash;
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
            'password' => static::$password ??= Hash::make('password'),
            'roleId' => fake()->rand(1, 3),
            'remember_token' => Str::random(10)
        ];
    }
}
