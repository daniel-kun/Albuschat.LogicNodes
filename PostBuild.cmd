mkdir %2Output

%2..\LogicNodesSDK\LogicNodeTool.exe updatemanifestversion %1Manifest.json %3 
erase %2Output\%4-*.zip
%2..\LogicNodesSDK\LogicNodeTool.exe create %1 %2Output\
for %%i in (%2Output\%4-*.zip) DO C:\Projects\LogicNodesSDK\SignLogicNodes.exe %2..\LogicNodeCertificate.p12 "" %%i
