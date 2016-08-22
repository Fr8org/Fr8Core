function EasyPeasyParallax() {
    var windowWidth = $(window).width();
    if (windowWidth > 980) {
        scrollPos = $(this).scrollTop();
        $('#welcome').css({
            'background-position': '50%' + (-scrollPos / 4) + "px"
        });
        $('.landing-welcome-area').css({
            'top': ($('#welcome').height - $('.navbar-fixed-top') - $(this).height) / 2,
            'margin-top': (scrollPos / 4) + "px",
            'opacity': 1 - (scrollPos / 250)
        });
        $('#about').css({
            'top': ($('#welcome').height - $('.navbar-fixed-top') - $(this).height) / 2,
            'margin-top': (scrollPos / 4) - 80 + "px",
            'opacity': 1 - (scrollPos / 250)
        });
        var opacityValue = $('.landing-welcome-area, #about').css('opacity');
        if (opacityValue == 0) {
            $('.landing-welcome-area, #about').hide();
        } else {
            $('.landing-welcome-area, #about').show();
        }
    }
}

$(document).ready(function () {
    localStorage.setItem("navbarLnk", null);
    var mywindow = $(window),
        offsetTop = $('.navbar').height(),
        htmlbody = $('html,body');

    $(window).scroll(function () {
        EasyPeasyParallax();
        if ($(this).scrollTop() > 300) {
            $('.goTop-link').fadeIn();
        } else {
            $('.goTop-link').fadeOut();
        }
    });

    $('.goTop, li.about-us-lnk').click(function () {
        $('body,html').animate({ scrollTop: 0 }, 1000);
        $('.navbar li').removeClass('active');
        $('li.about-us-lnk').addClass("active");
    });

    $('li.contact-lnk').click(function () {
        $('.navbar li').removeClass('active');
        $('li.contact-lnk').addClass("active");
        htmlbody.animate({
            scrollTop: $('#contacts').offset().top - offsetTop
        }, 1000);
        return false;
    });

    mywindow.scroll(function () {
        if (mywindow.scrollTop() < $('#welcome').height() - offsetTop - 5) {
            $('.navbar li').removeClass('active');
            $('.navbar li.about-us-lnk').addClass('active');
            
        } else {
            $('.navbar li.about-us-lnk').removeClass('active');
            $('li.contact-lnk').addClass("active");
        }
    });

    $('#welcome').click(function () {
        htmlbody.animate({
            scrollTop: $('#contacts').offset().top - offsetTop
        }, 1000);
    });

    $('.navbar li').click(function () {
        localStorage.setItem("navbarLnk", $(this).attr('data-section'));
    });

});

$(window).resize(function () {
    resizePageComponents();
})