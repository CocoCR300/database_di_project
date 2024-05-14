<?php

namespace App\Utils;

use Illuminate\Support\Arr;

class Data
{
    const HASHING_ALGO = 'SHA256';
    public static function hash(string $data)
    {
        return hash(self::HASHING_ALGO, $data);
    }

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