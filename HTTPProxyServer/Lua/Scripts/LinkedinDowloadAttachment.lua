 
   local function split(str, sep)
   local result = {}
   local regex = string.format("([^%s]+)", sep)
   for line,out in str:gmatch(regex) do
      table.insert(result, line)
   end
   return result
end

  if ProxyAPIObject:GetRequestHeaderValue("Host") == "www.linkedin.com" then
   local headerValue = ProxyAPIObject:GetResponseHeaderValue("Content-disposition")  
if headerValue == "" then
	return
end

local replcedVlaue =   ProxyAPIObject:ReplaceString(headerValue,"\"=?UTF-8?Q?", "")
replcedVlaue =   ProxyAPIObject:ReplaceString(replcedVlaue,"attachment; filename=", "")
   replcedVlaue =   ProxyAPIObject:ReplaceString(replcedVlaue ,"?=\"", "")
replcedVlaue =   ProxyAPIObject:ReplaceString(replcedVlaue ,"=5f", "_")

       local msg = "\n" .. ProxyAPIObject:GetDateTime() .. "\t" .. "Attempting to download the file " .. replcedVlaue  .. " in linkedin from " .. ProxyAPIObject:GetClientName()
		ProxyAPIObject:WriteToFile("E:\\linkedinlogin.txt", msg) 
  
  end


