/ip service set api port=8728 disabled=no
/user group add name=api-group policies=read,write,policy,test,api
/user add name=billing-api group=api-group password=PASSWORD_KUAT
/ip firewall filter add chain=input protocol=tcp dst-port=8728 src-address=IP_SERVER_BILLING action=accept
