$(document).ready(function () {          
    var containerHeight, loginFormTop 
    getLogiTop();
    landingWelcomeWidth();

    $(window).resize(function () {
        landingWelcomeWidth()
	});  

    if ($('#loginform, .registration-section, .login-section.status-message').length > 0) {
	    $('#main-container').addClass('login-page');
		$(window).resize(function () {
		    getLogiTop();
		});   
	} else {    
	    $('#main-container').removeClass('login-page');
    }

    if ($('.registration-section input.text-box.single-line').length > 0) {
        $('.registration-section input.text-box.single-line').addClass('form-control')
    }

    if ($('#emailInfoBox').length > 0) {
        $("body").removeClass('view-popup-window');
    }
});

function getLogiTop() {
    containerHeight = $(window).height() - $('.site-footer').height() - $('.site-header-wrap').height() - 30;
    loginFormTop = (containerHeight - $('.login-section').height()) / 2;
    if (loginFormTop > 0) {
        $('.login-section').css('margin-top', loginFormTop)
    }
}

function landingWelcomeWidth() {
    containerWidth = $("#welcome .inner-bg").width();
    $(".landing-welcome-area").width(containerWidth);
}
