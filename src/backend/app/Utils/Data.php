<?php

namespace App\Utils;

use Illuminate\Support\Arr;

class Data
{
    public static function trimValues(array $array)
    {
        $newArray = Arr::map($array, function ($value, string $key) {
            // Arrays can't be trimmed, so exclude them
            if (is_array($value)) {
                return $value;
            }

            return trim($value);
        });

        return $newArray;
    }
}