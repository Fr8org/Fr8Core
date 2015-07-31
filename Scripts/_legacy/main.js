function EasyPeasyParallax() {
	var	windowWidth = $(window).width();
	if(windowWidth > 980){
		scrollPos = $(this).scrollTop();
		$('#welcome').css({
			'background-position' : '50%' + (-scrollPos/4)+"px"
		});
		$('.landing-welcome-area').css({
			'top': ($('#welcome').height - $('.navbar-fixed-top') - $(this).height)/2,
			'margin-top': (scrollPos/4)+"px",
			'opacity': 1-(scrollPos/250)
		});
		$('#about').css({
		    'top': ($('#welcome').height - $('.navbar-fixed-top') - $(this).height) / 2,
		    'margin-top': (scrollPos / 4) - 80 + "px",
		    'opacity': 1 - (scrollPos / 250)
		});
		var opacityValue = $('.landing-welcome-area, #about').css('opacity');
		if(opacityValue == 0){
		    $('.landing-welcome-area, #about').hide();
		}else{
		    $('.landing-welcome-area, #about').show();
		}
	}	
}
function someResize(){
	EasyPeasyParallax();
	var	windowWidth = $(window).width(),
		maxHeight = $('.near-big').height() - 42
	if(windowWidth > 768){
		$('.big-preview .thumbnail').css('height', maxHeight);
	}
	else{
		$('.big-preview .thumbnail').css('height', 'auto');
	}	
	if($('.colorbox').length){
		$('a.colorbox').colorbox({
			rel:'gal',
			retinaImage: true,
			opacity: 1,
			current: false,
			maxWidth: '95%',
			maxHeight: '95%'
		})
	} 
}
$(document).ready(function () {

    $('body').bind('beforeunload', function () {
        localStorage.setItem("navbarLnk", null);
    });

    if ($('.video-frame').length) {
        var iframe = $('.video-frame')[0];
        var player = $f(iframe);
    }       

	if($('input, textarea').length) {
		$('input, textarea').placeholder();
	}
	
	someResize();
	
	$('a[rel="popover"]').popover();
	$('a[rel="tooltip"]').tooltip();
	$('.carousel').carousel({
		interval: false
	})
	
	/*menu*/
	
	var links = $('.navbar').find('li[data-section]'),
		section = $('.text-block'),
		button = $('.toSection'),
		mywindow = $(window),
		htmlbody = $('html,body'),
		offsetTop = $('.navbar').height();
		if ($('.home-welcome').length) {
			links.push($('.home-welcome')[0]);
		}
		if ($('a.try-kwasant-lnk').length) {
		    links.push($('a.try-kwasant-lnk'));
		}

		if (localStorage.getItem("navbarLnk") > 0) {
		    goToByScroll(localStorage.getItem("navbarLnk"));
		    localStorage.setItem("navbarLnk", null);
		}

	section.waypoint({
		 handler: function (direction) {
			var datasection = $(this).attr('data-section');
			if (direction === 'down') {
				$('.navbar li[data-section="' + datasection + '"]').addClass('active').siblings().removeClass('active');
			}
			else {
				var newsection = parseInt(datasection) - 1;
				$('.navbar li[data-section="' + newsection + '"]').addClass('active').siblings().removeClass('active');
			}
		}, 
		offset: offsetTop + 5
	});
	mywindow.scroll(function () {
		if (mywindow.scrollTop() < $('#welcome').height()- offsetTop - 5) {
			$('.navbar li[data-section="1"]').removeClass('active');
		}
	});
	function goToByScroll(datasection) {
		htmlbody.animate({
			scrollTop: $('.text-block[data-section="' + datasection + '"]').offset().top - offsetTop
		}, 1000);
	}
	var	windowWidth = $(window).width();
	if(windowWidth < 980){
	    function goToByScroll(datasection) {
			htmlbody.animate({
				scrollTop: $('.text-block[data-section="' + datasection + '"]').offset().top - offsetTop
			}, 1000);
		}
	}

	links.click(function (e) {
	    // alert('Hellolinks');
	    if (player) {
	        player.api('pause');
	    } 
	    var datasection = $(this).attr('data-section');
	    //if (datasection.get(0) === null)
	    //    return;

	    e.preventDefault();
	    goToByScroll(datasection);
	});

	button.click(function (e) {
	    //alert('Hellobutton');
		e.preventDefault();
		var datasection = $(this).attr('data-section');
		goToByScroll(datasection);
	}); 

	
	$(window).scroll(function() {
		EasyPeasyParallax();
		if ($(this).scrollTop() > 300) {
			$('.goTop-link').fadeIn();
		} else {
			$('.goTop-link').fadeOut();
		}
	});
	
	$('.goTop').on('click', function(){
		$('body,html').animate({scrollTop: 0}, 1000);  		
	}); 	
	
	$('img.video-screen').click(function (e) {
		e.stopPropagation();
		player.api('play');
		$('.video-frame').css('visibility', 'visible');
		$(this).fadeOut();
	});
	
});

$(window).resize(function(){
	someResize();
})