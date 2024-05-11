<?php

use App\Http\Controllers\BookingController;
use App\Http\Controllers\LodgingController;
use App\Http\Controllers\RoomController;
use App\Http\Controllers\UserController;
use Illuminate\Support\Facades\Route;

Route::prefix('v1')->group(
    function () {
        Route::get('/lodging/{lodging_id}/booking', [LodgingController::class, 'indexBooking']);
        Route::get('/user/{name}/booking', [UserController::class, 'indexBooking']);

        Route::apiResource('/booking', BookingController::class, ['except' => ['create', 'edit']]);
        Route::apiResource('/lodging', LodgingController::class, ['except' => ['create', 'edit']]);
        Route::apiResource('/lodging/{lodging_id}/room', RoomController::class, ['except' => ['create', 'edit']]);
        Route::apiResource('/user', UserController::class, ['except' => ['create', 'edit']]);

        Route::delete('/booking', [BookingController::class, 'destroy']);
        Route::delete('/lodging', [LodgingController::class, 'destroy']);
        Route::delete('/lodging/{lodging_id}/room', [RoomController::class, 'destroy']);
        Route::delete('/user/{name}', [UserController::class, 'destroy']);

        Route::patch('/user/{name}', [UserController::class,'updatePartial']);
    }
);
