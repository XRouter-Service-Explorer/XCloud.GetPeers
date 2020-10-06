#!/bin/bash
{
	apt-get -qq update -y
	apt-get -qq install -y jq
} || {
	echo "jq failed to install"
	exit 1
}

jq 'del(.CoinConfig[])' < appsettings.json > temp && mv temp appsettings.json

jq -c '.CoinConfig[]' config/GetDailyPeerList.config.json | while read coinconfig; 
do
	coin=`jq -r '.Coin' <<< $coinconfig`

	xbridge_conf=$( curl -s -L -X GET 'https://raw.githubusercontent.com/blocknetdx/blockchain-configuration-files/master/manifest.json' -H 'Content-Type: application/json'| jq --arg coin "$coin" --raw-output '[.[]|select(.ticker==$coin)][0].xbridge_conf')
	coin_long_name=$( curl -s -L -X GET 'https://raw.githubusercontent.com/blocknetdx/blockchain-configuration-files/master/xbridge-confs/'${xbridge_conf} -H 'Content-Type: application/json' | awk -F "=" '/Title/ {print $2}' | tr -d '\r')
	rpcport=$( curl -s -L -X GET 'https://raw.githubusercontent.com/blocknetdx/blockchain-configuration-files/master/xbridge-confs/'${xbridge_conf} -H 'Content-Type: application/json' | awk -F "=" '/Port/ {print $2}' | tr -d '\r')

	rpc_user=`jq -r '.RpcUserName' <<< $coinconfig`
	rpc_pass=`jq -r '.RpcPassword' <<< $coinconfig`
	endpoint=`jq -r '.Endpoint' <<< $coinconfig`

	daemonUrl="http://"
	daemonUrl=${daemonUrl}${rpc_user}:${rpc_pass}@${endpoint}:${rpcport}
	jq --arg coinShortName "$coin" --arg coinLongName "$coin_long_name" --arg daemonUrl "$daemonUrl" --arg rpcUser "$rpc_user" --arg rpcPass "$rpc_pass" '.CoinConfig += [{"CoinShortName": $coinShortName, "CoinLongName": $coinLongName,"DaemonUrl": $daemonUrl, "WalletPassword": "", "RpcUserName": $rpcUser, "RpcPassword": $rpcPass, "RpcRequestTimeoutInSeconds": 30}]' < appsettings.json > temp && mv temp appsettings.json	
done

dotnet XCloud.GetPeers.Api.dll
