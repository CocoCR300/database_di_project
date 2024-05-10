<?php

namespace Database\Factories;

use App\Models\Lodging;
use Illuminate\Database\Eloquent\Factories\Factory;

class LodgingFactory extends Factory
{
    public function definition(): array
    {
        return [
            'name' => fake()->company(),
            'lodgingType' => fake()->randomElement([
                Lodging::LODGING_TYPE_APARTMENT,
                Lodging::LODGING_TYPE_CABIN,
                Lodging::LODGING_TYPE_HOTEL,
                Lodging::LODGING_TYPE_SUMMER_HOUSE,
            ]),
            'address' => fake()->address()
        ];
    }
}
