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
	$("section.full-height-block").css({
		"min-height": sectionHeight
	});

	if ($("section.full-height-block:last-child")) {
		$("section.full-height-block:last-child .container.full-height-block").css({
			"padding-bottom": $("#site-footer").height(),
			"margin-bottom": 0 - $("#site-footer").height()
		});
	}

	if (($("section.full-height-block").length == 1)) {
		$("section.full-height-block .container.full-height-block").css({
			"padding-bottom": $("#site-footer").height(),
			"margin-bottom": 0 - $("#site-footer").height()
		});
	}

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
				$(this).css({
					"top": (headerHeight + 20) + "px",
					'margin-top': "0px"
				});
				$(this).parents("section#contacts").find(".inner-bg.full-size-bg").css("min-height", ($(this).height() + parseInt($(this).css('top'))));
				$(this).parents("section#contacts").css("padding-bottom", (parseInt($(this).css('top'))));
			}
		} else if (($("section.full-height-block").length == 1)) {
			if (contentInnerHeight < (sectionHeight - headerHeight - $("#site-footer").height())) {
				var contentTop = headerHeight + ((windowHeight - (headerHeight + $("#site-footer").height()))/2);
				var margitTop = 0 - ((contentInnerHeight/2) + (headerHeight/2));

				$(this).css({
					'top': (sectionHeight/2) + "px",
					'margin-top': margitTop + "px"
				});
			} else {
				$(this).css({
					"top": (headerHeight + 20) + "px",
					'margin-top': "0px"
				});
				$(this).parents("section").find(".inner-bg.full-size-bg").css("min-height", ($(this).height() + parseInt($(this).css('top'))));
				$(this).parents("section").css("min-height", ($(this).height()));
				$(this).parents("section").css("padding-bottom", (parseInt($(this).css('top'))));
			}
		} else {
			if (contentInnerHeight < (sectionHeight-headerHeight)) {
				var contentTop = (sectionHeight - contentInnerHeight)/2;
				$(this).css("top", contentTop);
			} else {
				$(this).css("top", headerHeight + 20);
				$(this).parents("section").find(".inner-bg.full-size-bg").css("min-height", ($(this).height() + parseInt($(this).css('top'))));
				$(this).parents("section").css("min-height", ($(this).height() + parseInt($(this).css('top'))));
				$(this).parents("section").css("margin-bottom", "60px");
			}
		}
	});

	$(".clear.clear-footer-spacer").height($("#site-footer").height());
	$("#wrap").css("margin-bottom", (0 - $("#site-footer").height()));
	$("#contacts .container").css("margin-bottom", (0 - $("#site-footer").height()));
}
$(document).ready(function () {

    $('body').bind('beforeunload', function () {
        localStorage.setItem("navbarLnk", null);
    });

    if ($('.navbar').hasClass('navbar-light')) {
        $('.navbar-brand img').attr('src', '/Content/img/dockyard_logo.png');
    }
    if (window.location.pathname === "/Services/DocuSign" || window.location.pathname === "/Services/Salesforce") {
        $('.navbar-brand img').attr('src', '/Content/img/dockyard_logo_dark.png');
    }

    if ($('.video-frame').length) {
        var iframe = $('.video-frame')[0];
        var player = $f(iframe);
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

	var links = $('.home-nav.navbar-nav > li[data-scroll = "scrolling"] > a'),
		section = $('.text-block'),
		button = $('.toSection'),
		mywindow = $(window),
		htmlbody = $('html,body'),
		offsetTop = $('.navbar').height(),
		headerHeight = $('#site-header').height();
		offsetTop = headerHeight;

		if (localStorage.getItem("navbarLnk") > 0) {
		    goToByScroll(localStorage.getItem("navbarLnk"));
		    localStorage.setItem("navbarLnk", null);
		}

	section.waypoint({
		 handler: function (direction) {
			var datasection = $(this).attr('data-section');
			 var currLnk;
			if (direction === 'down') {
				$('.home-nav.navbar-nav > li > a[href *= "' + datasection + '"]').parents("li").addClass('active').siblings().removeClass('active');
			}
			else {
				var newsection = parseInt(datasection) - 1;
				$('.home-nav.navbar-nav > li > a[href *= "' + datasection + '"]').parents("li").addClass('active').siblings().removeClass('active');
			}
		}, 
		offset: offsetTop
	});

	$("section#welcome").click(function(){
		goToByScroll("services");
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
	    var datasection = $(this).attr('href').split("#").pop();
	    e.preventDefault();
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