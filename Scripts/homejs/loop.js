jQuery(function ($) {

    /*----------------------/
    /* PAGE LOADER
    /*---------------------*/
    if ($('body.no-preloader').length <= 0) {
        $('body').jpreLoader({
            showSplash: false,
            loaderVPos: "50%"
        });
    }

    /*----------------------/
    /* MAIN NAVIGATION
    /*---------------------*/
    // Navbar and logo switch related with SCROLL POSITION
    $(window).on('scroll', function () {
        if ($(window).width() > 1024) {
            if ($(document).scrollTop() > 80) {
                setNavbarLight();
            } else {
                setNavbarTransparent();
            }
        }
    });

    // Navbar and logo switch related with SCREEN WIDTH
    function toggleNavbar() {
        if (($(window).width() > 1024) && ($(document).scrollTop() <= 150)) {
            setNavbarTransparent();
        } else {
            setNavbarLight();
        }
    }

    toggleNavbar();
    $(window).resize(function () {
        toggleNavbar();
    });

    // Setting navbar to solid white background
    function setNavbarLight() {
        $('.navbar').addClass('navbar-light');
        $('.navbar-brand img').attr('src', '/Content/img/dockyard_logo.png');
    }

    // Setting navbar to trasparent background and change its logo according to it
    function setNavbarTransparent() {
        $('.navbar').removeClass('navbar-light');
        if (window.location.pathname === "/Services/DocuSign" || window.location.pathname === "/Services/Salesforce") {
            $('.navbar-brand img').attr('src', '/Content/img/dockyard_logo_dark.png');
        } else {
            $('.navbar-brand img').attr('src', '/Content/img/dockyard_logo_white.png');
        }
    }

    // hide collapsible menu
    $('.navbar-nav li a').click(function () {
        if ($(this).parents('.navbar-collapse.collapse').hasClass('in')) {
            $('#main-nav').collapse('hide');
        }
    });

    $('#main-nav').localScroll({
        duration: 1000,
        easing: 'easeInOutExpo',
        offset: -96
    });

    $('.hero-buttons').localScroll({
        duration: 1000,
        easing: 'easeInOutExpo',
        offset: -96
    });


    /*----------------------/
	/* HERO UNIT SUPERSIZED
	/*---------------------*/
    if ($('.slideshow').length > 0) {
        $.supersized({

            // Functionality		
            autoplay: 1,				// Slideshow starts playing automatically
            slide_interval: 3000,		// Length between transitions
            transition: 1, 				// 0-None, 1-Fade, 2-Slide Top, 3-Slide Right, 4-Slide Bottom, 5-Slide Left, 6-Carousel Right, 7-Carousel Left
            transition_speed: 1000,		// Speed of transition

            // Components
            slide_links: 'blank',		// Individual links for each slide (Options: false, 'num', 'name', 'blank')
            thumb_links: 0,				// Individual thumb links for each slide
            slides: [				// Slideshow Images
							{ image: 'assets/img/sliders/slider1.jpg', title: '<div class="hero-text"><h2 class="hero-heading">HANDCRAFTED</h2><p>Built to provide great visitor experience</p></div>', thumb: '', url: '' },
							{ image: 'assets/img/sliders/slider2.jpg', title: '<div class="hero-text"><h2 class="hero-heading">PARALLAX</h2><p>Scrolling the page is fun with parallax background</p></div>', thumb: '', url: '' },
							{ image: 'assets/img/sliders/slider3.jpg', title: '<div class="hero-text"><h2 class="hero-heading">BUY ONE FOR TWO</h2><p>Buy one to get both of the agency and personal theme</p></div>', thumb: '', url: '' }
            ]
        });

        $(".fa-pause, .fa-play").click(function () {
            $(this).toggleClass("fa-pause fa-play");
        });
    }


    /*----------------------/
	/* PARALLAX
	/*---------------------*/
    $('.full-width-parallax').parallax(0, 0.1);

    function setParallax() {
        if ($(window).width() > 1024) {
            $('.full-width-parallax').parallax(0, 0.1);
        }
    }

    setParallax();
    $(window).resize(function () {
        setParallax();
    });
    
    /*----------------------/
	/* SKILLS
	/*---------------------*/
    $('#skills').waypoint(function () {
        $('.chart').each(function () {
            $(this).easyPieChart({
                size: 150,
                barColor: '#26A599',
                trackColor: '#eee',
                scaleColor: false,
                lineWidth: 2,
                easing: 'easeOutExpo',
                animate: 2000
            });
        });
    },
	{
	    offset: '70%'
	});


    /*----------------------/
	/* TWITTER STREAM
	/*---------------------*/
    /*
	* ### HOW TO CREATE A VALID ID TO USE: ###
	* Go to www.twitter.com and sign in as normal, go to your settings page.
	* Go to "Widgets" on the left hand side.
	* Create a new widget for what you need eg "user timeline" or "search" etc.
	* Feel free to check "exclude replies" if you dont want replies in results.
	* Now go back to settings page, and then go back to widgets page, you should
	* see the widget you just created. Click edit.
	* Now look at the URL in your web browser, you will see a long number like this:
	* 345735908357048478
	* Use this as your ID below instead!
	*/
    /**
	* How to use fetch function:
	* @param {string} Your Twitter widget ID.
	* @param {string} The ID of the DOM element you want to write results to. 
	* @param {int} Optional - the maximum number of tweets you want returned. Must
	*     be a number between 1 and 20.
	* @param {boolean} Optional - set true if you want urls and hashtags to be hyperlinked!
	* @param {boolean} Optional - Set false if you dont want user photo /
	*     name for tweet to show.
	* @param {boolean} Optional - Set false if you dont want time of tweet
	*     to show.
	* @param {function/string} Optional - A function you can specify to format
	*     tweet date/time however you like. This function takes a JavaScript date
	*     as a parameter and returns a String representation of that date.
	*     Alternatively you may specify the string 'default' to leave it with
	*     Twitter's default renderings.
	*/

    //twitterFetcher.fetch( '441767385733668865', 'tweet', 1, true, false, true, 'default');


    /*----------------------/
	/* SCROLL TO TOP
	/*---------------------*/
    if ($(window).width() > 992) {
        $(window).scroll(function () {
            if ($(this).scrollTop() > 300) {
                $('.back-to-top').fadeIn();
            } else {
                $('.back-to-top').fadeOut();
            }
        });

        $('.back-to-top').click(function (e) {
            e.preventDefault();

            $('body, html').animate({
                scrollTop: 0
            }, 800, 'easeInOutExpo');
        });
    }


    /*----------------------/
	/* WORKS
	/*---------------------*/
    var $container = $('.work-item-list');

    new imagesLoaded($container, function () {
        $container.isotope({
            itemSelector: '.work-item'
        });
    });

    $('.work-item-filters a').click(function (e) {

        var selector = $(this).attr('data-filter');
        $container.isotope({
            filter: selector
        });

        $('.work-item-filters a').removeClass('active');
        $(this).addClass('active');

        return false;
    });

    var originalTitle, currentItem;

    $('.media-popup').magnificPopup({
        type: 'image',
        callbacks: {
            beforeOpen: function () {

                // modify item title to include description
                currentItem = $(this.items)[this.index];
                originalTitle = currentItem.title;
                currentItem.title = '<h3>' + originalTitle + '</h3>' + '<p>' + $(currentItem).parents('.work-item').find('img').attr('alt') + '</p>';

                // adding animation
                this.st.mainClass = 'mfp-fade';
            },
            close: function () {
                currentItem.title = originalTitle;
            }
        }

    });


    /*----------------------/
	/* SOCIAL NETWORK
	/*---------------------*/
    if ($(window).width() > 1024) {
        wow = new WOW({
            animateClass: 'animated'
        });

        wow.init();
    } else {
        $('.wow').attr('class', '');
    }


    /*----------------------/
	/* TOOLTIP
	/*---------------------*/
    if ($(window).width() > 1024) {
        $('body').tooltip({
            selector: "[data-toggle=tooltip]",
            container: "body"
        });
    }


    /*----------------------/
	/* AJAX CONTACT FORM
	/*---------------------*/
    $('#contact-form').parsley();

    $('.contact-form form').submit(function (e) {

        e.preventDefault();

        if (!$(this).parsley('isValid'))
            return;

        $theForm = $(this);
        $btn = $(this).find('#submit-button');
        $btnText = $btn.text();
        $alert = $(this).parent().find('.alert');

        $btn.find('.loading-icon').addClass('fa-spinner fa-spin ');
        $btn.prop('disabled', true).find('span').text("Sending...");

        $.post('/Home/ProcessSubmittedEmail', { 'name': $('#name').val(), 'emailId': $('#email').val(), 'message': $('#message').val() }, function (data) {

            $message = data;

            if ($message == "success") {
                $theForm.slideUp('medium', function () {
                    $alert.removeClass('alert-danger');
                    $alert.addClass('alert-success').html("Thanks for contacting us. We will be back to you shortly.").slideDown('medium');
                });
            } else {
                $alert.addClass('alert-danger').html($message).slideDown('medium');
            }

            $btn.find('.loading-icon').removeClass('fa-spinner fa-spin ');
            $btn.prop('disabled', false).find('span').text($btnText);

        }).fail(function () { console.log('AJAX Error'); });
    });


    // init scrollspy except on Opera, it doesn't work because body has 100% height
    if (!navigator.userAgent.match("Opera/")) {
        $('body').scrollspy({
            target: '#main-nav'
        });
    } else {
        $('#main-nav .nav li').removeClass('active');
    }
});

/*menu*/
$("li.dropdown").hover(function () {
    $(this).find("a.dropdown-toggle").attr('aria-expanded', "true");
    $(this).addClass("open");
}, function () {
    $(this).find("a.dropdown-toggle").attr('aria-expanded', "false");
    $(this).removeClass("open");
});
