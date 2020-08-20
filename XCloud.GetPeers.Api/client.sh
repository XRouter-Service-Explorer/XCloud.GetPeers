#!/bin/bash
version=$1
url="http://localhost:8080/peers/getdailypeerlist?version="
url+="${version}"
echo "$url"
res=$(curl -s -L -X GET "$url" -H 'Content-Type: application/json')
echo "$res"
