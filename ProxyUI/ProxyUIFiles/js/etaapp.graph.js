/*
 * etaapp.graph.js
 * Malware Graph display screen
 */

 /*global $, etaapp */

 etaapp.graph = (function () {

 //---------------- BEGIN MODULE SCOPE VARIABLES --------------
 var
   configMap = {
     main_html : String()
       + '<div>'
       + 'Hi There'
       + '</div>' ,
       settable_map : {}
   },
   stateMap = { $container  : null },
   jqueryMap = {}, 

   setJqueryMap, configModule, initModule;

 //---------------- END MODULE SCOPE VARIABLES  ----------------

 //---------------- BEGIN UTILITY METHODS ---------------------- 

 //---------------- END UTILITY METHODS  -----------------------

 //---------------- BEGIN DOM METHODS  --------------------------
 setJqueryMap = function () {
   var $container = stateMap.$container;
   jqueryMap = { $container  : $container,
                 $modalcontent : $('#etaapp').find('.etaapp-shell-main-modal')
   };
 }
 // End DOM method /setJqueryMap/
 //---------------- END DOM METHODS  -----------------------------

 //----------------- BEGIN EVENT HANDLERS ------------------------ 

 //---------------- END EVENT HANDLERS ---------------------------

 //------------------ BEGIN PUBLIC METHODS -----------------------
 configModule = function (input_map) {
   etaapp.util.setConfigMap ({
     input_map     : input_map,
     settable_map  : configMap.settable_map,
     config_map    : configMap
   });
   return true;
 }; 

 initModule = function () {
   setJqueryMap();
   $localcontainer = jqueryMap.$modalcontent;
   $localcontainer.html(configMap.main_html);
   $(".etaapp-shell-main-modal").css("z-index", "-11");
   $(".etaapp-shell-main-modal").css("display", "block");


   return true;
 };

 return {
   configModule    : configModule,
   initModule      : initModule
 };

 } ());
 
