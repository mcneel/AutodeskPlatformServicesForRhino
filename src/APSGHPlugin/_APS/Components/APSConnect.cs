using System;
using System.Drawing;
using System.Windows.Forms;

using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Attributes;

using AutodeskPlatformServices;

using APSGHPlugin.Parameters;
using APSGHPlugin.Types;
using APSRHPlugin;

namespace APSGHPlugin.Components
{
    public class APSConnect : APSComponent
    {
        #region Fields
        APSConnectionInfo _connectionInfo = default;
        #endregion

        public override Guid ComponentGuid => new Guid("8403106e-d093-4339-8949-9a63b138b580");

        public APSConnect()
          : base("Connect", "C", "Authenticate and connect to Autodesk Platform Services API", "APS", "APS")
        {
        }

        class ExpireButtonAttributes : GH_ComponentAttributes
        {
            const string _btnText = "Connect";
            Rectangle _btnBounds = Rectangle.Empty;
            bool _pressed = false;

            new APSConnect Owner => base.Owner as APSConnect;

            public ExpireButtonAttributes(APSConnect owner) : base(owner) { }

            protected override void Layout()
            {
                base.Layout();

                if (!APSRhino.IsConnected())
                {
                    Rectangle newBounds = GH_Convert.ToRectangle(Bounds);
                    newBounds.Height += 22;
                    Rectangle buttonBounds = newBounds;
                    buttonBounds.Y = buttonBounds.Bottom - 22;
                    buttonBounds.Height = 22;
                    buttonBounds.Inflate(-2, -2);
                    Bounds = (RectangleF)newBounds;

                    _btnBounds = buttonBounds;
                }
                else
                    _btnBounds = Rectangle.Empty;
            }

            protected override void Render(GH_Canvas canvas, Graphics graphics, GH_CanvasChannel channel)
            {
                base.Render(canvas, graphics, channel);

                if (channel == GH_CanvasChannel.Objects && !APSRhino.IsConnected())
                {
                    using (var ghCapsule = _pressed ?
                      GH_Capsule.CreateTextCapsule(_btnBounds, _btnBounds, GH_Palette.Grey, _btnText, 2, 0) :
                      GH_Capsule.CreateTextCapsule(_btnBounds, _btnBounds, GH_Palette.Black, _btnText, 2, 0))
                        ghCapsule.Render(graphics, Selected, Owner.Locked, false);
                }
            }

            public override GH_ObjectResponse RespondToMouseUp(GH_Canvas sender, GH_CanvasMouseEvent e)
            {
                if (_pressed)
                {
                    _pressed = false;

                    ConnectAsync();

                    sender.Refresh();
                    return GH_ObjectResponse.Release;
                }

                return base.RespondToMouseUp(sender, e);
            }

            public override GH_ObjectResponse RespondToMouseDown(GH_Canvas sender, GH_CanvasMouseEvent e)
            {
                if (e.Button != MouseButtons.Left || !((RectangleF)_btnBounds).Contains(e.CanvasLocation))
                    return base.RespondToMouseDown(sender, e);

                _pressed = true;
                sender.Refresh();
                return GH_ObjectResponse.Capture;
            }

            async void ConnectAsync()
            {
                try
                {
                    await APSRhino.ConnectAsync();
                    Owner.ExpireSolution(true);
                }
                catch (Exception ex)
                {
                    Owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"Connection Error: {ex}\n{ex.StackTrace}");
                }
                finally
                {
                    Owner.OnDisplayExpired(true);
                }
            }
        }

        public override void CreateAttributes() => Attributes = new ExpireButtonAttributes(this);

        public override void AppendAdditionalMenuItems(ToolStripDropDown menu)
        {
            base.AppendAdditionalMenuItems(menu);

            ToolStripMenuItem syncItem = Menu_AppendItem(menu, "Reconnect", OnReconnect);
            syncItem.ToolTipText = "Reconnect to APS";
        }

        protected override void RegisterInputParams(GH_InputParamManager PM)
        {
            PM.AddParameter(new APSConnectionInfoParam(), "Connection Info", "CI", "APS API connection info", GH_ParamAccess.item);
            PM[0].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager PM)
        {
            PM.AddParameter(new APSConnectionInfoParam(), "Connection Info", "CI", "APS API connection info", GH_ParamAccess.item);
        }

        protected override void BeforeSolveInstance()
        {
            base.BeforeSolveInstance();

            _connectionInfo = default;
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // create app info only from the first set of app info
            if (_connectionInfo is null)
            {
                APSConnectionInfo connectionInfo = default;
                if (DA.GetData(0, ref connectionInfo))
                    _connectionInfo = connectionInfo;
            }

            if (APSRhino.IsConnected())
            {
                if (APSRhino.Configs.ConnectionInfo.Equals(_connectionInfo?.Value))
                    DA.SetData(0, _connectionInfo);
                else
                    DA.SetData(0, new APSConnectionInfo(APSRhino.Configs.ConnectionInfo, APSRhino.Configs.GetAccountId()));
            }
            else if (State.Errored == APSAPI.State)
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, APSAPI.GetErrorMessage());
        }

        async void OnReconnect(object sender, EventArgs e)
        {
            try
            {
                if (_connectionInfo != null)
                    APSRhino.Configs.ConnectionInfo = _connectionInfo.Value;

                await APSRhino.ReConnectAsync();
                ExpireSolution(true);
            }
            catch (Exception ex)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"Connection Error: {ex}\n{ex.StackTrace}");
            }
            finally
            {
                OnDisplayExpired(true);
            }
        }
    }
}