function EasyPeasyParallax() {
	var	windowWidth = $(window).width();
	if(windowWidth > 980){
		scrollPos = $(this).scrollTop();
	}
}
function someResize(){
	var	windowWidth = $(window).width(),
		windowHeight = $(window).height(),
		maxHeight = $('.near-big').height() - 42,
		windowHeight = $(window).height(),
		headerHeight = $('header#site-header .navbar').innerHeight(),
		footerHeight = $('#site-footer').height(),
		sectionHeight = windowHeight,
		contentInnerHeight = $(".inner-wrap.cetered").height();
	$("section.full-height-block").css("min-height", sectionHeight);
	$(".inner-bg.full-size-bg").css("min-height", sectionHeight);
	$(".container.full-height-block").css("min-height", sectionHeight);

	$(".inner-wrap.cetered").each(function(){
		contentInnerHeight = $(this).height();
		if ($(this).parents("section#contacts").length > 0) {
			if (contentInnerHeight < (sectionHeight - headerHeight - footerHeight)) {
				var contentTop = headerHeight + ((windowHeight - (headerHeight + footerHeight))/2);
				var margitTop = 0 - ((contentInnerHeight/2) + (headerHeight/2));

				$(this).css({
					'top': (sectionHeight/2) + "px",
					'margin-top': margitTop + "px"
				});
			} else {
				console.log("headerHeight 1: " + $('header#site-header').length);
				$(this).css({
					"top": (headerHeight + 20) + "px",
					'margin-top': "0px"
				});
				$(this).parents("section#contacts").find(".inner-bg.full-size-bg").css("min-height", ($(this).height() + parseInt($(this).css('top'))));
				$(this).parents("section#contacts").css("padding-bottom", (parseInt($(this).css('top'))));
			}

		} else {

			if (contentInnerHeight < (sectionHeight-headerHeight)) {
				var contentTop = (sectionHeight - contentInnerHeight)/2;
				$(this).css("top", contentTop);
			} else {
				$(this).css("top", headerHeight + 20);
				$(this).parents("section").find(".inner-bg.full-size-bg").css("min-height", ($(this).height() + parseInt($(this).css('top'))));
				$(this).parents("section").css("padding-bottom", (parseInt($(this).css('top'))));
			}
		}
	});
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

	$("li.dropdown").hover(function(){
		$(this).find("a.dropdown-toggle").attr('aria-expanded', "true");
		$(this).addClass("open");
	},function() {
		$(this).find("a.dropdown-toggle").attr('aria-expanded', "false");
		$(this).removeClass("open");
	});

	var links = $('.navbar').find('li[data-section]'),
		section = $('.text-block'),
		button = $('.toSection'),
		mywindow = $(window),
		htmlbody = $('html,body'),
		offsetTop = $('.navbar').height(),
		headerHeight = $('#site-header').height();
		offsetTop = headerHeight;
		if ($('.home-welcome').length) {
			links.push($('.home-welcome')[0]);
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
		offset: offsetTop
	});

	$("section#welcome").click(function(){
		goToByScroll("2");
	});

	function goToByScroll(datasection) {
		htmlbody.animate({
			scrollTop: $('.text-block[data-section="' + datasection + '"]').offset().top
		}, 1000);
	}
	var	windowWidth = $(window).width();
	if(windowWidth < 980){
	    function goToByScroll(datasection) {
			htmlbody.animate({
				scrollTop: $('.text-block[data-section="' + datasection + '"]').offset().top
			}, 1000);
		}
	}

	links.click(function (e) {
	    var datasection = $(this).attr('data-section');

	    e.preventDefault();
	    goToByScroll(datasection);
	});

	button.click(function (e) {
		e.preventDefault();
		var datasection = $(this).attr('data-section');
		goToByScroll(datasection);
	}); 

	
	$(window).scroll(function() {
		if ($(this).scrollTop() > 300) {
			$('.goTop-link').fadeIn();
		} else {
			$('.goTop-link').fadeOut();
		}
	});
	
	$('.goTop').on('click', function(){
		$('body,html').animate({scrollTop: 0}, 1000);  		
	}); 	
	

});

$(window).resize(function(){
	someResize();
})