/*
 * etaapp.menu.js
 * Menu Feature Module for Etaapp
 */

 /*global $, etaapp */

 etaapp.menu = (function () {

 //-------------- BEGIN MODULE SCOPE VARIABLES ----------------
 var
   configMap = {
      main_html : String()
        + '<div id="etaapp-menu-top" class="" >'
          + '<div id="etaapp-menu-report-div" class="">'
            + '<div id="etaapp-menu-report"><a href="#">Reports</a></div>'
          + '</div>'
          + '<div id="etaapp-menu-malware-div" class="">'
            + '<div id="etaapp-menu-malware"><a href="#">Malware</a></div>'
          + '</div>'
        + '</div>'
        + '<div id="menu1">'
          + '<div class="menubg-trends"><a href="#"><div class="menu-trends" >Browsing Trends</div></a></div>'
          + '<div class="menubg-run"><a href="#"><div class="menu-run">Download Trends</div></a></div>'
        + '</div>'
        + '<div id="menu2">'
          + '<div class="menubg-family"><a href="#"><div class="menu-family">Malware Families</div></a></div>'   
          + '<div class="menubg-behavior"><a href="#"><div class="menu-behavior">Malware Behaviors</div></a></div>'   
          + '<div class="menubg-search"><a href="#"><div class="menu-search">Behavior Search</div></a></div>'   
        + '</div>' ,
        settable_map : {}
   },
   stateMap = { $container   : null },
   jqueryMap = {},  

   setJqueryMap, configModule, initModule, setTopMenuHandler, 
   setDefaultMenu ;
  //-------------- END MODULE SCOPE VARIBALES -------------------

  //-------------- BEGIN UTILITY METHODS ------------------------

  //-------------- END UTILITY METHODS ---------------------------

  //------------------- BEGIN DOM METHODS -------------------------
  // Begin DOM method / setJqueryMap
  setJqueryMap = function () {
    var $container = stateMap.$container;
    jqueryMap = { $container  : $container, 
                  $maincontent :  $('#etaapp').find('.etaapp-shell-main-content')
    };        
  };
  // End DOM method /setJqueryMap/
  //-------------- END DOM METHODS -------------------------------- 


  //------------------- BEGIN EVENT HANDLERS -----------------------

   setTopMenuHandler = function () {
      $("#etaapp-menu-report-div").click(function() {
         $("#etaapp-menu-report-div").addClass("etaapp-menu-reportactive");
         $("#etaapp-menu-malware-div").removeClass("etaapp-menu-malwareactive");
         $("#menu1").css("display", "block");
         $("#menu2").css("display", "none");
         $("div.menubg-trends").css("background", "url(images/menu-active-bg.jpg) repeat-x");
         $("div.menu-trends").css("background", "url(images/arrow-down.png) no-repeat right");
         $("div.menubg-run").css("background", "url(images/menu-bg.jpg) repeat-x");
         $("div.menu-run").css("background", "url(images/arrow-left.png) no-repeat right");
         $('div.etaapp-shell-main-content').html('');
         etaapp.trends.configModule ( {} );
         etaapp.trends.initModule( jqueryMap.$maincontent );

      });

      $("#etaapp-menu-malware-div").click(function() {
         $("#etaapp-menu-malware-div").addClass("etaapp-menu-malwareactive");
         $("#etaapp-menu-report-div").removeClass("etaapp-menu-reportactive");
         $("#menu1").css("display", "none");
         $("#menu2").css("display", "block");
         $("div.menubg-family").css("background", "url(images/menu-active-bg.jpg) repeat-x");
         $("div.menu-family").css("background", "url(images/arrow-down.png) no-repeat right");
         $("div.menubg-behavior").css("background", "url(images/menu-bg.jpg) repeat-x");
         $("div.menu-behavior").css("background", "url(images/arrow-left.png) no-repeat right");
         $("div.menubg-search").css("background", "url(images/menu-bg.jpg) repeat-x");
         $("div.menu-search").css("background", "url(images/arrow-left.png) no-repeat right");
         $('div.etaapp-shell-main-content').html('');

         etaapp.model.invokeMenuFamily(jqueryMap.$maincontent);

      });
      $("div.menubg-trends").click(function() {
         $("#etaapp-menu-report-div").addClass("etaapp-menu-reportactive");
         $("#etaapp-menu-malware-div").removeClass("etaapp-menu-malwareactive");
         $("div.menubg-trends").css("background", "url(images/menu-active-bg.jpg) repeat-x");
         $("div.menu-trends").css("background", "url(images/arrow-down.png) no-repeat right");
         $("div.menubg-run").css("background", "url(images/menu-bg.jpg) repeat-x");
         $("div.menu-run").css("background", "url(images/arrow-left.png) no-repeat right");
         $('div.etaapp-shell-main-content').html('');
         etaapp.trends.configModule ( {} );
         etaapp.trends.initModule( jqueryMap.$maincontent );


      });

      $("div.menubg-run").click(function() {
         $("#etaapp-menu-report-div").addClass("etaapp-menu-reportactive");
         $("#etaapp-menu-malware-div").removeClass("etaapp-menu-malwareactive");
         $("div.menubg-run").css("background", "url(images/menu-active-bg.jpg) repeat-x");
         $("div.menu-run").css("background", "url(images/arrow-down.png) no-repeat right");
         $("div.menubg-trends").css("background", "url(images/menu-bg.jpg) repeat-x");
         $("div.menu-trends").css("background", "url(images/arrow-left.png) no-repeat right");
         $('div.etaapp-shell-main-content').html('');
         etaapp.run.configModule ( {} );
         etaapp.run.initModule( jqueryMap.$maincontent );


      });

      $("div.menubg-family").click(function() {
         $("#etaapp-menu-report-div").removeClass("etaapp-menu-reportactive");
         $("#etaapp-menu-malware-div").addClass("etaapp-menu-malwareactive");
         $("div.menubg-family").css("background", "url(images/menu-active-bg.jpg) repeat-x");
         $("div.menu-family").css("background", "url(images/arrow-down.png) no-repeat right");
         $("div.menubg-behavior").css("background", "url(images/menu-bg.jpg) repeat-x");
         $("div.menu-behavior").css("background", "url(images/arrow-left.png) no-repeat right");
         $("div.menubg-search").css("background", "url(images/menu-bg.jpg) repeat-x");
         $("div.menu-search").css("background", "url(images/arrow-left.png) no-repeat right");

         $('div.etaapp-shell-main-content').html('');
         etaapp.model.invokeMenuFamily(jqueryMap.$maincontent);
      });

      $("div.menubg-behavior").click(function() {
         $("#etaapp-menu-report-div").removeClass("etaapp-menu-reportactive");
         $("#etaapp-menu-malware-div").addClass("etaapp-menu-malwareactive");
         $("div.menubg-behavior").css("background", "url(images/menu-active-bg.jpg) repeat-x");
         $("div.menu-behavior").css("background", "url(images/arrow-down.png) no-repeat right");
         $("div.menubg-family").css("background", "url(images/menu-bg.jpg) repeat-x");
         $("div.menu-family").css("background", "url(images/arrow-left.png) no-repeat right");
         $("div.menubg-search").css("background", "url(images/menu-bg.jpg) repeat-x");
         $("div.menu-search").css("background", "url(images/arrow-left.png) no-repeat right");
         $('div.etaapp-shell-main-content').html('');
         etaapp.behavior.configModule ( {} );
         etaapp.behavior.initModule( jqueryMap.$maincontent );

 
      });

      $("div.menubg-search").click(function() {
         $("#etaapp-menu-report-div").removeClass("etaapp-menu-reportactive");
         $("#etaapp-menu-malware-div").addClass("etaapp-menu-malwareactive");
         $("div.menubg-search").css("background", "url(images/menu-active-bg.jpg) repeat-x");
         $("div.menu-search").css("background", "url(images/arrow-down.png) no-repeat right");
         $("div.menubg-family").css("background", "url(images/menu-bg.jpg) repeat-x");
         $("div.menu-family").css("background", "url(images/arrow-left.png) no-repeat right");
         $("div.menubg-behavior").css("background", "url(images/menu-bg.jpg) repeat-x");
         $("div.menu-behavior").css("background", "url(images/arrow-left.png) no-repeat right");
         $('div.etaapp-shell-main-content').html('');
         etaapp.search.configModule ( {} );
         etaapp.search.initModule( jqueryMap.$maincontent );


      });

   };

   setDefaultMenu = function () {
     $("#etaapp-menu-report-div").removeClass("etaapp-menu-reportactive");
     $("#etaapp-menu-malware-div").addClass("etaapp-menu-malwareactive");
     $("#menu1").css("display", "none");
     $("#menu2").css("display", "block");

     $("div.menubg-family").css("background", "url(images/menu-active-bg.jpg) repeat-x");
     $("div.menu-family").css("background", "url(images/arrow-down.png) no-repeat right");
     $("div.menubg-behavior").css("background", "url(images/menu-bg.jpg) repeat-x");
     $("div.menu-behavior").css("background", "url(images/arrow-left.png) no-repeat right");
     $("div.menubg-search").css("background", "url(images/menu-bg.jpg) repeat-x");
     $("div.menu-search").css("background", "url(images/arrow-left.png) no-repeat right");
     $('div.etaapp-shell-main-content').html('');

   }; 


  //------------------- END EVENT HANDLERS -------------------------

  //------------------- BEGIN PUBLIC METHODS -----------------------
  configModule = function ( input_map ) {
    etaapp.util.setConfigMap ({
      input_map     : input_map,
      settable_map  : configMap.settable_map,
      config_map    : configMap
    });
    return true;
  };

  initModule = function ( $container ) {
    $container.html(configMap.main_html );
    stateMap.$container = $container;
    setJqueryMap();
    setTopMenuHandler();
    setDefaultMenu();
    return true;
  };

  return {
    configModule    : configModule,
    initModule      : initModule
  };

  }()); 


