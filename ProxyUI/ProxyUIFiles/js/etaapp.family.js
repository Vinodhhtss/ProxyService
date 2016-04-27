/*
 * etaapp.family.js
 * Malware Family display screen
 */

 /*global $, etaapp */

 etaapp.family = (function () {

 //---------------- BEGIN MODULE SCOPE VARIABLES --------------
 var
   configMap = {
     main_html_1 : String(),
     main_html_2 : String()  + '<div id="etaapp-content-right-others" class="">'
         + '<div id="etaapp-content-titlebar" class="">'
           + '<div id="etaapp-content-titlebar-text">Malware Families</div>'
         + '</div>' 
         + '<div id="etaapp-scrollbar1" class="">'
           + '<div class="scrollbar">'
             + '<div class="track">'
               + '<div class="thumb">'
                 + '<div class="end"></div>' 
               + '</div>'
             + '</div>'
           + '</div>'
           + '<div class="viewport">'
             + '<div class="overview">'
               + '<ul id="etaapp-family-thumbs" >', 

     main_html_3 : String()
                 + '<li class="clearfix">'
                   + '<div class="etaapp-family-wrapper" id="1">'
                     + '<div id="etaapp-family-strip">'
                       + '<div id="etaapp-family-blocktype">'
                         + '<h4>Family Name</h4>'
                         + '<h3>Sality</h3>'
                         + '<h4>Sample Count</h4>'
                         + '<h3>8</h3>'
                       + '</div>'
                     + '</div>' 
                   + '</div>'
                   + '<div class="meta">'
                     + '<div style="height:40px"></div><div></div>'
                     + '<div>'
                     + '<h4>Menu</h4>'
                     + '</div>'
                     + '<div>'
                     + '<span><a href="#222">Samples</a></span>'
                     + '</div>'
                     + '<div>'
                     + '<span><a href="#223">Behaviors</a></span>'
                     + '</div>'
                   + '</div>'
                 + '</li>',

      main_html_4 :
               + '</ul>'
             + '</div>'
           + '</div>'
         + '</div>'
       + '</div>' ,
       settable_map : {}
   },
   stateMap = { $container  : null },
   jqueryMap = {}, 

   setJqueryMap, configModule, initModule, setReadyHandler, formHtml, updatescrollbar;

 //---------------- END MODULE SCOPE VARIABLES  ----------------

 //---------------- BEGIN UTILITY METHODS ---------------------- 

 //---------------- END UTILITY METHODS  -----------------------

 //---------------- BEGIN DOM METHODS  --------------------------

 formHtml = function ($container) {
   var local_html = String();
   var family_map;

   family_map  = etaapp.family_model.get_id_map(); 

   alert(family_map[1].id);       


   local_html += String(configMap.main_html_2);
   for (i=1;family_map[i] != null && family_map[i].id > 0; i++)
   {

     local_html +=  '<li class="clearfix"> <div class="etaapp-family-wrapper" id="' + String(family_map[i].id) + '" >';
     local_html +=  '<div id="etaapp-family-strip">'
                       + '<div id="etaapp-family-blocktype">'
                         + '<h4>Family Name</h4>'
                         + '<h3>' + family_map[i].name + '</h3>'
                         + '<h4>Sample Count</h4>'
                         + '<h3>' + String(family_map[i].samples) + '</h3>'
                       + '</div>'
                     + '</div>'
                   + '</div>'
                   + '<div class="meta">'
                     + '<div style="height:40px"></div><div></div>'
                     + '<div>'
                     + '<h4>Menu</h4>'
                     + '</div>'
                     + '<div>'
                     + '<span><a href="#222">Samples</a></span>'
                     + '</div>'
                     + '<div>'
                     + '<span><a href="#223">Behaviors</a></span>'
                     + '</div>'
                     + '<div>'
                     + '<span><a href="javascript:etaapp.graph.initModule();">IOC Graph</a></span>'
                     + '</div>'
                   + '</div>'
                 + '</li>';

   }

   local_html += String(configMap.main_html_4);


   $container.html(local_html);
 };
 
 setJqueryMap = function () {
   var $container = stateMap.$container;
   jqueryMap = { $container  : $container };
 };
 // End DOM method /setJqueryMap/
 //---------------- END DOM METHODS  -----------------------------

 //----------------- BEGIN EVENT HANDLERS ------------------------ 

updatescrollbar = function(){	
   var oScrollbar5 = $('#scrollbar1');
   oScrollbar5.tinyscrollbar();
   oScrollbar5.tinyscrollbar_update();
};

setReadyHandler = function () {
   $(document).ready(function(){
     $('#etaapp-scrollbar1').tinyscrollbar({ thumbsize : 15 });
   });
 };

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
   setJqueryMap();
   setReadyHandler();
   etaapp.family_model.initModule();
   formHtml($container);

   return true;
 };

 return {
   configModule    : configModule,
   initModule      : initModule
 };

 } ());
 
