# Autodesk Platform Services For Rhino

This is a Rhino+Grasshopper plugin that aims to provide easy access to Autodesk Platform Services. It requires an authenticated connection to the APIs, so you would need to create your own App or the APS platform and register for the available APIs.

This first release of the plugins include utilities to use the new [Parameter Services](https://blogs.autodesk.com/revit/2022/12/04/whats-new-in-parameters-service/):
- **Parameter Editor Panel** in Rhino: From this panel you can connect to APS and access the parameter collections under your account, adding or removing parameters to selected Rhino objects
- **Grasshopper Components** to connect to APS, and communicate with Parameter Services, querying disciplines, specs, categories, groups, collections, etc.

⚠️ To succssfully connect to the API on Windows, you would need to apply the fix described under **Build and Test**


## Build and Test
- Build
- Apply this fix: http://stackoverflow.com/questions/4019466/httplistener-access-denied
  - Open Terminal app or `cmd.exe` in Windows
  - Run `netsh http add urlacl url=http://+:8080/ user=<username>` but replace `<username>` with your machine username
- Add build path to `GrasshopperDeveloperSettings` command in Rhino
- Install `*.rhp` plugin inside the build path in Rhino

## Install

You can install this plugin from Rhino package manager by running **Package Manager** command. Make sure the "Include Pre-Releases" checkbox is checked. 

![](docs/rh_pkgmgr_install.png)


### Connect to APS (Rhino)

![](docs/rh_edit_panel.png)

Open **APS Parameter Editor** panel and click on **Connect**. From the connection dialog choose a method to provide the App information, either by typing in the values or providing the environment variable names that contain the values (you need to setup the environment variables before launching Rhino)

### Connect to APS (Grasshopper)

![](docs/gh_connect_comps.png)

Open Grasshopper, and drop a **Construct Connection Info** or **Construct Connection Info (Env Vars)** and feed the required values to the input parameters. Then drop the **Connect** component from APS panel onto the canvas, and feed the connection info to the component.

Click connect, login using your Autodesk account and "Allow" access

![](docs/gh_connect_envvars.png)

Use the **Deconstruct Connection Info** component to verify the output `Token` parameter contains a `Bearer ...` token

![](docs/gh_verify_token.png)


### Tests

Open any Rhino or Grasshopper files under `tests/`
