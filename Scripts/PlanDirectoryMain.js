jQuery(function ($) {

    // Navbar and logo switch related with screen width
    function toggleNavbar() {
        if (($(window).width() > 1024) && ($(document).scrollTop() <= 100)) {
            setNavbarTransparent();
        } else {
            setNavbarLight();
        }
    } 

    function setNavbarLight() {
        $('.navbar').addClass('navbar-light');
        $('.navbar-brand img').attr('src', '../Content/img/dockyard_logo.png');
    }

    function setNavbarTransparent() {
        $('.navbar').removeClass('navbar-light');
        $('.navbar-brand img').attr('src', '../Content/img/dockyard_logo_white.png');
    }

    function resizePageComponents() {
        var windowHeight = $(window).height(),
            headerHeight = $('header#site-header .navbar').innerHeight(),
            footerHeight = $('#site-footer').outerHeight(),
            contentInnerHeight = $(".inner-wrap.centered").height(),
            searchBarHeight = $('.search-bar-container').height();

        // Helps the background setting for footer
        $("section.full-height-block").css({
            "min-height": windowHeight - footerHeight
        });

        $(".inner-wrap.centered").each(function () {
            contentInnerHeight = $(this).height();
            if (($("section.full-height-block").length == 1)) {
                $(this).css({
                    "top": (headerHeight + 20) + "px",
                    'margin-top': "0"
                });
                $(this).parents("section").css("padding-bottom", (parseInt($(this).css('top'))));
            } else {
                if (contentInnerHeight < (windowHeight - headerHeight)) {
                    $(this).css("top", (windowHeight - contentInnerHeight) / 2);
                } else {
                    $(this).css("top", headerHeight + 20);
                    $(this).parents("section").find(".inner-bg.full-size-bg").css("min-height", ($(this).height() + parseInt($(this).css('top'))));
                    $(this).parents("section").css("margin-bottom", "60px");
                }
            }
        });

        $(".clear.clear-footer-spacer").height($("#site-footer").height());
        $("#wrap").css("margin-bottom", (0 - $("#site-footer").height()));
    }

    // Navbar and logo switch related with scroll position
    $(window).on('scroll', function () {
        if ($(window).width() > 1024) {
            if ($(document).scrollTop() > 50) {
                setNavbarLight();
            } else {
                setNavbarTransparent();
            }
        }
    });
    
    $(window).resize(function () {
        resizePageComponents();
        toggleNavbar();
    });

    $(document).ready(function () {
        $('body').bind('beforeunload', function () {
            localStorage.setItem("navbarLnk", null);
        });

        resizePageComponents();

        $('a[rel="popover"]').popover();
        $('a[rel="tooltip"]').tooltip();
        $('.carousel').carousel({
            interval: false
        })

        /* Menu*/
        $("li.dropdown").hover(function () {
            $(this).find("a.dropdown-toggle").attr('aria-expanded', "true");
            $(this).addClass("open");
        }, function () {
            $(this).find("a.dropdown-toggle").attr('aria-expanded', "false");
            $(this).removeClass("open");
        });
    });

    toggleNavbar();

    // Hide collapsible menu
    $('.navbar-nav li a').click(function () {
        if ($(this).parents('.navbar-collapse.collapse').hasClass('in')) {
            $('#main-nav').collapse('hide');
        }
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
