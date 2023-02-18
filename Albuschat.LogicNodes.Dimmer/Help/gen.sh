#!/bin/bash

for i in */
do
   pandoc -f markdown -o $i/d_albuschat_gmail_com.logic.Dimmer.html Help.md --template=easy_template.html --metadata title="Logikbaustein \"Umschalten und Dimmen per Binäreingang\""
done

