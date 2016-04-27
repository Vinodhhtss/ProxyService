 
   local function split(str, sep)
   local result = {}
   local regex = string.format("([^%s]+)", sep)
   for line,out in str:gmatch(regex) do
      table.insert(result, line)
   end
   return result
end

  if ProxyAPIObject:GetRequestHeaderValue("Host") == "mail.google.com" then
   local headerValue = ProxyAPIObject:GetRequestHeaderValue("Content-Disposition")  
if headerValue == "" then
	return
end


local replcedVlaue =   ProxyAPIObject:ReplaceString(headerValue,"\"=?UTF-8?Q?", "")
replcedVlaue =   ProxyAPIObject:ReplaceString(replcedVlaue,"attachment; filename=", "")
   replcedVlaue =   ProxyAPIObject:ReplaceString(replcedVlaue ,"?=\"", "")
replcedVlaue =   ProxyAPIObject:ReplaceString(replcedVlaue ,"=5f", "_")

--ProxyAPIObject:WriteToFile("E:\\gmaillogin.txt", replcedVlaue )
--   local splittedValue =   split( headerValue, "?")
-- ProxyAPIObject:WriteToFile("E:\\gmaillogin.txt", table.getn(splittedValue))
  --for key,value in replcedVlaue do 
       local msg = "\n" .. ProxyAPIObject:GetDateTime() .. "\t" .. "Attempting to attach the file " .. replcedVlaue  .. " in GMail from " .. ProxyAPIObject:GetClientName()
		ProxyAPIObject:WriteToFile("E:\\gmaillogin.txt", msg) 
  --end
  end


