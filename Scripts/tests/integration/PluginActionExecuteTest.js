


<!DOCTYPE html>
<html lang="en" class=" is-copy-enabled">
  <head prefix="og: http://ogp.me/ns# fb: http://ogp.me/ns/fb# object: http://ogp.me/ns/object# article: http://ogp.me/ns/article# profile: http://ogp.me/ns/profile#">
    <meta charset='utf-8'>
    <meta http-equiv="X-UA-Compatible" content="IE=edge">
    <meta http-equiv="Content-Language" content="en">
    <meta name="viewport" content="width=1020">
    <meta content="origin-when-crossorigin" name="referrer" />
    
    <title>fr8company/PluginActionExecuteTest.js at dev · alexed1/fr8company</title>
    <link rel="search" type="application/opensearchdescription+xml" href="/opensearch.xml" title="GitHub">
    <link rel="fluid-icon" href="https://github.com/fluidicon.png" title="GitHub">
    <link rel="apple-touch-icon" sizes="57x57" href="/apple-touch-icon-114.png">
    <link rel="apple-touch-icon" sizes="114x114" href="/apple-touch-icon-114.png">
    <link rel="apple-touch-icon" sizes="72x72" href="/apple-touch-icon-144.png">
    <link rel="apple-touch-icon" sizes="144x144" href="/apple-touch-icon-144.png">
    <meta property="fb:app_id" content="1401488693436528">

      <meta content="@github" name="twitter:site" /><meta content="summary" name="twitter:card" /><meta content="alexed1/fr8company" name="twitter:title" /><meta content="Contribute to fr8company development by creating an account on GitHub." name="twitter:description" /><meta content="https://avatars3.githubusercontent.com/u/3140883?v=3&amp;s=400" name="twitter:image:src" />
      <meta content="GitHub" property="og:site_name" /><meta content="object" property="og:type" /><meta content="https://avatars3.githubusercontent.com/u/3140883?v=3&amp;s=400" property="og:image" /><meta content="alexed1/fr8company" property="og:title" /><meta content="https://github.com/alexed1/fr8company" property="og:url" /><meta content="Contribute to fr8company development by creating an account on GitHub." property="og:description" />
      <meta name="browser-stats-url" content="https://api.github.com/_private/browser/stats">
    <meta name="browser-errors-url" content="https://api.github.com/_private/browser/errors">
    <link rel="assets" href="https://assets-cdn.github.com/">
    <link rel="web-socket" href="wss://live.github.com/_sockets/MTM2NDM3MDM6ZTkyNDA5MTAyMzIyM2ZjNDhmYzAzMTlhZWZhOGIxYTk6Mzc3MmRkYjAwMDA0N2I2OGE0MDIwMDU3YTc3MTdhNDFkY2M1NjhjMTAxOTViMGFmNTAwNDU3ZGE3YzY1ZTU5Mw==--7ecc9b26fc005d390990c8daeec0568d7c9e1d5f">
    <meta name="pjax-timeout" content="1000">
    <link rel="sudo-modal" href="/sessions/sudo_modal">

    <meta name="msapplication-TileImage" content="/windows-tile.png">
    <meta name="msapplication-TileColor" content="#ffffff">
    <meta name="selected-link" value="repo_source" data-pjax-transient>

    <meta name="google-site-verification" content="KT5gs8h0wvaagLKAVWq8bbeNwnZZK1r1XQysX3xurLU">
    <meta name="google-analytics" content="UA-3769691-2">

<meta content="collector.githubapp.com" name="octolytics-host" /><meta content="github" name="octolytics-app-id" /><meta content="70C65AAA:07C7:11911424:5622844E" name="octolytics-dimension-request_id" /><meta content="13643703" name="octolytics-actor-id" /><meta content="kggayo" name="octolytics-actor-login" /><meta content="3b49f7c5a5503f88d4ff62f0b01f5059e431710eb11507dff5ca933796b8fd10" name="octolytics-actor-hash" />
<meta content="/&lt;user-name&gt;/&lt;repo-name&gt;/blob/show" data-pjax-transient="true" name="analytics-location" />
<meta content="Rails, view, blob#show" data-pjax-transient="true" name="analytics-event" />


  <meta class="js-ga-set" name="dimension1" content="Logged In">
    <meta class="js-ga-set" name="dimension4" content="Current repo nav">




    <meta name="is-dotcom" content="true">
        <meta name="hostname" content="github.com">
    <meta name="user-login" content="kggayo">

      <link rel="mask-icon" href="https://assets-cdn.github.com/pinned-octocat.svg" color="#4078c0">
      <link rel="icon" type="image/x-icon" href="https://assets-cdn.github.com/favicon.ico">

    <meta content="87025f9b9d4f36784c77a32b1ca51ef5355bda59" name="form-nonce" />

    <link crossorigin="anonymous" href="https://assets-cdn.github.com/assets/github-1c72e5e1cbdeae0d6a25ee0e6f07ae0100db5696d3f5f10ed2acf1f0885ef5f0.css" integrity="sha256-HHLl4cverg1qJe4ObweuAQDbVpbT9fEO0qzx8Ihe9fA=" media="all" rel="stylesheet" />
    <link crossorigin="anonymous" href="https://assets-cdn.github.com/assets/github2-91f10774cc492e563a1bcd77a9b935a21bffbda4c7fefb5ed448d51a53217852.css" integrity="sha256-kfEHdMxJLlY6G813qbk1ohv/vaTH/vte1EjVGlMheFI=" media="all" rel="stylesheet" />
    
    
    


    <meta http-equiv="x-pjax-version" content="aaf0a95e6a9bf84b40427e5620ea23c2">

      
  <meta name="description" content="Contribute to fr8company development by creating an account on GitHub.">
  <meta name="go-import" content="github.com/alexed1/fr8company git https://github.com/alexed1/fr8company.git">

  <meta content="3140883" name="octolytics-dimension-user_id" /><meta content="alexed1" name="octolytics-dimension-user_login" /><meta content="37024812" name="octolytics-dimension-repository_id" /><meta content="alexed1/fr8company" name="octolytics-dimension-repository_nwo" /><meta content="false" name="octolytics-dimension-repository_public" /><meta content="false" name="octolytics-dimension-repository_is_fork" /><meta content="37024812" name="octolytics-dimension-repository_network_root_id" /><meta content="alexed1/fr8company" name="octolytics-dimension-repository_network_root_nwo" />
  <link href="https://github.com/alexed1/fr8company/commits/dev.atom?token=ANAvt0XftvhxWhKrF1x90tSSOFgWAiUoks60L7bTwA%3D%3D" rel="alternate" title="Recent Commits to fr8company:dev" type="application/atom+xml">

  </head>


  <body class="logged_in   env-production windows vis-private page-blob">
    <a href="#start-of-content" tabindex="1" class="accessibility-aid js-skip-to-content">Skip to content</a>

    
    
    



      <div class="header header-logged-in true" role="banner">
  <div class="container clearfix">

    <a class="header-logo-invertocat" href="https://github.com/" data-hotkey="g d" aria-label="Homepage" data-ga-click="Header, go to dashboard, icon:logo">
  <span class="mega-octicon octicon-mark-github"></span>
</a>


      <div class="site-search repo-scope js-site-search" role="search">
          <!-- </textarea> --><!-- '"` --><form accept-charset="UTF-8" action="/alexed1/fr8company/search" class="js-site-search-form" data-global-search-url="/search" data-repo-search-url="/alexed1/fr8company/search" method="get"><div style="margin:0;padding:0;display:inline"><input name="utf8" type="hidden" value="&#x2713;" /></div>
  <label class="js-chromeless-input-container form-control">
    <div class="scope-badge">This repository</div>
    <input type="text"
      class="js-site-search-focus js-site-search-field is-clearable chromeless-input"
      data-hotkey="s"
      name="q"
      placeholder="Search"
      aria-label="Search this repository"
      data-global-scope-placeholder="Search GitHub"
      data-repo-scope-placeholder="Search"
      tabindex="1"
      autocapitalize="off">
  </label>
</form>
      </div>

      <ul class="header-nav left" role="navigation">
        <li class="header-nav-item">
          <a href="/pulls" class="js-selected-navigation-item header-nav-link" data-ga-click="Header, click, Nav menu - item:pulls context:user" data-hotkey="g p" data-selected-links="/pulls /pulls/assigned /pulls/mentioned /pulls">
            Pull requests
</a>        </li>
        <li class="header-nav-item">
          <a href="/issues" class="js-selected-navigation-item header-nav-link" data-ga-click="Header, click, Nav menu - item:issues context:user" data-hotkey="g i" data-selected-links="/issues /issues/assigned /issues/mentioned /issues">
            Issues
</a>        </li>
          <li class="header-nav-item">
            <a class="header-nav-link" href="https://gist.github.com/" data-ga-click="Header, go to gist, text:gist">Gist</a>
          </li>
      </ul>

    
<ul class="header-nav user-nav right" id="user-links">
  <li class="header-nav-item">
      <span class="js-socket-channel js-updatable-content"
        data-channel="notification-changed:kggayo"
        data-url="/notifications/header">
      <a href="/notifications" aria-label="You have unread notifications" class="header-nav-link notification-indicator tooltipped tooltipped-s" data-ga-click="Header, go to notifications, icon:unread" data-hotkey="g n">
          <span class="mail-status unread"></span>
          <span class="octicon octicon-bell"></span>
</a>  </span>

  </li>

  <li class="header-nav-item dropdown js-menu-container">
    <a class="header-nav-link tooltipped tooltipped-s js-menu-target" href="/new"
       aria-label="Create new…"
       data-ga-click="Header, create new, icon:add">
      <span class="octicon octicon-plus left"></span>
      <span class="dropdown-caret"></span>
    </a>

    <div class="dropdown-menu-content js-menu-content">
      <ul class="dropdown-menu dropdown-menu-sw">
        
<a class="dropdown-item" href="/new" data-ga-click="Header, create new repository">
  New repository
</a>


  <a class="dropdown-item" href="/organizations/new" data-ga-click="Header, create new organization">
    New organization
  </a>



  <div class="dropdown-divider"></div>
  <div class="dropdown-header">
    <span title="alexed1/fr8company">This repository</span>
  </div>
    <a class="dropdown-item" href="/alexed1/fr8company/issues/new" data-ga-click="Header, create new issue">
      New issue
    </a>

      </ul>
    </div>
  </li>

  <li class="header-nav-item dropdown js-menu-container">
    <a class="header-nav-link name tooltipped tooltipped-s js-menu-target" href="/kggayo"
       aria-label="View profile and more"
       data-ga-click="Header, show menu, icon:avatar">
      <img alt="@kggayo" class="avatar" height="20" src="https://avatars0.githubusercontent.com/u/13643703?v=3&amp;s=40" width="20" />
      <span class="dropdown-caret"></span>
    </a>

    <div class="dropdown-menu-content js-menu-content">
      <div class="dropdown-menu  dropdown-menu-sw">
        <div class=" dropdown-header header-nav-current-user css-truncate">
            Signed in as <strong class="css-truncate-target">kggayo</strong>

        </div>


        <div class="dropdown-divider"></div>

          <a class="dropdown-item" href="/kggayo" data-ga-click="Header, go to profile, text:your profile">
            Your profile
          </a>
        <a class="dropdown-item" href="/stars" data-ga-click="Header, go to starred repos, text:your stars">
          Your stars
        </a>
        <a class="dropdown-item" href="/explore" data-ga-click="Header, go to explore, text:explore">
          Explore
        </a>
          <a class="dropdown-item" href="/integrations" data-ga-click="Header, go to integrations, text:integrations">
            Integrations
          </a>
        <a class="dropdown-item" href="https://help.github.com" data-ga-click="Header, go to help, text:help">
          Help
        </a>

          <div class="dropdown-divider"></div>

          <a class="dropdown-item" href="/settings/profile" data-ga-click="Header, go to settings, icon:settings">
            Settings
          </a>

          <!-- </textarea> --><!-- '"` --><form accept-charset="UTF-8" action="/logout" class="logout-form" data-form-nonce="87025f9b9d4f36784c77a32b1ca51ef5355bda59" method="post"><div style="margin:0;padding:0;display:inline"><input name="utf8" type="hidden" value="&#x2713;" /><input name="authenticity_token" type="hidden" value="W56kO3y2G64nE8BJYzdH7uEX810RjHIdyqzOpjkzZScgmTimaj5P8yH4hgUqY8Wx0Bkhio2tJNmGMpCl2vSPIQ==" /></div>
            <button class="dropdown-item dropdown-signout" data-ga-click="Header, sign out, icon:logout">
              Sign out
            </button>
</form>
      </div>
    </div>
  </li>
</ul>


    
  </div>
</div>

      

      


    <div id="start-of-content" class="accessibility-aid"></div>

    <div id="js-flash-container">
</div>


    <div role="main" class="main-content">
        <div itemscope itemtype="http://schema.org/WebPage">
    <div class="pagehead repohead instapaper_ignore readability-menu">

      <div class="container">

        <div class="clearfix">
          

<ul class="pagehead-actions">

  <li>
      <!-- </textarea> --><!-- '"` --><form accept-charset="UTF-8" action="/notifications/subscribe" class="js-social-container" data-autosubmit="true" data-form-nonce="87025f9b9d4f36784c77a32b1ca51ef5355bda59" data-remote="true" method="post"><div style="margin:0;padding:0;display:inline"><input name="utf8" type="hidden" value="&#x2713;" /><input name="authenticity_token" type="hidden" value="TfSCSGh8t8sNkKHgQs2RfzDaRgyStub2TV9eCkZNjKgj6g5QYZLOaaN+2Har97HSLF7VKXJ6mkOHSuZHAK5usQ==" /></div>    <input id="repository_id" name="repository_id" type="hidden" value="37024812" />

      <div class="select-menu js-menu-container js-select-menu">
        <a href="/alexed1/fr8company/subscription"
          class="btn btn-sm btn-with-count select-menu-button js-menu-target" role="button" tabindex="0" aria-haspopup="true"
          data-ga-click="Repository, click Watch settings, action:blob#show">
          <span class="js-select-button">
            <span class="octicon octicon-eye"></span>
            Unwatch
          </span>
        </a>
        <a class="social-count js-social-count" href="/alexed1/fr8company/watchers">
          18
        </a>

        <div class="select-menu-modal-holder">
          <div class="select-menu-modal subscription-menu-modal js-menu-content" aria-hidden="true">
            <div class="select-menu-header">
              <span class="select-menu-title">Notifications</span>
              <span class="octicon octicon-x js-menu-close" role="button" aria-label="Close"></span>
            </div>

            <div class="select-menu-list js-navigation-container" role="menu">

              <div class="select-menu-item js-navigation-item " role="menuitem" tabindex="0">
                <span class="select-menu-item-icon octicon octicon-check"></span>
                <div class="select-menu-item-text">
                  <input id="do_included" name="do" type="radio" value="included" />
                  <span class="select-menu-item-heading">Not watching</span>
                  <span class="description">Be notified when participating or @mentioned.</span>
                  <span class="js-select-button-text hidden-select-button-text">
                    <span class="octicon octicon-eye"></span>
                    Watch
                  </span>
                </div>
              </div>

              <div class="select-menu-item js-navigation-item selected" role="menuitem" tabindex="0">
                <span class="select-menu-item-icon octicon octicon octicon-check"></span>
                <div class="select-menu-item-text">
                  <input checked="checked" id="do_subscribed" name="do" type="radio" value="subscribed" />
                  <span class="select-menu-item-heading">Watching</span>
                  <span class="description">Be notified of all conversations.</span>
                  <span class="js-select-button-text hidden-select-button-text">
                    <span class="octicon octicon-eye"></span>
                    Unwatch
                  </span>
                </div>
              </div>

              <div class="select-menu-item js-navigation-item " role="menuitem" tabindex="0">
                <span class="select-menu-item-icon octicon octicon-check"></span>
                <div class="select-menu-item-text">
                  <input id="do_ignore" name="do" type="radio" value="ignore" />
                  <span class="select-menu-item-heading">Ignoring</span>
                  <span class="description">Never be notified.</span>
                  <span class="js-select-button-text hidden-select-button-text">
                    <span class="octicon octicon-mute"></span>
                    Stop ignoring
                  </span>
                </div>
              </div>

            </div>

          </div>
        </div>
      </div>
</form>
  </li>

  <li>
    
  <div class="js-toggler-container js-social-container starring-container ">

    <!-- </textarea> --><!-- '"` --><form accept-charset="UTF-8" action="/alexed1/fr8company/unstar" class="js-toggler-form starred js-unstar-button" data-form-nonce="87025f9b9d4f36784c77a32b1ca51ef5355bda59" data-remote="true" method="post"><div style="margin:0;padding:0;display:inline"><input name="utf8" type="hidden" value="&#x2713;" /><input name="authenticity_token" type="hidden" value="xb7BEZbPEA9H3vPSmM/r6edlkqOcE2nKxuILdyJKzQgx6czrnQV5TCfKjmtFqaETSia9QsI8aaxsx0wBUKm1SA==" /></div>
      <button
        class="btn btn-sm btn-with-count js-toggler-target"
        aria-label="Unstar this repository" title="Unstar alexed1/fr8company"
        data-ga-click="Repository, click unstar button, action:blob#show; text:Unstar">
        <span class="octicon octicon-star"></span>
        Unstar
      </button>
        <a class="social-count js-social-count" href="/alexed1/fr8company/stargazers">
          1
        </a>
</form>
    <!-- </textarea> --><!-- '"` --><form accept-charset="UTF-8" action="/alexed1/fr8company/star" class="js-toggler-form unstarred js-star-button" data-form-nonce="87025f9b9d4f36784c77a32b1ca51ef5355bda59" data-remote="true" method="post"><div style="margin:0;padding:0;display:inline"><input name="utf8" type="hidden" value="&#x2713;" /><input name="authenticity_token" type="hidden" value="3gLMeaJeor4QVZ3Gl0qyldbbGOt5y2a0YKK/EPXs4+DFcVrWdqytJyUjJ2Vv9QIDs4Cv0hpL/EPQdaOP0XIaXQ==" /></div>
      <button
        class="btn btn-sm btn-with-count js-toggler-target"
        aria-label="Star this repository" title="Star alexed1/fr8company"
        data-ga-click="Repository, click star button, action:blob#show; text:Star">
        <span class="octicon octicon-star"></span>
        Star
      </button>
        <a class="social-count js-social-count" href="/alexed1/fr8company/stargazers">
          1
        </a>
</form>  </div>

  </li>

  <li>
          <a href="#fork-destination-box" class="btn btn-sm btn-with-count"
              title="Fork your own copy of alexed1/fr8company to your account"
              aria-label="Fork your own copy of alexed1/fr8company to your account"
              rel="facebox"
              data-ga-click="Repository, show fork modal, action:blob#show; text:Fork">
            <span class="octicon octicon-repo-forked"></span>
            Fork
          </a>

          <div id="fork-destination-box" style="display: none;">
            <h2 class="facebox-header" data-facebox-id="facebox-header">Where should we fork this repository?</h2>
            <include-fragment src=""
                class="js-fork-select-fragment fork-select-fragment"
                data-url="/alexed1/fr8company/fork?fragment=1">
              <img alt="Loading" height="64" src="https://assets-cdn.github.com/images/spinners/octocat-spinner-128.gif" width="64" />
            </include-fragment>
          </div>

    <a href="/alexed1/fr8company/network" class="social-count">
      0
    </a>
  </li>
</ul>

          <h1 itemscope itemtype="http://data-vocabulary.org/Breadcrumb" class="entry-title private ">
  <span class="mega-octicon octicon-lock"></span>
  <span class="author"><a href="/alexed1" class="url fn" itemprop="url" rel="author"><span itemprop="title">alexed1</span></a></span><!--
--><span class="path-divider">/</span><!--
--><strong><a href="/alexed1/fr8company" data-pjax="#js-repo-pjax-container">fr8company</a></strong>
    <span class="repo-private-label">private</span>

  <span class="page-context-loader">
    <img alt="" height="16" src="https://assets-cdn.github.com/images/spinners/octocat-spinner-32.gif" width="16" />
  </span>

</h1>

        </div>
      </div>
    </div>

    <div class="container">
      <div class="repository-with-sidebar repo-container new-discussion-timeline ">
        <div class="repository-sidebar clearfix">
          
<nav class="sunken-menu repo-nav js-repo-nav js-sidenav-container-pjax js-octicon-loaders"
     role="navigation"
     data-pjax="#js-repo-pjax-container"
     data-issue-count-url="/alexed1/fr8company/issues/counts">
  <ul class="sunken-menu-group">
    <li class="tooltipped tooltipped-w" aria-label="Code">
      <a href="/alexed1/fr8company" aria-label="Code" aria-selected="true" class="js-selected-navigation-item selected sunken-menu-item" data-hotkey="g c" data-selected-links="repo_source repo_downloads repo_commits repo_releases repo_tags repo_branches /alexed1/fr8company">
        <span class="octicon octicon-code"></span> <span class="full-word">Code</span>
        <img alt="" class="mini-loader" height="16" src="https://assets-cdn.github.com/images/spinners/octocat-spinner-32.gif" width="16" />
</a>    </li>

      <li class="tooltipped tooltipped-w" aria-label="Issues">
        <a href="/alexed1/fr8company/issues" aria-label="Issues" class="js-selected-navigation-item sunken-menu-item" data-hotkey="g i" data-selected-links="repo_issues repo_labels repo_milestones /alexed1/fr8company/issues">
          <span class="octicon octicon-issue-opened"></span> <span class="full-word">Issues</span>
          <span class="js-issue-replace-counter"></span>
          <img alt="" class="mini-loader" height="16" src="https://assets-cdn.github.com/images/spinners/octocat-spinner-32.gif" width="16" />
</a>      </li>

    <li class="tooltipped tooltipped-w" aria-label="Pull requests">
      <a href="/alexed1/fr8company/pulls" aria-label="Pull requests" class="js-selected-navigation-item sunken-menu-item" data-hotkey="g p" data-selected-links="repo_pulls /alexed1/fr8company/pulls">
          <span class="octicon octicon-git-pull-request"></span> <span class="full-word">Pull requests</span>
          <span class="js-pull-replace-counter"></span>
          <img alt="" class="mini-loader" height="16" src="https://assets-cdn.github.com/images/spinners/octocat-spinner-32.gif" width="16" />
</a>    </li>

      <li class="tooltipped tooltipped-w" aria-label="Wiki">
        <a href="/alexed1/fr8company/wiki" aria-label="Wiki" class="js-selected-navigation-item sunken-menu-item" data-hotkey="g w" data-selected-links="repo_wiki /alexed1/fr8company/wiki">
          <span class="octicon octicon-book"></span> <span class="full-word">Wiki</span>
          <img alt="" class="mini-loader" height="16" src="https://assets-cdn.github.com/images/spinners/octocat-spinner-32.gif" width="16" />
</a>      </li>
  </ul>
  <div class="sunken-menu-separator"></div>
  <ul class="sunken-menu-group">

    <li class="tooltipped tooltipped-w" aria-label="Pulse">
      <a href="/alexed1/fr8company/pulse" aria-label="Pulse" class="js-selected-navigation-item sunken-menu-item" data-selected-links="pulse /alexed1/fr8company/pulse">
        <span class="octicon octicon-pulse"></span> <span class="full-word">Pulse</span>
        <img alt="" class="mini-loader" height="16" src="https://assets-cdn.github.com/images/spinners/octocat-spinner-32.gif" width="16" />
</a>    </li>

    <li class="tooltipped tooltipped-w" aria-label="Graphs">
      <a href="/alexed1/fr8company/graphs" aria-label="Graphs" class="js-selected-navigation-item sunken-menu-item" data-selected-links="repo_graphs repo_contributors /alexed1/fr8company/graphs">
        <span class="octicon octicon-graph"></span> <span class="full-word">Graphs</span>
        <img alt="" class="mini-loader" height="16" src="https://assets-cdn.github.com/images/spinners/octocat-spinner-32.gif" width="16" />
</a>    </li>
  </ul>


</nav>

            <div class="only-with-full-nav">
                
<div class="js-clone-url clone-url open"
  data-protocol-type="http">
  <h3 class="text-small"><span class="text-emphasized">HTTPS</span> clone URL</h3>
  <div class="input-group js-zeroclipboard-container">
    <input type="text" class="input-mini text-small input-monospace js-url-field js-zeroclipboard-target"
           value="https://github.com/alexed1/fr8company.git" readonly="readonly" aria-label="HTTPS clone URL">
    <span class="input-group-button">
      <button aria-label="Copy to clipboard" class="js-zeroclipboard btn btn-sm zeroclipboard-button tooltipped tooltipped-s" data-copied-hint="Copied!" type="button"><span class="octicon octicon-clippy"></span></button>
    </span>
  </div>
</div>

  
<div class="js-clone-url clone-url "
  data-protocol-type="ssh">
  <h3 class="text-small"><span class="text-emphasized">SSH</span> clone URL</h3>
  <div class="input-group js-zeroclipboard-container">
    <input type="text" class="input-mini text-small input-monospace js-url-field js-zeroclipboard-target"
           value="git@github.com:alexed1/fr8company.git" readonly="readonly" aria-label="SSH clone URL">
    <span class="input-group-button">
      <button aria-label="Copy to clipboard" class="js-zeroclipboard btn btn-sm zeroclipboard-button tooltipped tooltipped-s" data-copied-hint="Copied!" type="button"><span class="octicon octicon-clippy"></span></button>
    </span>
  </div>
</div>

  
<div class="js-clone-url clone-url "
  data-protocol-type="subversion">
  <h3 class="text-small"><span class="text-emphasized">Subversion</span> checkout URL</h3>
  <div class="input-group js-zeroclipboard-container">
    <input type="text" class="input-mini text-small input-monospace js-url-field js-zeroclipboard-target"
           value="https://github.com/alexed1/fr8company" readonly="readonly" aria-label="Subversion checkout URL">
    <span class="input-group-button">
      <button aria-label="Copy to clipboard" class="js-zeroclipboard btn btn-sm zeroclipboard-button tooltipped tooltipped-s" data-copied-hint="Copied!" type="button"><span class="octicon octicon-clippy"></span></button>
    </span>
  </div>
</div>



<div class="clone-options text-small">You can clone with
  <!-- </textarea> --><!-- '"` --><form accept-charset="UTF-8" action="/users/set_protocol?protocol_selector=http&amp;protocol_type=push" class="inline-form js-clone-selector-form is-enabled" data-form-nonce="87025f9b9d4f36784c77a32b1ca51ef5355bda59" data-remote="true" method="post"><div style="margin:0;padding:0;display:inline"><input name="utf8" type="hidden" value="&#x2713;" /><input name="authenticity_token" type="hidden" value="oc0PtB/CVdRXc9hOy3lNM1zT8YKjHB5HDXMFUX4ipcWu6KDai0ivk4mlIhlOYL5pmnyizzoDUGR/Gd37B6BHtQ==" /></div><button class="btn-link js-clone-selector" data-protocol="http" type="submit">HTTPS</button></form>, <!-- </textarea> --><!-- '"` --><form accept-charset="UTF-8" action="/users/set_protocol?protocol_selector=ssh&amp;protocol_type=push" class="inline-form js-clone-selector-form is-enabled" data-form-nonce="87025f9b9d4f36784c77a32b1ca51ef5355bda59" data-remote="true" method="post"><div style="margin:0;padding:0;display:inline"><input name="utf8" type="hidden" value="&#x2713;" /><input name="authenticity_token" type="hidden" value="paP6zrK+diRnoHPH48Vwtb4+ucm3ulSMr6Lk9rT6/IGipcBhZhGSL4XdTAX6OjFTCihQTmEnytzrYMTrWqcv3Q==" /></div><button class="btn-link js-clone-selector" data-protocol="ssh" type="submit">SSH</button></form>, or <!-- </textarea> --><!-- '"` --><form accept-charset="UTF-8" action="/users/set_protocol?protocol_selector=subversion&amp;protocol_type=push" class="inline-form js-clone-selector-form is-enabled" data-form-nonce="87025f9b9d4f36784c77a32b1ca51ef5355bda59" data-remote="true" method="post"><div style="margin:0;padding:0;display:inline"><input name="utf8" type="hidden" value="&#x2713;" /><input name="authenticity_token" type="hidden" value="Bwv5Tm16gxLIC5uSMMUvlLzJHPslsLCgxSfAkDyQ6wPeDcAOjk3wPbRKL7xujGcDF6qldchvnGEYJQW+ehfZ5w==" /></div><button class="btn-link js-clone-selector" data-protocol="subversion" type="submit">Subversion</button></form>.
  <a href="https://help.github.com/articles/which-remote-url-should-i-use" class="help tooltipped tooltipped-n" aria-label="Get help on which URL is right for you.">
    <span class="octicon octicon-question"></span>
  </a>
</div>
  <a href="github-windows://openRepo/https://github.com/alexed1/fr8company" class="btn btn-sm sidebar-button" title="Save alexed1/fr8company to your computer and use it in GitHub Desktop." aria-label="Save alexed1/fr8company to your computer and use it in GitHub Desktop.">
    <span class="octicon octicon-desktop-download"></span>
    Clone in Desktop
  </a>

              <a href="/alexed1/fr8company/archive/dev.zip"
                 class="btn btn-sm sidebar-button"
                 aria-label="Download the contents of alexed1/fr8company as a zip file"
                 title="Download the contents of alexed1/fr8company as a zip file"
                 rel="nofollow">
                <span class="octicon octicon-cloud-download"></span>
                Download ZIP
              </a>
            </div>
        </div>
        <div id="js-repo-pjax-container" class="repository-content context-loader-container" data-pjax-container>

          

<a href="/alexed1/fr8company/blob/e934b55b9bc133289857813a39da306397b004a4/Scripts/tests/integration/PluginActionExecuteTest.js" class="hidden js-permalink-shortcut" data-hotkey="y">Permalink</a>

<!-- blob contrib key: blob_contributors:v21:31aa22e9b92e12f17ef744015d99ce0d -->

  <div class="file-navigation js-zeroclipboard-container">
    
<div class="select-menu js-menu-container js-select-menu left">
  <button class="btn btn-sm select-menu-button js-menu-target css-truncate" data-hotkey="w"
    title="dev"
    type="button" aria-label="Switch branches or tags" tabindex="0" aria-haspopup="true">
    <i>Branch:</i>
    <span class="js-select-button css-truncate-target">dev</span>
  </button>

  <div class="select-menu-modal-holder js-menu-content js-navigation-container" data-pjax aria-hidden="true">

    <div class="select-menu-modal">
      <div class="select-menu-header">
        <span class="select-menu-title">Switch branches/tags</span>
        <span class="octicon octicon-x js-menu-close" role="button" aria-label="Close"></span>
      </div>

      <div class="select-menu-filters">
        <div class="select-menu-text-filter">
          <input type="text" aria-label="Find or create a branch…" id="context-commitish-filter-field" class="js-filterable-field js-navigation-enable" placeholder="Find or create a branch…">
        </div>
        <div class="select-menu-tabs">
          <ul>
            <li class="select-menu-tab">
              <a href="#" data-tab-filter="branches" data-filter-placeholder="Find or create a branch…" class="js-select-menu-tab" role="tab">Branches</a>
            </li>
            <li class="select-menu-tab">
              <a href="#" data-tab-filter="tags" data-filter-placeholder="Find a tag…" class="js-select-menu-tab" role="tab">Tags</a>
            </li>
          </ul>
        </div>
      </div>

      <div class="select-menu-list select-menu-tab-bucket js-select-menu-tab-bucket" data-tab-filter="branches" role="menu">

        <div data-filterable-for="context-commitish-filter-field" data-filterable-type="substring">


            <a class="select-menu-item js-navigation-item js-navigation-open "
               href="/alexed1/fr8company/blob/DO-813/Scripts/tests/integration/PluginActionExecuteTest.js"
               data-name="DO-813"
               data-skip-pjax="true"
               rel="nofollow">
              <span class="select-menu-item-icon octicon octicon-check"></span>
              <span class="select-menu-item-text css-truncate-target" title="DO-813">
                DO-813
              </span>
            </a>
            <a class="select-menu-item js-navigation-item js-navigation-open "
               href="/alexed1/fr8company/blob/DO-920/Scripts/tests/integration/PluginActionExecuteTest.js"
               data-name="DO-920"
               data-skip-pjax="true"
               rel="nofollow">
              <span class="select-menu-item-icon octicon octicon-check"></span>
              <span class="select-menu-item-text css-truncate-target" title="DO-920">
                DO-920
              </span>
            </a>
            <a class="select-menu-item js-navigation-item js-navigation-open "
               href="/alexed1/fr8company/blob/DO-1022/Scripts/tests/integration/PluginActionExecuteTest.js"
               data-name="DO-1022"
               data-skip-pjax="true"
               rel="nofollow">
              <span class="select-menu-item-icon octicon octicon-check"></span>
              <span class="select-menu-item-text css-truncate-target" title="DO-1022">
                DO-1022
              </span>
            </a>
            <a class="select-menu-item js-navigation-item js-navigation-open "
               href="/alexed1/fr8company/blob/DO-1026/Scripts/tests/integration/PluginActionExecuteTest.js"
               data-name="DO-1026"
               data-skip-pjax="true"
               rel="nofollow">
              <span class="select-menu-item-icon octicon octicon-check"></span>
              <span class="select-menu-item-text css-truncate-target" title="DO-1026">
                DO-1026
              </span>
            </a>
            <a class="select-menu-item js-navigation-item js-navigation-open "
               href="/alexed1/fr8company/blob/DO-1044/Scripts/tests/integration/PluginActionExecuteTest.js"
               data-name="DO-1044"
               data-skip-pjax="true"
               rel="nofollow">
              <span class="select-menu-item-icon octicon octicon-check"></span>
              <span class="select-menu-item-text css-truncate-target" title="DO-1044">
                DO-1044
              </span>
            </a>
            <a class="select-menu-item js-navigation-item js-navigation-open "
               href="/alexed1/fr8company/blob/DO-1048/Scripts/tests/integration/PluginActionExecuteTest.js"
               data-name="DO-1048"
               data-skip-pjax="true"
               rel="nofollow">
              <span class="select-menu-item-icon octicon octicon-check"></span>
              <span class="select-menu-item-text css-truncate-target" title="DO-1048">
                DO-1048
              </span>
            </a>
            <a class="select-menu-item js-navigation-item js-navigation-open "
               href="/alexed1/fr8company/blob/DO-1049/Scripts/tests/integration/PluginActionExecuteTest.js"
               data-name="DO-1049"
               data-skip-pjax="true"
               rel="nofollow">
              <span class="select-menu-item-icon octicon octicon-check"></span>
              <span class="select-menu-item-text css-truncate-target" title="DO-1049">
                DO-1049
              </span>
            </a>
            <a class="select-menu-item js-navigation-item js-navigation-open "
               href="/alexed1/fr8company/blob/DO-1064/Scripts/tests/integration/PluginActionExecuteTest.js"
               data-name="DO-1064"
               data-skip-pjax="true"
               rel="nofollow">
              <span class="select-menu-item-icon octicon octicon-check"></span>
              <span class="select-menu-item-text css-truncate-target" title="DO-1064">
                DO-1064
              </span>
            </a>
            <a class="select-menu-item js-navigation-item js-navigation-open "
               href="/alexed1/fr8company/blob/DO-1066/Scripts/tests/integration/PluginActionExecuteTest.js"
               data-name="DO-1066"
               data-skip-pjax="true"
               rel="nofollow">
              <span class="select-menu-item-icon octicon octicon-check"></span>
              <span class="select-menu-item-text css-truncate-target" title="DO-1066">
                DO-1066
              </span>
            </a>
            <a class="select-menu-item js-navigation-item js-navigation-open "
               href="/alexed1/fr8company/blob/DO-1067/Scripts/tests/integration/PluginActionExecuteTest.js"
               data-name="DO-1067"
               data-skip-pjax="true"
               rel="nofollow">
              <span class="select-menu-item-icon octicon octicon-check"></span>
              <span class="select-menu-item-text css-truncate-target" title="DO-1067">
                DO-1067
              </span>
            </a>
            <a class="select-menu-item js-navigation-item js-navigation-open "
               href="/alexed1/fr8company/blob/DO-1067-V2/Scripts/tests/integration/PluginActionExecuteTest.js"
               data-name="DO-1067-V2"
               data-skip-pjax="true"
               rel="nofollow">
              <span class="select-menu-item-icon octicon octicon-check"></span>
              <span class="select-menu-item-text css-truncate-target" title="DO-1067-V2">
                DO-1067-V2
              </span>
            </a>
            <a class="select-menu-item js-navigation-item js-navigation-open "
               href="/alexed1/fr8company/blob/DO-1068/Scripts/tests/integration/PluginActionExecuteTest.js"
               data-name="DO-1068"
               data-skip-pjax="true"
               rel="nofollow">
              <span class="select-menu-item-icon octicon octicon-check"></span>
              <span class="select-menu-item-text css-truncate-target" title="DO-1068">
                DO-1068
              </span>
            </a>
            <a class="select-menu-item js-navigation-item js-navigation-open "
               href="/alexed1/fr8company/blob/DO-1070/Scripts/tests/integration/PluginActionExecuteTest.js"
               data-name="DO-1070"
               data-skip-pjax="true"
               rel="nofollow">
              <span class="select-menu-item-icon octicon octicon-check"></span>
              <span class="select-menu-item-text css-truncate-target" title="DO-1070">
                DO-1070
              </span>
            </a>
            <a class="select-menu-item js-navigation-item js-navigation-open "
               href="/alexed1/fr8company/blob/DO-1078/Scripts/tests/integration/PluginActionExecuteTest.js"
               data-name="DO-1078"
               data-skip-pjax="true"
               rel="nofollow">
              <span class="select-menu-item-icon octicon octicon-check"></span>
              <span class="select-menu-item-text css-truncate-target" title="DO-1078">
                DO-1078
              </span>
            </a>
            <a class="select-menu-item js-navigation-item js-navigation-open "
               href="/alexed1/fr8company/blob/DO-1081/Scripts/tests/integration/PluginActionExecuteTest.js"
               data-name="DO-1081"
               data-skip-pjax="true"
               rel="nofollow">
              <span class="select-menu-item-icon octicon octicon-check"></span>
              <span class="select-menu-item-text css-truncate-target" title="DO-1081">
                DO-1081
              </span>
            </a>
            <a class="select-menu-item js-navigation-item js-navigation-open "
               href="/alexed1/fr8company/blob/DO-1086/Scripts/tests/integration/PluginActionExecuteTest.js"
               data-name="DO-1086"
               data-skip-pjax="true"
               rel="nofollow">
              <span class="select-menu-item-icon octicon octicon-check"></span>
              <span class="select-menu-item-text css-truncate-target" title="DO-1086">
                DO-1086
              </span>
            </a>
            <a class="select-menu-item js-navigation-item js-navigation-open "
               href="/alexed1/fr8company/blob/DO-1087/Scripts/tests/integration/PluginActionExecuteTest.js"
               data-name="DO-1087"
               data-skip-pjax="true"
               rel="nofollow">
              <span class="select-menu-item-icon octicon octicon-check"></span>
              <span class="select-menu-item-text css-truncate-target" title="DO-1087">
                DO-1087
              </span>
            </a>
            <a class="select-menu-item js-navigation-item js-navigation-open "
               href="/alexed1/fr8company/blob/DO-1090_v2/Scripts/tests/integration/PluginActionExecuteTest.js"
               data-name="DO-1090_v2"
               data-skip-pjax="true"
               rel="nofollow">
              <span class="select-menu-item-icon octicon octicon-check"></span>
              <span class="select-menu-item-text css-truncate-target" title="DO-1090_v2">
                DO-1090_v2
              </span>
            </a>
            <a class="select-menu-item js-navigation-item js-navigation-open "
               href="/alexed1/fr8company/blob/DO-1094/Scripts/tests/integration/PluginActionExecuteTest.js"
               data-name="DO-1094"
               data-skip-pjax="true"
               rel="nofollow">
              <span class="select-menu-item-icon octicon octicon-check"></span>
              <span class="select-menu-item-text css-truncate-target" title="DO-1094">
                DO-1094
              </span>
            </a>
            <a class="select-menu-item js-navigation-item js-navigation-open "
               href="/alexed1/fr8company/blob/DO-1103-v2-%40alexavrutin/Scripts/tests/integration/PluginActionExecuteTest.js"
               data-name="DO-1103-v2-@alexavrutin"
               data-skip-pjax="true"
               rel="nofollow">
              <span class="select-menu-item-icon octicon octicon-check"></span>
              <span class="select-menu-item-text css-truncate-target" title="DO-1103-v2-@alexavrutin">
                DO-1103-v2-@alexavrutin
              </span>
            </a>
            <a class="select-menu-item js-navigation-item js-navigation-open "
               href="/alexed1/fr8company/blob/DO-1107/Scripts/tests/integration/PluginActionExecuteTest.js"
               data-name="DO-1107"
               data-skip-pjax="true"
               rel="nofollow">
              <span class="select-menu-item-icon octicon octicon-check"></span>
              <span class="select-menu-item-text css-truncate-target" title="DO-1107">
                DO-1107
              </span>
            </a>
            <a class="select-menu-item js-navigation-item js-navigation-open "
               href="/alexed1/fr8company/blob/DO-1113/Scripts/tests/integration/PluginActionExecuteTest.js"
               data-name="DO-1113"
               data-skip-pjax="true"
               rel="nofollow">
              <span class="select-menu-item-icon octicon octicon-check"></span>
              <span class="select-menu-item-text css-truncate-target" title="DO-1113">
                DO-1113
              </span>
            </a>
            <a class="select-menu-item js-navigation-item js-navigation-open "
               href="/alexed1/fr8company/blob/DO-1123/Scripts/tests/integration/PluginActionExecuteTest.js"
               data-name="DO-1123"
               data-skip-pjax="true"
               rel="nofollow">
              <span class="select-menu-item-icon octicon octicon-check"></span>
              <span class="select-menu-item-text css-truncate-target" title="DO-1123">
                DO-1123
              </span>
            </a>
            <a class="select-menu-item js-navigation-item js-navigation-open "
               href="/alexed1/fr8company/blob/DO-1128/Scripts/tests/integration/PluginActionExecuteTest.js"
               data-name="DO-1128"
               data-skip-pjax="true"
               rel="nofollow">
              <span class="select-menu-item-icon octicon octicon-check"></span>
              <span class="select-menu-item-text css-truncate-target" title="DO-1128">
                DO-1128
              </span>
            </a>
            <a class="select-menu-item js-navigation-item js-navigation-open "
               href="/alexed1/fr8company/blob/DO-1133/Scripts/tests/integration/PluginActionExecuteTest.js"
               data-name="DO-1133"
               data-skip-pjax="true"
               rel="nofollow">
              <span class="select-menu-item-icon octicon octicon-check"></span>
              <span class="select-menu-item-text css-truncate-target" title="DO-1133">
                DO-1133
              </span>
            </a>
            <a class="select-menu-item js-navigation-item js-navigation-open "
               href="/alexed1/fr8company/blob/DO-1145/Scripts/tests/integration/PluginActionExecuteTest.js"
               data-name="DO-1145"
               data-skip-pjax="true"
               rel="nofollow">
              <span class="select-menu-item-icon octicon octicon-check"></span>
              <span class="select-menu-item-text css-truncate-target" title="DO-1145">
                DO-1145
              </span>
            </a>
            <a class="select-menu-item js-navigation-item js-navigation-open "
               href="/alexed1/fr8company/blob/DO-1166/Scripts/tests/integration/PluginActionExecuteTest.js"
               data-name="DO-1166"
               data-skip-pjax="true"
               rel="nofollow">
              <span class="select-menu-item-icon octicon octicon-check"></span>
              <span class="select-menu-item-text css-truncate-target" title="DO-1166">
                DO-1166
              </span>
            </a>
            <a class="select-menu-item js-navigation-item js-navigation-open "
               href="/alexed1/fr8company/blob/DO-1174_v2/Scripts/tests/integration/PluginActionExecuteTest.js"
               data-name="DO-1174_v2"
               data-skip-pjax="true"
               rel="nofollow">
              <span class="select-menu-item-icon octicon octicon-check"></span>
              <span class="select-menu-item-text css-truncate-target" title="DO-1174_v2">
                DO-1174_v2
              </span>
            </a>
            <a class="select-menu-item js-navigation-item js-navigation-open "
               href="/alexed1/fr8company/blob/DO-1176/Scripts/tests/integration/PluginActionExecuteTest.js"
               data-name="DO-1176"
               data-skip-pjax="true"
               rel="nofollow">
              <span class="select-menu-item-icon octicon octicon-check"></span>
              <span class="select-menu-item-text css-truncate-target" title="DO-1176">
                DO-1176
              </span>
            </a>
            <a class="select-menu-item js-navigation-item js-navigation-open "
               href="/alexed1/fr8company/blob/DO-1176neat/Scripts/tests/integration/PluginActionExecuteTest.js"
               data-name="DO-1176neat"
               data-skip-pjax="true"
               rel="nofollow">
              <span class="select-menu-item-icon octicon octicon-check"></span>
              <span class="select-menu-item-text css-truncate-target" title="DO-1176neat">
                DO-1176neat
              </span>
            </a>
            <a class="select-menu-item js-navigation-item js-navigation-open "
               href="/alexed1/fr8company/blob/DO-1186/Scripts/tests/integration/PluginActionExecuteTest.js"
               data-name="DO-1186"
               data-skip-pjax="true"
               rel="nofollow">
              <span class="select-menu-item-icon octicon octicon-check"></span>
              <span class="select-menu-item-text css-truncate-target" title="DO-1186">
                DO-1186
              </span>
            </a>
            <a class="select-menu-item js-navigation-item js-navigation-open "
               href="/alexed1/fr8company/blob/DO-1187/Scripts/tests/integration/PluginActionExecuteTest.js"
               data-name="DO-1187"
               data-skip-pjax="true"
               rel="nofollow">
              <span class="select-menu-item-icon octicon octicon-check"></span>
              <span class="select-menu-item-text css-truncate-target" title="DO-1187">
                DO-1187
              </span>
            </a>
            <a class="select-menu-item js-navigation-item js-navigation-open "
               href="/alexed1/fr8company/blob/DO-1192/Scripts/tests/integration/PluginActionExecuteTest.js"
               data-name="DO-1192"
               data-skip-pjax="true"
               rel="nofollow">
              <span class="select-menu-item-icon octicon octicon-check"></span>
              <span class="select-menu-item-text css-truncate-target" title="DO-1192">
                DO-1192
              </span>
            </a>
            <a class="select-menu-item js-navigation-item js-navigation-open "
               href="/alexed1/fr8company/blob/DO-1194/Scripts/tests/integration/PluginActionExecuteTest.js"
               data-name="DO-1194"
               data-skip-pjax="true"
               rel="nofollow">
              <span class="select-menu-item-icon octicon octicon-check"></span>
              <span class="select-menu-item-text css-truncate-target" title="DO-1194">
                DO-1194
              </span>
            </a>
            <a class="select-menu-item js-navigation-item js-navigation-open "
               href="/alexed1/fr8company/blob/DO-1195/Scripts/tests/integration/PluginActionExecuteTest.js"
               data-name="DO-1195"
               data-skip-pjax="true"
               rel="nofollow">
              <span class="select-menu-item-icon octicon octicon-check"></span>
              <span class="select-menu-item-text css-truncate-target" title="DO-1195">
                DO-1195
              </span>
            </a>
            <a class="select-menu-item js-navigation-item js-navigation-open "
               href="/alexed1/fr8company/blob/DO-1195-yakov/Scripts/tests/integration/PluginActionExecuteTest.js"
               data-name="DO-1195-yakov"
               data-skip-pjax="true"
               rel="nofollow">
              <span class="select-menu-item-icon octicon octicon-check"></span>
              <span class="select-menu-item-text css-truncate-target" title="DO-1195-yakov">
                DO-1195-yakov
              </span>
            </a>
            <a class="select-menu-item js-navigation-item js-navigation-open "
               href="/alexed1/fr8company/blob/DO-1196/Scripts/tests/integration/PluginActionExecuteTest.js"
               data-name="DO-1196"
               data-skip-pjax="true"
               rel="nofollow">
              <span class="select-menu-item-icon octicon octicon-check"></span>
              <span class="select-menu-item-text css-truncate-target" title="DO-1196">
                DO-1196
              </span>
            </a>
            <a class="select-menu-item js-navigation-item js-navigation-open "
               href="/alexed1/fr8company/blob/DO-1198-46/Scripts/tests/integration/PluginActionExecuteTest.js"
               data-name="DO-1198-46"
               data-skip-pjax="true"
               rel="nofollow">
              <span class="select-menu-item-icon octicon octicon-check"></span>
              <span class="select-menu-item-text css-truncate-target" title="DO-1198-46">
                DO-1198-46
              </span>
            </a>
            <a class="select-menu-item js-navigation-item js-navigation-open "
               href="/alexed1/fr8company/blob/DO-1206/Scripts/tests/integration/PluginActionExecuteTest.js"
               data-name="DO-1206"
               data-skip-pjax="true"
               rel="nofollow">
              <span class="select-menu-item-icon octicon octicon-check"></span>
              <span class="select-menu-item-text css-truncate-target" title="DO-1206">
                DO-1206
              </span>
            </a>
            <a class="select-menu-item js-navigation-item js-navigation-open "
               href="/alexed1/fr8company/blob/DO-1211/Scripts/tests/integration/PluginActionExecuteTest.js"
               data-name="DO-1211"
               data-skip-pjax="true"
               rel="nofollow">
              <span class="select-menu-item-icon octicon octicon-check"></span>
              <span class="select-menu-item-text css-truncate-target" title="DO-1211">
                DO-1211
              </span>
            </a>
            <a class="select-menu-item js-navigation-item js-navigation-open "
               href="/alexed1/fr8company/blob/DO-1213/Scripts/tests/integration/PluginActionExecuteTest.js"
               data-name="DO-1213"
               data-skip-pjax="true"
               rel="nofollow">
              <span class="select-menu-item-icon octicon octicon-check"></span>
              <span class="select-menu-item-text css-truncate-target" title="DO-1213">
                DO-1213
              </span>
            </a>
            <a class="select-menu-item js-navigation-item js-navigation-open "
               href="/alexed1/fr8company/blob/DO-1214/Scripts/tests/integration/PluginActionExecuteTest.js"
               data-name="DO-1214"
               data-skip-pjax="true"
               rel="nofollow">
              <span class="select-menu-item-icon octicon octicon-check"></span>
              <span class="select-menu-item-text css-truncate-target" title="DO-1214">
                DO-1214
              </span>
            </a>
            <a class="select-menu-item js-navigation-item js-navigation-open "
               href="/alexed1/fr8company/blob/DO-1215/Scripts/tests/integration/PluginActionExecuteTest.js"
               data-name="DO-1215"
               data-skip-pjax="true"
               rel="nofollow">
              <span class="select-menu-item-icon octicon octicon-check"></span>
              <span class="select-menu-item-text css-truncate-target" title="DO-1215">
                DO-1215
              </span>
            </a>
            <a class="select-menu-item js-navigation-item js-navigation-open "
               href="/alexed1/fr8company/blob/DO-1216/Scripts/tests/integration/PluginActionExecuteTest.js"
               data-name="DO-1216"
               data-skip-pjax="true"
               rel="nofollow">
              <span class="select-menu-item-icon octicon octicon-check"></span>
              <span class="select-menu-item-text css-truncate-target" title="DO-1216">
                DO-1216
              </span>
            </a>
            <a class="select-menu-item js-navigation-item js-navigation-open "
               href="/alexed1/fr8company/blob/DO-1222/Scripts/tests/integration/PluginActionExecuteTest.js"
               data-name="DO-1222"
               data-skip-pjax="true"
               rel="nofollow">
              <span class="select-menu-item-icon octicon octicon-check"></span>
              <span class="select-menu-item-text css-truncate-target" title="DO-1222">
                DO-1222
              </span>
            </a>
            <a class="select-menu-item js-navigation-item js-navigation-open "
               href="/alexed1/fr8company/blob/DO-1232/Scripts/tests/integration/PluginActionExecuteTest.js"
               data-name="DO-1232"
               data-skip-pjax="true"
               rel="nofollow">
              <span class="select-menu-item-icon octicon octicon-check"></span>
              <span class="select-menu-item-text css-truncate-target" title="DO-1232">
                DO-1232
              </span>
            </a>
            <a class="select-menu-item js-navigation-item js-navigation-open "
               href="/alexed1/fr8company/blob/DO-1236/Scripts/tests/integration/PluginActionExecuteTest.js"
               data-name="DO-1236"
               data-skip-pjax="true"
               rel="nofollow">
              <span class="select-menu-item-icon octicon octicon-check"></span>
              <span class="select-menu-item-text css-truncate-target" title="DO-1236">
                DO-1236
              </span>
            </a>
            <a class="select-menu-item js-navigation-item js-navigation-open "
               href="/alexed1/fr8company/blob/DO-1238/Scripts/tests/integration/PluginActionExecuteTest.js"
               data-name="DO-1238"
               data-skip-pjax="true"
               rel="nofollow">
              <span class="select-menu-item-icon octicon octicon-check"></span>
              <span class="select-menu-item-text css-truncate-target" title="DO-1238">
                DO-1238
              </span>
            </a>
            <a class="select-menu-item js-navigation-item js-navigation-open "
               href="/alexed1/fr8company/blob/DO-1239/Scripts/tests/integration/PluginActionExecuteTest.js"
               data-name="DO-1239"
               data-skip-pjax="true"
               rel="nofollow">
              <span class="select-menu-item-icon octicon octicon-check"></span>
              <span class="select-menu-item-text css-truncate-target" title="DO-1239">
                DO-1239
              </span>
            </a>
            <a class="select-menu-item js-navigation-item js-navigation-open "
               href="/alexed1/fr8company/blob/DO-1242/Scripts/tests/integration/PluginActionExecuteTest.js"
               data-name="DO-1242"
               data-skip-pjax="true"
               rel="nofollow">
              <span class="select-menu-item-icon octicon octicon-check"></span>
              <span class="select-menu-item-text css-truncate-target" title="DO-1242">
                DO-1242
              </span>
            </a>
            <a class="select-menu-item js-navigation-item js-navigation-open "
               href="/alexed1/fr8company/blob/DO-1244/Scripts/tests/integration/PluginActionExecuteTest.js"
               data-name="DO-1244"
               data-skip-pjax="true"
               rel="nofollow">
              <span class="select-menu-item-icon octicon octicon-check"></span>
              <span class="select-menu-item-text css-truncate-target" title="DO-1244">
                DO-1244
              </span>
            </a>
            <a class="select-menu-item js-navigation-item js-navigation-open "
               href="/alexed1/fr8company/blob/DO-1244-1/Scripts/tests/integration/PluginActionExecuteTest.js"
               data-name="DO-1244-1"
               data-skip-pjax="true"
               rel="nofollow">
              <span class="select-menu-item-icon octicon octicon-check"></span>
              <span class="select-menu-item-text css-truncate-target" title="DO-1244-1">
                DO-1244-1
              </span>
            </a>
            <a class="select-menu-item js-navigation-item js-navigation-open "
               href="/alexed1/fr8company/blob/DO-1244.bak/Scripts/tests/integration/PluginActionExecuteTest.js"
               data-name="DO-1244.bak"
               data-skip-pjax="true"
               rel="nofollow">
              <span class="select-menu-item-icon octicon octicon-check"></span>
              <span class="select-menu-item-text css-truncate-target" title="DO-1244.bak">
                DO-1244.bak
              </span>
            </a>
            <a class="select-menu-item js-navigation-item js-navigation-open "
               href="/alexed1/fr8company/blob/DO-1248/Scripts/tests/integration/PluginActionExecuteTest.js"
               data-name="DO-1248"
               data-skip-pjax="true"
               rel="nofollow">
              <span class="select-menu-item-icon octicon octicon-check"></span>
              <span class="select-menu-item-text css-truncate-target" title="DO-1248">
                DO-1248
              </span>
            </a>
            <a class="select-menu-item js-navigation-item js-navigation-open "
               href="/alexed1/fr8company/blob/DO-1250/Scripts/tests/integration/PluginActionExecuteTest.js"
               data-name="DO-1250"
               data-skip-pjax="true"
               rel="nofollow">
              <span class="select-menu-item-icon octicon octicon-check"></span>
              <span class="select-menu-item-text css-truncate-target" title="DO-1250">
                DO-1250
              </span>
            </a>
            <a class="select-menu-item js-navigation-item js-navigation-open "
               href="/alexed1/fr8company/blob/DO-1251/Scripts/tests/integration/PluginActionExecuteTest.js"
               data-name="DO-1251"
               data-skip-pjax="true"
               rel="nofollow">
              <span class="select-menu-item-icon octicon octicon-check"></span>
              <span class="select-menu-item-text css-truncate-target" title="DO-1251">
                DO-1251
              </span>
            </a>
            <a class="select-menu-item js-navigation-item js-navigation-open "
               href="/alexed1/fr8company/blob/DO-1274/Scripts/tests/integration/PluginActionExecuteTest.js"
               data-name="DO-1274"
               data-skip-pjax="true"
               rel="nofollow">
              <span class="select-menu-item-icon octicon octicon-check"></span>
              <span class="select-menu-item-text css-truncate-target" title="DO-1274">
                DO-1274
              </span>
            </a>
            <a class="select-menu-item js-navigation-item js-navigation-open "
               href="/alexed1/fr8company/blob/DO-1281/Scripts/tests/integration/PluginActionExecuteTest.js"
               data-name="DO-1281"
               data-skip-pjax="true"
               rel="nofollow">
              <span class="select-menu-item-icon octicon octicon-check"></span>
              <span class="select-menu-item-text css-truncate-target" title="DO-1281">
                DO-1281
              </span>
            </a>
            <a class="select-menu-item js-navigation-item js-navigation-open "
               href="/alexed1/fr8company/blob/DO-1282/Scripts/tests/integration/PluginActionExecuteTest.js"
               data-name="DO-1282"
               data-skip-pjax="true"
               rel="nofollow">
              <span class="select-menu-item-icon octicon octicon-check"></span>
              <span class="select-menu-item-text css-truncate-target" title="DO-1282">
                DO-1282
              </span>
            </a>
            <a class="select-menu-item js-navigation-item js-navigation-open "
               href="/alexed1/fr8company/blob/DO-1284/Scripts/tests/integration/PluginActionExecuteTest.js"
               data-name="DO-1284"
               data-skip-pjax="true"
               rel="nofollow">
              <span class="select-menu-item-icon octicon octicon-check"></span>
              <span class="select-menu-item-text css-truncate-target" title="DO-1284">
                DO-1284
              </span>
            </a>
            <a class="select-menu-item js-navigation-item js-navigation-open "
               href="/alexed1/fr8company/blob/DO-1284-1/Scripts/tests/integration/PluginActionExecuteTest.js"
               data-name="DO-1284-1"
               data-skip-pjax="true"
               rel="nofollow">
              <span class="select-menu-item-icon octicon octicon-check"></span>
              <span class="select-menu-item-text css-truncate-target" title="DO-1284-1">
                DO-1284-1
              </span>
            </a>
            <a class="select-menu-item js-navigation-item js-navigation-open "
               href="/alexed1/fr8company/blob/DO-1285/Scripts/tests/integration/PluginActionExecuteTest.js"
               data-name="DO-1285"
               data-skip-pjax="true"
               rel="nofollow">
              <span class="select-menu-item-icon octicon octicon-check"></span>
              <span class="select-menu-item-text css-truncate-target" title="DO-1285">
                DO-1285
              </span>
            </a>
            <a class="select-menu-item js-navigation-item js-navigation-open "
               href="/alexed1/fr8company/blob/DO-1290/Scripts/tests/integration/PluginActionExecuteTest.js"
               data-name="DO-1290"
               data-skip-pjax="true"
               rel="nofollow">
              <span class="select-menu-item-icon octicon octicon-check"></span>
              <span class="select-menu-item-text css-truncate-target" title="DO-1290">
                DO-1290
              </span>
            </a>
            <a class="select-menu-item js-navigation-item js-navigation-open "
               href="/alexed1/fr8company/blob/DO-1290_v2/Scripts/tests/integration/PluginActionExecuteTest.js"
               data-name="DO-1290_v2"
               data-skip-pjax="true"
               rel="nofollow">
              <span class="select-menu-item-icon octicon octicon-check"></span>
              <span class="select-menu-item-text css-truncate-target" title="DO-1290_v2">
                DO-1290_v2
              </span>
            </a>
            <a class="select-menu-item js-navigation-item js-navigation-open "
               href="/alexed1/fr8company/blob/DO-1291/Scripts/tests/integration/PluginActionExecuteTest.js"
               data-name="DO-1291"
               data-skip-pjax="true"
               rel="nofollow">
              <span class="select-menu-item-icon octicon octicon-check"></span>
              <span class="select-menu-item-text css-truncate-target" title="DO-1291">
                DO-1291
              </span>
            </a>
            <a class="select-menu-item js-navigation-item js-navigation-open "
               href="/alexed1/fr8company/blob/DO-1294/Scripts/tests/integration/PluginActionExecuteTest.js"
               data-name="DO-1294"
               data-skip-pjax="true"
               rel="nofollow">
              <span class="select-menu-item-icon octicon octicon-check"></span>
              <span class="select-menu-item-text css-truncate-target" title="DO-1294">
                DO-1294
              </span>
            </a>
            <a class="select-menu-item js-navigation-item js-navigation-open "
               href="/alexed1/fr8company/blob/DO-1303/Scripts/tests/integration/PluginActionExecuteTest.js"
               data-name="DO-1303"
               data-skip-pjax="true"
               rel="nofollow">
              <span class="select-menu-item-icon octicon octicon-check"></span>
              <span class="select-menu-item-text css-truncate-target" title="DO-1303">
                DO-1303
              </span>
            </a>
            <a class="select-menu-item js-navigation-item js-navigation-open "
               href="/alexed1/fr8company/blob/DO-1306/Scripts/tests/integration/PluginActionExecuteTest.js"
               data-name="DO-1306"
               data-skip-pjax="true"
               rel="nofollow">
              <span class="select-menu-item-icon octicon octicon-check"></span>
              <span class="select-menu-item-text css-truncate-target" title="DO-1306">
                DO-1306
              </span>
            </a>
            <a class="select-menu-item js-navigation-item js-navigation-open "
               href="/alexed1/fr8company/blob/DO-1307/Scripts/tests/integration/PluginActionExecuteTest.js"
               data-name="DO-1307"
               data-skip-pjax="true"
               rel="nofollow">
              <span class="select-menu-item-icon octicon octicon-check"></span>
              <span class="select-menu-item-text css-truncate-target" title="DO-1307">
                DO-1307
              </span>
            </a>
            <a class="select-menu-item js-navigation-item js-navigation-open "
               href="/alexed1/fr8company/blob/DO-1310/Scripts/tests/integration/PluginActionExecuteTest.js"
               data-name="DO-1310"
               data-skip-pjax="true"
               rel="nofollow">
              <span class="select-menu-item-icon octicon octicon-check"></span>
              <span class="select-menu-item-text css-truncate-target" title="DO-1310">
                DO-1310
              </span>
            </a>
            <a class="select-menu-item js-navigation-item js-navigation-open "
               href="/alexed1/fr8company/blob/DO-1313/Scripts/tests/integration/PluginActionExecuteTest.js"
               data-name="DO-1313"
               data-skip-pjax="true"
               rel="nofollow">
              <span class="select-menu-item-icon octicon octicon-check"></span>
              <span class="select-menu-item-text css-truncate-target" title="DO-1313">
                DO-1313
              </span>
            </a>
            <a class="select-menu-item js-navigation-item js-navigation-open "
               href="/alexed1/fr8company/blob/DO-1316/Scripts/tests/integration/PluginActionExecuteTest.js"
               data-name="DO-1316"
               data-skip-pjax="true"
               rel="nofollow">
              <span class="select-menu-item-icon octicon octicon-check"></span>
              <span class="select-menu-item-text css-truncate-target" title="DO-1316">
                DO-1316
              </span>
            </a>
            <a class="select-menu-item js-navigation-item js-navigation-open "
               href="/alexed1/fr8company/blob/DO-1318/Scripts/tests/integration/PluginActionExecuteTest.js"
               data-name="DO-1318"
               data-skip-pjax="true"
               rel="nofollow">
              <span class="select-menu-item-icon octicon octicon-check"></span>
              <span class="select-menu-item-text css-truncate-target" title="DO-1318">
                DO-1318
              </span>
            </a>
            <a class="select-menu-item js-navigation-item js-navigation-open "
               href="/alexed1/fr8company/blob/DO-1329_d/Scripts/tests/integration/PluginActionExecuteTest.js"
               data-name="DO-1329_d"
               data-skip-pjax="true"
               rel="nofollow">
              <span class="select-menu-item-icon octicon octicon-check"></span>
              <span class="select-menu-item-text css-truncate-target" title="DO-1329_d">
                DO-1329_d
              </span>
            </a>
            <a class="select-menu-item js-navigation-item js-navigation-open "
               href="/alexed1/fr8company/blob/DO-1332/Scripts/tests/integration/PluginActionExecuteTest.js"
               data-name="DO-1332"
               data-skip-pjax="true"
               rel="nofollow">
              <span class="select-menu-item-icon octicon octicon-check"></span>
              <span class="select-menu-item-text css-truncate-target" title="DO-1332">
                DO-1332
              </span>
            </a>
            <a class="select-menu-item js-navigation-item js-navigation-open "
               href="/alexed1/fr8company/blob/DO-1339/Scripts/tests/integration/PluginActionExecuteTest.js"
               data-name="DO-1339"
               data-skip-pjax="true"
               rel="nofollow">
              <span class="select-menu-item-icon octicon octicon-check"></span>
              <span class="select-menu-item-text css-truncate-target" title="DO-1339">
                DO-1339
              </span>
            </a>
            <a class="select-menu-item js-navigation-item js-navigation-open "
               href="/alexed1/fr8company/blob/DO-1340/Scripts/tests/integration/PluginActionExecuteTest.js"
               data-name="DO-1340"
               data-skip-pjax="true"
               rel="nofollow">
              <span class="select-menu-item-icon octicon octicon-check"></span>
              <span class="select-menu-item-text css-truncate-target" title="DO-1340">
                DO-1340
              </span>
            </a>
            <a class="select-menu-item js-navigation-item js-navigation-open "
               href="/alexed1/fr8company/blob/DO-1349/Scripts/tests/integration/PluginActionExecuteTest.js"
               data-name="DO-1349"
               data-skip-pjax="true"
               rel="nofollow">
              <span class="select-menu-item-icon octicon octicon-check"></span>
              <span class="select-menu-item-text css-truncate-target" title="DO-1349">
                DO-1349
              </span>
            </a>
            <a class="select-menu-item js-navigation-item js-navigation-open "
               href="/alexed1/fr8company/blob/DO-1367/Scripts/tests/integration/PluginActionExecuteTest.js"
               data-name="DO-1367"
               data-skip-pjax="true"
               rel="nofollow">
              <span class="select-menu-item-icon octicon octicon-check"></span>
              <span class="select-menu-item-text css-truncate-target" title="DO-1367">
                DO-1367
              </span>
            </a>
            <a class="select-menu-item js-navigation-item js-navigation-open "
               href="/alexed1/fr8company/blob/Do-1145/Scripts/tests/integration/PluginActionExecuteTest.js"
               data-name="Do-1145"
               data-skip-pjax="true"
               rel="nofollow">
              <span class="select-menu-item-icon octicon octicon-check"></span>
              <span class="select-menu-item-text css-truncate-target" title="Do-1145">
                Do-1145
              </span>
            </a>
            <a class="select-menu-item js-navigation-item js-navigation-open "
               href="/alexed1/fr8company/blob/Do-1147/Scripts/tests/integration/PluginActionExecuteTest.js"
               data-name="Do-1147"
               data-skip-pjax="true"
               rel="nofollow">
              <span class="select-menu-item-icon octicon octicon-check"></span>
              <span class="select-menu-item-text css-truncate-target" title="Do-1147">
                Do-1147
              </span>
            </a>
            <a class="select-menu-item js-navigation-item js-navigation-open "
               href="/alexed1/fr8company/blob/Do-1147%2C1146/Scripts/tests/integration/PluginActionExecuteTest.js"
               data-name="Do-1147,1146"
               data-skip-pjax="true"
               rel="nofollow">
              <span class="select-menu-item-icon octicon octicon-check"></span>
              <span class="select-menu-item-text css-truncate-target" title="Do-1147,1146">
                Do-1147,1146
              </span>
            </a>
            <a class="select-menu-item js-navigation-item js-navigation-open "
               href="/alexed1/fr8company/blob/Newtonsoft-fix/Scripts/tests/integration/PluginActionExecuteTest.js"
               data-name="Newtonsoft-fix"
               data-skip-pjax="true"
               rel="nofollow">
              <span class="select-menu-item-icon octicon octicon-check"></span>
              <span class="select-menu-item-text css-truncate-target" title="Newtonsoft-fix">
                Newtonsoft-fix
              </span>
            </a>
            <a class="select-menu-item js-navigation-item js-navigation-open "
               href="/alexed1/fr8company/blob/VSO/Scripts/tests/integration/PluginActionExecuteTest.js"
               data-name="VSO"
               data-skip-pjax="true"
               rel="nofollow">
              <span class="select-menu-item-icon octicon octicon-check"></span>
              <span class="select-menu-item-text css-truncate-target" title="VSO">
                VSO
              </span>
            </a>
            <a class="select-menu-item js-navigation-item js-navigation-open "
               href="/alexed1/fr8company/blob/buildbranch/Scripts/tests/integration/PluginActionExecuteTest.js"
               data-name="buildbranch"
               data-skip-pjax="true"
               rel="nofollow">
              <span class="select-menu-item-icon octicon octicon-check"></span>
              <span class="select-menu-item-text css-truncate-target" title="buildbranch">
                buildbranch
              </span>
            </a>
            <a class="select-menu-item js-navigation-item js-navigation-open selected"
               href="/alexed1/fr8company/blob/dev/Scripts/tests/integration/PluginActionExecuteTest.js"
               data-name="dev"
               data-skip-pjax="true"
               rel="nofollow">
              <span class="select-menu-item-icon octicon octicon-check"></span>
              <span class="select-menu-item-text css-truncate-target" title="dev">
                dev
              </span>
            </a>
            <a class="select-menu-item js-navigation-item js-navigation-open "
               href="/alexed1/fr8company/blob/dev_backup/Scripts/tests/integration/PluginActionExecuteTest.js"
               data-name="dev_backup"
               data-skip-pjax="true"
               rel="nofollow">
              <span class="select-menu-item-icon octicon octicon-check"></span>
              <span class="select-menu-item-text css-truncate-target" title="dev_backup">
                dev_backup
              </span>
            </a>
            <a class="select-menu-item js-navigation-item js-navigation-open "
               href="/alexed1/fr8company/blob/devbackup/Scripts/tests/integration/PluginActionExecuteTest.js"
               data-name="devbackup"
               data-skip-pjax="true"
               rel="nofollow">
              <span class="select-menu-item-icon octicon octicon-check"></span>
              <span class="select-menu-item-text css-truncate-target" title="devbackup">
                devbackup
              </span>
            </a>
            <a class="select-menu-item js-navigation-item js-navigation-open "
               href="/alexed1/fr8company/blob/do-782/Scripts/tests/integration/PluginActionExecuteTest.js"
               data-name="do-782"
               data-skip-pjax="true"
               rel="nofollow">
              <span class="select-menu-item-icon octicon octicon-check"></span>
              <span class="select-menu-item-text css-truncate-target" title="do-782">
                do-782
              </span>
            </a>
            <a class="select-menu-item js-navigation-item js-navigation-open "
               href="/alexed1/fr8company/blob/do-1162/Scripts/tests/integration/PluginActionExecuteTest.js"
               data-name="do-1162"
               data-skip-pjax="true"
               rel="nofollow">
              <span class="select-menu-item-icon octicon octicon-check"></span>
              <span class="select-menu-item-text css-truncate-target" title="do-1162">
                do-1162
              </span>
            </a>
            <a class="select-menu-item js-navigation-item js-navigation-open "
               href="/alexed1/fr8company/blob/do-1329/Scripts/tests/integration/PluginActionExecuteTest.js"
               data-name="do-1329"
               data-skip-pjax="true"
               rel="nofollow">
              <span class="select-menu-item-icon octicon octicon-check"></span>
              <span class="select-menu-item-text css-truncate-target" title="do-1329">
                do-1329
              </span>
            </a>
            <a class="select-menu-item js-navigation-item js-navigation-open "
               href="/alexed1/fr8company/blob/feature/DO-1196/Scripts/tests/integration/PluginActionExecuteTest.js"
               data-name="feature/DO-1196"
               data-skip-pjax="true"
               rel="nofollow">
              <span class="select-menu-item-icon octicon octicon-check"></span>
              <span class="select-menu-item-text css-truncate-target" title="feature/DO-1196">
                feature/DO-1196
              </span>
            </a>
            <a class="select-menu-item js-navigation-item js-navigation-open "
               href="/alexed1/fr8company/blob/fr8home/Scripts/tests/integration/PluginActionExecuteTest.js"
               data-name="fr8home"
               data-skip-pjax="true"
               rel="nofollow">
              <span class="select-menu-item-icon octicon octicon-check"></span>
              <span class="select-menu-item-text css-truncate-target" title="fr8home">
                fr8home
              </span>
            </a>
            <a class="select-menu-item js-navigation-item js-navigation-open "
               href="/alexed1/fr8company/blob/master/Scripts/tests/integration/PluginActionExecuteTest.js"
               data-name="master"
               data-skip-pjax="true"
               rel="nofollow">
              <span class="select-menu-item-icon octicon octicon-check"></span>
              <span class="select-menu-item-text css-truncate-target" title="master">
                master
              </span>
            </a>
            <a class="select-menu-item js-navigation-item js-navigation-open "
               href="/alexed1/fr8company/blob/revert-332-DO-1090/Scripts/tests/integration/PluginActionExecuteTest.js"
               data-name="revert-332-DO-1090"
               data-skip-pjax="true"
               rel="nofollow">
              <span class="select-menu-item-icon octicon octicon-check"></span>
              <span class="select-menu-item-text css-truncate-target" title="revert-332-DO-1090">
                revert-332-DO-1090
              </span>
            </a>
            <a class="select-menu-item js-navigation-item js-navigation-open "
               href="/alexed1/fr8company/blob/revert-340-DO-1187/Scripts/tests/integration/PluginActionExecuteTest.js"
               data-name="revert-340-DO-1187"
               data-skip-pjax="true"
               rel="nofollow">
              <span class="select-menu-item-icon octicon octicon-check"></span>
              <span class="select-menu-item-text css-truncate-target" title="revert-340-DO-1187">
                revert-340-DO-1187
              </span>
            </a>
            <a class="select-menu-item js-navigation-item js-navigation-open "
               href="/alexed1/fr8company/blob/revert-347-DO-1244/Scripts/tests/integration/PluginActionExecuteTest.js"
               data-name="revert-347-DO-1244"
               data-skip-pjax="true"
               rel="nofollow">
              <span class="select-menu-item-icon octicon octicon-check"></span>
              <span class="select-menu-item-text css-truncate-target" title="revert-347-DO-1244">
                revert-347-DO-1244
              </span>
            </a>
            <a class="select-menu-item js-navigation-item js-navigation-open "
               href="/alexed1/fr8company/blob/revert-371-DO-1236/Scripts/tests/integration/PluginActionExecuteTest.js"
               data-name="revert-371-DO-1236"
               data-skip-pjax="true"
               rel="nofollow">
              <span class="select-menu-item-icon octicon octicon-check"></span>
              <span class="select-menu-item-text css-truncate-target" title="revert-371-DO-1236">
                revert-371-DO-1236
              </span>
            </a>
            <a class="select-menu-item js-navigation-item js-navigation-open "
               href="/alexed1/fr8company/blob/revert-400-DO-1196/Scripts/tests/integration/PluginActionExecuteTest.js"
               data-name="revert-400-DO-1196"
               data-skip-pjax="true"
               rel="nofollow">
              <span class="select-menu-item-icon octicon octicon-check"></span>
              <span class="select-menu-item-text css-truncate-target" title="revert-400-DO-1196">
                revert-400-DO-1196
              </span>
            </a>
            <a class="select-menu-item js-navigation-item js-navigation-open "
               href="/alexed1/fr8company/blob/revert-405-DO-1195/Scripts/tests/integration/PluginActionExecuteTest.js"
               data-name="revert-405-DO-1195"
               data-skip-pjax="true"
               rel="nofollow">
              <span class="select-menu-item-icon octicon octicon-check"></span>
              <span class="select-menu-item-text css-truncate-target" title="revert-405-DO-1195">
                revert-405-DO-1195
              </span>
            </a>
        </div>

          <!-- </textarea> --><!-- '"` --><form accept-charset="UTF-8" action="/alexed1/fr8company/branches" class="js-create-branch select-menu-item select-menu-new-item-form js-navigation-item js-new-item-form" data-form-nonce="87025f9b9d4f36784c77a32b1ca51ef5355bda59" method="post"><div style="margin:0;padding:0;display:inline"><input name="utf8" type="hidden" value="&#x2713;" /><input name="authenticity_token" type="hidden" value="X4wxRc7+s33MEgBnqvT+EJ7QzbZphFnQbcvBUhjLJQwGLTRjbiSpw8DZEkuZdP4/3F59ieHpU+Pworqkv3N+9A==" /></div>
            <span class="octicon octicon-git-branch select-menu-item-icon"></span>
            <div class="select-menu-item-text">
              <span class="select-menu-item-heading">Create branch: <span class="js-new-item-name"></span></span>
              <span class="description">from ‘dev’</span>
            </div>
            <input type="hidden" name="name" id="name" class="js-new-item-value">
            <input type="hidden" name="branch" id="branch" value="dev">
            <input type="hidden" name="path" id="path" value="Scripts/tests/integration/PluginActionExecuteTest.js">
</form>
      </div>

      <div class="select-menu-list select-menu-tab-bucket js-select-menu-tab-bucket" data-tab-filter="tags">
        <div data-filterable-for="context-commitish-filter-field" data-filterable-type="substring">


        </div>

        <div class="select-menu-no-results">Nothing to show</div>
      </div>

    </div>
  </div>
</div>

describe("plugins execute actions tests", function () {
    var returnedData;
    var apiUrl = "http://localhost:30643/api/containers";
    var testProcessId1;
    var testProcessId2;

    <div class="breadcrumb js-zeroclipboard-target">
      <span class="repo-root js-repo-root"><span itemscope="" itemtype="http://data-vocabulary.org/Breadcrumb"><a href="/alexed1/fr8company" class="" data-branch="dev" data-pjax="true" itemscope="url"><span itemprop="title">fr8company</span></a></span></span><span class="separator">/</span><span itemscope="" itemtype="http://data-vocabulary.org/Breadcrumb"><a href="/alexed1/fr8company/tree/dev/Scripts" class="" data-branch="dev" data-pjax="true" itemscope="url"><span itemprop="title">Scripts</span></a></span><span class="separator">/</span><span itemscope="" itemtype="http://data-vocabulary.org/Breadcrumb"><a href="/alexed1/fr8company/tree/dev/Scripts/tests" class="" data-branch="dev" data-pjax="true" itemscope="url"><span itemprop="title">tests</span></a></span><span class="separator">/</span><span itemscope="" itemtype="http://data-vocabulary.org/Breadcrumb"><a href="/alexed1/fr8company/tree/dev/Scripts/tests/integration" class="" data-branch="dev" data-pjax="true" itemscope="url"><span itemprop="title">integration</span></a></span><span class="separator">/</span><strong class="final-path">PluginActionExecuteTest.js</strong>
    </div>
  </div>
    

  <div class="commit-tease">
      <span class="right">
        <a class="commit-tease-sha" href="/alexed1/fr8company/commit/dcf6bbc57933039b33963d42b8f17b352da42948" data-pjax>
          dcf6bb
        </a>
        <time datetime="2015-10-14T15:57:38Z" is="relative-time">Oct 14, 2015</time>
      </span>
      <div>
        <img alt="@alexed1" class="avatar" height="20" src="https://avatars2.githubusercontent.com/u/3140883?v=3&amp;s=40" width="20" />
        <a href="/alexed1" class="user-mention" rel="author">alexed1</a>
          <a href="/alexed1/fr8company/commit/dcf6bbc57933039b33963d42b8f17b352da42948" class="message" data-pjax="true" title="Revert &quot;update Monitor_DocuSign_Event&quot;">Revert "update Monitor_DocuSign_Event"</a>
      </div>
    
    <div class="commit-tease-contributors">
      <a class="muted-link contributors-toggle" href="#blob_contributors_box" rel="facebox">
        <strong>3</strong>
         contributors
      </a>
          <a class="avatar-link tooltipped tooltipped-s" aria-label="manishmishra256" href="/alexed1/fr8company/commits/dev/Scripts/tests/integration/PluginActionExecuteTest.js?author=manishmishra256"><img alt="@manishmishra256" class="avatar" height="20" src="https://avatars3.githubusercontent.com/u/3243256?v=3&amp;s=40" width="20" /> </a>
    <a class="avatar-link tooltipped tooltipped-s" aria-label="alexed1" href="/alexed1/fr8company/commits/dev/Scripts/tests/integration/PluginActionExecuteTest.js?author=alexed1"><img alt="@alexed1" class="avatar" height="20" src="https://avatars2.githubusercontent.com/u/3140883?v=3&amp;s=40" width="20" /> </a>
    <a class="avatar-link tooltipped tooltipped-s" aria-label="blazingmind" href="/alexed1/fr8company/commits/dev/Scripts/tests/integration/PluginActionExecuteTest.js?author=blazingmind"><img alt="@blazingmind" class="avatar" height="20" src="https://avatars1.githubusercontent.com/u/14808328?v=3&amp;s=40" width="20" /> </a>
    
    
    </div>

    <div id="blob_contributors_box" style="display:none">
      <h2 class="facebox-header" data-facebox-id="facebox-header">Users who have contributed to this file</h2>
      <ul class="facebox-user-list" data-facebox-id="facebox-description">
          <li class="facebox-user-list-item">
            <img alt="@manishmishra256" height="24" src="https://avatars1.githubusercontent.com/u/3243256?v=3&amp;s=48" width="24" />
            <a href="/manishmishra256">manishmishra256</a>
          </li>
          <li class="facebox-user-list-item">
            <img alt="@alexed1" height="24" src="https://avatars0.githubusercontent.com/u/3140883?v=3&amp;s=48" width="24" />
            <a href="/alexed1">alexed1</a>
          </li>
          <li class="facebox-user-list-item">
            <img alt="@blazingmind" height="24" src="https://avatars3.githubusercontent.com/u/14808328?v=3&amp;s=48" width="24" />
            <a href="/blazingmind">blazingmind</a>
          </li>
      </ul>
    </div>
  </div>

<div class="file">
  <div class="file-header">
  <div class="file-actions">
    
    <div class="btn-group">
      <a href="/alexed1/fr8company/raw/dev/Scripts/tests/integration/PluginActionExecuteTest.js" class="btn btn-sm " id="raw-url">Raw</a>
        <a href="/alexed1/fr8company/blame/dev/Scripts/tests/integration/PluginActionExecuteTest.js" class="btn btn-sm js-update-url-with-hash">Blame</a>
      <a href="/alexed1/fr8company/commits/dev/Scripts/tests/integration/PluginActionExecuteTest.js" class="btn btn-sm " rel="nofollow">History</a>
    </div>
    
      <a class="octicon-btn tooltipped tooltipped-nw"
         href="github-windows://openRepo/https://github.com/alexed1/fr8company?branch=dev&amp;filepath=Scripts%2Ftests%2Fintegration%2FPluginActionExecuteTest.js"
         aria-label="Open this file in GitHub Desktop"
         data-ga-click="Repository, open with desktop, type:windows">
          <span class="octicon octicon-device-desktop"></span>
      </a>
    
        <!-- </textarea> --><!-- '"` --><form accept-charset="UTF-8" action="/alexed1/fr8company/edit/dev/Scripts/tests/integration/PluginActionExecuteTest.js" class="inline-form js-update-url-with-hash" data-form-nonce="87025f9b9d4f36784c77a32b1ca51ef5355bda59" method="post"><div style="margin:0;padding:0;display:inline"><input name="utf8" type="hidden" value="&#x2713;" /><input name="authenticity_token" type="hidden" value="Dsdw/pfk0EGELLScuFGUx5Kc60jIaL69qo/JIZBfQ4S5tEATqaH9y/UrIrqHiY+IWm85r9gbWokqBP2EY4Vl9g==" /></div>
          <button class="octicon-btn tooltipped tooltipped-nw" type="submit"
            aria-label="Edit this file" data-hotkey="e" data-disable-with>
            <span class="octicon octicon-pencil"></span>
          </button>
</form>        <!-- </textarea> --><!-- '"` --><form accept-charset="UTF-8" action="/alexed1/fr8company/delete/dev/Scripts/tests/integration/PluginActionExecuteTest.js" class="inline-form" data-form-nonce="87025f9b9d4f36784c77a32b1ca51ef5355bda59" method="post"><div style="margin:0;padding:0;display:inline"><input name="utf8" type="hidden" value="&#x2713;" /><input name="authenticity_token" type="hidden" value="0zTEj2D0mSFQ6MrCtVCvrpCrIO4HY4TeZlwI/hqCM279WcDc7hi417XOIyJC/qzz+Guf/oguMUD8xEjv9LAI/Q==" /></div>
          <button class="octicon-btn octicon-btn-danger tooltipped tooltipped-nw" type="submit"
            aria-label="Delete this file" data-disable-with>
            <span class="octicon octicon-trashcan"></span>
          </button>
</form>  </div>

  <div class="file-info">
      212 lines (174 sloc)
      <span class="file-info-divider"></span>
    6.59 KB
  </div>
</div>



  <div class="blob-wrapper data type-javascript">
      <table class="highlight tab-size js-file-line-container" data-tab-size="8">
      <tr>
        <td id="L1" class="blob-num js-line-number" data-line-number="1"></td>
        <td id="LC1" class="blob-code blob-code-inner js-file-line"><span class="pl-c">/// &lt;reference path=&quot;../../lib/jpattern.js&quot; /&gt;</span></td>
      </tr>
      <tr>
        <td id="L2" class="blob-num js-line-number" data-line-number="2"></td>
        <td id="LC2" class="blob-code blob-code-inner js-file-line">
</td>
      </tr>
      <tr>
        <td id="L3" class="blob-num js-line-number" data-line-number="3"></td>
        <td id="LC3" class="blob-code blob-code-inner js-file-line">describe(<span class="pl-s"><span class="pl-pds">&quot;</span>plugins execute actions tests<span class="pl-pds">&quot;</span></span>, <span class="pl-k">function</span> () {</td>
      </tr>
      <tr>
        <td id="L4" class="blob-num js-line-number" data-line-number="4"></td>
        <td id="LC4" class="blob-code blob-code-inner js-file-line">    <span class="pl-k">var</span> returnedData;</td>
      </tr>
      <tr>
        <td id="L5" class="blob-num js-line-number" data-line-number="5"></td>
        <td id="LC5" class="blob-code blob-code-inner js-file-line">    <span class="pl-k">var</span> apiUrl <span class="pl-k">=</span> <span class="pl-s"><span class="pl-pds">&quot;</span>http://localhost:30643/api/processes<span class="pl-pds">&quot;</span></span>;</td>
      </tr>
      <tr>
        <td id="L6" class="blob-num js-line-number" data-line-number="6"></td>
        <td id="LC6" class="blob-code blob-code-inner js-file-line">    <span class="pl-k">var</span> testProcessId1;</td>
      </tr>
      <tr>
        <td id="L7" class="blob-num js-line-number" data-line-number="7"></td>
        <td id="LC7" class="blob-code blob-code-inner js-file-line">    <span class="pl-k">var</span> testProcessId2;</td>
      </tr>
      <tr>
        <td id="L8" class="blob-num js-line-number" data-line-number="8"></td>
        <td id="LC8" class="blob-code blob-code-inner js-file-line">
</td>
      </tr>
      <tr>
        <td id="L9" class="blob-num js-line-number" data-line-number="9"></td>
        <td id="LC9" class="blob-code blob-code-inner js-file-line">    <span class="pl-k">var</span> <span class="pl-en">errorHandler</span> <span class="pl-k">=</span> <span class="pl-k">function</span>(<span class="pl-smi">response</span>, <span class="pl-smi">done</span>) {</td>
      </tr>
      <tr>
        <td id="L10" class="blob-num js-line-number" data-line-number="10"></td>
        <td id="LC10" class="blob-code blob-code-inner js-file-line">        <span class="pl-k">if</span> (response.<span class="pl-c1">status</span> <span class="pl-k">===</span> <span class="pl-c1">401</span>) {</td>
      </tr>
      <tr>
        <td id="L11" class="blob-num js-line-number" data-line-number="11"></td>
        <td id="LC11" class="blob-code blob-code-inner js-file-line">            <span class="pl-en">console</span>.<span class="pl-c1">log</span>(<span class="pl-s"><span class="pl-pds">&quot;</span>User is not logged in, to run these tests, please login<span class="pl-pds">&quot;</span></span>);</td>
      </tr>
      <tr>
        <td id="L12" class="blob-num js-line-number" data-line-number="12"></td>
        <td id="LC12" class="blob-code blob-code-inner js-file-line">        } <span class="pl-k">else</span> {</td>
      </tr>
      <tr>
        <td id="L13" class="blob-num js-line-number" data-line-number="13"></td>
        <td id="LC13" class="blob-code blob-code-inner js-file-line">            <span class="pl-en">console</span>.<span class="pl-c1">log</span>(<span class="pl-s"><span class="pl-pds">&quot;</span>Something went wrong<span class="pl-pds">&quot;</span></span>);</td>
      </tr>
      <tr>
        <td id="L14" class="blob-num js-line-number" data-line-number="14"></td>
        <td id="LC14" class="blob-code blob-code-inner js-file-line">            <span class="pl-en">console</span>.<span class="pl-c1">log</span>(response);</td>
      </tr>
      <tr>
        <td id="L15" class="blob-num js-line-number" data-line-number="15"></td>
        <td id="LC15" class="blob-code blob-code-inner js-file-line">        }</td>
      </tr>
      <tr>
        <td id="L16" class="blob-num js-line-number" data-line-number="16"></td>
        <td id="LC16" class="blob-code blob-code-inner js-file-line">        done.fail(response.responseText);</td>
      </tr>
      <tr>
        <td id="L17" class="blob-num js-line-number" data-line-number="17"></td>
        <td id="LC17" class="blob-code blob-code-inner js-file-line">    };</td>
      </tr>
      <tr>
        <td id="L18" class="blob-num js-line-number" data-line-number="18"></td>
        <td id="LC18" class="blob-code blob-code-inner js-file-line">    </td>
      </tr>
      <tr>
        <td id="L19" class="blob-num js-line-number" data-line-number="19"></td>
        <td id="LC19" class="blob-code blob-code-inner js-file-line">    <span class="pl-k">var</span> <span class="pl-en">getDataFromApi</span> <span class="pl-k">=</span> <span class="pl-k">function</span>(<span class="pl-smi">done</span>, <span class="pl-smi">url</span>, <span class="pl-smi">onContinue</span>) {</td>
      </tr>
      <tr>
        <td id="L20" class="blob-num js-line-number" data-line-number="20"></td>
        <td id="LC20" class="blob-code blob-code-inner js-file-line">        $.ajax({</td>
      </tr>
      <tr>
        <td id="L21" class="blob-num js-line-number" data-line-number="21"></td>
        <td id="LC21" class="blob-code blob-code-inner js-file-line">            type<span class="pl-k">:</span> <span class="pl-s"><span class="pl-pds">&quot;</span>GET<span class="pl-pds">&quot;</span></span>,</td>
      </tr>
      <tr>
        <td id="L22" class="blob-num js-line-number" data-line-number="22"></td>
        <td id="LC22" class="blob-code blob-code-inner js-file-line">            url<span class="pl-k">:</span> apiUrl <span class="pl-k">+</span> url,</td>
      </tr>
      <tr>
        <td id="L23" class="blob-num js-line-number" data-line-number="23"></td>
        <td id="LC23" class="blob-code blob-code-inner js-file-line">            contentType<span class="pl-k">:</span> <span class="pl-s"><span class="pl-pds">&quot;</span>application/json; charset=utf-8<span class="pl-pds">&quot;</span></span>,</td>
      </tr>
      <tr>
        <td id="L24" class="blob-num js-line-number" data-line-number="24"></td>
        <td id="LC24" class="blob-code blob-code-inner js-file-line">            dataType<span class="pl-k">:</span> <span class="pl-s"><span class="pl-pds">&quot;</span>json<span class="pl-pds">&quot;</span></span></td>
      </tr>
      <tr>
        <td id="L25" class="blob-num js-line-number" data-line-number="25"></td>
        <td id="LC25" class="blob-code blob-code-inner js-file-line">        }).done(<span class="pl-k">function</span>(<span class="pl-smi">data</span>) {</td>
      </tr>
      <tr>
        <td id="L26" class="blob-num js-line-number" data-line-number="26"></td>
        <td id="LC26" class="blob-code blob-code-inner js-file-line">            returnedData <span class="pl-k">=</span> data;</td>
      </tr>
      <tr>
        <td id="L27" class="blob-num js-line-number" data-line-number="27"></td>
        <td id="LC27" class="blob-code blob-code-inner js-file-line">            <span class="pl-en">console</span>.<span class="pl-c1">log</span>(<span class="pl-s"><span class="pl-pds">&quot;</span>Got it Sucessfully<span class="pl-pds">&quot;</span></span>);</td>
      </tr>
      <tr>
        <td id="L28" class="blob-num js-line-number" data-line-number="28"></td>
        <td id="LC28" class="blob-code blob-code-inner js-file-line">            <span class="pl-en">console</span>.<span class="pl-c1">log</span>(returnedData);</td>
      </tr>
      <tr>
        <td id="L29" class="blob-num js-line-number" data-line-number="29"></td>
        <td id="LC29" class="blob-code blob-code-inner js-file-line">            onContinue(done);</td>
      </tr>
      <tr>
        <td id="L30" class="blob-num js-line-number" data-line-number="30"></td>
        <td id="LC30" class="blob-code blob-code-inner js-file-line">        }).fail(<span class="pl-k">function</span>(<span class="pl-smi">response</span>) {</td>
      </tr>
      <tr>
        <td id="L31" class="blob-num js-line-number" data-line-number="31"></td>
        <td id="LC31" class="blob-code blob-code-inner js-file-line">            errorHandler(response, done);</td>
      </tr>
      <tr>
        <td id="L32" class="blob-num js-line-number" data-line-number="32"></td>
        <td id="LC32" class="blob-code blob-code-inner js-file-line">        });</td>
      </tr>
      <tr>
        <td id="L33" class="blob-num js-line-number" data-line-number="33"></td>
        <td id="LC33" class="blob-code blob-code-inner js-file-line">    }</td>
      </tr>
      <tr>
        <td id="L34" class="blob-num js-line-number" data-line-number="34"></td>
        <td id="LC34" class="blob-code blob-code-inner js-file-line">
</td>
      </tr>
      <tr>
        <td id="L35" class="blob-num js-line-number" data-line-number="35"></td>
        <td id="LC35" class="blob-code blob-code-inner js-file-line">    <span class="pl-k">var</span> <span class="pl-en">executePluginAction</span> <span class="pl-k">=</span> <span class="pl-k">function</span>(<span class="pl-smi">done</span>, <span class="pl-smi">pluginUrl</span>, <span class="pl-smi">actionData</span>, <span class="pl-smi">onContinue</span>) {</td>
      </tr>
      <tr>
        <td id="L36" class="blob-num js-line-number" data-line-number="36"></td>
        <td id="LC36" class="blob-code blob-code-inner js-file-line">        $.ajax({</td>
      </tr>
      <tr>
        <td id="L37" class="blob-num js-line-number" data-line-number="37"></td>
        <td id="LC37" class="blob-code blob-code-inner js-file-line">            type<span class="pl-k">:</span> <span class="pl-s"><span class="pl-pds">&quot;</span>POST<span class="pl-pds">&quot;</span></span>,</td>
      </tr>
      <tr>
        <td id="L38" class="blob-num js-line-number" data-line-number="38"></td>
        <td id="LC38" class="blob-code blob-code-inner js-file-line">            url<span class="pl-k">:</span> pluginUrl <span class="pl-k">+</span> <span class="pl-s"><span class="pl-pds">&quot;</span>/actions/execute<span class="pl-pds">&quot;</span></span>,</td>
      </tr>
      <tr>
        <td id="L39" class="blob-num js-line-number" data-line-number="39"></td>
        <td id="LC39" class="blob-code blob-code-inner js-file-line">            contentType<span class="pl-k">:</span> <span class="pl-s"><span class="pl-pds">&quot;</span>application/json; charset=utf-8<span class="pl-pds">&quot;</span></span>,</td>
      </tr>
      <tr>
        <td id="L40" class="blob-num js-line-number" data-line-number="40"></td>
        <td id="LC40" class="blob-code blob-code-inner js-file-line">            data<span class="pl-k">:</span> <span class="pl-c1">JSON</span>.stringify(actionData),</td>
      </tr>
      <tr>
        <td id="L41" class="blob-num js-line-number" data-line-number="41"></td>
        <td id="LC41" class="blob-code blob-code-inner js-file-line">            dataType<span class="pl-k">:</span> <span class="pl-s"><span class="pl-pds">&quot;</span>json<span class="pl-pds">&quot;</span></span></td>
      </tr>
      <tr>
        <td id="L42" class="blob-num js-line-number" data-line-number="42"></td>
        <td id="LC42" class="blob-code blob-code-inner js-file-line">        }).done(<span class="pl-k">function</span> (<span class="pl-smi">data</span>) {</td>
      </tr>
      <tr>
        <td id="L43" class="blob-num js-line-number" data-line-number="43"></td>
        <td id="LC43" class="blob-code blob-code-inner js-file-line">            returnedData <span class="pl-k">=</span> data;</td>
      </tr>
      <tr>
        <td id="L44" class="blob-num js-line-number" data-line-number="44"></td>
        <td id="LC44" class="blob-code blob-code-inner js-file-line">            <span class="pl-en">console</span>.<span class="pl-c1">log</span>(<span class="pl-s"><span class="pl-pds">&quot;</span>Got it Sucessfully<span class="pl-pds">&quot;</span></span>);</td>
      </tr>
      <tr>
        <td id="L45" class="blob-num js-line-number" data-line-number="45"></td>
        <td id="LC45" class="blob-code blob-code-inner js-file-line">            <span class="pl-en">console</span>.<span class="pl-c1">log</span>(returnedData);</td>
      </tr>
      <tr>
        <td id="L46" class="blob-num js-line-number" data-line-number="46"></td>
        <td id="LC46" class="blob-code blob-code-inner js-file-line">            onContinue(done);</td>
      </tr>
      <tr>
        <td id="L47" class="blob-num js-line-number" data-line-number="47"></td>
        <td id="LC47" class="blob-code blob-code-inner js-file-line">        }).fail(<span class="pl-k">function</span> (<span class="pl-smi">response</span>) {</td>
      </tr>
      <tr>
        <td id="L48" class="blob-num js-line-number" data-line-number="48"></td>
        <td id="LC48" class="blob-code blob-code-inner js-file-line">            errorHandler(response, done);</td>
      </tr>
      <tr>
        <td id="L49" class="blob-num js-line-number" data-line-number="49"></td>
        <td id="LC49" class="blob-code blob-code-inner js-file-line">        });</td>
      </tr>
      <tr>
        <td id="L50" class="blob-num js-line-number" data-line-number="50"></td>
        <td id="LC50" class="blob-code blob-code-inner js-file-line">    }</td>
      </tr>
      <tr>
        <td id="L51" class="blob-num js-line-number" data-line-number="51"></td>
        <td id="LC51" class="blob-code blob-code-inner js-file-line">    </td>
      </tr>
      <tr>
        <td id="L52" class="blob-num js-line-number" data-line-number="52"></td>
        <td id="LC52" class="blob-code blob-code-inner js-file-line">    </td>
      </tr>
      <tr>
        <td id="L53" class="blob-num js-line-number" data-line-number="53"></td>
        <td id="LC53" class="blob-code blob-code-inner js-file-line">    <span class="pl-c">// Init</span></td>
      </tr>
      <tr>
        <td id="L54" class="blob-num js-line-number" data-line-number="54"></td>
        <td id="LC54" class="blob-code blob-code-inner js-file-line">    </td>
      </tr>
      <tr>
        <td id="L55" class="blob-num js-line-number" data-line-number="55"></td>
        <td id="LC55" class="blob-code blob-code-inner js-file-line">
</td>
      </tr>
      <tr>
        <td id="L56" class="blob-num js-line-number" data-line-number="56"></td>
        <td id="LC56" class="blob-code blob-code-inner js-file-line">    beforeAll(<span class="pl-k">function</span>(<span class="pl-smi">done</span>) {</td>
      </tr>
      <tr>
        <td id="L57" class="blob-num js-line-number" data-line-number="57"></td>
        <td id="LC57" class="blob-code blob-code-inner js-file-line">        getDataFromApi(done, <span class="pl-s"><span class="pl-pds">&quot;</span>/getIdsByName?name=TestTemplate{0B6944E1-3CC5-45BA-AF78-728FFBE57358}<span class="pl-pds">&quot;</span></span>,</td>
      </tr>
      <tr>
        <td id="L58" class="blob-num js-line-number" data-line-number="58"></td>
        <td id="LC58" class="blob-code blob-code-inner js-file-line">            <span class="pl-k">function</span> () {</td>
      </tr>
      <tr>
        <td id="L59" class="blob-num js-line-number" data-line-number="59"></td>
        <td id="LC59" class="blob-code blob-code-inner js-file-line">                testProcessId1 <span class="pl-k">=</span> returnedData[<span class="pl-c1">0</span>];</td>
      </tr>
      <tr>
        <td id="L60" class="blob-num js-line-number" data-line-number="60"></td>
        <td id="LC60" class="blob-code blob-code-inner js-file-line">
</td>
      </tr>
      <tr>
        <td id="L61" class="blob-num js-line-number" data-line-number="61"></td>
        <td id="LC61" class="blob-code blob-code-inner js-file-line">                getDataFromApi(done, <span class="pl-s"><span class="pl-pds">&quot;</span>/getIdsByName?name=TestTemplate{77D78B4E-111F-4F62-8AC6-6B77459042CB}<span class="pl-pds">&quot;</span></span>,</td>
      </tr>
      <tr>
        <td id="L62" class="blob-num js-line-number" data-line-number="62"></td>
        <td id="LC62" class="blob-code blob-code-inner js-file-line">                <span class="pl-k">function</span> () {</td>
      </tr>
      <tr>
        <td id="L63" class="blob-num js-line-number" data-line-number="63"></td>
        <td id="LC63" class="blob-code blob-code-inner js-file-line">                    testProcessId2 <span class="pl-k">=</span> returnedData[<span class="pl-c1">0</span>];</td>
      </tr>
      <tr>
        <td id="L64" class="blob-num js-line-number" data-line-number="64"></td>
        <td id="LC64" class="blob-code blob-code-inner js-file-line">                    done();</td>
      </tr>
      <tr>
        <td id="L65" class="blob-num js-line-number" data-line-number="65"></td>
        <td id="LC65" class="blob-code blob-code-inner js-file-line">                });</td>
      </tr>
      <tr>
        <td id="L66" class="blob-num js-line-number" data-line-number="66"></td>
        <td id="LC66" class="blob-code blob-code-inner js-file-line">            });</td>
      </tr>
      <tr>
        <td id="L67" class="blob-num js-line-number" data-line-number="67"></td>
        <td id="LC67" class="blob-code blob-code-inner js-file-line">    });</td>
      </tr>
      <tr>
        <td id="L68" class="blob-num js-line-number" data-line-number="68"></td>
        <td id="LC68" class="blob-code blob-code-inner js-file-line">    </td>
      </tr>
      <tr>
        <td id="L69" class="blob-num js-line-number" data-line-number="69"></td>
        <td id="LC69" class="blob-code blob-code-inner js-file-line">    </td>
      </tr>
      <tr>
        <td id="L70" class="blob-num js-line-number" data-line-number="70"></td>
        <td id="LC70" class="blob-code blob-code-inner js-file-line">    <span class="pl-c">// Specs</span></td>
      </tr>
      <tr>
        <td id="L71" class="blob-num js-line-number" data-line-number="71"></td>
        <td id="LC71" class="blob-code blob-code-inner js-file-line">    </td>
      </tr>
      <tr>
        <td id="L72" class="blob-num js-line-number" data-line-number="72"></td>
        <td id="LC72" class="blob-code blob-code-inner js-file-line">
</td>
      </tr>
      <tr>
        <td id="L73" class="blob-num js-line-number" data-line-number="73"></td>
        <td id="LC73" class="blob-code blob-code-inner js-file-line">    it(<span class="pl-s"><span class="pl-pds">&quot;</span>Docusign plugin can execute action Monitor_DocuSign<span class="pl-pds">&quot;</span></span>, <span class="pl-k">function</span> (<span class="pl-smi">done</span>) {</td>
      </tr>
      <tr>
        <td id="L74" class="blob-num js-line-number" data-line-number="74"></td>
        <td id="LC74" class="blob-code blob-code-inner js-file-line">
</td>
      </tr>
      <tr>
        <td id="L75" class="blob-num js-line-number" data-line-number="75"></td>
        <td id="LC75" class="blob-code blob-code-inner js-file-line">        <span class="pl-k">var</span> actionDTO <span class="pl-k">=</span> {</td>
      </tr>
      <tr>
        <td id="L76" class="blob-num js-line-number" data-line-number="76"></td>
        <td id="LC76" class="blob-code blob-code-inner js-file-line">            activityTemplate<span class="pl-k">:</span> {</td>
      </tr>
      <tr>
        <td id="L77" class="blob-num js-line-number" data-line-number="77"></td>
        <td id="LC77" class="blob-code blob-code-inner js-file-line">                Name<span class="pl-k">:</span> <span class="pl-s"><span class="pl-pds">&quot;</span>Monitor_DocuSign<span class="pl-pds">&quot;</span></span>,</td>
      </tr>
      <tr>
        <td id="L78" class="blob-num js-line-number" data-line-number="78"></td>
        <td id="LC78" class="blob-code blob-code-inner js-file-line">                Version<span class="pl-k">:</span> <span class="pl-s"><span class="pl-pds">&quot;</span>1<span class="pl-pds">&quot;</span></span></td>
      </tr>
      <tr>
        <td id="L79" class="blob-num js-line-number" data-line-number="79"></td>
        <td id="LC79" class="blob-code blob-code-inner js-file-line">            },</td>
      </tr>
      <tr>
        <td id="L80" class="blob-num js-line-number" data-line-number="80"></td>
        <td id="LC80" class="blob-code blob-code-inner js-file-line">            ProcessId<span class="pl-k">:</span> testProcessId1</td>
      </tr>
      <tr>
        <td id="L81" class="blob-num js-line-number" data-line-number="81"></td>
        <td id="LC81" class="blob-code blob-code-inner js-file-line">        };</td>
      </tr>
      <tr>
        <td id="L82" class="blob-num js-line-number" data-line-number="82"></td>
        <td id="LC82" class="blob-code blob-code-inner js-file-line">
</td>
      </tr>
      <tr>
        <td id="L83" class="blob-num js-line-number" data-line-number="83"></td>
        <td id="LC83" class="blob-code blob-code-inner js-file-line">        executePluginAction(done, <span class="pl-s"><span class="pl-pds">&quot;</span>http://localhost:53234<span class="pl-pds">&quot;</span></span>, actionDTO, <span class="pl-k">function</span> () {</td>
      </tr>
      <tr>
        <td id="L84" class="blob-num js-line-number" data-line-number="84"></td>
        <td id="LC84" class="blob-code blob-code-inner js-file-line">
</td>
      </tr>
      <tr>
        <td id="L85" class="blob-num js-line-number" data-line-number="85"></td>
        <td id="LC85" class="blob-code blob-code-inner js-file-line">            <span class="pl-en">console</span>.<span class="pl-c1">log</span>(<span class="pl-c1">JSON</span>.stringify(returnedData));</td>
      </tr>
      <tr>
        <td id="L86" class="blob-num js-line-number" data-line-number="86"></td>
        <td id="LC86" class="blob-code blob-code-inner js-file-line">            <span class="pl-k">var</span> responsePattern <span class="pl-k">=</span> {</td>
      </tr>
      <tr>
        <td id="L87" class="blob-num js-line-number" data-line-number="87"></td>
        <td id="LC87" class="blob-code blob-code-inner js-file-line">                CrateStorage<span class="pl-k">:</span> {</td>
      </tr>
      <tr>
        <td id="L88" class="blob-num js-line-number" data-line-number="88"></td>
        <td id="LC88" class="blob-code blob-code-inner js-file-line">                    crates<span class="pl-k">:</span> [</td>
      </tr>
      <tr>
        <td id="L89" class="blob-num js-line-number" data-line-number="89"></td>
        <td id="LC89" class="blob-code blob-code-inner js-file-line">                        {</td>
      </tr>
      <tr>
        <td id="L90" class="blob-num js-line-number" data-line-number="90"></td>
        <td id="LC90" class="blob-code blob-code-inner js-file-line">                            Label<span class="pl-k">:</span> <span class="pl-s"><span class="pl-pds">&quot;</span>Standard Event Report<span class="pl-pds">&quot;</span></span>,</td>
      </tr>
      <tr>
        <td id="L91" class="blob-num js-line-number" data-line-number="91"></td>
        <td id="LC91" class="blob-code blob-code-inner js-file-line">                            Contents<span class="pl-k">:</span> {</td>
      </tr>
      <tr>
        <td id="L92" class="blob-num js-line-number" data-line-number="92"></td>
        <td id="LC92" class="blob-code blob-code-inner js-file-line">                                EventNames<span class="pl-k">:</span> <span class="pl-s"><span class="pl-pds">&quot;</span>DocuSign Envelope Sent<span class="pl-pds">&quot;</span></span>,</td>
      </tr>
      <tr>
        <td id="L93" class="blob-num js-line-number" data-line-number="93"></td>
        <td id="LC93" class="blob-code blob-code-inner js-file-line">                                EventPayload<span class="pl-k">:</span> [</td>
      </tr>
      <tr>
        <td id="L94" class="blob-num js-line-number" data-line-number="94"></td>
        <td id="LC94" class="blob-code blob-code-inner js-file-line">                                    {</td>
      </tr>
      <tr>
        <td id="L95" class="blob-num js-line-number" data-line-number="95"></td>
        <td id="LC95" class="blob-code blob-code-inner js-file-line">                                        Label<span class="pl-k">:</span> <span class="pl-s"><span class="pl-pds">&quot;</span>Payload Data<span class="pl-pds">&quot;</span></span>,</td>
      </tr>
      <tr>
        <td id="L96" class="blob-num js-line-number" data-line-number="96"></td>
        <td id="LC96" class="blob-code blob-code-inner js-file-line">                                        Contents<span class="pl-k">:</span> [</td>
      </tr>
      <tr>
        <td id="L97" class="blob-num js-line-number" data-line-number="97"></td>
        <td id="LC97" class="blob-code blob-code-inner js-file-line">                                            {</td>
      </tr>
      <tr>
        <td id="L98" class="blob-num js-line-number" data-line-number="98"></td>
        <td id="LC98" class="blob-code blob-code-inner js-file-line">                                                Key<span class="pl-k">:</span> <span class="pl-s"><span class="pl-pds">&quot;</span>EnvelopeId<span class="pl-pds">&quot;</span></span></td>
      </tr>
      <tr>
        <td id="L99" class="blob-num js-line-number" data-line-number="99"></td>
        <td id="LC99" class="blob-code blob-code-inner js-file-line">                                            }</td>
      </tr>
      <tr>
        <td id="L100" class="blob-num js-line-number" data-line-number="100"></td>
        <td id="LC100" class="blob-code blob-code-inner js-file-line">                                        ]</td>
      </tr>
      <tr>
        <td id="L101" class="blob-num js-line-number" data-line-number="101"></td>
        <td id="LC101" class="blob-code blob-code-inner js-file-line">                                    }</td>
      </tr>
      <tr>
        <td id="L102" class="blob-num js-line-number" data-line-number="102"></td>
        <td id="LC102" class="blob-code blob-code-inner js-file-line">                                ]</td>
      </tr>
      <tr>
        <td id="L103" class="blob-num js-line-number" data-line-number="103"></td>
        <td id="LC103" class="blob-code blob-code-inner js-file-line">                            }</td>
      </tr>
      <tr>
        <td id="L104" class="blob-num js-line-number" data-line-number="104"></td>
        <td id="LC104" class="blob-code blob-code-inner js-file-line">                        },</td>
      </tr>
      <tr>
        <td id="L105" class="blob-num js-line-number" data-line-number="105"></td>
        <td id="LC105" class="blob-code blob-code-inner js-file-line">                        {</td>
      </tr>
      <tr>
        <td id="L106" class="blob-num js-line-number" data-line-number="106"></td>
        <td id="LC106" class="blob-code blob-code-inner js-file-line">                            Label<span class="pl-k">:</span> <span class="pl-s"><span class="pl-pds">&quot;</span>DocuSign Envelope Payload Data<span class="pl-pds">&quot;</span></span>,</td>
      </tr>
      <tr>
        <td id="L107" class="blob-num js-line-number" data-line-number="107"></td>
        <td id="LC107" class="blob-code blob-code-inner js-file-line">                            Contents<span class="pl-k">:</span> [</td>
      </tr>
      <tr>
        <td id="L108" class="blob-num js-line-number" data-line-number="108"></td>
        <td id="LC108" class="blob-code blob-code-inner js-file-line">                                {</td>
      </tr>
      <tr>
        <td id="L109" class="blob-num js-line-number" data-line-number="109"></td>
        <td id="LC109" class="blob-code blob-code-inner js-file-line">                                    Key<span class="pl-k">:</span> <span class="pl-s"><span class="pl-pds">&quot;</span>EnvelopeId<span class="pl-pds">&quot;</span></span></td>
      </tr>
      <tr>
        <td id="L110" class="blob-num js-line-number" data-line-number="110"></td>
        <td id="LC110" class="blob-code blob-code-inner js-file-line">                                }</td>
      </tr>
      <tr>
        <td id="L111" class="blob-num js-line-number" data-line-number="111"></td>
        <td id="LC111" class="blob-code blob-code-inner js-file-line">                            ]</td>
      </tr>
      <tr>
        <td id="L112" class="blob-num js-line-number" data-line-number="112"></td>
        <td id="LC112" class="blob-code blob-code-inner js-file-line">                        }</td>
      </tr>
      <tr>
        <td id="L113" class="blob-num js-line-number" data-line-number="113"></td>
        <td id="LC113" class="blob-code blob-code-inner js-file-line">                    ]</td>
      </tr>
      <tr>
        <td id="L114" class="blob-num js-line-number" data-line-number="114"></td>
        <td id="LC114" class="blob-code blob-code-inner js-file-line">                }</td>
      </tr>
      <tr>
        <td id="L115" class="blob-num js-line-number" data-line-number="115"></td>
        <td id="LC115" class="blob-code blob-code-inner js-file-line">            }</td>
      </tr>
      <tr>
        <td id="L116" class="blob-num js-line-number" data-line-number="116"></td>
        <td id="LC116" class="blob-code blob-code-inner js-file-line">            </td>
      </tr>
      <tr>
        <td id="L117" class="blob-num js-line-number" data-line-number="117"></td>
        <td id="LC117" class="blob-code blob-code-inner js-file-line">            <span class="pl-k">var</span> result <span class="pl-k">=</span> matchObjectWithPattern(responsePattern, returnedData);</td>
      </tr>
      <tr>
        <td id="L118" class="blob-num js-line-number" data-line-number="118"></td>
        <td id="LC118" class="blob-code blob-code-inner js-file-line">
</td>
      </tr>
      <tr>
        <td id="L119" class="blob-num js-line-number" data-line-number="119"></td>
        <td id="LC119" class="blob-code blob-code-inner js-file-line">            <span class="pl-k">if</span> (result <span class="pl-k">!==</span> <span class="pl-c1">true</span>) {</td>
      </tr>
      <tr>
        <td id="L120" class="blob-num js-line-number" data-line-number="120"></td>
        <td id="LC120" class="blob-code blob-code-inner js-file-line">                <span class="pl-en">console</span>.<span class="pl-c1">log</span>(result);</td>
      </tr>
      <tr>
        <td id="L121" class="blob-num js-line-number" data-line-number="121"></td>
        <td id="LC121" class="blob-code blob-code-inner js-file-line">            }</td>
      </tr>
      <tr>
        <td id="L122" class="blob-num js-line-number" data-line-number="122"></td>
        <td id="LC122" class="blob-code blob-code-inner js-file-line">            </td>
      </tr>
      <tr>
        <td id="L123" class="blob-num js-line-number" data-line-number="123"></td>
        <td id="LC123" class="blob-code blob-code-inner js-file-line">            expect(result).toBe(<span class="pl-c1">true</span>);</td>
      </tr>
      <tr>
        <td id="L124" class="blob-num js-line-number" data-line-number="124"></td>
        <td id="LC124" class="blob-code blob-code-inner js-file-line">
</td>
      </tr>
      <tr>
        <td id="L125" class="blob-num js-line-number" data-line-number="125"></td>
        <td id="LC125" class="blob-code blob-code-inner js-file-line">            done();</td>
      </tr>
      <tr>
        <td id="L126" class="blob-num js-line-number" data-line-number="126"></td>
        <td id="LC126" class="blob-code blob-code-inner js-file-line">        });</td>
      </tr>
      <tr>
        <td id="L127" class="blob-num js-line-number" data-line-number="127"></td>
        <td id="LC127" class="blob-code blob-code-inner js-file-line">    });</td>
      </tr>
      <tr>
        <td id="L128" class="blob-num js-line-number" data-line-number="128"></td>
        <td id="LC128" class="blob-code blob-code-inner js-file-line">
</td>
      </tr>
      <tr>
        <td id="L129" class="blob-num js-line-number" data-line-number="129"></td>
        <td id="LC129" class="blob-code blob-code-inner js-file-line">    </td>
      </tr>
      <tr>
        <td id="L130" class="blob-num js-line-number" data-line-number="130"></td>
        <td id="LC130" class="blob-code blob-code-inner js-file-line">
</td>
      </tr>
      <tr>
        <td id="L131" class="blob-num js-line-number" data-line-number="131"></td>
        <td id="LC131" class="blob-code blob-code-inner js-file-line">    it(<span class="pl-s"><span class="pl-pds">&quot;</span>Docusign plugin can execute action Extract_From_DocuSign_Envelope<span class="pl-pds">&quot;</span></span>, <span class="pl-k">function</span> (<span class="pl-smi">done</span>) {</td>
      </tr>
      <tr>
        <td id="L132" class="blob-num js-line-number" data-line-number="132"></td>
        <td id="LC132" class="blob-code blob-code-inner js-file-line">
</td>
      </tr>
      <tr>
        <td id="L133" class="blob-num js-line-number" data-line-number="133"></td>
        <td id="LC133" class="blob-code blob-code-inner js-file-line">        <span class="pl-k">var</span> actionDTO <span class="pl-k">=</span> {</td>
      </tr>
      <tr>
        <td id="L134" class="blob-num js-line-number" data-line-number="134"></td>
        <td id="LC134" class="blob-code blob-code-inner js-file-line">            activityTemplate<span class="pl-k">:</span> {</td>
      </tr>
      <tr>
        <td id="L135" class="blob-num js-line-number" data-line-number="135"></td>
        <td id="LC135" class="blob-code blob-code-inner js-file-line">                Name<span class="pl-k">:</span> <span class="pl-s"><span class="pl-pds">&quot;</span>Extract_From_DocuSign_Envelope<span class="pl-pds">&quot;</span></span>,</td>
      </tr>
      <tr>
        <td id="L136" class="blob-num js-line-number" data-line-number="136"></td>
        <td id="LC136" class="blob-code blob-code-inner js-file-line">                Version<span class="pl-k">:</span> <span class="pl-s"><span class="pl-pds">&quot;</span>1<span class="pl-pds">&quot;</span></span>,</td>
      </tr>
      <tr>
        <td id="L137" class="blob-num js-line-number" data-line-number="137"></td>
        <td id="LC137" class="blob-code blob-code-inner js-file-line">            },</td>
      </tr>
      <tr>
        <td id="L138" class="blob-num js-line-number" data-line-number="138"></td>
        <td id="LC138" class="blob-code blob-code-inner js-file-line">            CrateStorage <span class="pl-k">:</span> {</td>
      </tr>
      <tr>
        <td id="L139" class="blob-num js-line-number" data-line-number="139"></td>
        <td id="LC139" class="blob-code blob-code-inner js-file-line">                    crates<span class="pl-k">:</span> [</td>
      </tr>
      <tr>
        <td id="L140" class="blob-num js-line-number" data-line-number="140"></td>
        <td id="LC140" class="blob-code blob-code-inner js-file-line">                        {</td>
      </tr>
      <tr>
        <td id="L141" class="blob-num js-line-number" data-line-number="141"></td>
        <td id="LC141" class="blob-code blob-code-inner js-file-line">                            Label<span class="pl-k">:</span> <span class="pl-s"><span class="pl-pds">&quot;</span>DocuSignTemplateUserDefinedFields<span class="pl-pds">&quot;</span></span>,</td>
      </tr>
      <tr>
        <td id="L142" class="blob-num js-line-number" data-line-number="142"></td>
        <td id="LC142" class="blob-code blob-code-inner js-file-line">                            ManifestType<span class="pl-k">:</span> <span class="pl-s"><span class="pl-pds">&quot;</span>Standard Design-Time Fields<span class="pl-pds">&quot;</span></span>,</td>
      </tr>
      <tr>
        <td id="L143" class="blob-num js-line-number" data-line-number="143"></td>
        <td id="LC143" class="blob-code blob-code-inner js-file-line">                            Contents<span class="pl-k">:</span> <span class="pl-c1">JSON</span>.stringify({</td>
      </tr>
      <tr>
        <td id="L144" class="blob-num js-line-number" data-line-number="144"></td>
        <td id="LC144" class="blob-code blob-code-inner js-file-line">                                Fields<span class="pl-k">:</span> [</td>
      </tr>
      <tr>
        <td id="L145" class="blob-num js-line-number" data-line-number="145"></td>
        <td id="LC145" class="blob-code blob-code-inner js-file-line">                                { Key<span class="pl-k">:</span> <span class="pl-s"><span class="pl-pds">&quot;</span>ExternalEventType<span class="pl-pds">&quot;</span></span> },</td>
      </tr>
      <tr>
        <td id="L146" class="blob-num js-line-number" data-line-number="146"></td>
        <td id="LC146" class="blob-code blob-code-inner js-file-line">                                { Key<span class="pl-k">:</span> <span class="pl-s"><span class="pl-pds">&quot;</span>RecipientId<span class="pl-pds">&quot;</span></span> }]</td>
      </tr>
      <tr>
        <td id="L147" class="blob-num js-line-number" data-line-number="147"></td>
        <td id="LC147" class="blob-code blob-code-inner js-file-line">                            })</td>
      </tr>
      <tr>
        <td id="L148" class="blob-num js-line-number" data-line-number="148"></td>
        <td id="LC148" class="blob-code blob-code-inner js-file-line">                        }</td>
      </tr>
      <tr>
        <td id="L149" class="blob-num js-line-number" data-line-number="149"></td>
        <td id="LC149" class="blob-code blob-code-inner js-file-line">                    ]</td>
      </tr>
      <tr>
        <td id="L150" class="blob-num js-line-number" data-line-number="150"></td>
        <td id="LC150" class="blob-code blob-code-inner js-file-line">                },</td>
      </tr>
      <tr>
        <td id="L151" class="blob-num js-line-number" data-line-number="151"></td>
        <td id="LC151" class="blob-code blob-code-inner js-file-line">            ProcessId<span class="pl-k">:</span> testProcessId2</td>
      </tr>
      <tr>
        <td id="L152" class="blob-num js-line-number" data-line-number="152"></td>
        <td id="LC152" class="blob-code blob-code-inner js-file-line">        };</td>
      </tr>
      <tr>
        <td id="L153" class="blob-num js-line-number" data-line-number="153"></td>
        <td id="LC153" class="blob-code blob-code-inner js-file-line">
</td>
      </tr>
      <tr>
        <td id="L154" class="blob-num js-line-number" data-line-number="154"></td>
        <td id="LC154" class="blob-code blob-code-inner js-file-line">        executePluginAction(done, <span class="pl-s"><span class="pl-pds">&quot;</span>http://localhost:53234<span class="pl-pds">&quot;</span></span>, actionDTO, <span class="pl-k">function</span> () {</td>
      </tr>
      <tr>
        <td id="L155" class="blob-num js-line-number" data-line-number="155"></td>
        <td id="LC155" class="blob-code blob-code-inner js-file-line">
</td>
      </tr>
      <tr>
        <td id="L156" class="blob-num js-line-number" data-line-number="156"></td>
        <td id="LC156" class="blob-code blob-code-inner js-file-line">            <span class="pl-en">console</span>.<span class="pl-c1">log</span>(<span class="pl-c1">JSON</span>.stringify(returnedData));</td>
      </tr>
      <tr>
        <td id="L157" class="blob-num js-line-number" data-line-number="157"></td>
        <td id="LC157" class="blob-code blob-code-inner js-file-line">            <span class="pl-k">var</span> responsePattern <span class="pl-k">=</span> {</td>
      </tr>
      <tr>
        <td id="L158" class="blob-num js-line-number" data-line-number="158"></td>
        <td id="LC158" class="blob-code blob-code-inner js-file-line">                CrateStorage<span class="pl-k">:</span> {</td>
      </tr>
      <tr>
        <td id="L159" class="blob-num js-line-number" data-line-number="159"></td>
        <td id="LC159" class="blob-code blob-code-inner js-file-line">                    crates<span class="pl-k">:</span> [</td>
      </tr>
      <tr>
        <td id="L160" class="blob-num js-line-number" data-line-number="160"></td>
        <td id="LC160" class="blob-code blob-code-inner js-file-line">                        {</td>
      </tr>
      <tr>
        <td id="L161" class="blob-num js-line-number" data-line-number="161"></td>
        <td id="LC161" class="blob-code blob-code-inner js-file-line">                            Label<span class="pl-k">:</span> <span class="pl-s"><span class="pl-pds">&quot;</span>Standard Event Report<span class="pl-pds">&quot;</span></span>,</td>
      </tr>
      <tr>
        <td id="L162" class="blob-num js-line-number" data-line-number="162"></td>
        <td id="LC162" class="blob-code blob-code-inner js-file-line">                            Contents<span class="pl-k">:</span> {</td>
      </tr>
      <tr>
        <td id="L163" class="blob-num js-line-number" data-line-number="163"></td>
        <td id="LC163" class="blob-code blob-code-inner js-file-line">                                EventNames<span class="pl-k">:</span> <span class="pl-s"><span class="pl-pds">&quot;</span>DocuSign Envelope Sent<span class="pl-pds">&quot;</span></span>,</td>
      </tr>
      <tr>
        <td id="L164" class="blob-num js-line-number" data-line-number="164"></td>
        <td id="LC164" class="blob-code blob-code-inner js-file-line">                                EventPayload<span class="pl-k">:</span> [</td>
      </tr>
      <tr>
        <td id="L165" class="blob-num js-line-number" data-line-number="165"></td>
        <td id="LC165" class="blob-code blob-code-inner js-file-line">                                    {</td>
      </tr>
      <tr>
        <td id="L166" class="blob-num js-line-number" data-line-number="166"></td>
        <td id="LC166" class="blob-code blob-code-inner js-file-line">                                        Label<span class="pl-k">:</span> <span class="pl-s"><span class="pl-pds">&quot;</span>Payload Data<span class="pl-pds">&quot;</span></span>,</td>
      </tr>
      <tr>
        <td id="L167" class="blob-num js-line-number" data-line-number="167"></td>
        <td id="LC167" class="blob-code blob-code-inner js-file-line">                                        Contents<span class="pl-k">:</span> [</td>
      </tr>
      <tr>
        <td id="L168" class="blob-num js-line-number" data-line-number="168"></td>
        <td id="LC168" class="blob-code blob-code-inner js-file-line">                                            {</td>
      </tr>
      <tr>
        <td id="L169" class="blob-num js-line-number" data-line-number="169"></td>
        <td id="LC169" class="blob-code blob-code-inner js-file-line">                                                Key<span class="pl-k">:</span> <span class="pl-s"><span class="pl-pds">&quot;</span>EnvelopeId<span class="pl-pds">&quot;</span></span></td>
      </tr>
      <tr>
        <td id="L170" class="blob-num js-line-number" data-line-number="170"></td>
        <td id="LC170" class="blob-code blob-code-inner js-file-line">                                            }</td>
      </tr>
      <tr>
        <td id="L171" class="blob-num js-line-number" data-line-number="171"></td>
        <td id="LC171" class="blob-code blob-code-inner js-file-line">                                        ]</td>
      </tr>
      <tr>
        <td id="L172" class="blob-num js-line-number" data-line-number="172"></td>
        <td id="LC172" class="blob-code blob-code-inner js-file-line">                                    }</td>
      </tr>
      <tr>
        <td id="L173" class="blob-num js-line-number" data-line-number="173"></td>
        <td id="LC173" class="blob-code blob-code-inner js-file-line">                                ]</td>
      </tr>
      <tr>
        <td id="L174" class="blob-num js-line-number" data-line-number="174"></td>
        <td id="LC174" class="blob-code blob-code-inner js-file-line">                            }</td>
      </tr>
      <tr>
        <td id="L175" class="blob-num js-line-number" data-line-number="175"></td>
        <td id="LC175" class="blob-code blob-code-inner js-file-line">                        },</td>
      </tr>
      <tr>
        <td id="L176" class="blob-num js-line-number" data-line-number="176"></td>
        <td id="LC176" class="blob-code blob-code-inner js-file-line">                        {</td>
      </tr>
      <tr>
        <td id="L177" class="blob-num js-line-number" data-line-number="177"></td>
        <td id="LC177" class="blob-code blob-code-inner js-file-line">                            Label<span class="pl-k">:</span> <span class="pl-s"><span class="pl-pds">&quot;</span>DocuSign Envelope Payload Data<span class="pl-pds">&quot;</span></span>,</td>
      </tr>
      <tr>
        <td id="L178" class="blob-num js-line-number" data-line-number="178"></td>
        <td id="LC178" class="blob-code blob-code-inner js-file-line">                            Contents<span class="pl-k">:</span> [</td>
      </tr>
      <tr>
        <td id="L179" class="blob-num js-line-number" data-line-number="179"></td>
        <td id="LC179" class="blob-code blob-code-inner js-file-line">                                {</td>
      </tr>
      <tr>
        <td id="L180" class="blob-num js-line-number" data-line-number="180"></td>
        <td id="LC180" class="blob-code blob-code-inner js-file-line">                                    Key<span class="pl-k">:</span> <span class="pl-s"><span class="pl-pds">&quot;</span>EnvelopeId<span class="pl-pds">&quot;</span></span></td>
      </tr>
      <tr>
        <td id="L181" class="blob-num js-line-number" data-line-number="181"></td>
        <td id="LC181" class="blob-code blob-code-inner js-file-line">                                }</td>
      </tr>
      <tr>
        <td id="L182" class="blob-num js-line-number" data-line-number="182"></td>
        <td id="LC182" class="blob-code blob-code-inner js-file-line">                            ]</td>
      </tr>
      <tr>
        <td id="L183" class="blob-num js-line-number" data-line-number="183"></td>
        <td id="LC183" class="blob-code blob-code-inner js-file-line">                        },</td>
      </tr>
      <tr>
        <td id="L184" class="blob-num js-line-number" data-line-number="184"></td>
        <td id="LC184" class="blob-code blob-code-inner js-file-line">                        {</td>
      </tr>
      <tr>
        <td id="L185" class="blob-num js-line-number" data-line-number="185"></td>
        <td id="LC185" class="blob-code blob-code-inner js-file-line">                            Label<span class="pl-k">:</span> <span class="pl-s"><span class="pl-pds">&quot;</span>DocuSign Envelope Data<span class="pl-pds">&quot;</span></span>,</td>
      </tr>
      <tr>
        <td id="L186" class="blob-num js-line-number" data-line-number="186"></td>
        <td id="LC186" class="blob-code blob-code-inner js-file-line">                            ManifestType<span class="pl-k">:</span> <span class="pl-s"><span class="pl-pds">&quot;</span>Standard Payload Data<span class="pl-pds">&quot;</span></span>,</td>
      </tr>
      <tr>
        <td id="L187" class="blob-num js-line-number" data-line-number="187"></td>
        <td id="LC187" class="blob-code blob-code-inner js-file-line">                            ManifestId<span class="pl-k">:</span><span class="pl-c1">5</span>,</td>
      </tr>
      <tr>
        <td id="L188" class="blob-num js-line-number" data-line-number="188"></td>
        <td id="LC188" class="blob-code blob-code-inner js-file-line">                        }</td>
      </tr>
      <tr>
        <td id="L189" class="blob-num js-line-number" data-line-number="189"></td>
        <td id="LC189" class="blob-code blob-code-inner js-file-line">                    ]</td>
      </tr>
      <tr>
        <td id="L190" class="blob-num js-line-number" data-line-number="190"></td>
        <td id="LC190" class="blob-code blob-code-inner js-file-line">                }</td>
      </tr>
      <tr>
        <td id="L191" class="blob-num js-line-number" data-line-number="191"></td>
        <td id="LC191" class="blob-code blob-code-inner js-file-line">            }</td>
      </tr>
      <tr>
        <td id="L192" class="blob-num js-line-number" data-line-number="192"></td>
        <td id="LC192" class="blob-code blob-code-inner js-file-line">
</td>
      </tr>
      <tr>
        <td id="L193" class="blob-num js-line-number" data-line-number="193"></td>
        <td id="LC193" class="blob-code blob-code-inner js-file-line">            <span class="pl-k">var</span> result <span class="pl-k">=</span> matchObjectWithPattern(responsePattern, returnedData);</td>
      </tr>
      <tr>
        <td id="L194" class="blob-num js-line-number" data-line-number="194"></td>
        <td id="LC194" class="blob-code blob-code-inner js-file-line">
</td>
      </tr>
      <tr>
        <td id="L195" class="blob-num js-line-number" data-line-number="195"></td>
        <td id="LC195" class="blob-code blob-code-inner js-file-line">            <span class="pl-k">if</span> (result <span class="pl-k">!==</span> <span class="pl-c1">true</span>) {</td>
      </tr>
      <tr>
        <td id="L196" class="blob-num js-line-number" data-line-number="196"></td>
        <td id="LC196" class="blob-code blob-code-inner js-file-line">                <span class="pl-en">console</span>.<span class="pl-c1">log</span>(result);</td>
      </tr>
      <tr>
        <td id="L197" class="blob-num js-line-number" data-line-number="197"></td>
        <td id="LC197" class="blob-code blob-code-inner js-file-line">            }</td>
      </tr>
      <tr>
        <td id="L198" class="blob-num js-line-number" data-line-number="198"></td>
        <td id="LC198" class="blob-code blob-code-inner js-file-line">
</td>
      </tr>
      <tr>
        <td id="L199" class="blob-num js-line-number" data-line-number="199"></td>
        <td id="LC199" class="blob-code blob-code-inner js-file-line">            expect(result).toBe(<span class="pl-c1">true</span>);</td>
      </tr>
      <tr>
        <td id="L200" class="blob-num js-line-number" data-line-number="200"></td>
        <td id="LC200" class="blob-code blob-code-inner js-file-line">
</td>
      </tr>
      <tr>
        <td id="L201" class="blob-num js-line-number" data-line-number="201"></td>
        <td id="LC201" class="blob-code blob-code-inner js-file-line">            done();</td>
      </tr>
      <tr>
        <td id="L202" class="blob-num js-line-number" data-line-number="202"></td>
        <td id="LC202" class="blob-code blob-code-inner js-file-line">        });</td>
      </tr>
      <tr>
        <td id="L203" class="blob-num js-line-number" data-line-number="203"></td>
        <td id="LC203" class="blob-code blob-code-inner js-file-line">    });</td>
      </tr>
      <tr>
        <td id="L204" class="blob-num js-line-number" data-line-number="204"></td>
        <td id="LC204" class="blob-code blob-code-inner js-file-line">
</td>
      </tr>
      <tr>
        <td id="L205" class="blob-num js-line-number" data-line-number="205"></td>
        <td id="LC205" class="blob-code blob-code-inner js-file-line">    </td>
      </tr>
      <tr>
        <td id="L206" class="blob-num js-line-number" data-line-number="206"></td>
        <td id="LC206" class="blob-code blob-code-inner js-file-line">});</td>
      </tr>
      <tr>
        <td id="L207" class="blob-num js-line-number" data-line-number="207"></td>
        <td id="LC207" class="blob-code blob-code-inner js-file-line">
</td>
      </tr>
      <tr>
        <td id="L208" class="blob-num js-line-number" data-line-number="208"></td>
        <td id="LC208" class="blob-code blob-code-inner js-file-line">
</td>
      </tr>
      <tr>
        <td id="L209" class="blob-num js-line-number" data-line-number="209"></td>
        <td id="LC209" class="blob-code blob-code-inner js-file-line">
</td>
      </tr>
      <tr>
        <td id="L210" class="blob-num js-line-number" data-line-number="210"></td>
        <td id="LC210" class="blob-code blob-code-inner js-file-line">
</td>
      </tr>
      <tr>
        <td id="L211" class="blob-num js-line-number" data-line-number="211"></td>
        <td id="LC211" class="blob-code blob-code-inner js-file-line">
</td>
      </tr>
</table>
            
  </div>

</div>
            
<a href="#jump-to-line" rel="facebox[.linejump]" data-hotkey="l" style="display:none">Jump to Line</a>
<div id="jump-to-line" style="display:none">
  <!-- </textarea> --><!-- '"` --><form accept-charset="UTF-8" action="" class="js-jump-to-line-form" method="get"><div style="margin:0;padding:0;display:inline"><input name="utf8" type="hidden" value="&#x2713;" /></div>
    <input class="linejump-input js-jump-to-line-field" type="text" placeholder="Jump to line&hellip;" aria-label="Jump to line" autofocus>
    <button type="submit" class="btn">Go</button>
</form></div>

        </div>
      </div>
      <div class="modal-backdrop"></div>
    </div>
  </div>

    
    </div>

      <div class="container">
  <div class="site-footer" role="contentinfo">
    <ul class="site-footer-links right">
        <li><a href="https://status.github.com/" data-ga-click="Footer, go to status, text:status">Status</a></li>
      <li><a href="https://developer.github.com" data-ga-click="Footer, go to api, text:api">API</a></li>
      <li><a href="https://training.github.com" data-ga-click="Footer, go to training, text:training">Training</a></li>
      <li><a href="https://shop.github.com" data-ga-click="Footer, go to shop, text:shop">Shop</a></li>
        <li><a href="https://github.com/blog" data-ga-click="Footer, go to blog, text:blog">Blog</a></li>
        <li><a href="https://github.com/about" data-ga-click="Footer, go to about, text:about">About</a></li>
        <li><a href="https://github.com/pricing" data-ga-click="Footer, go to pricing, text:pricing">Pricing</a></li>

    </ul>

    <a href="https://github.com" aria-label="Homepage">
      <span class="mega-octicon octicon-mark-github" title="GitHub"></span>
</a>
    <ul class="site-footer-links">
      <li>&copy; 2015 <span title="0.06956s from github-fe128-cp1-prd.iad.github.net">GitHub</span>, Inc.</li>
        <li><a href="https://github.com/site/terms" data-ga-click="Footer, go to terms, text:terms">Terms</a></li>
        <li><a href="https://github.com/site/privacy" data-ga-click="Footer, go to privacy, text:privacy">Privacy</a></li>
        <li><a href="https://github.com/security" data-ga-click="Footer, go to security, text:security">Security</a></li>
        <li><a href="https://github.com/contact" data-ga-click="Footer, go to contact, text:contact">Contact</a></li>
        <li><a href="https://help.github.com" data-ga-click="Footer, go to help, text:help">Help</a></li>
    </ul>
  </div>
</div>






    
    <div id="ajax-error-message" class="flash flash-error">
      <span class="octicon octicon-alert"></span>
      <button type="button" class="flash-close js-flash-close js-ajax-error-dismiss" aria-label="Dismiss error">
        <span class="octicon octicon-x"></span>
      </button>
      Something went wrong with that request. Please try again.
    </div>


      <script crossorigin="anonymous" integrity="sha256-CA8cFVoo9aQxXUpoYq6vt+J7ygp022966eAEjjITadE=" src="https://assets-cdn.github.com/assets/frameworks-080f1c155a28f5a4315d4a6862aeafb7e27bca0a74db6f7ae9e0048e321369d1.js"></script>
      <script async="async" crossorigin="anonymous" integrity="sha256-rAM/AGK9P0fEaNPag51hXf+Rz88OQW6goAmfvNRIOro=" src="https://assets-cdn.github.com/assets/github-ac033f0062bd3f47c468d3da839d615dff91cfcf0e416ea0a0099fbcd4483aba.js"></script>


    <div class="js-stale-session-flash stale-session-flash flash flash-warn flash-banner hidden">
      <span class="octicon octicon-alert"></span>
      <span class="signed-in-tab-flash">You signed in with another tab or window. <a href="">Reload</a> to refresh your session.</span>
      <span class="signed-out-tab-flash">You signed out in another tab or window. <a href="">Reload</a> to refresh your session.</span>
    </div>
  </body>
</html>

