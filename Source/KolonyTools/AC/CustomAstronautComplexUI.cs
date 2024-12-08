using System;
using System.Collections.Generic;
using System.Linq;
using KSP.UI.Screens;
using UnityEngine;
using KSP.Localization;

//Borrowed from The Read Panda's excellent mod, TRP-Hire, with permission.

namespace KolonyTools.AC
{
    class CustomAstronautComplexUI : MonoBehaviour
    {
        private Rect _areaRect;
        private float KBulk = 1;
        private int KBulki = 1;
        private int crewWeCanHire = 10;
        private static float KStupidity = 50;
        private static float KCourage = 50;
        private static bool KFearless = false;
        private static int KCareer = 0;
        private List<Kolonist> _kolonists;
        private static int KLevel = 0;
        private string[] _recruitableKolonists;
        private string[] _displayRecruitableKolonists;
        private string[] KLevelStringsZero = new string[1] { Localizer.Format("#LOC_USI_MKS_AstronautComplex_Lv0") };
        private string[] KLevelStringsOne = new string[2] { Localizer.Format("#LOC_USI_MKS_AstronautComplex_Lv0"), Localizer.Format("#LOC_USI_MKS_AstronautComplex_Lv1") };
        private string[] KLevelStringsTwo = new string[3] { Localizer.Format("#LOC_USI_MKS_AstronautComplex_Lv0"), Localizer.Format("#LOC_USI_MKS_AstronautComplex_Lv1"), Localizer.Format("#LOC_USI_MKS_AstronautComplex_Lv2") };
        //private string[] KLevelStringsAll = new string[6] { "Level 0", "Level 1", "Level 2", "Level 3", "Level 4", "Level 5" };
        private static int KGender = 0;
        private GUIContent KMale;
        private GUIContent KFemale;
        private GUIContent KGRandom;
        private GUIContent[] KGendArray;
        private GUIStyle _ButtonStyle;
        private GUIStyle _GenderButtonStyle;
        private bool _hasInitStyles = false;
        Color basecolor;
        private float ACLevel = 0;
        private double KDead;
        private double DCost = 1;
        KerbalRoster roster;
        private bool hTest = true;
        private bool hasKredits = true;
        private bool kerExp;
        private static string RecruitLevel = "RecruitementLevel";

        [KSPAddon(KSPAddon.Startup.Instantly, true)]
        public class StaticLoader : MonoBehaviour
        {
            public StaticLoader()
            {
                //Debug.Log("InitStaticData");
                for (int level = 1; level <= 5; level++)
                {
                    var expValue = GetExperienceNeededFor(level);
                    KerbalRoster.AddExperienceType(RecruitLevel + level, "Recruited at level " + level + " on", 0.0f, expValue);
                }
            }
        }

        private void InitStyles()
        {
            _ButtonStyle = new GUIStyle(HighLogic.Skin.button);
            if (Localizer.CurrentLanguage == "zh-cn")
            {
                _ButtonStyle.fontStyle = FontStyle.Normal;
                _ButtonStyle.normal.textColor = Color.white;
            }  
            _GenderButtonStyle = new GUIStyle(HighLogic.Skin.button);
            if (Localizer.CurrentLanguage == "zh-cn")
            {
                _GenderButtonStyle.fontSize = 20;
                _GenderButtonStyle.fontStyle = FontStyle.Normal;
                _GenderButtonStyle.normal.textColor = Color.white;
            }
            _hasInitStyles = true;
        }

        private void Awake()
        {
            if (!_hasInitStyles)
            {
                InitStyles();
            }
            _areaRect = new Rect(-500f, -500f, 200f, 200f);
            KMale = new GUIContent(Localizer.Format("#LOC_USI_MKS_AstronautComplex_Male"), AssetBase.GetTexture("kerbalicon_recruit"));
            KFemale = new GUIContent(Localizer.Format("#LOC_USI_MKS_AstronautComplex_Female"), AssetBase.GetTexture("kerbalicon_recruit_female"));
            KGRandom = new GUIContent(Localizer.Format("#LOC_USI_MKS_AstronautComplex_Random"), "When this option is selected the kerbal might be male or female");
            basecolor = GUI.color;
            roster = HighLogic.CurrentGame.CrewRoster;
            kerExp = HighLogic.CurrentGame.Parameters.CustomParams<GameParameters.AdvancedParams>().KerbalExperienceEnabled(HighLogic.CurrentGame.Mode);

            _kolonists = new List<Kolonist>
            {
                new Kolonist { Name = "Pilot", displayName = Localizer.Format("#LOC_USI_MKS_AstronautComplex_Pilot"), isBase = true, Effects = "Autopilot, VesselControl, RepBoost, Logistics, Explorer" },
                new Kolonist { Name = "Scientist", displayName = Localizer.Format("#LOC_USI_MKS_AstronautComplex_Scientist"), isBase = true, Effects = "Science, Experiment, Botany, Agronomy, Medical, ScienceBoost" },
                new Kolonist { Name = "Engineer", displayName = Localizer.Format("#LOC_USI_MKS_AstronautComplex_Engineer"), isBase = true, Effects = "Repair, Converter, Drill, Geology, FundsBoost" },
                new Kolonist { Name = "Kolonist", displayName = Localizer.Format("#LOC_USI_MKS_AstronautComplex_Kolonist"), isBase = false, Effects = "RepBoost, FundsBoost, ScienceBoost" },
                new Kolonist { Name = "Miner", displayName = Localizer.Format("#LOC_USI_MKS_AstronautComplex_Miner"), isBase = false, Effects = "Drill, FundsBoost" },
                new Kolonist { Name = "Technician", displayName = Localizer.Format("#LOC_USI_MKS_AstronautComplex_Technician"), isBase = false, Effects = "Converter, FundsBoost" },
                new Kolonist { Name = "Mechanic", displayName = Localizer.Format("#LOC_USI_MKS_AstronautComplex_Mechanic"), isBase = false, Effects = "Repair, FundsBoost" },
                new Kolonist { Name = "Biologist", displayName = Localizer.Format("#LOC_USI_MKS_AstronautComplex_Biologist"), isBase = false, Effects = "Biology, ScienceBoost" },
                new Kolonist { Name = "Geologist", displayName = Localizer.Format("#LOC_USI_MKS_AstronautComplex_Geologist"), isBase = false, Effects = "Geology, FundsBoost" },
                new Kolonist { Name = "Farmer", displayName = Localizer.Format("#LOC_USI_MKS_AstronautComplex_Farmer"), isBase = false, Effects = "Agronomy, ScienceBoost, RepBoost" },
                new Kolonist { Name = "Medic", displayName = Localizer.Format("#LOC_USI_MKS_AstronautComplex_Medic"), isBase = false, Effects = "Medical, ScienceBoost, RepBoost" },
                new Kolonist { Name = "Quartermaster", displayName = Localizer.Format("#LOC_USI_MKS_AstronautComplex_Quartermaster"), isBase = false, Effects = "Logistics, RepBoost" },
                new Kolonist { Name = "Scout", displayName = Localizer.Format("#LOC_USI_MKS_AstronautComplex_Scout"), isBase = false, Effects = "Explorer" }
            };
            KGendArray = new GUIContent[3] { KGRandom, KMale, KFemale };
        }

        public void Initialize(Rect guiRect)
        {
            //var uiScaleMultiplier = GameSettings.UI_SCALE;

            // the supplied rect will have the UI scalar already factored in
            //
            // to respect the player's UI scale wishes, work out what the unscaled rect
            // would be. Then we'll apply the scale again in OnGUI so all of our GUILayout elements
            // will respect the multiplier
            //var correctedRect = new Rect(guiRect.x, guiRect.y, guiRect.width / uiScaleMultiplier,
            //    guiRect.height / uiScaleMultiplier);

            var correctedRect = new Rect(guiRect.x, guiRect.y, guiRect.width, guiRect.height);

            _areaRect = correctedRect;

            enabled = true;

            // Reset of the basic Stupidity and Courage if the customization of Kerbonauts is disabled during game
            if (!KolonyACOptions.CustomKerbonautsEnabled)
            {
                KStupidity = 50;
                KCourage = 50;
            }
        }

        private void kHire()
        {
            if (HighLogic.CurrentGame.Mode == Game.Modes.CAREER)
            {
                double myFunds = Funding.Instance.Funds;
                Funding.Instance.AddFunds(-costMath(), TransactionReasons.CrewRecruited);
                //Debug.Log("KSI :: Total Funds removed " + costMath());
            }

            for (int i = 0; i < KBulki; i++)
            {
                ProtoCrewMember.Gender? chosenGender =  null;
                var selectedGender = KGendArray[KGender].text;
                if (selectedGender.Equals(Localizer.Format("#LOC_USI_MKS_AstronautComplex_Male")))
                    chosenGender = ProtoCrewMember.Gender.Male;
                else if (selectedGender.Equals(Localizer.Format("#LOC_USI_MKS_AstronautComplex_Female")))
                    chosenGender = ProtoCrewMember.Gender.Female;
                ProtoCrewMember newKerb = SpawnKerbal(chosenGender);

                string career = _recruitableKolonists[KCareer];
                // Sets the kerbal's career based on the KCareer switch.
                KerbalRoster.SetExperienceTrait(newKerb, career);

                // Debug.Log("KSI :: KIA MIA Stat is: " + KDead);
                // Debug.Log("KSI :: " + newKerb.experienceTrait.TypeName + " " + newKerb.name + " has been created in: " + loopcount.ToString() + " loops.");
                newKerb.rosterStatus = ProtoCrewMember.RosterStatus.Available;
                newKerb.experience = 0;


                if (KolonyACOptions.CustomKerbonautsEnabled)
                {
                    newKerb.experienceLevel = 0;
                    newKerb.courage = KCourage/100;
                    newKerb.stupidity = KStupidity/100;
                    if (KFearless)
                    {
                        newKerb.isBadass = true;
                    }
                }

                // Debug.Log("PSH :: Status set to Available, courage and stupidity set, fearless trait set.");
                if (KLevel > 0)
                {
                    var logName = RecruitLevel + KLevel;
                    var homeworldName = FlightGlobals.Bodies.Where(cb => cb.isHomeWorld).FirstOrDefault().name;
                    newKerb.flightLog.AddEntry(logName, homeworldName);
                    newKerb.ArchiveFlightLog();
                    newKerb.experience = GetExperienceNeededFor(KLevel);
                    newKerb.experienceLevel = KLevel;
                }
                if (kerExp == false)
                {
                    newKerb.experience = 9999;
                    newKerb.experienceLevel = 5;
                    //Debug.Log("KSI :: Level set to 5 - Non-Career Mode default.");
                }
            }

            // Refreshes the AC so that new kerbal shows on the available roster.
            //Debug.Log("PSH :: Hiring Function Completed.");
            GameEvents.onGUIAstronautComplexDespawn.Fire();
            GameEvents.onGUIAstronautComplexSpawn.Fire();

        }

        private static ProtoCrewMember SpawnKerbal(ProtoCrewMember.Gender? chosenGender)
        {
            for (int attempt = 0; attempt < 100; attempt++)
            {
                var kerb = HighLogic.CurrentGame.CrewRoster.GetNewKerbal(ProtoCrewMember.KerbalType.Crew);
                if ((!chosenGender.HasValue) || (chosenGender.Value == kerb.gender))
                    return kerb;
                HighLogic.CurrentGame.CrewRoster.Remove(kerb);
            }
            Debug.LogError("Failed spawning a kerbal with the wanted gender");
            return HighLogic.CurrentGame.CrewRoster.GetNewKerbal(ProtoCrewMember.KerbalType.Crew);
        }



        private int costMath()
        {
            int active = HighLogic.CurrentGame.CrewRoster.GetActiveCrewCount();
            var defaultCost = (int)GameVariables.Instance.GetRecruitHireCost(active);

            if (KolonyACOptions.CostCapEnabled)
                defaultCost = Math.Min(defaultCost, KolonyACOptions.GetMaxCost);

            if (_kolonists[KCareer].isBase && !KolonyACOptions.AlternateCoreCostEnabled)
                return defaultCost;
            if (!_kolonists[KCareer].isBase && !KolonyACOptions.AlternateKolonistCostEnabled)
                return defaultCost;

            dCheck();
            float fearlessCost = 0;
            float basecost = KolonyACOptions.GetKolonistCost;
            if (_kolonists[KCareer].isBase)
                basecost = KolonyACOptions.GetCoreCost;

            float couragecost = KCourage * 150;
            float stupidRebate = KStupidity * 150;
            float diffcost = HighLogic.CurrentGame.Parameters.Career.FundsLossMultiplier;
            if (KFearless == true)
            {
                fearlessCost += 10000;
            }
            DCost = 1 + (KDead * 0.1f);

            double minCost = 0.1d * basecost;
            double attribsCost = Math.Max(minCost, basecost + couragecost - stupidRebate);

            double currentcost = (attribsCost + fearlessCost) * (KLevel + 1) * DCost * diffcost * KBulki;
            int finalcost = Convert.ToInt32(currentcost); 

            if (KolonyACOptions.CostCapEnabled)
                finalcost = Math.Min(finalcost, KolonyACOptions.GetMaxCost);

            return finalcost;
        }

        private string hireStatus()
        {

            string bText = Localizer.Format("#LOC_USI_MKS_AstronautComplex_Hire");
            if (HighLogic.CurrentGame.Mode == Game.Modes.CAREER)
            {
                double kredits = Funding.Instance.Funds;
                if (HighLogic.CurrentGame.CrewRoster.GetActiveCrewCount() >= GameVariables.Instance.GetActiveCrewLimit(ScenarioUpgradeableFacilities.GetFacilityLevel(SpaceCenterFacility.AstronautComplex)))
                {
                    bText = Localizer.Format("#LOC_USI_MKS_AstronautComplex_Full");
                    hTest = false;
                }
                else
                {
                    if (costMath() > kredits)
                    {
                        bText = Localizer.Format("#LOC_USI_MKS_AstronautComplex_NoFunds");
                        hTest = false;
                    }
                    else
                        hTest = true;
                }
            }
            return bText;
        }

        private int cbulktest()
        {
            if (HighLogic.CurrentGame.Mode == Game.Modes.CAREER)
            {
                crewWeCanHire = Mathf.Clamp(GameVariables.Instance.GetActiveCrewLimit(ScenarioUpgradeableFacilities.GetFacilityLevel(SpaceCenterFacility.AstronautComplex)) - HighLogic.CurrentGame.CrewRoster.GetActiveCrewCount(), 0, 10);
            }
            return crewWeCanHire;
        }

        private void dCheck()
        {
            KDead = 0;
            // 10 percent for dead and 5 percent for missing, note can only have dead in some career modes.
            foreach (ProtoCrewMember kerbal in roster.Crew)
            {
                if (kerbal.rosterStatus.ToString() == "Dead")
                {
                    if (kerbal.experienceTrait.Title == _kolonists[KCareer].Name)
                    {
                        KDead += 1;
                    }
                }
                if (kerbal.rosterStatus.ToString() == "Missing")
                {
                    if (kerbal.experienceTrait.Title == _kolonists[KCareer].Name)
                    {
                        KDead += 0.5;
                    }
                }
            }
        }

        private void OnGUI()
        {

            GUI.skin = HighLogic.Skin;
            if (HighLogic.CurrentGame.Mode == Game.Modes.SANDBOX)
            {
                hasKredits = false;
                ACLevel = 5;
            }
            if (HighLogic.CurrentGame.Mode == Game.Modes.SCIENCE_SANDBOX)
            {
                hasKredits = false;
                ACLevel = 5;
            }
            if (HighLogic.CurrentGame.Mode == Game.Modes.CAREER)
            {
                hasKredits = true;
                ACLevel = ScenarioUpgradeableFacilities.GetFacilityLevel(SpaceCenterFacility.AstronautComplex);
            }

            GUILayout.BeginArea(_areaRect);
            {
                GUILayout.Label(Localizer.Format("#LOC_USI_MKS_AstronautComplex_Recruit")); // Testing Renaming Label Works

                // Gender selection 
                GUILayout.BeginHorizontal("box");
                KGender = GUILayout.Toolbar(KGender, KGendArray, _GenderButtonStyle);
                GUILayout.EndHorizontal();

                // Career selection
                GUILayout.BeginVertical("box");
                if (KolonyACOptions.KolonistHiringEnabled)
                {
                    _recruitableKolonists = _kolonists.Select(k => k.Name).ToArray();
                }
                else
                {
                    _recruitableKolonists = _kolonists.Select(k => k.Name).Take(3).ToArray();
                }
                if (KolonyACOptions.KolonistHiringEnabled)
                {
                    _displayRecruitableKolonists = _kolonists.Select(k => k.displayName).ToArray();
                }
                else
                {
                    _displayRecruitableKolonists = _kolonists.Select(k => k.displayName).Take(3).ToArray();
                }
                KCareer = GUILayout.SelectionGrid(KCareer, _displayRecruitableKolonists, 4, _ButtonStyle, GUILayout.Height(144f));

                // Adding a section for 'number/bulk hire' here using the int array kBulk 
                if (cbulktest() < 1)
                {
                    GUILayout.Label(Localizer.Format("#LOC_USI_MKS_AstronautComplex_NoBulk"));
                }
                else
                {
                    GUILayout.Label(Localizer.Format("#LOC_USI_MKS_AstronautComplex_Bulk") + KBulki);
                    KBulk = GUILayout.HorizontalSlider(KBulk, 1, cbulktest());
                    KBulki = Convert.ToInt32(KBulk);

                }

                GUI.contentColor = basecolor;
                GUILayout.EndVertical();

                if (KolonyACOptions.CustomKerbonautsEnabled)
                {
                    // Courage Brains and BadS flag selections
                    GUILayout.BeginVertical("box");
                    GUILayout.Label(Localizer.Format("#LOC_USI_MKS_AstronautComplex_Courage") + Math.Truncate(KCourage));
                    KCourage = GUILayout.HorizontalSlider(KCourage, 0, 100);
                    GUILayout.Label(Localizer.Format("#LOC_USI_MKS_AstronautComplex_Stupidity") + Math.Truncate(KStupidity));
                    KStupidity = GUILayout.HorizontalSlider(KStupidity, 0, 100);
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(Localizer.Format("#LOC_USI_MKS_AstronautComplex_isFearless"));
                    KFearless = GUILayout.Toggle(KFearless, Localizer.Format("#LOC_USI_MKS_AstronautComplex_Fearless"));
                    GUILayout.EndHorizontal();
                    GUILayout.EndVertical();

                    // Level selection
                    GUILayout.BeginVertical("box");
                    GUILayout.Label(Localizer.Format("#LOC_USI_MKS_AstronautComplex_SelectLv"));

                    // If statements for level options
                    if (kerExp == false)
                    {
                        GUILayout.Label(Localizer.Format("#LOC_USI_MKS_AstronautComplex_Lv5_Career"));
                    }
                    else
                    {
                        if (ACLevel == 0)
                        {
                            KLevel = GUILayout.Toolbar(KLevel, KLevelStringsZero, _ButtonStyle, GUILayout.Height(36f));
                        }
                        if (ACLevel == 0.5)
                        {
                            KLevel = GUILayout.Toolbar(KLevel, KLevelStringsOne, _ButtonStyle, GUILayout.Height(36f));
                        }
                        if (ACLevel == 1)
                        {
                            KLevel = GUILayout.Toolbar(KLevel, KLevelStringsTwo, _ButtonStyle, GUILayout.Height(36f));
                        }
                        if (ACLevel == 5)
                        {
                            GUILayout.Label(Localizer.Format("#LOC_USI_MKS_AstronautComplex_Lv5_Sandbox"));
                        }
                    }
                    GUILayout.EndVertical();
                }

                if (hasKredits == true)
                {
                    GUILayout.BeginHorizontal("window");
                    GUILayout.BeginVertical();
                    //GUILayout.FlexibleSpace();
                    if (costMath() <= Funding.Instance.Funds)
                    {
                        GUILayout.Label(Localizer.Format("#LOC_USI_MKS_AstronautComplex_Cost") + costMath(), HighLogic.Skin.textField);
                    }
                    else
                    {
                        GUI.color = Color.red;
                        GUILayout.Label(Localizer.Format("#LOC_USI_MKS_AstronautComplex_InsFunds") + costMath(), HighLogic.Skin.textField);
                        GUI.color = basecolor;
                    }
                    // GUILayout.FlexibleSpace();
                    GUILayout.EndVertical();
                    GUILayout.EndHorizontal();
                }
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();

                string statusText = hireStatus();
                if (hTest)
                {
                    if (GUILayout.Button(statusText, _ButtonStyle, GUILayout.Width(200f), GUILayout.Height(36f)))
                        kHire();
                }
                else
                    GUILayout.Button(statusText, _ButtonStyle, GUILayout.Width(200f), GUILayout.Height(36f));

                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                GUILayout.EndArea();
            }
        }
        void Update()
        {
            AstronautComplex ac = GameObject.FindObjectOfType<AstronautComplex>();
            if (ac != null)
            {
                if (ac.ScrollListApplicants.Count > 0)
                {
                    //Debug.Log("TRP: Clearing Applicant List");
                    ac.ScrollListApplicants.Clear(true);
                }
            }
        }

        public static float GetExperienceNeededFor(int level)
        {
            switch (level)
            {
                case 0:
                    return 0;
                case 1:
                    return 2;
                case 2:
                    return 8;
                case 3:
                    return 16;
                case 4:
                    return 32;
                case 5:
                    return 64;
                default:
                    return 0;
            }
        }

    }

}
