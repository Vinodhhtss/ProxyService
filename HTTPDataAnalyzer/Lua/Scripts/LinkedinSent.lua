  if ProxyAPIObject:GetRequestHeaderValue("Host") == "www.linkedin.com" then
    
    local requestBody = ProxyAPIObject:GetRequestBody()
   i, j = string.find(requestBody, "recipientNames=")
   if i == nil  or j== nil then
     return
   end   
   
    local reqBody1 = requestBody:sub(j+1)
     k, l = reqBody1:find('%&') 
     if k == nil or l == nil then
       return
     end
      if k > -1 then 
      local required = reqBody1:sub(0 ,k-1)
      if string.len(required) == 0 then
                return 
                end
           local msg = "\n" .. ProxyAPIObject:GetDateTime() .. "\t"  .. " Sending mail to " .. required .. " from " .. ProxyAPIObject:GetClientName()  
                  ProxyAPIObject:WriteToFile("E:\\linkedinlogin.txt", msg)
      end
    end       
 
