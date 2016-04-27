/*
 * etaapp.shell.js
 * Shell module for Etaapp
 */

 /*global $, etaapp */
 
 etaapp.shell = (function () {
   // --------------- BEGIN MODULE SCOPE VARIABLES ---------------
   var
     configMap = {
       main_html : String()
         + '<div class="etaapp-shell-head">'
           + '<div class="etaapp-shell-head-logo"><img src="images/logo.png" /></div>'
           + '<h1 class="etaapp-shell-head-title">ETA Test Portal</h1>'
           + '<div class="etaapp-shell-head-acct"></div>'
           + '<div class="etaapp-shell-head-search"></div>'
         + '</div>' 
         + '<div class="etaapp-shell-head-border"></div>'
         + '<div class="etaapp-shell-main">'
           + '<div class="etaapp-menu-left-others">'
             + '<div class="etaapp-shell-main-nav"></div>'
           + '</div>'
           + '<div class="etaapp-shell-main-content"></div>'
         + '</div>'
         + '<div class="etaapp-shell-foot">' 
      //      + '<div class="etaapp-shell-foot-border"></div>'

            + '<div class="etaapp-shell-foot-text"><b>Copyright @ 2013 FireEye, All rights reserved.</b></div>'
         + '</div>'
         + '<div class="etaapp-shell-main-modal"></div>'
       },
       stateMap = { $container  : null },
       jqueryMap = {},

       setJqueryMap, initModule;

    // ------------- END MODULE SCOPE VARIABLES ------------------

    // ---------------- BEGIN UTILITY METHODS ---------------------
    // ---------------- END UTILITY METHODS -----------------------

    // ---------------- BEGIN DOM METHODS -------------------------
    // Begin DOM method /setJqueryMap /
    setJqueryMap = function () {
      var $container = stateMap.$container;
      jqueryMap = { 
           $container : $container,
           $menu : $container.find('.etaapp-shell-main-nav'),
           $family : $container.find('.etaapp-shell-main-content')
      };
    };
    //----------------- END DOM METHODS ----------------------------

    //----------------- BEGIN PUBLIC METHODS ----------------------
    // Begin public method / initModule/
    initModule = function ( $container ) {
      stateMap.$container = $container;
      $container.html( configMap.main_html );
      setJqueryMap();

      etaapp.menu.configModule ( {} );
      etaapp.menu.initModule( jqueryMap.$menu );

      etaapp.family.configModule ( {} );
      etaapp.family.initModule( jqueryMap.$family );
    };
    // End PUBLIC method /initModule/

    return { initModule : initModule };
    // ---------------- END PUBLIC METHODS ------------------------
  }());


