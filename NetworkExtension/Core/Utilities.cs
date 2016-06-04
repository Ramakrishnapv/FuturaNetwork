using ESRI.ArcGIS.esriSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Futura.ArcGIS.NetworkExtension
{
    public static class PhaseCodeBitgateMapping
    {
        public const int Unknown_BitgateValue = 128;
        public const int A_BitgateValue = 129;
        public const int B_BitgateValue = 130;
        public const int C_BitgateValue = 132;
        public const int AB_BitgateValue = 131;
        public const int AC_BitgateValue = 133;
        public const int BC_BitgateValue = 134;
        public const int ABC_BitgateValue = 135;
    }

    public class SdeConnectionProperties
    {
        private IPropertySet _propertySet = null;
        private string[] _names = null;
        private object[] _values = null;
        public SdeConnectionProperties(IPropertySet propertySet)
        {
            this._propertySet = propertySet;
            object names = null;
            object values = null;
            if (this._propertySet != null && this._propertySet.Count > 0)
            {
                this._propertySet.GetAllProperties(out names, out values);
                if (names != null && values != null)
                {
                    _names = (string[])names;
                    _values = (object[])values;
                }
            }
        }

        private string GetProperty(string propName)
        {
            if (this._names == null || this._values == null) return string.Empty;
            for (int i = 0; i < this._propertySet.Count; i++)
            {
                if (string.Compare(this._names[i], propName, true) == 0)
                    return this._values[i].ToString();
            }
            return string.Empty;
        }


        public string Server
        {
            get
            {
                return this.GetProperty("SERVER");
            }
        }

        public string Instance
        {
            get
            {
                return this.GetProperty("INSTANCE");
            }
        }

        public string Database
        {
            get
            {
                return this.GetProperty("DATABASE");
            }
        }

        public string User
        {
            get
            {
                return this.GetProperty("USER");
            }
        }

        public string Version
        {
            get
            {
                return this.GetProperty("VERSION");
            }
        }

        public string EncryptedPassword
        {
            get
            {
                return this.GetProperty("PASSWORD");
            }
        }

    }
    public class Utilities
    {
        public static string GenerateGUID(bool withBrackets)
        {
            string uid = System.Guid.NewGuid().ToString();
            if (withBrackets)
            {
                uid = uid.Insert(0, "{");
                uid = uid.Insert(uid.Length, "}");
            }
            return uid;
        }

        public static bool IsNumeric(string s)
        {
            // Returns true if the string passed only contains numeric characters and no whitespace chars
            int numCount = 0;

            try
            {
                if (s == null || s == string.Empty || s.Length == 0) return false;

                for (int i = 0; i < s.Length; i++)
                {
                    int keyCode = System.Convert.ToInt32(System.Convert.ToChar(s.Substring(i, 1)));
                    if (keyCode > 47 && keyCode < 58)
                        numCount++;
                    else
                        return false;
                }
            }
            catch
            {
                numCount = 0;
            }

            if (numCount > 0)
                return true; //At least one numeric value exists with no chars
            else
                return false;

        }

        public static int AddPhaseCodes(int phase1, int phase2)
        {
            int sum = 128;
            if (IsPhaseCodeValid(phase1) && IsPhaseCodeValid(phase2))
                sum = phase1 | phase2;
            return sum;
        }

        public static bool IsPhaseCodeValid(int phase)
        {
            bool valid = false;
            if (phase >= 128 && phase <= 135) valid = true;
            return valid;
        }

        public static string BitgatePhaseToStringPhase(int phaseCode)
        {
            // Return the string format of the numeric phase value. 1-7 or 129-135

            string message = string.Empty;
            try
            {
                if (phaseCode <= 7)
                {
                    if (phaseCode == 1) message = "A";
                    else if (phaseCode == 2) message = "B";
                    else if (phaseCode == 3) message = "C";
                    else if (phaseCode == 4) message = "A" + "B";
                    else if (phaseCode == 5) message = "A" + "C";
                    else if (phaseCode == 6) message = "B" + "C";
                    else if (phaseCode == 7) message = "A" + "B" + "C";
                }
                else if (phaseCode >= 128 && phaseCode <= 135)
                {
                    try
                    {
                        if ((phaseCode & 1) > 0) message += "A";
                        if ((phaseCode & 2) > 0) message += "B";
                        if ((phaseCode & 4) > 0) message += "C";
                    }
                    catch (Exception e)
                    {
                        throw new Exception(e.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return message;
        }

        public static int CommonPhase(int phase1, int phase2)
        {
            if (IsPhaseCodeValid(phase1) && IsPhaseCodeValid(phase2))
                return phase1 & phase2;
            return PhaseCodeBitgateMapping.Unknown_BitgateValue;
        }

        public static void WhichPhaseIsMissing(int phase1, int phase2, out int phase1MissingPhases, out int phase2MissingPhases)
        {
            phase1MissingPhases = PhaseCodeBitgateMapping.Unknown_BitgateValue;
            phase2MissingPhases = PhaseCodeBitgateMapping.Unknown_BitgateValue;
            if (IsPhaseCodeValid(phase1) && IsPhaseCodeValid(phase2))
            {
                if (HaveCommonPhaseCodes(phase1, phase2))
                {
                    int common = CommonPhase(phase1, phase2);
                    int remainder = phase1 ^ common;
                    phase1MissingPhases = remainder | PhaseCodeBitgateMapping.Unknown_BitgateValue;
                    remainder = phase2 ^ common;
                    phase2MissingPhases = remainder | PhaseCodeBitgateMapping.Unknown_BitgateValue;
                }
            }
        }

        public static bool IsMultiPhasePhaseCode(int phase)
        {
            bool multiPhase = false;
            if (IsPhaseCodeValid(phase))
            {
                int remainder = PhaseCodeBitgateMapping.Unknown_BitgateValue ^ phase;
                if (remainder != 0 && remainder != 1 && remainder != 2 && remainder != 4)
                    multiPhase = true;
            }
            return multiPhase;
        }

        public static int GetFirstPhaseCode(int phase)
        {
            int returnPhase = phase;
            if (IsPhaseCodeValid(phase))
            {
                int remainder = PhaseCodeBitgateMapping.Unknown_BitgateValue ^ phase;
                if (remainder != 0 && remainder != 1 && remainder != 2 && remainder != 4)
                {
                    remainder = remainder % 2;
                    if (remainder == 0)
                        remainder = 2;
                    returnPhase = PhaseCodeBitgateMapping.Unknown_BitgateValue ^ remainder;
                }
            }
            return returnPhase;
        }

        public static IList<int> GetIndividualPhasesFromPhaseCode(int phaseCode)
        {
            int humptyDumptyPieces = phaseCode;
            IList<int> backTogetherAgain = new List<int>();
            if (IsPhaseCodeValid(humptyDumptyPieces))
            {
                if (IsMultiPhasePhaseCode(humptyDumptyPieces))
                {
                    int humptyDumpty = PhaseCodeBitgateMapping.Unknown_BitgateValue;
                    int humptyDumptyPiece = GetFirstPhaseCode(humptyDumptyPieces);
                    while (humptyDumpty != phaseCode)
                    {
                        humptyDumpty = AddPhaseCodes(humptyDumpty, humptyDumptyPiece);
                        humptyDumptyPieces = SubtractPhaseCodes(humptyDumptyPieces, humptyDumptyPiece);
                        if (!backTogetherAgain.Contains(humptyDumptyPiece)) backTogetherAgain.Add(humptyDumptyPiece);
                        humptyDumptyPiece = GetFirstPhaseCode(humptyDumptyPieces);
                    }
                }
                else
                    backTogetherAgain.Add(phaseCode);
            }
            return backTogetherAgain;
        }

        public static bool IsPhaseCodePresent(int basePhase, int searchPhase)
        {
            bool present = false;
            if (IsPhaseCodeValid(basePhase) && IsPhaseCodeValid(searchPhase))
            {
                int bit = (PhaseCodeBitgateMapping.Unknown_BitgateValue ^ searchPhase);
                if ((basePhase & bit) == bit) present = true;
            }
            return present;
        }

        public static bool HaveCommonPhaseCodes(int phase1, int phase2)
        {
            bool haveCommonPhase = false;
            if (IsPhaseCodeValid(phase1) && IsPhaseCodeValid(phase2))
            {
                if (CommonPhase(phase1, phase2) != PhaseCodeBitgateMapping.Unknown_BitgateValue)
                    haveCommonPhase = true;
                if (haveCommonPhase == false)
                    haveCommonPhase = IsPhaseCodePresent(phase2, phase1);
            }
            return haveCommonPhase;
        }

        public static int SubtractPhaseCodes(int phase1, int phase2)
        {
            int remainder = PhaseCodeBitgateMapping.Unknown_BitgateValue;
            if (IsPhaseCodeValid(phase1) && IsPhaseCodeValid(phase2))
            {
                remainder = phase1 ^ phase2;
                remainder = remainder | PhaseCodeBitgateMapping.Unknown_BitgateValue;
            }
            if (IsPhaseCodeValid(remainder) == false)
                remainder = PhaseCodeBitgateMapping.Unknown_BitgateValue;
            return remainder;
        }

    }

    public static class ElectricTableNames
    {
        public static string SecondaryConductor_TableName
        {
            get { return  "SecondaryConductor"; }
        }

        public static string PrimaryConductor_TableName
        {
            get { return "PrimaryConductor"; }
        }

        public static string TransformerBank_TableName
        {
            get { return  "TransformerBank"; }
        }

        public static string StepTransformerBank_TableName
        {
            get { return "StepTransformerBank"; }
        }

        public static string ConstructionUnit_TableName
        {
            get { return  "ConstructionUnits"; }
        }

        public static string SwitchBank_TableName
        {
            get { return  "SwitchBank"; }
        }

        public static string RecloserBank_TableName
        {
            get { return  "RecloserBank"; }
        }

        public static string FuseBank_TableName
        {
            get { return  "FuseBank"; }
        }

        public static string CapacitorBank_TableName
        {
            get { return  "CapacitorBank"; }
        }

        public static string RegulatorBank_TableName
        {
            get { return "RegulatorBank"; }
        }

        public static string SectionalizerBank_TableName
        {
            get { return "SectionalizerBank"; }
        }

        public static string Feeder_TableName
        {
            get { return "Feeder"; }
        }

        public static string DistributionSource_TableName
        {
            get { return "DistributionSource"; }
        }

        public static string Consumer_TableName
        {
            get { return  "Consumer"; }
        }

        public static string Structure_TableName
        {
            get { return  "Structure"; }
        }

        public static string AssetWarehouse_TableName
        {
            get { return  "Warehouse"; }
        }

        public static string OpenPoint_TableName
        {
            get { return "OpenPoint"; }
        }
    }

    public static class Settings
    {
        public static List<string> DistributionFieldsIgnoredWhileInheritingAttributes
        {
            get
            {
                return "objectid,shape,phasecode,phasing,constructedphase,length,shape_length,enabled,networkid,structureguid,futuraguid,mapnumber,comments,templateguid,workordernumber,datecreated,datemodified,globalid,lastmodifiedby,shape.stlength()".Split(',').ToList();
            }
        }
    }
}
