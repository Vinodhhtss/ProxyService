function split(str,sep)
    local array = {}
    local reg = string.format("([^%s]+)",sep)
    for mem in string.gmatch(str,reg) do
        table.insert(array, mem)
    end
    return array
end  
 

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
   
     k, l =  requestBody:find('%[') 
     if k == nil or l == nil then
       return
     end
 o, p =  requestBody:find('%]') 
     if o == nil or p == nil then
       return
     end
   if k+1 == o then
     return
   end
    local temprequired = requestBody:sub(k+2 ,o-2) 
myTable = split(temprequired, "|")

      local required = myTable[3]:sub(37,string.len(myTable[3]) -1)
      if string.len(required) == 0 then
                return 
                end
            local msg = "\n" .. ProxyAPIObject:GetDateTime() .. "\t" .. "Attempting to attach the file " .. required  .. " in hotmail from " .. ProxyAPIObject :GetClientName() 
                  ProxyAPIObject:WriteToFile("E:\\hotmaillogin.txt", msg)
     
    end    


