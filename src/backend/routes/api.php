<?php

use App\Http\Controllers\BookingController;
use App\Http\Controllers\LessorController;
use App\Http\Controllers\LodgingController;
use App\Http\Controllers\UserController;
use Illuminate\Http\Request;
use Illuminate\Support\Facades\Route;


Route::prefix('v1')->group(
    function () {
        Route::post('/user', [UserController::class, 'store']);


        Route::get('/user', [UserController::class, 'index']);


        Route::apiResource('/booking', BookingController::class, ['except' => ['create', 'edit']]);
        Route::apiResource('/lessor', LessorController::class, ['except' => ['create', 'edit']]);
        Route::apiResource('/lodging', LodgingController::class, ['except' => ['create', 'edit']]);


        Route::delete('/booking', [BookingController::class, 'destroy']);
        Route::delete('/lessor', [LessorController::class, 'destroy']);
        Route::delete('/lodging', [LodgingController::class, 'destroy']);
        Route::delete('/user/{name}', [UserController::class,'destroy']);

        Route::patch('/user/{name}', [UserController::class,'updatePartial']);
    }
);
