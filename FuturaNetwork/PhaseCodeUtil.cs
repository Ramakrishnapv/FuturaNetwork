using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FuturaNetwork
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

    public static class PhaseCodeUtil
    {
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

        public static bool DownsectionPCCompatible(int parentPC, int childPC)
        {
            if (PhaseCodeUtil.IsPhaseCodePresent(parentPC, childPC))
                return true;
            else return false;
        }

        public static bool IsOnePhaseCodeDifferent(int pc1, int pc2)
        {
            if (!IsMultiPhasePhaseCode(pc1) || !IsMultiPhasePhaseCode(pc2)) return false;
            return !IsMultiPhasePhaseCode(SubtractPhaseCodes(pc1, pc2)); 
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
}
