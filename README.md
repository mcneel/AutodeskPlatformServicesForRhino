# AutodeskPlatformServices
Autodesk Platform Services Utilities


## Build and Test
- Build
- Apply this fix: http://stackoverflow.com/questions/4019466/httplistener-access-denied
  - Open Terminal app or `cmd.exe` in Windows
  - Run `netsh http add urlacl url=http://+:8080/ user=<username>` but replace `<username>` with your machine username
- Add build path to `GrasshopperDeveloperSettings` command in Rhino
- Install `*.rhp` plugin inside the build path in Rhino

### Connect to APS
- Open Grasshopper, and drop the `Connect` component from APS panel onto the canvas.
- Click connect, login using your Autodesk account and "Allow" access
- Verify the output `Token` parameter contains a `Bearer ...` token

### Tests
- Open any Grasshopper files under `tests/`
