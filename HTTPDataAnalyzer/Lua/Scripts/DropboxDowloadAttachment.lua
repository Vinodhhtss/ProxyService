 

  if ProxyAPIObject:GetRequestHeaderValue("Host") == "dl-web.dropbox.com" then
   local headerValue = ProxyAPIObject:GetResponseHeaderValue("content-disposition")  
if headerValue == "" then
	return
end
i, j = string.find(headerValue , "filename=")
   if j == nil then
     return
   end
 local headerV = headerValue:sub(j+1)
       k, l = headerV:find(';') 
local replcedVlaue = headerV
     if k ~= nil  then
    replcedVlaue = headerV:sub(0, k-1) 
     end

       local msg = "\n" .. ProxyAPIObject:GetDateTime() .. "\t" .. "Attempting to download the file " .. replcedVlaue  .. " in Dropbox from " .. ProxyAPIObject:GetClientName()
		ProxyAPIObject:WriteToFile("E:\\dropboxlogin.txt", msg) 
  
  end


