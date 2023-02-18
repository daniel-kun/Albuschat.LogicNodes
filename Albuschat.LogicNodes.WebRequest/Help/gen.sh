#!/bin/bash

for i in */
do
   pandoc -f markdown -o $i/d_albuschat_gmail_com.logic.WebRequest.html Help.md --template=easy_template.html --metadata title="Logikbaustein \"Web Request\""
done

