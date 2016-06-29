function EasyPeasyParallax() {
    if($(window).width() > 980){
        scrollPos = $(this).scrollTop();
    }
}

function resizePageComponents() {
    var	windowHeight = $(window).height(),
        headerHeight = $('header#site-header .navbar').innerHeight(),
        footerHeight = $('#site-footer').height() ? $('#site-footer').height() : $('#footer').outerHeight(),
        contentInnerHeight = $(".inner-wrap.centered").height();

    // Give each section much space as window height 
    $("section.full-height-block").css({
        "min-height": windowHeight
    });

    // Sets sections according to footer
    var sectionElement = $("section.full-height-block:last-child .container.full-height-block");
    if (sectionElement) {
        sectionElement.css({
            "padding-bottom": footerHeight,
            "margin-bottom": 0 - footerHeight
        });
    }
    sectionElement = $("section.full-height-block .container.full-height-block");
    if (sectionElement) {
        sectionElement.css({
            "padding-bottom": footerHeight,
            "margin-bottom": 0 - footerHeight
        });
    }

    // Sets mostly section containers height
    $(".inner-wrap.centered").each(function () {
        contentInnerHeight = $(this).height();

        /* This code is legacy and must be fixed. Refactoring only done for reducing complexity. */
        var parentSection;
        var isFullHeightSection = true;
        if ( $(this).parents("section#contacts").length > 0 ) {
            parentSection = $(this).parents("section#contacts");
        } else if ( $(this).parents("section.full-height-block").length == 1 ) {
            parentSection = $(this).parents("section.full-height-block");
        } else {
            parentSection = $(this).parents("section");
            isFullHeightSection = false;
        }

        if (isFullHeightSection) {
            // Checks whether content is fitting with including footer
            if (contentInnerHeight < (windowHeight - headerHeight - footerHeight) ) {
                $(this).css({
                    'top': (windowHeight / 2) + "px",
                    'margin-top': 0 - ((contentInnerHeight / 2) + (headerHeight / 2)) + "px"
                });
            } else {
                $(this).css("top", headerHeight + 20);
                parentSection.find(".inner-bg.full-size-bg").css("min-height", ($(this).height() + parseInt($(this).css('top'))));
                parentSection.css("padding-bottom", (parseInt($(this).css('top'))));
                parentSection.css("min-height", ($(this).height()));
            }
            // Checks whether content is fitting without footer and not full height section
        } else if (contentInnerHeight < (windowHeight - headerHeight)) {
            $(this).css("top", (windowHeight - contentInnerHeight) / 2);
        } else {
            $(this).css("top", headerHeight + 20);
            parentSection.find(".inner-bg.full-size-bg").css("min-height", windowHeight - footerHeight);
            parentSection.css("min-height", ($(this).height() + parseInt($(this).css('top'))));
            parentSection.css("margin-bottom", "60px");
        }
    });

    // Miscellaneous settings
    $("section#support .inner-bg.full-size-bg").css("max-height", windowHeight - footerHeight);
    $(".clear.clear-footer-spacer").height(footerHeight);
    $("#wrap").css("margin-bottom", (0 - footerHeight));
    $("#contacts .container").css("margin-bottom", (0 - footerHeight));
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
        $f(iframe);
    }

    resizePageComponents();

    $('a[rel="popover"]').popover();
    $('a[rel="tooltip"]').tooltip();
    $('.carousel').carousel({
        interval: false
    });

    /* Menu Dropdown */
    $("li.dropdown").hover(function() {
        $(this).find("a.dropdown-toggle").attr('aria-expanded', "true");
        $(this).addClass("open");
    }, function() {
        $(this).find("a.dropdown-toggle").attr('aria-expanded', "false");
        $(this).removeClass("open");
    });

    var links = $('.home-nav.navbar-nav > li[data-scroll = "scrolling"] > a'),
        section = $('.text-block'),
        htmlbody = $('html,body'),
        headerHeight = $('#site-header').height();

    if (localStorage.getItem("navbarLnk") > 0) {
        goToByScroll(localStorage.getItem("navbarLnk"));
        localStorage.setItem("navbarLnk", null);
    }

    section.waypoint({
        offset: headerHeight,
        handler: function (direction) {
            var datasection = $(this).attr('data-section');
            if (direction === 'down') {
                $('.home-nav.navbar-nav > li > a[href *= "' + datasection + '"]').parents("li").addClass('active').siblings().removeClass('active');
            } else {
                $('.home-nav.navbar-nav > li > a[href *= "' + datasection + '"]').parents("li").addClass('active').siblings().removeClass('active');
            }
        }
    });

    $("section#welcome").click(function(){
        goToByScroll("services");
    });

    function goToByScroll(datasection) {
        htmlbody.animate({
            scrollTop: $('.text-block[data-section="' + datasection + '"]').offset().top
        }, 1000);
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
    resizePageComponents();
})