#!/bin/bash

for i in */
do
   pandoc -f markdown -o $i/d_albuschat_gmail_com.logic.WakeOnLan.html Help.md --template=easy_template.html --metadata title="Logikbaustein \"Wake on LAN\""
done

