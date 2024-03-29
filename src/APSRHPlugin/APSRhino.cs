﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

using Rhino;
using Rhino.UI;
using Rhino.DocObjects;

using AutodeskPlatformServices;
using APSRHPlugin.Dialogs;

namespace APSRHPlugin
{
    public static class APSRhino
    {
        public static APSRHPluginConfigs Configs { get; } = APSRHPluginConfigs.Read();

        public static bool IsConnected() => Configs.ConnectionInfo != null && APSAPI.State == State.Connected;

        public static async Task<bool> CreateConnectionAsync()
        {
            var dlg = new APSConnectDialog();
            await dlg.ShowModalAsync();

            if (!dlg.Cancelled)
            {
                Configs.ConnectionInfo = dlg.Connection;
                Configs.Account = dlg.Account;
            }

            Configs.Write();

            return !dlg.Cancelled;
        }

        public static Task<State> ConnectAsync() => APSAPI.ConnectAsync(Configs.ConnectionInfo);

        public static Task<State> ReConnectAsync() => APSAPI.ReConnectAsync(Configs.ConnectionInfo);

        public static void ReportAPIError(Exception ex = null) => ReportError(ex?.Message ?? APSAPI.GetErrorMessage());

        public static void ReportError(string message)
        {
            Rhino.UI.Dialogs.ShowMessage(
                RhinoEtoApp.MainWindow,
                $"API Error: {message}",
                "APS Parameters",
                ShowMessageButton.OK,
                ShowMessageIcon.Error,
                ShowMessageDefaultButton.Button1,
                ShowMessageOptions.TopMost,
                ShowMessageMode.ApplicationModal
               );
        }

        #region Parameters API
        public static class Parameters
        {
            public static Task<GetGroupsResult> GetGroupsAsync()
            {
                if (Configs.HasAccountId())
                    return APSAPI.Parameters.GetGroupsAsync(Configs.GetAccountId());
                return Task.FromResult(new GetGroupsResult());
            }

            public static Task<GetCollectionsResult> GetCollectionsAsync(string groupId)
            {
                if (Configs.HasAccountId())
                    return APSAPI.Parameters.GetCollectionsAsync(Configs.GetAccountId(), groupId);
                return Task.FromResult(new GetCollectionsResult());
            }

            public static Task<GetParametersResult> GetParametersAsync(string groupId, string collectionId)
            {
                if (Configs.HasAccountId())
                    return APSAPI.Parameters.GetParametersAsync(Configs.GetAccountId(), groupId, collectionId);
                return Task.FromResult(new GetParametersResult());
            }

            public static Task<GetParametersResult> GetParametersAsync(string groupId, string collectionId, string searchTerm)
            {
                if (Configs.HasAccountId())
                {
                    if (string.IsNullOrEmpty(searchTerm))
                        return APSAPI.Parameters.GetParametersAsync(Configs.GetAccountId(), groupId, collectionId);
                    else
                        return APSAPI.Parameters.SearchParametersAsync(Configs.GetAccountId(), groupId, collectionId, searchTerm);
                }

                return Task.FromResult(new GetParametersResult());
            }

            public static IEnumerable<Parameter> GetParameters(RhinoDoc doc)
            {
                var parameters = new HashSet<Parameter>();

                foreach (RhinoObject obj in doc.Objects)
                    foreach (Parameter param in GetParameters(obj))
                        parameters.Add(param);

                return parameters;
            }

            public static bool GetParameter(ObjRef objRef, Parameter param, out object value) => GetParameter(objRef.Object(), param, out value);

            public static bool GetParameter(RhinoObject obj, Parameter param, out object value)
            {
                if (obj is null)
                    throw new ArgumentNullException(nameof(obj));

                value = default;

                if (obj.Attributes.UserData.Find(typeof(APSParamsData)) is APSParamsData objectParams)
                {
                    return objectParams.GetParam(param, out value);
                }

                return false;
            }

            public static IEnumerable<Parameter> GetParameters(ObjRef objRef) => GetParameters(objRef.Object());

            public static IEnumerable<Parameter> GetParameters(RhinoObject obj)
            {
                if (obj is null)
                    throw new ArgumentNullException(nameof(obj));

                if (obj.Attributes.UserData.Find(typeof(APSParamsData)) is APSParamsData objectParams)
                {
                    return objectParams.GetParameterIds().Select(p =>
                    {
                        if (APSAPI.Parameters.GetParameter(p) is Parameter param)
                            return param;
                        return null;
                    }).OfType<Parameter>()
                      .AsEnumerable();
                }

                return Enumerable.Empty<Parameter>();
            }

            public static async Task<IEnumerable<Parameter>> GetParametersAsync(RhinoObject obj)
            {
                if (obj is null)
                    throw new ArgumentNullException(nameof(obj));

                if (obj.Attributes.UserData.Find(typeof(APSParamsData)) is APSParamsData objectParams)
                {
                    var parameters = new List<Parameter>();
                    foreach (var paramId in objectParams.GetParameterIds())
                    {
                        if (await APSAPI.Parameters.GetParameterAsync(paramId) is Parameter param)
                            parameters.Add(param);
                    }

                    return parameters.AsEnumerable();
                }

                return Enumerable.Empty<Parameter>();
            }

            public static bool SetParameter(ObjRef objRef, Parameter param, object value = default) => SetParameter(objRef.Object(), param, value);

            public static bool SetParameter(RhinoObject obj, Parameter param, object value = default)
            {
                if (obj is null)
                    throw new ArgumentNullException(nameof(obj));

                APSParamsData objectParams;
                if (obj.Attributes.UserData.Find(typeof(APSParamsData)) is APSParamsData op)
                {
                    objectParams = op;
                }
                else
                {
                    objectParams = new APSParamsData();
                    obj.Attributes.UserData.Add(objectParams);
                }

                var res = objectParams.SetParam(param, value);
                obj.Document.Modified |= res;
                return res;
            }

            public static bool UnsetParameter(RhinoObject obj, Parameter param)
            {
                if (obj is null)
                    throw new ArgumentNullException(nameof(obj));

                if (obj.Attributes.UserData.Find(typeof(APSParamsData)) is APSParamsData objectParams)
                {
                    var res = objectParams.UnsetParam(param);
                    obj.Document.Modified |= res;
                    return res;
                }
                else
                    return false;
            }
        }
        #endregion
    }
}
