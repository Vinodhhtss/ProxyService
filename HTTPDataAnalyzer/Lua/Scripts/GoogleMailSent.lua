  if ProxyAPIObject:GetRequestHeaderValue("Host") == "mail.google.com" then
    if ProxyAPIObject:IsResponseHeaderExists("Content-Security-Policy") == false then
      return
    end
    local requestBody = ProxyAPIObject:GetRequestBody()
   i, j = string.find(requestBody, "to=")
   if j== nil then
     return
   end
   j1 = requestBody:sub(j+1 ,1)
   if j1 == "&" then
     return
   end
   if i == nil then
     return
   end
    if i  > -1  then
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
           local msg = "\n" .. ProxyAPIObject:GetDateTime() .. "\t" .. " Sending mail to " .. required .. " from " .. ProxyAPIObject:GetClientName()      
                  ProxyAPIObject:WriteToFile("E:\\gmaillogin.txt", msg)
      end
    end       
  end
