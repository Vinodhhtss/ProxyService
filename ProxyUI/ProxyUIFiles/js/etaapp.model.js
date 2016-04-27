/*
 * etaapp.model.js
 * Model module
 */


 /*global $, spa */

  etaapp.model = (function () { 

  //--------------- BEGIN MODULE SCOPE VARIABLES -------------- 
  var
    configMap = { },
    stateMap  = { },
    jqueryMap = { }, 

    initModule, registerFamily, invokeFamily; 

  //--------------- END MODULE SCOPE VARIABLES ----------------- 

  //---------------- BEGIN UTILITY METHODS ----------------------


  //---------------- END UTILITY METHODS ------------------------

  //---------------- BEGIN DOM METHODS --------------------------

  //---------------- END DOM METHODS ----------------------------

  //---------------- BEGIN EVENT HANDLERS ----------------------- 

  registerFamily = function () {

  }; 

  //---------------- END EVENT HANDLERS -------------------------

  invokeMenuFamily = function ($container) {
    etaapp.family.configModule ({});
    etaapp.family.initModule($container);
  }; 


  initModule = function ($container) { 

    return true;
  };

  return {
    initModule         : initModule,
    invokeMenuFamily   : invokeMenuFamily
  }; 

  }());
