/**
 * etaapp.util_b.js
 * JS Browser utilities
 */

 /* global $, etaapp, getComputedStyle */

 etaapp.util_b = (function () {
  'use strict' ;
  //--------------------- BEGIN MODULE SCOPE VARIABLES ------------

  var
    configMap = {
      regex_encode_html  : /[&"'><]/g,
      regex_encode_noamp : /["'><]/g,
      html_encode_map    : {
       '&'   : '&#38;',
       '"'   : '&#34;',
       "'"   : '&#39;',
       '>'   : '&#62;',
       '<'   : '&#60;'
      } 
  },

  decodeHtml, encodeHtml, getEmSize;

  configMap.encode_noamp_map = $.extend(
    {}, configMap.html_encode_map
  );
  delete configMap.encode_noamp_map['&'];
  
  //-------------- END MODULE SCOPE VARIABLES -----------------------

  //-------------- BEGIN UTILITY METHODS ----------------------------
  // Begin decodeHtml
  // Decodes HTML entities in a browser-friendly way
  //
  decodeHtml = function (str) {
    return $('<div/>').html(str || '').text();
  };

  // Begin encodeHtml
  // Encoder for Html
  //
  encodeHtml = function ( input_arg_str, exclude_amp ) {
    var
      input_str = String(input_arg_str),
      regex, lookup_map ;

    if (exclude_amp) {
      lookup_map = configMap.encode_noamp_map;
      regex      = configMap.regex_encode_noamp;
    }
    else {
      lookup_map = configMap.encode_noamp_map;
      regex      = configMap.regex_encode_html;  
    }
    return input_str.replace(regex,
      function (match, name) {
        return lookup_map[match] || '';
      }
    );
  };
  // End encodeHtml

  // Begin getEmSize
  // returns sie of ems in pixels
  getEmSize = function (elem) {
    return Number (
      getComputedStyle(elem, '').fontSize.match(/\d*\.?\d*/)[0]
    );
  };
  // End getEmSize

  //export methods
  return {
    decodeHtml  : decodeHtml,
    encodeHtml  : encodeHtml,
    getEmSize   : getEmSize
  };
  //-------------------- END PUBLIC METHODS -----------------------
 }());
    


