# Contains useful Logic Nodes

(c) Daniel Albuschat, 2018-2024

See:
- Albuschat.LogicNodes.WebRequest/README.md
- Albuschat.LogicNodes.Dimmer/README.md
- Albuschat.LogicNodes.WakeOnLan/README.md

# How to Build

This guide is using C:\Source\ as a common root folder for this repository and the logic node SDK. You can choose any other folder as your root folder, just make sure that you replace C:\Source\ with your folder in the following steps.

## Prerequisites

- Building these logic nodes is only tested on Microsoft Windows.
- Install Visual Studio 2022 (later versions might work, but are not tested) ([Link](https://visualstudio.com)).
- Install .NET 4.6.2 Developer Pack ([Link](https://dotnet.microsoft.com/en-us/download/dotnet-framework/net462)).
- Download the Gira Logic Node SDK from https://developer.gira.com.
- Copy the contained "LogicNodeSDK" to `C:\Source\`, so that the folder `C:\Source\LogidNodeSDK\` contains the Gira Logic Node SDK libraries.
- Follow all steps to request and create a Developer Certificate `.p12` file.
- Save your final .p12 file as `C:\Source\LogicNodeCertificate.p12` - the exact file name and location is important so that the following build steps work out of the box.

## Clone this repository

```
git clone https://github.com/daniel-kun/Albuschat.LogicNodes.git
```

## Open and Build the solution

- Navigate to `C:\Source\Albuschat.LogicNodes\` and open the contained `AlbsuchatLogicNodes.sln` file with Visual Studio.
- Execute `Build > Build solution`.

## Troubleshooting

- If an error message appears regarding missing dependencies `LogicModule.Nodes.Helpers` or `LogicModule.ObjectModel`, then the "LogicNodeSDK" folder does not exist next to the "Albuschat.LogicNodes" folder.
- If an error message appears that contains texts like "Signature" or "Signing" or similar, then:
-- Either your `LogicNodesCertificate.p12` file does not exist or is not in the `C:\Source\` folder.
-- You did not change the file extension from `*.pfx` to `*.p12`.

# How to test locally

## Creating a project

- Create a new project in the most recent version of the Gira Project Assitant.
- Make sure that your Gira X1 firmware is using the most recent version that is available.
- Open the created project, navigate to the `Logic Editor`.
-- Select the "Add logic node" button in the top left corner
-- Select one of the .zip files from `C:\Source\Albuschat.LogicNodes\Output\`
- Now you can create a new logic page and you'll find the logic node on the logic page's library on the left side.

## Testing changes

If you made changes to your logic node that you want to test in the GPA Simulator or on the device, make sure to follow these steps:

- Increase the version number in the `Manifest.json` file of the logic node that you modified.
- Add the logic node to the GPA, as in the previous step
- After adding a logic node with an increased version number, a button "Update all logic nodes" will appear that you need to press
- Now you need to re-open the project and then you can test your changes in the Simulator and on the device.

Make sure that you follow these steps for each and any change that you want to test. Missing one of the steps will leave you wonder why your changes did not have an effect.

## Troubleshooting

- If you get an error message after commissioning that says "Failed to verify signature" or similar:
-- The signature Post-Build step has failed during build. See Troubleshooting section in "How to build" above.
-- Your `LogicNodesCertificate.p12` does not contain the whole certificate chain. Make sure to select the file format "PKCS#12 __chain__ (*.pfx)" in the xca tool.

# Something did not work as expected

Please open a ticket in this repository. If I can find the time, I'll be glad to help you out!
