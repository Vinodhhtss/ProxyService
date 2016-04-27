/*
 * etaapp.family.js
 * Malware Family display screen
 */

 /*global $, etaapp */

 etaapp.family = (function () {

 //---------------- BEGIN MODULE SCOPE VARIABLES --------------
 var
   configMap = {
     main_html : String()
       + '<div id="etaapp-family-top" class="">'
       + 'Happy New Year'
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
   jqueryMap = { $container  : $container };
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

 initModule = function ( $container ) {
   $container.html(configMap.main_html);
   setJqueryMap();
   return true;
 };

 return {
   configModule    : configModule,
   initModule      : initModule
 };

 } ());
 
