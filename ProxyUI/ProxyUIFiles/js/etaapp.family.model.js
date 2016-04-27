/*
 * etaapp.family.model.js
 * Family Model 
 */

 /*global $, etaapp */

 etaapp.family_model = (function () {
   'use strict';

   var
     configMap = {},

     stateMap = {
       family_id_map : {},
       family_db  : TAFFY()
     },

     isFakeData = true,

     makeFamily, familyProto, get_db, get_id_map,  initModule;

   familyProto = {
     get_family_name : function () {
       return this.name;
     },

     get_family_samples : function () {
       return this.samples;
     },

     get_family_id : function () {
       return this.id;
     }
   };

   makeFamily = function (family_map) {
     var family,
       id       = family_map.id,
       samples  = family_map.samples,
       name     = family_map.name; 

       family   = Object.create(familyProto);
       family.name = name;
       family.samples = samples;
       family.id    =  id;

       stateMap.family_id_map[id] = family;     
       stateMap.family_db.insert(family);

       return family;
   };


   get_db = function () { alert('Hi'); return stateMap.family_db; };
   get_id_map  = function () { 
       return stateMap.family_id_map; 
     };


   initModule = function () {
     var i, family_list, family_map;

     if (isFakeData) {
         family_list = etaapp.fake.getFamilyList();
         for (i=0; i<family_list.length; i++ ) {
           family_map = family_list[i];
           makeFamily({
              name :  family_map.name,
              id   :  family_map.id,
              samples   : family_map.samples
           });
         }
      }

    }; 

    return {
      initModule  : initModule,
      get_db      : get_db,
      get_id_map     : get_id_map 
    };
 }());
