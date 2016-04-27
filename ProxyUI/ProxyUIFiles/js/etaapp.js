/*
 * etaapp.js
 * Root namespace module
 */

  /* global $, etaapp */
  var etaapp = (function () {
    var initModule = function ( $container ) {
      etaapp.shell.initModule( $container ); 
    };

    return { initModule: initModule };
  }());
