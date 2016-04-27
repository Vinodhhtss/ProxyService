  if ProxyAPIObject:GetRequestHeaderValue("Host") == "login.live.com" then
    local requestBody = ProxyAPIObject:GetRequestBody()
     i, j = string.find(requestBody, "login=")
     if i == nil or j == nil then
       return
     end
local subRequestBody = requestBody:sub(j + 1)
  l, k = subRequestBody:find('%&')
         if k == nil then
           return
         end
   
         if k > -1 then 
                local required = subRequestBody:sub(0, k-1)     
                if string.len(required) == 0 then
                return 
                end
                local msg = "\n" .. ProxyAPIObject:GetDateTime() .. "\t" .. required .. " attempts for signing into HotMail from " .. ProxyAPIObject:GetClientName() 
                  ProxyAPIObject:WriteToFile("E:\\hotmaillogin.txt", msg)
           end    
      end       

