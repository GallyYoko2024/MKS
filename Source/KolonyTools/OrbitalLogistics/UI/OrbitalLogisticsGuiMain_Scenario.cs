﻿using System;

using UnityEngine;
using USITools.UI;
using KSP.Localization;

namespace KolonyTools
{
    /// <summary>
    /// Displays the UI for <see cref="ScenarioOrbitalLogistics"/>.
    /// </summary>
    public class OrbitalLogisticsGuiMain_Scenario : MonoBehaviour, ITransferRequestViewParent
    {
        #region Local instance variables
        private ScenarioOrbitalLogistics _scenario;
        private Vector2 _scrollPosition;
        private OrbitalLogisticsTransferRequest _selectedTransfer;
        private bool _isVisible;
        private GUIStyle _labelStyle;

        public OrbitalLogisticsGuiMain_Scenario()
        {
            _labelStyle = new GUIStyle(HighLogic.Skin.label);
            if (Localizer.CurrentLanguage == "zh-cn")
            {
                _labelStyle.fontSize = 15;
            }
        }

        #endregion

        #region Public instance variables
        public OrbitalLogisticsGui_ReviewTransfer ReviewTransferGui;
        #endregion

        #region Constructors
        public OrbitalLogisticsGuiMain_Scenario(ScenarioOrbitalLogistics scenario)
        {
            _scenario = scenario;
            SetVisible(true);
        }
        #endregion

        /// <summary>
        /// Called by <see cref="KolonizationMonitor"/> to render the UI.
        /// </summary>
        public void DrawWindow()
        {
            if (!_isVisible)
                return;

            // Declare some temporary variables
            GUIStyle labelStyle;
            OrbitalLogisticsTransferRequest transfer;
            OrbitalLogisticsTransferRequest[] pendingTransfers = _scenario.PendingTransfers.ToArray();
            OrbitalLogisticsTransferRequest[] expiredTransfers = _scenario.ExpiredTransfers.ToArray();

            // Display everything inside a scroll view
            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, UIHelper.scrollStyle, GUILayout.Width(680), GUILayout.Height(360));
            GUILayout.BeginVertical();

            // Display pending transfer section header
            GUILayout.BeginHorizontal();
            GUILayout.Label(Localizer.Format("#LOC_USI_MKS_KolonizationMonitor_OL_PTran"), UIHelper.labelStyle, GUILayout.Width(200));
            GUILayout.EndHorizontal();

            GUILayout.BeginVertical();

            if (pendingTransfers.Length == 0)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(Localizer.Format("#LOC_USI_MKS_KolonizationMonitor_OL_None"), UIHelper.whiteLabelStyle, GUILayout.Width(160));
                GUILayout.EndHorizontal();
            }
            else
            {
                // Display pending transfer column headers
                GUILayout.BeginHorizontal();
                GUILayout.Label(string.Empty, UIHelper.labelStyle, GUILayout.Width(25));
                GUILayout.Label(Localizer.Format("#LOC_USI_MKS_KolonizationMonitor_OL_Ori"), UIHelper.whiteLabelStyle, GUILayout.Width(165));
                GUILayout.Label(Localizer.Format("#LOC_USI_MKS_KolonizationMonitor_OL_Des"), UIHelper.whiteLabelStyle, GUILayout.Width(165));
                GUILayout.Label(Localizer.Format("#LOC_USI_MKS_KolonizationMonitor_OL_Cost"), UIHelper.whiteLabelStyle, GUILayout.Width(80));
                GUILayout.Label(Localizer.Format("#LOC_USI_MKS_KolonizationMonitor_OL_Mass"), UIHelper.whiteLabelStyle, GUILayout.Width(80));
                GUILayout.Label(Localizer.Format("#LOC_USI_MKS_KolonizationMonitor_OL_ArT"), UIHelper.whiteLabelStyle, GUILayout.Width(90));
                GUILayout.EndHorizontal();

                // Display pending transfers
                for (int i = 0; i < pendingTransfers.Length; i++)
                {
                    transfer = pendingTransfers[i];

                    // Determine text color based on transfer status
                    if (transfer.Status == DeliveryStatus.Returning)
                    {
                        labelStyle = UIHelper.redLabelStyle;
                    }
                    else
                    {
                        labelStyle = UIHelper.yellowLabelStyle;
                    }

                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button(UIHelper.rightArrowSymbol, UIHelper.buttonStyle, GUILayout.Width(25), GUILayout.Height(22)))
                    {
                        _selectedTransfer = transfer;

                        if (ReviewTransferGui == null)
                            ReviewTransferGui = new OrbitalLogisticsGui_ReviewTransfer(_selectedTransfer, this);
                        else
                            ReviewTransferGui.Transfer = _selectedTransfer;

                        ReviewTransferGui.SetVisible(true);
                    }
                    GUILayout.Label(" " + transfer.OriginVesselName ?? "(Missing)", labelStyle, GUILayout.Width(165));
                    GUILayout.Label(transfer.DestinationVesselName ?? "(Missing)", labelStyle, GUILayout.Width(165));
                    GUILayout.Label(transfer.CalculateFuelUnits().ToString("F2"), labelStyle, GUILayout.Width(80));
                    GUILayout.Label(transfer.TotalMass().ToString("F2"), labelStyle, GUILayout.Width(80));
                    GUILayout.Label(
                        Utilities.FormatTime(transfer.GetArrivalTime() - Planetarium.GetUniversalTime()),
                        labelStyle, GUILayout.Width(90)
                    );
                    if (GUILayout.Button(UIHelper.deleteSymbol, UIHelper.buttonStyle, GUILayout.Width(22), GUILayout.Height(22)))
                    {
                        _selectedTransfer = null;

                        if (ReviewTransferGui != null)
                        {
                            ReviewTransferGui.Transfer = null;
                            ReviewTransferGui.SetVisible(false);
                        }

                        // Cancel transfer
                        AbortTransfer(transfer);
                    }
                    GUILayout.EndHorizontal();
                }
            }

            GUILayout.EndVertical();

            // Display a blank section to create some visual separation between sections
            GUILayout.BeginHorizontal();
            GUILayout.Label(string.Empty, UIHelper.labelStyle, GUILayout.Width(200));
            GUILayout.EndHorizontal();

            // Display expired transfer section header
            GUILayout.BeginHorizontal();
            GUILayout.Label(Localizer.Format("#LOC_USI_MKS_KolonizationMonitor_OL_ETran"), UIHelper.labelStyle, GUILayout.Width(200));
            GUILayout.EndHorizontal();

            GUILayout.BeginVertical();

            if (expiredTransfers.Length == 0)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(Localizer.Format("#LOC_USI_MKS_KolonizationMonitor_OL_None"), UIHelper.whiteLabelStyle, GUILayout.Width(160));
                GUILayout.EndHorizontal();
            }
            else
            {
                // Display expired transfer column headers
                GUILayout.BeginHorizontal();
                GUILayout.Label(string.Empty, UIHelper.labelStyle, GUILayout.Width(25));
                GUILayout.Label(Localizer.Format("#LOC_USI_MKS_KolonizationMonitor_OL_Ori"), UIHelper.whiteLabelStyle, GUILayout.Width(165));
                GUILayout.Label(Localizer.Format("#LOC_USI_MKS_KolonizationMonitor_OL_Des"), UIHelper.whiteLabelStyle, GUILayout.Width(165));
                GUILayout.Label(Localizer.Format("#LOC_USI_MKS_KolonizationMonitor_OL_Cost"), UIHelper.whiteLabelStyle, GUILayout.Width(80));
                GUILayout.Label(Localizer.Format("#LOC_USI_MKS_KolonizationMonitor_OL_Mass"), UIHelper.whiteLabelStyle, GUILayout.Width(80));
                GUILayout.Label(Localizer.Format("#LOC_USI_MKS_KolonizationMonitor_OL_Stt"), UIHelper.whiteLabelStyle, GUILayout.Width(90));
                GUILayout.EndHorizontal();

                // Diplay expired transfers
                for (int i = 0; i < expiredTransfers.Length; i++)
                {
                    transfer = expiredTransfers[i];

                    // Determine text color based on transfer status
                    if (transfer.Status == DeliveryStatus.Delivered)
                    {
                        labelStyle = UIHelper.yellowLabelStyle;
                    }
                    else
                    {
                        labelStyle = UIHelper.redLabelStyle;
                    }

                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button(UIHelper.rightArrowSymbol, UIHelper.buttonStyle, GUILayout.Width(25), GUILayout.Height(22)))
                    {
                        _selectedTransfer = transfer;

                        if (ReviewTransferGui == null)
                            ReviewTransferGui = new OrbitalLogisticsGui_ReviewTransfer(_selectedTransfer, this);
                        else
                            ReviewTransferGui.Transfer = _selectedTransfer;

                        ReviewTransferGui.SetVisible(true);
                    }
                    GUILayout.Label(" " + transfer.OriginVesselName ?? "(Missing)", labelStyle, GUILayout.Width(165));
                    GUILayout.Label(transfer.DestinationVesselName ?? "(Missing)", labelStyle, GUILayout.Width(165));
                    GUILayout.Label(transfer.CalculateFuelUnits().ToString("F2"), labelStyle, GUILayout.Width(80));
                    GUILayout.Label(transfer.TotalMass().ToString("F2"), labelStyle, GUILayout.Width(80));
                    GUILayout.Label(transfer.Status.ToString(), labelStyle, GUILayout.Width(90));
                    if (GUILayout.Button(UIHelper.deleteSymbol, UIHelper.buttonStyle, GUILayout.Width(22), GUILayout.Height(22)))
                    {
                        _selectedTransfer = null;

                        if (ReviewTransferGui != null)
                        {
                            ReviewTransferGui.Transfer = null;
                            ReviewTransferGui.SetVisible(false);
                        }

                        // Remove transfer from expired transfers list
                        _scenario.ExpiredTransfers.Remove(transfer);
                    }
                    GUILayout.EndHorizontal();
                }

                // Display a blank line to create some visual separation
                GUILayout.BeginHorizontal();
                GUILayout.Label(string.Empty, UIHelper.labelStyle, GUILayout.Width(200));
                GUILayout.EndHorizontal();

                // Display clear all expired transfers button
                GUILayout.BeginHorizontal();
                GUILayout.Label(string.Empty, UIHelper.labelStyle, GUILayout.Width(300));
                if (GUILayout.Button("Clear All", UIHelper.buttonStyle, GUILayout.Width(80), GUILayout.Height(25)))
                {
                    _selectedTransfer = null;

                    if (ReviewTransferGui != null)
                    {
                        ReviewTransferGui.Transfer = null;
                        ReviewTransferGui.SetVisible(false);
                    }

                    // Remove all transfers from expired transfers list
                    _scenario.ExpiredTransfers.Clear();
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();

            // Close main scroll view
            GUILayout.EndVertical();
            GUILayout.EndScrollView();
        }

        /// <summary>
        /// Implementation of <see cref="Window.SetVisible(bool)"/>.
        /// </summary>
        /// <param name="newValue"></param>
        public void SetVisible(bool newValue)
        {
            _isVisible = newValue;

            // Always hide child windows when main window visibility is altered
            if (ReviewTransferGui != null)
                ReviewTransferGui.SetVisible(false);
        }

        /// <summary>
        /// Returns the current visibility status.
        /// </summary>
        /// <returns></returns>
        public bool IsVisible()
        {
            return _isVisible;
        }

        /// <summary>
        /// Aborts a transfer via <see cref="ScenarioOrbitalLogistics"/>.
        /// </summary>
        /// <remarks>
        /// Implementation of <see cref="ITransferRequestViewParent.AbortTransfer(OrbitalLogisticsTransferRequest)"/>.
        /// </remarks>
        /// <param name="transfer"></param>
        public void AbortTransfer(OrbitalLogisticsTransferRequest transfer)
        {
            _scenario.AbortTransfer(transfer);
        }

        /// <summary>
        /// Resumes a cancelled transfer via <see cref="ScenarioOrbitalLogistics"/>.
        /// </summary>
        /// <remarks>
        /// Implementation of <see cref="ITransferRequestViewParent.ResumeTransfer(OrbitalLogisticsTransferRequest)"/>.
        /// </remarks>
        /// <param name="transfer"></param>
        public void ResumeTransfer(OrbitalLogisticsTransferRequest transfer)
        {
            _scenario.ResumeTransfer(transfer);
        }
    }
}
