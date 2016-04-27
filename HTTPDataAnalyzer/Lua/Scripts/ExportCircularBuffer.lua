local sitesNames = { "google", "hotmail"}
local fileLocation = "E:\\Temp\\lua.har" 

for _,v in pairs(sitesNames) do
  if ProxyAPIObject:HostContains(v) then
     ProxyAPIObject:SetCircularBufferLocation(fileLocation)
  end
end
