$("#home-button").on('click',function(){routing('home')});
$("#create-user").on('click',function(){routing('create')});
$("#show-users").on('click',function(){routing('show')});

function routing(route){
    $('#main-container').load('views/'+route+'.html');
}